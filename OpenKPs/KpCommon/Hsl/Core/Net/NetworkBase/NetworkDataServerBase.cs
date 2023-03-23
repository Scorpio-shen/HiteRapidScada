using HslCommunication.BasicFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using HslCommunication.Core.Types;
using System.Net;
using HslCommunication.Reflection;
using System.IO.Ports;
using HslCommunication.Core.IMessage;
#if !NET35 && !NET20
using System.Threading.Tasks;
#endif

namespace HslCommunication.Core.Net
{
	/// <summary>
	/// 所有虚拟的数据服务器的基类，提供了基本的数据读写，存储加载的功能方法，具体的字节读写需要继承重写。<br />
	/// The base class of all virtual data servers provides basic methods for reading and writing data and storing and loading. 
	/// Specific byte reads and writes need to be inherited and override.
	/// </summary>
	public class NetworkDataServerBase : NetworkAuthenticationServerBase, IDisposable, IReadWriteNet
	{
		#region Constructor

		/// <summary>
		/// 实例化一个默认的数据服务器的对象<br />
		/// Instantiate an object of the default data server
		/// </summary>
		public NetworkDataServerBase( )
		{
			ConnectionId          = SoftBasic.GetUniqueStringByGuidAndRandom( );
			serialPort            = new SerialPort( );   // 实例化串口对象
		}

		#endregion

		#region File Load Save

		/// <summary>
		/// 将本系统的数据池数据存储到指定的文件<br />
		/// Store the data pool data of this system to the specified file
		/// </summary>
		/// <param name="path">指定文件的路径</param>
		/// <exception cref="ArgumentException"></exception>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="System.IO.PathTooLongException"></exception>
		/// <exception cref="System.IO.DirectoryNotFoundException"></exception>
		/// <exception cref="System.IO.IOException"></exception>
		/// <exception cref="UnauthorizedAccessException"></exception>
		/// <exception cref="NotSupportedException"></exception>
		/// <exception cref="System.Security.SecurityException"></exception>
		public void SaveDataPool( string path )
		{
			byte[] content = SaveToBytes( );
			System.IO.File.WriteAllBytes( path, content );
		}

		/// <summary>
		/// 从文件加载数据池信息<br />
		/// Load datapool information from a file
		/// </summary>
		/// <param name="path">文件路径</param>
		/// <exception cref="ArgumentException"></exception>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="System.IO.PathTooLongException"></exception>
		/// <exception cref="System.IO.DirectoryNotFoundException"></exception>
		/// <exception cref="System.IO.IOException"></exception>
		/// <exception cref="UnauthorizedAccessException"></exception>
		/// <exception cref="NotSupportedException"></exception>
		/// <exception cref="System.Security.SecurityException"></exception>
		public void LoadDataPool( string path )
		{
			if (System.IO.File.Exists( path ))
			{
				byte[] buffer = System.IO.File.ReadAllBytes( path );
				LoadFromBytes( buffer );
			}
		}

		/// <summary>
		/// 从字节数据加载数据信息，需要进行重写方法<br />
		/// Loading data information from byte data requires rewriting method
		/// </summary>
		/// <param name="content">字节数据</param>
		protected virtual void LoadFromBytes( byte[] content ) { }

		/// <summary>
		/// 将数据信息存储到字节数组去，需要进行重写方法<br />
		/// To store data information into a byte array, a rewrite method is required
		/// </summary>
		/// <returns>所有的内容</returns>
		protected virtual byte[] SaveToBytes( ) => new byte[0];

		#endregion

		#region Public Members

		/// <inheritdoc cref="NetworkDoubleBase.ByteTransform"/>
		public IByteTransform ByteTransform { get; set; }

		/// <inheritdoc cref="IReadWriteNet.ConnectionId"/>
		public string ConnectionId { get; set; }

        /// <summary>
        /// 获取或设置当前的服务器接收串口数据时候，是否强制只接收一次数据，默认为false，适合点对点通信，如果你总线形式的连接，则需要设置 True<br />
        /// Get or set whether to force the data to be received only once when the current server receives serial port data. The default value is false, 
		/// which is suitable for point-to-point communication. If you have a bus connection, you need to set True
        /// </summary>
        public bool ForceSerialReceiveOnce { get=> this.forceReceiveOnce; set => this.forceReceiveOnce = value; }

		/// <summary>
		/// 获取或设置当前的服务器是否允许远程客户端进行写入数据操作，默认为<c>True</c><br />
		/// Gets or sets whether the current server allows remote clients to write data, the default is <c>True</c>
		/// </summary>
		/// <remarks>
		/// 如果设置为<c>False</c>，那么所有远程客户端的操作都会失败，直接返回错误码或是关闭连接。
		/// </remarks>
		public bool EnableWrite { get; set; } = true;

		/// <summary>
		/// 当接收到来自客户的数据信息时触发的对象，该数据可能来自tcp或是串口<br />
		/// The object that is triggered when receiving data information from the customer, the data may come from tcp or serial port
		/// </summary>
		/// <param name="sender">触发的服务器对象</param>
		/// <param name="source">消息的来源对象</param>
		/// <param name="data">实际的数据信息</param>
		public delegate void DataReceivedDelegate( object sender, object source, byte[] data );

		/// <summary>
		/// 接收到数据的时候就触发的事件，示例详细参考API文档信息<br />
		/// An event that is triggered when data is received
		/// </summary>
		/// <remarks>
		/// 事件共有三个参数，sender指服务器本地的对象，例如 <see cref="ModBus.ModbusTcpServer"/> 对象，source 指会话对象，网口对象为 <see cref="AppSession"/>，
		/// 串口为<see cref="System.IO.Ports.SerialPort"/> 对象，需要根据实际判断，data 为收到的原始数据 byte[] 对象
		/// </remarks>
		/// <example>
		/// 我们以Modbus的Server为例子，其他的虚拟服务器同理，因为都集成自本服务器对象
		/// <code lang="cs" source="HslCommunication_Net45.Test\Documentation\Samples\Core\NetworkDataServerBaseSample.cs" region="OnDataReceivedSample" title="数据接收触发的示例" />
		/// </example>
		public event DataReceivedDelegate OnDataReceived;

		/// <summary>
		/// 触发一个数据接收的事件信息<br />
		/// Event information that triggers a data reception
		/// </summary>
		/// <param name="source">数据的发送方</param>
		/// <param name="receive">接收数据信息</param>
		protected void RaiseDataReceived( object source, byte[] receive ) => OnDataReceived?.Invoke( this, source, receive );

		/// <summary>
		/// 数据发送的时候委托<br />
		/// Show DataSend To PLC
		/// </summary>
		/// <param name="sender">数据发送对象</param>
		/// <param name="data">数据内容</param>
		public delegate void DataSendDelegate( object sender, byte[] data );

		/// <summary>
		/// 数据发送的时候就触发的事件<br />
		/// Events that are triggered when data is sent
		/// </summary>
		public event DataSendDelegate OnDataSend;

