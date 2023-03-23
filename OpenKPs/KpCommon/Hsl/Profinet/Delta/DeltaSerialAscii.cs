using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HslCommunication.ModBus;
using HslCommunication.Reflection;
using HslCommunication.Core;
using HslCommunication.Profinet.Delta.Helper;
#if !NET20 && !NET35
using System.Threading.Tasks;
#endif

namespace HslCommunication.Profinet.Delta
{
	/// <summary>
	/// 台达PLC的串口通讯类，基于Modbus-Ascii协议开发，按照台达的地址进行实现。<br />
	/// The serial communication class of Delta PLC is developed based on the Modbus-Ascii protocol and implemented according to Delta's address.
	/// </summary>
	/// <remarks>
	/// 适用于DVP-ES/EX/EC/SS型号，DVP-SA/SC/SX/EH型号以及AS300型号，地址参考API文档，同时地址可以携带站号信息，举例：[s=2;D100],[s=3;M100]，可以动态修改当前报文的站号信息。<br />
	/// Suitable for DVP-ES/EX/EC/SS models, DVP-SA/SC/SX/EH models and AS300 model, the address refers to the API document, and the address can carry station number information,
	/// for example: [s=2;D100],[s= 3;M100], you can dynamically modify the station number information of the current message.
	/// </remarks>
	/// <example>
	/// <inheritdoc cref="DeltaSerial" path="example"/>
	/// </example>
	public class DeltaSerialAscii : ModbusAscii, IDelta
	{
		#region Constructor

		/// <inheritdoc cref="DeltaSerial()"/>
		public DeltaSerialAscii( ) : base( ) { ByteTransform.DataFormat = DataFormat.CDAB; }

		/// <inheritdoc cref="DeltaSerial(byte)"/>
		public DeltaSerialAscii( byte station = 0x01 ) : base( station ) { ByteTransform.DataFormat = DataFormat.CDAB; }

		#endregion

		#region Public Properties

		/// <inheritdoc cref="IDelta.Series"/>
		public DeltaSeries Series { get; set; } = DeltaSeries.Dvp;

		#endregion

		#region Override

		/// <inheritdoc/>
		public override OperateResult<string> TranslateToModbusAddress( string address, byte modbusCode ) => DeltaHelper.TranslateToModbusAddress( this, address, modbusCode );

		#endregion

		#region ReadWrite

		/// <inheritdoc cref="DeltaTcpNet.ReadBool(string, ushort)"/>
		public override OperateResult<bool[]> ReadBool( string address, ushort length ) => DeltaHelper.ReadBool( this, base.ReadBool, address, length );

		/// <inheritdoc cref="DeltaTcpNet.Write(string, bool[])"/>
		public override OperateResult Write( string address, bool[] values ) => DeltaHelper.Write( this, base.Write, address, values );

		/// <inheritdoc cref="DeltaTcpNet.Read(string, ushort)"/>
		public override OperateResult<byte[]> Read( string address, ushort length ) => DeltaHelper.Read( this, base.Read, address, length );

		/// <inheritdoc cref="DeltaTcpNet.Write(string, byte[])"/>
		public override OperateResult Write( string address, byte[] value ) => DeltaHelper.Write( this, base.Write, address, value );

		#endregion

		#region Object Override

		/// <inheritdoc/>
		public override string ToString( ) => $"DeltaSerialAscii[{PortName}:{BaudRate}]";

		#endregion
	}
}
