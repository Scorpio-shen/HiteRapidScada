using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HslCommunication.Core;
using HslCommunication;
using HslCommunication.Core.Net;
using System.Net.Sockets;
using HslCommunication.Core.IMessage;
using HslCommunication.Core.Address;
using HslCommunication.BasicFramework;
using HslCommunication.Reflection;
using System.IO.Ports;
using HslCommunication.ModBus;
#if !NET20 && !NET35
using System.Threading.Tasks;
#endif

namespace HslCommunication.ModBus
{
	/// <summary>
	/// <b>[商业授权]</b> Modbus的虚拟服务器，同时支持Tcp，Rtu，Ascii的机制，支持线圈，离散输入，寄存器和输入寄存器的读写操作，同时支持掩码写入功能，可以用来当做系统的数据交换池<br />
	/// <b>[Authorization]</b> Modbus virtual server supports Tcp and Rtu mechanisms at the same time, supports read and write operations of coils, discrete inputs, r
	/// egisters and input registers, and supports mask write function, which can be used as a system data exchange pool
	/// </summary>
	/// <remarks>
	/// 可以基于本类实现一个功能复杂的modbus服务器，支持Modbus-Tcp，启动串口后，还支持Modbus-Rtu和Modbus-ASCII，会根据报文进行动态的适配。<br />
	/// 线圈，功能码对应01，05，15<br />
	/// 离散输入，功能码对应02，服务器写入离散输入的地址使用 x=2;100<br />
	/// 寄存器，功能码对应03，06，16<br />
	/// 输入寄存器，功能码对应04，输入寄存器在服务器写入使用地址 x=4;100<br />
	/// 掩码写入，功能码对应22，可以对字寄存器进行位操作<br />
	/// 特别说明1: <see cref="StationDataIsolation"/> 属性如果设置为 True 的话，则服务器为每一个站号（0-255）都创建一个数据区，客户端使用站号作为区分可以写入不同的数据区，服务器也可以读取不同数据区的数据，例如 s=2;100<br />
	/// 特别说明2: 如果多个modbus server使用485总线连接，那么属性 <see cref="NetworkDataServerBase.ForceSerialReceiveOnce"/> 需要设置为 <c>True</c>
	/// </remarks>
	/// <example>
	/// <list type="number">
	/// <item>线圈，功能码对应01，05，15</item>
	/// <item>离散输入，功能码对应02</item>
	/// <item>寄存器，功能码对应03，06，16</item>
	/// <item>输入寄存器，功能码对应04，输入寄存器在服务器端可以实现读写的操作</item>
	/// <item>掩码写入，功能码对应22，可以对字寄存器进行位操作</item>
	/// </list>
	/// 读写的地址格式为富文本地址，具体请参照下面的示例代码。
	/// <code lang="cs" source="HslCommunication_Net45.Test\Documentation\Samples\Modbus\ModbusTcpServer.cs" region="ModbusTcpServerExample" title="ModbusTcpServer示例" />
	/// </example>
	public class ModbusTcpServer : NetworkDataServerBase
	{
		#region Constructor

		/// <summary>
		/// 实例化一个Modbus Tcp及Rtu的服务器，支持数据读写操作
		/// </summary>
		public ModbusTcpServer( )
		{
			this.dictModbusDataPool  = new ModbusDataDict( );

			subscriptions            = new List<ModBusMonitorAddress>( );
			subcriptionHybirdLock    = new SimpleHybirdLock( );
			ByteTransform            = new ReverseWordTransform( );
			WordLength               = 1;
		}

		#endregion

		#region Public Members

		/// <inheritdoc cref="ModbusTcpNet.DataFormat"/>
		public DataFormat DataFormat
		{
			get { return ByteTransform.DataFormat; }
			set { ByteTransform.DataFormat = value; }
		}

		/// <inheritdoc cref="ModbusTcpNet.IsStringReverse"/>
		public bool IsStringReverse
		{
			get { return ByteTransform.IsStringReverseByteWord; }
			set { ByteTransform.IsStringReverseByteWord = value; }
		}

		/// <inheritdoc cref="ModbusTcpNet.Station"/>
		public byte Station
		{
			get { return station; }
			set { station = value; }
		}

		/// <summary>
		/// 获取或设置当前的TCP服务器是否使用modbus-rtu报文进行通信，如果设置为 <c>True</c>，那么客户端需要使用 <see cref="ModbusRtuOverTcp"/><br />
		/// Get or set whether the current TCP server uses modbus-rtu messages for communication.
		/// If it is set to <c>True</c>, then the client needs to use <see cref="ModbusRtuOverTcp"/>
		/// </summary>
		/// <remarks>
		/// 需要注意的是，本属性设置为<c>False</c>时，客户端使用<see cref="ModbusTcpNet"/>，否则，使用<see cref="ModbusRtuOverTcp"/>，不能混合使用
		/// </remarks>
		public bool UseModbusRtuOverTcp { get; set; }

		/// <summary>
		/// 获取或设置两次请求直接的延时时间，单位毫秒，默认是0，不发生延时，设置为20的话，可以有效防止有客户端疯狂进行请求而导致服务器的CPU占用率上升。<br />
		/// Get or set the direct delay time of two requests, in milliseconds, the default is 0, no delay occurs, if it is set to 20, 
		/// it can effectively prevent the client from making crazy requests and causing the server's CPU usage to increase.
		/// </summary>
		public int RequestDelayTime { get; set; }

		/// <summary>
		/// 获取或设置是否启动站点数据隔离功能，默认为 <c>False</c>，也即虚拟服务器模拟一个站点服务器，客户端使用正确的站号才能通信。
		/// 当设置为 <c>True</c> 时，虚拟服务器模式256个站点，无论客户端使用的什么站点，都能读取或是写入对应站点里去。服务器同时也可以访问任意站点自身的数据。<br />
		/// Get or set whether to enable the site data isolation function, the default is <c>False</c>, that is, the virtual server simulates a site server, and the client can communicate with the correct site number.
		/// When set to<c> True</c>, 256 sites in virtual server mode, no matter what site the client uses, can read or write to the corresponding site.The server can also access any site's own data.
		/// </summary>
		/// <remarks>
		/// 当启动站号隔离之后，服务器访问自身的站号2的数据，地址写为 s=2;100
		/// </remarks>
		public bool StationDataIsolation
		{
			get => this.stationDataIsolation;
			set
			{
				this.stationDataIsolation = value;
				this.dictModbusDataPool.Set( value );
			}
		}

		#endregion

		#region Data Persistence

			/// <inheritdoc/>
		protected override byte[] SaveToBytes( )
		{
			return dictModbusDataPool.GetModbusPool( station ).SaveToBytes( );
		}

		/// <inheritdoc/>
		protected override void LoadFromBytes( byte[] content )
		{
			dictModbusDataPool.GetModbusPool( station ).LoadFromBytes( content, 0 );
		}

		#endregion

		#region Coil Read Write
		
		/// <inheritdoc cref="ModbusDataPool.ReadCoil(string)"/>
		public bool ReadCoil( string address )
		{
			byte station = (byte)HslHelper.ExtractParameter( ref address, "s", this.station ); 
			return dictModbusDataPool.GetModbusPool( station ).ReadCoil( address );
		}

