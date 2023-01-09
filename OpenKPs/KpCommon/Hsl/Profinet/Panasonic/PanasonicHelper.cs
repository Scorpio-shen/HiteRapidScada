using HslCommunication.BasicFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
#if !NET35 && !NET20
using System.Threading.Tasks;
#endif

namespace HslCommunication.Profinet.Panasonic
{
	/// <summary>
	/// 松下PLC的辅助类，提供了基本的辅助方法，用于解析地址，计算校验和，创建报文<br />
	/// The auxiliary class of Panasonic PLC provides basic auxiliary methods for parsing addresses, calculating checksums, and creating messages
	/// </summary>
	public class PanasonicHelper
	{
		#region Static Helper

		private static string CalculateCrc( StringBuilder sb )
		{
			byte tmp = 0;
			tmp = (byte)sb[0];
			for (int i = 1; i < sb.Length; i++)
			{
				tmp ^= (byte)sb[i];
			}
			return BasicFramework.SoftBasic.ByteToHexString( new byte[] { tmp } );
		}

		/// <summary>
		/// 位地址转换方法，101等同于10.1等同于10*16+1=161<br />
		/// Bit address conversion method, 101 is equivalent to 10.1 is equivalent to 10 * 16 + 1 = 161
		/// </summary>
		/// <param name="address">地址信息</param>
		/// <param name="fromBase">倍率信息</param>
		/// <returns>实际的位地址信息</returns>
		public static int CalculateComplexAddress( string address, int fromBase = 16 )
		{
			int add = 0;
			if (address.IndexOf( "." ) < 0)
			{
				if(address.Length == 1)
					add = Convert.ToInt32( address, fromBase );
				else
					add = Convert.ToInt32( address.Substring( 0, address.Length - 1 ) ) * fromBase + Convert.ToInt32( address.Substring( address.Length - 1 ), fromBase );
			}
			else
			{
				add = Convert.ToInt32( address.Substring( 0, address.IndexOf( "." ) ) ) * fromBase;
				string bit = address.Substring( address.IndexOf( "." ) + 1 );
				add += HslCommunication.Core.HslHelper.CalculateBitStartIndex( bit );
			}
			return add;
		}

		/// <summary>
		/// 解析数据地址，解析出地址类型，起始地址<br />
		/// Parse the data address, resolve the address type, start address
		/// </summary>
		/// <param name="address">数据地址</param>
		/// <returns>解析出地址类型，起始地址</returns>
		public static OperateResult<string, int> AnalysisAddress( string address )
		{
			var result = new OperateResult<string, int>( );
			try
			{
				result.Content2 = 0;
				if (address.StartsWith( "IX" ) || address.StartsWith( "ix" ))
				{
					result.Content1 = "IX";
					result.Content2 = int.Parse( address.Substring( 2 ) );
				}
				else if (address.StartsWith( "IY" ) || address.StartsWith( "iy" ))
				{
					result.Content1 = "IY";
					result.Content2 = int.Parse( address.Substring( 2 ) );
				}
				else if (address.StartsWith( "ID" ) || address.StartsWith( "id" ))
				{
					result.Content1 = "ID";
					result.Content2 = int.Parse( address.Substring( 2 ) );
				}
				else if (address.StartsWith( "SR" ) || address.StartsWith( "sr" ))
				{
					result.Content1 = "SR";
					result.Content2 = CalculateComplexAddress( address.Substring( 2 ) );
				}
				else if (address.StartsWith( "LD" ) || address.StartsWith( "ld" ))
				{
					result.Content1 = "LD";
					result.Content2 = int.Parse( address.Substring( 2 ) );
				}
				else if (address[0] == 'X' || address[0] == 'x')
				{
					result.Content1 = "X";
					result.Content2 = CalculateComplexAddress( address.Substring( 1 ) );
				}
				else if (address[0] == 'Y' || address[0] == 'y')
				{
					result.Content1 = "Y";
					result.Content2 = CalculateComplexAddress( address.Substring( 1 ) );
				}
				else if (address[0] == 'R' || address[0] == 'r')
				{
					result.Content1 = "R";
					result.Content2 = CalculateComplexAddress( address.Substring( 1 ) );
				}
				else if (address[0] == 'T' || address[0] == 't')
				{
					result.Content1 = "T";
					result.Content2 = int.Parse( address.Substring( 1 ) );
				}
				else if (address[0] == 'C' || address[0] == 'c')
				{
					result.Content1 = "C";
					result.Content2 = int.Parse( address.Substring( 1 ) );
				}
				else if (address[0] == 'L' || address[0] == 'l')
				{
					result.Content1 = "L";
					result.Content2 = CalculateComplexAddress( address.Substring( 1 ) );
				}
				else if (address[0] == 'D' || address[0] == 'd')
				{
					result.Content1 = "D";
					result.Content2 = int.Parse( address.Substring( 1 ) );
				}
				else if (address[0] == 'F' || address[0] == 'f')
				{
					result.Content1 = "F";
					result.Content2 = int.Parse( address.Substring( 1 ) );
				}
				else if (address[0] == 'S' || address[0] == 's')
				{
					result.Content1 = "S";
					result.Content2 = int.Parse( address.Substring( 1 ) );
				}
				else if (address[0] == 'K' || address[0] == 'k')
				{
					result.Content1 = "K";
					result.Content2 = int.Parse( address.Substring( 1 ) );
				}
				else
				{
					throw new Exception( StringResources.Language.NotSupportedDataType );
				}
			}
			catch (Exception ex)
			{
				result.Message = ex.Message;
				return result;
			}

			result.IsSuccess = true;
			return result;
		}

