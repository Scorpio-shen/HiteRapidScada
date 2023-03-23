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
	/// 欧姆龙的OmronHostLink相关辅助方法
	/// </summary>
	public class OmronHostLinkHelper
	{

		/// <summary>
		/// 验证欧姆龙的Fins-TCP返回的数据是否正确的数据，如果正确的话，并返回所有的数据内容
		/// </summary>
		/// <param name="send">发送的报文信息</param>
		/// <param name="response">来自欧姆龙返回的数据内容</param>
		/// <returns>带有是否成功的结果对象</returns>
		public static OperateResult<byte[]> ResponseValidAnalysis( byte[] send, byte[] response )
		{
			// 数据有效性分析
			// @00FA00400000000102000040*\cr
			if (response.Length >= 27)
			{
				string commandSend = Encoding.ASCII.GetString( send, 14, 4 );
				string commandReceive = Encoding.ASCII.GetString( response, 15, 4 );
				if (commandReceive != commandSend)
					return new OperateResult<byte[]>( $"Send Command [{commandSend}] not the same as receive command [{commandReceive}] source:[{SoftBasic.GetAsciiStringRender( response )}]" );
				int err;
				// 提取错误码
				try
				{
					err = Convert.ToInt32( Encoding.ASCII.GetString( response, 19, 4 ), 16 );
				}
				catch( Exception ex )
				{
					return new OperateResult<byte[]>( "Get error code failed: " + ex.Message + Environment.NewLine + "Source Data: " + SoftBasic.GetAsciiStringRender( response ) );
				}
				byte[] content = new byte[0];
				if (response.Length > 27) content = SoftBasic.HexStringToBytes( Encoding.ASCII.GetString( response, 23, response.Length - 27 ) );

				if (err > 0) return new OperateResult<byte[]>( )
				{
					ErrorCode = err,
					Content = content,
					Message = GetErrorText( err )
				};
				else
				{
					// 多个数据块读取的情况，还需要二次解析数据
					if (Encoding.ASCII.GetString( response, 15, 4 ) == "0104")
					{
						byte[] buffer = content.Length > 0 ? new byte[content.Length * 2 / 3] : new byte[0];
						for (int i = 0; i < content.Length / 3; i++)
						{
							buffer[i * 2 + 0] = content[i * 3 + 1];
							buffer[i * 2 + 1] = content[i * 3 + 2];
						}
						content = buffer;
					}
					return OperateResult.CreateSuccessResult( content );
				}
			}

			return new OperateResult<byte[]>( StringResources.Language.OmronReceiveDataError + " Source Data: " + response.ToHexString( ' ' ) );
		}

		/// <summary>
		/// 根据错误信息获取当前的文本描述信息
		/// </summary>
		/// <param name="error">错误代号</param>
		/// <returns>文本消息</returns>
		public static string GetErrorText( int error )
		{
			switch (error)
			{
				case 0x0001: return "Service was canceled.";
				case 0x0101: return "Local node is not participating in the network.";
				case 0x0102: return "Token does not arrive.";
				case 0x0103: return "Send was not possible during the specified number of retries.";
				case 0x0104: return "Cannot send because maximum number of event frames exceeded.";
				case 0x0105: return "Node address setting error occurred.";
				case 0x0106: return "The same node address has been set twice in the same network.";
				case 0x0201: return "The destination node is not in the network.";
				case 0x0202: return "There is no Unit with the specified unit address.";
				case 0x0203: return "The third node does not exist.";
				case 0x0204: return "The destination node is busy.";
				case 0x0205: return "The message was destroyed by noise";
				case 0x0301: return "An error occurred in the communications controller.";
				case 0x0302: return "A CPU error occurred in the destination CPU Unit.";
				case 0x0303: return "A response was not returned because an error occurred in the Board.";
				case 0x0304: return "The unit number was set incorrectly";
				case 0x0401: return "The Unit/Board does not support the specified command code.";
				case 0x0402: return "The command cannot be executed because the model or version is incorrect";
				case 0x0501: return "The destination network or node address is not set in the routing tables.";
				case 0x0502: return "Relaying is not possible because there are no routing tables";
				case 0x0503: return "There is an error in the routing tables.";
				case 0x0504: return "An attempt was made to send to a network that was over 3 networks away";
				// Command format error
				case 0x1001: return "The command is longer than the maximum permissible length.";
				case 0x1002: return "The command is shorter than the minimum permissible length.";
				case 0x1003: return "The designated number of elements differs from the number of write data items.";
				case 0x1004: return "An incorrect format was used.";
				case 0x1005: return "Either the relay table in the local node or the local network table in the relay node is incorrect.";
				// Parameter error
				case 0x1101: return "The specified word does not exist in the memory area or there is no EM Area.";
				case 0x1102: return "The access size specification is incorrect or an odd word address is specified.";
				case 0x1103: return "The start address in command process is beyond the accessible area";
				case 0x1104: return "The end address in command process is beyond the accessible area.";
				case 0x1106: return "FFFF hex was not specified.";
				case 0x1109: return "A large–small relationship in the elements in the command data is incorrect.";
				case 0x110B: return "The response format is longer than the maximum permissible length.";
				case 0x110C: return "There is an error in one of the parameter settings.";
				// Read Not Possible
				case 0x2002: return "The program area is protected.";
				case 0x2003: return "A table has not been registered.";
				case 0x2004: return "The search data does not exist.";
				case 0x2005: return "A non-existing program number has been specified.";
				case 0x2006: return "The file does not exist at the specified file device.";
				case 0x2007: return "A data being compared is not the same.";
				// Write not possible
				case 0x2101: return "The specified area is read-only.";
				case 0x2102: return "The program area is protected.";
				case 0x2103: return "The file cannot be created because the limit has been exceeded.";
				case 0x2105: return "A non-existing program number has been specified.";
				case 0x2106: return "The file does not exist at the specified file device.";
				case 0x2107: return "A file with the same name already exists in the specified file device.";
				case 0x2108: return "The change cannot be made because doing so would create a problem.";
				// Not executable in current mode
				case 0x2201:
				case 0x2202:
				case 0x2208: return "The mode is incorrect.";
				case 0x2203: return "The PLC is in PROGRAM mode.";
				case 0x2204: return "The PLC is in DEBUG mode.";
				case 0x2205: return "The PLC is in MONITOR mode.";
				case 0x2206: return "The PLC is in RUN mode.";
				case 0x2207: return "The specified node is not the polling node.";
				//  No such device
				case 0x2301: return "The specified memory does not exist as a file device.";
				case 0x2302: return "There is no file memory.";
				case 0x2303: return "There is no clock.";
				case 0x2401: return "The data link tables have not been registered or they contain an error.";
				default: return StringResources.Language.UnknownError;
			}
		}

		/// <summary>
		/// 将 fins 命令的报文打包成 HostLink 格式的报文信息，打包之后的结果可以直接发送给PLC<br />
		/// Pack the message of the fins command into the message information in the HostLink format, and the packaged result can be sent directly to the PLC
		/// </summary>
		/// <param name="hostLink">HostLink协议的plc通信对象</param>
		/// <param name="station">站号信息</param>
		/// <param name="cmd">fins命令</param>
		/// <returns>可发送PLC的完整的报文信息</returns>
		public static byte[] PackCommand( IHostLink hostLink, byte station, byte[] cmd )
		{
			cmd = SoftBasic.BytesToAsciiBytes( cmd );

			byte[] buffer = new byte[18 + cmd.Length];

			buffer[ 0] = (byte)'@';
			buffer[ 1] = SoftBasic.BuildAsciiBytesFrom( station )[0];
			buffer[ 2] = SoftBasic.BuildAsciiBytesFrom( station )[1];
			buffer[ 3] = (byte)'F';
			buffer[ 4] = (byte)'A';
			buffer[ 5] = hostLink.ResponseWaitTime;
			buffer[ 6] = SoftBasic.BuildAsciiBytesFrom( hostLink.ICF )[0];
			buffer[ 7] = SoftBasic.BuildAsciiBytesFrom( hostLink.ICF )[1];
			buffer[ 8] = SoftBasic.BuildAsciiBytesFrom( hostLink.DA2 )[0];
			buffer[ 9] = SoftBasic.BuildAsciiBytesFrom( hostLink.DA2 )[1];
			buffer[10] = SoftBasic.BuildAsciiBytesFrom( hostLink.SA2 )[0];
			buffer[11] = SoftBasic.BuildAsciiBytesFrom( hostLink.SA2 )[1];
			buffer[12] = SoftBasic.BuildAsciiBytesFrom( hostLink.SID )[0];
			buffer[13] = SoftBasic.BuildAsciiBytesFrom( hostLink.SID )[1];
			buffer[buffer.Length - 2] = (byte)'*';
			buffer[buffer.Length - 1] = 0x0D;
			cmd.CopyTo( buffer, 14 );
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


		/// <inheritdoc cref="OmronFinsNet.Read(string, ushort)"/>
		public static OperateResult<byte[]> Read( IHostLink hostLink, string address, ushort length )
		{
			byte station = (byte)HslHelper.ExtractParameter( ref address, "s", hostLink.UnitNumber );

			// 解析地址
			var command = OmronFinsNetHelper.BuildReadCommand( address, length, false, hostLink.ReadSplits );
			if (!command.IsSuccess) return command.ConvertFailed<byte[]>( );

			List<byte> contentArray = new List<byte>( );
			for (int i = 0; i < command.Content.Count; i++)
			{
				// 核心交互
				OperateResult<byte[]> read = hostLink.ReadFromCoreServer( PackCommand( hostLink, station, command.Content[i] ) );
				if (!read.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( read );

				// 读取到了正确的数据
				contentArray.AddRange( read.Content );
			}
			return OperateResult.CreateSuccessResult( contentArray.ToArray( ) );
		}

		/// <inheritdoc cref="OmronFinsNetHelper.Read(IReadWriteDevice, string[])"/>
		/// <remarks>
		/// 如果需要需要额外指定站号的话，在第一个地址里，使用 s=2;D100 这种携带地址的功能
		/// </remarks>
		public static OperateResult<byte[]> Read( IHostLink hostLink, string[] address )
		{
			byte station = hostLink.UnitNumber;
			if (address?.Length > 0) station = (byte)HslHelper.ExtractParameter( ref address[0], "s", hostLink.UnitNumber );

			// 解析地址
			var command = OmronFinsNetHelper.BuildReadCommand( address );
			if (!command.IsSuccess) return command.ConvertFailed<byte[]>( );

			List<byte> contentArray = new List<byte>( );
			for (int i = 0; i < command.Content.Count; i++)
			{
				// 核心交互
				OperateResult<byte[]> read = hostLink.ReadFromCoreServer( PackCommand( hostLink, station, command.Content[i] ) );
				if (!read.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( read );

				// 读取到了正确的数据
				contentArray.AddRange( read.Content );
			}
			return OperateResult.CreateSuccessResult( contentArray.ToArray( ) );
		}

		/// <inheritdoc cref="OmronFinsNet.Write(string, byte[])"/>
		public static OperateResult Write( IHostLink hostLink, string address, byte[] value )
		{
			byte station = (byte)HslHelper.ExtractParameter( ref address, "s", hostLink.UnitNumber );
			// 获取指令
			var command = OmronFinsNetHelper.BuildWriteWordCommand( address, value, false ); ;
			if (!command.IsSuccess) return command;

			// 核心数据交互
			OperateResult<byte[]> read = hostLink.ReadFromCoreServer( PackCommand( hostLink, station, command.Content ) );
			if (!read.IsSuccess) return read;

			// 成功
			return OperateResult.CreateSuccessResult( );
		}

		#region Async Read Write Support
#if !NET35 && !NET20
		/// <inheritdoc cref="OmronFinsNet.Read(string, ushort)"/>
		public static async Task<OperateResult<byte[]>> ReadAsync( IHostLink hostLink, string address, ushort length )
		{
			byte station = (byte)HslHelper.ExtractParameter( ref address, "s", hostLink.UnitNumber );

			// 解析地址
			var command = OmronFinsNetHelper.BuildReadCommand( address, length, false, hostLink.ReadSplits );
			if (!command.IsSuccess) return command.ConvertFailed<byte[]>( );

			List<byte> contentArray = new List<byte>( );
			for (int i = 0; i < command.Content.Count; i++)
			{
				// 核心交互
				OperateResult<byte[]> read = await hostLink.ReadFromCoreServerAsync( PackCommand( hostLink, station, command.Content[i] ) );
				if (!read.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( read );

				// 读取到了正确的数据
				contentArray.AddRange( read.Content );
			}

			return OperateResult.CreateSuccessResult( contentArray.ToArray( ) );
		}

		/// <inheritdoc cref="OmronFinsNet.Write(string, byte[])"/>
		public static async Task<OperateResult> WriteAsync( IHostLink hostLink, string address, byte[] value )
		{
			byte station = (byte)HslHelper.ExtractParameter( ref address, "s", hostLink.UnitNumber );

			// 获取指令
			var command = OmronFinsNetHelper.BuildWriteWordCommand( address, value, false ); ;
			if (!command.IsSuccess) return command;

			// 核心数据交互
			OperateResult<byte[]> read = await hostLink.ReadFromCoreServerAsync( PackCommand( hostLink, station, command.Content ) );
			if (!read.IsSuccess) return read;

			// 成功
			return OperateResult.CreateSuccessResult( );
		}

		/// <inheritdoc cref="Read(IHostLink, string[])"/>
		public async static Task<OperateResult<byte[]>> ReadAsync( IHostLink hostLink, string[] address )
		{
			byte station = hostLink.UnitNumber;
			if (address?.Length > 0) station = (byte)HslHelper.ExtractParameter( ref address[0], "s", hostLink.UnitNumber );

			// 解析地址
			var command = OmronFinsNetHelper.BuildReadCommand( address );
			if (!command.IsSuccess) return command.ConvertFailed<byte[]>( );

			List<byte> contentArray = new List<byte>( );
			for (int i = 0; i < command.Content.Count; i++)
			{
				// 核心交互
				OperateResult<byte[]> read = await hostLink.ReadFromCoreServerAsync( PackCommand( hostLink, station, command.Content[i] ) );
				if (!read.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( read );

				// 读取到了正确的数据
				contentArray.AddRange( read.Content );
			}
			return OperateResult.CreateSuccessResult( contentArray.ToArray( ) );
		}
#endif
		#endregion


		/// <inheritdoc cref="OmronFinsNet.ReadBool(string, ushort)"/>
		public static OperateResult<bool[]> ReadBool( IHostLink hostLink, string address, ushort length )
		{
			byte station = (byte)HslHelper.ExtractParameter( ref address, "s", hostLink.UnitNumber );

			// 获取指令
			var command = OmronFinsNetHelper.BuildReadCommand( address, length, true );
			if (!command.IsSuccess) return OperateResult.CreateFailedResult<bool[]>( command );

			List<bool> contentArray = new List<bool>( );
			for (int i = 0; i < command.Content.Count; i++)
			{
				// 核心交互
				OperateResult<byte[]> read = hostLink.ReadFromCoreServer( PackCommand( hostLink, station, command.Content[i] ) );
				if (!read.IsSuccess) return OperateResult.CreateFailedResult<bool[]>( read );

				// 返回正确的数据信息
				if (read.Content.Length == 0) return new OperateResult<bool[]>( "Data is empty." );
				contentArray.AddRange( read.Content.Select( m => m != 0x00 ) );
			}
			return OperateResult.CreateSuccessResult( contentArray.ToArray( ) );
		}

		/// <inheritdoc cref="OmronFinsNet.Write(string, bool[])"/>
		public static OperateResult Write( IHostLink hostLink, string address, bool[] values )
		{
			byte station = (byte)HslHelper.ExtractParameter( ref address, "s", hostLink.UnitNumber );

			// 获取指令
			var command = OmronFinsNetHelper.BuildWriteWordCommand( address, values.Select( m => m ? (byte)0x01 : (byte)0x00 ).ToArray( ), true ); ;
			if (!command.IsSuccess) return command;

			// 核心数据交互
			OperateResult<byte[]> read = hostLink.ReadFromCoreServer( PackCommand( hostLink, station, command.Content ) );
			if (!read.IsSuccess) return read;

			// 成功
			return OperateResult.CreateSuccessResult( );
		}


		#region Async Read Write Bool
#if !NET35 && !NET20
		/// <inheritdoc cref="OmronFinsNet.ReadBool(string, ushort)"/>
		public static async Task<OperateResult<bool[]>> ReadBoolAsync( IHostLink hostLink, string address, ushort length )
		{
			byte station = (byte)HslHelper.ExtractParameter( ref address, "s", hostLink.UnitNumber );

			// 获取指令
			var command = OmronFinsNetHelper.BuildReadCommand( address, length, true );
			if (!command.IsSuccess) return OperateResult.CreateFailedResult<bool[]>( command );

			List<bool> contentArray = new List<bool>( );
			for (int i = 0; i < command.Content.Count; i++)
			{
				// 核心交互
				OperateResult<byte[]> read = await hostLink.ReadFromCoreServerAsync( PackCommand( hostLink, station, command.Content[i] ) );
				if (!read.IsSuccess) return OperateResult.CreateFailedResult<bool[]>( read );

				// 返回正确的数据信息
				contentArray.AddRange( read.Content.Select( m => m != 0x00 ) );
			}
			return OperateResult.CreateSuccessResult( contentArray.ToArray( ) );
		}

		/// <inheritdoc cref="OmronFinsNet.Write(string, bool[])"/>
		public static async Task<OperateResult> WriteAsync( IHostLink hostLink, string address, bool[] values )
		{
			byte station = (byte)HslHelper.ExtractParameter( ref address, "s", hostLink.UnitNumber );

			// 获取指令
			var command = OmronFinsNetHelper.BuildWriteWordCommand( address, values.Select( m => m ? (byte)0x01 : (byte)0x00 ).ToArray( ), true ); ;
			if (!command.IsSuccess) return command;

			// 核心数据交互
			OperateResult<byte[]> read = await hostLink.ReadFromCoreServerAsync( PackCommand( hostLink, station, command.Content ) );
			if (!read.IsSuccess) return read;

			// 成功
			return OperateResult.CreateSuccessResult( );
		}
#endif
		#endregion

	}
}
