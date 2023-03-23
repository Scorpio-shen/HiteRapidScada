using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HslCommunication.Core;
using HslCommunication.Instrument.DLT.Helper;
#if !NET35 && !NET20
using System.Threading.Tasks;
#endif

namespace HslCommunication.Instrument.DLT
{
	/// <summary>
	/// DTL数据转换
	/// </summary>
	public class DLTTransform
	{
		/// <summary>
		/// Byte[]转ToHexString
		/// </summary>
		/// <param name="content">原始的字节内容</param>
		/// <param name="length">长度信息</param>
		/// <returns>字符串的结果信息</returns>
		public static OperateResult<string> TransStringFromDLt( byte[] content, ushort length )
		{
			OperateResult<string> result;
			try
			{
				string empty = string.Empty;
				byte[] buffer = content.SelectBegin( length ).Reverse( ).ToArray( ).EveryByteAdd( -0x33 );
				result = OperateResult.CreateSuccessResult( Encoding.ASCII.GetString( buffer ) );
			}
			catch (Exception ex)
			{
				result = new OperateResult<string>( ex.Message + " Reason: " + content.ToHexString( ' ' ) );
			}
			return result;
		}

		/// <summary>
		/// 从读取的原始字节里解析出实际的字符串数组
		/// </summary>
		/// <param name="type">类型信息</param>
		/// <param name="content">原始字节内容</param>
		/// <param name="dataID">地址信息</param>
		/// <param name="reverse">指示当前的结果数据是否发生数据反转的操作</param>
		/// <returns>字符串的数组内容</returns>
		public static OperateResult<string[]> TransStringsFromDLt( DLT645Type type, byte[] content, byte[] dataID, bool reverse )
		{
			List<string> strings = new List<string>();
			try
			{
				int[] formats = type == DLT645Type.DLT2007 ? GetDLT2007FormatWithDataArea( dataID ) : GetDLT1997FormatWithDataArea( dataID );
				if (formats == null)
				{
					// 直接返回普通的字符串信息
					if (reverse)
						strings.Add( content.Reverse( ).ToArray( ).EveryByteAdd( -0x33 ).ToHexString( ) );
					else
						strings.Add( content.EveryByteAdd( -0x33 ).ToHexString( ) );
					return OperateResult.CreateSuccessResult( strings.ToArray( ) );
					// return new OperateResult<string[]>( "Current address not supported prase" );
				}

				int offset = 0;
				for (int i = 0; i < formats.Length; i++)
				{
					if (offset >= content.Length) return OperateResult.CreateSuccessResult( strings.ToArray( ) );

					byte[] array = BitConverter.GetBytes( formats[i] );
					int byteCount    = array[0];
					int decimalCount = array[1];
					int length       = array[2];

					if (array[3] == 1)
					{
						// ASCII字符串
						if (reverse)
							strings.Add( Encoding.ASCII.GetString( content.SelectMiddle( offset, byteCount ).Reverse( ).ToArray( ).EveryByteAdd( -0x33 ) ) );
						else
							strings.Add( Encoding.ASCII.GetString( content.SelectMiddle( offset, byteCount ).EveryByteAdd( -0x33 ) ) );
					}
					else
					{
						double scale = 1d;
						// 数值
						byte[] buffer = reverse ?
							content.SelectMiddle( offset, byteCount ).Reverse( ).ToArray( ).EveryByteAdd( -0x33 ) :
							content.SelectMiddle( offset, byteCount ).EveryByteAdd( -0x33 );
						if (decimalCount >= 0x80)
						{
							decimalCount = decimalCount - 0x80;
							scale = ((buffer[0] & 0x80) == 0x80) ? -1d : 1d;        // 检测最高位是否为true，来设置正负，0正1负
							buffer[0] = (byte)(buffer[0] & 0x7F);                   // 最高位设置为0
						}
						string hex = buffer.ToHexString( );
						if (decimalCount == 0)
						{
							strings.Add( hex );
						}
						else
						{
							try
							{
								strings.Add( (Convert.ToDouble( hex ) * scale / Math.Pow( 10, decimalCount )).ToString( ) );
							}
							catch (Exception ex)
							{
								return new OperateResult<string[]>( ex.Message + " ID:" + dataID.ToHexString( '-' ) + " Value:" + hex + Environment.NewLine + ex.StackTrace );
							}
						}
					}

					offset += byteCount;
				}
				return OperateResult.CreateSuccessResult( strings.ToArray( ) );
			}
			catch (Exception ex)
			{
				return new OperateResult<string[]>( ex.Message + " ID:" + dataID.ToHexString( '-' ) + Environment.NewLine + ex.StackTrace );
			}
		}


