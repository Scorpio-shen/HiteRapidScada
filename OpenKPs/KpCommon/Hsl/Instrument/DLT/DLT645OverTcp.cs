using System;
using System.Linq;
using System.Text;
using HslCommunication.BasicFramework;
using HslCommunication.Core;
using HslCommunication.Core.Net;
using HslCommunication.Core.IMessage;
using HslCommunication.Reflection;
using HslCommunication.Instrument.DLT.Helper;
#if !NET35 && !NET20
using System.Threading.Tasks;
#endif
namespace HslCommunication.Instrument.DLT
{
	/// <summary>
	/// 基于多功能电能表通信协议实现的通讯类，参考的文档是DLT645-2007，主要实现了对电表数据的读取和一些功能方法，
	/// 在点对点模式下，需要在连接后调用 <see cref="ReadAddress"/> 方法，数据标识格式为 00-00-00-00，具体参照文档手册。<br />
	/// The communication type based on the communication protocol of the multifunctional electric energy meter. 
	/// The reference document is DLT645-2007, which mainly realizes the reading of the electric meter data and some functional methods. 
	/// In the point-to-point mode, you need to call <see cref="ReadAddress" /> method after connect the device.
	/// the data identification format is 00-00-00-00, refer to the documentation manual for details.
	/// </summary>
	/// <remarks>
	/// 如果一对多的模式，地址可以携带地址域访问，例如 "s=2;00-00-00-00"，主要使用 <see cref="ReadDouble(string, ushort)"/> 方法来读取浮点数，
	/// <see cref="NetworkDeviceBase.ReadString(string, ushort)"/> 方法来读取字符串
	/// </remarks>
	/// <example>
	/// <inheritdoc cref="DLT645" path="example"/>
	/// </example>
	public class DLT645OverTcp : NetworkDeviceBase, IDlt645
	{
		#region Constructor

		/// <summary>
		/// 指定IP地址，端口，地址域，密码，操作者代码来实例化一个对象<br />
		/// Specify the IP address, port, address field, password, and operator code to instantiate an object
		/// </summary>
		/// <param name="ipAddress">TcpServer的IP地址</param>
		/// <param name="port">TcpServer的端口</param>
		/// <param name="station">设备的站号信息</param>
		/// <param name="password">密码，写入的时候进行验证的信息</param>
		/// <param name="opCode">操作者代码</param>
		public DLT645OverTcp( string ipAddress, int port = 502, string station = "1", string password = "", string opCode = "" )
		{
			this.IpAddress       = ipAddress;
			this.Port            = port;
			base.WordLength      = 1;
			base.ByteTransform   = new ReverseWordTransform( );
			this.station         = station;
			this.password        = string.IsNullOrEmpty( password ) ? "00000000" : password;
			this.opCode          = string.IsNullOrEmpty( opCode ) ? "00000000" : opCode;
		}

		/// <inheritdoc/>
		protected override INetMessage GetNewNetMessage( ) => new DLT645Message( );

		/// <inheritdoc/>
		public override byte[] PackCommandWithHeader( byte[] command )
		{
			if (EnableCodeFE)
				return SoftBasic.SpliceArray( new byte[] { 0xfe, 0xfe, 0xfe, 0xfe }, command );
			return base.PackCommandWithHeader( command );
		}

		#endregion

		#region Public Method

		/// <inheritdoc cref="DLT645.ActiveDeveice"/>
		public OperateResult ActiveDeveice( ) => ReadFromCoreServer( new byte[] { 0xFE, 0xFE, 0xFE, 0xFE }, false );

		/// <inheritdoc cref="DLT645.Read(string, ushort)"/>
		[HslMqttApi( "ReadByteArray", "" )]
		public override OperateResult<byte[]> Read( string address, ushort length ) => Helper.DLT645Helper.Read( this, address, length );

		/// <inheritdoc/>
		[HslMqttApi( "ReadDoubleArray", "" )]
		public override OperateResult<double[]> ReadDouble(string address, ushort length) => Helper.DLT645Helper.ReadDouble( this, address, length );

		/// <inheritdoc/>
		public override OperateResult<string> ReadString( string address, ushort length, Encoding encoding ) => ByteTransformHelper.GetResultFromArray( ReadStringArray( address ) );

		/// <inheritdoc cref="DLT645.ReadStringArray(string)"/>
		public OperateResult<string[]> ReadStringArray( string address ) => Helper.DLT645Helper.ReadStringArray( this, address );

#if !NET35 && !NET20

		/// <inheritdoc cref="DLT645.ActiveDeveice"/>
		public async Task<OperateResult> ActiveDeveiceAsync( ) => await ReadFromCoreServerAsync( new byte[] { 0xFE, 0xFE, 0xFE, 0xFE }, hasResponseData: false, usePackAndUnpack: false );