		/// <summary>
		/// 触发一个数据发送的事件信息<br />
		/// Event information that triggers a data transmission
		/// </summary>
		/// <param name="send">数据内容</param>
		protected void RaiseDataSend( byte[] send ) => OnDataSend?.Invoke( this, send );

		/// <summary>
		/// 获取串口模式下消息的日志记录方式，可以继承重写。<br />
		/// Get the logging method of messages in serial mode, which can be inherited and rewritten.
		/// </summary>
		/// <param name="data">原始数据</param>
		/// <returns>消息</returns>
		protected virtual string GetSerialMessageLogText( byte[] data )
		{
			return LogMsgFormatBinary ? SoftBasic.ByteToHexString( data, ' ' ) : SoftBasic.GetAsciiStringRender( data );
		}

		#endregion

		#region Protect Member

		/// <inheritdoc cref="NetworkDeviceBase.WordLength"/>
		protected ushort WordLength { get; set; } = 1;

		/// <inheritdoc cref="NetworkDoubleBase.LogMsgFormatBinary"/>
		protected bool LogMsgFormatBinary = true;

		/// <inheritdoc cref="NetworkDeviceBase.GetWordLength(string, int, int)"/>
		protected virtual ushort GetWordLength( string address, int length, int dataTypeLength )
		{
			if (WordLength == 0)
			{
				int len = length * dataTypeLength * 2 / 4;
				return len == 0 ? (ushort)1 : (ushort)len;
			}
			return (ushort)(WordLength * length * dataTypeLength);
		}

		/// <inheritdoc cref="NetworkDoubleBase.GetNewNetMessage"/>
		protected virtual INetMessage GetNewNetMessage( ) => null;

		/// <inheritdoc cref="NetworkDoubleBase.ReadFromCoreServer(byte[])"/>
		protected virtual OperateResult<byte[]> ReadFromCoreServer( AppSession session, byte[] receive ) => new OperateResult<byte[]>( StringResources.Language.NotSupportedFunction );

		#endregion

		#region NetworkAuthenticationServerBase Override

		/// <summary>
		/// 从远程Socket异步接收的数据信息
		/// </summary>
		/// <param name="ar">异步接收的对象</param>
#if NET20 || NET35
		protected override void SocketAsyncCallBack( IAsyncResult ar )
#else
		protected override async void SocketAsyncCallBack( IAsyncResult ar )
#endif
		{
			if (ar.AsyncState is AppSession session)
			{
				try
				{
					int receiveCount = session.WorkSocket.EndReceive( ar );
					OperateResult<byte[]> read1;

					if (!Authorization.asdniasnfaksndiqwhawfskhfaiw( )) { RemoveClient( session ); return; };
#if NET20 || NET35
					read1 = ReceiveByMessage( session.WorkSocket, 5000, GetNewNetMessage( ) );
#else
					read1 = await ReceiveByMessageAsync( session.WorkSocket, 5000, GetNewNetMessage( ) );
#endif
					if (!read1.IsSuccess) { RemoveClient( session ); return; };

					LogNet?.WriteDebug( ToString( ), $"[{session.IpEndPoint}] Tcp {StringResources.Language.Receive}：{(LogMsgFormatBinary ? SoftBasic.ByteToHexString( read1.Content, ' ' ) : SoftBasic.GetAsciiStringRender( read1.Content ))}" );

					OperateResult<byte[]> read = ReadFromCoreServer( session, read1.Content );
					if (!read.IsSuccess)
					{
						LogNet?.WriteDebug( ToString( ), $"[{session.IpEndPoint}] Tcp {read.Message}" );
						if (read.Content != null && read.Content.Length > 0) read.IsSuccess = true;
					}

					if (read.IsSuccess)
					{
						if (read.Content != null)
						{
							session.WorkSocket.Send( read.Content );
							LogNet?.WriteDebug( ToString( ), $"[{session.IpEndPoint}] Tcp {StringResources.Language.Send}：{(LogMsgFormatBinary ? SoftBasic.ByteToHexString( read.Content, ' ' ) : SoftBasic.GetAsciiStringRender( read.Content ))}" );
						}
						else
						{
							RemoveClient( session );
							return;
						}

					}
					session.UpdateHeartTime( );
					RaiseDataReceived( session, read1.Content );
					session.WorkSocket.BeginReceive( new byte[0], 0, 0, SocketFlags.None, new AsyncCallback( SocketAsyncCallBack ), session );
				}
				catch (Exception ex)
				{
					RemoveClient( session, $"SocketAsyncCallBack Exception -> {ex.Message}" );
				}
			}
		}

		#endregion

		#region Serial Support

		/// <summary>
		/// 获取或设置串口模式下，接收一条数据最短的时间要求，当设备发送的数据非常慢的时候，或是分割发送数据的时候，就需要将本值设置的大一点，默认为20ms<br />
		/// Get or set the shortest time required to receive a piece of data in serial port mode. 
		/// When the data sent by the device is very slow, or when the data is divided and sent, you need to set this value to a larger value, the default is 20ms
		/// </summary>
		public int SerialReceiveAtleastTime
		{
			get => this.receiveAtleastTime;
			set => this.receiveAtleastTime = value;
		}

		/// <summary>
		/// 启动串口的从机服务，使用默认的参数进行初始化串口，9600波特率，8位数据位，无奇偶校验，1位停止位<br />
		/// Start the slave service of serial, initialize the serial port with default parameters, 9600 baud rate, 8 data bits, no parity, 1 stop bit
		/// </summary>
		/// <remarks>
		/// com支持格式化的方式，例如输入 COM3-9600-8-N-1，COM5-19200-7-E-2，其中奇偶校验的字母可选，N:无校验，O：奇校验，E:偶校验，停止位可选 0, 1, 2, 1.5 四种选项
		/// </remarks>
		/// <param name="com">串口信息</param>
		public void StartSerialSlave( string com )
		{
			if (com.Contains( "-" ) || com.Contains( ";" ))
				StartSerialSlave( sp =>
				{
					sp.IniSerialByFormatString( com );
				} );
			else
				StartSerialSlave( com, 9600 );
		}

		/// <summary>
		/// 启动串口的从机服务，使用默认的参数进行初始化串口，8位数据位，无奇偶校验，1位停止位<br />
		/// Start the slave service of serial, initialize the serial port with default parameters, 8 data bits, no parity, 1 stop bit
		/// </summary>
		/// <param name="com">串口信息</param>
		/// <param name="baudRate">波特率</param>
		public void StartSerialSlave( string com, int baudRate )
		{
			StartSerialSlave( sp =>
			{
				sp.PortName = com;
				sp.BaudRate = baudRate;
				sp.DataBits = 8;
				sp.Parity   = Parity.None;
				sp.StopBits = StopBits.One;
			} );
		}

