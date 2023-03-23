using HslCommunication.Core;
using HslCommunication.Serial;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HslCommunication.BasicFramework;
using HslCommunication.Reflection;
using HslCommunication.Profinet.FATEK.Helper;
using System.IO;

namespace HslCommunication.Profinet.FATEK
{
	/// <summary>
	/// 台湾永宏公司的编程口协议，具体的地址信息请查阅api文档信息，地址允许携带站号信息，例如：s=2;D100<br />
	/// The programming port protocol of Taiwan Yonghong company, 
	/// please refer to the api document for specific address information, The address can carry station number information, such as s=2;D100
	/// </summary>
	/// <remarks>
	/// <inheritdoc cref="FatekProgramOverTcp" path="remarks"/>
	/// </remarks>
	/// <example>
	/// <inheritdoc cref="FatekProgramOverTcp" path="example"/>
	/// </example>
	public class FatekProgram : SerialDeviceBase
	{
		#region Constructor

		/// <inheritdoc cref="FatekProgramOverTcp( )"/>
		public FatekProgram( )
		{
			this.ByteTransform         = new RegularByteTransform( );
			this.WordLength            = 1;
			this.LogMsgFormatBinary    = false;
			this.ReceiveEmptyDataCount = 5;
		}

		/// <inheritdoc/>
		protected override bool CheckReceiveDataComplete( MemoryStream ms ) => FatekProgramHelper.CheckReceiveDataComplete( ms );

		#endregion

		#region Public Member

		/// <inheritdoc cref="FatekProgramOverTcp.Station"/>
		public byte Station { get => station; set => station = value; }

		#endregion

		#region Read Write Support

		/// <inheritdoc cref="FatekProgramHelper.Read(IReadWriteDevice, byte, string, ushort)"/>
		[HslMqttApi( "ReadByteArray", "" )]
		public override OperateResult<byte[]> Read( string address, ushort length ) => FatekProgramHelper.Read( this, this.station, address, length );

		/// <inheritdoc cref="FatekProgramHelper.Write(IReadWriteDevice, byte, string, byte[])"/>
		[HslMqttApi( "WriteByteArray", "" )]
		public override OperateResult Write( string address, byte[] value ) => FatekProgramHelper.Write( this, this.station, address, value );

		#endregion

		#region Bool Read Write

		/// <inheritdoc cref="FatekProgramHelper.ReadBool(IReadWriteDevice, byte, string, ushort)"/>
		[HslMqttApi( "ReadBoolArray", "" )]
		public override OperateResult<bool[]> ReadBool( string address, ushort length ) => FatekProgramHelper.ReadBool( this, this.station, address, length );

		/// <inheritdoc cref="FatekProgramHelper.Write(IReadWriteDevice, byte, string, bool[])"/>
		[HslMqttApi( "WriteBoolArray", "" )]
		public override OperateResult Write( string address, bool[] value ) => FatekProgramHelper.Write( this, this.station, address, value );

		#endregion

		#region Public Method

		/// <inheritdoc cref="FatekProgramHelper.Run(IReadWriteDevice, byte)"/>
		public OperateResult Run( byte station ) => FatekProgramHelper.Run( this, station );

		/// <inheritdoc cref="Run(byte)"/>
		[HslMqttApi( "Run", "使PLC处于RUN状态" )]
		public OperateResult Run( ) => Run( this.Station );

		/// <inheritdoc cref="FatekProgramHelper.Stop(IReadWriteDevice, byte)"/>
		public OperateResult Stop( byte station ) => FatekProgramHelper.Stop( this, station );

		/// <inheritdoc cref="Stop(byte)"/>
		[HslMqttApi( "Stop", "使PLC处于STOP状态" )]
		public OperateResult Stop( ) => Stop( this.Station );

		/// <inheritdoc cref="FatekProgramHelper.ReadStatus(IReadWriteDevice, byte)"/>
		public OperateResult<bool[]> ReadStatus( byte station ) => FatekProgramHelper.ReadStatus( this, station );

		/// <inheritdoc cref="ReadStatus(byte)"/>
		[HslMqttApi( "ReadStatus", "读取PLC基本的状态信息" )]
		public OperateResult<bool[]> ReadStatus( ) => ReadStatus( this.Station );

		#endregion

		#region Object Override

		/// <inheritdoc/>
		public override string ToString( ) => $"FatekProgram[{PortName}:{BaudRate}]";

		#endregion

		#region Private Member

		private byte station = 0x01;                 // PLC的站号信息

		#endregion
	}
}
