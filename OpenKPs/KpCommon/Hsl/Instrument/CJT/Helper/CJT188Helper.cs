using HslCommunication.BasicFramework;
using HslCommunication.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
#if !NET35 && !NET20
using System.Threading.Tasks;
#endif

namespace HslCommunication.Instrument.CJT.Helper
{
	/// <summary>
	/// CJT188辅助方法
	/// </summary>
	public class CJT188Helper
	{
		/// <summary>
		/// 从用户输入的地址信息中解析出真实的地址及数据标识
		/// </summary>
		/// <param name="address">用户输入的地址信息</param>
		/// <param name="defaultStation">默认的地址域</param>
		/// <returns>解析结果信息</returns>
		public static OperateResult<string, byte[]> AnalysisBytesAddress( string address, string defaultStation )
		{
			try
			{
				string region = defaultStation;
				byte[] dataId = new byte[3];

				if (address.IndexOf( ';' ) > 0)
				{
					string[] splits = address.Split( new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries );
					for (int i = 0; i < splits.Length; i++)
					{
						if (splits[i].StartsWith( "s=" ))
						{
							region = splits[i].Substring( 2 );
						}
						else
						{
							splits[i].ToHexBytes( ).Reverse( ).ToArray( ).CopyTo( dataId, 0 );
						}
					}
				}
				else
				{
					address.ToHexBytes( ).Reverse( ).ToArray( ).CopyTo( dataId, 0 );
				}
				return OperateResult.CreateSuccessResult( region, dataId );
			}
			catch (Exception ex)
			{
				return new OperateResult<string, byte[]>( "Address prase wrong: " + ex.Message );
			}
		}

		/// <summary>
		/// 将地址解析成BCD码的地址，并且扩充到14位，不够的补0操作
		/// </summary>
		/// <param name="address">地址域信息</param>
		/// <returns>实际的结果</returns>
		public static OperateResult<byte[]> GetAddressByteFromString( string address )
		{
			if (address == null || address.Length == 0) return new OperateResult<byte[]>( StringResources.Language.DLTAddressCannotNull );
			if (address.Length > 14) return new OperateResult<byte[]>( StringResources.Language.DLTAddressCannotMoreThan12 );
			if (!Regex.IsMatch( address, "^[0-9A-A]+$" )) return new OperateResult<byte[]>( StringResources.Language.DLTAddressMatchFailed );
			if (address.Length < 14) address = address.PadLeft( 14, '0' );
			return OperateResult.CreateSuccessResult( address.ToHexBytes( ).Reverse( ).ToArray( ) );
		}

		/// <summary>
		/// 将指定的地址信息，控制码信息，数据域信息打包成完整的报文命令
		/// </summary>
		/// <param name="address">地址域信息，地址域由7个字节构成，每字节2位BCD码，地址长度可达14位十进制数。地址域支持锁位寻址，即从若干低位起，剩余高位补AAH作为通配符进行读表操作</param>
		/// <param name="type">仪表类型</param>
		/// <param name="control">控制码信息</param>
		/// <param name="dataArea">数据域的内容</param>
		/// <returns>返回是否报文创建成功</returns>
		public static OperateResult<byte[]> Build188EntireCommand( string address, byte type, byte control, byte[] dataArea )
		{
			if (dataArea == null) dataArea = new byte[0];
			OperateResult<byte[]> add = GetAddressByteFromString( address );
			if (!add.IsSuccess) return add;

			byte[] buffer = new byte[13 + dataArea.Length];
			buffer[0] = 0x68;                                  // 帧起始符
			buffer[1] = type;                                  // 仪表类型
			add.Content.CopyTo( buffer, 2 );                   // BCD码的地址信息
			buffer[9] = control;                               // 控制码
			buffer[10] = (byte)dataArea.Length;                // 数据域长度，读的时候小于等于200，写的时候，小于等于50
			if (dataArea.Length > 0)
			{
				dataArea.CopyTo( buffer, 11 );
				//for (int i = 0; i < dataArea.Length; i++)
				//{
				//	// 数据域，发送之前增加0x33
				//	buffer[i + 10] += 0x33;
				//}
			}

			// 求校验码
			int count = 0;
			for (int i = 0; i < buffer.Length - 2; i++)
			{
				count += buffer[i];
			}
			buffer[buffer.Length - 2] = (byte)count;           // 校验码
			buffer[buffer.Length - 1] = AsciiControl.SYN;      // 结束符
			return OperateResult.CreateSuccessResult( buffer );
		}


