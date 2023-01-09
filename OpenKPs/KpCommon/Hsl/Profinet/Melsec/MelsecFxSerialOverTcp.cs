using HslCommunication.BasicFramework;
using HslCommunication.Core;
using HslCommunication.Core.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HslCommunication.Reflection;
using System.Net.Sockets;
using System.Net;
#if !NET35 && !NET20
using System.Threading.Tasks;
#endif

namespace HslCommunication.Profinet.Melsec
{
	/// <summary>
	/// 三菱串口协议的网络版，如果使用的是 FX3U编程口(fx2n) -> GOT1000(RS232)(或是GOT2000)  -> 上位机(以太网) 的方式，那么就需要把<see cref="UseGOT"/>设置为 <c>True</c><br />
	/// The network version of the Mitsubishi serial port protocol, if you use the FX3U programming port (fx2n) -> GOT1000 (RS232) (or GOT2000) -> host computer (Ethernet) method, 
	/// then you need to put <see cref="UseGOT" /> is set to <c>True</c>
	/// </summary>
	/// <remarks>
	/// 一般老旧的型号，例如FX2N之类的，需要将<see cref="IsNewVersion"/>设置为<c>False</c>，如果是FX3U新的型号，则需要将<see cref="IsNewVersion"/>设置为<c>True</c>
	/// </remarks>
	/// <example>
	/// 字读写地址支持的列表如下：
	/// <list type="table">
	///   <listheader>
	///     <term>地址名称</term>
	///     <term>地址代号</term>
	///     <term>示例</term>
	///     <term>地址范围</term>
	///     <term>地址进制</term>
	///     <term>备注</term>
	///   </listheader>
	///   <item>
	///     <term>数据寄存器</term>
	///     <term>D</term>
	///     <term>D100,D200</term>
	///     <term>D0-D511,D8000-D8255</term>
	///     <term>10</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>定时器的值</term>
	///     <term>TN</term>
	///     <term>TN10,TN20</term>
	///     <term>TN0-TN255</term>
	///     <term>10</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>计数器的值</term>
	///     <term>CN</term>
	///     <term>CN10,CN20</term>
	///     <term>CN0-CN199,CN200-CN255</term>
	///     <term>10</term>
	///     <term></term>
	///   </item>
	/// </list>
	/// 位地址支持的列表如下：
	/// <list type="table">
	///   <listheader>
	///     <term>地址名称</term>
	///     <term>地址代号</term>
	///     <term>示例</term>
	///     <term>地址范围</term>
	///     <term>地址进制</term>
	///     <term>备注</term>
	///   </listheader>
	///   <item>
	///     <term>内部继电器</term>
	///     <term>M</term>
	///     <term>M100,M200</term>
	///     <term>M0-M1023,M8000-M8255</term>
	///     <term>10</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>输入继电器</term>
	///     <term>X</term>
	///     <term>X1,X20</term>
	///     <term>X0-X177</term>
	///     <term>8</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>输出继电器</term>
	///     <term>Y</term>
	///     <term>Y10,Y20</term>
	///     <term>Y0-Y177</term>
	///     <term>8</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>步进继电器</term>
	///     <term>S</term>
	///     <term>S100,S200</term>
	///     <term>S0-S999</term>
	///     <term>10</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>定时器触点</term>
	///     <term>TS</term>
	///     <term>TS10,TS20</term>
	///     <term>TS0-TS255</term>
	///     <term>10</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>定时器线圈</term>
	///     <term>TC</term>
	///     <term>TC10,TC20</term>
	///     <term>TC0-TC255</term>
	///     <term>10</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>计数器触点</term>
	///     <term>CS</term>
	///     <term>CS10,CS20</term>
	///     <term>CS0-CS255</term>
	///     <term>10</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>计数器线圈</term>
	///     <term>CC</term>
	///     <term>CC10,CC20</term>
	///     <term>CC0-CC255</term>
	///     <term>10</term>
	///     <term></term>
	///   </item>
	/// </list>
	/// <code lang="cs" source="HslCommunication_Net45.Test\Documentation\Samples\Profinet\MelsecFxSerial.cs" region="Usage" title="简单的使用" />
	/// </example>
	public class MelsecFxSerialOverTcp : NetworkDeviceBase
	{
		#region Constructor

		/// <summary>
		/// 实例化网络版的三菱的串口协议的通讯对象<br />
		/// Instantiate the communication object of Mitsubishi's serial protocol on the network
		/// </summary>
		public MelsecFxSerialOverTcp( )
		{
			this.WordLength     = 1;
			this.ByteTransform  = new RegularByteTransform( );
			this.IsNewVersion   = true;
			this.ByteTransform.IsStringReverseByteWord = true;
			this.SleepTime      = 20;
			this.incrementCount = new SoftIncrementCount( int.MaxValue, 1 );
		}

