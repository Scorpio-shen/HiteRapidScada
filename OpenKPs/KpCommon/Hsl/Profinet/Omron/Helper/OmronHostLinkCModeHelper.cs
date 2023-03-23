using HslCommunication.BasicFramework;
using HslCommunication.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
#if !NET20 && !NET35
using System.Threading.Tasks;
#endif

namespace HslCommunication.Profinet.Omron.Helper
{
	/// <summary>
	/// 欧姆龙的OmronHostLinkCMode的辅助类方法
	/// </summary>
	public class OmronHostLinkCModeHelper
	{
		/// <inheritdoc cref="OmronFinsNet.Read(string, ushort)"/>
		/// <remarks>
		/// 地址里可以额外指定单元号信息，例如 s=2;D100
		/// </remarks>
		public static OperateResult<byte[]> Read( IReadWriteDevice omron, byte unitNumber, string address, ushort length )
		{
			byte station = (byte)HslHelper.ExtractParameter( ref address, "s", unitNumber );

			// 解析地址
			var command = BuildReadCommand( address, length, false );
			if (!command.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( command );

			List<byte> array = new List<byte>( );
			for (int i = 0; i < command.Content.Count; i++)
			{
				// 核心交互
				OperateResult<byte[]> read = omron.ReadFromCoreServer( PackCommand( command.Content[i], station ) );
				if (!read.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( read );

				// 数据有效性分析
				OperateResult<byte[]> valid = ResponseValidAnalysis( read.Content, true );
				if (!valid.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( valid );

				// 读取到了正确的数据
				array.AddRange( valid.Content );
			}

			return OperateResult.CreateSuccessResult( array.ToArray( ) );
		}

#if !NET20 && !NET35
		/// <inheritdoc cref="Read(IReadWriteDevice, byte, string, ushort)"/>
		public static async Task<OperateResult<byte[]>> ReadAsync( IReadWriteDevice omron, byte unitNumber, string address, ushort length )
		{
			byte station = (byte)HslHelper.ExtractParameter( ref address, "s", unitNumber );

			// 解析地址
			var command = BuildReadCommand( address, length, false );
			if (!command.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( command );

			List<byte> array = new List<byte>( );
			for (int i = 0; i < command.Content.Count; i++)
			{
				// 核心交互
				OperateResult<byte[]> read = await omron.ReadFromCoreServerAsync( PackCommand( command.Content[i], station ) );
				if (!read.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( read );

				// 数据有效性分析
				OperateResult<byte[]> valid = ResponseValidAnalysis( read.Content, true );
				if (!valid.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( valid );

				// 读取到了正确的数据
				array.AddRange( valid.Content );
			}

			return OperateResult.CreateSuccessResult( array.ToArray( ) );
		}
#endif

		/// <inheritdoc cref="OmronFinsNet.Write(string, byte[])"/>
		/// <remarks>
		/// 地址里可以额外指定单元号信息，例如 s=2;D100
		/// </remarks>
		public static OperateResult Write( IReadWriteDevice omron, byte unitNumber, string address, byte[] value )
		{
			byte station = (byte)HslHelper.ExtractParameter( ref address, "s", unitNumber );

			// 获取指令
			var command = BuildWriteWordCommand( address, value ); ;
			if (!command.IsSuccess) return command;

			for (int i = 0; i < command.Content.Count; i++)
			{
				// 核心数据交互
				OperateResult<byte[]> read = omron.ReadFromCoreServer( PackCommand( command.Content[i], station ) );
				if (!read.IsSuccess) return read;

				// 数据有效性分析
				OperateResult<byte[]> valid = ResponseValidAnalysis( read.Content, false );
				if (!valid.IsSuccess) return valid;

			}

			// 成功
			return OperateResult.CreateSuccessResult( );
		}
#if !NET20 && !NET35
		/// <inheritdoc cref="Write(IReadWriteDevice, byte, string, byte[])"/>
		public static async Task<OperateResult> WriteAsync( IReadWriteDevice omron, byte unitNumber, string address, byte[] value )
		{
			byte station = (byte)HslHelper.ExtractParameter( ref address, "s", unitNumber );

			// 获取指令
			var command = BuildWriteWordCommand( address, value ); ;
			if (!command.IsSuccess) return command;

			for (int i = 0; i < command.Content.Count; i++)
			{
				// 核心数据交互
				OperateResult<byte[]> read = await omron.ReadFromCoreServerAsync( PackCommand( command.Content[i], station ) );
				if (!read.IsSuccess) return read;

				// 数据有效性分析
				OperateResult<byte[]> valid = ResponseValidAnalysis( read.Content, false );
				if (!valid.IsSuccess) return valid;

			}

			// 成功
			return OperateResult.CreateSuccessResult( );
		}
#endif
		/// <summary>
		/// <b>[商业授权]</b> 读取PLC的当前的型号信息<br />
		/// <b>[Authorization]</b> Read the current model information of the PLC
		/// </summary>
		/// <param name="omron">PLC连接对象</param>
		/// <param name="unitNumber">站号信息</param>
		/// <returns>型号</returns>
		public static OperateResult<string> ReadPlcType( IReadWriteDevice omron, byte unitNumber )
		{
			if (!Authorization.asdniasnfaksndiqwhawfskhfaiw( )) return new OperateResult<string>( StringResources.Language.InsufficientPrivileges );

			// 核心数据交互
			OperateResult<byte[]> read = omron.ReadFromCoreServer( PackCommand( Encoding.ASCII.GetBytes( "MM" ), unitNumber ) );
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<string>( read );

			// 数据有效性分析
			int err = Convert.ToInt32( Encoding.ASCII.GetString( read.Content, 5, 2 ), 16 );
			if (err > 0) return new OperateResult<string>( err, "Unknown Error" );

			// 成功
			string model = Encoding.ASCII.GetString( read.Content, 7, 2 );
			return GetModelText( model );
		}

		/// <summary>
		/// <b>[商业授权]</b> 读取PLC当前的操作模式，0: 编程模式  1: 运行模式  2: 监视模式<br />
		/// <b>[Authorization]</b> Reads the Operation mode of the CPU Unit. 0: PROGRAM mode  1: RUN mode  2: MONITOR mode
		/// </summary>
		/// <param name="omron">PLC连接对象</param>
		/// <param name="unitNumber">站号信息</param>
		/// <returns>0: 编程模式  1: 运行模式  2: 监视模式</returns>
		public static OperateResult<int> ReadPlcMode( IReadWriteDevice omron, byte unitNumber )
		{
			if (!Authorization.asdniasnfaksndiqwhawfskhfaiw( )) return new OperateResult<int>( StringResources.Language.InsufficientPrivileges );

			// 核心数据交互
			OperateResult<byte[]> read = omron.ReadFromCoreServer( PackCommand( Encoding.ASCII.GetBytes( "MS" ), unitNumber ) );
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<int>( read );

			// 数据有效性分析
			int err = Convert.ToInt32( Encoding.ASCII.GetString( read.Content, 5, 2 ), 16 );
			if (err > 0) return new OperateResult<int>( err, "Unknown Error" );

			// 成功
			byte[] model = Encoding.ASCII.GetString( read.Content, 7, 4 ).ToHexBytes( );
			return OperateResult.CreateSuccessResult( model[0] & 0x03 );
		}

		/// <summary>
		/// <b>[商业授权]</b> 将当前PLC的模式变更为指定的模式，0: 编程模式  1: 运行模式  2: 监视模式<br />
		/// <b>[Authorization]</b> Change the current PLC mode to the specified mode, 0: programming mode 1: running mode 2: monitoring mode
		/// </summary>
		/// <param name="omron">PLC连接对象</param>
		/// <param name="unitNumber">站号信息</param>
		/// <param name="mode">0: 编程模式  1: 运行模式  2: 监视模式</param>
		/// <returns>是否变更成功</returns>
		public static OperateResult ChangePlcMode( IReadWriteDevice omron, byte unitNumber, byte mode )
		{
			if (!Authorization.asdniasnfaksndiqwhawfskhfaiw( )) return new OperateResult<int>( StringResources.Language.InsufficientPrivileges );

			// 核心数据交互
			OperateResult<byte[]> read = omron.ReadFromCoreServer( PackCommand( 
				Encoding.ASCII.GetBytes( "SC" + mode.ToString( "X2" ) ), unitNumber ) );
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<int>( read );

			// 数据有效性分析
			int err = Convert.ToInt32( Encoding.ASCII.GetString( read.Content, 5, 2 ), 16 );
			if (err > 0) return new OperateResult<int>( err, "Unknown Error" );

			// 成功
			return OperateResult.CreateSuccessResult( );
		}

		#region Build Command

		/// <summary>
		/// 解析欧姆龙的数据地址，参考来源是Omron手册第188页，比如D100， E1.100<br />
		/// Analyze Omron's data address, the reference source is page 188 of the Omron manual, such as D100, E1.100
		/// </summary>
		/// <param name="address">数据地址</param>
		/// <param name="isBit">是否是位地址</param>
		/// <param name="isRead">是否读取</param>
		/// <returns>解析后的结果地址对象</returns>
		public static OperateResult<string, int> AnalysisAddress( string address, bool isBit, bool isRead )
		{
			var result = new OperateResult<string, int>( );
			try
			{
				switch (address[0])
				{
					case 'D':
					case 'd':
						{
							// DM区数据
							result.Content1 = isRead ? "RD" : "WD";
							break;
						}
					case 'C':
					case 'c':
						{
							// CIO区数据
							result.Content1 = isRead ? "RR" : "WR";
							break;
						}
					case 'H':
					case 'h':
						{
							// HR区
							result.Content1 = isRead ? "RH" : "WH";
							break;
						}
					case 'A':
					case 'a':
						{
							// AR区
							result.Content1 = isRead ? "RJ" : "WJ";
							break;
						}
					case 'E':
					case 'e':
						{
							// E区，比较复杂，需要专门的计算
							string[] splits = address.Split( new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries );
							int block = Convert.ToInt32( splits[0].Substring( 1 ), 16 );
							result.Content1 = (isRead ? "RE" : "WE") + Encoding.ASCII.GetString( SoftBasic.BuildAsciiBytesFrom( (byte)block ) );
							break;
						}
					default: throw new Exception( StringResources.Language.NotSupportedDataType );
				}

				if (address[0] == 'E' || address[0] == 'e')
				{
					string[] splits = address.Split( new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries );
					if (isBit)
					{
						// 位操作
						//ushort addr = ushort.Parse( splits[1] );
						//result.Content2 = new byte[3];
						//result.Content2[0] = BitConverter.GetBytes( addr )[1];
						//result.Content2[1] = BitConverter.GetBytes( addr )[0];

						//if (splits.Length > 2)
						//{
						//	result.Content2[2] = byte.Parse( splits[2] );
						//	if (result.Content2[2] > 15)
						//	{
						//		throw new Exception( StringResources.Language.OmronAddressMustBeZeroToFiveteen );
						//	}
						//}
					}
					else
					{
						// 字操作
						ushort addr = ushort.Parse( splits[1] );
						result.Content2 = addr;
					}
				}
				else
				{
					if (isBit)
					{
						// 位操作
						//string[] splits = address.Substring( 1 ).Split( new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries );
						//ushort addr = ushort.Parse( splits[0] );
						//result.Content2 = new byte[3];
						//result.Content2[0] = BitConverter.GetBytes( addr )[1];
						//result.Content2[1] = BitConverter.GetBytes( addr )[0];

						//if (splits.Length > 1)
						//{
						//	result.Content2[2] = byte.Parse( splits[1] );
						//	if (result.Content2[2] > 15)
						//	{
						//		throw new Exception( StringResources.Language.OmronAddressMustBeZeroToFiveteen );
						//	}
						//}
					}
					else
					{
						// 字操作
						ushort addr = ushort.Parse( address.Substring( 1 ) );
						result.Content2 = addr;
					}
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
		/// 根据读取的地址，长度，是否位读取创建Fins协议的核心报文<br />
		/// According to the read address, length, whether to read the core message that creates the Fins protocol
		/// </summary>
		/// <param name="address">地址，具体格式请参照示例说明</param>
		/// <param name="length">读取的数据长度</param>
		/// <param name="isBit">是否使用位读取</param>
		/// <returns>带有成功标识的Fins核心报文</returns>
		public static OperateResult<List<byte[]>> BuildReadCommand( string address, ushort length, bool isBit )
		{
			var analysis = AnalysisAddress( address, isBit, true );
			if (!analysis.IsSuccess) return OperateResult.CreateFailedResult<List<byte[]>>( analysis );

			int[] lens = SoftBasic.SplitIntegerToArray( length, 30 );
			List<byte[]> array = new List<byte[]>( );
			for (int i = 0; i < lens.Length; i++)
			{
				StringBuilder sb = new StringBuilder( );
				sb.Append( analysis.Content1 );
				sb.Append( analysis.Content2.ToString( "D4" ) );
				sb.Append( lens[i].ToString( "D4" ) );
				array.Add( Encoding.ASCII.GetBytes( sb.ToString( ) ) );

				analysis.Content2 += lens[i];
			}
			return OperateResult.CreateSuccessResult( array );
		}

		/// <summary>
		/// 根据读取的地址，长度，是否位读取创建Fins协议的核心报文<br />
		/// According to the read address, length, whether to read the core message that creates the Fins protocol
		/// </summary>
		/// <param name="address">地址，具体格式请参照示例说明</param>
		/// <param name="value">等待写入的数据</param>
		/// <returns>带有成功标识的Fins核心报文</returns>
		public static OperateResult<List<byte[]>> BuildWriteWordCommand( string address, byte[] value )
		{
			var analysis = AnalysisAddress( address, false, false );
			if (!analysis.IsSuccess) return OperateResult.CreateFailedResult<List<byte[]>>( analysis );

			List<byte[]> array = SoftBasic.ArraySplitByLength( value, 60 );
			List<byte[]> result = new List<byte[]>( );
			for (int i = 0; i < array.Count; i++)
			{
				StringBuilder sb = new StringBuilder( );
				sb.Append( analysis.Content1 );
				sb.Append( analysis.Content2.ToString( "D4" ) );
				if (array[i].Length > 0) sb.Append( array[i].ToHexString( ) );
				result.Add( Encoding.ASCII.GetBytes( sb.ToString( ) ) );

				analysis.Content2 += array[i].Length / 2;
			}

			return OperateResult.CreateSuccessResult( result );
		}


		/// <summary>
		/// 验证欧姆龙的Fins-TCP返回的数据是否正确的数据，如果正确的话，并返回所有的数据内容
		/// </summary>
		/// <param name="response">来自欧姆龙返回的数据内容</param>
		/// <param name="isRead">是否读取</param>
		/// <returns>带有是否成功的结果对象</returns>
		public static OperateResult<byte[]> ResponseValidAnalysis( byte[] response, bool isRead )
		{
			// 数据有效性分析
			if (response.Length >= 11)
			{
				// 提取错误码
				int err = Convert.ToInt32( Encoding.ASCII.GetString( response, 5, 2 ), 16 );
				byte[] Content = null;

				if (response.Length > 11) Content = Encoding.ASCII.GetString( response, 7, response.Length - 11 ).ToHexBytes( );

				if (err > 0)
				{
					return new OperateResult<byte[]>( )
					{
						ErrorCode = err,
						Message = GetErrorMessage( err ),
						Content = Content
					};
				}
				else
				{
					return OperateResult.CreateSuccessResult( Content );
				}
			}

			return new OperateResult<byte[]>( StringResources.Language.OmronReceiveDataError );
		}

		/// <summary>
		/// 将普通的指令打包成完整的指令
		/// </summary>
		/// <param name="cmd">fins指令</param>
		/// <param name="unitNumber">站号信息</param>
		/// <returns>完整的质量</returns>
		public static byte[] PackCommand( byte[] cmd, byte unitNumber )
		{
			byte[] buffer = new byte[7 + cmd.Length];

			buffer[0] = (byte)'@';
			buffer[1] = SoftBasic.BuildAsciiBytesFrom( unitNumber )[0];
			buffer[2] = SoftBasic.BuildAsciiBytesFrom( unitNumber )[1];
			buffer[buffer.Length - 2] = (byte)'*';
			buffer[buffer.Length - 1] = 0x0D;
			cmd.CopyTo( buffer, 3 );
			// 计算FCS
			int tmp = buffer[0];
			for (int i = 1; i < buffer.Length - 4; i++)
			{
				tmp = (tmp ^ buffer[i]);
			}
			buffer[buffer.Length - 4] = SoftBasic.BuildAsciiBytesFrom( (byte)tmp )[0];
			buffer[buffer.Length - 3] = SoftBasic.BuildAsciiBytesFrom( (byte)tmp )[1];
			return buffer;
		}

		/// <summary>
		/// 获取model的字符串描述信息
		/// </summary>
		/// <param name="model">型号代码</param>
		/// <returns>是否解析成功</returns>
		public static OperateResult<string> GetModelText( string model )
		{
			switch (model)
			{
				case "30": return OperateResult.CreateSuccessResult( "CS/CJ" );
				case "01": return OperateResult.CreateSuccessResult( "C250" );
				case "02": return OperateResult.CreateSuccessResult( "C500" );
				case "03": return OperateResult.CreateSuccessResult( "C120/C50" );
				case "09": return OperateResult.CreateSuccessResult( "C250F" );
				case "0A": return OperateResult.CreateSuccessResult( "C500F" );
				case "0B": return OperateResult.CreateSuccessResult( "C120F" );
				case "0E": return OperateResult.CreateSuccessResult( "C2000" );
				case "10": return OperateResult.CreateSuccessResult( "C1000H" );
				case "11": return OperateResult.CreateSuccessResult( "C2000H/CQM1/CPM1" );
				case "12": return OperateResult.CreateSuccessResult( "C20H/C28H/C40H, C200H, C200HS, C200HX/HG/HE (-ZE)" );
				case "20": return OperateResult.CreateSuccessResult( "CV500" );
				case "21": return OperateResult.CreateSuccessResult( "CV1000" );
				case "22": return OperateResult.CreateSuccessResult( "CV2000" );
				case "40": return OperateResult.CreateSuccessResult( "CVM1-CPU01-E" );
				case "41": return OperateResult.CreateSuccessResult( "CVM1-CPU11-E" );
				case "42": return OperateResult.CreateSuccessResult( "CVM1-CPU21-E" );
				default: return new OperateResult<string>( "Unknown model, model code:" + model );
			}
		}

		/// <summary>
		/// 根据错误码的信息，返回错误的具体描述的文本<br />
		/// According to the information of the error code, return the text of the specific description of the error
		/// </summary>
		/// <param name="err">错误码</param>
		/// <returns>错误的描述文本</returns>
		public static string GetErrorMessage( int err )
		{
			switch (err)
			{
				case 0x01: return "Not executable in RUN mode";
				case 0x02: return "Not executable in MONITOR mode";
				case 0x03: return "UM write-protected";
				case 0x04: return "Address over: The program address setting in an read or write command is above the highest program address.";
				case 0x0B: return "Not executable in PROGRAM mode";
				case 0x13: return "The FCS is wrong.";
				case 0x14: return "The command format is wrong, or a command that cannot be divided has been divided, or the frame length is smaller than the minimum length for the applicable command.";
				case 0x15: return "1. The data is outside of the specified range or too long. 2.Hexadecimal data has not been specified.";
				case 0x16: return "Command not supported: The operand specified in an SV Read or SV Change command does not exist in the program.";
				case 0x18: return "Frame length error: The maximum frame length of 131 bytes was exceeded.";
				case 0x19: return "Not executable: The read SV exceeded 9,999, or an I/O memory batch read was executed when items to read were not registered for composite command, or access right was not obtained.";
				case 0x20: return "Could not create I/O table";
				case 0x21: return "Not executable due to CPU Unit CPU error( See note.)";
				case 0x23: return "User memory protected, The UM is read-protected or writeprotected.";
				case 0xA3: return "Aborted due to FCS error in transmission data";
				case 0xA4: return "Aborted due to format error in transmission data";
				case 0xA5: return "Aborted due to entry number data error in transmission data";
				case 0xA8: return "Aborted due to frame length error in transmission data";
				default: return StringResources.Language.UnknownError;
			}
		}

		#endregion
	}
}