		/// <summary>
		/// 启动串口的从机服务，使用指定的参数进行初始化串口，指定数据位，指定奇偶校验，指定停止位<br />
		/// </summary>
		/// <param name="com">串口信息</param>
		/// <param name="baudRate">波特率</param>
		/// <param name="dataBits">数据位</param>
		/// <param name="parity">奇偶校验</param>
		/// <param name="stopBits">停止位</param>
		public void StartSerialSlave( string com, int baudRate, int dataBits, Parity parity, StopBits stopBits )
		{
			StartSerialSlave( sp =>
			{
				sp.PortName = com;
				sp.BaudRate = baudRate;
				sp.DataBits = dataBits;
				sp.Parity   = parity;
				sp.StopBits = stopBits;
			} );
		}

		/// <summary>
		/// 启动串口的从机服务，使用自定义的初始化方法初始化串口的参数<br />
		/// Start the slave service of serial and initialize the parameters of the serial port using a custom initialization method
		/// </summary>
		/// <param name="inni">初始化信息的委托</param>
		public void StartSerialSlave( Action<SerialPort> inni )
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
		/// 关闭提供从机服务的串口对象<br />
		/// Close the serial port object that provides slave services
		/// </summary>
		public void CloseSerialSlave( )
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
			if (!Authorization.asdniasnfaksndiqwhawfskhfaiw( )) { return; };
			int rCount   = 0;
			int rTick    = 0;
			byte[] buffer = new byte[2048];
			DateTime start = DateTime.Now;
			while (true)
			{
				// 在总线模式的server连接下，其他站号不匹配的服务器，不应该响应数据，但是此处会接收所有的读写数据，引发超出buffer的异常报错
				try
				{
					int count = serialPort.Read( buffer, rCount, serialPort.BytesToRead );
					// 接收不到数据之后，并且达到超时才算真的
					if (count == 0 && rTick != 0 && (DateTime.Now - start).TotalMilliseconds >= this.receiveAtleastTime) break;
					rCount += count;
					rTick++;
				}
				catch(Exception ex)
				{
					// 可能会超出缓存的限制，从而引发异常
					LogNet?.WriteError( ToString( ), $"SerialPort_DataReceived Error: " + ex.Message );
					break;
				}

				// 检查数据是否完整，完整的话，直接进行返回
				if (forceReceiveOnce && rCount > 0) break;
				if (CheckSerialReceiveDataComplete( buffer, rCount )) break;
				System.Threading.Thread.Sleep( 20 );            // 此处做个微小的延时，等待数据接收完成
			}

			if (rCount == 0) return;
			try
			{
				byte[] receive = buffer.SelectBegin( rCount ); // [{(int)(DateTime.Now - start).TotalMilliseconds} ms]
				LogNet?.WriteDebug( ToString( ), $"[{GetSerialPort( ).PortName}] {StringResources.Language.Receive}：{GetSerialMessageLogText( receive )}" );
				OperateResult<byte[]> dealResult = DealWithSerialReceivedData( receive );

				if (dealResult.IsSuccess)
				{
					if (dealResult.Content != null)
					{
						this.serialPort.Write( dealResult.Content, 0, dealResult.Content.Length );
						if (IsStarted) RaiseDataReceived( sender, dealResult.Content );
						LogNet?.WriteDebug( ToString( ), $"[{GetSerialPort( ).PortName}] {StringResources.Language.Send}：{GetSerialMessageLogText( dealResult.Content )}" );
					}
				}
				else
				{
					LogNet?.WriteError( ToString( ), $"[{GetSerialPort( ).PortName}] " + dealResult.Message );
				}
			}
			catch(Exception ex)
			{
				LogNet?.WriteException( ToString( ), ex );
			}
		}

		/// <summary>
		/// 检查串口接收的数据是否完成的方法，如果接收完成，则返回<c>True</c>
		/// </summary>
		/// <param name="buffer">缓存的数据信息</param>
		/// <param name="receivedLength">当前已经接收的数据长度信息</param>
		/// <returns>是否接收完成</returns>
		protected virtual bool CheckSerialReceiveDataComplete(byte[] buffer, int receivedLength )
		{
			return false;
		}

		/// <summary>
		/// 处理串口接收数据的功能方法，需要在继承类中进行相关的重写操作
		/// </summary>
		/// <param name="data">串口接收到的原始字节数据</param>
		protected virtual OperateResult<byte[]> DealWithSerialReceivedData( byte[] data )
		{
			return ReadFromCoreServer( null, data );
		}

		/// <summary>
		/// 获取当前的串口对象信息
		/// </summary>
		/// <returns>串口对象</returns>
		protected SerialPort GetSerialPort( ) => this.serialPort;

		#endregion

		#region IDisposable Support

		/// <summary>
		/// 释放当前的对象
		/// </summary>
		/// <param name="disposing">是否托管对象</param>
		protected override void Dispose( bool disposing )
		{
			if (disposing)
			{
				this.OnDataSend = null;
				this.OnDataReceived = null;
				this.serialPort?.Dispose( );
			}
			base.Dispose( disposing );
		}

		#endregion

		#region Private Member

		/// <summary>
		/// 接收一次的数据的最少时间，当重写了报文结束的检查代码时，可以适当的将本值设置的大一点。<br />
		/// The minimum time to receive the data once, when the check code of the end of the message is rewritten, this value can be appropriately set larger.
		/// </summary>
		private int receiveAtleastTime = 20;
		private SerialPort serialPort;                                         // 核心的串口对象
		private bool forceReceiveOnce = false;                                 // 强制接收一次数据及完成

		#endregion

		#region Object Override

		/// <inheritdoc/>
		public override string ToString( ) => $"NetworkDataServerBase[{Port}]";

		#endregion

		// 以下内容和 NetworkDoubleBase 相同，因为不存在多继承，所以暂时选择了复制代码

		#region Read Write Bytes Bool

		/**************************************************************************************************
		 * 
		 *    说明：子类中需要重写基础的读取和写入方法，来支持不同的数据访问规则
		 *    
		 *    此处没有将读写位纳入进来，因为各种设备的支持不尽相同，比较麻烦
		 * 
		 **************************************************************************************************/

		/// <inheritdoc cref="IReadWriteNet.Read(string, ushort)"/>
		[HslMqttApi( "ReadByteArray", "" )]
		public virtual OperateResult<byte[]> Read( string address, ushort length ) => new OperateResult<byte[]>( StringResources.Language.NotSupportedFunction );

		/// <inheritdoc cref="IReadWriteNet.Write(string, byte[])"/>
		[HslMqttApi( "WriteByteArray", "" )]
		public virtual OperateResult Write( string address, byte[] value ) => new OperateResult( StringResources.Language.NotSupportedFunction );

		/*************************************************************************************************************
		 * 
		 * Bool类型的读写，不一定所有的设备都实现，比如西门子，就没有实现bool[]的读写，Siemens的fetch/write没有实现bool操作
		 * 
		 ************************************************************************************************************/