		/// <summary>
		/// 指定ip地址及端口号来实例化三菱的串口协议的通讯对象<br />
		/// Specify the IP address and port number to instantiate the communication object of Mitsubishi's serial protocol
		/// </summary>
		/// <param name="ipAddress">Ip地址</param>
		/// <param name="port">端口号</param>
		public MelsecFxSerialOverTcp( string ipAddress, int port ) : this( )
		{
			this.IpAddress     = ipAddress;
			this.Port          = port;
		}

		/// <summary>
		/// 获取或设置是否使用GOT连接三菱的PLC，当使用了GOT连接到
		/// </summary>
		public bool UseGOT { get => this.useGot; set => this.useGot = value; }

		#endregion

		#region Pack Unpack

		private byte[] GetBytesSend( byte[] command )
		{
			List<byte> array = new List<byte>( );

			for (int i = 0; i < command.Length; i++)
			{
				if (i < 2)
				{
					array.Add( command[i] );
				}
				else if (i < command.Length - 4)
				{
					if (command[i] == 0x10) array.Add( command[i] );
					array.Add( command[i] );
				}
				else
				{
					array.Add( command[i] );
				}
			}
			return array.ToArray( );
		}

		private byte[] GetBytesReceive( byte[] response )
		{
			List<byte> array = new List<byte>( );
			for (int i = 0; i < response.Length; i++)
			{
				if (i < 2)
				{
					array.Add( response[i] );
				}
				else if (i < response.Length - 4)
				{
					if (response[i] == 0x10 && response[i + 1] == 0x10)
					{
						array.Add( response[i] );
						i++;
					}
					else
					{
						array.Add( response[i] );
					}
				}
				else
				{
					array.Add( response[i] );
				}
			}
			return array.ToArray( );
		}

		/// <inheritdoc/>
		public override byte[] PackCommandWithHeader( byte[] command )
		{
			if (useGot)
			{
				byte[] buffer = new byte[32 + 34 + command.Length];
				buffer[ 0] = 0x10;
				buffer[ 1] = 0x02;
				buffer[ 2] = 0x5E;
				buffer[ 6] = 0xFC;
				buffer[12] = 0x12;
				buffer[13] = 0x12;
				buffer[17] = 0xFF;
				buffer[18] = 0xFF;
				buffer[19] = 0x03;
				buffer[22] = 0xFF;
				buffer[23] = 0x03;
				buffer[26] = BitConverter.GetBytes( 34 + command.Length )[0];  // 报文长度信息 0x2D
				buffer[27] = BitConverter.GetBytes( 34 + command.Length )[1];
				buffer[28] = 0x1C;
				buffer[29] = 0x09;
				buffer[30] = 0x1A;
				buffer[31] = 0x18;

				buffer[41] = 0xFC;
				buffer[44] = 0x12;
				buffer[45] = 0x12;
				buffer[46] = 0x04;
				buffer[47] = 0x14;
				buffer[49] = 0x01;
				buffer[50] = BitConverter.GetBytes( Port )[1];                    // Port
				buffer[51] = BitConverter.GetBytes( Port )[0];
				buffer[52] = IPAddress.Parse( IpAddress ).GetAddressBytes( )[0];  // IP
				buffer[53] = IPAddress.Parse( IpAddress ).GetAddressBytes( )[1];
				buffer[54] = IPAddress.Parse( IpAddress ).GetAddressBytes( )[2];
				buffer[55] = IPAddress.Parse( IpAddress ).GetAddressBytes( )[3];
				buffer[56] = 0x01;
				buffer[57] = 0x02;
				BitConverter.GetBytes( (int)incrementCount.GetCurrentValue( ) ).CopyTo( buffer, 58 );  // Message ID
				command.CopyTo( buffer, 62 );
				buffer[buffer.Length - 4] = 0x10;
				buffer[buffer.Length - 3] = 0x03;
				MelsecHelper.FxCalculateCRC( buffer, 2, 4 ).CopyTo( buffer, buffer.Length - 2 );       // CRC
				return GetBytesSend( buffer );
			}
			else
			{
				return base.PackCommandWithHeader( command );
			}
		}

