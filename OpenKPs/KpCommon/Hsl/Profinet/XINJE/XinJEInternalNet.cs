using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HslCommunication.BasicFramework;
using HslCommunication.Core;
using HslCommunication.Core.IMessage;
using HslCommunication.Core.Net;
using HslCommunication.ModBus;
using HslCommunication.Reflection;
#if !NET35 && !NET20
using System.Threading.Tasks;
#endif

namespace HslCommunication.Profinet.XINJE
{
	/// <summary>
	/// 信捷内部的TCP信息
	/// </summary>
	public class XinJEInternalNet : NetworkDeviceBase
	{
		#region Constructor

		/// <summary>
		/// 实例化一个XINJE-Tcp协议的客户端对象<br />
		/// Instantiate a client object of the Modbus-Tcp protocol
		/// </summary>
		public XinJEInternalNet( )
		{
			this.softIncrementCount = new SoftIncrementCount( ushort.MaxValue );
			this.WordLength = 1;
			this.station = 1;
			this.ByteTransform = new ReverseWordTransform( );
			this.ByteTransform.DataFormat = DataFormat.CDAB;
		}

		/// <summary>
		/// 指定服务器地址，端口号，客户端自己的站号来初始化<br />
		/// Specify the server address, port number, and client's own station number to initialize
		/// </summary>
		/// <param name="ipAddress">服务器的Ip地址</param>
		/// <param name="port">服务器的端口号</param>
		/// <param name="station">客户端自身的站号</param>
		public XinJEInternalNet( string ipAddress, int port = 502, byte station = 0x01 ) : this( )
		{
			this.IpAddress = ipAddress;
			this.Port = port;
			this.station = station;
		}

		/// <inheritdoc/>
		protected override INetMessage GetNewNetMessage( ) => new ModbusTcpMessage( );

		#endregion

		#region Private Member

		private byte station = 0x01;                                         // 本客户端的站号
		private readonly SoftIncrementCount softIncrementCount;              // 自增消息的对象

		#endregion

		#region Public Member

		/// <summary>
		/// 获取或者重新修改服务器的默认站号信息，当然，你可以再读写的时候动态指定，参见备注<br />
		/// Get or modify the default station number information of the server. Of course, you can specify it dynamically when reading and writing, see note
		/// </summary>
		/// <remarks>
		/// 当你调用 ReadCoil("100") 时，对应的站号就是本属性的值，当你调用 ReadCoil("s=2;100") 时，就忽略本属性的值，读写寄存器的时候同理
		/// </remarks>
		public byte Station
		{
			get { return station; }
			set { station = value; }
		}

		/// <inheritdoc cref="ByteTransformBase.DataFormat"/>
		public DataFormat DataFormat
		{
			get { return ByteTransform.DataFormat; }
			set { ByteTransform.DataFormat = value; }
		}

		/// <summary>
		/// 字符串数据是否按照字来反转，默认为False<br />
		/// Whether the string data is reversed according to words. The default is False.
		/// </summary>
		/// <remarks>
		/// 字符串按照2个字节的排列进行颠倒，根据实际情况进行设置
		/// </remarks>
		public bool IsStringReverse
		{
			get { return ByteTransform.IsStringReverseByteWord; }
			set { ByteTransform.IsStringReverseByteWord = value; }
		}

		/// <summary>
		/// 获取协议自增的消息号，你可以自定义modbus的消息号的规则，详细参见<see cref="XinJEInternalNet"/>说明，也可以查找<see cref="SoftIncrementCount"/>说明。<br />
		/// Get the message number incremented by the modbus protocol. You can customize the rules of the message number of the modbus. For details, please refer to the description of <see cref = "ModbusTcpNet" />, or you can find the description of <see cref = "SoftIncrementCount" />
		/// </summary>
		public SoftIncrementCount MessageId => softIncrementCount;

		#endregion

		#region Core Override

		/// <inheritdoc/>
		public override byte[] PackCommandWithHeader( byte[] command )
		{
			return ModbusInfo.PackCommandToTcp( command, (ushort)softIncrementCount.GetCurrentValue( ) );
		}

		/// <inheritdoc/>
		public override OperateResult<byte[]> UnpackResponseContent( byte[] send, byte[] response )
		{
			return ModbusInfo.ExtractActualData( ModbusInfo.ExplodeTcpCommandToCore( response ) );
		}

		#endregion

		#region Read Write Support

