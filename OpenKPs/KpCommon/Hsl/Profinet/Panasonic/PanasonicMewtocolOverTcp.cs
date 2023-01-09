using HslCommunication.Core;
using HslCommunication.Core.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HslCommunication.Reflection;
using HslCommunication.Core.IMessage;
#if !NET35 && !NET20
using System.Threading.Tasks;
#endif

namespace HslCommunication.Profinet.Panasonic
{
	/// <summary>
	/// 松下PLC的数据交互协议，采用Mewtocol协议通讯，基于Tcp透传实现的机制，支持的地址列表参考api文档<br />
	/// The data exchange protocol of Panasonic PLC adopts Mewtocol protocol for communication. 
	/// It is based on the mechanism of Tcp transparent transmission. For the list of supported addresses, refer to the api document.
	/// </summary>
	/// <remarks>
	/// 地址支持携带站号的访问方式，例如：s=2;D100
	/// </remarks>
	/// <example>
	/// 触点地址的输入的格式说明如下：
	/// <list type="table">
	///   <listheader>
	///     <term>地址名称</term>
	///     <term>地址代号</term>
	///     <term>示例</term>
	///     <term>地址进制</term>
	///     <term>字操作</term>
	///     <term>位操作</term>
	///     <term>备注</term>
	///   </listheader>
	///   <item>
	///     <term>外部输入继电器</term>
	///     <term>X</term>
	///     <term>X11,X1F</term>
	///     <term>10</term>
	///     <term>×</term>
	///     <term>√</term>
	///     <term>X33 等同于 X3.3</term>
	///   </item>
	///   <item>
	///     <term>外部输出继电器</term>
	///     <term>Y</term>
	///     <term>Y22,Y2A</term>
	///     <term>10</term>
	///     <term>×</term>
	///     <term>√</term>
	///     <term>Y21 等同于 Y2.1</term>
	///   </item>
	///   <item>
	///     <term>内部继电器</term>
	///     <term>R</term>
	///     <term>R0F,R100F</term>
	///     <term>10</term>
	///     <term>×</term>
	///     <term>√</term>
	///     <term>R21 等同于 R2.1</term>
	///   </item>
	///   <item>
	///     <term>定时器</term>
	///     <term>T</term>
	///     <term>T0,T100</term>
	///     <term>10</term>
	///     <term>×</term>
	///     <term>√</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>计数器</term>
	///     <term>C</term>
	///     <term>C0,C100</term>
	///     <term>10</term>
	///     <term>×</term>
	///     <term>√</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>链接继电器</term>
	///     <term>L</term>
	///     <term>L0F,L100F</term>
	///     <term>10</term>
	///     <term>×</term>
	///     <term>√</term>
	///     <term>L21 等同于 L2.1</term>
	///   </item>
	/// </list>
	/// 数据地址的输入的格式说明如下：
	/// <list type="table">
	///   <listheader>
	///     <term>地址名称</term>
	///     <term>地址代号</term>
	///     <term>示例</term>
	///     <term>地址进制</term>
	///     <term>字操作</term>
	///     <term>位操作</term>
	///     <term>备注</term>
	///   </listheader>
	///   <item>
	///     <term>数据寄存器 DT</term>
	///     <term>D</term>
	///     <term>D0,D100</term>
	///     <term>10</term>
	///     <term>√</term>
	///     <term>×</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>链接寄存器 LD</term>
	///     <term>LD</term>
	///     <term>LD0,LD100</term>
	///     <term>10</term>
	///     <term>√</term>
	///     <term>×</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>文件寄存器 FL</term>
	///     <term>F</term>
	///     <term>F0,F100</term>
	///     <term>10</term>
	///     <term>√</term>
	///     <term>×</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>目标值 SV</term>
	///     <term>S</term>
	///     <term>S0,S100</term>
	///     <term>10</term>
	///     <term>√</term>
	///     <term>×</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>经过值 EV</term>
	///     <term>K</term>
	///     <term>K0,K100</term>
	///     <term>10</term>
	///     <term>√</term>
	///     <term>×</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>索引寄存器 IX</term>
	///     <term>IX</term>
	///     <term>IX0,IX100</term>
	///     <term>10</term>
	///     <term>√</term>
	///     <term>×</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>索引寄存器 IY</term>
	///     <term>IY</term>
	///     <term>IY0,IY100</term>
	///     <term>10</term>
	///     <term>√</term>
	///     <term>×</term>
	///     <term></term>
	///   </item>
	/// </list>
	/// </example>
	public class PanasonicMewtocolOverTcp : NetworkDeviceBase
	{
		#region Constructor

		/// <summary>
		/// 实例化一个默认的松下PLC通信对象，默认站号为0xEE<br />
		/// Instantiate a default Panasonic PLC communication object, the default station number is 0xEE
		/// </summary>
		/// <param name="station">站号信息，默认为0xEE</param>
		public PanasonicMewtocolOverTcp( byte station = 238 )
		{
			this.ByteTransform            = new RegularByteTransform( );
			this.Station                  = station;
			this.ByteTransform.DataFormat = DataFormat.DCBA;
			this.WordLength               = 1;
			this.LogMsgFormatBinary       = false;
		}

		/// <summary>
		/// 实例化一个默认的松下PLC通信对象，指定ip地址，端口，默认站号为0xEE<br />
		/// Instantiate a default Panasonic PLC communication object, specify the IP address, port, and the default station number is 0xEE
		/// </summary>
		/// <param name="ipAddress">Ip地址数据</param>
		/// <param name="port">端口号</param>
		/// <param name="station">站号信息，默认为0xEE</param>
		public PanasonicMewtocolOverTcp( string ipAddress, int port, byte station = 238 ) : this( station )
		{
			this.IpAddress                = ipAddress;
			this.Port                     = port;
		}

