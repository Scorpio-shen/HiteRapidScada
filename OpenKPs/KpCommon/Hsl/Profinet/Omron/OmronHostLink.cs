using HslCommunication.BasicFramework;
using HslCommunication.Core;
using HslCommunication.Serial;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HslCommunication.Reflection;
using System.IO;

namespace HslCommunication.Profinet.Omron
{
	/// <summary>
	/// 欧姆龙的HostLink协议的实现，地址支持示例 DM区:D100; CIO区:C100; Work区:W100; Holding区:H100; Auxiliary区: A100<br />
	/// Implementation of Omron's HostLink protocol, address support example DM area: D100; CIO area: C100; Work area: W100; Holding area: H100; Auxiliary area: A100
	/// </summary>
	/// <remarks>
	/// 感谢 深圳～拾忆 的测试，地址可以携带站号信息，例如 s=2;D100 
	/// <br />
	/// <note type="important">
	/// 如果发现串口线和usb同时打开才能通信的情况，需要按照如下的操作：<br />
	/// 串口线不是标准的串口线，电脑的串口线的235引脚分别接PLC的329引脚，45线短接，就可以通讯，感谢 深圳-小君(QQ932507362)提供的解决方案。
	/// </note>
	/// </remarks>
	/// <example>
	/// <inheritdoc cref="OmronHostLinkOverTcp" path="example"/>
	/// </example>
	public class OmronHostLink : SerialDeviceBase, Helper.IHostLink
	{
		#region Constructor

		/// <inheritdoc cref="OmronFinsNet()"/>
		public OmronHostLink( )
		{
			this.ByteTransform                         = new ReverseWordTransform( );
			this.WordLength                            = 1;
			this.ByteTransform.DataFormat              = DataFormat.CDAB;
			this.ByteTransform.IsStringReverseByteWord = true;
			this.LogMsgFormatBinary                    = false;
			this.ReceiveEmptyDataCount                 = 5;         // 多接收5个周期的空白数据才认为是结束，防止数据不完整即返回
		}

		#endregion

		#region Public Member

		/// <inheritdoc cref="OmronHostLinkOverTcp.ICF"/>
		public byte ICF { get; set; } = 0x00;

		/// <inheritdoc cref="OmronHostLinkOverTcp.DA2"/>
		public byte DA2 { get; set; } = 0x00;

		/// <inheritdoc cref="OmronHostLinkOverTcp.SA2"/>
		public byte SA2 { get; set; }

		/// <inheritdoc cref="OmronHostLinkOverTcp.SID"/>
		public byte SID { get; set; } = 0x00;

		/// <inheritdoc cref="OmronHostLinkOverTcp.ResponseWaitTime"/>
		public byte ResponseWaitTime { get; set; } = 0x30;

		/// <inheritdoc cref="OmronHostLinkOverTcp.UnitNumber"/>
		public byte UnitNumber { get; set; }

		/// <inheritdoc cref="OmronHostLinkOverTcp.ReadSplits"/>
		public int ReadSplits { get; set; } = 260;

		#endregion

		#region Pack Unpack Override

		/// <inheritdoc/>
		public override OperateResult<byte[]> UnpackResponseContent( byte[] send, byte[] response )
		{
			return Helper.OmronHostLinkHelper.ResponseValidAnalysis( send, response );
		}

		/// <inheritdoc/>
		protected override bool CheckReceiveDataComplete( MemoryStream ms )
		{
			byte[] buffer = ms.ToArray( );
			if (buffer.Length > 1) return buffer[buffer.Length - 1] == 0x0D;
			return false;
		}

		/// <summary>
		/// 初始化串口信息，9600波特率，7位数据位，1位停止位，偶校验<br />
		/// Initial serial port information, 9600 baud rate, 7 data bits, 1 stop bit, even parity
		/// </summary>
		/// <param name="portName">端口号信息，例如"COM3"</param>
		public override void SerialPortInni( string portName )
		{
			base.SerialPortInni( portName );
		}

		/// <summary>
		/// 初始化串口信息，波特率，7位数据位，1位停止位，偶校验<br />
		/// Initializes serial port information, baud rate, 7-bit data bit, 1-bit stop bit, even parity
		/// </summary>
		/// <param name="portName">端口号信息，例如"COM3"</param>
		/// <param name="baudRate">波特率</param>
		public override void SerialPortInni( string portName, int baudRate )
		{
			base.SerialPortInni( portName, baudRate, 7, System.IO.Ports.StopBits.One, System.IO.Ports.Parity.Even);
		}
		#endregion

		#region Read Write Support

		/// <inheritdoc cref="OmronFinsNet.Read(string, ushort)"/>
		[HslMqttApi( "ReadByteArray", "" )]
		public override OperateResult<byte[]> Read( string address, ushort length ) => Helper.OmronHostLinkHelper.Read( this, address, length );

		/// <inheritdoc cref="OmronFinsNet.Write(string, byte[])"/>
		[HslMqttApi( "WriteByteArray", "" )]
		public override OperateResult Write( string address, byte[] value ) => Helper.OmronHostLinkHelper.Write( this, address, value );

		/// <inheritdoc cref="Helper.OmronHostLinkHelper.Read(Helper.IHostLink, string[])"/>
		public OperateResult<byte[]> Read( string[] address ) => Helper.OmronHostLinkHelper.Read( this, address );

		#endregion

		#region Bool Read Write

		/// <inheritdoc cref="OmronFinsNet.ReadBool(string, ushort)"/>
		[HslMqttApi( "ReadBoolArray", "" )]
		public override OperateResult<bool[]> ReadBool( string address, ushort length ) => Helper.OmronHostLinkHelper.ReadBool( this, address, length );

		/// <inheritdoc cref="OmronFinsNet.Write(string, bool[])"/>
		[HslMqttApi( "WriteBoolArray", "" )]
		public override OperateResult Write( string address, bool[] values ) => Helper.OmronHostLinkHelper.Write( this, address, values );

		#endregion

		#region Object Override

		/// <inheritdoc/>
		public override string ToString( ) => $"OmronHostLink[{PortName}:{BaudRate}]";

		#endregion

	}
}
