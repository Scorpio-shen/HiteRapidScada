using HslCommunication.BasicFramework;
using HslCommunication.Core;
using HslCommunication.Core.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HslCommunication.Reflection;
using HslCommunication.Profinet.Melsec.Helper;
#if !NET35 && !NET20
using System.Threading.Tasks;
#endif

namespace HslCommunication.Profinet.Melsec
{
	/// <summary>
	/// 三菱计算机链接协议的网口版本，适用FX3U系列，FX3G，FX3S等等系列，通常在PLC侧连接的是485的接线口<br />
	/// Network port version of Mitsubishi Computer Link Protocol, suitable for FX3U series, FX3G, FX3S, etc., usually the 485 connection port is connected on the PLC side
	/// </summary>
	/// <remarks>
	/// </remarks>
	/// <example>
	/// 支持的通讯的系列如下参考
	/// <list type="table">
	///     <listheader>
	///         <term>系列</term>
	///         <term>是否支持</term>
	///         <term>备注</term>
	///     </listheader>
	///     <item>
	///         <description>FX3UC系列</description>
	///         <description>支持</description>
	///         <description></description>
	///     </item>
	///     <item>
	///         <description>FX3U系列</description>
	///         <description>支持</description>
	///         <description></description>
	///     </item>
	///     <item>
	///         <description>FX3GC系列</description>
	///         <description>支持</description>
	///         <description></description>
	///     </item>
	///     <item>
	///         <description>FX3G系列</description>
	///         <description>支持</description>
	///         <description></description>
	///     </item>
	///     <item>
	///         <description>FX3S系列</description>
	///         <description>支持</description>
	///         <description></description>
	///     </item>
	///     <item>
	///         <description>FX2NC系列</description>
	///         <description>支持</description>
	///         <description></description>
	///     </item>
	///     <item>
	///         <description>FX2N系列</description>
	///         <description>部分支持(v1.06+)</description>
	///         <description>通过监控D8001来确认版本号</description>
	///     </item>
	///     <item>
	///         <description>FX1NC系列</description>
	///         <description>支持</description>
	///         <description></description>
	///     </item>
	///     <item>
	///         <description>FX1N系列</description>
	///         <description>支持</description>
	///         <description></description>
	///     </item>
	///     <item>
	///         <description>FX1S系列</description>
	///         <description>支持</description>
	///         <description></description>
	///     </item>
	///     <item>
	///         <description>FX0N系列</description>
	///         <description>部分支持(v1.20+)</description>
	///         <description></description>
	///     </item>
	///     <item>
	///         <description>FX0S系列</description>
	///         <description>不支持</description>
	///         <description></description>
	///     </item>
	///     <item>
	///         <description>FX0系列</description>
	///         <description>不支持</description>
	///         <description></description>
	///     </item>
	///     <item>
	///         <description>FX2C系列</description>
	///         <description>部分支持(v3.30+)</description>
	///         <description></description>
	///     </item>
	///     <item>
	///         <description>FX2(FX)系列</description>
	///         <description>部分支持(v3.30+)</description>
	///         <description></description>
	///     </item>
	///     <item>
	///         <description>FX1系列</description>
	///         <description>不支持</description>
	///         <description></description>
	///     </item>
	/// </list>
	/// 数据地址支持的格式如下：
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
	///     <term>内部继电器</term>
	///     <term>M</term>
	///     <term>M100,M200</term>
	///     <term>10</term>
	///     <term>√</term>
	///     <term>√</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>输入继电器</term>
	///     <term>X</term>
	///     <term>X10,X20</term>
	///     <term>8</term>
	///     <term>√</term>
	///     <term>√</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>输出继电器</term>
	///     <term>Y</term>
	///     <term>Y10,Y20</term>
	///     <term>8</term>
	///     <term>√</term>
	///     <term>√</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>步进继电器</term>
	///     <term>S</term>
	///     <term>S100,S200</term>
	///     <term>10</term>
	///     <term>√</term>
	///     <term>√</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>定时器的触点</term>
	///     <term>TS</term>
	///     <term>TS100,TS200</term>
	///     <term>10</term>
	///     <term>√</term>
	///     <term>√</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>定时器的当前值</term>
	///     <term>TN</term>
	///     <term>TN100,TN200</term>
	///     <term>10</term>
	///     <term>√</term>
	///     <term>×</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>计数器的触点</term>
	///     <term>CS</term>
	///     <term>CS100,CS200</term>
	///     <term>10</term>
	///     <term>√</term>
	///     <term>√</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>计数器的当前</term>
	///     <term>CN</term>
	///     <term>CN100,CN200</term>
	///     <term>10</term>
	///     <term>√</term>
	///     <term>×</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>数据寄存器</term>
	///     <term>D</term>
	///     <term>D1000,D2000</term>
	///     <term>10</term>
	///     <term>√</term>
	///     <term>×</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>文件寄存器</term>
	///     <term>R</term>
	///     <term>R100,R200</term>
	///     <term>10</term>
	///     <term>√</term>
	///     <term>×</term>
	///     <term></term>
	///   </item>
	/// </list>
	/// </example>
	public class MelsecFxLinksOverTcp : NetworkDeviceBase, IReadWriteFxLinks
	{
		#region Constructor

