using HslCommunication.BasicFramework;
using HslCommunication.Serial;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HslCommunication.Core;
using HslCommunication.Core.Address;
using HslCommunication.Reflection;
using System.IO;
#if !NET35 && !NET20
using System.Threading.Tasks;
#endif

namespace HslCommunication.ModBus
{
	/// <summary>
	/// Modbus-Rtu通讯协议的类库，多项式码0xA001，支持标准的功能码，也支持扩展的功能码实现，地址采用富文本的形式，详细见备注说明<br />
	/// Modbus-Rtu communication protocol class library, polynomial code 0xA001, supports standard function codes, 
	/// and also supports extended function code implementation. The address is in rich text. For details, see the remark
	/// </summary>
	/// <remarks>
	/// 本客户端支持的标准的modbus协议，Modbus-Tcp及Modbus-Udp内置的消息号会进行自增，地址支持富文本格式，具体参考示例代码。<br />
	/// 读取线圈，输入线圈，寄存器，输入寄存器的方法中的读取长度对商业授权用户不限制，内部自动切割读取，结果合并。
	/// </remarks>
	/// <example>
	/// <inheritdoc cref="ModbusTcpNet" path="example"/>
	/// </example>
	public class ModbusRtu : SerialDeviceBase, IModbus
	{
		#region Constructor

		/// <summary>
		/// 实例化一个Modbus-Rtu协议的客户端对象<br />
		/// Instantiate a client object of the Modbus-Rtu protocol
		/// </summary>
		public ModbusRtu( ) 
		{
			this.ByteTransform            = new ReverseWordTransform( );
			this.ReceiveEmptyDataCount    = 5;
		}

		/// <summary>
		/// 指定Modbus从站的站号来初始化<br />
		/// Specify the station number of the Modbus slave to initialize
		/// </summary>
		/// <param name="station">Modbus从站的站号</param>
		public ModbusRtu( byte station = 0x01 )
		{
			this.station               = station;
			this.ByteTransform         = new ReverseWordTransform( );
			this.ReceiveEmptyDataCount = 5;
		}

		#endregion

		#region Private Member

		private byte station = 0x01;                                 // 本客户端的站号
		private bool isAddressStartWithZero = true;                  // 线圈值的地址值是否从零开始

		#endregion

		#region Public Member

		/// <inheritdoc cref="ModbusTcpNet.AddressStartWithZero"/>
		public bool AddressStartWithZero
		{
			get { return isAddressStartWithZero; }
			set { isAddressStartWithZero = value; }
		}

		/// <inheritdoc cref="ModbusTcpNet.Station"/>
		public byte Station
		{
			get { return station; }
			set { station = value; }
		}

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

		/// <summary>
		/// 获取或设置是否启用CRC16校验码的检查功能，默认启用，如果需要忽略检查CRC16，则设置为 false 即可。<br />
		/// Gets or sets whether to enable the check function of CRC16 check code. It is enabled by default. If you need to ignore the check of CRC16, you can set it to false.
		/// </summary>
		public bool Crc16CheckEnable { get; set; } = true;

		/// <inheritdoc cref="IModbus.TranslateToModbusAddress(string,byte)"/>
		public virtual OperateResult<string> TranslateToModbusAddress( string address, byte modbusCode )
		{
			return OperateResult.CreateSuccessResult( address );
		}

		public bool IsConnected
		{
			get => IsOpen();
		}

		#endregion

		#region 断开设备连接
		public void DisConnect()
		{
			Close();
			Dispose();
		}
		#endregion

		#region Core Interative

		/// <inheritdoc/>
		public override byte[] PackCommandWithHeader( byte[] command ) => ModbusInfo.PackCommandToRtu( command );

		/// <inheritdoc/>
		public override OperateResult<byte[]> UnpackResponseContent( byte[] send, byte[] response ) => ModbusHelper.ExtraRtuResponseContent( send, response, Crc16CheckEnable );

		/// <summary>
		/// 将Modbus报文数据发送到当前的通道中，并从通道中接收Modbus的报文，通道将根据当前连接自动获取，本方法是线程安全的。<br />
		/// Send Modbus message data to the current channel, and receive Modbus messages from the channel. The channel will automatically obtain it according to the current connection. This method is thread-safe.
		/// </summary>
		/// <param name="send">发送的完整的报文信息</param>
		/// <returns>接收到的Modbus报文信息</returns>
		/// <remarks>
		/// 需要注意的是，本方法的发送和接收都只需要输入Modbus核心报文，例如读取寄存器0的字数据 01 03 00 00 00 01，最后面两个字节的CRC是自动添加的，收到的数据也是只有modbus核心报文，例如：01 03 02 00 00 , 已经成功校验CRC校验并移除了，所以在解析的时候需要注意。<br />
		/// It should be noted that the sending and receiving of this method only need to input Modbus core messages, for example, read the word data 01 03 00 00 00 01 of register 0, the last two bytes of CRC are automatically added, 
		/// and received The data is also only modbus core messages, for example: 01 03 02 00 00, CRC has been successfully checked and removed, so you need to pay attention when parsing.
		/// </remarks>
		public override OperateResult<byte[]> ReadFromCoreServer( byte[] send ) => base.ReadFromCoreServer( send );

		/// <inheritdoc/>
		protected override bool CheckReceiveDataComplete( MemoryStream ms )
		{
			return ModbusInfo.CheckRtuReceiveDataComplete( ms.ToArray( ) );
		}

		#endregion

		#region Read Support

		/// <inheritdoc cref="ModbusTcpNet.ReadCoil(string)"/>
		public OperateResult<bool> ReadCoil( string address ) => ReadBool( address );