		/// <inheritdoc cref="ModbusDataPool.ReadCoil(string, ushort)"/>
		public bool[] ReadCoil( string address, ushort length )
		{
			byte station = (byte)HslHelper.ExtractParameter( ref address, "s", this.station );
			return dictModbusDataPool.GetModbusPool( station ).ReadCoil( address, length );
		}

		/// <inheritdoc cref="ModbusDataPool.WriteCoil(string, bool)"/>
		public void WriteCoil( string address, bool data )
		{
			byte station = (byte)HslHelper.ExtractParameter( ref address, "s", this.station );
			dictModbusDataPool.GetModbusPool( station ).WriteCoil( address, data );
		}

		/// <inheritdoc cref="ModbusDataPool.WriteCoil(string, bool[])"/>
		public void WriteCoil( string address, bool[] data )
		{
			byte station = (byte)HslHelper.ExtractParameter( ref address, "s", this.station );
			dictModbusDataPool.GetModbusPool( station ).WriteCoil( address, data );
		}

		#endregion

		#region Discrete Read Write

		/// <inheritdoc cref="ModbusDataPool.ReadDiscrete(string)"/>
		public bool ReadDiscrete( string address )
		{
			byte station = (byte)HslHelper.ExtractParameter( ref address, "s", this.station );
			return dictModbusDataPool.GetModbusPool( station ).ReadDiscrete( address );
		}

		/// <inheritdoc cref="ModbusDataPool.ReadDiscrete(string, ushort)"/>
		public bool[] ReadDiscrete( string address, ushort length )
		{
			byte station = (byte)HslHelper.ExtractParameter( ref address, "s", this.station );
			return dictModbusDataPool.GetModbusPool( station ).ReadDiscrete( address, length );
		}

		/// <inheritdoc cref="ModbusDataPool.WriteDiscrete(string, bool)"/>
		public void WriteDiscrete( string address, bool data )
		{
			byte station = (byte)HslHelper.ExtractParameter( ref address, "s", this.station );
			dictModbusDataPool.GetModbusPool( station ).WriteDiscrete( address, data );
		}

		/// <inheritdoc cref="ModbusDataPool.WriteDiscrete(string, bool[])"/>
		public void WriteDiscrete( string address, bool[] data )
		{
			byte station = (byte)HslHelper.ExtractParameter( ref address, "s", this.station );
			dictModbusDataPool.GetModbusPool( station ).WriteDiscrete( address, data );
		}

		#endregion

		#region NetworkDataServerBase Override

		/// <inheritdoc cref="ModbusTcpNet.Read(string, ushort)"/>
		[HslMqttApi( "ReadByteArray", "" )]
		public override OperateResult<byte[]> Read( string address, ushort length )
		{
			byte station = (byte)HslHelper.ExtractParameter( ref address, "s", this.station );
			return dictModbusDataPool.GetModbusPool( station ).Read( address, length );
		}

		/// <inheritdoc cref="ModbusTcpNet.Write(string, byte[])"/>
		[HslMqttApi( "WriteByteArray", "" )]
		public override OperateResult Write( string address, byte[] value )
		{
			byte station = (byte)HslHelper.ExtractParameter( ref address, "s", this.station );
			return dictModbusDataPool.GetModbusPool( station ).Write( address, value );
		}

		/// <inheritdoc cref="ModbusTcpNet.ReadBool(string, ushort)"/>
		[HslMqttApi( "ReadBoolArray", "" )]
		public override OperateResult<bool[]> ReadBool( string address, ushort length )
		{
			byte station = (byte)HslHelper.ExtractParameter( ref address, "s", this.station );
			return dictModbusDataPool.GetModbusPool( station ).ReadBool( address, length );
		}

		/// <inheritdoc cref="ModbusTcpNet.Write(string, bool[])"/>
		[HslMqttApi( "WriteBoolArray", "" )]
		public override OperateResult Write( string address, bool[] value )
		{
			byte station = (byte)HslHelper.ExtractParameter( ref address, "s", this.station );
			return dictModbusDataPool.GetModbusPool( station ).Write( address, value );
		}

		/// <summary>
		/// 写入寄存器数据，指定字节数据
		/// </summary>
		/// <param name="address">起始地址，示例："100"，如果是输入寄存器："x=4;100"</param>
		/// <param name="high">高位数据</param>
		/// <param name="low">地位数据</param>
		public void Write( string address, byte high, byte low ) => Write( address, new byte[] { high, low } );

		#endregion

		#region NetServer Override

		/// <inheritdoc/>
		protected override INetMessage GetNewNetMessage( ) => UseModbusRtuOverTcp ? null : new ModbusTcpMessage( );

		/// <inheritdoc/>
		protected override OperateResult<byte[]> ReadFromCoreServer( AppSession session, byte[] receive )
		{
			if (RequestDelayTime > 0) System.Threading.Thread.Sleep( RequestDelayTime );

			if (UseModbusRtuOverTcp)
			{
				if (!Serial.SoftCRC16.CheckCRC16( receive )) return new OperateResult<byte[]>( "CRC Check Failed " );
				byte[] modbusCore = receive.RemoveLast( 2 );
				// 指令长度验证错误，关闭网络连接
				if (!CheckModbusMessageLegal( modbusCore )) return new OperateResult<byte[]>( "Modbus rtu message check failed " );
				if (!StationDataIsolation && station != modbusCore[0]) return new OperateResult<byte[]>( "Station not match Modbus-rtu " );
				// 需要回发消息
				return OperateResult.CreateSuccessResult( ModbusInfo.PackCommandToRtu( ReadFromModbusCore( modbusCore ) ) );
			}
			else
			{
				if (!CheckModbusMessageLegal( receive.RemoveBegin( 6 ) )) return new OperateResult<byte[]>( "Modbus message check failed" );
				ushort id = (ushort)(receive[0] * 256 + receive[1]);
				if (!StationDataIsolation && station != receive[6]) return new OperateResult<byte[]>( "Station not match Modbus-tcp " );

				byte[] back = ModbusInfo.PackCommandToTcp( ReadFromModbusCore( receive.RemoveBegin( 6 ) ), id );
				return OperateResult.CreateSuccessResult( back );
			}
		}
		
#endregion

#region Function Process Center

		/// <summary>
		/// 创建特殊的功能标识，然后返回该信息<br />
		/// Create a special feature ID and return this information
		/// </summary>
		/// <param name="modbusCore">modbus核心报文</param>
		/// <param name="error">错误码</param>
		/// <returns>携带错误码的modbus报文</returns>
		private byte[] CreateExceptionBack( byte[] modbusCore, byte error ) => new byte[] { modbusCore[0], (byte)(modbusCore[1] + 0x80), error };

		/// <summary>
		/// 创建返回消息<br />
		/// Create return message
		/// </summary>
		/// <param name="modbusCore">modbus核心报文</param>
		/// <param name="content">返回的实际数据内容</param>
		/// <returns>携带内容的modbus报文</returns>
		private byte[] CreateReadBack( byte[] modbusCore, byte[] content ) => SoftBasic.SpliceArray<byte>( new byte[] { modbusCore[0], modbusCore[1], (byte)content.Length }, content );