		/// <summary>
		/// Byte[]转Dlt double[]，使用一样的 format 格式来转换多个double类型的数据
		/// </summary>
		/// <param name="content">原始的字节数据</param>
		/// <param name="length">需要转换的数据长度</param>
		/// <param name="format">当前数据的解析格式</param>
		/// <returns>结果内容</returns>
		public static OperateResult<double[]> TransDoubleFromDLt( byte[] content, ushort length, string format = "XXXXXX.XX" )
		{
			try
			{
				format = format.ToUpper( );
				int byteCount = format.Count( m => m != '.' ) / 2;
				int decimalCount = format.IndexOf( '.' ) >= 0 ? format.Length - format.IndexOf( '.' ) - 1 : 0;

				double[] values = new double[length];
				for (int i = 0; i < values.Length; i++)
				{
					byte[] buffer = content.SelectMiddle( i * byteCount, byteCount ).Reverse( ).ToArray( ).EveryByteAdd( -0x33 );
					values[i] = Convert.ToDouble( buffer.ToHexString( ) ) / Math.Pow( 10, decimalCount );
				}
				return OperateResult.CreateSuccessResult( values );
			}
			catch (Exception ex)
			{
				return new OperateResult<double[]>( ex.Message );
			}
		}

		//  4,1;6,0,2;
		private static int GetFormat( byte byteLength, int digtal, byte length = 1, bool negativeFlag = false )
		{
			if (digtal >=0)
				return BitConverter.ToInt32( new byte[] { byteLength, (byte)(digtal + (negativeFlag ? 0x80 : 0x00)), length, 0x00 }, 0 );
			else
				return BitConverter.ToInt32( new byte[] { byteLength, 0, length, 0x01 }, 0 );
		}

		private static int[] get03_30_02( )
		{
			List<int> result = new List<int>( 50 );
			result.Add( GetFormat( 6, 0 ) );
			result.Add( GetFormat( 4, 0 ) );
			for (int i = 0; i < 24; i++)
			{
				result.Add( GetFormat( 3, 4 ) );
				result.Add( GetFormat( 5, 0 ) );
			}
			return result.ToArray( );
		}

