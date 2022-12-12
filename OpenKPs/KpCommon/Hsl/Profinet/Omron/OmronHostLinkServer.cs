using HslCommunication.BasicFramework;
using HslCommunication.Core.Net;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Net.Sockets;
using System.Text;
#if !NET20 && !NET35
using System.Threading.Tasks;
#endif

namespace HslCommunication.Profinet.Omron
{
	/// <summary>
	/// <b>[商业授权]</b> 欧姆龙的HostLink虚拟服务器，支持DM区，CIO区，Work区，Hold区，Auxiliary区，可以方便的进行测试<br />
	/// <b>[Authorization]</b> Omron's HostLink virtual server supports DM area, CIO area, Work area, Hold area, and Auxiliary area, which can be easily tested
	/// </summary>
	/// <remarks>
	/// 支持TCP的接口以及串口，方便客户端进行测试，或是开发用于教学的虚拟服务器对象
	/// </remarks>
	public class OmronHostLinkServer : OmronFinsServer
	{
		/// <inheritdoc cref="OmronFinsServer.OmronFinsServer()"/>
		public OmronHostLinkServer( )
		{
			serialPort = new SerialPort( );
		}

		/// <inheritdoc cref="OmronHostLink.UnitNumber"/>
		public byte UnitNumber { get; set; }

		#region NetServer Override