		/// <inheritdoc cref="IReadWriteNet.ReadBool(string, ushort)"/>
		[HslMqttApi( "ReadBoolArray", "" )]
		public virtual OperateResult<bool[]> ReadBool( string address, ushort length ) => new OperateResult<bool[]>( StringResources.Language.NotSupportedFunction );

		/// <inheritdoc cref="IReadWriteNet.ReadBool(string)"/>
		[HslMqttApi( "ReadBool", "" )]
		public virtual OperateResult<bool> ReadBool( string address ) => ByteTransformHelper.GetResultFromArray( ReadBool( address, 1 ) );

		/// <inheritdoc cref="IReadWriteNet.Write(string, bool[])"/>
		[HslMqttApi( "WriteBoolArray", "" )]
		public virtual OperateResult Write( string address, bool[] value ) => new OperateResult( StringResources.Language.NotSupportedFunction );

		/// <inheritdoc cref="IReadWriteNet.Write(string, bool)"/>
		[HslMqttApi( "WriteBool", "" )]
		public virtual OperateResult Write( string address, bool value ) => Write( address, new bool[] { value } );

		#endregion

		#region Customer Read Write

		/// <inheritdoc cref="IReadWriteNet.ReadCustomer{T}(string)"/>
		public OperateResult<T> ReadCustomer<T>( string address ) where T : IDataTransfer, new() => ReadWriteNetHelper.ReadCustomer<T>( this, address );

		/// <inheritdoc cref="IReadWriteNet.ReadCustomer{T}(string, T)"/>
		public OperateResult<T> ReadCustomer<T>( string address, T obj ) where T : IDataTransfer, new() => ReadWriteNetHelper.ReadCustomer( this, address, obj );

		/// <inheritdoc cref="IReadWriteNet.WriteCustomer{T}(string, T)"/>
		public OperateResult WriteCustomer<T>( string address, T data ) where T : IDataTransfer, new() => ReadWriteNetHelper.WriteCustomer( this, address, data );

		#endregion

		#region Reflection Read Write

		/// <inheritdoc cref="IReadWriteNet.Read{T}"/>
		public virtual OperateResult<T> Read<T>( ) where T : class, new() => HslReflectionHelper.Read<T>( this );

		/// <inheritdoc cref="IReadWriteNet.Write{T}(T)"/>
		public virtual OperateResult Write<T>( T data ) where T : class, new() => HslReflectionHelper.Write<T>( data, this );

		/// <inheritdoc cref="IReadWriteNet.ReadStruct{T}(string, ushort)"/>
		public virtual OperateResult<T> ReadStruct<T>( string address, ushort length ) where T : class, new() => ReadWriteNetHelper.ReadStruct<T>( this, address, length, this.ByteTransform );

		#endregion

		#region Read Support

		/// <inheritdoc cref="IReadWriteNet.ReadInt16(string)"/>
		[HslMqttApi( "ReadInt16", "" )]
		public OperateResult<short> ReadInt16( string address ) => ByteTransformHelper.GetResultFromArray( ReadInt16( address, 1 ) );

		/// <inheritdoc cref="IReadWriteNet.ReadInt16(string, ushort)"/>
		[HslMqttApi( "ReadInt16Array", "" )]
		public virtual OperateResult<short[]> ReadInt16( string address, ushort length ) => ByteTransformHelper.GetResultFromBytes( Read( address, GetWordLength( address, length, 1 ) ), m => ByteTransform.TransInt16( m, 0, length ) );

		/// <inheritdoc cref="IReadWriteNet.ReadUInt16(string)"/>
		[HslMqttApi( "ReadUInt16", "" )]
		public OperateResult<ushort> ReadUInt16( string address ) => ByteTransformHelper.GetResultFromArray( ReadUInt16( address, 1 ) );

		/// <inheritdoc cref="IReadWriteNet.ReadUInt16(string, ushort)"/>
		[HslMqttApi( "ReadUInt16Array", "" )]
		public virtual OperateResult<ushort[]> ReadUInt16( string address, ushort length ) => ByteTransformHelper.GetResultFromBytes( Read( address, GetWordLength( address, length, 1 ) ), m => ByteTransform.TransUInt16( m, 0, length ) );

		/// <inheritdoc cref="IReadWriteNet.ReadInt32(string)"/>
		[HslMqttApi( "ReadInt32", "" )]
		public OperateResult<int> ReadInt32( string address ) => ByteTransformHelper.GetResultFromArray( ReadInt32( address, 1 ) );

		/// <inheritdoc cref="IReadWriteNet.ReadInt32(string, ushort)"/>
		[HslMqttApi( "ReadInt32Array", "" )]
		public virtual OperateResult<int[]> ReadInt32( string address, ushort length ) => ByteTransformHelper.GetResultFromBytes( Read( address, GetWordLength( address, length, 2 ) ), m => ByteTransform.TransInt32( m, 0, length ) );

		/// <inheritdoc cref="IReadWriteNet.ReadUInt32(string)"/>
		[HslMqttApi( "ReadUInt32", "" )]
		public OperateResult<uint> ReadUInt32( string address ) => ByteTransformHelper.GetResultFromArray( ReadUInt32( address, 1 ) );

		/// <inheritdoc cref="IReadWriteNet.ReadUInt32(string, ushort)"/>
		[HslMqttApi( "ReadUInt32Array", "" )]
		public virtual OperateResult<uint[]> ReadUInt32( string address, ushort length ) => ByteTransformHelper.GetResultFromBytes( Read( address, GetWordLength( address, length, 2 ) ), m => ByteTransform.TransUInt32( m, 0, length ) );

		/// <inheritdoc cref="IReadWriteNet.ReadFloat(string)"/>
		[HslMqttApi( "ReadFloat", "" )]
		public OperateResult<float> ReadFloat( string address ) => ByteTransformHelper.GetResultFromArray( ReadFloat( address, 1 ) );

		/// <inheritdoc cref="IReadWriteNet.ReadFloat(string, ushort)"/>
		[HslMqttApi( "ReadFloatArray", "" )]
		public virtual OperateResult<float[]> ReadFloat( string address, ushort length ) => ByteTransformHelper.GetResultFromBytes( Read( address, GetWordLength( address, length, 2 ) ), m => ByteTransform.TransSingle( m, 0, length ) );

		/// <inheritdoc cref="IReadWriteNet.ReadInt64(string)"/>
		[HslMqttApi( "ReadInt64", "" )]
		public OperateResult<long> ReadInt64( string address ) => ByteTransformHelper.GetResultFromArray( ReadInt64( address, 1 ) );

		/// <inheritdoc cref="IReadWriteNet.ReadInt64(string, ushort)"/>
		[HslMqttApi( "ReadInt64Array", "" )]
		public virtual OperateResult<long[]> ReadInt64( string address, ushort length ) => ByteTransformHelper.GetResultFromBytes( Read( address, GetWordLength( address, length, 4 ) ), m => ByteTransform.TransInt64( m, 0, length ) );