		/// <inheritdoc/>
		protected override INetMessage GetNewNetMessage( ) => new SpecifiedCharacterMessage( AsciiControl.CR );

		#endregion

		#region Public Properties

		/// <summary>
		/// PLC设备的目标站号，需要根据实际的设置来填写<br />
		/// The target station number of the PLC device needs to be filled in according to the actual settings
		/// </summary>
		public byte Station { get; set; }

		#endregion

		#region Read Write Override

		/// <inheritdoc cref="Helper.MewtocolHelper.Read(IReadWriteDevice, byte, string, ushort)"/>
		[HslMqttApi( "ReadByteArray", "" )]
		public override OperateResult<byte[]> Read( string address, ushort length ) => Helper.MewtocolHelper.Read( this, this.Station, address, length );

		/// <inheritdoc cref="Helper.MewtocolHelper.Write(IReadWriteDevice, byte, string, byte[])"/>
		[HslMqttApi( "WriteByteArray", "" )]
		public override OperateResult Write( string address, byte[] value ) => Helper.MewtocolHelper.Write( this, this.Station, address, value );

		#endregion

		#region Async Read Write Override
#if !NET35 && !NET20
		/// <inheritdoc cref="Read(string, ushort)"/>
		public async override Task<OperateResult<byte[]>> ReadAsync( string address, ushort length ) => await Helper.MewtocolHelper.ReadAsync( this, this.Station, address, length );

		/// <inheritdoc cref="Write(string, byte[])"/>
		public async override Task<OperateResult> WriteAsync( string address, byte[] value ) => await Helper.MewtocolHelper.WriteAsync( this, this.Station, address, value );
#endif
		#endregion

		#region Read Write Bool

		/// <inheritdoc cref="Helper.MewtocolHelper.ReadBool(IReadWriteDevice, byte, string, ushort)"/>
		[HslMqttApi( "ReadBoolArray", "" )]
		public override OperateResult<bool[]> ReadBool( string address, ushort length ) => Helper.MewtocolHelper.ReadBool( this, this.Station, address, length );

		/// <inheritdoc cref="Helper.MewtocolHelper.ReadBool(IReadWriteDevice, byte, string)"/>
		[HslMqttApi( "ReadBool", "" )]
		public override OperateResult<bool> ReadBool( string address ) => Helper.MewtocolHelper.ReadBool( this, this.Station, address );

		/// <inheritdoc cref="Helper.MewtocolHelper.ReadBool(IReadWriteDevice, byte, string[])"/>
		public OperateResult<bool[]> ReadBool( string[] address ) => Helper.MewtocolHelper.ReadBool( this, this.Station, address );

		/// <inheritdoc cref="Helper.MewtocolHelper.Write(IReadWriteDevice, byte, string, bool[])"/>
		[HslMqttApi( "WriteBoolArray", "" )]
		public override OperateResult Write( string address, bool[] values ) => Helper.MewtocolHelper.Write( this, this.Station, address, values );

		/// <inheritdoc cref="Helper.MewtocolHelper.Write(IReadWriteDevice, byte, string, bool)"/>
		[HslMqttApi( "WriteBool", "" )]
		public override OperateResult Write( string address, bool value ) => Helper.MewtocolHelper.Write( this, this.Station, address, value );

		/// <inheritdoc cref="Helper.MewtocolHelper.Write(IReadWriteDevice, byte, string[], bool[])"/>
		public OperateResult Write( string[] address, bool[] value ) => Helper.MewtocolHelper.Write( this, this.Station, address, value );

		#endregion

		#region Async Read Write Bool
#if !NET35 && !NET20
		/// <inheritdoc cref="ReadBool(string)"/>
		public async override Task<OperateResult<bool[]>> ReadBoolAsync( string address, ushort length ) => await Helper.MewtocolHelper.ReadBoolAsync( this, this.Station, address, length );

		/// <inheritdoc cref="ReadBool(string)"/>
		public async override Task<OperateResult<bool>> ReadBoolAsync( string address ) => await Helper.MewtocolHelper.ReadBoolAsync( this, this.Station, address );

		/// <inheritdoc cref="Write(string, bool[])"/>
		public async override Task<OperateResult> WriteAsync( string address, bool[] values ) => await Helper.MewtocolHelper.WriteAsync( this, this.Station, address, values );

		/// <inheritdoc cref="Write(string, bool)"/>
		public async override Task<OperateResult> WriteAsync( string address, bool value ) => await Helper.MewtocolHelper.WriteAsync( this, this.Station, address, value );

		/// <inheritdoc cref="ReadBool(string[])"/>
		public async Task<OperateResult<bool[]>> ReadBoolAsync( string[] address ) => await Helper.MewtocolHelper.ReadBoolAsync( this, this.Station, address );

		/// <inheritdoc cref="Write(string[], bool[])"/>
		public async Task<OperateResult> WriteAsync( string[] address, bool[] value ) => await Helper.MewtocolHelper.WriteAsync( this, this.Station, address, value );
#endif
		#endregion

		#region Object Override

		/// <inheritdoc/>
		public override string ToString( ) => $"PanasonicMewtocolOverTcp[{IpAddress}:{Port}]";

		#endregion
	}
}
