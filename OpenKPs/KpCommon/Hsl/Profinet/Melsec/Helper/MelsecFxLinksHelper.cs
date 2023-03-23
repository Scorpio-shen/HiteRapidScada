using HslCommunication.BasicFramework;
using HslCommunication.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HslCommunication.Core.Address;
using HslCommunication.Serial;
#if !NET20 && !NET35
using System.Threading.Tasks;
#endif

namespace HslCommunication.Profinet.Melsec.Helper
{
	/// <summary>
	/// 三菱的FxLinks的辅助方法信息
	/// </summary>
	public class MelsecFxLinksHelper
	{
		#region Static Helper

		/// <summary>
		/// 将当前的报文进行打包，根据和校验的方式以及格式信息来实现打包操作
		/// </summary>
		/// <param name="plc">PLC通信对象</param>
		/// <param name="command">原始的命令数据</param>
		/// <returns>打包后的命令</returns>
		public  static byte[] PackCommandWithHeader( IReadWriteFxLinks plc, byte[] command )
		{
			byte[] core = command;
			if (plc.SumCheck)
			{
				core = new byte[command.Length + 2];
				command.CopyTo( core, 0 );
				SoftLRC.CalculateAccAndFill( core, 0, 2 );
			}

			if (plc.Format == 1)
				return SoftBasic.SpliceArray( new byte[] { AsciiControl.ENQ }, core );
			else if (plc.Format == 4)
				return SoftBasic.SpliceArray( new byte[] { AsciiControl.ENQ }, core, new byte[] { AsciiControl.CR, AsciiControl.LF } );
			return SoftBasic.SpliceArray( new byte[] { AsciiControl.ENQ }, core );
		}

		/// <summary>
		/// 创建一条读取的指令信息，需要指定一些参数
		/// </summary>
		/// <param name="station">PLC的站号</param>
		/// <param name="address">地址信息</param>
		/// <param name="length">数据长度</param>
		/// <param name="isBool">是否位读取</param>
		/// <param name="waitTime">等待时间</param>
		/// <returns>是否成功的结果对象</returns>
		public static OperateResult<List<byte[]>> BuildReadCommand( byte station, string address, ushort length, bool isBool, byte waitTime = 0x00 )
		{
			OperateResult<MelsecFxLinksAddress> addressAnalysis = MelsecFxLinksAddress.ParseFrom( address );
			if (!addressAnalysis.IsSuccess) return OperateResult.CreateFailedResult<List<byte[]>>( addressAnalysis );

			int[] lens = SoftBasic.SplitIntegerToArray( length, isBool ? 256 : 64 );
			List<byte[]> list = new List<byte[]>( );
			for (int i = 0; i < lens.Length; i++)
			{
				StringBuilder stringBuilder = new StringBuilder( );
				stringBuilder.Append( station.ToString( "X2" ) );
				stringBuilder.Append( "FF" );

				if (isBool)
					stringBuilder.Append( "BR" );
				else
				{
					if( addressAnalysis.Content.AddressStart >= 10000)
						stringBuilder.Append( "QR" );
					else
						stringBuilder.Append( "WR" );
				}

				stringBuilder.Append( waitTime.ToString( "X" ) );
				stringBuilder.Append( addressAnalysis.Content.ToString( ) );
				// 如果长度为256则使用00来表示
				if (lens[i] == 256)
					stringBuilder.Append( "00" );
				else
					stringBuilder.Append( lens[i].ToString( "X2" ) );

				list.Add( Encoding.ASCII.GetBytes( stringBuilder.ToString( ) ) );
				addressAnalysis.Content.AddressStart += lens[i];
			}

			return OperateResult.CreateSuccessResult( list );
		}