		/// <inheritdoc cref="IReadWriteNet.ReadUInt64(string)"/>
		[HslMqttApi( "ReadUInt64", "" )]
		public OperateResult<ulong> ReadUInt64( string address ) => ByteTransformHelper.GetResultFromArray( ReadUInt64( address, 1 ) );

		/// <inheritdoc cref="IReadWriteNet.ReadUInt64(string, ushort)"/>
		[HslMqttApi( "ReadUInt64Array", "" )]
		public virtual OperateResult<ulong[]> ReadUInt64( string address, ushort length ) => ByteTransformHelper.GetResultFromBytes( Read( address, GetWordLength( address, length, 4 ) ), m => ByteTransform.TransUInt64( m, 0, length ) );

		/// <inheritdoc cref="IReadWriteNet.ReadDouble(string)"/>
		[HslMqttApi( "ReadDouble", "" )]
		public OperateResult<double> ReadDouble( string address ) => ByteTransformHelper.GetResultFromArray( ReadDouble( address, 1 ) );

		/// <inheritdoc cref="IReadWriteNet.ReadDouble(string, ushort)"/>
		[HslMqttApi( "ReadDoubleArray", "" )]
		public virtual OperateResult<double[]> ReadDouble( string address, ushort length ) => ByteTransformHelper.GetResultFromBytes( Read( address, GetWordLength( address, length, 4 ) ), m => ByteTransform.TransDouble( m, 0, length ) );

		/// <inheritdoc cref="IReadWriteNet.ReadString(string, ushort)"/>
		[HslMqttApi( "ReadString", "" )]
		public virtual OperateResult<string> ReadString( string address, ushort length ) => ReadString( address, length, Encoding.ASCII );

		/// <inheritdoc cref="IReadWriteNet.ReadString(string, ushort, Encoding)"/>
		public virtual OperateResult<string> ReadString( string address, ushort length, Encoding encoding ) => ByteTransformHelper.GetResultFromBytes( Read( address, length ), m => ByteTransform.TransString( m, 0, m.Length, encoding ) );

		#endregion

		#region Write Support

		/// <inheritdoc cref="IReadWriteNet.Write(string, short[])"/>
		[HslMqttApi( "WriteInt16Array", "" )]
		public virtual OperateResult Write( string address, short[] values ) => Write( address, ByteTransform.TransByte( values ) );

		/// <inheritdoc cref="IReadWriteNet.Write(string, short)"/>
		[HslMqttApi( "WriteInt16", "" )]
		public virtual OperateResult Write( string address, short value ) => Write( address, new short[] { value } );

		/// <inheritdoc cref="IReadWriteNet.Write(string, ushort[])"/>
		[HslMqttApi( "WriteUInt16Array", "" )]
		public virtual OperateResult Write( string address, ushort[] values ) => Write( address, ByteTransform.TransByte( values ) );

		/// <inheritdoc cref="IReadWriteNet.Write(string, ushort)"/>
		[HslMqttApi( "WriteUInt16", "" )]
		public virtual OperateResult Write( string address, ushort value ) => Write( address, new ushort[] { value } );

		/// <inheritdoc cref="IReadWriteNet.Write(string, int[])"/>
		[HslMqttApi( "WriteInt32Array", "" )]
		public virtual OperateResult Write( string address, int[] values ) => Write( address, ByteTransform.TransByte( values ) );

		/// <inheritdoc cref="IReadWriteNet.Write(string, int)"/>
		[HslMqttApi( "WriteInt32", "" )]
		public virtual OperateResult Write( string address, int value ) => Write( address, new int[] { value } );

		/// <inheritdoc cref="IReadWriteNet.Write(string, uint[])"/>
		[HslMqttApi( "WriteUInt32Array", "" )]
		public virtual OperateResult Write( string address, uint[] values ) => Write( address, ByteTransform.TransByte( values ) );

		/// <inheritdoc cref="IReadWriteNet.Write(string, uint)"/>
		[HslMqttApi( "WriteUInt32", "" )]
		public virtual OperateResult Write( string address, uint value ) => Write( address, new uint[] { value } );

		/// <inheritdoc cref="IReadWriteNet.Write(string, float[])"/>
		[HslMqttApi( "WriteFloatArray", "" )]
		public virtual OperateResult Write( string address, float[] values ) => Write( address, ByteTransform.TransByte( values ) );

		/// <inheritdoc cref="IReadWriteNet.Write(string, float)"/>
		[HslMqttApi( "WriteFloat", "" )]
		public virtual OperateResult Write( string address, float value ) => Write( address, new float[] { value } );

		/// <inheritdoc cref="IReadWriteNet.Write(string, long[])"/>
		[HslMqttApi( "WriteInt64Array", "" )]
		public virtual OperateResult Write( string address, long[] values ) => Write( address, ByteTransform.TransByte( values ) );

		/// <inheritdoc cref="IReadWriteNet.Write(string, long)"/>
		[HslMqttApi( "WriteInt64", "" )]
		public virtual OperateResult Write( string address, long value ) => Write( address, new long[] { value } );

		/// <inheritdoc cref="IReadWriteNet.Write(string, ulong[])"/>
		[HslMqttApi( "WriteUInt64Array", "" )]
		public virtual OperateResult Write( string address, ulong[] values ) => Write( address, ByteTransform.TransByte( values ) );

		/// <inheritdoc cref="IReadWriteNet.Write(string, ulong)"/>
		[HslMqttApi( "WriteUInt64", "" )]
		public virtual OperateResult Write( string address, ulong value ) => Write( address, new ulong[] { value } );

		/// <inheritdoc cref="IReadWriteNet.Write(string, double[])"/>
		[HslMqttApi( "WriteDoubleArray", "" )]
		public virtual OperateResult Write( string address, double[] values ) => Write( address, ByteTransform.TransByte( values ) );

		/// <inheritdoc cref="IReadWriteNet.Write(string, double)"/>
		[HslMqttApi( "WriteDouble", "" )]
		public virtual OperateResult Write( string address, double value ) => Write( address, new double[] { value } );

		/// <inheritdoc cref="IReadWriteNet.Write(string, string)"/>
		[HslMqttApi( "WriteString", "" )]
		public virtual OperateResult Write( string address, string value ) => Write( address, value, Encoding.ASCII );

		/// <inheritdoc cref="IReadWriteNet.Write(string, string, int)"/>
		public virtual OperateResult Write( string address, string value, int length ) => Write( address, value, length, Encoding.ASCII );

		/// <inheritdoc cref="IReadWriteNet.Write(string, string, Encoding)"/>
		public virtual OperateResult Write( string address, string value, Encoding encoding )
		{
			byte[] temp = ByteTransform.TransByte( value, encoding );
			if (this.WordLength == 1) temp = SoftBasic.ArrayExpandToLengthEven( temp );
			return Write( address, temp );
		}