		/// <summary>
		/// 根据不同的数据地址，返回实际的数据格式，然后解析出正确的数据
		/// </summary>
		/// <param name="dataArea">数据标识地址，实际的byte数组，地位在前，高位在后</param>
		/// <returns>实际的数据格式信息</returns>
		public static int[] GetDLT2007FormatWithDataArea( byte[] dataArea )
		{
			if (dataArea[3] == 0x00)
			{
				if (dataArea[2] == 0x00) return new int[] { GetFormat( 4, 2, 1, true ) };                          // 组合有功
				if (dataArea[2] == 0x01) return new int[] { GetFormat( 4, 2 ) };                                   // 正向有功
				if (dataArea[2] == 0x02) return new int[] { GetFormat( 4, 2 ) };                                   // 反向有功
				if (dataArea[2] == 0x03) return new int[] { GetFormat( 4, 2, 1, true ) };                          // 组合无功1
				if (dataArea[2] == 0x04) return new int[] { GetFormat( 4, 2, 1, true ) };                          // 组合无功2
				return new int[] { GetFormat( 4, 2 ) };                                                            // 正向视在
			}
			if (dataArea[3] == 0x01)
			{
				if (dataArea[2] == 0x03) return new int[] { GetFormat( 3, 4, 1, true ), GetFormat( 5, 0 ) };       // 组合无功1最大需量
				if (dataArea[2] == 0x04) return new int[] { GetFormat( 3, 4, 1, true ), GetFormat( 5, 0 ) };       // 组合无功2最大需量
				return new int[] { GetFormat( 3, 4 ), GetFormat( 5, 0 ) };                                         // 最大需求量及发生时间 YYMMDDhhmm
			}
			if (dataArea[3] == 0x02)
			{
				if (dataArea[2] == 0x01) return new int[] { GetFormat( 2, 1 ) };                                   // A,B,C 电压
				if (dataArea[2] == 0x02) return new int[] { GetFormat( 3, 3, 1, true ) };                          // A,B,C 电流
				if (dataArea[2] < 6)     return new int[] { GetFormat( 3, 4, 1, true ) };                          // 瞬时总有功，总无功，总视在功率
				if (dataArea[2] == 0x06) return new int[] { GetFormat( 2, 3, 1, true ) };                          // 功率因数
				if (dataArea[2] == 0x07) return new int[] { GetFormat( 2, 1 ) };                                   // 相角
				if (dataArea[2] < 0x80)  return new int[] { GetFormat( 2, 2 ) };                                   // 电压，电流波形失真度
				if (dataArea[2] == 0x80 && dataArea[0] == 0x01) return new int[] { GetFormat( 3, 3, 1, true ) };   // 零线电流
				if (dataArea[2] == 0x80 && dataArea[0] == 0x02) return new int[] { GetFormat( 2, 2 ) };            // 电网频率
				if (dataArea[2] == 0x80 && dataArea[0] == 0x03) return new int[] { GetFormat( 3, 4 ) };            // 一分钟有功总平均功率
				if (dataArea[2] == 0x80 && dataArea[0] == 0x04) return new int[] { GetFormat( 3, 4, 1, true ) };   // 当前有功需量
				if (dataArea[2] == 0x80 && dataArea[0] == 0x05) return new int[] { GetFormat( 3, 4, 1, true ) };   // 当前无功需量
				if (dataArea[2] == 0x80 && dataArea[0] == 0x06) return new int[] { GetFormat( 3, 4, 1, true ) };   // 当前视在需量
				if (dataArea[2] == 0x80 && dataArea[0] == 0x07) return new int[] { GetFormat( 2, 1, 1, true ) };   // 表内温度
				if (dataArea[2] == 0x80 && dataArea[0] == 0x08) return new int[] { GetFormat( 2, 2 ) };
				if (dataArea[2] == 0x80 && dataArea[0] == 0x09) return new int[] { GetFormat( 2, 2 ) };
				if (dataArea[2] == 0x80 && dataArea[0] == 0x0A) return new int[] { GetFormat( 4, 0 ) };     // 内部电池工作时间
			}

			// 失压相关
			if (dataArea[3] == 0x03)
			{
				if (dataArea[2] < 0x05 && dataArea[1] == 0x00 && dataArea[0] == 0x00) return new int[] { GetFormat( 3, 0, 6 ) };
				if (dataArea[2] < 0x05) return new int[] {
					GetFormat( 6, 0, 2 ),
					GetFormat( 4, 2, 4 ),
					GetFormat( 4, 2, 4 ),
					GetFormat( 2, 1 ),
					GetFormat( 3, 3 ),
					GetFormat( 3, 4, 2 ),
					GetFormat( 2, 3 ),
					GetFormat( 4, 2, 4 ),
					GetFormat( 2, 1 ),
					GetFormat( 3, 3 ),
					GetFormat( 3, 4, 2 ),
					GetFormat( 2, 3 ),
					GetFormat( 4, 2, 4 ),
					GetFormat( 2, 1 ),
					GetFormat( 3, 3 ),
					GetFormat( 3, 4, 2 ),
					GetFormat( 2, 3 ),
					GetFormat( 4, 2, 4 )};
				if (dataArea[2] == 0x05 && dataArea[1] == 0x00 && dataArea[0] == 0x00)                                                 return new int[] { GetFormat( 3, 0, 2 ) };
				if (dataArea[2] == 0x05 && dataArea[1] == 0x00)                                                                        return new int[] { GetFormat( 6, 0 ), GetFormat( 3, 3 ), GetFormat( 6, 0 ) };
				if (dataArea[2] == 0x06 && dataArea[1] == 0x00 && dataArea[0] == 0x00)                                                 return new int[] { GetFormat( 3, 0, 2 ) };
				if (dataArea[2] == 0x06 && dataArea[1] == 0x00)                                                                        return new int[] { GetFormat( 6, 0 ), GetFormat( 6, 0 ) };
				if (dataArea[2] == 0x07 && dataArea[1] == 0x00 && dataArea[0] == 0x00)                                                 return new int[] { GetFormat( 3, 0, 2 ) };
				if (dataArea[2] == 0x07 && dataArea[1] == 0x00)                                                                        return new int[] { GetFormat( 6, 0, 2 ), GetFormat( 4, 2, 16 ) };
				if (dataArea[2] == 0x08 && dataArea[1] == 0x00 && dataArea[0] == 0x00)                                                 return new int[] { GetFormat( 3, 0, 2 ) };
				if (dataArea[2] == 0x08 && dataArea[1] == 0x00)                                                                        return new int[] { GetFormat( 6, 0, 2 ), GetFormat( 4, 2, 16 ) };
				if (dataArea[2] == 0x09 && dataArea[1] == 0x00 && dataArea[0] == 0x00)                                                 return new int[] { GetFormat( 3, 0, 2 ) };
				if (dataArea[2] == 0x09 && dataArea[1] == 0x00)                                                                        return new int[] { GetFormat( 6, 0, 2 ), GetFormat( 2, 2 ), GetFormat( 4, 2, 16 ) };
				if (dataArea[2] == 0x0A && dataArea[1] == 0x00 && dataArea[0] == 0x00)                                                 return new int[] { GetFormat( 3, 0, 2 ) };
				if (dataArea[2] == 0x0A && dataArea[1] == 0x00)                                                                        return new int[] { GetFormat( 6, 0, 2 ), GetFormat( 2, 2 ), GetFormat( 4, 2, 16 ) };
				if ((dataArea[2] == 0x0B || dataArea[2] == 0x0C || dataArea[2] == 0x0D) && dataArea[1] == 0x00 && dataArea[0] == 0x00) return new int[] { GetFormat( 3, 0, 6 ) };
				if (dataArea[2] == 0x0B || dataArea[2] == 0x0C || dataArea[2] == 0x0D)                                                 return new int[] {
					GetFormat( 6, 0, 2 ),
					GetFormat( 4, 2, 4 ),
					GetFormat( 4, 2, 4 ),
					GetFormat( 2, 1 ),
					GetFormat( 3, 3 ),
					GetFormat( 3, 4, 2 ),
					GetFormat( 2, 3 ),
					GetFormat( 4, 2, 4 ),
					GetFormat( 2, 1 ),
					GetFormat( 3, 3 ),
					GetFormat( 3, 4, 2 ),
					GetFormat( 2, 3 ),
					GetFormat( 4, 2, 4 ),
					GetFormat( 2, 1 ),
					GetFormat( 3, 3 ),
					GetFormat( 3, 4, 2 ),
					GetFormat( 2, 3 )
				};
				if (dataArea[2] == 0x0E && dataArea[1] == 0x00 && dataArea[0] == 0x00) return new int[] { GetFormat( 3, 0, 6 ) };                                          // 潮流反向
				if (dataArea[2] == 0x0E )                                              return new int[] { GetFormat( 6, 0, 2 ), GetFormat( 4, 2, 16 ) };
				if (dataArea[2] == 0x0F && dataArea[1] == 0x00 && dataArea[0] == 0x00) return new int[] { GetFormat( 3, 0, 6 ) };                                          // 过载记录
				if (dataArea[2] == 0x0F)                                               return new int[] { GetFormat( 6, 0, 2 ), GetFormat( 4, 2, 16 ) };
				if (dataArea[2] == 0x10)                                               return new int[] { GetFormat( 3, 0 ), GetFormat( 3, 2, 2 ), GetFormat( 3, 0, 2 ), GetFormat( 2, 1 ), GetFormat( 4, 0 ), GetFormat( 2, 1 ), GetFormat( 4, 0 ) }; // 电压合格率统计
				if (dataArea[2] == 0x11 && dataArea[1] == 0x00 && dataArea[0] == 0x00) return new int[] { GetFormat( 3, 0 ) };                                             // 掉电总次数
				if (dataArea[2] == 0x11 && dataArea[1] == 0x00)                        return new int[] { GetFormat( 6, 0, 2 ) };                                          // 掉电时刻
				if (dataArea[2] == 0x12 && dataArea[1] == 0x00 && dataArea[0] == 0x00) return new int[] { GetFormat( 3, 0, 6 ) };                                          // 有功需量
				if (dataArea[2] == 0x12)                                               return new int[] { GetFormat( 6, 0, 2 ), GetFormat( 3, 4 ), GetFormat( 5, 0, 0 ) };
				if (dataArea[2] == 0x30 && dataArea[1] == 0x00 && dataArea[0] == 0x00) return new int[] { GetFormat( 3, 0 ) };                                              // 编程总次数
				if (dataArea[2] == 0x30 && dataArea[1] == 0x00)                        return new int[] { GetFormat( 6, 0 ), GetFormat( 4, 0 ), GetFormat( 4, 0, 10 ) };    // 编程记录
				if (dataArea[2] == 0x30 && dataArea[1] == 0x01 && dataArea[0] == 0x00) return new int[] { GetFormat( 3, 0 ) };                                              // 点表清零总次数
				if (dataArea[2] == 0x30 && dataArea[1] == 0x01)                        return new int[] { GetFormat( 6, 0 ), GetFormat( 4, 0 ), GetFormat( 4, 2, 24 ) };    // 点表清零记录
				if (dataArea[2] == 0x30 && dataArea[1] == 0x02 && dataArea[0] == 0x00) return new int[] { GetFormat( 3, 0 ) };                                              // 需量清零总次数
				if (dataArea[2] == 0x30 && dataArea[1] == 0x02)                        return get03_30_02( );                                                               // 需量清零记录
				if (dataArea[2] == 0x30 && dataArea[1] == 0x03 && dataArea[0] == 0x00) return new int[] { GetFormat( 3, 0 ) };                                              // 事件清零总次数
				if (dataArea[2] == 0x30 && dataArea[1] == 0x03)                        return new int[] { GetFormat( 6, 0 ), GetFormat( 4, 0 ), GetFormat( 4, 0 ) };        // 事件清零记录
				if (dataArea[2] == 0x30 && dataArea[1] == 0x04 && dataArea[0] == 0x00) return new int[] { GetFormat( 3, 0 ) };                                              // 校时总次数
				if (dataArea[2] == 0x30 && dataArea[1] == 0x04 )                       return new int[] { GetFormat( 4, 0 ), GetFormat( 6, 0, 2 ) };                        // 校时记录
				if (dataArea[2] == 0x30 && dataArea[1] == 0x05 && dataArea[0] == 0x00) return new int[] { GetFormat( 3, 0 ) };                                              // 时段表编程总次数
				if (dataArea[2] == 0x30 && dataArea[1] == 0x05)                        return new int[] { GetFormat( 6, 0 ), GetFormat( 4, 0 ), GetFormat( 3, 0, 14 ) };    // 时段表编程记录
				if (dataArea[2] == 0x30 && dataArea[1] == 0x06 && dataArea[0] == 0x00) return new int[] { GetFormat( 3, 0 ) };                                              // 时区表编程总次数
				if (dataArea[2] == 0x30 && dataArea[1] == 0x06)                        return new int[] { GetFormat( 6, 0 ), GetFormat( 4, 0 ), GetFormat( 3, 0, 28 ) };    // 时段表编程记录
				if (dataArea[2] == 0x30 && dataArea[1] == 0x07 && dataArea[0] == 0x00) return new int[] { GetFormat( 3, 0 ) };                                              // 周休日编程总次数
				if (dataArea[2] == 0x30 && dataArea[1] == 0x07)                        return new int[] { GetFormat( 6, 0 ), GetFormat( 4, 0 ), GetFormat( 1, 0 ) };        // 周休日编程记录
				if (dataArea[2] == 0x30 && dataArea[1] == 0x08 && dataArea[0] == 0x00) return new int[] { GetFormat( 3, 0 ) };                                              // 节假日编程总次数
				if (dataArea[2] == 0x30 && dataArea[1] == 0x08)                        return new int[] { GetFormat( 6, 0 ), GetFormat( 4, 0 ), GetFormat( 4, 0, 254 ) };   // 节假日编程记录
				if (dataArea[2] == 0x30 && dataArea[1] == 0x09 && dataArea[0] == 0x00) return new int[] { GetFormat( 3, 0 ) };                                              // 有功组合编程总次数
				if (dataArea[2] == 0x30 && dataArea[1] == 0x09)                        return new int[] { GetFormat( 6, 0 ), GetFormat( 4, 0 ), GetFormat( 1, 0 ) };        // 有功组合编程记录
				if (dataArea[2] == 0x30 && dataArea[1] == 0x0A && dataArea[0] == 0x00) return new int[] { GetFormat( 3, 0 ) };                                              // 无功组合1编程总次数
				if (dataArea[2] == 0x30 && dataArea[1] == 0x0A)                        return new int[] { GetFormat( 6, 0 ), GetFormat( 4, 0 ), GetFormat( 1, 0 ) };        // 无功组合1编程记录
				if (dataArea[2] == 0x30 && dataArea[1] == 0x0B && dataArea[0] == 0x00) return new int[] { GetFormat( 3, 0 ) };                                              // 无功组合2编程总次数
				if (dataArea[2] == 0x30 && dataArea[1] == 0x0B)                        return new int[] { GetFormat( 6, 0 ), GetFormat( 4, 0 ), GetFormat( 1, 0 ) };        // 无功组合2编程记录
				if (dataArea[2] == 0x30 && dataArea[1] == 0x0C && dataArea[0] == 0x00) return new int[] { GetFormat( 3, 0 ) };                                              // 结算日编程总次数
				if (dataArea[2] == 0x30 && dataArea[1] == 0x0C)                        return new int[] { GetFormat( 6, 0 ), GetFormat( 4, 0 ), GetFormat( 2, 0, 3 ) };     // 结算日编程记录
				if (dataArea[2] == 0x30 && dataArea[1] == 0x0D && dataArea[0] == 0x00) return new int[] { GetFormat( 3, 0 ) };                                              // 开表盖总次数
				if (dataArea[2] == 0x30 && dataArea[1] == 0x0D)                        return new int[] { GetFormat( 6, 0, 2 ), GetFormat( 4, 2, 12 ) };                    // 开表盖记录
				if (dataArea[2] == 0x30 && dataArea[1] == 0x0E && dataArea[0] == 0x00) return new int[] { GetFormat( 3, 0 ) };                                              // 开端钮盒次数
				if (dataArea[2] == 0x30 && dataArea[1] == 0x0E)                        return new int[] { GetFormat( 6, 0, 2 ), GetFormat( 4, 2, 12 ) };                    // 开端钮盒记录

				return null;
			}

			// 参数变量数据标识
			if (dataArea[3] == 0x04)
			{
				if (dataArea[2] == 0x00 && dataArea[1] == 0x01 && dataArea[0] == 0x01) return new int[] { GetFormat( 4, 0 ) };   // 日期及星期
				if (dataArea[2] == 0x00 && dataArea[1] == 0x01 && dataArea[0] == 0x02) return new int[] { GetFormat( 3, 0 ) };   // 时分秒
				if (dataArea[2] == 0x00 && dataArea[1] == 0x01 && dataArea[0] == 0x03) return new int[] { GetFormat( 1, 0 ) };   // 最大需量周期
				if (dataArea[2] == 0x00 && dataArea[1] == 0x01 && dataArea[0] == 0x04) return new int[] { GetFormat( 1, 0 ) };   // 滑差时间
				if (dataArea[2] == 0x00 && dataArea[1] == 0x01 && dataArea[0] == 0x05) return new int[] { GetFormat( 2, 0 ) };   // 校表脉冲宽度
				if (dataArea[2] == 0x00 && dataArea[1] == 0x01 && dataArea[0] == 0x06) return new int[] { GetFormat( 5, 0 ) };   // 两套时区表切换时间
				if (dataArea[2] == 0x00 && dataArea[1] == 0x01 && dataArea[0] == 0x07) return new int[] { GetFormat( 5, 0 ) };   // 两套日时段表切换时间
				if (dataArea[2] == 0x00 && dataArea[1] == 0x02 && dataArea[0] == 0x05) return new int[] { GetFormat( 2, 0 ) };
				if (dataArea[2] == 0x00 && dataArea[1] == 0x02)                        return new int[] { GetFormat( 1, 0 ) };
				if (dataArea[2] == 0x00 && dataArea[1] == 0x03)                        return new int[] { GetFormat( 1, 0 ) };

				if (dataArea[2] == 0x00 && dataArea[1] == 0x04 && dataArea[0] <= 0x02) return new int[] { GetFormat( 6, 0 ) };      // 通信地址，表号
				if (dataArea[2] == 0x00 && dataArea[1] == 0x04 && dataArea[0] == 0x03) return new int[] { GetFormat( 32, -1 ) };    // 资产管理
				if (dataArea[2] == 0x00 && dataArea[1] == 0x04 && dataArea[0] <= 0x06) return new int[] { GetFormat( 6, -1 ) };     // 额定电压，额定电流，最大电流
				if (dataArea[2] == 0x00 && dataArea[1] == 0x04 && dataArea[0] <= 0x08) return new int[] { GetFormat( 4, -1 ) };     // 有功无功准确度等级
				if (dataArea[2] == 0x00 && dataArea[1] == 0x04 && dataArea[0] <= 0x0A) return new int[] { GetFormat( 3, 0 ) };      // 
				if (dataArea[2] == 0x00 && dataArea[1] == 0x04 && dataArea[0] <= 0x0C) return new int[] { GetFormat( 10, -1 ) };    // 电表型号，生产日期
				if (dataArea[2] == 0x00 && dataArea[1] == 0x04 && dataArea[0] == 0x0D) return new int[] { GetFormat( 16, -1 ) };    // 协议版本号
				if (dataArea[2] == 0x00 && dataArea[1] == 0x05)                        return new int[] { GetFormat( 2, 0 ) };
				if (dataArea[2] == 0x00 && dataArea[1] == 0x06)                        return new int[] { GetFormat( 1, 0 ) };
				if (dataArea[2] == 0x00 && dataArea[1] == 0x07)                        return new int[] { GetFormat( 1, 0 ) };
				if (dataArea[2] == 0x00 && dataArea[1] == 0x08)                        return new int[] { GetFormat( 1, 0 ) };
				if (dataArea[2] == 0x00 && dataArea[1] == 0x09)                        return new int[] { GetFormat( 1, 0 ) };
				if (dataArea[2] == 0x00 && dataArea[1] == 0x0A && dataArea[0] == 0x01) return new int[] { GetFormat( 4, 0 ) };      // 负荷记录起始时间
				if (dataArea[2] == 0x00 && dataArea[1] == 0x0A)                        return new int[] { GetFormat( 2, 0 ) };      // 负荷记录间隔时间
				if (dataArea[2] == 0x00 && dataArea[1] == 0x0B)                        return new int[] { GetFormat( 2, 0 ) };      // 每月结算日
				if (dataArea[2] == 0x00 && dataArea[1] == 0x0C)                        return new int[] { GetFormat( 4, 0 ) };      // 各等级密码
				if (dataArea[2] == 0x00 && dataArea[1] == 0x0D)                        return new int[] { GetFormat( 2, 3 ) };      // 各种系数
				if (dataArea[2] == 0x00 && dataArea[1] == 0x0E && dataArea[0] < 0x03)  return new int[] { GetFormat( 3, 4 ) };
				if (dataArea[2] == 0x00 && dataArea[1] == 0x0E)                        return new int[] { GetFormat( 2, 1 ) };
				if (dataArea[2] == 0x01 && dataArea[1] == 0x00)                        return new int[] { GetFormat( 3, 0, 14 ) };
				if (dataArea[2] == 0x02 && dataArea[1] == 0x00)                        return new int[] { GetFormat( 3, 0, 14 ) };
				if (dataArea[2] == 0x03 && dataArea[1] == 0x00)                        return new int[] { GetFormat( 4, 0 ) };
				if (dataArea[2] == 0x04 && dataArea[1] == 0x01)                        return new int[] { GetFormat( 4, 0 ) };
				if (dataArea[2] == 0x04 && dataArea[1] == 0x02)                        return new int[] { GetFormat( 4, 0 ) };
				if (dataArea[2] == 0x80) return new int[] { GetFormat( 32, -1 ) };

				return null;
			}
			if (dataArea[3] == 0x05)
			{
				if (dataArea[2] == 0x00 && dataArea[1] == 0x00 && dataArea[0] == 0x01) return new int[] { GetFormat( 5, 0 ) };   // 日期及星期
				if (dataArea[2] == 0x00 && dataArea[1] == 0x01) return new int[] { GetFormat( 4, 2 ) };                          // 正向有功电能
				if (dataArea[2] == 0x00 && dataArea[1] == 0x02) return new int[] { GetFormat( 4, 2 ) };                          // 反向有功电能
				if (dataArea[2] == 0x00 && dataArea[1] == 0x03) return new int[] { GetFormat( 4, 2 ) };                          // 组合无功1电能
				if (dataArea[2] == 0x00 && dataArea[1] == 0x04) return new int[] { GetFormat( 4, 2 ) };                          // 组合无功2电能
				if (dataArea[2] == 0x00 && dataArea[1] == 0x05) return new int[] { GetFormat( 4, 2 ) };                          // 第一象限无功电能
				if (dataArea[2] == 0x00 && dataArea[1] == 0x06) return new int[] { GetFormat( 4, 2 ) };                          // 第二象限无功电能
				if (dataArea[2] == 0x00 && dataArea[1] == 0x07) return new int[] { GetFormat( 4, 2 ) };                          // 第三象限无功电能
				if (dataArea[2] == 0x00 && dataArea[1] == 0x08) return new int[] { GetFormat( 4, 2 ) };                          // 第四象限无功电能
				if (dataArea[2] == 0x00 && dataArea[1] == 0x09) return new int[] { GetFormat( 3, 4 ), GetFormat( 5, 0 ) };       // 
				if (dataArea[2] == 0x00 && dataArea[1] == 0x0A) return new int[] { GetFormat( 3, 4 ), GetFormat( 5, 0 ) };       // 
				if (dataArea[2] == 0x00 && dataArea[1] == 0x10) return new int[] { GetFormat( 3, 4, 8 ) };                       // 

				return null;
			}
			if (dataArea[3] == 0x06)
			{
				if (dataArea[1] == 0x00 && dataArea[0] == 0x00) return new int[] { GetFormat( 1, 0 ) };   // 最早记录块
				if (dataArea[1] == 0x00 && dataArea[0] == 0x01) return new int[] { GetFormat( 6, 0 ) };   // 给定时间记录块
				if (dataArea[1] == 0x00 && dataArea[0] == 0x02) return new int[] { GetFormat( 1, 0 ) };   // 最近一个记录块

				return null;
			}

			return null;
		}