		/// <inheritdoc/>
		public override OperateResult<byte[]> UnpackResponseContent( byte[] send, byte[] response )
		{
			if (useGot)
			{
				if (response.Length > 68)
				{
					response = GetBytesReceive( response );                     // 去除多余的0x10

					int index = -1;
					for (int i = 0; i < response.Length - 4; i++)
					{
						if (response[i] == 0x10 && response[i + 1] == 0x02)
						{
							index = i;
							break;
						}
					}

					if (index >= 0) return OperateResult.CreateSuccessResult( response.RemoveDouble( 64 + index, 4 ) );
				}
				return new OperateResult<byte[]>( "Got failed: " + response.ToHexString( ' ', 16 ) );
			}
			else
			{
				return base.UnpackResponseContent( send, response );
			}
		}

		#endregion

		#region ReadFromServer

		private List<string> inis = new List<string>( )
		{
			"00 10 02 FF FF FC 01 10 03",
			"10025E000000FC00000000001212000000FFFF030000FF0300002D001C091A18000000000000000000FC000012120414000113960AC4E5D7010201000000023030453032303203364310033543",
			"10025E000000FC00000000001212000000FFFF030000FF0300002D001C091A18000000000000000000FC000012120414000113960AC4E5D7010202000000023030454341303203384510033833",
			"10025E000000FC00000000001212000000FFFF030000FF0300002D001C091A18000000000000000000FC000012120414000113960AC4E5D7010203000000023030453032303203364310033545",
			"10025E000000FC00000000001212000000FFFF030000FF0300002D001C091A18000000000000000000FC000012120414000113960AC4E5D7010204000000023030454341303203384510033835",
			"10025E000000FC00000000001212000000FFFF030000FF0300002F001C091A18000000000000000000FC000012120414000113960AC4E5D70102050000000245303138303030343003443510034342",
			"10025E000000FC00000000001212000000FFFF030000FF0300002F001C091A18000000000000000000FC000012120414000113960AC4E5D70102060000000245303138303430314303453910034535",
			"10025E000000FC00000000001212000000FFFF030000FF0300002F001C091A18000000000000000000FC000012120414000113960AC4E5D70102070000000245303030453030343003453110034436",
			"10025E000000FC00000000001212000000FFFF030000FF0300002F001C091A18000000000000000000FC000012120414000113960AC4E5D70102080000000245303030453430343003453510034446",
			"10025E000000FC00000000001212000000FFFF030000FF0300002F001C091A18000000000000000000FC000012120414000113960AC4E5D70102090000000245303030453830343003453910034538",
			"10025E000000FC00000000001212000000FFFF030000FF0300002F001C091A18000000000000000000FC000012120414000113960AC4E5D701020A0000000245303030454330343003463410034630"
		};

		/// <inheritdoc/>
		protected override OperateResult InitializationOnConnect( Socket socket )
		{
			if (useGot)
			{
				for (int i = 0; i < inis.Count; i++)
				{
					OperateResult ini1 = ReadFromCoreServer( socket, inis[i].ToHexBytes( ), hasResponseData: true, usePackAndUnpack: false );
					if (!ini1.IsSuccess) return ini1;
				}
			}
			return base.InitializationOnConnect( socket );
		}
		// 这部分的逻辑代码是失败或是读取的数据很短的话再读一次

			/// <inheritdoc/>
		public override OperateResult<byte[]> ReadFromCoreServer( Socket socket, byte[] send, bool hasResponseData = true, bool usePackAndUnpack = true )
		{
			OperateResult<byte[]> read = base.ReadFromCoreServer( socket, send, hasResponseData, usePackAndUnpack );
			if (!read.IsSuccess) return read;

			if (read.Content == null) return read;
			if (read.Content.Length > 2) return read;

			OperateResult<byte[]> read2 = base.ReadFromCoreServer( socket, send, hasResponseData, usePackAndUnpack );
			if (!read2.IsSuccess) return read2;

			return OperateResult.CreateSuccessResult( SoftBasic.SpliceArray( read.Content, read2.Content ) );
		}
#if !NET20 && !NET35
		/// <inheritdoc/>
		public override async Task<OperateResult<byte[]>> ReadFromCoreServerAsync( Socket socket, byte[] send, bool hasResponseData = true, bool usePackAndUnpack = true )
		{
			OperateResult<byte[]> read = await base.ReadFromCoreServerAsync( socket, send, hasResponseData, usePackAndUnpack );
			if (!read.IsSuccess) return read;

			if (read.Content == null) return read;
			if (read.Content.Length > 2) return read;

			OperateResult<byte[]> read2 = await base.ReadFromCoreServerAsync( socket, send, hasResponseData, usePackAndUnpack );
			if (!read2.IsSuccess) return read2;

			return OperateResult.CreateSuccessResult( SoftBasic.SpliceArray( read.Content, read2.Content ) );
		}