		/// <summary>
		/// 检查当前的反馈数据信息是否正确
		/// </summary>
		/// <param name="cjt">CJT188的通信对象</param>
		/// <param name="response">从仪表反馈的数据信息</param>
		/// <returns>是否校验成功</returns>
		public static OperateResult CheckResponse( ICjt188 cjt, byte[] response )
		{
			if (response.Length < 13) return new OperateResult( StringResources.Language.ReceiveDataLengthTooShort );
			if ((response[9] & 0x40) == 0x40)
			{
				// 异常的响应
				byte error = response[12];
				if (error.GetBoolByIndex( 0 )) return new OperateResult( "阀门关" );
				if (error.GetBoolByIndex( 1 )) return new OperateResult( "阀门异常" );
				if (error.GetBoolByIndex( 2 )) return new OperateResult( "电池欠压" );
				if (error > 0) return new OperateResult( error, "厂商定义的异常" );
				return OperateResult.CreateSuccessResult( );
			}
			else
			{
				// 正常的响应
				return OperateResult.CreateSuccessResult( );
			}
		}

		private static OperateResult<byte[]> ReadWithAddress( ICjt188 cjt, string address, byte[] dataArea )
		{
			OperateResult<byte[]> command = Build188EntireCommand( address, cjt.InstrumentType, CJTControl.ReadData, dataArea );
			if (!command.IsSuccess) return command;

			OperateResult<byte[]> read = cjt.ReadFromCoreServer( command.Content );
			if (!read.IsSuccess) return read;

			OperateResult check = CheckResponse( cjt, read.Content );
			if (!check.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( check );

			if (dataArea[0] == 0x0A && dataArea[1] == 0x81) return OperateResult.CreateSuccessResult( read.Content.SelectMiddle( 2, 7 ).Reverse( ).ToArray( ) );
			if (read.Content.Length < 16) return OperateResult.CreateSuccessResult( new byte[0] );
			return OperateResult.CreateSuccessResult( read.Content.SelectMiddle( 14, read.Content[10] - 3 ) );
		}
#if !NET35 && !NET20
		private static async Task<OperateResult<byte[]>> ReadWithAddressAsync( ICjt188 cjt, string address, byte[] dataArea )
		{
			OperateResult<byte[]> command = Build188EntireCommand( address, cjt.InstrumentType, CJTControl.ReadData, dataArea );
			if (!command.IsSuccess) return command;

			OperateResult<byte[]> read = await cjt.ReadFromCoreServerAsync( command.Content );
			if (!read.IsSuccess) return read;

			OperateResult check = CheckResponse( cjt, read.Content );
			if (!check.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( check );

			if (dataArea[0] == 0x0A && dataArea[1] == 0x81) return OperateResult.CreateSuccessResult( read.Content.SelectMiddle( 2, 7 ).Reverse( ).ToArray( ) );
			if (read.Content.Length < 16) return OperateResult.CreateSuccessResult( new byte[0] );
			return OperateResult.CreateSuccessResult( read.Content.SelectMiddle( 14, read.Content[10] - 3 ) );
		}
#endif

		#region Static Helper

		/// <summary>
		/// 根据指定的数据标识来读取相关的原始数据信息，地址标识根据手册来，从高位到地位，例如 91-1F，分割符可以任意特殊字符或是没有分隔符。<br />
		/// Read the relevant original data information according to the specified data identifier. The address identifier is based on the manual, 
		/// from high to position, such as 91-1F. The separator can be any special character or no separator.
		/// </summary>
		/// <remarks>
		/// 地址可以携带地址域信息，例如 "s=2;90-1F" 或是 "s=100000;90-1F"，关于数据域信息，需要查找手册，例如:D1-20 表示： 上一月结算日累积流量
		/// </remarks>
		/// <param name="cjt">CJT188的通信对象</param>
		/// <param name="address">数据标识，具体需要查找手册来对应</param>
		/// <param name="length">数据长度信息</param>
		/// <returns>结果信息</returns>
		public static OperateResult<byte[]> Read( ICjt188 cjt, string address, ushort length )
		{
			OperateResult<string, byte[]> analysis = AnalysisBytesAddress( address, cjt.Station );
			if (!analysis.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( analysis );

			return ReadWithAddress( cjt, analysis.Content1, analysis.Content2 );
		}


		/// <summary>
		/// 根据指定的数据标识来写入相关的原始数据信息，地址标识根据手册来，从高位到地位，例如 90-1F，分割符可以任意特殊字符或是没有分隔符。<br />
		/// Read the relevant original data information according to the specified data identifier. The address identifier is based on the manual, 
		/// from high to position, such as 90-1F. The separator can be any special character or no separator.
		/// </summary>
		/// <remarks>
		/// 地址可以携带地址域信息，例如 "s=2;90-1F" 或是 "s=100000;90-1F"，关于数据域信息，需要查找手册，例如:00-01-00-00 表示： (当前)正向有功总电能<br />
		/// 注意：本命令必须与编程键配合使用
		/// </remarks>
		/// <param name="cjt">CJT188的通信对象</param>
		/// <param name="address">地址信息</param>
		/// <param name="value">写入的数据值</param>
		/// <returns>是否写入成功</returns>
		public static OperateResult Write( ICjt188 cjt, string address, byte[] value )
		{
			OperateResult<string, byte[]> analysis = AnalysisBytesAddress( address, cjt.Station );
			if (!analysis.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( analysis );

			byte[] content = SoftBasic.SpliceArray( analysis.Content2, value );

			OperateResult<byte[]> command = Build188EntireCommand( analysis.Content1, cjt.InstrumentType, CJTControl.WriteData, content );
			if (!command.IsSuccess) return command;

			OperateResult<byte[]> read = cjt.ReadFromCoreServer( command.Content );
			if (!read.IsSuccess) return read;

			return CheckResponse( cjt, read.Content );
		}

		/// <summary>
		/// 读取数据的数组信息，需要指定如何从字符串转换的功能方法
		/// </summary>
		/// <typeparam name="T">类型信息</typeparam>
		/// <param name="cjt">CJT188的通信对象</param>
		/// <param name="address">地址信息</param>
		/// <param name="length">数据长度</param>
		/// <param name="trans">转换方法</param>
		/// <returns>包含泛型数组的结果对象</returns>
		public static OperateResult<T[]> ReadValue<T>( ICjt188 cjt, string address, ushort length, Func<string, T> trans )
		{
			OperateResult<string[]> read = ReadStringArray( cjt, address );
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<T[]>( read );

			return OperateResult.CreateSuccessResult( read.Content.SelectBegin( length ).Select( m => trans( m ) ).ToArray( ) );
		}

		#endregion


		/// <summary>
		/// 读取设备的通信地址，仅支持点对点通讯的情况，返回地址域数据，例如：14910000729012<br />
		/// Read the communication address of the device, only support point-to-point communication, and return the address field data, for example: 149100007290
		/// </summary>
		/// <param name="cjt">CJT188的通信对象</param>
		/// <returns>设备的通信地址</returns>
		public static OperateResult<string> ReadAddress( ICjt188 cjt )
		{
			OperateResult<byte[]> command = Build188EntireCommand( "AAAAAAAAAAAAAA", cjt.InstrumentType, CJTControl.ReadAddress, new byte[] { 0x0A, 0x81, 0x00 } );
			if (!command.IsSuccess) return OperateResult.CreateFailedResult<string>( command );

			OperateResult<byte[]> read = cjt.ReadFromCoreServer( command.Content );
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<string>( read );

			OperateResult check = CheckResponse( cjt, read.Content );
			if (!check.IsSuccess) return OperateResult.CreateFailedResult<string>( check );

			cjt.Station = read.Content.SelectMiddle( 2, 7 ).Reverse( ).ToArray( ).ToHexString( );
			return OperateResult.CreateSuccessResult( read.Content.SelectMiddle( 2, 7 ).Reverse( ).ToArray( ).ToHexString( ) );
		}


		/// <summary>
		/// 写入设备的地址域信息，仅支持点对点通讯的情况，需要指定地址域信息，例如：14910000729011<br />
		/// Write the address domain information of the device, only support point-to-point communication, 
		/// you need to specify the address domain information, for example: 14910000729011
		/// </summary>
		/// <param name="cjt">CJT188的通信对象</param>
		/// <param name="address">等待写入的地址域</param>
		/// <returns>是否写入成功</returns>
		public static OperateResult WriteAddress( ICjt188 cjt, string address )
		{
			OperateResult<byte[]> add = CJT188Helper.GetAddressByteFromString( address );
			if (!add.IsSuccess) return add;

			OperateResult<byte[]> command = CJT188Helper.Build188EntireCommand( cjt.Station, cjt.InstrumentType, CJTControl.WriteAddress, add.Content );
			if (!command.IsSuccess) return command;

			OperateResult<byte[]> read = cjt.ReadFromCoreServer( command.Content );
			if (!read.IsSuccess) return read;

			OperateResult check = CheckResponse( cjt, read.Content );
			if (!check.IsSuccess) return check;

			if (SoftBasic.IsTwoBytesEquel( read.Content.SelectMiddle( 2, 7 ), GetAddressByteFromString( address ).Content ))
				return OperateResult.CreateSuccessResult( );
			else
				return new OperateResult( StringResources.Language.DLTErrorWriteReadCheckFailed );
		}


		private static OperateResult<double, string> GetActualValueAndUnit( byte[] source, int index, int length, double scale, bool hasUnit, string defaultUnit )
		{
			string temp = source.SelectMiddle( index, length ).Reverse( ).ToArray( ).ToHexString( );
			double unitScale = 1d;
			if (hasUnit)  // 说明有数据单位信息
			{
				switch (source[index + length])
				{
					case 0x02: defaultUnit = "Wh";   unitScale = 1d; break;
					case 0x03: defaultUnit = "Wh";   unitScale = 10d; break;
					case 0x04: defaultUnit = "Wh";   unitScale = 100d; break;
					case 0x05: defaultUnit = "Wh";   unitScale = 1000d; break;
					case 0x06: defaultUnit = "Wh";   unitScale = 10000d; break;
					case 0x07: defaultUnit = "Wh";   unitScale = 100000d; break;
					case 0x08: defaultUnit = "Wh";   unitScale = 1000000d; break;
					case 0x09: defaultUnit = "Wh";   unitScale = 10000000d; break;
					case 0x0A: defaultUnit = "Wh";   unitScale = 100000000d; break;
					case 0x01: defaultUnit = "J";    unitScale = 1d; break;
					case 0x0B: defaultUnit = "J";    unitScale = 1000d; break;
					case 0x0C: defaultUnit = "J";    unitScale = 10000d; break;
					case 0x0D: defaultUnit = "J";    unitScale = 100000d; break;
					case 0x0E: defaultUnit = "J";    unitScale = 1000000d; break;
					case 0x0F: defaultUnit = "J";    unitScale = 10000000d; break;
					case 0x10: defaultUnit = "J";    unitScale = 100000000d; break;
					case 0x11: defaultUnit = "J";    unitScale = 1000000000d; break;
					case 0x12: defaultUnit = "J";    unitScale = 10000000000d; break;
					case 0x13: defaultUnit = "J";    unitScale = 100000000000d; break;
					case 0x29: defaultUnit = "L";    unitScale = 1d; break;
					case 0x2A: defaultUnit = "L";    unitScale = 10d; break;
					case 0x2B: defaultUnit = "L";    unitScale = 100d; break;
					case 0x2C: defaultUnit = "m³";   unitScale = 1d; break;
					case 0x2D: defaultUnit = "m³";   unitScale = 10d; break;
					case 0x2E: defaultUnit = "m³";   unitScale = 100d; break;
					case 0x14: defaultUnit = "W";    unitScale = 1d; break;
					case 0x15: defaultUnit = "W";    unitScale = 10d; break;
					case 0x16: defaultUnit = "W";    unitScale = 100d; break;
					case 0x17: defaultUnit = "W";    unitScale = 1000d; break;
					case 0x18: defaultUnit = "W";    unitScale = 10000d; break;
					case 0x19: defaultUnit = "W";    unitScale = 100000d; break;
					case 0x1A: defaultUnit = "W";    unitScale = 1000000d; break;
					case 0x1B: defaultUnit = "W";    unitScale = 10000000d; break;
					case 0x1C: defaultUnit = "W";    unitScale = 100000000d; break;
					case 0x40: defaultUnit = "J/h";  unitScale = 1d; break;
					case 0x43: defaultUnit = "J/h";  unitScale = 1000d; break;
					case 0x44: defaultUnit = "J/h";  unitScale = 10000d; break;
					case 0x45: defaultUnit = "J/h";  unitScale = 100000d; break;
					case 0x46: defaultUnit = "J/h";  unitScale = 1000000d; break;
					case 0x47: defaultUnit = "J/h";  unitScale = 10000000d; break;
					case 0x48: defaultUnit = "J/h";  unitScale = 100000000d; break;
					case 0x49: defaultUnit = "J/h";  unitScale = 1000000000d; break;
					case 0x4A: defaultUnit = "J/h";  unitScale = 10000000000d; break;
					case 0x4B: defaultUnit = "J/h";  unitScale = 100000000000d; break;
					case 0x32: defaultUnit = "L/h";  unitScale = 1d; break;
					case 0x33: defaultUnit = "L/h";  unitScale = 10d; break;
					case 0x34: defaultUnit = "L/h";  unitScale = 100d; break;
					case 0x35: defaultUnit = "m³/h"; unitScale = 1d; break;
					case 0x36: defaultUnit = "m³/h"; unitScale = 10d; break;
					case 0x37: defaultUnit = "m³/h"; unitScale = 100d; break;
					default: break;
				}

			}
			if (temp.Contains( "FF" )) return OperateResult.CreateSuccessResult( double.NaN, defaultUnit );
			return OperateResult.CreateSuccessResult( Convert.ToDouble( temp ) / scale * unitScale, defaultUnit );
		}

		private static string GetDateTime( byte[] source, int index )
		{
			return $"{source.SelectMiddle( index, 2 ).ToHexString( )}-{source.SelectMiddle( index + 2, 1 ).ToHexString( )}-{source.SelectMiddle( index + 3, 1 ).ToHexString( )}" +
				$" {source.SelectMiddle( index + 4, 1 ).ToHexString( )}:{source.SelectMiddle( index + 5, 1 ).ToHexString( )}:{source.SelectMiddle( index + 6, 1 ).ToHexString( )}";
		}

		private static string GetUnitScale( string unit, double unitScale )
		{
			if (unitScale == 1) return unit;
			if (unitScale == 10) return unit + "*10";
			if (unitScale == 100) return unit + "*100";
			if (unitScale == 1000) return "k" + unit;
			if (unitScale == 10000) return "k" + unit + "*10";
			if (unitScale == 100000) return "k" + unit + "*100";
			if (unitScale == 1000000) return "M" + unit;
			if (unitScale == 10000000) return "M" + unit + "*10";
			if (unitScale == 100000000) return "M" + unit + "*100";
			if (unitScale == 1000000000) return "G" + unit;
			if (unitScale == 10000000000) return "G" + unit + "*10";
			if (unitScale == 100000000000) return "G" + unit + "*100";
			return unit + "*" + unitScale.ToString( );
		}

		private static string[] TransStringsFromCJT( ICjt188 cjt, byte[] source, ushort dido )
		{
			if (dido == 0x901F)
			{
				var value1 = GetActualValueAndUnit( source, 0, 4, 100, true, string.Empty );
				var value2 = GetActualValueAndUnit( source, 5, 4, 100, true, string.Empty );
				return new string[] { value1.Content1.ToString( ), value1.Content2, value2.Content1.ToString( ), value2.Content2,
					GetDateTime(source, 10), BitConverter.ToUInt16(source, 17).ToString( ) };
			}
			else if (dido >= 0xD120 && dido < 0xD130)
			{
				var value1 = GetActualValueAndUnit( source, 0, 4, 100, true, string.Empty );
				return new string[] { value1.Content1.ToString( ), value1.Content2 };
			}
			else if (dido == 0x8102)
			{
				List<string> list = new List<string>( );
				for (int i = 0; i < 3; i++)
				{
					var value1 = GetActualValueAndUnit( source, i * 6, 3, 100, false, "元/单位用量" );
					var value2 = GetActualValueAndUnit( source, i * 6 + 3, 3, 1, false, "m³" );

					list.Add( value1.Content1.ToString( ) );
					list.Add( value1.Content2 );

					list.Add( value2.Content1.ToString( ) );
					list.Add( value2.Content2 );
				}
				return list.ToArray( );
			}
			else if (dido == 0x8103 || dido == 0x8104)
			{
				return new string[] { source.ToHexString( ) };
			}
			else if (dido == 0x8105)
			{
				return new string[]
				{
					source.SelectMiddle( 0, 1 ).ToHexString( ),
					GetActualValueAndUnit( source, 1, 4, 2, false, "" ).Content1.ToString( ),
					GetActualValueAndUnit( source, 5, 4, 2, false, "" ).Content1.ToString( ),
					GetActualValueAndUnit( source, 9, 4, 2, false, "" ).Content1.ToString( ),
					BitConverter.ToUInt16( source, 13).ToString( )
				};
			}
			else if (dido == 0x8106)
			{
				return new string[] { source.SelectMiddle( 0, 1 ).ToHexString( ) };
			}
			else if (dido == 0x810A)
			{
				return new string[] { source.ToHexString( ) };
			}
			else
			{
				return new string[] { source.ToHexString( ) };
			}
		}

		/// <summary>
		/// 读取指定地址的所有的字符串数据信息，一般来说，一个地址只有一个数据
		/// </summary>
		/// <remarks>
		/// 地址可以携带地址域信息，例如 "s=2;90-1F" 或是 "s=100000;90-1F"
		/// </remarks>
		/// <param name="cjt">CJT通信对象</param>
		/// <param name="address">数据标识，具体需要查找手册来对应</param>
		/// <returns>字符串数组信息</returns>
		public static OperateResult<string[]> ReadStringArray( ICjt188 cjt, string address )
		{
			OperateResult<string, byte[]> analysis = AnalysisBytesAddress( address, cjt.Station );
			if (!analysis.IsSuccess) return OperateResult.CreateFailedResult<string[]>( analysis );

			OperateResult<byte[]> read = ReadWithAddress( cjt, analysis.Content1, analysis.Content2 );
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<string[]>( read );

			return OperateResult.CreateSuccessResult( TransStringsFromCJT( cjt, read.Content, BitConverter.ToUInt16( analysis.Content2, 0 ) ) );
		}

	}
}
