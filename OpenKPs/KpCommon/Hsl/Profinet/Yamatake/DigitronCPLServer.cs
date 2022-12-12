using HslCommunication.BasicFramework;
using HslCommunication.Core.Net;
using HslCommunication.Core;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Net.Sockets;
using System.Text;
#if !NET20 && !NET35
using System.Threading.Tasks;
#endif

namespace HslCommunication.Profinet.Yamatake
{
	/// <summary>
	/// 山武的数字指示调节器的虚拟设备，支持和HSL本身进行数据通信测试<br />
	/// Yamatake’s digital indicating regulator is a virtual device that supports data communication testing with HSL itself
	/// </summary>
	public class DigitronCPLServer : NetworkDataServerBase
	{
		/// <summary>
		/// 实例化一个默认的对象<br />
		/// Instantiate a default object
		/// </summary>
		public DigitronCPLServer( )
		{
			serialPort    = new SerialPort( );
			softBuffer    = new SoftBuffer( DataPoolLength * 2 );
			ByteTransform = new RegularByteTransform( );
			Station       = 1;
		}

		/// <summary>
		/// 获取或设置当前虚拟仪表的站号信息，如果站号不一致，将不予访问<br />
		/// Get or set the station number information of the current virtual instrument. If the station number is inconsistent, it will not be accessed
		/// </summary>
		public byte Station { get; set; }

		#region Read Write

		/// <inheritdoc/>
		public override OperateResult<byte[]> Read( string address, ushort length )
		{
			try
			{
				ushort add = ushort.Parse( address );
				return OperateResult.CreateSuccessResult( softBuffer.GetBytes( add * 2, length * 2 ) );
			}
			catch( Exception ex)
			{
				return new OperateResult<byte[]>( "Read Failed: " + ex.Message );
			}
		}

		/// <inheritdoc/>
		public override OperateResult Write( string address, byte[] value )
		{
			try
			{
				ushort add = ushort.Parse( address );
				softBuffer.SetBytes( value, add * 2 );
				return OperateResult.CreateSuccessResult( );
			}
			catch (Exception ex)
			{
				return new OperateResult( "Write Failed: " + ex.Message );
			}
		}

		#endregion


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
					OperateResult<byte[]> read1 = ReceiveCommandLineFromSocket( session.WorkSocket, 0x0A, 5000 );
#else
					OperateResult<byte[]> read1 = await ReceiveCommandLineFromSocketAsync( session.WorkSocket, 0x0A, 5000 );
#endif
					if (!read1.IsSuccess) { RemoveClient( session ); return; };

					if (!Authorization.asdniasnfaksndiqwhawfskhfaiw( )) { RemoveClient( session ); return; };

					LogNet?.WriteDebug( ToString( ), $"[{session.IpEndPoint}] Tcp {StringResources.Language.Receive}：{SoftBasic.GetAsciiStringRender( read1.Content )}" );

					byte[] back = ReadFromCore( read1.Content );
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

		private byte[] ReadFromCore( byte[] command )
		{
			try
			{
				int endIndex = 9;
				for (int i = 9; i < command.Length; i++)
				{
					if (command[i] == 0x03)
					{
						endIndex = i;
						break;
					}
				}

				byte station = Convert.ToByte( Encoding.ASCII.GetString( command, 1, 2 ), 16 );
				if(station != this.Station) return Helper.DigitronCPLHelper.PackResponseContent( this.Station, 40, null, 0x57 );

				string[] datas = Encoding.ASCII.GetString( command, 9, endIndex - 9 ).Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries );
				string cmd = Encoding.ASCII.GetString( command, 6, 2 );
				int address = int.Parse( datas[0].Substring( 0, datas[0].Length - 1 ) );
				byte dataType = datas[0].EndsWith( "W" ) ? (byte)0x57 : (byte)0x53;

				if (address >= 65536 || address < 0) return Helper.DigitronCPLHelper.PackResponseContent( this.Station, 42, null, dataType );
				
				if (cmd == "RS")
				{
					int length = int.Parse( datas[1] );
					if ((address + length) > 65535 ) return Helper.DigitronCPLHelper.PackResponseContent( this.Station, 42, null, dataType );

					if (length > 16) return Helper.DigitronCPLHelper.PackResponseContent( this.Station, 41, null, dataType );
					return Helper.DigitronCPLHelper.PackResponseContent( this.Station, 0,
						softBuffer.GetBytes( address * 2, length * 2 ), dataType );
				}
				else if (cmd == "WS")
				{
					if (!EnableWrite) return Helper.DigitronCPLHelper.PackResponseContent( this.Station, 46, null, dataType );

					if (datas.Length > 17) return Helper.DigitronCPLHelper.PackResponseContent( this.Station, 41, null, dataType );
					byte[] buffer = new byte[(datas.Length - 1) * 2];
					for (int i = 1; i < datas.Length; i++)
					{
						if (dataType == 0x57)
							BitConverter.GetBytes( short.Parse( datas[i] ) ).CopyTo( buffer, i * 2 - 2 );
						else
							BitConverter.GetBytes( ushort.Parse( datas[i] ) ).CopyTo( buffer, i * 2 - 2 );
					}
					softBuffer.SetBytes( buffer, address * 2 );
					return Helper.DigitronCPLHelper.PackResponseContent( this.Station, 0, null, dataType );
				}
				else
					return Helper.DigitronCPLHelper.PackResponseContent( this.Station, 40, null, dataType );
			}
			catch
			{
				return null;
			}
		}


		#endregion


		#region Serial Support

		private SerialPort serialPort;            // 核心的串口对象

		/// <summary>
		/// 启动CPL串口的从机服务，使用默认的参数进行初始化串口，9600波特率，8位数据位，无奇偶校验，1位停止位<br />
		/// Start the slave service of hostlink, initialize the serial port with default parameters, 9600 baud rate, 8 data bits, no parity, 1 stop bit
		/// </summary>
		/// <param name="com">串口信息</param>
		public void StartSerial( string com ) => StartSerial( com, 9600 );

		/// <summary>
		/// 启动CPL串口的从机服务，使用默认的参数进行初始化串口，7位数据位，偶校验，1位停止位<br />
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
		/// 启动CPL串口的从机服务，使用自定义的初始化方法初始化串口的参数<br />
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
		/// 关闭CPL的串口对象<br />
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
				if (buffer[rCount - 1] == 0x0A) break;
				if (count == 0) break;
			}

			if (rCount == 0) return;
			byte[] receive = buffer.SelectBegin( rCount );

			if (!Authorization.asdniasnfaksndiqwhawfskhfaiw( )) { return; };

			LogNet?.WriteDebug( ToString( ), $"[{serialPort.PortName}] {StringResources.Language.Receive}：{SoftBasic.GetAsciiStringRender( receive )}" );

			byte[] back = ReadFromCore( receive );
			if (back != null)
			{
				serialPort.Write( back, 0, back.Length );
				LogNet?.WriteDebug( ToString( ), $"[{serialPort.PortName}] {StringResources.Language.Send}：{SoftBasic.GetAsciiStringRender( back )}" );
			}

			RaiseDataReceived( sender, receive );
		}

		#endregion

		#region Private Member

		private SoftBuffer softBuffer;                 // 输入寄存器的数据池
		private const int DataPoolLength = 65536;      // 数据的长度

		#endregion

		#region Object Override

		/// <inheritdoc/>
		public override string ToString( ) => $"DigitronCPLServer[{Port}]";

		#endregion
	}
}