		/// <summary>
		/// 创建写入成功的反馈信号<br />
		/// Create feedback signal for successful write
		/// </summary>
		/// <param name="modbus">modbus核心报文</param>
		/// <returns>携带成功写入的信息</returns>
		private byte[] CreateWriteBack( byte[] modbus ) => modbus.SelectBegin( 6 );

		private byte[] ReadCoilBack( byte[] modbus, string addressHead )
		{
			try
			{
				ushort address = ByteTransform.TransUInt16( modbus, 2 );
				ushort length  = ByteTransform.TransUInt16( modbus, 4 );

				// 越界检测
				if ((address + length) > ushort.MaxValue + 1) return CreateExceptionBack( modbus, ModbusInfo.FunctionCodeOverBound );

				// 地址长度检测
				if (length > 2040) return CreateExceptionBack( modbus, ModbusInfo.FunctionCodeQuantityOver );

				bool[] read = dictModbusDataPool.GetModbusPool( modbus[0] ).ReadBool( addressHead + address.ToString( ), length ).Content;
				return CreateReadBack( modbus, SoftBasic.BoolArrayToByte( read ) );
			}
			catch (Exception ex)
			{
				LogNet?.WriteException( ToString( ), StringResources.Language.ModbusTcpReadCoilException, ex );
				return CreateExceptionBack( modbus, ModbusInfo.FunctionCodeReadWriteException );
			}
		}

		private byte[] ReadRegisterBack( byte[] modbus, string addressHead )
		{
			try
			{
				ushort address = ByteTransform.TransUInt16( modbus, 2 );
				ushort length  = ByteTransform.TransUInt16( modbus, 4 );

				// 越界检测
				if ((address + length) > ushort.MaxValue + 1) return CreateExceptionBack( modbus, ModbusInfo.FunctionCodeOverBound );

				// 地址长度检测
				if (length > 127) return CreateExceptionBack( modbus, ModbusInfo.FunctionCodeQuantityOver );

				byte[] buffer = dictModbusDataPool.GetModbusPool( modbus[0] ).Read( addressHead + address.ToString( ), length ).Content;
				return CreateReadBack( modbus, buffer );
			}
			catch (Exception ex)
			{
				LogNet?.WriteException( ToString( ), StringResources.Language.ModbusTcpReadRegisterException, ex );
				return CreateExceptionBack( modbus, ModbusInfo.FunctionCodeReadWriteException );
			}
		}

		private byte[] WriteOneCoilBack( byte[] modbus )
		{
			try
			{
				// 先判断是否有写入的权利，没有的话，直接返回写入异常
				if (!this.EnableWrite) return CreateExceptionBack( modbus, ModbusInfo.FunctionCodeReadWriteException );

				ushort address = ByteTransform.TransUInt16( modbus, 2 );

				if      (modbus[4] == 0xFF && modbus[5] == 0x00) dictModbusDataPool.GetModbusPool( modbus[0] ).Write( address.ToString( ), new bool[] { true } );
				else if (modbus[4] == 0x00 && modbus[5] == 0x00) dictModbusDataPool.GetModbusPool( modbus[0] ).Write( address.ToString( ), new bool[] { false } );

				return CreateWriteBack( modbus );
			}
			catch (Exception ex)
			{
				LogNet?.WriteException( ToString( ), StringResources.Language.ModbusTcpWriteCoilException, ex );
				return CreateExceptionBack( modbus, ModbusInfo.FunctionCodeReadWriteException );
			}
		}

		private byte[] WriteOneRegisterBack( byte[] modbus )
		{
			try
			{
				// 先判断是否有写入的权利，没有的话，直接返回写入异常
				if (!this.EnableWrite) return CreateExceptionBack( modbus, ModbusInfo.FunctionCodeReadWriteException );

				ushort address = ByteTransform.TransUInt16( modbus, 2 );
				short ValueOld = ReadInt16( address.ToString( ) ).Content;

				dictModbusDataPool.GetModbusPool( modbus[0] ).Write( address.ToString( ), new byte[] { modbus[4], modbus[5] } );
				short ValueNew = ReadInt16( address.ToString( ) ).Content;

				OnRegisterBeforWrite( address, ValueOld, ValueNew );
				return CreateWriteBack( modbus );
			}
			catch (Exception ex)
			{
				LogNet?.WriteException( ToString( ), StringResources.Language.ModbusTcpWriteRegisterException, ex );
				return CreateExceptionBack( modbus, ModbusInfo.FunctionCodeReadWriteException );
			}
		}

		private byte[] WriteCoilsBack( byte[] modbus )
		{
			try
			{
				// 先判断是否有写入的权利，没有的话，直接返回写入异常
				if (!this.EnableWrite) return CreateExceptionBack( modbus, ModbusInfo.FunctionCodeReadWriteException );

				ushort address = ByteTransform.TransUInt16( modbus, 2 );
				ushort length  = ByteTransform.TransUInt16( modbus, 4 );

				if ((address + length) > ushort.MaxValue + 1) return CreateExceptionBack( modbus, ModbusInfo.FunctionCodeOverBound );

				if (length > 2040) return CreateExceptionBack( modbus, ModbusInfo.FunctionCodeQuantityOver );

				dictModbusDataPool.GetModbusPool( modbus[0] ).Write( address.ToString( ), modbus.RemoveBegin( 7 ).ToBoolArray( length ) );
				return CreateWriteBack( modbus );
			}
			catch (Exception ex)
			{
				LogNet?.WriteException( ToString( ), StringResources.Language.ModbusTcpWriteCoilException, ex );
				return CreateExceptionBack( modbus, ModbusInfo.FunctionCodeReadWriteException );
			}
		}

		private byte[] WriteRegisterBack( byte[] modbus )
		{
			try
			{
				// 先判断是否有写入的权利，没有的话，直接返回写入异常
				if (!this.EnableWrite) return CreateExceptionBack( modbus, ModbusInfo.FunctionCodeReadWriteException );

				ushort address = ByteTransform.TransUInt16( modbus, 2 );
				ushort length  = ByteTransform.TransUInt16( modbus, 4 );

				if ((address + length) > ushort.MaxValue + 1) return CreateExceptionBack( modbus, ModbusInfo.FunctionCodeOverBound );

				if (length > 127) return CreateExceptionBack( modbus, ModbusInfo.FunctionCodeQuantityOver );

				byte[] oldValue = dictModbusDataPool.GetModbusPool( modbus[0] ).Read( address.ToString( ), length ).Content;
				dictModbusDataPool.GetModbusPool( modbus[0] ).Write( address.ToString( ), modbus.RemoveBegin( 7 ) );

				// 为了使服务器的数据订阅更加的准确，决定将设计改为等待所有的数据写入完成后，再统一触发订阅，2018年3月4日 20:56:47
				MonitorAddress[] addresses = new MonitorAddress[length];
				for (ushort i = 0; i < length; i++)
				{
					short ValueOld = ByteTransform.TransInt16( oldValue, i * 2);
					short ValueNew = ByteTransform.TransInt16( modbus, i * 2 + 7 );

					// 触发写入请求
					addresses[i] = new MonitorAddress( )
					{
						Address = (ushort)(address + i),
						ValueOrigin = ValueOld,
						ValueNew = ValueNew
					};
				}

				// 所有数据都更改完成后，再触发消息
				for (int i = 0; i < addresses.Length; i++)
				{
					OnRegisterBeforWrite( addresses[i].Address, addresses[i].ValueOrigin, addresses[i].ValueNew );
				}

				return CreateWriteBack( modbus );
			}
			catch (Exception ex)
			{
				LogNet?.WriteException( ToString( ), StringResources.Language.ModbusTcpWriteRegisterException, ex );
				return CreateExceptionBack( modbus, ModbusInfo.FunctionCodeReadWriteException );
			}
		}