		/// <summary>
		/// 实例化默认的对象<br />
		/// Instantiate the default object
		/// </summary>
		public MelsecFxLinksOverTcp( )
		{
			this.WordLength    = 1;
			this.ByteTransform = new RegularByteTransform( );
			this.SleepTime     = 20;
		}

		/// <summary>
		/// 指定ip地址和端口号来实例化默认的对象<br />
		/// Specify the IP address and port number to instantiate the default object
		/// </summary>
		/// <param name="ipAddress">Ip地址信息</param>
		/// <param name="port">端口号</param>
		public MelsecFxLinksOverTcp( string ipAddress, int port ) : this( )
		{
			this.IpAddress     = ipAddress;
			this.Port          = port;
		}

		/// <inheritdoc/>
		public override byte[] PackCommandWithHeader( byte[] command ) => MelsecFxLinksHelper.PackCommandWithHeader( this, command );

		#endregion

		#region Public Member

		/// <inheritdoc cref="IReadWriteFxLinks.Station"/>
		public byte Station { get => station; set => station = value; }

		/// <inheritdoc cref="IReadWriteFxLinks.WaittingTime"/>
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

		/// <inheritdoc cref="IReadWriteFxLinks.SumCheck"/>
		public bool SumCheck { get => sumCheck; set => sumCheck = value; }

		/// <inheritdoc cref="IReadWriteFxLinks.Format"/>
		public int Format { get; set; } = 1;

		#endregion

		#region Read Write Support

		/// <inheritdoc cref="MelsecFxLinksHelper.Read(IReadWriteFxLinks, string, ushort)"/>
		[HslMqttApi( "ReadByteArray", "Read PLC data in batches, in units of words, supports reading X, Y, M, S, D, T, C." )]
		public override OperateResult<byte[]> Read( string address, ushort length ) => MelsecFxLinksHelper.Read( this, address, length );

		/// <inheritdoc cref="MelsecFxLinksHelper.Write(IReadWriteFxLinks, string, byte[])"/>
		[HslMqttApi( "WriteByteArray", "The data written to the PLC in batches is in units of words, that is, at least 2 bytes of information. It supports X, Y, M, S, D, T, and C. " )]
		public override OperateResult Write( string address, byte[] value ) => MelsecFxLinksHelper.Write( this, address, value );

		#endregion

		#region Async Read Write Support
#if !NET35 && !NET20
		/// <inheritdoc cref="Read(string, ushort)"/>
		public override async Task<OperateResult<byte[]>> ReadAsync( string address, ushort length ) => await MelsecFxLinksHelper.ReadAsync( this, address, length );

