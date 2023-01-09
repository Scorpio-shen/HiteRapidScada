using HslCommunication.Core;
using HslCommunication.Serial;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HslCommunication.Reflection;
using System.IO;
#if !NET35 && !NET20
using System.Threading.Tasks;
#endif

namespace HslCommunication.Profinet.Panasonic
{
	/// <summary>
	/// 松下PLC的数据交互协议，采用Mewtocol协议通讯，支持的地址列表参考api文档<br />
	/// The data exchange protocol of Panasonic PLC adopts Mewtocol protocol for communication. For the list of supported addresses, refer to the api document.
	/// </summary>
	/// <remarks>
	/// 地址支持携带站号的访问方式，例如：s=2;D100
	/// </remarks>
	/// <example>
	/// <inheritdoc cref="PanasonicMewtocolOverTcp" path="example"/>
	/// </example>
	public class PanasonicMewtocol : SerialDeviceBase
	{
		#region Constructor

		/// <inheritdoc cref="PanasonicMewtocolOverTcp(byte)"/>
		public PanasonicMewtocol( byte station = 238 )
		{
			this.ByteTransform            = new RegularByteTransform( );
			this.Station                  = station;
			this.ByteTransform.DataFormat = DataFormat.DCBA;
			this.WordLength               = 1;
			this.ReceiveEmptyDataCount    = 5;
		}

		#endregion

		#region Public Properties
		
		/// <inheritdoc cref="PanasonicMewtocolOverTcp.Station"/>
		public byte Station { get; set; }

		/// <inheritdoc/>
		protected override bool CheckReceiveDataComplete( MemoryStream ms )
		{
			byte[] buffer = ms.ToArray( );
			if (buffer.Length > 5) return buffer[buffer.Length - 1] == 0x0D;
			return false;
		}
		#endregion

		#region Read Write Override

		/// <inheritdoc cref="PanasonicMewtocolOverTcp.Read(string, ushort)"/>
		[HslMqttApi( "ReadByteArray", "" )]
		public override OperateResult<byte[]> Read( string address, ushort length ) => Helper.MewtocolHelper.Read( this, this.Station, address, length );

		/// <inheritdoc cref="PanasonicMewtocolOverTcp.Write(string, byte[])"/>
		[HslMqttApi( "WriteByteArray", "" )]
		public override OperateResult Write( string address, byte[] value ) => Helper.MewtocolHelper.Write( this, this.Station, address, value );

		#endregion

		#region Read Write Bool

		/// <inheritdoc cref="PanasonicMewtocolOverTcp.ReadBool(string, ushort)"/>
		[HslMqttApi( "ReadBoolArray", "" )]
		public override OperateResult<bool[]> ReadBool( string address, ushort length ) => Helper.MewtocolHelper.ReadBool( this, this.Station, address, length );

		/// <inheritdoc cref="Helper.MewtocolHelper.ReadBool(IReadWriteDevice, byte, string[])"/>
		public OperateResult<bool[]> ReadBool( string[] address ) => Helper.MewtocolHelper.ReadBool( this, this.Station, address );

		/// <inheritdoc cref="PanasonicMewtocolOverTcp.ReadBool(string)"/>
		[HslMqttApi( "ReadBool", "" )]
		public override OperateResult<bool> ReadBool( string address ) => Helper.MewtocolHelper.ReadBool( this, this.Station, address );

		/// <inheritdoc cref="PanasonicMewtocolOverTcp.Write(string, bool[])"/>
		[HslMqttApi( "WriteBoolArray", "" )]
		public override OperateResult Write( string address, bool[] values ) => Helper.MewtocolHelper.Write( this, this.Station, address, values );

		/// <inheritdoc cref="PanasonicMewtocolOverTcp.Write(string, bool)"/>
		[HslMqttApi( "WriteBool", "" )]
		public override OperateResult Write( string address, bool value ) => Helper.MewtocolHelper.Write( this, this.Station, address, value );

		/// <inheritdoc cref="Helper.MewtocolHelper.Write(IReadWriteDevice, byte, string[], bool[])"/>
		public OperateResult Write( string[] address, bool[] value ) => Helper.MewtocolHelper.Write( this, this.Station, address, value );

		#endregion

		#region Async Read Write Bool
#if !NET35 && !NET20
		/// <inheritdoc cref="ReadBool(string)"/>
		public async override Task<OperateResult<bool>> ReadBoolAsync( string address ) => await Task.Run( ( ) => ReadBool( address ) );

		/// <inheritdoc cref="Write(string, bool)"/>
		public async override Task<OperateResult> WriteAsync( string address, bool value ) => await Task.Run( ( ) => Write( address, value ) );

		/// <inheritdoc cref="ReadBool(string[])"/>
		public async Task<OperateResult<bool[]>> ReadBoolAsync(string[] address ) => await Helper.MewtocolHelper.ReadBoolAsync( this, this.Station, address );

		/// <inheritdoc cref="Write(string[], bool[])"/>
		public async Task<OperateResult> WriteAsync(string[] address, bool[] value) => await Helper.MewtocolHelper.WriteAsync( this, this.Station, address, value );
#endif
		#endregion

		#region Object Override

		/// <inheritdoc/>
		public override string ToString( ) => $"PanasonicMewtocol[{PortName}:{BaudRate}]";

		#endregion
	}
}