		/// <inheritdoc cref="IReadWriteNet.Write(string, string, int, Encoding)"/>
		public virtual OperateResult Write( string address, string value, int length, Encoding encoding )
		{
			byte[] temp = ByteTransform.TransByte( value, encoding );
			if (this.WordLength == 1) temp = SoftBasic.ArrayExpandToLengthEven( temp );
			temp = SoftBasic.ArrayExpandToLength( temp, length );
			return Write( address, temp );
		}

		#endregion

		#region Wait Support

		/// <inheritdoc cref="IReadWriteNet.Wait(string, bool, int, int)"/>
		[HslMqttApi( "WaitBool", "" )]
		public OperateResult<TimeSpan> Wait( string address, bool waitValue, int readInterval = 100, int waitTimeout = -1 ) => ReadWriteNetHelper.Wait( this, address, waitValue, readInterval, waitTimeout );

		/// <inheritdoc cref="IReadWriteNet.Wait(string, short, int, int)"/>
		[HslMqttApi( "WaitInt16", "" )]
		public OperateResult<TimeSpan> Wait( string address, short waitValue, int readInterval = 100, int waitTimeout = -1 ) => ReadWriteNetHelper.Wait( this, address, waitValue, readInterval, waitTimeout );

		/// <inheritdoc cref="IReadWriteNet.Wait(string, ushort, int, int)"/>
		[HslMqttApi( "WaitUInt16", "" )]
		public OperateResult<TimeSpan> Wait( string address, ushort waitValue, int readInterval = 100, int waitTimeout = -1 ) => ReadWriteNetHelper.Wait( this, address, waitValue, readInterval, waitTimeout );

		/// <inheritdoc cref="IReadWriteNet.Wait(string, int, int, int)"/>
		[HslMqttApi( "WaitInt32", "" )]
		public OperateResult<TimeSpan> Wait( string address, int waitValue, int readInterval = 100, int waitTimeout = -1 ) => ReadWriteNetHelper.Wait( this, address, waitValue, readInterval, waitTimeout );

		/// <inheritdoc cref="IReadWriteNet.Wait(string, uint, int, int)"/>
		[HslMqttApi( "WaitUInt32", "" )]
		public OperateResult<TimeSpan> Wait( string address, uint waitValue, int readInterval = 100, int waitTimeout = -1 ) => ReadWriteNetHelper.Wait( this, address, waitValue, readInterval, waitTimeout );

		/// <inheritdoc cref="IReadWriteNet.Wait(string, long, int, int)"/>
		[HslMqttApi( "WaitInt64", "" )]
		public OperateResult<TimeSpan> Wait( string address, long waitValue, int readInterval = 100, int waitTimeout = -1 ) => ReadWriteNetHelper.Wait( this, address, waitValue, readInterval, waitTimeout );

		/// <inheritdoc cref="IReadWriteNet.Wait(string, ulong, int, int)"/>
		[HslMqttApi( "WaitUInt64", "" )]
		public OperateResult<TimeSpan> Wait( string address, ulong waitValue, int readInterval = 100, int waitTimeout = -1 ) => ReadWriteNetHelper.Wait( this, address, waitValue, readInterval, waitTimeout );
#if !NET20 && !NET35

		/// <inheritdoc cref="IReadWriteNet.Wait(string, bool, int, int)"/>
		public async Task<OperateResult<TimeSpan>> WaitAsync( string address, bool waitValue, int readInterval = 100, int waitTimeout = -1 ) => await ReadWriteNetHelper.WaitAsync( this, address, waitValue, readInterval, waitTimeout );

		/// <inheritdoc cref="IReadWriteNet.Wait(string, short, int, int)"/>
		public async Task<OperateResult<TimeSpan>> WaitAsync( string address, short waitValue, int readInterval = 100, int waitTimeout = -1 ) => await ReadWriteNetHelper.WaitAsync( this, address, waitValue, readInterval, waitTimeout );

		/// <inheritdoc cref="IReadWriteNet.Wait(string, ushort, int, int)"/>
		public async Task<OperateResult<TimeSpan>> WaitAsync( string address, ushort waitValue, int readInterval = 100, int waitTimeout = -1 ) => await ReadWriteNetHelper.WaitAsync( this, address, waitValue, readInterval, waitTimeout );

		/// <inheritdoc cref="IReadWriteNet.Wait(string, int, int, int)"/>
		public async Task<OperateResult<TimeSpan>> WaitAsync( string address, int waitValue, int readInterval = 100, int waitTimeout = -1 ) => await ReadWriteNetHelper.WaitAsync( this, address, waitValue, readInterval, waitTimeout );

		/// <inheritdoc cref="IReadWriteNet.Wait(string, uint, int, int)"/>
		public async Task<OperateResult<TimeSpan>> WaitAsync( string address, uint waitValue, int readInterval = 100, int waitTimeout = -1 ) => await ReadWriteNetHelper.WaitAsync( this, address, waitValue, readInterval, waitTimeout );

		/// <inheritdoc cref="IReadWriteNet.Wait(string, long, int, int)"/>
		public async Task<OperateResult<TimeSpan>> WaitAsync( string address, long waitValue, int readInterval = 100, int waitTimeout = -1 ) => await ReadWriteNetHelper.WaitAsync( this, address, waitValue, readInterval, waitTimeout );

		/// <inheritdoc cref="IReadWriteNet.Wait(string, ulong, int, int)"/>
		public async Task<OperateResult<TimeSpan>> WaitAsync( string address, ulong waitValue, int readInterval = 100, int waitTimeout = -1 ) => await ReadWriteNetHelper.WaitAsync( this, address, waitValue, readInterval, waitTimeout );
#endif
		#endregion

		#region Asycn Read Write Bytes Bool
#if !NET35 && !NET20
		/// <inheritdoc cref="IReadWriteNet.ReadAsync(string, ushort)"/>
		public virtual async Task<OperateResult<byte[]>> ReadAsync( string address, ushort length ) => await Task.Run( ( ) => Read( address, length ) );

		/// <inheritdoc cref="IReadWriteNet.WriteAsync(string, byte[])"/>
		public virtual async Task<OperateResult> WriteAsync( string address, byte[] value ) => await Task.Run( ( ) => Write( address, value ) );


		/*************************************************************************************************************
		 * 
		 * Bool类型的读写，不一定所有的设备都实现，比如西门子，就没有实现bool[]的读写，Siemens的fetch/write没有实现bool操作
		 * 假设不进行任何的实现，那么就将使用同步代码来封装异步操作
		 * 
		 ************************************************************************************************************/

		/// <inheritdoc cref="IReadWriteNet.ReadBoolAsync(string, ushort)"/>
		public virtual async Task<OperateResult<bool[]>> ReadBoolAsync( string address, ushort length ) => await Task.Run( ( ) => ReadBool( address, length ) );