		private byte[] WriteMaskRegisterBack( byte[] modbus )
		{
			try
			{
				// 先判断是否有写入的权利，没有的话，直接返回写入异常
				if (!this.EnableWrite) return CreateExceptionBack( modbus, ModbusInfo.FunctionCodeReadWriteException );

				ushort address = ByteTransform.TransUInt16( modbus, 2 );
				int and_Mask   = ByteTransform.TransUInt16( modbus, 4 );
				int or_Mask    = ByteTransform.TransUInt16( modbus, 6 );

				int ValueOld = ReadInt16( $"s={modbus[0]};" + address.ToString( ) ).Content;
				short ValueNew = (short)((ValueOld & and_Mask) | or_Mask);
				Write( $"s={modbus[0]};" + address.ToString( ), ValueNew );

				// 触发写入请求
				MonitorAddress addresses = new MonitorAddress( )
				{
					Address     = address,
					ValueOrigin = (short)ValueOld,
					ValueNew    = ValueNew
				};

				// 所有数据都更改完成后，再触发消息
				OnRegisterBeforWrite( addresses.Address, addresses.ValueOrigin, addresses.ValueNew );

				return modbus;
			}
			catch (Exception ex)
			{
				LogNet?.WriteException( ToString( ), StringResources.Language.ModbusTcpWriteRegisterException, ex );
				return CreateExceptionBack( modbus, ModbusInfo.FunctionCodeReadWriteException );
			}
		}


#endregion

#region Subscription Support

		// 本服务器端支持指定地址的数据订阅器，目前仅支持寄存器操作

		private List<ModBusMonitorAddress> subscriptions;     // 数据订阅集合
		private SimpleHybirdLock subcriptionHybirdLock;       // 集合锁

		/// <summary>
		/// 新增一个数据监视的任务，针对的是寄存器地址的数据<br />
		/// Added a data monitoring task for data at register addresses
		/// </summary>
		/// <param name="monitor">监视地址对象</param>
		public void AddSubcription( ModBusMonitorAddress monitor )
		{
			subcriptionHybirdLock.Enter( );
			subscriptions.Add( monitor );
			subcriptionHybirdLock.Leave( );
		}

		/// <summary>
		/// 移除一个数据监视的任务<br />
		/// Remove a data monitoring task
		/// </summary>
		/// <param name="monitor">监视地址对象</param>
		public void RemoveSubcrption( ModBusMonitorAddress monitor )
		{
			subcriptionHybirdLock.Enter( );
			subscriptions.Remove( monitor );
			subcriptionHybirdLock.Leave( );
		}

		/// <summary>
		/// 在数据变更后，进行触发是否产生订阅<br />
		/// Whether to generate a subscription after triggering data changes
		/// </summary>
		/// <param name="address">数据地址</param>
		/// <param name="before">修改之前的数</param>
		/// <param name="after">修改之后的数</param>
		private void OnRegisterBeforWrite( ushort address, short before, short after )
		{
			subcriptionHybirdLock.Enter( );
			for (int i = 0; i < subscriptions.Count; i++)
			{
				if (subscriptions[i].Address == address)
				{
					subscriptions[i].SetValue( after );
					if (before != after)
					{
						subscriptions[i].SetChangeValue( before, after );
					}
				}
			}
			subcriptionHybirdLock.Leave( );
		}

#endregion

#region Modbus Core Logic

		/// <summary>
		/// 检测当前的Modbus接收的指定是否是合法的<br />
		/// Check if the current Modbus datad designation is valid
		/// </summary>
		/// <param name="buffer">缓存数据</param>
		/// <returns>是否合格</returns>
		private bool CheckModbusMessageLegal( byte[] buffer )
		{
			bool check = false;
			switch (buffer[1])
			{
				case ModbusInfo.ReadCoil:
				case ModbusInfo.ReadDiscrete:
				case ModbusInfo.ReadRegister:
				case ModbusInfo.ReadInputRegister:
				case ModbusInfo.WriteOneCoil:
				case ModbusInfo.WriteOneRegister: check = buffer.Length == 0x06; break;
				case ModbusInfo.WriteCoil:
				case ModbusInfo.WriteRegister: check = buffer.Length > 6 && (buffer[6] == (buffer.Length - 7)); break;
				case ModbusInfo.WriteMaskRegister: check = buffer.Length == 0x08; break;
				default: check = true; break;
			}
			if (check == false) LogNet?.WriteError( ToString( ), $"Receive Nosense Modbus-rtu : " + buffer.ToHexString( ' ' ) );
			return check;
		}

		/// <summary>
		/// Modbus核心数据交互方法，允许重写自己来实现，报文只剩下核心的Modbus信息，去除了MPAB报头信息<br />
		/// The Modbus core data interaction method allows you to rewrite it to achieve the message. 
		/// Only the core Modbus information is left in the message, and the MPAB header information is removed.
		/// </summary>
		/// <param name="modbusCore">核心的Modbus报文</param>
		/// <returns>进行数据交互之后的结果</returns>
		protected virtual byte[] ReadFromModbusCore( byte[] modbusCore )
		{
			byte[] buffer;
			switch (modbusCore[1])
			{
				case ModbusInfo.ReadCoil:              buffer = ReadCoilBack( modbusCore, string.Empty ); break;
				case ModbusInfo.ReadDiscrete:          buffer = ReadCoilBack( modbusCore, "x=2;" ); break;
				case ModbusInfo.ReadRegister:          buffer = ReadRegisterBack( modbusCore, string.Empty ); break;
				case ModbusInfo.ReadInputRegister:     buffer = ReadRegisterBack( modbusCore, "x=4;" ); break;
				case ModbusInfo.WriteOneCoil:          buffer = WriteOneCoilBack( modbusCore ); break;
				case ModbusInfo.WriteOneRegister:      buffer = WriteOneRegisterBack( modbusCore ); break;
				case ModbusInfo.WriteCoil:             buffer = WriteCoilsBack( modbusCore ); break;
				case ModbusInfo.WriteRegister:         buffer = WriteRegisterBack( modbusCore ); break;
				case ModbusInfo.WriteMaskRegister:     buffer = WriteMaskRegisterBack( modbusCore ); break;
				default:                               buffer = CreateExceptionBack( modbusCore, ModbusInfo.FunctionCodeNotSupport ); break;
			}

			return buffer;
		}

		#endregion

		#region Serial Support

