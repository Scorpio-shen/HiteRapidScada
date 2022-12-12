using HslCommunication.BasicFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HslCommunication;
using HslCommunication.Reflection;
using HslCommunication.Core.Net;
using System.Net.Sockets;
using System.IO.Ports;
using HslCommunication.Core.IMessage;
#if !NET20 && !NET35
using System.Threading.Tasks;
#endif

namespace HslCommunication.Profinet.Panasonic
{
	/// <summary>
	/// 松下Mewtocol协议的虚拟服务器，支持串口和网口的操作
	/// </summary>
	public class PanasonicMewtocolServer : NetworkDataServerBase
	{
		#region Constructor

		/// <summary>
		/// 实例化一个默认的对象
		/// </summary>
		public PanasonicMewtocolServer( )
		{
			rBuffer = new SoftBuffer( DataPoolLength * 2 );
			dBuffer = new SoftBuffer( DataPoolLength * 2 );
		}

		#endregion

		#region Public Members

		/// <inheritdoc cref="PanasonicMewtocol.Station"/>
		public byte Station
		{
			get { return station; }
			set { station = value; }
		}

		#endregion

		#region Data Persistence

		/// <inheritdoc/>
		protected override byte[] SaveToBytes( )
		{
			byte[] buffer = new byte[DataPoolLength * 4];
			Array.Copy( rBuffer.GetBytes( ), 0, buffer, DataPoolLength * 0, DataPoolLength * 2 );
			Array.Copy( dBuffer.GetBytes( ), 0, buffer, DataPoolLength * 2, DataPoolLength * 2 );
			return buffer;
		}

		/// <inheritdoc/>
		protected override void LoadFromBytes( byte[] content )
		{
			if (content.Length < DataPoolLength * 4) throw new Exception( "File is not correct" );

			rBuffer.SetBytes( content, DataPoolLength * 0, 0, DataPoolLength * 2 );
			dBuffer.SetBytes( content, DataPoolLength * 2, 0, DataPoolLength * 2);
		}

		#endregion

		#region NetworkDataServerBase Override

		/// <inheritdoc cref="PanasonicMewtocol.Read(string, ushort)"/>
		[HslMqttApi( "ReadByteArray", "" )]
		public override OperateResult<byte[]> Read( string address, ushort length )
		{
			return new OperateResult<byte[]>( StringResources.Language.NotSupportedDataType );
		}

		/// <inheritdoc cref="PanasonicMewtocol.Write(string, byte[])"/>
		[HslMqttApi( "WriteByteArray", "" )]
		public override OperateResult Write( string address, byte[] value )
		{
			return new OperateResult<byte[]>( StringResources.Language.NotSupportedDataType );
		}

		/// <inheritdoc cref="PanasonicMewtocol.ReadBool(string, ushort)"/>
		[HslMqttApi( "ReadBoolArray", "" )]
		public override OperateResult<bool[]> ReadBool( string address, ushort length )
		{
			return new OperateResult<bool[]>( StringResources.Language.NotSupportedDataType );
		}

		/// <inheritdoc cref="PanasonicMewtocol.Write(string, bool[])"/>
		[HslMqttApi( "WriteBoolArray", "" )]
		public override OperateResult Write( string address, bool[] value )
		{
			return new OperateResult<byte[]>( StringResources.Language.NotSupportedDataType );
		}

		#endregion

		#region NetServer Override

		/// <inheritdoc/>
		protected override void ThreadPoolLoginAfterClientCheck( Socket socket, System.Net.IPEndPoint endPoint )
		{
			// 开始接收数据信息
			AppSession appSession = new AppSession( );
			appSession.IpEndPoint = endPoint;
			appSession.WorkSocket = socket;

			if (socket.BeginReceiveResult( SocketAsyncCallBack, appSession ).IsSuccess)
				AddClient( appSession );
			else
				LogNet?.WriteDebug( ToString( ), string.Format( StringResources.Language.ClientOfflineInfo, endPoint ) );

		}
#if NET20 || NET35
		private void SocketAsyncCallBack( IAsyncResult ar )
#else
		private async void SocketAsyncCallBack( IAsyncResult ar )
#endif
		{
			if (ar.AsyncState is AppSession session)
			{
				if (!session.WorkSocket.EndReceiveResult( ar ).IsSuccess) { RemoveClient( session ); return; }

#if NET20 || NET35
				OperateResult<byte[]> read1 = ReceiveCommandLineFromSocket( session.WorkSocket, 0x0d, 2000 );
#else
				OperateResult<byte[]> read1 = await ReceiveCommandLineFromSocketAsync( session.WorkSocket, 0x0d, 2000 );
#endif
				if (!read1.IsSuccess) { RemoveClient( session ); return; };

				if (!Authorization.asdniasnfaksndiqwhawfskhfaiw( )) { RemoveClient( session ); return; };

				LogNet?.WriteDebug( ToString( ), $"[{session.IpEndPoint}] Tcp {StringResources.Language.Receive}：{Encoding.ASCII.GetString(read1.Content.RemoveLast(1))}" );

				byte[] back = PanasonicHelper.PackPanasonicCommand( station, ReadFromCommand( read1.Content ) ).Content;

				if (back == null) { RemoveClient( session ); return; }
				if (!Send( session.WorkSocket, back ).IsSuccess) { RemoveClient( session ); return; }

				LogNet?.WriteDebug( ToString( ), $"[{session.IpEndPoint}] Tcp {StringResources.Language.Send}：{Encoding.ASCII.GetString( back.RemoveLast( 1 ) )}" );

				session.HeartTime = DateTime.Now;
				RaiseDataReceived( session, read1.Content );
				if (!session.WorkSocket.BeginReceiveResult( SocketAsyncCallBack, session ).IsSuccess) RemoveClient( session );
			}
		}

