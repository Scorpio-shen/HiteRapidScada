using HslCommunication.Serial;
using HslCommunication.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HslCommunication.BasicFramework;
using HslCommunication.Reflection;
using HslCommunication.Profinet.Melsec.Helper;
using System.IO;

namespace HslCommunication.Profinet.Melsec
{
	/// <summary>
	/// 三菱计算机链接协议，适用FX3U系列，FX3G，FX3S等等系列，通常在PLC侧连接的是485的接线口<br />
	/// Mitsubishi Computer Link Protocol, suitable for FX3U series, FX3G, FX3S, etc., usually the 485 connection port is connected on the PLC side
	/// </summary>
	/// <remarks>
	/// 关于在PLC侧的配置信息，协议：专用协议  传送控制步骤：格式一  站号设置：0
	/// </remarks>
	/// <example>
	/// <inheritdoc cref="MelsecFxLinksOverTcp" path="example"/>
	/// </example>
	public class MelsecFxLinks : SerialDeviceBase, IReadWriteFxLinks
	{
		#region Constructor

		/// <inheritdoc cref="MelsecFxLinksOverTcp()"/>
		public MelsecFxLinks( )
		{
			ByteTransform = new RegularByteTransform( );
			WordLength = 1;
		}

		/// <inheritdoc/>
		public override byte[] PackCommandWithHeader( byte[] command ) => MelsecFxLinksHelper.PackCommandWithHeader( this, command );

		/// <inheritdoc/>
		protected override bool CheckReceiveDataComplete( MemoryStream ms )
		{
			byte[] data = ms.ToArray( );
			if (data.Length < 5) return false;
			if (Format == 1)
			{
				if (data[0] == AsciiControl.NAK) return data.Length == 7;
				if (data[0] == AsciiControl.ACK) return data.Length == 5;
				if (data[0] == AsciiControl.STX)
				{
					if (SumCheck)
						return data[data.Length - 3] == AsciiControl.ETX;
					else
						return data[data.Length - 1] == AsciiControl.ETX;
				}
				return false;
			}
			else if (Format == 4)
			{
				return data[data.Length - 1] == AsciiControl.LF && data[data.Length - 2] == AsciiControl.CR;
			}
			return false;
		}

		#endregion

		#region Public Member

		/// <inheritdoc cref="MelsecFxLinksOverTcp.Station"/>
		public byte Station { get => station; set => station = value; }

		/// <inheritdoc cref="MelsecFxLinksOverTcp.WaittingTime"/>
		public byte WaittingTime
		{
			get => watiingTime;
			set
			{
				if (watiingTime > 0x0F)
				{
					watiingTime = 0x0F;
				}
				else
				{
					watiingTime = value;
				}
			}
		}

		/// <inheritdoc cref="MelsecFxLinksOverTcp.SumCheck"/>
		public bool SumCheck { get => sumCheck; set => sumCheck = value; }

		/// <inheritdoc cref="MelsecFxLinksOverTcp.Format"/>
		public int Format { get; set; } = 1;

		#endregion

		#region Read Write Support

		/// <inheritdoc cref="MelsecFxLinksOverTcp.Read(string, ushort)"/>
		[HslMqttApi( "ReadByteArray", "" )]
		public override OperateResult<byte[]> Read( string address, ushort length ) => MelsecFxLinksHelper.Read( this, address, length );

		/// <inheritdoc cref="MelsecFxLinksOverTcp.Write(string, byte[])"/>
		[HslMqttApi( "WriteByteArray", "" )]
		public override OperateResult Write( string address, byte[] value ) => MelsecFxLinksHelper.Write( this, address, value );

		#endregion

		#region Bool Read Write

		/// <inheritdoc cref="MelsecFxLinksOverTcp.ReadBool(string, ushort)"/>
		[HslMqttApi( "ReadBoolArray", "" )]
		public override OperateResult<bool[]> ReadBool( string address, ushort length ) => MelsecFxLinksHelper.ReadBool( this, address, length );

		/// <inheritdoc cref="MelsecFxLinksOverTcp.Write(string, bool[])"/>
		[HslMqttApi( "WriteBoolArray", "" )]
		public override OperateResult Write( string address, bool[] value ) => MelsecFxLinksHelper.Write( this, address, value );

		#endregion

		#region Start Stop

		/// <inheritdoc cref="MelsecFxLinksOverTcp.StartPLC(string)"/>
		[HslMqttApi( Description = "Start the PLC operation, you can carry additional parameter information and specify the station number. Example: s=2; Note: The semicolon is required." )]
		public OperateResult StartPLC( string parameter = "" ) => MelsecFxLinksHelper.StartPLC( this, parameter );

		/// <inheritdoc cref="MelsecFxLinksOverTcp.StopPLC(string)"/>
		[HslMqttApi( Description = "Stop PLC operation, you can carry additional parameter information and specify the station number. Example: s=2; Note: The semicolon is required." )]
		public OperateResult StopPLC( string parameter = "" ) => MelsecFxLinksHelper.StopPLC( this, parameter );

		/// <inheritdoc cref="MelsecFxLinksOverTcp.ReadPlcType(string)"/>
		[HslMqttApi( Description = "Read the PLC model information, you can carry additional parameter information, and specify the station number. Example: s=2; Note: The semicolon is required." )]
		public OperateResult<string> ReadPlcType( string parameter = "" ) => MelsecFxLinksHelper.ReadPlcType( this, parameter );

		#endregion

		#region Private Member

		private byte station = 0x00;                 // PLC的站号信息
		private byte watiingTime = 0x00;             // 报文的等待时间，设置为0-15
		private bool sumCheck = true;                // 是否启用和校验

		#endregion

		#region Object Override

		/// <inheritdoc/>
		public override string ToString( ) => $"MelsecFxLinks[{PortName}:{BaudRate}]";

		#endregion
	}
}