		/// <inheritdoc/>
		protected override bool CheckSerialReceiveDataComplete( byte[] buffer, int dataLength )
		{
			if (dataLength > 5)
			{
				if (ModbusInfo.CheckAsciiReceiveDataComplete( buffer, dataLength )) return true;
				if (ModbusInfo.CheckServerRtuReceiveDataComplete( buffer.SelectBegin( dataLength ) )) return true;
			}
			return false;
		}

		/// <inheritdoc/>
		protected override string GetSerialMessageLogText( byte[] data )
		{
			if (data[0] != 0x3A)
			{
				return $"[Rtu] {data.ToHexString( ' ' )}";
			}
			else
			{
				return $"[Ascii] {SoftBasic.GetAsciiStringRender( data )}";
			}
		}

		/// <inheritdoc/>
		protected override OperateResult<byte[]> DealWithSerialReceivedData( byte[] data )
		{
			if (data.Length < 3) return new OperateResult<byte[]>( $"Uknown Data：{data.ToHexString( ' ' )}" );

			if (data[0] != 0x3A)
			{
				if (Serial.SoftCRC16.CheckCRC16( data ))
				{
					byte[] modbusCore = data.RemoveLast( 2 );

					// 指令长度验证错误，关闭网络连接
					if (!CheckModbusMessageLegal( modbusCore )) return new OperateResult<byte[]>( $"Unlegal Data：{data.ToHexString( ' ' )}" );

					// 验证站号是否一致
					if ( !StationDataIsolation && station != modbusCore[0])
					{
						return new OperateResult<byte[]>( $"Station not match Modbus-rtu : {data.ToHexString( ' ' )}" );
					}

					// 需要回发消息
					byte[] back = ModbusInfo.PackCommandToRtu( ReadFromModbusCore( modbusCore ) );

					return OperateResult.CreateSuccessResult( back );
				}
				else
				{
					return new OperateResult<byte[]>( $"CRC Check Failed : {data.ToHexString( ' ' )}" );
				}
			}
			else
			{
				OperateResult<byte[]> ascii = ModbusInfo.TransAsciiPackCommandToCore( data );
				if (!ascii.IsSuccess) return ascii;

				byte[] modbusCore = ascii.Content;
				// 指令长度验证错误，关闭网络连接
				if (!CheckModbusMessageLegal( modbusCore )) return new OperateResult<byte[]>( $"Unlegal Data：{data.ToHexString( ' ' )}" );

				// 验证站号是否一致
				if (!StationDataIsolation && station != modbusCore[0])
				{
					return new OperateResult<byte[]>( $"Station not match Modbus-Ascii : {SoftBasic.GetAsciiStringRender( data )}" );
				}

				// 需要回发消息
				byte[] back = ModbusInfo.TransModbusCoreToAsciiPackCommand( ReadFromModbusCore( modbusCore ) );

				return OperateResult.CreateSuccessResult( back );
			}
		}

#endregion

#region IDisposable Support

		/// <inheritdoc/>
		protected override void Dispose( bool disposing )
		{
			if (disposing)
			{
				subcriptionHybirdLock?.Dispose( );
				subscriptions?.Clear( );
				dictModbusDataPool?.Dispose( );
				GC.Collect( );
			}
			base.Dispose( disposing );
		}

		#endregion

		#region Private Member

		private ModbusDataDict dictModbusDataPool;
		private byte station = 1;                                          // 服务器的站号数据，对于tcp无效，对于rtu来说，如果小于0，则忽略站号信息
		private bool stationDataIsolation = false;                         // 各站点的数据是否进行隔离

		#endregion

		#region DataFormat Support

		/// <inheritdoc cref="IReadWriteNet.ReadInt32(string, ushort)"/>
		[HslMqttApi( "ReadInt32Array", "" )]
		public override OperateResult<int[]> ReadInt32( string address, ushort length )
		{
			IByteTransform transform = HslHelper.ExtractTransformParameter( ref address, this.ByteTransform );
			return ByteTransformHelper.GetResultFromBytes( Read( address, (ushort)(length * WordLength * 2) ), m => transform.TransInt32( m, 0, length ) );
		}

		/// <inheritdoc cref="IReadWriteNet.ReadUInt32(string, ushort)"/>
		[HslMqttApi( "ReadUInt32Array", "" )]
		public override OperateResult<uint[]> ReadUInt32( string address, ushort length )
		{
			IByteTransform transform = HslHelper.ExtractTransformParameter( ref address, this.ByteTransform );
			return ByteTransformHelper.GetResultFromBytes( Read( address, (ushort)(length * WordLength * 2) ), m => transform.TransUInt32( m, 0, length ) );
		}

		/// <inheritdoc cref="IReadWriteNet.ReadFloat(string, ushort)"/>
		[HslMqttApi( "ReadFloatArray", "" )]
		public override OperateResult<float[]> ReadFloat( string address, ushort length )
		{
			IByteTransform transform = HslHelper.ExtractTransformParameter( ref address, this.ByteTransform );
			return ByteTransformHelper.GetResultFromBytes( Read( address, (ushort)(length * WordLength * 2) ), m => transform.TransSingle( m, 0, length ) );
		}

		/// <inheritdoc cref="IReadWriteNet.ReadInt64(string, ushort)"/>
		[HslMqttApi( "ReadInt64Array", "" )]
		public override OperateResult<long[]> ReadInt64( string address, ushort length )
		{
			IByteTransform transform = HslHelper.ExtractTransformParameter( ref address, this.ByteTransform );
			return ByteTransformHelper.GetResultFromBytes( Read( address, (ushort)(length * WordLength * 4) ), m => transform.TransInt64( m, 0, length ) );
		}

		/// <inheritdoc cref="IReadWriteNet.ReadUInt64(string, ushort)"/>
		[HslMqttApi( "ReadUInt64Array", "" )]
		public override OperateResult<ulong[]> ReadUInt64( string address, ushort length )
		{
			IByteTransform transform = HslHelper.ExtractTransformParameter( ref address, this.ByteTransform );
			return ByteTransformHelper.GetResultFromBytes( Read( address, (ushort)(length * WordLength * 4) ), m => transform.TransUInt64( m, 0, length ) );
		}

		/// <inheritdoc cref="IReadWriteNet.ReadDouble(string, ushort)"/>
		[HslMqttApi( "ReadDoubleArray", "" )]
		public override OperateResult<double[]> ReadDouble( string address, ushort length )
		{
			IByteTransform transform = HslHelper.ExtractTransformParameter( ref address, this.ByteTransform );
			return ByteTransformHelper.GetResultFromBytes( Read( address, (ushort)(length * WordLength * 4) ), m => transform.TransDouble( m, 0, length ) );
		}

		/// <inheritdoc cref="IReadWriteNet.Write(string, int[])"/>
		[HslMqttApi( "WriteInt32Array", "" )]
		public override OperateResult Write( string address, int[] values )
		{
			IByteTransform transform = HslHelper.ExtractTransformParameter( ref address, this.ByteTransform );
			return Write( address, transform.TransByte( values ) );
		}

