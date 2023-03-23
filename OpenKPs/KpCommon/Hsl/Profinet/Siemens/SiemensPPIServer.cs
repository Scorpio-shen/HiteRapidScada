using HslCommunication.BasicFramework;
using HslCommunication.Core.Address;
using HslCommunication.Core.IMessage;
using HslCommunication.Core.Net;
using HslCommunication.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using HslCommunication.Serial;
#if !NET35 && !NET20
using System.Threading.Tasks;
#endif

namespace HslCommunication.Profinet.Siemens
{
	/// <summary>
	/// PPIServer的虚拟服务器对象，支持的地址类型和S7虚拟服务器一致
	/// </summary>
	public class SiemensPPIServer : SiemensS7Server
	{
		/// <summary>
		/// 实例化一个默认的对象
		/// </summary>
		public SiemensPPIServer( )
		{
		}

		#region Public Properties

		/// <inheritdoc cref="SiemensPPIOverTcp.Station"/>
		public byte Station { get; set; }

		#endregion

		#region NetServer Override

		/// <inheritdoc/>
		protected override INetMessage GetNewNetMessage( ) => new SiemensPPIMessage( );

		/// <inheritdoc/>
		protected override void ThreadPoolLoginAfterClientCheck( Socket socket, System.Net.IPEndPoint endPoint )
		{
			// 开始接收数据信息
			AppSession appSession = new AppSession( socket );
			try
			{
				socket.BeginReceive( new byte[0], 0, 0, SocketFlags.None, new AsyncCallback( SocketAsyncCallBack ), appSession );
				AddClient( appSession );
			}
			catch
			{
				socket.Close( );
				LogNet?.WriteDebug( ToString( ), string.Format( StringResources.Language.ClientOfflineInfo, endPoint ) );
			}
		}

		/// <inheritdoc/>
		protected override OperateResult<byte[]> ReadFromCoreServer( AppSession session, byte[] receive )
		{
			// 检测站号
			if (receive[4] != Station) return new OperateResult<byte[]>( $"Station not match, expect: {Station}, but actual: {receive[4]}" );

			// 回发一个0xE5的消息
			Send( session.WorkSocket, new byte[] { 0xE5 } );
			LogNet?.WriteDebug( ToString( ),  $"[{session.IpEndPoint}] Tcp {StringResources.Language.Send}：E5" );

			// 接收6个字节的确认信息
			OperateResult<byte[]> check = ReceiveByMessage( session.WorkSocket, 5000, new SpecifiedCharacterMessage( AsciiControl.SYN ) );
			if (!check.IsSuccess) return check;

			LogNet?.WriteDebug( ToString( ), $"[{session.IpEndPoint}] Tcp {StringResources.Language.Receive}：{ SoftBasic.ByteToHexString( check.Content, ' ' ) }" );
			return base.ReadFromCoreServer( session, receive );
		}

		#endregion

		#region Siemens Override

		/// <inheritdoc/>
		protected override bool IsNeedShakeHands( ) => false;

		/// <inheritdoc/>
		protected override byte[] PackReadBack( byte[] command, List<byte> content )
		{
			byte[] back = new byte[21 + content.Count + 2];
			SoftBasic.HexStringToBytes( "68 1D 1D 68 00 02 08 32 03 00 00 00 00 00 02 00 0C 00 00 04 01" ).CopyTo( back, 0 );
			back[ 1] = (byte)(back.Length - 6);
			back[ 2] = (byte)(back.Length - 6);
			back[15] = (byte)(content.Count / 256);
			back[16] = (byte)(content.Count % 256);
			back[20] = command[18];
			content.CopyTo( back, 21 );

			back[back.Length - 2] = (byte)SoftLRC.CalculateAcc( back, 4, 2 );
			back[back.Length - 1] = AsciiControl.SYN;
			return back;
		}

		/// <inheritdoc/>
		protected override byte[] PackWriteBack( byte[] packCommand, byte status )
		{
			byte[] buffer = SoftBasic.HexStringToBytes( "68 12 12 68 00 02 08 32 03 00 00 00 01 00 02 00 01 00 00 05 01 04 00 16" );
			buffer[buffer.Length - 3] = status;
			buffer[buffer.Length - 2] = (byte)SoftLRC.CalculateAcc( buffer, 4, 2 );
			return buffer;
		}

		/// <inheritdoc/>
		protected override OperateResult<byte[]> DealWithSerialReceivedData( byte[] data )
		{
			// 检测站号
			if (data[4] != Station) return new OperateResult<byte[]>( $"Station not match, expect: {Station}, but actual: {data[4]}" );

			GetSerialPort( ).Write( new byte[] { 0xE5 }, 0, 1 );
			LogNet?.WriteDebug( ToString( ), $"[{GetSerialPort( ).PortName}] {StringResources.Language.Send}：E5" );

			OperateResult<byte[]> check = GetSerialPort( ).Receive( 6, 5000, 20 );
			if (!check.IsSuccess) return check;
			LogNet?.WriteDebug( ToString( ), $"[{GetSerialPort( ).PortName}] {StringResources.Language.Receive}：{GetSerialMessageLogText(check.Content)}" );
			
			OperateResult<byte[]> read = base.ReadFromCoreServer( null, data );
			if (!read.IsSuccess) return read;

			return read;
		}

		/// <inheritdoc/>
		protected override bool CheckSerialReceiveDataComplete( byte[] buffer, int receivedLength )
		{
			if (receivedLength == 6 && buffer[0] == AsciiControl.DLE && buffer[1] == AsciiControl.STX && buffer[5] == AsciiControl.SYN) return true;
			if (receivedLength > 6 && buffer[0] == 0x68 && (buffer[1] + 6 == receivedLength) && buffer[receivedLength - 1] == AsciiControl.SYN) return true;
			return base.CheckSerialReceiveDataComplete( buffer, receivedLength );
		}

		#endregion

		#region Object Override

		/// <inheritdoc/>
		public override string ToString( ) => $"SiemensPPIServer[{Port}]";

		#endregion
	}
}