		/// <inheritdoc cref="ModbusTcpNet.ReadCoil(string, ushort)"/>
		public OperateResult<bool[]> ReadCoil( string address, ushort length ) => ReadBool( address, length );

		/// <inheritdoc cref="ModbusTcpNet.ReadDiscrete(string)"/>
		public OperateResult<bool> ReadDiscrete( string address ) => ByteTransformHelper.GetResultFromArray( ReadDiscrete( address, 1 ) );

		/// <inheritdoc cref="ModbusTcpNet.ReadDiscrete(string, ushort)"/>
		public OperateResult<bool[]> ReadDiscrete( string address, ushort length ) => ModbusHelper.ReadBoolHelper( this, address, length, ModbusInfo.ReadDiscrete );

		/// <inheritdoc cref="ModbusTcpNet.Read(string, ushort)"/>
		[HslMqttApi( "ReadByteArray", "" )]
		public override OperateResult<byte[]> Read( string address, ushort length ) => ModbusHelper.Read( this, address, length );

		/// <inheritdoc cref="ModbusTcpNet.Write(string, byte[])"/>
		[HslMqttApi( "WriteByteArray", "" )]
		public override OperateResult Write( string address, byte[] value ) => ModbusHelper.Write( this, address, value );

		/// <inheritdoc cref="ModbusTcpNet.Write(string, short)"/>
		[HslMqttApi( "WriteInt16", "" )]
		public override OperateResult Write( string address, short value ) => ModbusHelper.Write( this, address, value );

		/// <inheritdoc cref="ModbusTcpNet.Write(string, ushort)"/>
		[HslMqttApi( "WriteUInt16", "" )]
		public override OperateResult Write( string address, ushort value ) => ModbusHelper.Write( this, address, value );

		/// <inheritdoc cref="ModbusTcpNet.WriteMask(string, ushort, ushort)"/>
		[HslMqttApi( "WriteMask", "" )]
		public OperateResult WriteMask( string address, ushort andMask, ushort orMask ) => ModbusHelper.WriteMask( this, address, andMask, orMask );

		#endregion

		#region Write One Registe

		/// <inheritdoc cref="Write(string, short)"/>
		public OperateResult WriteOneRegister( string address, short value ) => Write( address, value );

		/// <inheritdoc cref="Write(string, ushort)"/>
		public OperateResult WriteOneRegister( string address, ushort value ) => Write( address, value );

		#endregion

		#region Async Read Support
#if !NET35 && !NET20
		/// <inheritdoc cref="Write(string, short)"/>/param>
		public async override Task<OperateResult> WriteAsync( string address, short value ) => await Task.Run( ( ) => Write( address, value ) );

		/// <inheritdoc cref="Write(string, ushort)"/>/param>
		public async override Task<OperateResult> WriteAsync( string address, ushort value ) => await Task.Run( ( ) => Write( address, value ) );

		/// <inheritdoc cref="ReadCoil(string)"/>
		public async Task<OperateResult<bool>> ReadCoilAsync( string address ) => await Task.Run( ( ) => ReadCoil( address ) );

		/// <inheritdoc cref="ReadCoil(string, ushort)"/>
		public async Task<OperateResult<bool[]>> ReadCoilAsync( string address, ushort length ) => await Task.Run( ( ) => ReadCoil( address, length ) );

		/// <inheritdoc cref="ReadDiscrete(string)"/>
		public async Task<OperateResult<bool>> ReadDiscreteAsync( string address ) => await Task.Run( ( ) => ReadDiscrete( address ) );

		/// <inheritdoc cref="ReadDiscrete(string, ushort)"/>
		public async Task<OperateResult<bool[]>> ReadDiscreteAsync( string address, ushort length ) => await Task.Run( ( ) => ReadDiscrete( address, length ) );

		/// <inheritdoc cref="WriteOneRegister(string, short)"/>
		public async Task<OperateResult> WriteOneRegisterAsync( string address, short value ) => await Task.Run( ( ) => WriteOneRegister( address, value ) );

		/// <inheritdoc cref="WriteOneRegister(string, ushort)"/>
		public async Task<OperateResult> WriteOneRegisterAsync( string address, ushort value ) => await Task.Run( ( ) => WriteOneRegister( address, value ) );

		/// <inheritdoc cref="WriteMask(string, ushort, ushort)"/>
		public async Task<OperateResult> WriteMaskAsync( string address, ushort andMask, ushort orMask ) => await Task.Run( ( ) => WriteMask( address, andMask, orMask ) );
#endif
		#endregion

		#region Bool Support

		/// <inheritdoc cref="ModbusTcpNet.ReadBool(string, ushort)"/>
		[HslMqttApi( "ReadBoolArray", "" )]
		public override OperateResult<bool[]> ReadBool( string address, ushort length ) => ModbusHelper.ReadBoolHelper( this, address, length, ModbusInfo.ReadCoil );

		/// <inheritdoc cref="ModbusTcpNet.Write(string, bool[])"/>
		[HslMqttApi( "WriteBoolArray", "" )]
		public override OperateResult Write( string address, bool[] values ) => ModbusHelper.Write( this, address, values );

		/// <inheritdoc cref="ModbusTcpNet.Write(string, bool)"/>
		[HslMqttApi( "WriteBool", "" )]
		public override OperateResult Write( string address, bool value ) => ModbusHelper.Write( this, address, value );

		#endregion

		#region Async Bool Support
#if !NET35 && !NET20
		/// <inheritdoc cref="Write(string, bool)"/>
		public override async Task<OperateResult> WriteAsync( string address, bool value ) => await Task.Run( ( ) => Write( address, value ) );
#endif
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
		public override string ToString( ) => $"ModbusRtu[{PortName}:{BaudRate}]";

		#endregion
	}
}