		/// <inheritdoc cref="IReadWriteNet.Write(string, uint[])"/>
		[HslMqttApi( "WriteUInt32Array", "" )]
		public override OperateResult Write( string address, uint[] values )
		{
			IByteTransform transform = HslHelper.ExtractTransformParameter( ref address, this.ByteTransform );
			return Write( address, transform.TransByte( values ) );
		}

		/// <inheritdoc cref="IReadWriteNet.Write(string, float[])"/>
		[HslMqttApi( "WriteFloatArray", "" )]
		public override OperateResult Write( string address, float[] values )
		{
			IByteTransform transform = HslHelper.ExtractTransformParameter( ref address, this.ByteTransform );
			return Write( address, transform.TransByte( values ) );
		}

		/// <inheritdoc cref="IReadWriteNet.Write(string, long[])"/>
		[HslMqttApi( "WriteInt64Array", "" )]
		public override OperateResult Write( string address, long[] values )
		{
			IByteTransform transform = HslHelper.ExtractTransformParameter( ref address, this.ByteTransform );
			return Write( address, transform.TransByte( values ) );
		}

		/// <inheritdoc cref="IReadWriteNet.Write(string, ulong[])"/>
		[HslMqttApi( "WriteUInt64Array", "" )]
		public override OperateResult Write( string address, ulong[] values )
		{
			IByteTransform transform = HslHelper.ExtractTransformParameter( ref address, this.ByteTransform );
			return Write( address, transform.TransByte( values ) );
		}

		/// <inheritdoc cref="IReadWriteNet.Write(string, double[])"/>
		[HslMqttApi( "WriteDoubleArray", "" )]
		public override OperateResult Write( string address, double[] values )
		{
			IByteTransform transform = HslHelper.ExtractTransformParameter( ref address, this.ByteTransform );
			return Write( address, transform.TransByte( values ) );
		}

#if !NET35 && !NET20
		/// <inheritdoc cref="IReadWriteNet.ReadInt32Async(string, ushort)"/>
		public override async Task<OperateResult<int[]>> ReadInt32Async( string address, ushort length )
		{
			IByteTransform transform = HslHelper.ExtractTransformParameter( ref address, this.ByteTransform );
			return ByteTransformHelper.GetResultFromBytes( await ReadAsync( address, (ushort)(length * WordLength * 2) ), m => transform.TransInt32( m, 0, length ) );
		}

		/// <inheritdoc cref="IReadWriteNet.ReadUInt32Async(string, ushort)"/>
		public override async Task<OperateResult<uint[]>> ReadUInt32Async( string address, ushort length )
		{
			IByteTransform transform = HslHelper.ExtractTransformParameter( ref address, this.ByteTransform );
			return ByteTransformHelper.GetResultFromBytes( await ReadAsync( address, (ushort)(length * WordLength * 2) ), m => transform.TransUInt32( m, 0, length ) );
		}

		/// <inheritdoc cref="IReadWriteNet.ReadFloatAsync(string, ushort)"/>
		public override async Task<OperateResult<float[]>> ReadFloatAsync( string address, ushort length )
		{
			IByteTransform transform = HslHelper.ExtractTransformParameter( ref address, this.ByteTransform );
			return ByteTransformHelper.GetResultFromBytes( await ReadAsync( address, (ushort)(length * WordLength * 2) ), m => transform.TransSingle( m, 0, length ) );
		}

		/// <inheritdoc cref="IReadWriteNet.ReadInt64Async(string, ushort)"/>
		public override async Task<OperateResult<long[]>> ReadInt64Async( string address, ushort length )
		{
			IByteTransform transform = HslHelper.ExtractTransformParameter( ref address, this.ByteTransform );
			return ByteTransformHelper.GetResultFromBytes( await ReadAsync( address, (ushort)(length * WordLength * 4) ), m => transform.TransInt64( m, 0, length ) );
		}

		/// <inheritdoc cref="IReadWriteNet.ReadUInt64Async(string, ushort)"/>
		public override async Task<OperateResult<ulong[]>> ReadUInt64Async( string address, ushort length )
		{
			IByteTransform transform = HslHelper.ExtractTransformParameter( ref address, this.ByteTransform );
			return ByteTransformHelper.GetResultFromBytes( await ReadAsync( address, (ushort)(length * WordLength * 4) ), m => transform.TransUInt64( m, 0, length ) );
		}

		/// <inheritdoc cref="IReadWriteNet.ReadDoubleAsync(string, ushort)"/>
		public override async Task<OperateResult<double[]>> ReadDoubleAsync( string address, ushort length )
		{
			IByteTransform transform = HslHelper.ExtractTransformParameter( ref address, this.ByteTransform );
			return ByteTransformHelper.GetResultFromBytes( await ReadAsync( address, (ushort)(length * WordLength * 4) ), m => transform.TransDouble( m, 0, length ) );
		}

		/// <inheritdoc cref="IReadWriteNet.WriteAsync(string, int[])"/>
		public override async Task<OperateResult> WriteAsync( string address, int[] values )
		{
			IByteTransform transform = HslHelper.ExtractTransformParameter( ref address, this.ByteTransform );
			return await WriteAsync( address, transform.TransByte( values ) );
		}

		/// <inheritdoc cref="IReadWriteNet.WriteAsync(string, uint[])"/>
		public override async Task<OperateResult> WriteAsync( string address, uint[] values )
		{
			IByteTransform transform = HslHelper.ExtractTransformParameter( ref address, this.ByteTransform );
			return await WriteAsync( address, transform.TransByte( values ) );
		}

		/// <inheritdoc cref="IReadWriteNet.WriteAsync(string, float[])"/>
		public override async Task<OperateResult> WriteAsync( string address, float[] values )
		{
			IByteTransform transform = HslHelper.ExtractTransformParameter( ref address, this.ByteTransform );
			return await WriteAsync( address, transform.TransByte( values ) );
		}

		/// <inheritdoc cref="IReadWriteNet.WriteAsync(string, long[])"/>
		public override async Task<OperateResult> WriteAsync( string address, long[] values )
		{
			IByteTransform transform = HslHelper.ExtractTransformParameter( ref address, this.ByteTransform );
			return await WriteAsync( address, transform.TransByte( values ) );
		}

		/// <inheritdoc cref="IReadWriteNet.WriteAsync(string, ulong[])"/>
		public override async Task<OperateResult> WriteAsync( string address, ulong[] values )
		{
			IByteTransform transform = HslHelper.ExtractTransformParameter( ref address, this.ByteTransform );
			return await WriteAsync( address, transform.TransByte( values ) );
		}

		/// <inheritdoc cref="IReadWriteNet.WriteAsync(string, double[])"/>
		public override async Task<OperateResult> WriteAsync( string address, double[] values )
		{
			IByteTransform transform = HslHelper.ExtractTransformParameter( ref address, this.ByteTransform );
			return await WriteAsync( address, transform.TransByte( values ) );
		}
#endif
		#endregion

		#region Object Override

		/// <inheritdoc/>
		public override string ToString( ) => $"ModbusTcpServer[{Port}]";

#endregion
	}
}

internal class ModbusDataDict : IDisposable
{
	public ModbusDataDict( )
	{
		dictModbusDataPool = new Dictionary<int, ModbusDataPool>( );
		dictModbusDataPool.Add( 0, new ModbusDataPool( 0 ) );
	}