		/// <summary>
		/// 将松下的命令打包成带有%开头，CRC校验，CR结尾的完整的命令报文。如果参数 <c>useExpandedHeader</c> 设置为 <c>Ture</c>，则命令头使用 &lt; 开头
		/// </summary>
		/// <param name="station">站号信息</param>
		/// <param name="cmd">松下的命令。例如 RCSR100F</param>
		/// <param name="useExpandedHeader">设置是否使用扩展的命令头消息</param>
		/// <returns>原始的字节数组的命令</returns>
		public static OperateResult<byte[]> PackPanasonicCommand( byte station, string cmd, bool useExpandedHeader )
		{
			StringBuilder sb = new StringBuilder( useExpandedHeader ? "<" : "%" );
			sb.Append( station.ToString( "X2" ) );
			sb.Append( cmd );                           // 追加命令，例如 RCSR100F, RCP2R100F
			sb.Append( CalculateCrc( sb ) );
			sb.Append( '\u000D' );
			return OperateResult.CreateSuccessResult( Encoding.ASCII.GetBytes( sb.ToString( ) ) );
		}

		private static OperateResult AppendCoil( StringBuilder sb, string address )
		{
			// 解析地址
			OperateResult<string, int> analysis = AnalysisAddress( address );
			if (!analysis.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( analysis );

			sb.Append( analysis.Content1 );
			if (analysis.Content1 == "X" || analysis.Content1 == "Y" || analysis.Content1 == "R" || analysis.Content1 == "L")
			{
				sb.Append( (analysis.Content2 / 16).ToString( "D3" ) );
				sb.Append( (analysis.Content2 % 16).ToString( "X1" ) );
			}
			else if (analysis.Content1 == "T" || analysis.Content1 == "C")
			{
				sb.Append( "0" );
				sb.Append( analysis.Content2.ToString( "D3" ) );
			}
			else
			{
				return new OperateResult<byte[]>( StringResources.Language.NotSupportedDataType );
			}

			return OperateResult.CreateSuccessResult( );
		}

		/// <summary>
		/// 创建读取离散触点的报文指令<br />
		/// Create message instructions for reading discrete contacts
		/// </summary>
		/// <param name="station">站号信息</param>
		/// <param name="address">地址信息</param>
		/// <returns>包含是否成功的结果对象</returns>
		public static OperateResult<byte[]> BuildReadOneCoil( byte station, string address )
		{
			// 参数检查
			if (address == null) return new OperateResult<byte[]>( "address is not allowed null" );
			if (address.Length < 1 || address.Length > 8) return new OperateResult<byte[]>( "length must be 1-8" );

			StringBuilder sb = new StringBuilder( "#RCS" );
			// 增加地址
			OperateResult append = AppendCoil( sb, address );
			if (!append.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( append );

			return PackPanasonicCommand( station, sb.ToString( ), false );
		}

		/// <summary>
		/// 创建读取多个bool值得报文命令
		/// </summary>
		/// <param name="station">站号信息</param>
		/// <param name="address">等待读取的地址数组</param>
		/// <returns>包含是否成功的结果对象</returns>
		public static OperateResult<List<byte[]>> BuildReadCoils( byte station, string[] address )
		{
			List<byte[]> list = new List<byte[]>( );
			// 分割成8个长度的单元的指令
			List<string[]> splits = SoftBasic.ArraySplitByLength( address, 8 );
			for (int i = 0; i < splits.Count; i++)
			{
				StringBuilder sb = new StringBuilder( "#RCP" );
				sb.Append( splits[i].Length.ToString( ) );        // 指定多个触点
				for (int j = 0; j < splits[i].Length; j++)
				{
					// 增加地址
					OperateResult append = AppendCoil( sb, splits[i][j] );
					if (!append.IsSuccess) return OperateResult.CreateFailedResult<List<byte[]>>( append );
				}

				list.Add( PackPanasonicCommand( station, sb.ToString( ), false ).Content );
			}
			return OperateResult.CreateSuccessResult( list );
		}

		/// <summary>
		/// 创建写入离散触点的报文指令<br />
		/// Create message instructions to write discrete contacts
		/// </summary>
		/// <param name="station">站号信息</param>
		/// <param name="address">地址信息</param>
		/// <param name="value">bool值数组</param>
		/// <returns>包含是否成功的结果对象</returns>
		public static OperateResult<byte[]> BuildWriteOneCoil( byte station, string address, bool value )
		{
			StringBuilder sb = new StringBuilder( "#WCS" );

			// 增加地址
			OperateResult append = AppendCoil( sb, address );
			if (!append.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( append );

			sb.Append( value ? '1' : '0' );
			return PackPanasonicCommand( station, sb.ToString( ), false );
		}

		/// <summary>
		/// 创建写入多个离散触点的报文指令
		/// </summary>
		/// <param name="station">站号信息</param>
		/// <param name="address">等待写入的地址列表</param>
		/// <param name="value">等待写入的值列表，长度应和地址长度一致</param>
		/// <returns>所有写入命令的报文列表</returns>
		public static OperateResult<List<byte[]>> BuildWriteCoils( byte station, string[] address, bool[] value )
		{
			if (address == null) return new OperateResult<List<byte[]>>( "Parameter address can't be null" );
			if (value == null) return new OperateResult<List<byte[]>>( "Parameter value can't be null" );
			if (address.Length != value.Length) return new OperateResult<List<byte[]>>( "Parameter address and parameter value, length is not same!" );
			List<byte[]> list = new List<byte[]>( );
			List<string[]> splits_add   = SoftBasic.ArraySplitByLength( address, 8 );
			List<bool[]>   splits_value = SoftBasic.ArraySplitByLength( value, 8 );
			for (int i = 0; i < splits_add.Count; i++)
			{
				StringBuilder sb = new StringBuilder( "#WCP" );
				sb.Append( splits_add[i].Length.ToString( ) );        // 指定多个触点
				for (int j = 0; j < splits_add[i].Length; j++)
				{
					// 增加地址
					OperateResult append = AppendCoil( sb, splits_add[i][j] );
					if (!append.IsSuccess) return OperateResult.CreateFailedResult<List<byte[]>>( append );

					sb.Append( splits_value[i][j] ? '1' : '0' );
				}

				list.Add( PackPanasonicCommand( station, sb.ToString( ), false ).Content );
			}
			return OperateResult.CreateSuccessResult( list );
		}

		/// <summary>
		/// 创建批量读取触点的报文指令<br />
		/// Create message instructions for batch reading contacts
		/// </summary>
		/// <param name="station">站号信息</param>
		/// <param name="address">地址信息</param>
		/// <param name="length">数据长度</param>
		/// <param name="isBit">是否进行位为单位</param>
		/// <returns>包含是否成功的结果对象</returns>
		public static OperateResult<List<byte[]>> BuildReadCommand( byte station, string address, ushort length, bool isBit )
		{
			// 参数检查
			if (address == null) return new OperateResult<List<byte[]>>( StringResources.Language.PanasonicAddressParameterCannotBeNull );

			// 解析地址
			OperateResult<string, int> analysis = AnalysisAddress( address );
			if (!analysis.IsSuccess) return OperateResult.CreateFailedResult<List<byte[]>>( analysis );

			List<byte[]> list = new List<byte[]>( );
			if (isBit)
			{
				length += (ushort)(analysis.Content2 % 16);
				analysis.Content2 -= analysis.Content2 % 16;
				int[] splits = SoftBasic.SplitIntegerToArray( length, 25 * 16 );

				for (int i = 0; i < splits.Length; i++)
				{
					int len = splits[i];

					StringBuilder sb = new StringBuilder( "#" );
					if (analysis.Content1 == "X" || analysis.Content1 == "Y" || analysis.Content1 == "R" || analysis.Content1 == "L")
					{
						sb.Append( "RCC" );
						sb.Append( analysis.Content1 );

						int wordStart = analysis.Content2 / 16;
						int wordFinish = (analysis.Content2 + len - 1) / 16;
						sb.Append( wordStart.ToString( "D4" ) );
						sb.Append( wordFinish.ToString( "D4" ) );

						analysis.Content2 += len;
					}
					else
						return new OperateResult<List<byte[]>>( "Bit read only support X,Y,R,L" );

					list.Add( PackPanasonicCommand( station, sb.ToString( ), false ).Content );
				}
				return OperateResult.CreateSuccessResult( list );
			}
			else
			{
				int[] splits = SoftBasic.SplitIntegerToArray( length, 500 );
				for (int i = 0; i < splits.Length; i++)
				{
					int len = splits[i];

					StringBuilder sb = new StringBuilder( "#" );
					if (analysis.Content1 == "X" || analysis.Content1 == "Y" || analysis.Content1 == "R" || analysis.Content1 == "L")
					{
						sb.Append( "RCC" );
						sb.Append( analysis.Content1 );

						int wordStart = analysis.Content2 / 16;
						int wordFinish = (analysis.Content2 + (len - 1) * 16) / 16;
						sb.Append( wordStart.ToString( "D4" ) );
						sb.Append( wordFinish.ToString( "D4" ) );

						analysis.Content2 += len * 16;
					}
					else if (analysis.Content1 == "D" || analysis.Content1 == "LD" || analysis.Content1 == "F")
					{
						sb.Append( "RD" );
						sb.Append( analysis.Content1.Substring( 0, 1 ) );

						sb.Append( analysis.Content2.ToString( "D5" ) );
						sb.Append( (analysis.Content2 + len - 1).ToString( "D5" ) );

						analysis.Content2 += len;
					}
					else if (analysis.Content1 == "IX" || analysis.Content1 == "IY" || analysis.Content1 == "ID")
					{
						sb.Append( "RD" );
						sb.Append( analysis.Content1 );
						sb.Append( "000000000" );

						analysis.Content2 += len;
					}
					else if (analysis.Content1 == "C" || analysis.Content1 == "T")
					{
						sb.Append( "RS" );
						sb.Append( analysis.Content2.ToString( "D4" ) );
						sb.Append( (analysis.Content2 + len - 1).ToString( "D4" ) );

						analysis.Content2 += len;
					}
					else if (analysis.Content1 == "K" || analysis.Content1 == "S")
					{
						// 经过值及目标值
						sb.Append( "R" );
						sb.Append( analysis.Content1 );
						sb.Append( analysis.Content2.ToString( "D4" ) );
						sb.Append( (analysis.Content2 + len - 1).ToString( "D4" ) );

						analysis.Content2 += len;
					}
					else
					{
						return new OperateResult<List<byte[]>>( StringResources.Language.NotSupportedDataType );
					}

					list.Add( PackPanasonicCommand( station, sb.ToString( ), len > 27 ).Content );
				}
			}
			return OperateResult.CreateSuccessResult( list );
		}

		/// <summary>
		/// 创建批量读取触点的报文指令<br />
		/// Create message instructions for batch reading contacts
		/// </summary>
		/// <param name="station">设备站号</param>
		/// <param name="address">地址信息</param>
		/// <param name="values">数据值</param>
		/// <returns>包含是否成功的结果对象</returns>
		public static OperateResult<byte[]> BuildWriteCommand( byte station, string address, byte[] values )
		{
			// 参数检查
			if (address == null) return new OperateResult<byte[]>( StringResources.Language.PanasonicAddressParameterCannotBeNull );

			// 解析地址
			OperateResult<string, int> analysis = AnalysisAddress( address );
			if (!analysis.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( analysis );

			// 确保偶数长度
			values = SoftBasic.ArrayExpandToLengthEven( values );
			short length = (short)(values.Length / 2);

			StringBuilder sb = new StringBuilder( "#" );
			if (analysis.Content1 == "X" || analysis.Content1 == "Y" || analysis.Content1 == "R" || analysis.Content1 == "L")
			{
				sb.Append( "WCC" );
				sb.Append( analysis.Content1 );

				int wordStart = analysis.Content2 / 16;
				int wordFinish = (analysis.Content2 + values.Length * 8 - 1) / 16;
				sb.Append( wordStart. ToString( "D4" ) );
				sb.Append( wordFinish.ToString( "D4" ) );
			}
			else if (analysis.Content1 == "D" || analysis.Content1 == "LD" || analysis.Content1 == "F")
			{
				sb.Append( "WD" );
				sb.Append( analysis.Content1.Substring( 0, 1 ) );
				sb.Append( analysis.Content2.ToString( "D5" ) );
				sb.Append( (analysis.Content2 + length - 1).ToString( "D5" ) );
			}
			else if (analysis.Content1 == "IX" || analysis.Content1 == "IY" || analysis.Content1 == "ID")
			{
				sb.Append( "WD" );
				sb.Append( analysis.Content1 );
				sb.Append( analysis.Content2.ToString( "D9" ) );
				sb.Append( (analysis.Content2 + length - 1).ToString( "D9" ) );
			}
			else if (analysis.Content1 == "C" || analysis.Content1 == "T")
			{
				sb.Append( "WS" );
				sb.Append( analysis.Content2.ToString( "D4" ) );
				sb.Append( (analysis.Content2 + length - 1).ToString( "D4" ) );
			}
			else if (analysis.Content1 == "K" || analysis.Content1 == "S")
			{
				// 经过值
				sb.Append( "W" );
				sb.Append( analysis.Content1 );
				sb.Append( analysis.Content2.ToString( "D4" ) );
				sb.Append( (analysis.Content2 + length - 1).ToString( "D4" ) );
			}

			sb.Append( SoftBasic.ByteToHexString( values ) );

			return PackPanasonicCommand( station, sb.ToString( ), sb.Length > 112 );
		}

		/// <summary>
		/// 检查从PLC反馈的数据，并返回正确的数据内容<br />
		/// Check the data feedback from the PLC and return the correct data content
		/// </summary>
		/// <param name="response">反馈信号</param>
		/// <returns>是否成功的结果信息</returns>
		public static OperateResult<byte[]> ExtraActualData( byte[] response )
		{
			if (response.Length < 9) return new OperateResult<byte[]>( StringResources.Language.PanasonicReceiveLengthMustLargerThan9 );

			if (response[3] == '$')
			{
				byte[] data = new byte[response.Length - 9];
				if (data.Length > 0)
				{
					Array.Copy( response, 6, data, 0, data.Length );
					data = SoftBasic.HexStringToBytes( Encoding.ASCII.GetString( data ) );
				}
				return OperateResult.CreateSuccessResult( data );
			}
			else if (response[3] == '!')
			{
				int err = int.Parse( Encoding.ASCII.GetString( response, 4, 2 ) );
				return new OperateResult<byte[]>( err, GetErrorDescription( err ) );
			}
			else
			{
				return new OperateResult<byte[]>( StringResources.Language.UnknownError + " Source Data: " + HslCommunication.BasicFramework.SoftBasic.GetAsciiStringRender( response ) );
			}
		}

		/// <summary>
		/// 检查从PLC反馈的数据，并返回正确的数据内容<br />
		/// Check the data feedback from the PLC and return the correct data content
		/// </summary>
		/// <param name="response">反馈信号</param>
		/// <returns>是否成功的结果信息</returns>
		public static OperateResult<bool[]> ExtraActualBool( byte[] response )
		{
			if (response.Length < 9) return new OperateResult<bool[]>( StringResources.Language.PanasonicReceiveLengthMustLargerThan9 + " Source: " + SoftBasic.GetAsciiStringRender( response ) );

			if (response[3] == '$')
			{
				byte[] value = response.SelectMiddle( 6, response.Length - 9 );
				return OperateResult.CreateSuccessResult( value.Select( m => m == 0x31 ).ToArray( ) );
			}
			else if (response[3] == '!')
			{
				int err = int.Parse( Encoding.ASCII.GetString( response, 4, 2 ) );
				return new OperateResult<bool[]>( err, GetErrorDescription( err ) );
			}
			else
			{
				return new OperateResult<bool[]>( StringResources.Language.UnknownError + " Source: " + SoftBasic.GetAsciiStringRender( response ) );
			}
		}

		/// <summary>
		/// 根据错误码获取到错误描述文本<br />
		/// Get the error description text according to the error code
		/// </summary>
		/// <param name="err">错误代码</param>
		/// <returns>字符信息</returns>
		public static string GetErrorDescription( int err )
		{
			switch (err)
			{
				case 20: return StringResources.Language.PanasonicMewStatus20;
				case 21: return StringResources.Language.PanasonicMewStatus21;
				case 22: return StringResources.Language.PanasonicMewStatus22;
				case 23: return StringResources.Language.PanasonicMewStatus23;
				case 24: return StringResources.Language.PanasonicMewStatus24;
				case 25: return StringResources.Language.PanasonicMewStatus25;
				case 26: return StringResources.Language.PanasonicMewStatus26;
				case 27: return StringResources.Language.PanasonicMewStatus27;
				case 28: return StringResources.Language.PanasonicMewStatus28;
				case 29: return StringResources.Language.PanasonicMewStatus29;
				case 30: return StringResources.Language.PanasonicMewStatus30;
				case 40: return StringResources.Language.PanasonicMewStatus40;
				case 41: return StringResources.Language.PanasonicMewStatus41;
				case 42: return StringResources.Language.PanasonicMewStatus42;
				case 43: return StringResources.Language.PanasonicMewStatus43;
				case 50: return StringResources.Language.PanasonicMewStatus50;
				case 51: return StringResources.Language.PanasonicMewStatus51;
				case 52: return StringResources.Language.PanasonicMewStatus52;
				case 53: return StringResources.Language.PanasonicMewStatus53;
				case 60: return StringResources.Language.PanasonicMewStatus60;
				case 61: return StringResources.Language.PanasonicMewStatus61;
				case 62: return StringResources.Language.PanasonicMewStatus62;
				case 63: return StringResources.Language.PanasonicMewStatus63;
				case 64: return StringResources.Language.PanasonicMewStatus64;
				case 65: return StringResources.Language.PanasonicMewStatus65;
				case 66: return StringResources.Language.PanasonicMewStatus66;
				case 67: return StringResources.Language.PanasonicMewStatus67;
				case 68: return StringResources.Language.PanasonicMewStatus68;
				case 71: return StringResources.Language.PanasonicMewStatus71;
				case 78: return StringResources.Language.PanasonicMewStatus78;
				case 80: return StringResources.Language.PanasonicMewStatus80;
				case 81: return StringResources.Language.PanasonicMewStatus81;
				case 90: return StringResources.Language.PanasonicMewStatus90;
				case 92: return StringResources.Language.PanasonicMewStatus92;
				default: return StringResources.Language.UnknownError;
			}
		}

		/// <summary>
		/// 根据MC的错误码去查找对象描述信息
		/// </summary>
		/// <param name="code">错误码</param>
		/// <returns>描述信息</returns>
		public static string GetMcErrorDescription( int code )
		{
			switch (code)
			{
				case 0x4031: return StringResources.Language.PanasonicMc4031;
				case 0xC051: return StringResources.Language.PanasonicMcC051;
				case 0xC056: return StringResources.Language.PanasonicMcC056;
				case 0xC059: return StringResources.Language.PanasonicMcC059;
				case 0xC05B: return StringResources.Language.PanasonicMcC05B;
				case 0xC05C: return StringResources.Language.PanasonicMcC05C;
				case 0xC05F: return StringResources.Language.PanasonicMcC05F;
				case 0xC060: return StringResources.Language.PanasonicMcC060;
				case 0xC061: return StringResources.Language.PanasonicMcC061;
				default: return StringResources.Language.MelsecPleaseReferToManualDocument;
			}
		}

		#endregion
	}
}
