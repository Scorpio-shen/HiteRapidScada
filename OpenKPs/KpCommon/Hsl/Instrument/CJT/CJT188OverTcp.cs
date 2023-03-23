using HslCommunication.Instrument.CJT.Helper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using HslCommunication.Core;
using HslCommunication.Core.Net;
using HslCommunication.Core.IMessage;
using HslCommunication.BasicFramework;
using HslCommunication.Reflection;
using System.Net.Sockets;
#if !NET20 && !NET35
using System.Threading.Tasks;
#endif

namespace HslCommunication.Instrument.CJT
{
	/// <summary>
	/// CJT188串口透传协议
	/// </summary>
	public class CJT188OverTcp : NetworkDeviceBase, ICjt188
	{
		#region Constructor

		/// <summary>
		/// 指定地址域来实例化一个对象，密码及操作者代码在写入操作的时候进行验证<br />
		/// Specify the address field to instantiate an object, and the password and operator code are validated during write operations, 
		/// which address field is a 14-character BCD code, for example: 14910000729011
		/// </summary>
		/// <param name="station">设备的地址信息，是一个14字符的BCD码</param>
		public CJT188OverTcp( string station )
		{
			this.ByteTransform = new RegularByteTransform( );
			this.station = station;
		}

		/// <inheritdoc/>
		protected override INetMessage GetNewNetMessage( ) => new CJT188Message( );

		/// <inheritdoc/>
		public override byte[] PackCommandWithHeader( byte[] command )
		{
			if (EnableCodeFE)
				return SoftBasic.SpliceArray( new byte[] { 0xfe, 0xfe }, command );
			return base.PackCommandWithHeader( command );
		}

		/// <inheritdoc/>
		protected override OperateResult<byte[]> ReceiveByMessage( Socket socket, int timeOut, INetMessage netMessage, Action<long, long> reportProgress = null )
		{
			DateTime startTime = DateTime.Now;        // 加入超时，防止无限循环接收
			while (true)
			{
				OperateResult<byte[]> read = base.ReceiveByMessage( socket, timeOut, netMessage, reportProgress );
				if (!read.IsSuccess) return read;

				if (!StationMatch) return read;
				string station = read.Content.SelectMiddle( 2, 7 ).Reverse( ).ToArray( ).ToHexString( );

				if (this.Station == "AAAAAAAAAAAAAA" || station == "AAAAAAAAAAAAAA" || this.Station == station) return read;

				// 如果还需要接收的话，就显示下报文信息
				LogNet?.WriteDebug( ToString( ), StringResources.Language.Receive + " : " + (LogMsgFormatBinary ? read.Content.ToHexString( ' ' ) : SoftBasic.GetAsciiStringRender( read.Content )) );

				// 一直不完整的话，引发超时
				if (ReceiveTimeOut > 0 && (DateTime.Now - startTime).TotalMilliseconds > ReceiveTimeOut)
					return new OperateResult<byte[]>( StringResources.Language.ReceiveDataTimeout + ReceiveTimeOut );
			}
		}
#if !NET20 && !NET35
		/// <inheritdoc/>
		protected override async Task<OperateResult<byte[]>> ReceiveByMessageAsync( Socket socket, int timeOut, INetMessage netMessage, Action<long, long> reportProgress = null )
		{
			DateTime startTime = DateTime.Now;        // 加入超时，防止无限循环接收
			while (true)
			{
				OperateResult<byte[]> read = await base.ReceiveByMessageAsync( socket, timeOut, netMessage, reportProgress );
				if (!read.IsSuccess) return read;

				if (!StationMatch) return read;
				string station = read.Content.SelectMiddle( 2, 7 ).Reverse( ).ToArray( ).ToHexString( );

				if (this.Station == "AAAAAAAAAAAAAA" || station == "AAAAAAAAAAAAAA" || this.Station == station) return read;

				// 如果还需要接收的话，就显示下报文信息
				LogNet?.WriteDebug( ToString( ), StringResources.Language.Receive + " : " + (LogMsgFormatBinary ? read.Content.ToHexString( ' ' ) : SoftBasic.GetAsciiStringRender( read.Content )) );

				// 一直不完整的话，引发超时
				if (ReceiveTimeOut > 0 && (DateTime.Now - startTime).TotalMilliseconds > ReceiveTimeOut)
					return new OperateResult<byte[]>( StringResources.Language.ReceiveDataTimeout + ReceiveTimeOut );
			}
		}
#endif
		#endregion