		/// <inheritdoc cref="DLT645.Read(string, ushort)"/>
		public async override Task<OperateResult<byte[]>> ReadAsync( string address, ushort length ) => await Helper.DLT645Helper.ReadAsync( this, address, length );

		/// <inheritdoc cref="ReadDouble(string, ushort)"/>
		public async override Task<OperateResult<double[]>> ReadDoubleAsync( string address, ushort length ) => await Helper.DLT645Helper.ReadDoubleAsync( this, address, length );

		/// <inheritdoc/>
		public async override Task<OperateResult<string>> ReadStringAsync( string address, ushort length, Encoding encoding ) => ByteTransformHelper.GetResultFromArray( await ReadStringArrayAsync( address ) );

		/// <inheritdoc cref="DLT645.ReadStringArray(string)"/>
		public async Task<OperateResult<string[]>> ReadStringArrayAsync( string address ) => await Helper.DLT645Helper.ReadStringArrayAsync( this, address );
#endif
		/// <inheritdoc cref="DLT645.Write(string, byte[])"/>
		public override OperateResult Write( string address, byte[] value ) => Helper.DLT645Helper.Write( this, this.password, this.opCode, address, value );

		/// <inheritdoc cref="DLT645.ReadAddress"/>
		public OperateResult<string> ReadAddress( ) => Helper.DLT645Helper.ReadAddress( this );

		/// <inheritdoc cref="DLT645.WriteAddress(string)"/>
		public OperateResult WriteAddress(string address) => Helper.DLT645Helper.WriteAddress( this, address );

		/// <inheritdoc cref="DLT645.BroadcastTime(DateTime)"/>
		public OperateResult BroadcastTime( DateTime dateTime ) => Helper.DLT645Helper.BroadcastTime( this, dateTime );

		/// <inheritdoc cref="DLT645.FreezeCommand(string)"/>
		public OperateResult FreezeCommand( string dataArea ) => Helper.DLT645Helper.FreezeCommand( this, dataArea );

		/// <inheritdoc cref="DLT645.ChangeBaudRate(string)"/>
		public OperateResult ChangeBaudRate( string baudRate ) => Helper.DLT645Helper.ChangeBaudRate( this, baudRate );

#if !NET20 && !NET35
		/// <inheritdoc cref="Helper.DLT645Helper.Write(IDlt645,string, string, string, byte[])"/>
		public async override Task<OperateResult> WriteAsync( string address, byte[] value ) => await Helper.DLT645Helper.WriteAsync( this, this.password, this.opCode, address, value );

		/// <inheritdoc cref="Helper.DLT645Helper.ReadAddress(IDlt645)"/>
		public async Task<OperateResult<string>> ReadAddressAsync( ) => await Helper.DLT645Helper.ReadAddressAsync( this );

		/// <inheritdoc cref="Helper.DLT645Helper.WriteAddress(IDlt645,string)"/>
		public async Task<OperateResult> WriteAddressAsync( string address ) => await Helper.DLT645Helper.WriteAddressAsync( this, address );

		/// <inheritdoc cref="Helper.DLT645Helper.BroadcastTime(IDlt645,DateTime)"/>
		public async Task<OperateResult> BroadcastTimeAsync( DateTime dateTime ) => await Helper.DLT645Helper.BroadcastTimeAsync( this, dateTime, this.ReadFromCoreServerAsync );

		/// <inheritdoc cref="Helper.DLT645Helper.FreezeCommand(IDlt645,string)"/>
		public async Task<OperateResult> FreezeCommandAsync( string dataArea ) => await Helper.DLT645Helper.FreezeCommandAsync( this, dataArea );

		/// <inheritdoc cref="Helper.DLT645Helper.ChangeBaudRate(IDlt645,string)"/>
		public async Task<OperateResult> ChangeBaudRateAsync( string baudRate ) => await Helper.DLT645Helper.ChangeBaudRateAsync( this, baudRate );
#endif
		#endregion

		#region Public Property

		/// <inheritdoc cref="DLT645.Station"/>
		public string Station { get => this.station; set => this.station = value; }

		/// <inheritdoc cref="DLT645.EnableCodeFE"/>
		public bool EnableCodeFE { get; set; }

		/// <inheritdoc cref="IDlt645.DLTType"/>
		public DLT645Type DLTType { get; } = DLT645Type.DLT2007;

		#endregion

		#region Private Member

		private string station = "1";                  // 地址域信息
		private string password = "00000000";          // 密码
		private string opCode = "00000000";            // 操作者代码

		#endregion

		#region Object Override

		/// <inheritdoc/>
		public override string ToString() => $"DLT645OverTcp[{IpAddress}:{Port}]";

		#endregion

	}
}