		/// <inheritdoc cref="IReadWriteNet.ReadBoolAsync(string)"/>
		public virtual async Task<OperateResult<bool>> ReadBoolAsync( string address ) => ByteTransformHelper.GetResultFromArray( await ReadBoolAsync( address, 1 ) );

		/// <inheritdoc cref="IReadWriteNet.WriteAsync(string, bool[])"/>
		public virtual async Task<OperateResult> WriteAsync( string address, bool[] value ) => await Task.Run( ( ) => Write( address, value ) );

		/// <inheritdoc cref="IReadWriteNet.WriteAsync(string, bool)"/>
		public virtual async Task<OperateResult> WriteAsync( string address, bool value ) => await WriteAsync( address, new bool[] { value } );
#endif
		#endregion

		#region Async Customer Read Write
#if !NET35 && !NET20
		/// <inheritdoc cref="IReadWriteNet.ReadCustomerAsync{T}(string)"/>
		public async Task<OperateResult<T>> ReadCustomerAsync<T>( string address ) where T : IDataTransfer, new() => await ReadWriteNetHelper.ReadCustomerAsync<T>( this, address );

		/// <inheritdoc cref="IReadWriteNet.ReadCustomerAsync{T}(string, T)"/>
		public async Task<OperateResult<T>> ReadCustomerAsync<T>( string address, T obj ) where T : IDataTransfer, new() => await ReadWriteNetHelper.ReadCustomerAsync( this, address, obj );

		/// <inheritdoc cref="IReadWriteNet.WriteCustomerAsync{T}(string, T)"/>
		public async Task<OperateResult> WriteCustomerAsync<T>( string address, T data ) where T : IDataTransfer, new() => await ReadWriteNetHelper.WriteCustomerAsync( this, address, data );
#endif
		#endregion

		#region Async Reflection Read Write
#if !NET35 && !NET20
		/// <inheritdoc cref="IReadWriteNet.ReadAsync{T}"/>
		public virtual async Task<OperateResult<T>> ReadAsync<T>( ) where T : class, new() => await HslReflectionHelper.ReadAsync<T>( this );

		/// <inheritdoc cref="IReadWriteNet.WriteAsync{T}(T)"/>
		public virtual async Task<OperateResult> WriteAsync<T>( T data ) where T : class, new() => await HslReflectionHelper.WriteAsync<T>( data, this );

		/// <inheritdoc cref="IReadWriteNet.ReadStruct{T}(string, ushort)"/>
		public virtual async Task<OperateResult<T>> ReadStructAsync<T>( string address, ushort length ) where T : class, new() => await ReadWriteNetHelper.ReadStructAsync<T>( this, address, length, this.ByteTransform );

#endif
		#endregion

		#region Async Read Support
#if !NET35 && !NET20
		/// <inheritdoc cref="IReadWriteNet.ReadInt16Async(string)"/>
		public async Task<OperateResult<short>> ReadInt16Async( string address ) => ByteTransformHelper.GetResultFromArray( await ReadInt16Async( address, 1 ) );

		/// <inheritdoc cref="IReadWriteNet.ReadInt16Async(string, ushort)"/>
		public virtual async Task<OperateResult<short[]>> ReadInt16Async( string address, ushort length ) => ByteTransformHelper.GetResultFromBytes( await ReadAsync( address, GetWordLength( address, length, 1 ) ), m => ByteTransform.TransInt16( m, 0, length ) );

		/// <inheritdoc cref="IReadWriteNet.ReadUInt16Async(string)"/>
		public async Task<OperateResult<ushort>> ReadUInt16Async( string address ) => ByteTransformHelper.GetResultFromArray( await ReadUInt16Async( address, 1 ) );

		/// <inheritdoc cref="IReadWriteNet.ReadUInt16Async(string, ushort)"/>
		public virtual async Task<OperateResult<ushort[]>> ReadUInt16Async( string address, ushort length ) => ByteTransformHelper.GetResultFromBytes( await ReadAsync( address, GetWordLength( address, length, 1 ) ), m => ByteTransform.TransUInt16( m, 0, length ) );

		/// <inheritdoc cref="IReadWriteNet.ReadInt32Async(string)"/>
		public async Task<OperateResult<int>> ReadInt32Async( string address ) => ByteTransformHelper.GetResultFromArray( await ReadInt32Async( address, 1 ) );

		/// <inheritdoc cref="IReadWriteNet.ReadInt32Async(string, ushort)"/>
		public virtual async Task<OperateResult<int[]>> ReadInt32Async( string address, ushort length ) => ByteTransformHelper.GetResultFromBytes( await ReadAsync( address, GetWordLength( address, length, 2 ) ), m => ByteTransform.TransInt32( m, 0, length ) );

		/// <inheritdoc cref="IReadWriteNet.ReadUInt32Async(string)"/>
		public async Task<OperateResult<uint>> ReadUInt32Async( string address ) => ByteTransformHelper.GetResultFromArray( await ReadUInt32Async( address, 1 ) );

		/// <inheritdoc cref="IReadWriteNet.ReadUInt32Async(string, ushort)"/>
		public virtual async Task<OperateResult<uint[]>> ReadUInt32Async( string address, ushort length ) => ByteTransformHelper.GetResultFromBytes( await ReadAsync( address, GetWordLength( address, length, 2 ) ), m => ByteTransform.TransUInt32( m, 0, length ) );

		/// <inheritdoc cref="IReadWriteNet.ReadFloatAsync(string)"/>
		public async Task<OperateResult<float>> ReadFloatAsync( string address ) => ByteTransformHelper.GetResultFromArray( await ReadFloatAsync( address, 1 ) );

		/// <inheritdoc cref="IReadWriteNet.ReadFloatAsync(string, ushort)"/>
		public virtual async Task<OperateResult<float[]>> ReadFloatAsync( string address, ushort length ) => ByteTransformHelper.GetResultFromBytes( await ReadAsync( address, GetWordLength( address, length, 2 ) ), m => ByteTransform.TransSingle( m, 0, length ) );

		/// <inheritdoc cref="IReadWriteNet.ReadInt64Async(string)"/>
		public async Task<OperateResult<long>> ReadInt64Async( string address ) => ByteTransformHelper.GetResultFromArray( await ReadInt64Async( address, 1 ) );

		/// <inheritdoc cref="IReadWriteNet.ReadInt64Async(string, ushort)"/>
		public virtual async Task<OperateResult<long[]>> ReadInt64Async( string address, ushort length ) => ByteTransformHelper.GetResultFromBytes( await ReadAsync( address, GetWordLength( address, length, 4 ) ), m => ByteTransform.TransInt64( m, 0, length ) );

		/// <inheritdoc cref="IReadWriteNet.ReadUInt64Async(string)"/>
		public async Task<OperateResult<ulong>> ReadUInt64Async( string address ) => ByteTransformHelper.GetResultFromArray( await ReadUInt64Async( address, 1 ) );