		#region Public Method

		/// <summary>
		/// 激活设备的命令，只发送数据到设备，不等待设备数据返回<br />
		/// The command to activate the device, only send data to the device, do not wait for the device data to return
		/// </summary>
		/// <returns>是否发送成功</returns>
		public OperateResult ActiveDeveice( ) => ReadFromCoreServer( new byte[] { 0xFE, 0xFE }, hasResponseData: false, usePackAndUnpack: false );

		/// <inheritdoc cref="Helper.CJT188Helper.Read(ICjt188, string, ushort)"/>
		[HslMqttApi( "ReadByteArray", "" )]
		public override OperateResult<byte[]> Read( string address, ushort length ) => Helper.CJT188Helper.Read( this, address, length );

		/// <inheritdoc cref="Helper.CJT188Helper.Write(ICjt188, string, byte[])"/>
		[HslMqttApi( "WriteByteArray", "" )]
		public override OperateResult Write( string address, byte[] value ) => Helper.CJT188Helper.Write( this, address, value );

		/// <inheritdoc/>
		[HslMqttApi( "ReadFloatArray", "" )]
		public override OperateResult<float[]> ReadFloat( string address, ushort length ) => Helper.CJT188Helper.ReadValue( this, address, length, float.Parse );

		/// <inheritdoc/>
		[HslMqttApi( "ReadDoubleArray", "" )]
		public override OperateResult<double[]> ReadDouble( string address, ushort length ) => Helper.CJT188Helper.ReadValue( this, address, length, double.Parse );

		/// <inheritdoc/>
		public override OperateResult<string> ReadString( string address, ushort length, Encoding encoding ) => ByteTransformHelper.GetResultFromArray( ReadStringArray( address ) );

		/// <inheritdoc cref="Helper.CJT188Helper.ReadStringArray(ICjt188, string)"/>
		public OperateResult<string[]> ReadStringArray( string address ) => Helper.CJT188Helper.ReadStringArray( this, address );

#if !NET35 && !NET20

		/// <inheritdoc cref="ReadFloat(string, ushort)"/>
		public async override Task<OperateResult<float[]>> ReadFloatAsync( string address, ushort length ) => await Task.Run( ( ) => ReadFloat( address, length ) );

		/// <inheritdoc cref="ReadDouble(string, ushort)"/>
		public async override Task<OperateResult<double[]>> ReadDoubleAsync( string address, ushort length ) => await Task.Run( ( ) => ReadDouble( address, length ) );

		/// <inheritdoc/>
		public async override Task<OperateResult<string>> ReadStringAsync( string address, ushort length, Encoding encoding ) => await Task.Run( ( ) => ReadString( address, length, encoding ) );
#endif

		/// <inheritdoc cref="CJT188Helper.WriteAddress(ICjt188, string)"/>
		public OperateResult WriteAddress( string address ) => Helper.CJT188Helper.WriteAddress( this, address );

		/// <inheritdoc cref="CJT188Helper.ReadAddress(ICjt188)"/>
		public OperateResult<string> ReadAddress( ) => Helper.CJT188Helper.ReadAddress( this );

		#endregion

		#region Public Property

		/// <summary>
		/// 仪表的类型
		/// </summary>
		public byte InstrumentType { get; set; }

		/// <inheritdoc cref="DLT.Helper.IDlt645.Station"/>
		public string Station { get => this.station; set => this.station = value; }

		/// <inheritdoc cref="DLT.Helper.IDlt645.EnableCodeFE"/>
		public bool EnableCodeFE { get; set; } = true;

		/// <summary>
		/// 获取或设置是否验证匹配接收到的站号信息<br />
		/// Gets or sets whether to verify that the received station number information is matched
		/// </summary>
		public bool StationMatch { get; set; } = false;

		#endregion

		#region Private Member

		private string station = "1";                  // 地址域信息
		private string password = "00000000";          // 密码
		private string opCode = "00000000";            // 操作者代码

		#endregion

		#region Object Override

		/// <inheritdoc/>
		public override string ToString( ) => $"CJT188OverTcp[{IpAddress}:{Port}]";

		#endregion
	}
}