		/// <inheritdoc/>
		/// <remarks>
		/// 地址支持 D100, SD100, TD100, CD100, HD100, FD100, ETD100, HTD100, HCD100, HSD100, 各自的地址范围取决于实际PLC的范围，比如D的地址在XLH型号上可达 0~499999<br />
		/// Address support D100, SD100, TD100, CD100, HD100, FD100, ETD100, HTD100, HCD100, HSD100, the respective address range depends on the actual PLC range, for example, the address of D can reach 0~499999 on the XLH model
		/// </remarks>
		[HslMqttApi( "ReadByteArray", "" )]
		public override OperateResult<byte[]> Read( string address, ushort length )
		{
			OperateResult<List<byte[]>> command = XinJEHelper.BuildReadCommand( Station, address, length, false );
			if (!command.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( command );

			return ReadFromCoreServer( command.Content );
		}

		/// <inheritdoc/>
		/// <remarks>
		/// 地址支持 M100, X100, Y100, SM100, T100, C100, HM100, HS100, HT100, HSC100 各自的地址范围取决于实际PLC的范围，比如M的地址在XLH型号上可达 0~199999<br />
		/// The address supports M100, X100, Y100, SM100, T100, C100, HM100, HS100, HT100, HSC100, the respective address range depends on the actual PLC range, for example, the address of M can reach 0~199999 on the XLH model
		/// </remarks>
		[HslMqttApi( "ReadBoolArray", "" )]
		public override OperateResult<bool[]> ReadBool( string address, ushort length )
		{
			OperateResult<List<byte[]>> command = XinJEHelper.BuildReadCommand( Station, address, length, true );
			if (!command.IsSuccess) return OperateResult.CreateFailedResult<bool[]>( command );

			OperateResult<byte[]> read = ReadFromCoreServer( command.Content );
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<bool[]>( read );

			return OperateResult.CreateSuccessResult( read.Content.ToBoolArray( ).SelectBegin( length ) );
		}

		/// <inheritdoc/>
		/// <remarks>
		/// 地址支持 D100, SD100, TD100, CD100, HD100, FD100, ETD100, HTD100, HCD100, HSD100,  各自的地址范围取决于实际PLC的范围，比如D的地址在XLH型号上可达 0~499999<br />
		/// Address support D100, SD100, TD100, CD100, HD100, FD100, ETD100, HTD100, HCD100, HSD100,  the respective address range depends on the actual PLC range, for example, the address of D can reach 0~499999 on the XLH model
		/// </remarks>
		[HslMqttApi( "WriteByteArray", "" )]
		public override OperateResult Write( string address, byte[] value )
		{
			OperateResult<byte[]> command = XinJEHelper.BuildWriteWordCommand( Station, address, value );
			if (!command.IsSuccess) return command;

			return ReadFromCoreServer( command.Content );
		}

		/// <inheritdoc/>
		/// <remarks>
		/// 地址支持 M100, Y100, SM100, T100, C100, 各自的地址范围取决于实际PLC的范围，比如M的地址在XLH型号上可达 0~199999<br />
		/// The address supports M100, Y100, SM100, T100, C100, the respective address range depends on the actual PLC range, for example, the address of M can reach 0~199999 on the XLH model
		/// </remarks>
		[HslMqttApi( "WriteBoolArray", "" )]
		public override OperateResult Write( string address, bool[] value )
		{
			OperateResult<byte[]> command = XinJEHelper.BuildWriteBoolCommand( Station, address, value );
			if (!command.IsSuccess) return command;

			return ReadFromCoreServer( command.Content );
		}
#if !NET35 && !NET20
		/// <inheritdoc cref="Read(string, ushort)"/>
		public async override Task<OperateResult<byte[]>> ReadAsync( string address, ushort length )
		{
			OperateResult<List<byte[]>> command = XinJEHelper.BuildReadCommand( Station, address, length, false );
			if (!command.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( command );

			return await ReadFromCoreServerAsync( command.Content );
		}
		/// <inheritdoc cref="ReadBool(string, ushort)"/>
		public async override Task<OperateResult<bool[]>> ReadBoolAsync( string address, ushort length )
		{
			OperateResult<List<byte[]>> command = XinJEHelper.BuildReadCommand( Station, address, length, true );
			if (!command.IsSuccess) return OperateResult.CreateFailedResult<bool[]>( command );

			OperateResult<byte[]> read = await ReadFromCoreServerAsync( command.Content );
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<bool[]>( read );

			return OperateResult.CreateSuccessResult( read.Content.ToBoolArray( ).SelectBegin( length ) );
		}

		/// <inheritdoc cref="Write(string, byte[])"/>
		public async override Task<OperateResult> WriteAsync( string address, byte[] value )
		{
			OperateResult<byte[]> command = XinJEHelper.BuildWriteWordCommand( Station, address, value );
			if (!command.IsSuccess) return command;

			return await ReadFromCoreServerAsync( command.Content );
		}

		/// <inheritdoc cref="Write(string, bool[])"/>
		public async override Task<OperateResult> WriteAsync( string address, bool[] value )
		{
			OperateResult<byte[]> command = XinJEHelper.BuildWriteBoolCommand( Station, address, value );
			if (!command.IsSuccess) return command;

			return await ReadFromCoreServerAsync( command.Content );
		}
#endif
		#endregion

		#region Object Override

		/// <inheritdoc/>
		public override string ToString( ) => $"XinJEInternalNet[{IpAddress}:{Port}]";

		#endregion
	}
}