		/// <inheritdoc/>
		protected override void ThreadPoolLoginAfterClientCheck( Socket socket, System.Net.IPEndPoint endPoint )
		{
			AppSession appSession = new AppSession( );
			appSession.IpEndPoint = endPoint;
			appSession.WorkSocket = socket;
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

#if NET20 || NET35
		private void SocketAsyncCallBack( IAsyncResult ar )
#else
		private async void SocketAsyncCallBack( IAsyncResult ar )
#endif
		{
			if (ar.AsyncState is AppSession session)
			{
				try
				{
					int receiveCount = session.WorkSocket.EndReceive( ar );
#if NET20 || NET35
					OperateResult<byte[]> read1 = ReceiveCommandLineFromSocket( session.WorkSocket, 0x0d, 5000 );
#else
					OperateResult<byte[]> read1 = await ReceiveCommandLineFromSocketAsync( session.WorkSocket, 0x0d, 5000 );
#endif
					if (!read1.IsSuccess) { RemoveClient( session ); return; };

					if (!Authorization.asdniasnfaksndiqwhawfskhfaiw( )) { RemoveClient( session ); return; };

					LogNet?.WriteDebug( ToString( ), $"[{session.IpEndPoint}] Tcp {StringResources.Language.Receive}：{SoftBasic.GetAsciiStringRender( read1.Content )}" );

					string hexFinsCore = Encoding.ASCII.GetString( read1.Content, 14, read1.Content.Length - 18 );
					byte[] back = ReadFromFinsCore( SoftBasic.HexStringToBytes( hexFinsCore ) );
					if (back != null)
					{
						session.WorkSocket.Send( back );
						LogNet?.WriteDebug( ToString( ), $"[{session.IpEndPoint}] Tcp {StringResources.Language.Send}：{SoftBasic.GetAsciiStringRender( back )}" );
					}
					else
					{
						RemoveClient( session );
						return;
					}

					session.HeartTime = DateTime.Now;
					RaiseDataReceived( session, read1.Content );
					session.WorkSocket.BeginReceive( new byte[0], 0, 0, SocketFlags.None, new AsyncCallback( SocketAsyncCallBack ), session );
				}
				catch
				{
					RemoveClient( session );
				}
			}
		}


		/// <inheritdoc/>
		protected override byte[] PackCommand( int status, byte[] finsCore, byte[] data )
		{
			if (data == null) data = new byte[0];
			data = SoftBasic.BytesToAsciiBytes( data );

			byte[] back = new byte[27 + data.Length];
			Encoding.ASCII.GetBytes( "@00FA0040000000" ).CopyTo( back, 0 );
			Encoding.ASCII.GetBytes( UnitNumber.ToString( "X2" ) ).CopyTo( back, 1 );

			if (data.Length > 0) data.CopyTo( back, 23 );
			Encoding.ASCII.GetBytes( finsCore.SelectBegin( 2 ).ToHexString( ) ).CopyTo( back, 15 );
			Encoding.ASCII.GetBytes( status.ToString( "X4" ) ).CopyTo( back, 19 );
			// 计算FCS
			int tmp = back[0];
			for (int i = 1; i < back.Length - 4; i++)
			{
				tmp ^= back[i];
			}
			SoftBasic.BuildAsciiBytesFrom( (byte)tmp ).CopyTo( back, back.Length - 4 );
			back[back.Length - 2] = (byte)'*';
			back[back.Length - 1] = 0x0D;
			return back;
		}

		#endregion


		#region Serial Support

		private SerialPort serialPort;            // 核心的串口对象

		/// <summary>
		/// 启动HostLink串口的从机服务，使用默认的参数进行初始化串口，9600波特率，8位数据位，无奇偶校验，1位停止位<br />
		/// Start the slave service of hostlink, initialize the serial port with default parameters, 9600 baud rate, 8 data bits, no parity, 1 stop bit
		/// </summary>
		/// <param name="com">串口信息</param>
		public void StartSerial( string com ) => StartSerial( com, 9600 );

		/// <summary>
		/// 启动HostLink串口的从机服务，使用默认的参数进行初始化串口，7位数据位，偶校验，1位停止位<br />
		/// Start the slave service of hostlink, initialize the serial port with default parameters, 8 data bits, no parity, 1 stop bit
		/// </summary>
		/// <param name="com">串口信息</param>
		/// <param name="baudRate">波特率</param>
		public void StartSerial( string com, int baudRate )
		{
			StartSerial( sp =>
			{
				sp.PortName = com;
				sp.BaudRate = baudRate;
				sp.DataBits = 7;
				sp.Parity = Parity.Even;
				sp.StopBits = StopBits.One;
			} );
		}

		/// <summary>
		/// 启动HostLink串口的从机服务，使用自定义的初始化方法初始化串口的参数<br />
		/// Start the slave service of hostlink and initialize the parameters of the serial port using a custom initialization method
		/// </summary>
		/// <param name="inni">初始化信息的委托</param>
		public void StartSerial( Action<SerialPort> inni )
		{
			if (!serialPort.IsOpen)
			{
				inni?.Invoke( serialPort );

				serialPort.ReadBufferSize = 1024;
				serialPort.ReceivedBytesThreshold = 1;
				serialPort.Open( );
				serialPort.DataReceived += SerialPort_DataReceived;
			}
		}

		/// <summary>
		/// 关闭hostlink的串口对象<br />
		/// Close the serial port object of hostlink
		/// </summary>
		public void CloseSerial( )
		{
			if (serialPort.IsOpen)
			{
				serialPort.Close( );
			}
		}

		/// <summary>
		/// 接收到串口数据的时候触发
		/// </summary>
		/// <param name="sender">串口对象</param>
		/// <param name="e">消息</param>
		private void SerialPort_DataReceived( object sender, SerialDataReceivedEventArgs e )
		{
			int rCount = 0;
			byte[] buffer = new byte[1024];
			while (true)
			{
				System.Threading.Thread.Sleep( 20 );            // 此处做个微小的延时，等待数据接收完成
				int count = serialPort.Read( buffer, rCount, serialPort.BytesToRead );
				rCount += count;
				if (buffer[rCount - 1] == 0x0D) break;
				if (count == 0) break;
			}

			if (rCount == 0) return;
			byte[] receive = buffer.SelectBegin( rCount );
			if (receive.Length < 22)
			{
				LogNet?.WriteError( ToString( ), $"[{serialPort.PortName}] Uknown Data：{receive.ToHexString( ' ' )}" );
				return;
			}


			if (!Authorization.asdniasnfaksndiqwhawfskhfaiw( )) { return; };

			LogNet?.WriteDebug( ToString( ), $"[{serialPort.PortName}] {StringResources.Language.Receive}：{SoftBasic.GetAsciiStringRender( receive )}" );

			string hexFinsCore = Encoding.ASCII.GetString( receive, 14, receive.Length - 18 );
			byte[] back = ReadFromFinsCore( SoftBasic.HexStringToBytes( hexFinsCore ) );
			if (back != null)
			{
				serialPort.Write( back, 0, back.Length );
				LogNet?.WriteDebug( ToString( ), $"[{serialPort.PortName}] {StringResources.Language.Send}：{SoftBasic.GetAsciiStringRender( back )}" );
			}

			RaiseDataReceived( sender, receive );
		}

		#endregion

		#region Object Override

		/// <inheritdoc/>
		public override string ToString( ) => $"OmronHostLinkServer[{Port}]";

		#endregion
	}
}
