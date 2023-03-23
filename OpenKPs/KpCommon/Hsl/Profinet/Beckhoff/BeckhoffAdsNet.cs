using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HslCommunication.Core;
using HslCommunication.Core.IMessage;
using HslCommunication.Core.Net;
using HslCommunication.BasicFramework;
using System.Net.Sockets;
using System.Net;
using HslCommunication.Reflection;
using HslCommunication.Profinet.Beckhoff.Helper;
#if !NET35 && !NET20
using System.Threading.Tasks;
#endif

namespace HslCommunication.Profinet.Beckhoff
{
	/// <summary>
	/// 倍福的ADS协议，支持读取倍福的地址数据，关于端口号的选择，TwinCAT2，端口号801；TwinCAT3，端口号为851，NETID可以选择手动输入，自动输入方式，具体参考API文档的示例代码<br />
	/// Beckhoff's ADS protocol supports reading Beckhoff address data. Regarding the choice of port number, TwinCAT2, port number is 801; TwinCAT3, port number is 851, NETID can be input manually or automatically. 
	/// For details, please refer to the example of API documentation code
	/// </summary>
	/// <remarks>
	/// 支持的地址格式分四种，第一种是绝对的地址表示，比如M100，I100，Q100；第二种是字符串地址，采用s=aaaa;的表示方式；第三种是绝对内存地址采用i=1000000;的表示方式，第四种是自定义的index group, IG=0xF020;0 的地址<br />
	/// There are four supported address formats, the first is absolute address representation, such as M100, I100, Q100; the second is string address, using s=aaaa; representation; 
	/// the third is absolute memory address using i =1000000; representation, the fourth is the custom index group, the address of IG=0xF020;0
	/// <br />
	/// <note type="important">
	/// 在实际的测试中，由于打开了VS软件对倍福PLC进行编程操作，会导致HslCommunicationDemo读取PLC发生间歇性读写失败的问题，此时需要关闭Visual Studio软件对倍福的连接，之后HslCommunicationDemo就会读写成功，感谢QQ：1813782515 提供的解决思路。
	/// </note>
	/// </remarks>
	/// <example>
	/// 地址既支持 M100, I100，Q100 ，读取bool时，支持输入 M100.0,  也支持符号的地址，s=MAIN.a  ,也支持绝对地址的形式， i=1235467;<br />
	/// 下面是实例化的例子，可选两种方式
	/// <code lang="cs" source="HslCommunication_Net45.Test\Documentation\Samples\Profinet\BeckhoffAdsNetSample.cs" region="Sample1" title="实例化" />
	/// 实例化之后，就可以连接操作了
	/// <code lang="cs" source="HslCommunication_Net45.Test\Documentation\Samples\Profinet\BeckhoffAdsNetSample.cs" region="Sample2" title="连接" />
	/// 连接成功之后，就可以进行读写操作了
	/// <code lang="cs" source="HslCommunication_Net45.Test\Documentation\Samples\Profinet\BeckhoffAdsNetSample.cs" region="Sample3" title="读写示例" />
	/// 也可以高级的批量读取，需要自己手动解析下数据
	/// <code lang="cs" source="HslCommunication_Net45.Test\Documentation\Samples\Profinet\BeckhoffAdsNetSample.cs" region="Sample4" title="批量读取" />
	/// 当然，还可以进一步，既实现了批量的高性能读取，又自动解析。
	/// <code lang="cs" source="HslCommunication_Net45.Test\Documentation\Samples\Profinet\BeckhoffAdsNetSample.cs" region="Sample5" title="类型读取" />
	/// </example>
	public class BeckhoffAdsNet : NetworkDeviceBase
	{
		#region Constructor

		/// <summary>
		/// 实例化一个默认的对象<br />
		/// Instantiate a default object
		/// </summary>
		public BeckhoffAdsNet( )
		{
			this.WordLength          = 2;
			this.targetAMSNetId[4]   = 1;
			this.targetAMSNetId[5]   = 1;
			this.targetAMSNetId[6]   = 0x53;
			this.targetAMSNetId[7]   = 0x03;
			this.sourceAMSNetId[4]   = 1;
			this.sourceAMSNetId[5]   = 1;
			this.ByteTransform       = new RegularByteTransform( );
			this.UseServerActivePush = true;
		}

		/// <summary>
		/// 通过指定的ip地址以及端口号实例化一个默认的对象<br />
		/// Instantiate a default object with the specified IP address and port number
		/// </summary>
		/// <param name="ipAddress">IP地址信息</param>
		/// <param name="port">端口号</param>
		public BeckhoffAdsNet( string ipAddress, int port ) : this( )
		{
			this.IpAddress         = ipAddress;
			this.Port              = port;
		}

		/// <inheritdoc/>
		protected override INetMessage GetNewNetMessage( ) => new AdsNetMessage( );

		#endregion

		#region Ip Port Override

		///<inheritdoc/>
		[HslMqttApi( HttpMethod = "GET", Description = "Get or set the IP address of the remote server. If it is a local test, then it needs to be set to 127.0.0.1" )]
		public override string IpAddress 
		{ 
			get => base.IpAddress;
			set { 
				base.IpAddress = value;
				string[] ip = base.IpAddress.Split( new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries );
				for (int i = 0; i < ip.Length; i++)
				{
					targetAMSNetId[i] = byte.Parse( ip[i] );
				}
			}
		}