		/// <summary>
		/// DLT645/1997的地址参数数据格式
		/// </summary>
		/// <param name="dataArea">地址域</param>
		/// <returns>数据信息</returns>
		public static int[] GetDLT1997FormatWithDataArea( byte[] dataArea )
		{
			if (( dataArea[1] & 0xF0 ) == 0x90)
			{
				// 电能量数据标识
				return new int[] { GetFormat( 4, 2 ) };
			}
			else if ((dataArea[1] & 0xF0) == 0xA0)
			{
				// 最大需量数据标识
				return new int[] { GetFormat( 3, 4 ) };
			}
			else if (
				dataArea[1] == 0xB0 ||
				dataArea[1] == 0xB1 ||
				dataArea[1] == 0xB4 ||
				dataArea[1] == 0xB5 ||
				dataArea[1] == 0xB8 ||
				dataArea[1] == 0xB9)
			{
				// 最大需量发生时间数据标识
				return new int[] { GetFormat( 4, 0 ) };
			}
			else if (dataArea[1] == 0xB2)
			{
				if (dataArea[0] == 0x10 || dataArea[0] == 0x11) return new int[] { GetFormat( 4, 0 ) };   // 最近一次编程时间, 最近一次最大需量清零时间
				if (dataArea[0] == 0x12 || dataArea[0] == 0x13) return new int[] { GetFormat( 2, 0 ) };   // 编程次数, 最大需量清零次数
				return new int[] { GetFormat( 3, 0 ) }; // 电池工作时间，单位分钟
			}
			else if (dataArea[1] == 0xB3)
			{
				if ((dataArea[0] & 0xF0) == 0x10) return new int[] { GetFormat( 2, 0 ) }; // 总断相次数
				if ((dataArea[0] & 0xF0) == 0x20) return new int[] { GetFormat( 3, 0 ) }; // 断相时间累计值
				if ((dataArea[0] & 0xF0) == 0x30) return new int[] { GetFormat( 4, 0 ) }; // 最近一次断相起始时刻
				if ((dataArea[0] & 0xF0) == 0x40) return new int[] { GetFormat( 4, 0 ) }; // 最近一次断相结束时刻
			}
			else if (dataArea[1] == 0xB6)
			{
				if ((dataArea[0] & 0xF0) == 0x10) return new int[] { GetFormat( 2, 0 ) }; // 电压
				if ((dataArea[0] & 0xF0) == 0x20) return new int[] { GetFormat( 2, 2 ) }; // 电流
				if (dataArea[0] >= 0x30 && dataArea[0] < 0x34) return new int[] { GetFormat( 3, 4 ) }; // 瞬时有功功率
				if (dataArea[0] == 0x34) return new int[] { GetFormat( 2, 2 ) }; // 正向有功功率上限值
				if (dataArea[0] == 0x35) return new int[] { GetFormat( 2, 2 ) }; // 反向有功功率上限值
				if ((dataArea[0] & 0xF0) == 0x40) return new int[] { GetFormat( 2, 2 ) }; // 瞬时无功功率
				if ((dataArea[0] & 0xF0) == 0x50) return new int[] { GetFormat( 2, 2 ) }; // 总功率因数
			}
			else if (dataArea[1] == 0xC0)
			{
				if (dataArea[0] == 0x10) return new int[] { GetFormat( 4, 0 ) }; // 日期及周次
				if (dataArea[0] == 0x11) return new int[] { GetFormat( 3, 0 ) }; // 时间
				if ((dataArea[0] & 0xF0) == 0x20) return new int[] { GetFormat( 1, 0 ) }; // 电表运行状态字, 电网状态字，周休日状态字
				if (dataArea[0] == 0x30) return new int[] { GetFormat( 3, 0 ) }; // 电表常数(有功)
				if (dataArea[0] == 0x31) return new int[] { GetFormat( 3, 0 ) }; // 电表常数(无功)
				if (dataArea[0] == 0x32) return new int[] { GetFormat( 6, 0 ) }; // 表号
				if (dataArea[0] == 0x33) return new int[] { GetFormat( 6, 0 ) }; // 用户号
				if (dataArea[0] == 0x34) return new int[] { GetFormat( 6, 0 ) }; // 设备码
			}
			else if (dataArea[1] == 0xC1)
			{
				if (dataArea[0] == 0x11) return new int[] { GetFormat( 1, 0 ) }; // 最大需量周期，分钟
				if (dataArea[0] == 0x12) return new int[] { GetFormat( 1, 0 ) }; // 滑差时间，分钟
				if (dataArea[0] == 0x13) return new int[] { GetFormat( 1, 0 ) }; // 循显时间，分钟
				if (dataArea[0] == 0x14) return new int[] { GetFormat( 1, 0 ) }; // 停显时间，分钟
				if (dataArea[0] == 0x15) return new int[] { GetFormat( 1, 0 ) }; // 显示电能小数位数
				if (dataArea[0] == 0x16) return new int[] { GetFormat( 1, 0 ) }; // 显示功率(最大需量)小数位数
				if (dataArea[0] == 0x17) return new int[] { GetFormat( 2, 0 ) }; // 自动抄表日期
				if (dataArea[0] == 0x18) return new int[] { GetFormat( 1, 0 ) }; // 负荷代表日
				if (dataArea[0] == 0x19) return new int[] { GetFormat( 4, 1 ) }; // 有功电能起始读数 kwh
				if (dataArea[0] == 0x1A) return new int[] { GetFormat( 4, 1 ) }; // 无功电能起始读数 kvarh
			}
			else if (dataArea[1] == 0xC2)
			{
				if (dataArea[0] == 0x11) return new int[] { GetFormat( 2, 0 ) }; // 输出脉冲宽度 ms
				if (dataArea[0] == 0x12) return new int[] { GetFormat( 4, 0 ) }; // 密码权限及密码
			}
			else if (dataArea[1] == 0xC3)
			{
				if ((dataArea[0] & 0xF0) == 0x10) return new int[] { GetFormat( 1, 0 ) }; // 年时区数
				return new int[] { GetFormat( 3, 0 ) };
			}
			else if (dataArea[1] == 0xC4)
			{
				if (dataArea[0] == 0x1E) return new int[] { GetFormat( 1, 0 ) }; // 周休日采用的日时段表号
				return new int[] { GetFormat( 3, 0 ) };
			}
			else if (dataArea[1] == 0xC5)
			{
				if (dataArea[0] == 0x10) return new int[] { GetFormat( 4, 0 ) }; // 负荷记录起始时间
				return new int[] { GetFormat( 2, 0 ) };
			}

			return new int[] { GetFormat( 3, 0 ) };
		}
	}
}