		/// <inheritdoc/>
		protected override async Task<OperateResult> InitializationOnConnectAsync( Socket socket )
		{
			if (useGot)
			{
				for (int i = 0; i < inis.Count; i++)
				{
					OperateResult ini1 = await ReadFromCoreServerAsync( socket, inis[i].ToHexBytes( ), hasResponseData: true, usePackAndUnpack: false );
					if (!ini1.IsSuccess) return ini1;
				}
			}
			return await base.InitializationOnConnectAsync( socket );
		}
#endif
		#endregion

		/// <summary>
		/// 当前的编程口协议是否为新版，默认为新版，如果无法读取，切换旧版再次尝试<br />
		/// Whether the current programming port protocol is the new version, the default is the new version, 
		/// if it cannot be read, switch to the old version and try again
		/// </summary>
		public bool IsNewVersion { get; set; }

		#region Read Write Byte

		/// <inheritdoc cref="Helper.MelsecFxSerialHelper.Read(IReadWriteDevice, string, ushort, bool)"/>
		[HslMqttApi( "ReadByteArray", "" )]
		public override OperateResult<byte[]> Read( string address, ushort length ) => 
			Helper.MelsecFxSerialHelper.Read( this, address, length, IsNewVersion );

		/// <inheritdoc cref="Helper.MelsecFxSerialHelper.Write(IReadWriteDevice, string, byte[], bool)"/>
		[HslMqttApi( "WriteByteArray", "" )]
		public override OperateResult Write( string address, byte[] value ) => 
			Helper.MelsecFxSerialHelper.Write( this, address, value, IsNewVersion );

		#endregion

		#region Async Read Write Byte
#if !NET35 && !NET20
		/// <inheritdoc cref="Read(string, ushort)"/>
		public override async Task<OperateResult<byte[]>> ReadAsync( string address, ushort length ) =>
			await Helper.MelsecFxSerialHelper.ReadAsync( this, address, length, IsNewVersion );

		/// <inheritdoc cref="Write(string, byte[])"/>
		public override async Task<OperateResult> WriteAsync( string address, byte[] value ) =>
			await Helper.MelsecFxSerialHelper.WriteAsync( this, address, value, IsNewVersion );
#endif
		#endregion

		#region Read Write Bool

		/// <inheritdoc cref="Helper.MelsecFxSerialHelper.ReadBool(IReadWriteDevice, string, ushort, bool)"/>
		[HslMqttApi( "ReadBoolArray", "" )]
		public override OperateResult<bool[]> ReadBool( string address, ushort length ) => 
			Helper.MelsecFxSerialHelper.ReadBool( this, address, length, IsNewVersion );

		/// <inheritdoc cref="Helper.MelsecFxSerialHelper.Write(IReadWriteDevice, string, bool)"/>
		[HslMqttApi( "WriteBool", "" )]
		public override OperateResult Write( string address, bool value ) =>
			Helper.MelsecFxSerialHelper.Write( this, address, value );

		#endregion

		#region Async Read Write Bool
#if !NET35 && !NET20
		/// <inheritdoc cref="ReadBool(string, ushort)"/>
		public override async Task<OperateResult<bool[]>> ReadBoolAsync( string address, ushort length ) =>
			await Helper.MelsecFxSerialHelper.ReadBoolAsync( this, address, length, IsNewVersion );

		/// <inheritdoc cref="Write(string, bool)"/>
		public override async Task<OperateResult> WriteAsync( string address, bool value ) =>
			await Helper.MelsecFxSerialHelper.WriteAsync( this, address, value );
#endif
		#endregion

		#region Active

		/// <inheritdoc cref="Helper.MelsecFxSerialHelper.ActivePlc(IReadWriteDevice)"/>
		[HslMqttApi( )]
		public OperateResult ActivePlc( ) => Helper.MelsecFxSerialHelper.ActivePlc( this );
#if !NET20 && !NET35
		/// <inheritdoc cref="Helper.MelsecFxSerialHelper.ActivePlc(IReadWriteDevice)"/>
		public async Task<OperateResult> ActivePlcAsync( ) => await Helper.MelsecFxSerialHelper.ActivePlcAsync( this );
#endif
		#endregion

		#region Private Member

		private bool useGot = false;                 // 是否使用GOT连接的操作
		private SoftIncrementCount incrementCount;   // 自增的消息号信息

		#endregion

		#region Object Override

		///<inheritdoc/>
		public override string ToString( ) => $"MelsecFxSerialOverTcp[{IpAddress}:{Port}]";

		#endregion

	}
}