		#endregion

		#region Public Properties

		/// <summary>
		/// 是否使用标签的名称缓存功能，默认为 <c>False</c><br />
		/// Whether to use tag name caching. The default is <c>False</c>
		/// </summary>
		public bool UseTagCache
		{
			get => useTagCache;
			set => useTagCache = value;
		}

		/// <summary>
		/// 是否使用服务器自动的NETID信息，默认手动设置<br />
		/// Whether to use the server's automatic NETID information, manually set by default
		/// </summary>
		public bool UseAutoAmsNetID
		{
			get => useAutoAmsNetID;
			set => useAutoAmsNetID = value;
		}

		/// <summary>
		/// 获取或设置Ams的端口号信息，TwinCAT2，端口号801,811,821,831；TwinCAT3，端口号为851,852,853<br />
		/// Get or set the port number information of Ams, TwinCAT2, the port number is 801, 811, 821, 831; TwinCAT3, the port number is 851, 852, 853
		/// </summary>
		public int AmsPort
		{
			get
			{
				return BitConverter.ToUInt16( this.targetAMSNetId, 6 );
			}
			set
			{
				this.targetAMSNetId[6] = BitConverter.GetBytes( value )[0];
				this.targetAMSNetId[7] = BitConverter.GetBytes( value )[1];
			}
		}

		#endregion

		#region AdsNetId

		private byte[] targetAMSNetId = new byte[8];
		private byte[] sourceAMSNetId = new byte[8];
		private string senderAMSNetId = string.Empty;
		private string _targetAmsNetID = string.Empty;

		/// <summary>
		/// 目标的地址，举例 192.168.0.1.1.1；也可以是带端口号 192.168.0.1.1.1:801<br />
		/// The address of the destination, for example 192.168.0.1.1.1; it can also be the port number 192.168.0.1.1.1: 801
		/// </summary>
		/// <remarks>
		/// Port：1: AMS Router; 2: AMS Debugger; 800: Ring 0 TC2 PLC; 801: TC2 PLC Runtime System 1; 811: TC2 PLC Runtime System 2; <br />
		/// 821: TC2 PLC Runtime System 3; 831: TC2 PLC Runtime System 4; 850: Ring 0 TC3 PLC; 851: TC3 PLC Runtime System 1<br />
		/// 852: TC3 PLC Runtime System 2; 853: TC3 PLC Runtime System 3; 854: TC3 PLC Runtime System 4; ...
		/// </remarks>
		/// <param name="amsNetId">AMSNet Id地址</param>
		public void SetTargetAMSNetId( string amsNetId )
		{
			if (!string.IsNullOrEmpty( amsNetId ))
			{
				AdsHelper.StrToAMSNetId( amsNetId ).CopyTo( targetAMSNetId, 0 );
				this._targetAmsNetID = amsNetId;
			}
		}

		/// <summary>
		/// 设置原目标地址 举例 192.168.0.100.1.1；也可以是带端口号 192.168.0.100.1.1:34567<br />
		/// Set the original destination address Example: 192.168.0.100.1.1; it can also be the port number 192.168.0.100.1.1: 34567
		/// </summary>
		/// <param name="amsNetId">原地址</param>
		public void SetSenderAMSNetId( string amsNetId )
		{
			if (!string.IsNullOrEmpty( amsNetId ))
			{
				AdsHelper.StrToAMSNetId( amsNetId ).CopyTo( sourceAMSNetId, 0 );
				senderAMSNetId = amsNetId;
			}
		}

		/// <summary>
		/// 获取当前发送的AMS的网络ID信息
		/// </summary>
		/// <returns>AMS发送信息</returns>
		public string GetSenderAMSNetId( ) => AdsHelper.GetAmsNetIdString( sourceAMSNetId, 0 );

		/// <summary>
		/// 获取当前目标的AMS网络的ID信息
		/// </summary>
		/// <returns>AMS目标信息</returns>
		public string GetTargetAMSNetId( ) => AdsHelper.GetAmsNetIdString( targetAMSNetId, 0 );

		#endregion

		#region Pack Unpack

		/// <inheritdoc/>
		public override byte[] PackCommandWithHeader( byte[] command )
		{
			uint invokeId = (uint)incrementCount.GetCurrentValue( );
			targetAMSNetId.CopyTo( command, 6 + 0 );
			sourceAMSNetId.CopyTo( command, 6 + 8 );

			command[6 + 28] = BitConverter.GetBytes( invokeId )[0];
			command[6 + 29] = BitConverter.GetBytes( invokeId )[1];
			command[6 + 30] = BitConverter.GetBytes( invokeId )[2];
			command[6 + 31] = BitConverter.GetBytes( invokeId )[3];
			return base.PackCommandWithHeader( command );
		}

