using HslCommunication.BasicFramework;
using HslCommunication.Core;
using HslCommunication.Core.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HslCommunication.Reflection;
using System.Net.Sockets;
using HslCommunication.Core.IMessage;
#if !NET35 && !NET20
using System.Threading.Tasks;
#endif

namespace HslCommunication.Profinet.Omron
{
	/// <summary>
	/// 欧姆龙的HostLink协议的实现，基于Tcp实现，地址支持示例 DM区:D100; CIO区:C100; Work区:W100; Holding区:H100; Auxiliary区: A100<br />
	/// Implementation of Omron's HostLink protocol, based on tcp protocol, address support example DM area: D100; CIO area: C100; Work area: W100; Holding area: H100; Auxiliary area: A100
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
	/// 欧姆龙的地址参考如下：
	/// 地址支持的列表如下：
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
	///     <term>DM Area</term>
	///     <term>D</term>
	///     <term>D100,D200</term>
	///     <term>10</term>
	///     <term>√</term>
	///     <term>√</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>CIO Area</term>
	///     <term>C</term>
	///     <term>C100,C200</term>
	///     <term>10</term>
	///     <term>√</term>
	///     <term>√</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>Work Area</term>
	///     <term>W</term>
	///     <term>W100,W200</term>
	///     <term>10</term>
	///     <term>√</term>
	///     <term>√</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>Holding Bit Area</term>
	///     <term>H</term>
	///     <term>H100,H200</term>
	///     <term>10</term>
	///     <term>√</term>
	///     <term>√</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>Auxiliary Bit Area</term>
	///     <term>A</term>
	///     <term>A100,A200</term>
	///     <term>10</term>
	///     <term>√</term>
	///     <term>√</term>
	///     <term></term>
	///   </item>
	/// </list>
	/// </example>
	public class OmronHostLinkOverTcp : NetworkDeviceBase, Helper.IHostLink
	{
		#region Constructor

		/// <inheritdoc cref="OmronFinsNet()"/>
		public OmronHostLinkOverTcp( )
		{
			this.ByteTransform            = new ReverseWordTransform( );
			this.WordLength               = 1;
			this.ByteTransform.DataFormat = DataFormat.CDAB;
			//this.SleepTime                = 20;
			this.LogMsgFormatBinary       = false;
		}

		/// <inheritdoc cref="OmronCipNet(string,int)"/>
		public OmronHostLinkOverTcp( string ipAddress, int port ) : this( ) 
		{ 
			this.IpAddress = ipAddress; 
			this.Port      = port; 
		}

		/// <inheritdoc/>
		protected override INetMessage GetNewNetMessage( ) => new SpecifiedCharacterMessage( AsciiControl.CR );

		#endregion

		#region Pack Unpack Override

		/// <inheritdoc/>
		public override OperateResult<byte[]> UnpackResponseContent( byte[] send, byte[] response )
		{
			return Helper.OmronHostLinkHelper.ResponseValidAnalysis( send, response );
		}

		#endregion

		#region Public Member

		/// <summary>
		/// Specifies whether or not there are network relays. Set “80” (ASCII: 38,30) 
		/// when sending an FINS command to a CPU Unit on a network.Set “00” (ASCII: 30,30) 
		/// when sending to a CPU Unit connected directly to the host computer.
		/// </summary>
		public byte ICF { get; set; } = 0x00;

		/// <inheritdoc cref="OmronFinsNet.DA2"/>
		public byte DA2 { get; set; } = 0x00;

		/// <inheritdoc cref="OmronFinsNet.SA2"/>
		public byte SA2 { get; set; }

		/// <inheritdoc cref="OmronFinsNet.SID"/>
		public byte SID { get; set; } = 0x00;

		/// <summary>
		/// The response wait time sets the time from when the CPU Unit receives a command block until it starts 
		/// to return a response.It can be set from 0 to F in hexadecimal, in units of 10 ms.
		/// If F(15) is set, the response will begin to be returned 150 ms (15 × 10 ms) after the command block was received.
		/// </summary>
		public byte ResponseWaitTime { get; set; } = 0x30;

		/// <summary>
		/// PLC设备的站号信息<br />
		/// PLC device station number information
		/// </summary>
		public byte UnitNumber { get; set; }

		/// <summary>
		/// 进行字读取的时候对于超长的情况按照本属性进行切割，默认260。<br />
		/// When reading words, it is cut according to this attribute for the case of overlength. The default is 260.
		/// </summary>
		public int ReadSplits { get; set; } = 260;

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

		#region Async Read Write Support
#if !NET35 && !NET20
		/// <inheritdoc cref="OmronFinsNet.Read(string, ushort)"/>
		public override async Task<OperateResult<byte[]>> ReadAsync( string address, ushort length ) => await Helper.OmronHostLinkHelper.ReadAsync( this, address, length );

		/// <inheritdoc cref="OmronFinsNet.Write(string, byte[])"/>
		public override async Task<OperateResult> WriteAsync( string address, byte[] value ) => await Helper.OmronHostLinkHelper.WriteAsync( this, address, value );

		/// <inheritdoc cref="Helper.OmronHostLinkHelper.Read(Helper.IHostLink, string[])"/>
		public async Task<OperateResult<byte[]>> ReadAsync( string[] address ) => await Helper.OmronHostLinkHelper.ReadAsync( this, address );

#endif
		#endregion

		#region Read Write Bool

		/// <inheritdoc cref="OmronFinsNet.ReadBool(string, ushort)"/>
		[HslMqttApi( "ReadBoolArray", "" )]
		public override OperateResult<bool[]> ReadBool( string address, ushort length ) => Helper.OmronHostLinkHelper.ReadBool( this, address, length );

		/// <inheritdoc cref="OmronFinsNet.Write(string, bool[])"/>
		[HslMqttApi( "WriteBoolArray", "" )]
		public override OperateResult Write( string address, bool[] values ) => Helper.OmronHostLinkHelper.Write( this, address, values );

		#endregion

		#region Async Read Write Bool
#if !NET35 && !NET20
		/// <inheritdoc cref="OmronFinsNet.ReadBool(string, ushort)"/>
		public override async Task<OperateResult<bool[]>> ReadBoolAsync( string address, ushort length ) => await Helper.OmronHostLinkHelper.ReadBoolAsync( this, address, length );

		/// <inheritdoc cref="OmronFinsNet.Write(string, bool[])"/>
		public override async Task<OperateResult> WriteAsync( string address, bool[] values ) => await Helper.OmronHostLinkHelper.WriteAsync( this, address, values );
#endif
		#endregion

		#region Object Override

		/// <inheritdoc/>
		public override string ToString( ) => $"OmronHostLinkOverTcp[{IpAddress}:{Port}]";

		#endregion
	}
}