		/// <summary>
		/// 创建一条别入bool数据的指令信息，需要指定一些参数
		/// </summary>
		/// <param name="station">站号</param>
		/// <param name="address">地址</param>
		/// <param name="value">数组值</param>
		/// <param name="waitTime">等待时间</param>
		/// <returns>是否创建成功</returns>
		public static OperateResult<byte[]> BuildWriteBoolCommand( byte station, string address, bool[] value, byte waitTime = 0x00 )
		{
			OperateResult<MelsecFxLinksAddress> addressAnalysis = MelsecFxLinksAddress.ParseFrom( address );
			if (!addressAnalysis.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( addressAnalysis );

			StringBuilder stringBuilder = new StringBuilder( );
			stringBuilder.Append( station.ToString( "X2" ) );
			stringBuilder.Append( "FF" );
			stringBuilder.Append( "BW" );
			stringBuilder.Append( waitTime.ToString( "X" ) );
			stringBuilder.Append( addressAnalysis.Content.ToString( ) );
			stringBuilder.Append( value.Length.ToString( "X2" ) );
			for (int i = 0; i < value.Length; i++)
			{
				stringBuilder.Append( value[i] ? "1" : "0" );
			}

			return OperateResult.CreateSuccessResult( Encoding.ASCII.GetBytes( stringBuilder.ToString( ) ) );
		}

		/// <summary>
		/// 创建一条别入byte数据的指令信息，需要指定一些参数，按照字单位
		/// </summary>
		/// <param name="station">站号</param>
		/// <param name="address">地址</param>
		/// <param name="value">数组值</param>
		/// <param name="waitTime">等待时间</param>
		/// <returns>命令报文的结果内容对象</returns>
		public static OperateResult<byte[]> BuildWriteByteCommand( byte station, string address, byte[] value, byte waitTime = 0x00 )
		{
			OperateResult<MelsecFxLinksAddress> addressAnalysis = MelsecFxLinksAddress.ParseFrom( address );
			if (!addressAnalysis.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( addressAnalysis );

			StringBuilder stringBuilder = new StringBuilder( );
			stringBuilder.Append( station.ToString( "X2" ) );
			stringBuilder.Append( "FF" );
			if (addressAnalysis.Content.AddressStart >= 10000)
				stringBuilder.Append( "QW" );
			else
				stringBuilder.Append( "WW" );
			stringBuilder.Append( waitTime.ToString( "X" ) );
			stringBuilder.Append( addressAnalysis.Content.ToString( ) );
			stringBuilder.Append( (value.Length / 2).ToString( "X2" ) );

			// 字写入
			byte[] buffer = new byte[value.Length * 2];
			for (int i = 0; i < value.Length / 2; i++)
			{
				SoftBasic.BuildAsciiBytesFrom( BitConverter.ToUInt16( value, i * 2 ) ).CopyTo( buffer, 4 * i );
			}
			stringBuilder.Append( Encoding.ASCII.GetString( buffer ) );
			return OperateResult.CreateSuccessResult( Encoding.ASCII.GetBytes( stringBuilder.ToString( ) ) );
		}

		/// <summary>
		/// 创建启动PLC的报文信息
		/// </summary>
		/// <param name="station">站号信息</param>
		/// <param name="waitTime">等待时间</param>
		/// <returns>命令报文的结果内容对象</returns>
		public static OperateResult<byte[]> BuildStart( byte station, byte waitTime = 0x00 )
		{
			if (!Authorization.asdniasnfaksndiqwhawfskhfaiw( )) return new OperateResult<byte[]>( StringResources.Language.InsufficientPrivileges );

			StringBuilder stringBuilder = new StringBuilder( );
			stringBuilder.Append( station.ToString( "X2" ) );
			stringBuilder.Append( "FF" );
			stringBuilder.Append( "RR" );
			stringBuilder.Append( waitTime.ToString( "X" ) );
			return OperateResult.CreateSuccessResult( Encoding.ASCII.GetBytes( stringBuilder.ToString( ) ) );
		}

		/// <summary>
		/// 创建启动PLC的报文信息
		/// </summary>
		/// <param name="station">站号信息</param>
		/// <param name="waitTime">等待时间</param>
		/// <returns>命令报文的结果内容对象</returns>
		public static OperateResult<byte[]> BuildStop( byte station, byte waitTime = 0x00 )
		{
			if (!Authorization.asdniasnfaksndiqwhawfskhfaiw( )) return new OperateResult<byte[]>( StringResources.Language.InsufficientPrivileges );

			StringBuilder stringBuilder = new StringBuilder( );
			stringBuilder.Append( station.ToString( "X2" ) );
			stringBuilder.Append( "FF" );
			stringBuilder.Append( "RS" );
			stringBuilder.Append( waitTime.ToString( "X" ) );

			return OperateResult.CreateSuccessResult( Encoding.ASCII.GetBytes( stringBuilder.ToString( ) ) );
		}

		/// <summary>
		/// 创建读取PLC类型的命令报文
		/// </summary>
		/// <param name="station">站号信息</param>
		/// <param name="waitTime">等待实际</param>
		/// <returns>命令报文的结果内容对象</returns>
		public static OperateResult<byte[]> BuildReadPlcType( byte station, byte waitTime = 0x00 )
		{
			if (!Authorization.asdniasnfaksndiqwhawfskhfaiw( )) return new OperateResult<byte[]>( StringResources.Language.InsufficientPrivileges );

			StringBuilder stringBuilder = new StringBuilder( );
			stringBuilder.Append( station.ToString( "X2" ) );
			stringBuilder.Append( "FF" );
			stringBuilder.Append( "PC" );
			stringBuilder.Append( waitTime.ToString( "X" ) );

			return OperateResult.CreateSuccessResult( Encoding.ASCII.GetBytes( stringBuilder.ToString( ) ) );
		}

		/// <summary>
		/// 从编码中提取PLC的型号信息
		/// </summary>
		/// <param name="code">编码</param>
		/// <returns>PLC的型号信息</returns>
		public static OperateResult<string> GetPlcTypeFromCode( string code )
		{
			switch (code)
			{
				case "F2": return OperateResult.CreateSuccessResult( "FX1S" );
				case "8E": return OperateResult.CreateSuccessResult( "FX0N" );
				case "8D": return OperateResult.CreateSuccessResult( "FX2/FX2C" );
				case "9E": return OperateResult.CreateSuccessResult( "FX1N/FX1NC" );
				case "9D": return OperateResult.CreateSuccessResult( "FX2N/FX2NC" );
				case "F4": return OperateResult.CreateSuccessResult( "FX3G" );
				case "F3": return OperateResult.CreateSuccessResult( "FX3U/FX3UC" );
				case "98": return OperateResult.CreateSuccessResult( "A0J2HCPU" );
				case "A1": return OperateResult.CreateSuccessResult( "A1CPU /A1NCPU" );
				// case "98": return OperateResult.CreateSuccessResult( "A1SCPU/A1SJCPU" );
				case "A2": return OperateResult.CreateSuccessResult( "A2CPU/A2NCPU/A2SCPU" );
				case "92": return OperateResult.CreateSuccessResult( "A2ACPU" );
				case "93": return OperateResult.CreateSuccessResult( "A2ACPU-S1" );
				case "9A": return OperateResult.CreateSuccessResult( "A2CCPU" );
				case "82": return OperateResult.CreateSuccessResult( "A2USCPU" );
				case "83": return OperateResult.CreateSuccessResult( "A2CPU-S1/A2USCPU-S1" );
				case "A3": return OperateResult.CreateSuccessResult( "A3CPU/A3NCPU" );
				case "94": return OperateResult.CreateSuccessResult( "A3ACPU" );
				case "A4": return OperateResult.CreateSuccessResult( "A3HCPU/A3MCPU" );
				case "84": return OperateResult.CreateSuccessResult( "A3UCPU" );
				case "85": return OperateResult.CreateSuccessResult( "A4UCPU" );
				// case "9A": return OperateResult.CreateSuccessResult( "A52GCPU" );
				// case "A3": return OperateResult.CreateSuccessResult( "A73CPU" );
				// case "A3": return OperateResult.CreateSuccessResult( "A7LMS-F" );
				case "AB": return OperateResult.CreateSuccessResult( "AJ72P25/R25" );
				case "8B": return OperateResult.CreateSuccessResult( "AJ72LP25/BR15" );
				default: return new OperateResult<string>( StringResources.Language.NotSupportedDataType + " Code:" + code );
			}
		}

		private static string GetErrorText( int error )
		{
			switch (error)
			{
				case 0x02: return StringResources.Language.MelsecFxLinksError02;
				case 0x03: return StringResources.Language.MelsecFxLinksError03;
				case 0x06: return StringResources.Language.MelsecFxLinksError06;
				case 0x07: return StringResources.Language.MelsecFxLinksError07;
				case 0x0A: return StringResources.Language.MelsecFxLinksError0A;
				case 0x10: return StringResources.Language.MelsecFxLinksError10;
				case 0x18: return StringResources.Language.MelsecFxLinksError18;
				default: return StringResources.Language.UnknownError;
			}
		}

		/// <summary>
		/// 检查PLC的消息反馈是否合法，合法则提取当前的数据信息，当时写入的命令消息时，无任何的数据返回<br />
		/// Check whether the PLC's message feedback is legal. If it is legal, extract the current data information. When the command message is written at that time, no data is returned.
		/// </summary>
		/// <param name="response">从PLC反馈的数据消息</param>
		/// <returns>检查的结果消息</returns>
		public static OperateResult<byte[]> CheckPlcResponse( byte[] response )
		{
			try
			{
				if (response[0] == AsciiControl.NAK)
				{
					int err = Convert.ToInt32( Encoding.ASCII.GetString( response, 5, 2 ), 16 );
					return new OperateResult<byte[]>( err, GetErrorText( err ) );
				}
				if ((response[0] != AsciiControl.STX) && (response[0] != AsciiControl.ACK)) 
					return new OperateResult<byte[]>( response[0], "Check command failed: " + SoftBasic.GetAsciiStringRender( response ) );

				if (response[0] == AsciiControl.ACK) return OperateResult.CreateSuccessResult( new byte[0] );
				int etxIndex = -1;
				for (int i = 5; i < response.Length; i++)
				{
					if(response[i] == AsciiControl.ETX)
					{
						etxIndex = i;
						break;
					}
				}
				if (etxIndex == -1) etxIndex = response.Length;
				return OperateResult.CreateSuccessResult( response.SelectMiddle( 5, etxIndex - 5 ) );
			}
			catch ( Exception ex)
			{
				return new OperateResult<byte[]>( "Error: " + ex.Message + " Source: " + SoftBasic.GetAsciiStringRender( response ) );
			}
		}

		#endregion

		/// <summary>
		/// 批量读取PLC的数据，以字为单位，支持读取X,Y,M,S,D,T,C，具体的地址范围需要根据PLC型号来确认，地址支持动态指定站号，例如：s=2;D100<br />
		/// Read PLC data in batches, in units of words, supports reading X, Y, M, S, D, T, C. 
		/// The specific address range needs to be confirmed according to the PLC model, 
		/// The address supports dynamically specifying the station number, for example: s=2;D100
		/// </summary>
		/// <param name="plc">PLC通信对象</param>
		/// <param name="address">地址信息</param>
		/// <param name="length">数据长度</param>
		/// <returns>读取结果信息</returns>
		public static OperateResult<byte[]> Read( IReadWriteFxLinks plc, string address, ushort length )
		{
			byte stat = (byte)HslHelper.ExtractParameter( ref address, "s", plc.Station );

			// 解析指令
			OperateResult<List<byte[]>> command = MelsecFxLinksHelper.BuildReadCommand( stat, address, length, false, plc.WaittingTime );
			if (!command.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( command );

			List<byte> result = new List<byte>( );
			for (int j = 0; j < command.Content.Count; j++)
			{
				// 核心交互
				OperateResult<byte[]> read = plc.ReadFromCoreServer( command.Content[j] );
				if (!read.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( read );

				// 结果验证
				OperateResult<byte[]> extra = CheckPlcResponse( read.Content );
				if (!extra.IsSuccess) return extra;

				// 提取结果
				byte[] Content = new byte[extra.Content.Length / 2];
				for (int i = 0; i < Content.Length / 2; i++)
				{
					ushort tmp = Convert.ToUInt16( Encoding.ASCII.GetString( extra.Content, i * 4, 4 ), 16 );
					BitConverter.GetBytes( tmp ).CopyTo( Content, i * 2 );
				}
				result.AddRange( Content );
			}
			return OperateResult.CreateSuccessResult( result.ToArray( ) );
		}

		/// <summary>
		/// 批量写入PLC的数据，以字为单位，也就是说最少2个字节信息，支持X,Y,M,S,D,T,C，具体的地址范围需要根据PLC型号来确认，地址支持动态指定站号，例如：s=2;D100<br />
		/// The data written to the PLC in batches is in units of words, that is, at least 2 bytes of information. 
		/// It supports X, Y, M, S, D, T, and C. The specific address range needs to be confirmed according to the PLC model, 
		/// The address supports dynamically specifying the station number, for example: s=2;D100
		/// </summary>
		/// <param name="plc">PLC通信对象</param>
		/// <param name="address">地址信息</param>
		/// <param name="value">数据值</param>
		/// <returns>是否写入成功</returns>
		public static OperateResult Write( IReadWriteFxLinks plc, string address, byte[] value )
		{
			byte stat = (byte)HslHelper.ExtractParameter( ref address, "s", plc.Station );

			// 解析指令
			OperateResult<byte[]> command = BuildWriteByteCommand( stat, address, value, plc.WaittingTime );
			if (!command.IsSuccess) return command;

			// 核心交互
			OperateResult<byte[]> read = plc.ReadFromCoreServer( command.Content );
			if (!read.IsSuccess) return read;

			// 结果验证
			OperateResult<byte[]> extra = CheckPlcResponse( read.Content );
			if (!extra.IsSuccess) return extra;

			// 提取结果
			return OperateResult.CreateSuccessResult( );
		}


#if !NET35 && !NET20
		/// <inheritdoc cref="Read(IReadWriteFxLinks, string, ushort)"/>
		public static async Task<OperateResult<byte[]>> ReadAsync( IReadWriteFxLinks plc, string address, ushort length )
		{
			byte stat = (byte)HslHelper.ExtractParameter( ref address, "s", plc.Station );

			// 解析指令
			OperateResult<List<byte[]>> command = BuildReadCommand( stat, address, length, false, plc.WaittingTime );
			if (!command.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( command );

			List<byte> result = new List<byte>( );
			for (int j = 0; j < command.Content.Count; j++)
			{
				// 核心交互
				OperateResult<byte[]> read = await plc.ReadFromCoreServerAsync( command.Content[j] );
				if (!read.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( read );

				// 结果验证
				OperateResult<byte[]> extra = CheckPlcResponse( read.Content );
				if (!extra.IsSuccess) return extra;

				// 提取结果
				byte[] Content = new byte[extra.Content.Length / 2];
				for (int i = 0; i < Content.Length / 2; i++)
				{
					ushort tmp = Convert.ToUInt16( Encoding.ASCII.GetString( extra.Content, i * 4, 4 ), 16 );
					BitConverter.GetBytes( tmp ).CopyTo( Content, i * 2 );
				}
				result.AddRange( Content );
			}
			return OperateResult.CreateSuccessResult( result.ToArray( ) );
		}

		/// <inheritdoc cref="Write(IReadWriteFxLinks, string, byte[])"/>
		public static async Task<OperateResult> WriteAsync( IReadWriteFxLinks plc, string address, byte[] value )
		{
			byte stat = (byte)HslHelper.ExtractParameter( ref address, "s", plc.Station );

			// 解析指令
			OperateResult<byte[]> command = BuildWriteByteCommand( stat, address, value, plc.WaittingTime );
			if (!command.IsSuccess) return command;

			// 核心交互
			OperateResult<byte[]> read = await plc.ReadFromCoreServerAsync( command.Content );
			if (!read.IsSuccess) return read;

			// 结果验证
			OperateResult<byte[]> extra = CheckPlcResponse( read.Content );
			if (!extra.IsSuccess) return extra;

			// 提取结果
			return OperateResult.CreateSuccessResult( );
		}
#endif

		/// <summary>
		/// 批量读取bool类型数据，支持的类型为X,Y,S,T,C，具体的地址范围取决于PLC的类型，地址支持动态指定站号，例如：s=2;D100<br />
		/// Read bool data in batches. The supported types are X, Y, S, T, C. The specific address range depends on the type of PLC, 
		/// The address supports dynamically specifying the station number, for example: s=2;D100
		/// </summary>
		/// <param name="plc">PLC通信对象</param>
		/// <param name="address">地址信息，比如X10,Y17，注意X，Y的地址是8进制的</param>
		/// <param name="length">读取的长度</param>
		/// <returns>读取结果信息</returns>
		public static OperateResult<bool[]> ReadBool( IReadWriteFxLinks plc, string address, ushort length )
		{
			byte stat = (byte)HslHelper.ExtractParameter( ref address, "s", plc.Station );

			// 解析指令
			OperateResult<List<byte[]>> command = BuildReadCommand( stat, address, length, true, plc.WaittingTime );
			if (!command.IsSuccess) return OperateResult.CreateFailedResult<bool[]>( command );

			List<bool> result = new List<bool>( );
			for (int i = 0; i < command.Content.Count; i++)
			{
				// 核心交互
				OperateResult<byte[]> read = plc.ReadFromCoreServer( command.Content[i] );
				if (!read.IsSuccess) return OperateResult.CreateFailedResult<bool[]>( read );

				// 结果验证
				OperateResult<byte[]> extra = CheckPlcResponse( read.Content );
				if (!extra.IsSuccess) return OperateResult.CreateFailedResult<bool[]>( extra );

				// 提取结果
				result.AddRange( extra.Content.Select( m => m == 0x31 ).ToArray( ) );
			}
			return OperateResult.CreateSuccessResult( result.ToArray( ) );
		}

		/// <summary>
		/// 批量写入bool类型的数组，支持的类型为X,Y,S,T,C，具体的地址范围取决于PLC的类型，地址支持动态指定站号，例如：s=2;D100<br />
		/// Write arrays of type bool in batches. The supported types are X, Y, S, T, C. The specific address range depends on the type of PLC, 
		/// The address supports dynamically specifying the station number, for example: s=2;D100
		/// </summary>
		/// <param name="plc">PLC通信对象</param>
		/// <param name="address">PLC的地址信息</param>
		/// <param name="value">数据信息</param>
		/// <returns>是否写入成功</returns>
		public static OperateResult Write( IReadWriteFxLinks plc, string address, bool[] value )
		{
			byte stat = (byte)HslHelper.ExtractParameter( ref address, "s", plc.Station );

			// 解析指令
			OperateResult<byte[]> command = BuildWriteBoolCommand( stat, address, value, plc.WaittingTime );
			if (!command.IsSuccess) return command;

			// 核心交互
			OperateResult<byte[]> read = plc.ReadFromCoreServer( command.Content );
			if (!read.IsSuccess) return read;

			// 结果验证
			OperateResult<byte[]> extra = CheckPlcResponse( read.Content );
			if (!extra.IsSuccess) return extra;

			// 提取结果
			return OperateResult.CreateSuccessResult( );
		}


#if !NET35 && !NET20
		/// <inheritdoc cref="ReadBool(IReadWriteFxLinks, string, ushort)"/>
		public static async Task<OperateResult<bool[]>> ReadBoolAsync( IReadWriteFxLinks plc, string address, ushort length )
		{
			byte stat = (byte)HslHelper.ExtractParameter( ref address, "s", plc.Station );

			// 解析指令
			OperateResult<List<byte[]>> command = BuildReadCommand( stat, address, length, true, plc.WaittingTime );
			if (!command.IsSuccess) return OperateResult.CreateFailedResult<bool[]>( command );

			List<bool> result = new List<bool>( );
			for (int i = 0; i < command.Content.Count; i++)
			{
				// 核心交互
				OperateResult<byte[]> read = await plc.ReadFromCoreServerAsync( command.Content[i] );
				if (!read.IsSuccess) return OperateResult.CreateFailedResult<bool[]>( read );

				// 结果验证
				OperateResult<byte[]> extra = CheckPlcResponse( read.Content );
				if (!extra.IsSuccess) return OperateResult.CreateFailedResult<bool[]>( extra );

				// 提取结果
				result.AddRange( extra.Content.Select( m => m == 0x31 ).ToArray( ) );
			}
			return OperateResult.CreateSuccessResult( result.ToArray( ) );
		}

		/// <inheritdoc cref="Write(IReadWriteFxLinks, string, bool[])"/>
		public static async Task<OperateResult> WriteAsync( IReadWriteFxLinks plc, string address, bool[] value )
		{
			byte stat = (byte)HslHelper.ExtractParameter( ref address, "s", plc.Station );

			// 解析指令
			OperateResult<byte[]> command = BuildWriteBoolCommand( stat, address, value, plc.WaittingTime );
			if (!command.IsSuccess) return command;

			// 核心交互
			OperateResult<byte[]> read = await plc.ReadFromCoreServerAsync( command.Content );
			if (!read.IsSuccess) return read;

			// 结果验证
			OperateResult<byte[]> extra = CheckPlcResponse( read.Content );
			if (!extra.IsSuccess) return extra;

			// 提取结果
			return OperateResult.CreateSuccessResult( );
		}
#endif


		#region Start Stop

		/// <summary>
		/// <b>[商业授权]</b> 启动PLC的操作，可以携带额外的参数信息，指定站号。举例：s=2; 注意：分号是必须的。<br />
		/// <b>[Authorization]</b> Start the PLC operation, you can carry additional parameter information and specify the station number. Example: s=2; Note: The semicolon is required.
		/// </summary>
		/// <param name="plc">PLC通信对象</param>
		/// <param name="parameter">允许携带的参数信息，例如s=2; 也可以为空</param>
		/// <returns>是否启动成功</returns>
		public static OperateResult StartPLC( IReadWriteFxLinks plc, string parameter = "" )
		{
			byte stat = (byte)HslHelper.ExtractParameter( ref parameter, "s", plc.Station );

			// 解析指令
			OperateResult<byte[]> command = BuildStart( stat, plc.WaittingTime );
			if (!command.IsSuccess) return command;

			// 核心交互
			OperateResult<byte[]> read = plc.ReadFromCoreServer( command.Content );
			if (!read.IsSuccess) return read;

			// 结果验证
			OperateResult<byte[]> extra = CheckPlcResponse( read.Content );
			if (!extra.IsSuccess) return extra;

			// 提取结果
			return OperateResult.CreateSuccessResult( );
		}

		/// <summary>
		/// <b>[商业授权]</b> 停止PLC的操作，可以携带额外的参数信息，指定站号。举例：s=2; 注意：分号是必须的。<br />
		/// <b>[Authorization]</b> Stop PLC operation, you can carry additional parameter information and specify the station number. Example: s=2; Note: The semicolon is required.
		/// </summary>
		/// <param name="plc">PLC通信对象</param>
		/// <param name="parameter">允许携带的参数信息，例如s=2; 也可以为空</param>
		/// <returns>是否停止成功</returns>
		public static OperateResult StopPLC( IReadWriteFxLinks plc, string parameter = "" )
		{
			byte stat = (byte)HslHelper.ExtractParameter( ref parameter, "s", plc.Station );

			// 解析指令
			OperateResult<byte[]> command = BuildStop( stat, plc.WaittingTime );
			if (!command.IsSuccess) return command;

			// 核心交互
			OperateResult<byte[]> read = plc.ReadFromCoreServer( command.Content );
			if (!read.IsSuccess) return read;

			// 结果验证
			OperateResult<byte[]> extra = CheckPlcResponse( read.Content );
			if (!extra.IsSuccess) return extra;

			// 提取结果
			return OperateResult.CreateSuccessResult( );
		}

		/// <summary>
		/// <b>[商业授权]</b> 读取PLC的型号信息，可以携带额外的参数信息，指定站号。举例：s=2; 注意：分号是必须的。<br />
		/// <b>[Authorization]</b> Read the PLC model information, you can carry additional parameter information, and specify the station number. Example: s=2; Note: The semicolon is required.
		/// </summary>
		/// <param name="plc">PLC通信对象</param>
		/// <param name="parameter">允许携带的参数信息，例如s=2; 也可以为空</param>
		/// <returns>带PLC型号的结果信息</returns>
		public static OperateResult<string> ReadPlcType( IReadWriteFxLinks plc, string parameter = "" )
		{
			byte stat = (byte)HslHelper.ExtractParameter( ref parameter, "s", plc.Station );

			// 解析指令
			OperateResult<byte[]> command = BuildReadPlcType( stat, plc.WaittingTime );
			if (!command.IsSuccess) return OperateResult.CreateFailedResult<string>( command );

			// 核心交互
			OperateResult<byte[]> read = plc.ReadFromCoreServer( command.Content );
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<string>( read );

			// 结果验证
			OperateResult<byte[]> extra = CheckPlcResponse( read.Content );
			if (!extra.IsSuccess) return OperateResult.CreateFailedResult<string>( extra );

			// 提取结果
			return GetPlcTypeFromCode( Encoding.ASCII.GetString( read.Content, 5, 2 ) );
		}

		#endregion

		#region Start Stop
#if !NET35 && !NET20
		/// <inheritdoc cref="StartPLC(IReadWriteFxLinks, string)"/>
		public static async Task<OperateResult> StartPLCAsync( IReadWriteFxLinks plc, string parameter = "" )
		{
			byte stat = (byte)HslHelper.ExtractParameter( ref parameter, "s", plc.Station );

			// 解析指令
			OperateResult<byte[]> command = BuildStart( stat, plc.WaittingTime );
			if (!command.IsSuccess) return command;

			// 核心交互
			OperateResult<byte[]> read = await plc.ReadFromCoreServerAsync( command.Content );
			if (!read.IsSuccess) return read;

			// 结果验证
			OperateResult<byte[]> extra = CheckPlcResponse( read.Content );
			if (!extra.IsSuccess) return extra;

			// 提取结果
			return OperateResult.CreateSuccessResult( );
		}

		/// <inheritdoc cref="StopPLC(IReadWriteFxLinks, string)"/>
		public static async Task<OperateResult> StopPLCAsync( IReadWriteFxLinks plc, string parameter = "" )
		{
			byte stat = (byte)HslHelper.ExtractParameter( ref parameter, "s", plc.Station );

			// 解析指令
			OperateResult<byte[]> command = BuildStop( stat, plc.WaittingTime );
			if (!command.IsSuccess) return command;

			// 核心交互
			OperateResult<byte[]> read = await plc.ReadFromCoreServerAsync( command.Content );
			if (!read.IsSuccess) return read;

			// 结果验证
			OperateResult<byte[]> extra = CheckPlcResponse( read.Content );
			if (!extra.IsSuccess) return extra;

			// 提取结果
			return OperateResult.CreateSuccessResult( );
		}

		/// <inheritdoc cref="ReadPlcType(IReadWriteFxLinks, string)"/>
		public static async Task<OperateResult<string>> ReadPlcTypeAsync( IReadWriteFxLinks plc, string parameter = "" )
		{
			byte stat = (byte)HslHelper.ExtractParameter( ref parameter, "s", plc.Station );

			// 解析指令
			OperateResult<byte[]> command = BuildReadPlcType( stat, plc.WaittingTime );
			if (!command.IsSuccess) return OperateResult.CreateFailedResult<string>( command );

			// 核心交互
			OperateResult<byte[]> read = await plc.ReadFromCoreServerAsync( command.Content );
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<string>( read );

			// 结果验证
			OperateResult<byte[]> extra = CheckPlcResponse( read.Content );
			if (!extra.IsSuccess) return OperateResult.CreateFailedResult<string>( extra );

			// 提取结果
			return GetPlcTypeFromCode( Encoding.ASCII.GetString( read.Content, 5, 2 ) );
		}

#endif
		#endregion
	}
}