		/// <inheritdoc/>
		public override OperateResult<byte[]> UnpackResponseContent( byte[] send, byte[] response )
		{
			if (response.Length >= 38)
			{
				ushort commandId = ByteTransform.TransUInt16( response, 22 );

				OperateResult check = AdsHelper.CheckResponse( response );
				if (!check.IsSuccess)
				{
					if ((check.ErrorCode == 1809) && (commandId == BeckhoffCommandId.Read || commandId == BeckhoffCommandId.Write ))
					{
						// 因为当前的数据句柄不存在了，设备可能已经重启了，清空标记缓存，重新设置所有的数据
						lock (tagLock) tagCaches.Clear( );
					}
					return OperateResult.CreateFailedResult<byte[]>( check );
				}

				if (commandId == BeckhoffCommandId.ReadDeviceInfo)           return OperateResult.CreateSuccessResult( response.RemoveBegin( 42 ) );
				if (commandId == BeckhoffCommandId.Read)                     return OperateResult.CreateSuccessResult( response.RemoveBegin( 46 ) );
				if (commandId == BeckhoffCommandId.Write)                    return OperateResult.CreateSuccessResult( new byte[0] );
				if (commandId == BeckhoffCommandId.ReadState)                return OperateResult.CreateSuccessResult( response.RemoveBegin( 42 ) );
				if (commandId == BeckhoffCommandId.WriteControl)             return OperateResult.CreateSuccessResult( response.RemoveBegin( 42 ) );
				if (commandId == BeckhoffCommandId.AddDeviceNotification)    return OperateResult.CreateSuccessResult( response.RemoveBegin( 42 ) );
				if (commandId == BeckhoffCommandId.DeleteDeviceNotification) return OperateResult.CreateSuccessResult( new byte[0] );
				if (commandId == BeckhoffCommandId.ReadWrite)                return OperateResult.CreateSuccessResult( response.RemoveBegin( 46 ) );
			}
			return base.UnpackResponseContent( send, response );
		}

		/// <inheritdoc/>
		protected override void ExtraAfterReadFromCoreServer( OperateResult read )
		{
			if (!read.IsSuccess)
			{
				if (read.ErrorCode < 0)
				{
					// 当因为发生网络错误的时候，清空标记缓存，重新设置所有的数据
					if (useTagCache)
						lock (tagLock) tagCaches.Clear( );
				}
			}
			base.ExtraAfterReadFromCoreServer( read );
		}

		#endregion

		#region Initialization Override

		/// <inheritdoc/>
		protected override OperateResult InitializationOnConnect( Socket socket )
		{
			if (string.IsNullOrEmpty( senderAMSNetId ) && string.IsNullOrEmpty( _targetAmsNetID )) useAutoAmsNetID = true;
			if (useAutoAmsNetID)
			{
				// 请求 AMS 信息，这个在 TC3 上才支持
				OperateResult<byte[]> read1 = GetLocalNetId( );
				if (!read1.IsSuccess) return read1;
				if (read1.Content.Length >= 12) Array.Copy( read1.Content, 6, this.targetAMSNetId, 0, 6 );

				OperateResult send2 = Send( socket, AdsHelper.PackAmsTcpHelper( AmsTcpHeaderFlags.PortConnect, new byte[2] ) );
				if (!send2.IsSuccess) return send2;

				OperateResult<byte[]> read2 = ReceiveByMessage( socket, ReceiveTimeOut, GetNewNetMessage( ) );
				if (!read2.IsSuccess) return read2;
				if (read2.Content.Length >= 14) Array.Copy( read2.Content, 6, this.sourceAMSNetId, 0, 8 );

				return base.InitializationOnConnect( socket );
			}
			else if (string.IsNullOrEmpty( senderAMSNetId ))
			{
				IPEndPoint iPEndPoint = (IPEndPoint)socket.LocalEndPoint;
				sourceAMSNetId[6] = BitConverter.GetBytes( iPEndPoint.Port )[0];
				sourceAMSNetId[7] = BitConverter.GetBytes( iPEndPoint.Port )[1];
				iPEndPoint.Address.GetAddressBytes( ).CopyTo( sourceAMSNetId, 0 );
			}

			// 每次连接的时候，重置当前的标签ID信息
			if(useTagCache) 
				lock (tagLock) tagCaches.Clear( );

			return base.InitializationOnConnect( socket );
		}

