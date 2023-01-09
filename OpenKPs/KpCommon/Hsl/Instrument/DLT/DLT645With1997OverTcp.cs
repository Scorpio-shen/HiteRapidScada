using HslCommunication.BasicFramework;
using HslCommunication.Core;
using HslCommunication.Core.IMessage;
using HslCommunication.Core.Net;
using HslCommunication.Instrument.DLT.Helper;
using HslCommunication.Reflection;
using HslCommunication.Serial;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
#if !NET35 && !NET20
using System.Threading.Tasks;
#endif
namespace HslCommunication.Instrument.DLT
{
	/// <summary>
	/// 基于多功能电能表通信协议实现的通讯类，参考的文档是DLT645-1997，主要实现了对电表数据的读取和一些功能方法，数据标识格式为 B6-11，具体参照文档手册。<br />
	/// Based on the communication class implemented by the multi-function energy meter communication protocol, the reference document is DLT645-1997, 
	/// which mainly implements the reading of meter data and some functional methods, the data identification format is B6-11, please refer to the document manual for details.
	/// </summary>
	/// <remarks>
	/// 如果一对多的模式，地址可以携带地址域访问，例如 "s=2;B6-11"，主要使用 <see cref="ReadDouble(string, ushort)"/> 方法来读取浮点数，<see cref="SerialDeviceBase.ReadString(string, ushort)"/> 方法来读取字符串
	/// </remarks>
	/// <example>
	/// <inheritdoc cref="DLT645With1997" path="example"/>
	/// </example>
	public class DLT645With1997OverTcp : NetworkDeviceBase, IDlt645
	{
		#region Constructor

		/// <summary>
		/// 指定IP地址，端口，地址域来实例化一个对象<br />
		/// Specify the IP address, port, address field, password, and operator code to instantiate an object
		/// </summary>
		/// <param name="ipAddress">TcpServer的IP地址</param>
		/// <param name="port">TcpServer的端口</param>
		/// <param name="station">设备的站号信息</param>
		public DLT645With1997OverTcp( string ipAddress, int port = 502, string station = "1" )
		{
			this.IpAddress = ipAddress;
			this.Port = port;
			base.ByteTransform = new RegularByteTransform( );
			this.station = station;
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

		/// <inheritdoc cref="DLT645With1997.ActiveDeveice"/>
		public OperateResult ActiveDeveice( ) => ReadFromCoreServer( new byte[] { 0xFE, 0xFE, 0xFE, 0xFE }, false );

		/// <inheritdoc cref="DLT645With1997.Read(string, ushort)"/>
		[HslMqttApi( "ReadByteArray", "" )]
		public override OperateResult<byte[]> Read( string address, ushort length ) => Helper.DLT645Helper.Read( this, address, length );

		/// <inheritdoc cref="DLT645With1997.ReadDouble(string, ushort)"/>
		[HslMqttApi( "ReadDoubleArray", "" )]
		public override OperateResult<double[]> ReadDouble( string address, ushort length ) => Helper.DLT645Helper.ReadDouble( this, address, length );

		/// <inheritdoc cref="DLT645With1997.ReadString(string, ushort, Encoding)"/>
		public override OperateResult<string> ReadString( string address, ushort length, Encoding encoding ) => ByteTransformHelper.GetResultFromArray( ReadStringArray( address ) );

		/// <inheritdoc cref="DLT645With1997.ReadStringArray(string)"/>
		public OperateResult<string[]> ReadStringArray( string address ) => Helper.DLT645Helper.ReadStringArray( this, address );

#if !NET35 && !NET20

		/// <inheritdoc cref="DLT645With1997.ActiveDeveice"/>
		public async Task<OperateResult> ActiveDeveiceAsync( ) => await ReadFromCoreServerAsync( new byte[] { 0xFE, 0xFE, 0xFE, 0xFE }, hasResponseData: false, usePackAndUnpack: false );

		/// <inheritdoc cref="DLT645With1997.Read(string, ushort)"/>
		public async override Task<OperateResult<byte[]>> ReadAsync( string address, ushort length ) => await Helper.DLT645Helper.ReadAsync( this, address, length );

		/// <inheritdoc cref="ReadDouble(string, ushort)"/>
		public async override Task<OperateResult<double[]>> ReadDoubleAsync( string address, ushort length ) => await Helper.DLT645Helper.ReadDoubleAsync( this, address, length );

		/// <inheritdoc cref="ReadString(string, ushort, Encoding)"/>
		public async override Task<OperateResult<string>> ReadStringAsync( string address, ushort length, Encoding encoding ) => ByteTransformHelper.GetResultFromArray( await ReadStringArrayAsync( address ) );

		/// <inheritdoc cref="DLT645With1997.ReadStringArray(string)"/>
		public async Task<OperateResult<string[]>> ReadStringArrayAsync( string address ) => await Helper.DLT645Helper.ReadStringArrayAsync( this, address );
#endif
		/// <inheritdoc cref="DLT645With1997.Write(string, byte[])"/>
		public override OperateResult Write( string address, byte[] value ) => Helper.DLT645Helper.Write( this, "", "", address, value );

		/// <inheritdoc cref="DLT645With1997.WriteAddress(string)"/>
		public OperateResult WriteAddress( string address ) => Helper.DLT645Helper.WriteAddress( this, address );

		/// <inheritdoc cref="DLT645With1997.BroadcastTime(DateTime)"/>
		public OperateResult BroadcastTime( DateTime dateTime ) => Helper.DLT645Helper.BroadcastTime( this, dateTime );

		/// <inheritdoc cref="DLT645With1997.ChangeBaudRate(string)"/>
		public OperateResult ChangeBaudRate( string baudRate ) => Helper.DLT645Helper.ChangeBaudRate( this, baudRate );

#if !NET20 && !NET35
		/// <inheritdoc cref="DLT645With1997.Write(string, byte[])"/>
		public async override Task<OperateResult> WriteAsync( string address, byte[] value ) => await Helper.DLT645Helper.WriteAsync( this, "", "", address, value );

		/// <inheritdoc cref="DLT645With1997.WriteAddress(string)"/>
		public async Task<OperateResult> WriteAddressAsync( string address ) => await Helper.DLT645Helper.WriteAddressAsync( this, address );

		/// <inheritdoc cref="DLT645With1997.BroadcastTime(DateTime)"/>
		public async Task<OperateResult> BroadcastTimeAsync( DateTime dateTime ) => await Helper.DLT645Helper.BroadcastTimeAsync( this, dateTime, this.ReadFromCoreServerAsync );

		/// <inheritdoc cref="DLT645With1997.ChangeBaudRate(string)"/>
		public async Task<OperateResult> ChangeBaudRateAsync( string baudRate ) => await Helper.DLT645Helper.ChangeBaudRateAsync( this, baudRate );
#endif
		#endregion

		#region Public Property

		/// <inheritdoc cref="DLT645.Station"/>
		public string Station { get => this.station; set => this.station = value; }

		/// <inheritdoc cref="DLT645.EnableCodeFE"/>
		public bool EnableCodeFE { get; set; }

		/// <inheritdoc cref="IDlt645.DLTType"/>
		public DLT645Type DLTType { get; } = DLT645Type.DLT1997;

		#endregion

		#region Private Member

		private string station = "1";                  // 地址域信息

		#endregion
		#region Object Override
		/// <inheritdoc/>


		public override string ToString( ) => $"DLT645With1997OverTcp[{IpAddress}:{Port}]";

		#endregion

	}
}