	public void Set( bool stationDataIsolation )
	{
		if (this.stationDataIsolation != stationDataIsolation)
		{
			this.stationDataIsolation = stationDataIsolation;
			if (this.stationDataIsolation)
			{
				dictModbusDataPool = new Dictionary<int, ModbusDataPool>( );
				for (int i = 0; i < 255; i++)
				{
					dictModbusDataPool.Add( i, new ModbusDataPool( (byte)i ) );
				}
			}
			else
			{
				dictModbusDataPool = new Dictionary<int, ModbusDataPool>( );
				dictModbusDataPool.Add( 0, new ModbusDataPool( 0 ) );
			}
		}
	}

	public ModbusDataPool GetModbusPool( byte station )
	{
		if (this.stationDataIsolation)
		{
			return dictModbusDataPool[station];
		}
		else
		{
			return dictModbusDataPool.FirstOrDefault( ).Value;
		}
	}

	#region IDisposable Support

	/// <inheritdoc/>
	public void Dispose( )
	{
		foreach (var dataPool in dictModbusDataPool.Values)
		{
			dataPool.Dispose( );
		}
		dictModbusDataPool.Clear( );
	}

	#endregion



	private Dictionary<int, ModbusDataPool> dictModbusDataPool;
	private bool stationDataIsolation = false;
}


internal class ModbusDataPool : IDisposable
{
	public ModbusDataPool( byte station )
	{
		this.coilBuffer               = new SoftBuffer( DataPoolLength );
		this.inputBuffer              = new SoftBuffer( DataPoolLength );
		this.registerBuffer           = new SoftBuffer( DataPoolLength * 2 );
		this.inputRegisterBuffer      = new SoftBuffer( DataPoolLength * 2 );

		this.registerBuffer.IsBoolReverseByWord      = true;
		this.inputRegisterBuffer.IsBoolReverseByWord = true;
		this.station = station;
	}