		#endregion

		#region Function Process Center

		/// <summary>
		/// 创建一个失败的返回消息，指定错误码即可，会自动计算出来BCC校验和
		/// </summary>
		/// <param name="code">错误码</param>
		/// <returns>原始字节报文，用于反馈消息</returns>
		protected string CreateFailedResponse( byte code ) => "!" + code.ToString( "D2" );

		/// <summary>
		/// 根据命令来获取相关的数据内容
		/// </summary>
		/// <param name="cmd"></param>
		/// <returns></returns>
		public virtual string ReadFromCommand( byte[] cmd )
		{
			try
			{
				string strCommand = Encoding.ASCII.GetString( cmd );
				if (strCommand[0] != '%') return CreateFailedResponse( 41 );
				byte stat = Convert.ToByte( strCommand.Substring( 1, 2 ), 16 );
				if (stat != station)
				{
					LogNet?.WriteError( ToString( ), $"Station not match, need:{station}, but now: {stat}" );
					return CreateFailedResponse( 50 );
				}
				if (strCommand[3] != '#') return CreateFailedResponse( 41 );
				if (strCommand.Substring( 4, 3 ) == "RCS")
				{
					// 读取单个的bool的值
					if(strCommand[7] == 'R')
					{
						int bitIndex = Convert.ToInt32( strCommand.Substring( 8, 3 ) ) * 16 + Convert.ToInt32( strCommand.Substring( 11, 1 ), 16 );
						bool value = rBuffer.GetBool( bitIndex );
						return "$RC" + (value ? "1" : "0");
					}
				}
				else if (strCommand.Substring( 4, 3 ) == "WCS")
				{
					// 写入单个的bool的值
					if (strCommand[7] == 'R')
					{
						int bitIndex = Convert.ToInt32( strCommand.Substring( 8, 3 ) ) * 16 + Convert.ToInt32( strCommand.Substring( 11, 1 ), 16 );
						rBuffer.SetBool( strCommand[12] == '1', bitIndex );
						return "$WC";
					}
				}
				if (strCommand.Substring( 4, 3 ) == "RCC")
				{
					if (strCommand[7] == 'R')
					{
						int addressStart = Convert.ToInt32( strCommand.Substring( 8, 4 ) );
						int addressEnd = Convert.ToInt32( strCommand.Substring( 12, 4 ) );
						int length = addressEnd - addressStart + 1;
						byte[] buffer = rBuffer.GetBytes( addressStart * 2, length * 2 );
						return "$RC" + buffer.ToHexString( );
					}
				}
				else if (strCommand.Substring( 4, 3 ) == "WCC")
				{
					// 写入单个的bool的值
					if (strCommand[7] == 'R')
					{
						int addressStart = Convert.ToInt32( strCommand.Substring( 8, 4 ) );
						int addressEnd = Convert.ToInt32( strCommand.Substring( 12, 4 ) );
						int length = addressEnd - addressStart + 1;
						byte[] buffer = strCommand.Substring( 16, length * 4 ).ToHexBytes( );
						rBuffer.SetBytes( buffer, addressStart * 2 );
						return "$WC";
					}
				}

				if (strCommand.Substring( 4, 2 ) == "RD")
				{
					if (strCommand[6] == 'D')
					{
						int addressStart = Convert.ToInt32( strCommand.Substring( 7, 5 ) );
						int addressEnd   = Convert.ToInt32( strCommand.Substring( 12, 5 ) );
						int length       = addressEnd - addressStart + 1;
						byte[] buffer    = dBuffer.GetBytes( addressStart * 2, length * 2 );
						return "$RD" + buffer.ToHexString( );
					}
				}
				else if (strCommand.Substring( 4, 2 ) == "WD")
				{
					// 写入单个的bool的值
					if (strCommand[6] == 'D')
					{
						int addressStart = Convert.ToInt32( strCommand.Substring( 7, 5 ) );
						int addressEnd   = Convert.ToInt32( strCommand.Substring( 12, 5 ) );
						int length       = addressEnd - addressStart + 1;
						byte[] buffer    = strCommand.Substring( 17, length * 4 ).ToHexBytes( );
						dBuffer.SetBytes( buffer, addressStart * 2 );
						return "$WD";
					}
				}
				return CreateFailedResponse( 41 );
			}
			catch
			{
				return CreateFailedResponse( 41 );
			}
		}