		/// <inheritdoc cref="Write(string, byte[])"/>
		public override async Task<OperateResult> WriteAsync( string address, byte[] value ) => await MelsecFxLinksHelper.WriteAsync( this, address, value );
#endif
		#endregion

		#region Bool Read Write

		/// <inheritdoc cref="MelsecFxLinksHelper.ReadBool(IReadWriteFxLinks, string, ushort)"/>
		[HslMqttApi( "ReadBoolArray", "Read bool data in batches. The supported types are X, Y, S, T, C." )]
		public override OperateResult<bool[]> ReadBool( string address, ushort length ) => MelsecFxLinksHelper.ReadBool( this, address, length );

		/// <inheritdoc cref="MelsecFxLinksHelper.Write(IReadWriteFxLinks, string, bool[])"/>
		[HslMqttApi( "WriteBoolArray", "Write arrays of type bool in batches. The supported types are X, Y, S, T, C." )]
		public override OperateResult Write( string address, bool[] value ) => MelsecFxLinksHelper.Write( this, address, value );

		#endregion

		#region Async Bool Read Write
#if !NET35 && !NET20
		/// <inheritdoc cref="ReadBool(string, ushort)"/>
		public override async Task<OperateResult<bool[]>> ReadBoolAsync( string address, ushort length ) => await MelsecFxLinksHelper.ReadBoolAsync( this, address, length );

		/// <inheritdoc cref="Write(string, bool[])"/>
		public override async Task<OperateResult> WriteAsync( string address, bool[] value ) => await MelsecFxLinksHelper.WriteAsync( this, address, value );
#endif
		#endregion

		#region Start Stop

		/// <inheritdoc cref="MelsecFxLinksHelper.StartPLC(IReadWriteFxLinks, string)"/>
		[HslMqttApi( Description = "Start the PLC operation, you can carry additional parameter information and specify the station number. Example: s=2; Note: The semicolon is required." )]
		public OperateResult StartPLC( string parameter = "" ) => MelsecFxLinksHelper.StartPLC( this, parameter );

		/// <inheritdoc cref="MelsecFxLinksHelper.StopPLC(IReadWriteFxLinks, string)"/>
		[HslMqttApi( Description = "Stop PLC operation, you can carry additional parameter information and specify the station number. Example: s=2; Note: The semicolon is required." )]
		public OperateResult StopPLC( string parameter = "" ) => MelsecFxLinksHelper.StopPLC( this, parameter );

		/// <inheritdoc cref="MelsecFxLinksHelper.ReadPlcType(IReadWriteFxLinks, string)"/>
		[HslMqttApi( Description = "Read the PLC model information, you can carry additional parameter information, and specify the station number. Example: s=2; Note: The semicolon is required." )]
		public OperateResult<string> ReadPlcType( string parameter = "" ) => MelsecFxLinksHelper.ReadPlcType( this, parameter );

		#endregion

		#region Start Stop
#if !NET35 && !NET20
		/// <inheritdoc cref="StartPLC(string)"/>
		public async Task<OperateResult> StartPLCAsync( string parameter = "" ) => await MelsecFxLinksHelper.StartPLCAsync( this, parameter );

		/// <inheritdoc cref="StopPLC(string)"/>
		public async Task<OperateResult> StopPLCAsync( string parameter = "" ) => await MelsecFxLinksHelper.StopPLCAsync( this, parameter );

		/// <inheritdoc cref="ReadPlcType(string)"/>
		public async Task<OperateResult<string>> ReadPlcTypeAsync( string parameter = "" ) => await MelsecFxLinksHelper.ReadPlcTypeAsync( this, parameter );

#endif
		#endregion

		#region Private Member

		private byte station = 0x00;                 // PLC的站号信息
		private byte watiingTime = 0x00;             // 报文的等待时间，设置为0-15
		private bool sumCheck = true;                // 是否启用和校验

		#endregion

		#region Object Override

		/// <inheritdoc/>
		public override string ToString( ) => $"MelsecFxLinksOverTcp[{IpAddress}:{Port}]";

		#endregion

	}
}