	public OperateResult<byte[]> Read( string address, ushort length )
	{
		OperateResult<ModbusAddress> analysis = ModbusInfo.AnalysisAddress( address, station, true, ModbusInfo.ReadRegister );
		if (!analysis.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( analysis );

		if (analysis.Content.Function == ModbusInfo.ReadRegister)
			return OperateResult.CreateSuccessResult( registerBuffer.GetBytes( analysis.Content.Address * 2, length * 2 ) );
		else if (analysis.Content.Function == ModbusInfo.ReadInputRegister)
			return OperateResult.CreateSuccessResult( inputRegisterBuffer.GetBytes( analysis.Content.Address * 2, length * 2 ) );
		else
			return new OperateResult<byte[]>( StringResources.Language.NotSupportedDataType );
	}

	public OperateResult Write( string address, byte[] value )
	{
		OperateResult<ModbusAddress> analysis = ModbusInfo.AnalysisAddress( address, station, true, ModbusInfo.ReadRegister );
		if (!analysis.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( analysis );

		if (analysis.Content.Function == ModbusInfo.ReadRegister || analysis.Content.Function == ModbusInfo.WriteOneRegister || analysis.Content.Function == ModbusInfo.WriteRegister)
		{
			registerBuffer.SetBytes( value, analysis.Content.Address * 2 );
			return OperateResult.CreateSuccessResult( );
		}
		else if (analysis.Content.Function == ModbusInfo.ReadInputRegister)
		{
			inputRegisterBuffer.SetBytes( value, analysis.Content.Address * 2 );
			return OperateResult.CreateSuccessResult( );
		}
		else
		{
			return new OperateResult<byte[]>( StringResources.Language.NotSupportedDataType );
		}
	}

	public OperateResult<bool[]> ReadBool( string address, ushort length )
	{
		if (address.IndexOf( '.' ) < 0)
		{
			OperateResult<ModbusAddress> analysis = ModbusInfo.AnalysisAddress( address, station, true, ModbusInfo.ReadCoil );
			if (!analysis.IsSuccess) return OperateResult.CreateFailedResult<bool[]>( analysis );

			if (analysis.Content.Function == ModbusInfo.ReadCoil || analysis.Content.Function == ModbusInfo.WriteOneCoil || analysis.Content.Function == ModbusInfo.WriteCoil)
				return OperateResult.CreateSuccessResult( coilBuffer.GetBytes( analysis.Content.Address, length ).Select( m => m != 0x00 ).ToArray( ) );
			else if (analysis.Content.Function == ModbusInfo.ReadDiscrete)
				return OperateResult.CreateSuccessResult( inputBuffer.GetBytes( analysis.Content.Address, length ).Select( m => m != 0x00 ).ToArray( ) );
			else
				return new OperateResult<bool[]>( StringResources.Language.NotSupportedDataType );
		}
		else
		{
			// 以位的方式读取寄存器，输入寄存器的值
			try
			{
				int bitIndex = Convert.ToInt32( address.Substring( address.IndexOf( '.' ) + 1 ) );
				address = address.Substring( 0, address.IndexOf( '.' ) );

				OperateResult<ModbusAddress> analysis = ModbusInfo.AnalysisAddress( address, station, true, ModbusInfo.ReadRegister );
				if (!analysis.IsSuccess) return OperateResult.CreateFailedResult<bool[]>( analysis );

				bitIndex = analysis.Content.Address * 16 + bitIndex;
				if (analysis.Content.Function == ModbusInfo.ReadRegister)
				{
					return OperateResult.CreateSuccessResult( registerBuffer.GetBool( bitIndex, length ) );
				}
				else if (analysis.Content.Function == ModbusInfo.ReadInputRegister)
				{
					return OperateResult.CreateSuccessResult( inputRegisterBuffer.GetBool( bitIndex, length ) );
				}
				else
				{
					return new OperateResult<bool[]>( StringResources.Language.NotSupportedDataType );
				}
			}
			catch (Exception ex)
			{
				return new OperateResult<bool[]>( ex.Message );
			}
		}
	}

	public OperateResult Write( string address, bool[] value )
	{
		if (address.IndexOf( '.' ) < 0)
		{
			OperateResult<ModbusAddress> analysis = ModbusInfo.AnalysisAddress( address, station, true, ModbusInfo.ReadCoil );
			if (!analysis.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( analysis );

			if (analysis.Content.Function == ModbusInfo.ReadCoil || analysis.Content.Function == ModbusInfo.WriteCoil || analysis.Content.Function == ModbusInfo.WriteOneCoil)
			{
				coilBuffer.SetBytes( value.Select( m => (byte)(m ? 0x01 : 0x00) ).ToArray( ), analysis.Content.Address );
				return OperateResult.CreateSuccessResult( );
			}
			else if (analysis.Content.Function == ModbusInfo.ReadDiscrete)
			{
				inputBuffer.SetBytes( value.Select( m => (byte)(m ? 0x01 : 0x00) ).ToArray( ), analysis.Content.Address );
				return OperateResult.CreateSuccessResult( );
			}
			else
			{
				return new OperateResult<byte[]>( StringResources.Language.NotSupportedDataType );
			}
		}
		else
		{
			try
			{
				int bitIndex = Convert.ToInt32( address.Substring( address.IndexOf( '.' ) + 1 ) );
				address = address.Substring( 0, address.IndexOf( '.' ) );

				OperateResult<ModbusAddress> analysis = ModbusInfo.AnalysisAddress( address, station, true, ModbusInfo.ReadRegister );
				if (!analysis.IsSuccess) return analysis;

				bitIndex = analysis.Content.Address * 16 + bitIndex;
				if (analysis.Content.Function == ModbusInfo.ReadRegister)
				{
					registerBuffer.SetBool( value, bitIndex );
					return OperateResult.CreateSuccessResult( );
				}
				else if (analysis.Content.Function == ModbusInfo.ReadInputRegister)
				{
					inputRegisterBuffer.SetBool( value, bitIndex );
					return OperateResult.CreateSuccessResult( );
				}
				else
				{
					return new OperateResult( StringResources.Language.NotSupportedDataType );
				}
			}
			catch (Exception ex)
			{
				return new OperateResult( ex.Message );
			}
		}
	}

	#region Coil Read Write

	/// <summary>
	/// 读取地址的线圈的通断情况
	/// </summary>
	/// <param name="address">起始地址，示例："100"</param>
	/// <returns><c>True</c>或是<c>False</c></returns>
	/// <exception cref="IndexOutOfRangeException"></exception>
	public bool ReadCoil( string address )
	{
		ushort add = ushort.Parse( address );
		return coilBuffer.GetByte( add ) != 0x00;
	}

	/// <summary>
	/// 批量读取地址的线圈的通断情况
	/// </summary>
	/// <param name="address">起始地址，示例："100"</param>
	/// <param name="length">读取长度</param>
	/// <returns><c>True</c>或是<c>False</c></returns>
	/// <exception cref="IndexOutOfRangeException"></exception>
	public bool[] ReadCoil( string address, ushort length )
	{
		ushort add = ushort.Parse( address );
		return coilBuffer.GetBytes( add, length ).Select( m => m != 0x00 ).ToArray( );
	}

	/// <summary>
	/// 写入线圈的通断值
	/// </summary>
	/// <param name="address">起始地址，示例："100"</param>
	/// <param name="data">是否通断</param>
	/// <returns><c>True</c>或是<c>False</c></returns>
	/// <exception cref="IndexOutOfRangeException"></exception>
	public void WriteCoil( string address, bool data )
	{
		ushort add = ushort.Parse( address );
		coilBuffer.SetValue( (byte)(data ? 0x01 : 0x00), add );
	}

	/// <summary>
	/// 写入线圈数组的通断值
	/// </summary>
	/// <param name="address">起始地址，示例："100"</param>
	/// <param name="data">是否通断</param>
	/// <returns><c>True</c>或是<c>False</c></returns>
	/// <exception cref="IndexOutOfRangeException"></exception>
	public void WriteCoil( string address, bool[] data )
	{
		if (data == null) return;

		ushort add = ushort.Parse( address );
		coilBuffer.SetBytes( data.Select( m => (byte)(m ? 0x01 : 0x00) ).ToArray( ), add );
	}

	#endregion

	#region Discrete Read Write

	/// <summary>
	/// 读取地址的离散线圈的通断情况
	/// </summary>
	/// <param name="address">起始地址，示例："100"</param>
	/// <returns><c>True</c>或是<c>False</c></returns>
	/// <exception cref="IndexOutOfRangeException"></exception>
	public bool ReadDiscrete( string address )
	{
		ushort add = ushort.Parse( address );
		return inputBuffer.GetByte( add ) != 0x00;
	}

	/// <summary>
	/// 批量读取地址的离散线圈的通断情况
	/// </summary>
	/// <param name="address">起始地址，示例："100"</param>
	/// <param name="length">读取长度</param>
	/// <returns><c>True</c>或是<c>False</c></returns>
	/// <exception cref="IndexOutOfRangeException"></exception>
	public bool[] ReadDiscrete( string address, ushort length )
	{
		ushort add = ushort.Parse( address );
		return inputBuffer.GetBytes( add, length ).Select( m => m != 0x00 ).ToArray( );
	}

	/// <summary>
	/// 写入离散线圈的通断值
	/// </summary>
	/// <param name="address">起始地址，示例："100"</param>
	/// <param name="data">是否通断</param>
	/// <exception cref="IndexOutOfRangeException"></exception>
	public void WriteDiscrete( string address, bool data )
	{
		ushort add = ushort.Parse( address );
		inputBuffer.SetValue( (byte)(data ? 0x01 : 0x00), add );
	}

	/// <summary>
	/// 写入离散线圈数组的通断值
	/// </summary>
	/// <param name="address">起始地址，示例："100"</param>
	/// <param name="data">是否通断</param>
	/// <exception cref="IndexOutOfRangeException"></exception>
	public void WriteDiscrete( string address, bool[] data )
	{
		if (data == null) return;

		ushort add = ushort.Parse( address );
		inputBuffer.SetBytes( data.Select( m => (byte)(m ? 0x01 : 0x00) ).ToArray( ), add );
	}

	#endregion

	/// <inheritdoc cref="ModbusTcpServer.SaveToBytes"/>
	public byte[] SaveToBytes( )
	{
		byte[] buffer = new byte[DataPoolLength * 6];
		Array.Copy( coilBuffer.GetBytes( ), 0,          buffer, DataPoolLength * 0, DataPoolLength );
		Array.Copy( inputBuffer.GetBytes( ), 0,         buffer, DataPoolLength * 1, DataPoolLength );
		Array.Copy( registerBuffer.GetBytes( ), 0,      buffer, DataPoolLength * 2, DataPoolLength * 2 );
		Array.Copy( inputRegisterBuffer.GetBytes( ), 0, buffer, DataPoolLength * 4, DataPoolLength * 2 );
		return buffer;
	}

	/// <inheritdoc cref="ModbusTcpServer.LoadFromBytes(byte[])"/>
	public void LoadFromBytes( byte[] content, int index )
	{
		if (content.Length < DataPoolLength * 6) throw new Exception( "File is not correct" );

		coilBuffer.         SetBytes( content, DataPoolLength * 0 + index, 0, DataPoolLength );
		inputBuffer.        SetBytes( content, DataPoolLength * 1 + index, 0, DataPoolLength );
		registerBuffer.     SetBytes( content, DataPoolLength * 2 + index, 0, DataPoolLength * 2 );
		inputRegisterBuffer.SetBytes( content, DataPoolLength * 4 + index, 0, DataPoolLength * 2 );
	}

	#region IDisposable Support

	/// <inheritdoc/>
	public void Dispose( )
	{
		coilBuffer?.Dispose( );
		inputBuffer?.Dispose( );
		registerBuffer?.Dispose( );
		inputRegisterBuffer?.Dispose( );
	}

	#endregion

	private byte station = 1;                     // 当前的数据池的站号信息
	private SoftBuffer coilBuffer;                // 线圈的数据池
	private SoftBuffer inputBuffer;               // 离散输入的数据池
	private SoftBuffer registerBuffer;            // 寄存器的数据池
	private SoftBuffer inputRegisterBuffer;       // 输入寄存器的数据池
	private const int DataPoolLength = 65536;     // 数据的长度
}