		#endregion

		#region Serial Support

		private SerialPort serialPort;            // 核心的串口对象

		/// <summary>
		/// 启动modbus-rtu的从机服务，使用默认的参数进行初始化串口，9600波特率，8位数据位，无奇偶校验，1位停止位<br />
		/// Start the slave service of modbus-rtu, initialize the serial port with default parameters, 9600 baud rate, 8 data bits, no parity, 1 stop bit
		/// </summary>
		/// <param name="com">串口信息</param>
		public void StartModbusRtu( string com ) => StartModbusRtu( com, 9600 );

		/// <summary>
		/// 启动modbus-rtu的从机服务，使用默认的参数进行初始化串口，8位数据位，无奇偶校验，1位停止位<br />
		/// Start the slave service of modbus-rtu, initialize the serial port with default parameters, 8 data bits, no parity, 1 stop bit
		/// </summary>
		/// <param name="com">串口信息</param>
		/// <param name="baudRate">波特率</param>
		public void StartModbusRtu( string com, int baudRate )
		{
			StartModbusRtu( sp =>
			{
				sp.PortName = com;
				sp.BaudRate = baudRate;
				sp.DataBits = 8;
				sp.Parity = Parity.None;
				sp.StopBits = StopBits.One;
			} );
		}

		/// <summary>
		/// 启动modbus-rtu的从机服务，使用自定义的初始化方法初始化串口的参数<br />
		/// Start the slave service of modbus-rtu and initialize the parameters of the serial port using a custom initialization method
		/// </summary>
		/// <param name="inni">初始化信息的委托</param>
		public void StartModbusRtu( Action<SerialPort> inni )
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
		/// 关闭modbus-rtu的串口对象<br />
		/// Close the serial port object of modbus-rtu
		/// </summary>
		public void CloseModbusRtu( )
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
				if (count == 0) break;
			}

			if (rCount == 0) return;
			byte[] receive = buffer.SelectBegin( rCount );
			if (receive.Length < 5)
			{
				LogNet?.WriteError( ToString( ), $"[{serialPort.PortName}] Uknown Data：{receive.ToHexString( ' ' )}" ); return;
			}

			LogNet?.WriteDebug( ToString( ), $"[{serialPort.PortName}] Rtu {StringResources.Language.Receive}：{receive.ToHexString( ' ' )}" );

			// 需要回发消息
			byte[] back = PanasonicHelper.PackPanasonicCommand( station, ReadFromCommand( receive ) ).Content;

			serialPort.Write( back, 0, back.Length );

			LogNet?.WriteDebug( ToString( ), $"[{serialPort.PortName}] Rtu {StringResources.Language.Send}：{back.ToHexString( ' ' )}" );
			if (IsStarted) RaiseDataReceived( sender, receive );
		}

		#endregion

		#region IDisposable Support

		/// <inheritdoc/>
		protected override void Dispose( bool disposing )
		{
			if (disposing)
			{
				rBuffer?.Dispose( );
				dBuffer?.Dispose( );
				serialPort?.Dispose( );
			}
			base.Dispose( disposing );
		}

		#endregion

		#region Private Member

		private SoftBuffer xBuffer;                // 输入线圈的数据池
		private SoftBuffer rBuffer;                // 线圈的数据池
		private SoftBuffer dBuffer;                // 数据的数据池
		private SoftBuffer lBuffer;                // L寄存器数据池
		private SoftBuffer fBuffer;                // F寄存器数据池

		private const int DataPoolLength = 65536;     // 数据的长度
		private byte station = 1;                     // 服务器的站号数据

		#endregion

		#region Object Override

		/// <inheritdoc/>
		public override string ToString( ) => $"PanasonicMewtocolServer[{Port}]";

		#endregion

	}
}