		private OperateResult<byte[]> GetLocalNetId( )
		{
			OperateResult<Socket> opSocket = CreateSocketAndConnect( this.IpAddress, this.Port, this.ConnectTimeOut );
			if (!opSocket.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( opSocket );

			OperateResult send = Send( opSocket.Content, AdsHelper.PackAmsTcpHelper( AmsTcpHeaderFlags.GetLocalNetId, new byte[4] ) );
			if (!send.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( send );

			OperateResult<byte[]> read = ReceiveByMessage( opSocket.Content, ReceiveTimeOut, GetNewNetMessage( ) );
			if (!read.IsSuccess) return read;

			opSocket.Content?.Close( );
			return read;
		}

#if !NET35 && !NET20
		/// <inheritdoc/>
		protected override async Task<OperateResult> InitializationOnConnectAsync( Socket socket )
		{
			if (string.IsNullOrEmpty( senderAMSNetId ) && string.IsNullOrEmpty( _targetAmsNetID )) useAutoAmsNetID = true;
			if (useAutoAmsNetID)
			{
				// 请求 AMS 信息，这个在 TC3 上才支持
				OperateResult<byte[]> read1 = await GetLocalNetIdAsync( );
				if (!read1.IsSuccess) return read1;
				if (read1.Content.Length >= 12) Array.Copy( read1.Content, 6, this.targetAMSNetId, 0, 6 );

				OperateResult send2 = await SendAsync( socket, AdsHelper.PackAmsTcpHelper( AmsTcpHeaderFlags.PortConnect, new byte[2] ) );
				if (!send2.IsSuccess) return send2;

				OperateResult<byte[]> read2 = await ReceiveByMessageAsync( socket, ReceiveTimeOut, GetNewNetMessage( ) );
				if (!read2.IsSuccess) return read2;
				if (read2.Content.Length >= 14) Array.Copy( read2.Content, 6, this.sourceAMSNetId, 0, 8 );
			}
			else if (string.IsNullOrEmpty( senderAMSNetId ))
			{
				IPEndPoint iPEndPoint = (IPEndPoint)socket.LocalEndPoint;
				sourceAMSNetId[6] = BitConverter.GetBytes( iPEndPoint.Port )[0];
				sourceAMSNetId[7] = BitConverter.GetBytes( iPEndPoint.Port )[1];
				iPEndPoint.Address.GetAddressBytes( ).CopyTo( sourceAMSNetId, 0 );
			}

			// 每次连接的时候，重置当前的标签ID信息
			if (useTagCache)
				lock (tagLock) tagCaches.Clear( );

			return await base.InitializationOnConnectAsync( socket );
		}

		private async Task<OperateResult<byte[]>> GetLocalNetIdAsync( )
		{
			OperateResult<Socket> opSocket = await CreateSocketAndConnectAsync( this.IpAddress, this.Port, this.ConnectTimeOut );
			if (!opSocket.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( opSocket );

			OperateResult send = await SendAsync( opSocket.Content, AdsHelper.PackAmsTcpHelper( AmsTcpHeaderFlags.GetLocalNetId, new byte[4] ) );
			if (!send.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( send );

			OperateResult<byte[]> read = await ReceiveByMessageAsync( opSocket.Content, ReceiveTimeOut, GetNewNetMessage( ) );
			if (!read.IsSuccess) return read;

			opSocket.Content?.Close( );
			return read;
		}
#endif

		/// <inheritdoc/>
		protected override bool DecideWhetherQAMessage( Socket socket, OperateResult<byte[]> receive )
		{
			if (!receive.IsSuccess)
			{
				// 重置当前的标签ID信息
				if (useTagCache)
					lock (tagLock) tagCaches.Clear( );

				return false;
			}


			byte[] response = receive.Content;
			if (response.Length >= 2)
			{
				AmsTcpHeaderFlags headerFlags = (AmsTcpHeaderFlags)BitConverter.ToUInt16( response, 0 );
				if (headerFlags == AmsTcpHeaderFlags.Command) return true;
			}
			return false;
		}
		#endregion

		/// <summary>
		/// 根据当前标签的地址获取到内存偏移地址<br />
		/// Get the memory offset address based on the address of the current label
		/// </summary>
		/// <param name="address">带标签的地址信息，例如s=A,那么标签就是A</param>
		/// <returns>内存偏移地址</returns>
		public OperateResult<uint> ReadValueHandle( string address )
		{
			if (!address.StartsWith( "s=" )) return new OperateResult<uint>( "When read valueHandle, address must startwith 's=', forexample: s=MAIN.A" );

			OperateResult<byte[]> build = AdsHelper.BuildReadWriteCommand( address, 4, false, AdsHelper.StrToAdsBytes( address.Substring( 2 ) ) );
			if (!build.IsSuccess) return OperateResult.CreateFailedResult<uint>( build );

			OperateResult<byte[]> read = ReadFromCoreServer( build.Content );
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<uint>( read );

			return OperateResult.CreateSuccessResult( BitConverter.ToUInt32( read.Content, 0 ) );
		}

		/// <summary>
		/// 将字符串的地址转换为内存的地址，其他地址则不操作<br />
		/// Converts the address of a string to the address of a memory, other addresses do not operate
		/// </summary>
		/// <param name="address">地址信息，s=A的地址转换为i=100000的形式</param>
		/// <returns>地址</returns>
		public OperateResult<string> TransValueHandle( string address )
		{
			if (address.StartsWith( "s=" ) || address.StartsWith( "S=" ))
			{
				if (useTagCache)
				{
					lock (tagLock)
					{
						if (tagCaches.ContainsKey( address ))
						{
							return OperateResult.CreateSuccessResult( $"i={tagCaches[address]}" );
						}
					}
				}
				OperateResult<uint> read = ReadValueHandle( address );
				if (!read.IsSuccess) return OperateResult.CreateFailedResult<string>( read );

				if (useTagCache)
				{
					lock (tagLock)
					{
						if (!tagCaches.ContainsKey( address ))
						{
							tagCaches.Add( address, read.Content );
						}
					}
				}
				return OperateResult.CreateSuccessResult( $"i={read.Content}" );
			}
			else
				return OperateResult.CreateSuccessResult( address );
		}

		/// <summary>
		/// 读取Ads设备的设备信息。主要是版本号，设备名称<br />
		/// Read the device information of the Ads device. Mainly version number, device name
		/// </summary>
		/// <returns>设备信息</returns>
		[HslMqttApi( "ReadAdsDeviceInfo", "读取Ads设备的设备信息。主要是版本号，设备名称" )]
		public OperateResult<AdsDeviceInfo> ReadAdsDeviceInfo( )
		{
			OperateResult<byte[]> build = AdsHelper.BuildReadDeviceInfoCommand( );
			if (!build.IsSuccess) return OperateResult.CreateFailedResult<AdsDeviceInfo>( build );

			OperateResult<byte[]> read = ReadFromCoreServer( build.Content );
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<AdsDeviceInfo>( read );

			return OperateResult.CreateSuccessResult( new AdsDeviceInfo( read.Content ) );
		}

		/// <summary>
		/// 读取Ads设备的状态信息，其中<see cref="OperateResult{T1, T2}.Content1"/>是Ads State，<see cref="OperateResult{T1, T2}.Content2"/>是Device State<br />
		/// Read the status information of the Ads device, where <see cref="OperateResult{T1, T2}.Content1"/> is the Ads State, and <see cref="OperateResult{T1, T2}.Content2"/> is the Device State
		/// </summary>
		/// <returns>设备状态信息</returns>
		[HslMqttApi( "ReadAdsState", "读取Ads设备的状态信息" )]
		public OperateResult<ushort, ushort> ReadAdsState( )
		{
			OperateResult<byte[]> build = AdsHelper.BuildReadStateCommand( );
			if (!build.IsSuccess) return OperateResult.CreateFailedResult<ushort, ushort>( build );

			OperateResult<byte[]> read = ReadFromCoreServer( build.Content );
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<ushort, ushort>( read );

			return OperateResult.CreateSuccessResult( BitConverter.ToUInt16( read.Content, 0 ), BitConverter.ToUInt16( read.Content, 2 ) );
		}

		/// <summary>
		/// 写入Ads的状态，可以携带数据信息，数据可以为空<br />
		/// Write the status of Ads, can carry data information, and the data can be empty
		/// </summary>
		/// <param name="state">ads state</param>
		/// <param name="deviceState">device state</param>
		/// <param name="data">数据信息</param>
		/// <returns>是否写入成功</returns>
		[HslMqttApi( "WriteAdsState", "写入Ads的状态，可以携带数据信息，数据可以为空" )]
		public OperateResult WriteAdsState( short state, short deviceState, byte[] data )
		{
			OperateResult<byte[]> build = AdsHelper.BuildWriteControlCommand( state, deviceState, data );
			if (!build.IsSuccess) return build;

			return ReadFromCoreServer( build.Content );
		}

		/// <summary>
		/// 释放当前的系统句柄，该句柄是通过<see cref="ReadValueHandle(string)"/>获取的
		/// </summary>
		/// <param name="handle">句柄</param>
		/// <returns>是否释放成功</returns>
		public OperateResult ReleaseSystemHandle( uint handle )
		{
			OperateResult<byte[]> build = AdsHelper.BuildReleaseSystemHandle( handle );
			if (!build.IsSuccess) return build;

			return ReadFromCoreServer( build.Content );
		}
#if !NET35 && !NET20
		/// <inheritdoc cref="ReadValueHandle(string)"/>
		public async Task<OperateResult<uint>> ReadValueHandleAsync( string address )
		{
			if (!address.StartsWith( "s=" )) return new OperateResult<uint>( "When read valueHandle, address must startwith 's=', forexample: s=MAIN.A" );

			OperateResult<byte[]> build = AdsHelper.BuildReadWriteCommand( address, 4, false, AdsHelper.StrToAdsBytes( address.Substring( 2 ) ) );
			if (!build.IsSuccess) return OperateResult.CreateFailedResult<uint>( build );

			OperateResult<byte[]> read = await ReadFromCoreServerAsync( build.Content );
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<uint>( read );

			return OperateResult.CreateSuccessResult( BitConverter.ToUInt32( read.Content, 0 ) );
		}

		/// <inheritdoc cref="TransValueHandle(string)"/>
		public async Task<OperateResult<string>> TransValueHandleAsync( string address )
		{
			if (address.StartsWith( "s=" ) || address.StartsWith( "S=" ))
			{
				if (useTagCache)
				{
					lock (tagLock)
					{
						if (tagCaches.ContainsKey( address ))
						{
							return OperateResult.CreateSuccessResult( $"i={tagCaches[address]}" );
						}
					}
				}
				OperateResult<uint> read = await ReadValueHandleAsync( address );
				if (!read.IsSuccess) return OperateResult.CreateFailedResult<string>( read );

				if (useTagCache)
				{
					lock (tagLock)
					{
						if (!tagCaches.ContainsKey( address ))
						{
							tagCaches.Add( address, read.Content );
						}
					}
				}
				return OperateResult.CreateSuccessResult( $"i={read.Content}" );
			}
			else
				return OperateResult.CreateSuccessResult( address );
		}

		/// <inheritdoc cref="ReadAdsDeviceInfo"/>
		public async Task<OperateResult<AdsDeviceInfo>> ReadAdsDeviceInfoAsync( )
		{
			OperateResult<byte[]> build = AdsHelper.BuildReadDeviceInfoCommand( );
			if (!build.IsSuccess) return OperateResult.CreateFailedResult<AdsDeviceInfo>( build );

			OperateResult<byte[]> read = await ReadFromCoreServerAsync( build.Content );
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<AdsDeviceInfo>( read );

			return OperateResult.CreateSuccessResult( new AdsDeviceInfo( read.Content ) );
		}

		/// <inheritdoc cref="ReadAdsState"/>
		public async Task<OperateResult<ushort, ushort>> ReadAdsStateAsync( )
		{
			OperateResult<byte[]> build = AdsHelper.BuildReadStateCommand( );
			if (!build.IsSuccess) return OperateResult.CreateFailedResult<ushort, ushort>( build );

			OperateResult<byte[]> read = await ReadFromCoreServerAsync( build.Content );
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<ushort, ushort>( read );

			return OperateResult.CreateSuccessResult( BitConverter.ToUInt16( read.Content, 0 ), BitConverter.ToUInt16( read.Content, 2 ) );
		}

		/// <inheritdoc cref="WriteAdsState(short, short, byte[])"/>
		public async Task<OperateResult> WriteAdsStateAsync( short state, short deviceState, byte[] data )
		{
			OperateResult<byte[]> build = AdsHelper.BuildWriteControlCommand( state, deviceState, data );
			if (!build.IsSuccess) return build;

			return await ReadFromCoreServerAsync( build.Content );
		}

		/// <inheritdoc cref="ReleaseSystemHandle(uint)"/>
		public async Task<OperateResult> ReleaseSystemHandleAsync( uint handle )
		{
			OperateResult<byte[]> build = AdsHelper.BuildReleaseSystemHandle( handle );
			if (!build.IsSuccess) return build;

			return await ReadFromCoreServerAsync( build.Content );
		}
#endif

		#region Read Write Override

		/// <summary>
		/// 读取PLC的数据，地址共有三种格式，一：I,Q,M数据信息，举例M0,M100；二：内存地址，i=100000；三：标签地址，s=A<br />
		/// Read PLC data, there are three formats of address, one: I, Q, M data information, such as M0, M100; two: memory address, i = 100000; three: tag address, s = A
		/// </summary>
		/// <param name="address">地址信息，地址共有三种格式，一：I,Q,M数据信息，举例M0,M100；二：内存地址，i=100000；三：标签地址，s=A</param>
		/// <param name="length">长度</param>
		/// <returns>包含是否成功的结果对象</returns>
		[HslMqttApi( "ReadByteArray", "" )]
		public override OperateResult<byte[]> Read( string address, ushort length )
		{
			// 先检查地址
			OperateResult<string> addressCheck = TransValueHandle( address );
			if (!addressCheck.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( addressCheck );

			address = addressCheck.Content;

			OperateResult<byte[]> build = AdsHelper.BuildReadCommand( address, length, false );
			if (!build.IsSuccess) return build;

			return ReadFromCoreServer( build.Content );
		}

		private void AddLengthAndOffset( List<ushort> length, List<int> offset, ref int index, int len )
		{
			length.Add( (ushort)len );
			offset.Add( index );
			index += len;
		}

		/// <inheritdoc/>
		public override OperateResult<T> Read<T>( )
		{
			var type = typeof( T );
			var obj = type.Assembly.CreateInstance( type.FullName );

			List<HslAddressProperty> array = HslReflectionHelper.GetHslPropertyInfos( type, this.GetType( ), null, this.ByteTransform );
			string[] address = array.Select( m => m.DeviceAddressAttribute.Address ).ToArray( );
			ushort[] length  = array.Select( m => (ushort)m.ByteLength ).ToArray( );

			OperateResult<byte[]> read = Read( address, length );
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<T>( read );

			HslReflectionHelper.SetPropertyValueFrom( this.ByteTransform, obj, array, read.Content );
			return OperateResult.CreateSuccessResult( (T)obj );
		}

		/// <summary>
		/// 批量读取PLC的数据，需要传入地址数组，以及读取的长度数组信息，长度单位为字节单位，如果是读取bool变量的，则以bool为单位，统一返回一串字节数据信息，需要进行二次解析的操作。<br />
		/// To read PLC data in batches, you need to pass in the address array and the read length array information. The unit of length is in bytes. If you read a bool variable, 
		/// it will return a string of byte data information in units of bool. , which requires a secondary parsing operation.
		/// </summary>
		/// <remarks>
		/// 关于二次解析的参考信息，可以参考API文档，地址：http://api.hslcommunication.cn<br />
		/// For reference information about secondary parsing, you can refer to the API documentation, address: http://api.hslcommunication.cn
		/// </remarks>
		/// <param name="address">地址数组信息</param>
		/// <param name="length">读取的长度数组信息</param>
		/// <returns>原始字节数组的结果对象</returns>
		[HslMqttApi( "ReadBatchByte", "To read PLC data in batches, you need to pass in the address array and the read length array information." )]
		public OperateResult<byte[]> Read( string[] address, ushort[] length )
		{
			if (address.Length != length.Length) return new OperateResult<byte[]>( StringResources.Language.TwoParametersLengthIsNotSame );

			// 先检查地址
			for (int i = 0; i < address.Length; i++)
			{
				OperateResult<string> addressCheck = TransValueHandle( address[i] );
				if (!addressCheck.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( addressCheck );

				address[i] = addressCheck.Content;
			}

			// 打包命令
			OperateResult<byte[]> build = AdsHelper.BuildReadCommand( address, length );
			if (!build.IsSuccess) return build;

			return ReadFromCoreServer( build.Content );
		}

		/// <summary>
		/// 写入PLC的数据，地址共有三种格式，一：I,Q,M数据信息，举例M0,M100；二：内存地址，i=100000；三：标签地址，s=A<br />
		/// There are three formats for the data written into the PLC. One: I, Q, M data information, such as M0, M100; two: memory address, i = 100000; three: tag address, s = A
		/// </summary>
		/// <param name="address">地址信息，地址共有三种格式，一：I,Q,M数据信息，举例M0,M100；二：内存地址，i=100000；三：标签地址，s=A</param>
		/// <param name="value">数据值</param>
		/// <returns>是否写入成功</returns>
		[HslMqttApi( "WriteByteArray", "" )]
		public override OperateResult Write( string address, byte[] value )
		{
			OperateResult<string> addressCheck = TransValueHandle( address );
			if (!addressCheck.IsSuccess) return addressCheck;

			address = addressCheck.Content;

			OperateResult<byte[]> build = AdsHelper.BuildWriteCommand( address, value, false );
			if (!build.IsSuccess) return build;

			return ReadFromCoreServer( build.Content );
		}

		// 调试不通
		//public OperateResult Write( string[] address, List<byte[]> value )
		//{
		//	if (address.Length != value.Count) return new OperateResult<byte[]>( StringResources.Language.TwoParametersLengthIsNotSame );

		//	// 先检查地址
		//	for (int i = 0; i < address.Length; i++)
		//	{
		//		OperateResult<string> addressCheck = TransValueHandle( address[i] );
		//		if (!addressCheck.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( addressCheck );

		//		address[i] = addressCheck.Content;
		//	}

		//	// 打包命令
		//	OperateResult<byte[]> build = AdsHelper.BuildWriteCommand( address, value );
		//	if (!build.IsSuccess) return build;

		//	//Console.WriteLine( "Write data: " + Environment.NewLine + build.Content.ToHexString( ' ', 32 ) );
		//	return ReadFromCoreServer( build.Content );
		//}

		/// <summary>
		/// 读取PLC的数据，地址共有三种格式，一：I,Q,M数据信息，举例M0,M100；二：内存地址，i=100000；三：标签地址，s=A<br />
		/// Read PLC data, there are three formats of address, one: I, Q, M data information, such as M0, M100; two: memory address, i = 100000; three: tag address, s = A
		/// </summary>
		/// <param name="address">PLC的地址信息，例如 M10</param>
		/// <param name="length">数据长度</param>
		/// <returns>包含是否成功的结果对象</returns>
		[HslMqttApi( "ReadBoolArray", "" )]
		public override OperateResult<bool[]> ReadBool( string address, ushort length )
		{
			OperateResult<string> addressCheck = TransValueHandle( address );
			if (!addressCheck.IsSuccess) return OperateResult.CreateFailedResult<bool[]>( addressCheck );

			address = addressCheck.Content;

			OperateResult<byte[]> build = AdsHelper.BuildReadCommand( address, length, true );
			if (!build.IsSuccess) return OperateResult.CreateFailedResult<bool[]>( build );

			OperateResult<byte[]> read = ReadFromCoreServer( build.Content );
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<bool[]>( read ); ;

			return OperateResult.CreateSuccessResult( read.Content.Select( m => m != 0x00 ).ToArray( ) );
		}

		/// <summary>
		/// 写入PLC的数据，地址共有三种格式，一：I,Q,M数据信息，举例M0,M100；二：内存地址，i=100000；三：标签地址，s=A<br />
		/// There are three formats for the data written into the PLC. One: I, Q, M data information, such as M0, M100; two: memory address, i = 100000; three: tag address, s = A
		/// </summary>
		/// <param name="address">地址信息</param>
		/// <param name="value">数据值</param>
		/// <returns>是否写入成功</returns>
		[HslMqttApi( "WriteBoolArray", "" )]
		public override OperateResult Write( string address, bool[] value )
		{
			OperateResult<string> addressCheck = TransValueHandle( address );
			if (!addressCheck.IsSuccess) return addressCheck;

			address = addressCheck.Content;

			OperateResult<byte[]> build = AdsHelper.BuildWriteCommand( address, value, true );
			if (!build.IsSuccess) return build;

			return ReadFromCoreServer( build.Content );
		}

		/// <summary>
		/// 读取PLC的数据，地址共有三种格式，一：I,Q,M数据信息，举例M0,M100；二：内存地址，i=100000；三：标签地址，s=A<br />
		/// Read PLC data, there are three formats of address, one: I, Q, M data information, such as M0, M100; two: memory address, i = 100000; three: tag address, s = A
		/// </summary>
		/// <param name="address">地址信息</param>
		/// <returns>包含是否成功的结果对象</returns>
		[HslMqttApi( "ReadByte", "" )]
		public OperateResult<byte> ReadByte( string address ) => ByteTransformHelper.GetResultFromArray( Read( address, 1 ) );

		/// <summary>
		/// 写入PLC的数据，地址共有三种格式，一：I,Q,M数据信息，举例M0,M100；二：内存地址，i=100000；三：标签地址，s=A<br />
		/// There are three formats for the data written into the PLC. One: I, Q, M data information, such as M0, M100; two: memory address, i = 100000; three: tag address, s = A
		/// </summary>
		/// <param name="address">地址信息</param>
		/// <param name="value">数据值</param>
		/// <returns>是否写入成功</returns>
		[HslMqttApi( "WriteByte", "" )]
		public OperateResult Write( string address, byte value ) => Write( address, new byte[] { value } );

		#endregion

		#region Read Write Async Override
#if !NET35 && !NET20
		/// <inheritdoc/>
		public async override Task<OperateResult<T>> ReadAsync<T>( )
		{
			var type = typeof( T );
			var obj = type.Assembly.CreateInstance( type.FullName );

			List<HslAddressProperty> array = HslReflectionHelper.GetHslPropertyInfos( type, this.GetType( ), null, this.ByteTransform );
			string[] address = array.Select( m => m.DeviceAddressAttribute.Address ).ToArray( );
			ushort[] length = array.Select( m => (ushort)m.ByteLength ).ToArray( );

			OperateResult<byte[]> read = await ReadAsync( address, length );
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<T>( read );

			HslReflectionHelper.SetPropertyValueFrom( this.ByteTransform, obj, array, read.Content );
			return OperateResult.CreateSuccessResult( (T)obj );
		}

		/// <inheritdoc cref="Read(string, ushort)"/>
		public override async Task<OperateResult<byte[]>> ReadAsync( string address, ushort length )
		{
			// 先检查地址
			OperateResult<string> addressCheck = await TransValueHandleAsync( address );
			if (!addressCheck.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( addressCheck );

			address = addressCheck.Content;

			OperateResult<byte[]> build = AdsHelper.BuildReadCommand( address, length, false );
			if (!build.IsSuccess) return build;

			return await ReadFromCoreServerAsync( build.Content );
		}

		/// <inheritdoc cref="Read(string[], ushort[])"/>
		public async Task<OperateResult<byte[]>> ReadAsync( string[] address, ushort[] length )
		{
			if (address.Length != length.Length) return new OperateResult<byte[]>( StringResources.Language.TwoParametersLengthIsNotSame );

			// 先检查地址
			for (int i = 0; i < address.Length; i++)
			{
				OperateResult<string> addressCheck = TransValueHandle( address[i] );
				if (!addressCheck.IsSuccess) return OperateResult.CreateFailedResult<byte[]>( addressCheck );

				address[i] = addressCheck.Content;
			}

			// 打包命令
			OperateResult<byte[]> build = AdsHelper.BuildReadCommand( address, length );
			if (!build.IsSuccess) return build;

			return await ReadFromCoreServerAsync( build.Content );
		}

		/// <inheritdoc cref="Write(string, byte[])"/>
		public override async Task<OperateResult> WriteAsync( string address, byte[] value )
		{
			OperateResult<string> addressCheck = await TransValueHandleAsync( address );
			if (!addressCheck.IsSuccess) return addressCheck;

			address = addressCheck.Content;

			OperateResult<byte[]> build = AdsHelper.BuildWriteCommand( address, value, false );
			if (!build.IsSuccess) return build;

			return await ReadFromCoreServerAsync( build.Content );
		}

		/// <inheritdoc cref="ReadBool(string, ushort)"/>
		public override async Task<OperateResult<bool[]>> ReadBoolAsync( string address, ushort length )
		{
			OperateResult<string> addressCheck = await TransValueHandleAsync( address );
			if (!addressCheck.IsSuccess) return OperateResult.CreateFailedResult<bool[]>( addressCheck );

			address = addressCheck.Content;

			OperateResult<byte[]> build = AdsHelper.BuildReadCommand( address, length, true );
			if (!build.IsSuccess) return OperateResult.CreateFailedResult<bool[]>( build );

			OperateResult<byte[]> read = await ReadFromCoreServerAsync( build.Content );
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<bool[]>( read ); ;

			return OperateResult.CreateSuccessResult( read.Content.Select( m => m != 0x00 ).ToArray( ) );
		}

		/// <inheritdoc cref="Write(string, bool[])"/>
		public override async Task<OperateResult> WriteAsync( string address, bool[] value )
		{
			OperateResult<string> addressCheck = await TransValueHandleAsync( address );
			if (!addressCheck.IsSuccess) return addressCheck;

			address = addressCheck.Content;

			OperateResult<byte[]> build = AdsHelper.BuildWriteCommand( address, value, true );
			if (!build.IsSuccess) return build;

			return await ReadFromCoreServerAsync( build.Content );
		}

		/// <inheritdoc cref="ReadByte(string)"/>
		public async Task<OperateResult<byte>> ReadByteAsync( string address ) => ByteTransformHelper.GetResultFromArray( await ReadAsync( address, 1 ) );

		/// <inheritdoc cref="Write(string, byte)"/>
		public async Task<OperateResult> WriteAsync( string address, byte value ) => await WriteAsync( address, new byte[] { value } );
#endif
		#endregion

		#region Private Member

		private bool useAutoAmsNetID = false;
		private bool useTagCache = false;
		private readonly Dictionary<string, uint> tagCaches = new Dictionary<string, uint>( );
		private readonly object tagLock = new object( );
		private readonly SoftIncrementCount incrementCount = new SoftIncrementCount( int.MaxValue, 1, 1 );

		#endregion

		#region Object Override

		/// <inheritdoc/>
		public override string ToString( ) => $"BeckhoffAdsNet[{IpAddress}:{Port}]";

		#endregion
	}
}