		/// <inheritdoc cref="IReadWriteNet.ReadUInt64Async(string, ushort)"/>
		public virtual async Task<OperateResult<ulong[]>> ReadUInt64Async( string address, ushort length ) => ByteTransformHelper.GetResultFromBytes( await ReadAsync( address, GetWordLength( address, length, 4 ) ), m => ByteTransform.TransUInt64( m, 0, length ) );

		/// <inheritdoc cref="IReadWriteNet.ReadDoubleAsync(string)"/>
		public async Task<OperateResult<double>> ReadDoubleAsync( string address ) => ByteTransformHelper.GetResultFromArray( await ReadDoubleAsync( address, 1 ) );

		/// <inheritdoc cref="IReadWriteNet.ReadDoubleAsync(string, ushort)"/>
		public virtual async Task<OperateResult<double[]>> ReadDoubleAsync( string address, ushort length ) => ByteTransformHelper.GetResultFromBytes( await ReadAsync( address, GetWordLength( address, length, 4 ) ), m => ByteTransform.TransDouble( m, 0, length ) );

		/// <inheritdoc cref="IReadWriteNet.ReadStringAsync(string, ushort)"/>
		public virtual async Task<OperateResult<string>> ReadStringAsync( string address, ushort length ) => await ReadStringAsync( address, length, Encoding.ASCII );

		/// <inheritdoc cref="IReadWriteNet.ReadStringAsync(string, ushort, Encoding)"/>
		public virtual async Task<OperateResult<string>> ReadStringAsync( string address, ushort length, Encoding encoding ) => ByteTransformHelper.GetResultFromBytes( await ReadAsync( address, length ), m => ByteTransform.TransString( m, 0, m.Length, encoding ) );
#endif
		#endregion

		#region Async Write Support
#if !NET35 && !NET20
		/// <inheritdoc cref="IReadWriteNet.WriteAsync(string, short[])"/>
		public virtual async Task<OperateResult> WriteAsync( string address, short[] values ) => await WriteAsync( address, ByteTransform.TransByte( values ) );

		/// <inheritdoc cref="IReadWriteNet.WriteAsync(string, short)"/>
		public virtual async Task<OperateResult> WriteAsync( string address, short value ) => await WriteAsync( address, new short[] { value } );

		/// <inheritdoc cref="IReadWriteNet.WriteAsync(string, ushort[])"/>
		public virtual async Task<OperateResult> WriteAsync( string address, ushort[] values ) => await WriteAsync( address, ByteTransform.TransByte( values ) );

		/// <inheritdoc cref="IReadWriteNet.WriteAsync(string, ushort)"/>
		public virtual async Task<OperateResult> WriteAsync( string address, ushort value ) => await WriteAsync( address, new ushort[] { value } );

		/// <inheritdoc cref="IReadWriteNet.WriteAsync(string, int[])"/>
		public virtual async Task<OperateResult> WriteAsync( string address, int[] values ) => await WriteAsync( address, ByteTransform.TransByte( values ) );

		/// <inheritdoc cref="IReadWriteNet.WriteAsync(string, int)"/>
		public virtual async Task<OperateResult> WriteAsync( string address, int value ) => await WriteAsync( address, new int[] { value } );

		/// <inheritdoc cref="IReadWriteNet.WriteAsync(string, uint[])"/>
		public virtual async Task<OperateResult> WriteAsync( string address, uint[] values ) => await WriteAsync( address, ByteTransform.TransByte( values ) );

		/// <inheritdoc cref="IReadWriteNet.WriteAsync(string, uint)"/>
		public virtual async Task<OperateResult> WriteAsync( string address, uint value ) => await WriteAsync( address, new uint[] { value } );

		/// <inheritdoc cref="IReadWriteNet.WriteAsync(string, float[])"/>
		public virtual async Task<OperateResult> WriteAsync( string address, float[] values ) => await WriteAsync( address, ByteTransform.TransByte( values ) );

		/// <inheritdoc cref="IReadWriteNet.WriteAsync(string, float)"/>
		public virtual async Task<OperateResult> WriteAsync( string address, float value ) => await WriteAsync( address, new float[] { value } );

		/// <inheritdoc cref="IReadWriteNet.WriteAsync(string, long[])"/>
		public virtual async Task<OperateResult> WriteAsync( string address, long[] values ) => await WriteAsync( address, ByteTransform.TransByte( values ) );

		/// <inheritdoc cref="IReadWriteNet.WriteAsync(string, long)"/>
		public virtual async Task<OperateResult> WriteAsync( string address, long value ) => await WriteAsync( address, new long[] { value } );

		/// <inheritdoc cref="IReadWriteNet.WriteAsync(string, ulong[])"/>
		public virtual async Task<OperateResult> WriteAsync( string address, ulong[] values ) => await WriteAsync( address, ByteTransform.TransByte( values ) );

		/// <inheritdoc cref="IReadWriteNet.WriteAsync(string, ulong)"/>
		public virtual async Task<OperateResult> WriteAsync( string address, ulong value ) => await WriteAsync( address, new ulong[] { value } );

		/// <inheritdoc cref="IReadWriteNet.WriteAsync(string, double[])"/>
		public virtual async Task<OperateResult> WriteAsync( string address, double[] values ) => await WriteAsync( address, ByteTransform.TransByte( values ) );

		/// <inheritdoc cref="IReadWriteNet.WriteAsync(string, double)"/>
		public virtual async Task<OperateResult> WriteAsync( string address, double value ) => await WriteAsync( address, new double[] { value } );

		/// <inheritdoc cref="IReadWriteNet.WriteAsync(string, string)" />
		public virtual async Task<OperateResult> WriteAsync( string address, string value ) => await WriteAsync( address, value, Encoding.ASCII );

		/// <inheritdoc cref="IReadWriteNet.WriteAsync(string, string, Encoding)"/>
		public virtual async Task<OperateResult> WriteAsync( string address, string value, Encoding encoding )
		{
			byte[] temp = ByteTransform.TransByte( value, encoding );
			if (this.WordLength == 1) temp = SoftBasic.ArrayExpandToLengthEven( temp );
			return await WriteAsync( address, temp );
		}

		/// <inheritdoc cref="IReadWriteNet.WriteAsync(string, string, int)"/>
		public virtual async Task<OperateResult> WriteAsync( string address, string value, int length ) => await WriteAsync( address, value, length, Encoding.ASCII );

		/// <inheritdoc cref="IReadWriteNet.WriteAsync(string, string, int, Encoding)"/>
		public virtual async Task<OperateResult> WriteAsync( string address, string value, int length, Encoding encoding )
		{
			byte[] temp = ByteTransform.TransByte( value, encoding );
			if (this.WordLength == 1) temp = SoftBasic.ArrayExpandToLengthEven( temp );
			temp = SoftBasic.ArrayExpandToLength( temp, length );
			return await WriteAsync( address, temp );
		}
#endif
		#endregion

	}
}
