using System;
using HslCommunication;
using System.Text;
using System.Collections;
using HslCommunication.Core;
using HslCommunication.Core.Net;
using HslCommunication.BasicFramework;
using HslCommunication.Serial;
using System.Net.Sockets;
using HslCommunication.Core.IMessage;
using HslCommunication.Secs.Message;
using System.Threading;
using HslCommunication.Secs.Helper;
using System.Collections.Generic;
using HslCommunication.Secs.Types;
#if !NET20 && !NET35
using System.Threading.Tasks;
#endif

namespace HslCommunication.Secs
{
	/// <summary>
	/// HSMS的协议实现，SECS基于TCP的版本
	/// </summary>
	/// <remarks>
	/// </remarks>
	/// <example>
	/// 下面就看看基本的操作内容
	/// <code lang="cs" source="HslCommunication_Net45.Test\Documentation\Samples\Secs\SecsGemSample.cs" region="Sample1" title="基本的读写" />
	/// 如果想要手动处理下设备主要返回的数据，比如报警之类的，可以参考下面的方法
	/// <code lang="cs" source="HslCommunication_Net45.Test\Documentation\Samples\Secs\SecsGemSample.cs" region="Sample3" title="事件回调处理" />
	/// 关于<see cref="SecsValue"/>类型，可以非常灵活的实例化，参考下面的示例代码
	/// <code lang="cs" source="HslCommunication_Net45.Test\Documentation\Samples\Secs\SecsGemSample.cs" region="Sample2" title="SecsValue说明" />
	/// </example>
	public class SecsHsms : NetworkDoubleBase, ISecs
	{
		#region Constructor

		/// <summary>
		/// 实例化一个默认的对象<br />
		/// instantiate a default object
		/// </summary>
		public SecsHsms( )
		{
			this.incrementCount = new SoftIncrementCount( uint.MaxValue, 1 );
			this.ByteTransform  = new ReverseBytesTransform( );
			// this.WordLength = 2;
			this.UseServerActivePush = true;
			this.Gem = new Gem( this );
		}

		/// <summary>
		/// 指定ip地址和端口号来实例化一个默认的对象<br />
		/// Specify the IP address and port number to instantiate a default object
		/// </summary>
		/// <param name="ipAddress">PLC的Ip地址</param>
		/// <param name="port">PLC的端口</param>
		public SecsHsms( string ipAddress, int port ) : this( )
		{
			this.IpAddress = ipAddress;
			this.Port      = port;
		}

		/// <inheritdoc/>
		protected override INetMessage GetNewNetMessage( ) => new SecsHsmsMessage( );

		#endregion

		#region Public Properties

		/// <summary>
		/// 获取或设置当前的DeivceID信息
		/// </summary>
		public ushort DeviceID { get; set; }

		/// <summary>
		/// 获取或设置当前的GEM信息，可以用来方便的调用一些常用的功能接口，或是自己实现自定义的接口方法
		/// </summary>
		public Gem Gem { get; set; }

		/// <summary>
		/// 是否使用S0F0来初始化当前的设备对象信息
		/// </summary>
		public bool InitializationS0F0 { get; set; } = false;

		/// <summary>
		/// 获取或设置用于字符串解析的编码信息
		/// </summary>
		public Encoding StringEncoding { get => this.stringEncoding; set => this.stringEncoding = value; }

		#endregion

		#region Override Method

		/// <inheritdoc/>
		protected override OperateResult InitializationOnConnect( Socket socket )
		{
			if (InitializationS0F0) Send( socket, Secs1.BuildHSMSMessage( ushort.MaxValue, 0, 0, 1, (uint)this.incrementCount.GetCurrentValue( ), null, false ) );

			return base.InitializationOnConnect( socket );
		}
#if !NET20 && !NET35
		/// <inheritdoc/>
		protected async override Task<OperateResult> InitializationOnConnectAsync( Socket socket )
		{
			if (InitializationS0F0) await SendAsync( socket, Secs1.BuildHSMSMessage( ushort.MaxValue, 0, 0, 1, (uint)this.incrementCount.GetCurrentValue( ), null, false ) );

			return await base.InitializationOnConnectAsync( socket );
		}
#endif
		/// <inheritdoc/>
		protected override bool DecideWhetherQAMessage( Socket socket, OperateResult<byte[]> receive )
		{
			if (!receive.IsSuccess) return false;
			byte[] response = receive.Content;
			SecsMessage secsMessage = new SecsMessage( response, 4 );
			secsMessage.StringEncoding = this.stringEncoding;                             // 设置当前对象的编码信息

			if (secsMessage.StreamNo == 0 && secsMessage.FunctionNo == 0 && secsMessage.BlockNo % 2 == 1)
			{
				Send( socket, Secs1.BuildHSMSMessage( ushort.MaxValue, 0, 0, (ushort)(secsMessage.BlockNo + 1), secsMessage.MessageID, null, false ) );
				return false;
			}

			if (secsMessage.FunctionNo % 2 == 0 && secsMessage.FunctionNo != 0)
			{
				bool isQA = false;
				lock (identityQAs)
				{
					isQA = identityQAs.Remove( secsMessage.MessageID );
				}
				if (isQA) return isQA;
			}

			if (secsMessage.StreamNo == 1 && secsMessage.FunctionNo == 13) // 自动处理 S1F13的消息
			{
				SendByCommand( 1, 14, new SecsValue( new object[] { new byte[] { 0x00 }, SecsValue.EmptyListValue( ) } ).ToSourceBytes( ), false );
				return false;
			}
			else if (secsMessage.StreamNo == 2 && secsMessage.FunctionNo == 17)
			{
				SendByCommand( 2, 18, new SecsValue( DateTime.Now.ToString("yyyyMMddHHmmssff") ), false );
				return false;
			}
			else if (secsMessage.StreamNo == 1 && secsMessage.FunctionNo == 1)
			{
				SendByCommand( 1, 2, SecsValue.EmptyListValue( ), false );
				return false;
			}

			OnSecsMessageReceived?.Invoke( this, secsMessage );
			return false;
		}


		#endregion

		#region Public Method

		/// <inheritdoc cref="ISecs.SendByCommand(byte, byte, byte[], bool)"/>
		public OperateResult SendByCommand( byte stream, byte function, byte[] data, bool back )
		{
			byte[] command = Secs1.BuildHSMSMessage( DeviceID, stream, function, 0, (uint)this.incrementCount.GetCurrentValue( ), data, back );
			OperateResult send = Send( this.pipeSocket.Socket, command );

			if (!send.IsSuccess && send.ErrorCode < 0) this.pipeSocket.IsSocketError = true;
			return send;
		}

		/// <inheritdoc cref="ISecs.SendByCommand(byte, byte, SecsValue, bool)"/>
		public OperateResult SendByCommand( byte stream, byte function, SecsValue data, bool back ) => SendByCommand( stream, function, data.ToSourceBytes( this.stringEncoding ), back );

		/// <inheritdoc cref="ISecs.ReadSecsMessage(byte, byte, byte[], bool)"/>
		public OperateResult<SecsMessage> ReadSecsMessage( byte stream, byte function, byte[] data, bool back )
		{
			uint identityQA = (uint)this.incrementCount.GetCurrentValue( );
			lock (identityQAs) identityQAs.Add( identityQA );

			OperateResult<byte[]> read = ReadFromCoreServer( Secs1.BuildHSMSMessage( DeviceID, stream, function, 0, identityQA, data, back ) );
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<SecsMessage>( read );

			return OperateResult.CreateSuccessResult( new SecsMessage( read.Content, 4 ) { StringEncoding = this.stringEncoding } );
		}

		/// <inheritdoc cref="ISecs.ReadSecsMessage(byte, byte, SecsValue, bool)"/>
		public OperateResult<SecsMessage> ReadSecsMessage( byte stream, byte function, SecsValue data, bool back ) => ReadSecsMessage( stream, function, data.ToSourceBytes( this.stringEncoding ), back );

#if !NET20 && !NET35
		/// <inheritdoc cref="ISecs.SendByCommand(byte, byte, byte[], bool)"/>
		public async Task<OperateResult> SendByCommandAsync( byte stream, byte function, byte[] data, bool back )
		{
			byte[] command = Secs1.BuildHSMSMessage( DeviceID, stream, function, 0, (uint)this.incrementCount.GetCurrentValue( ), data, back );
			OperateResult send = await SendAsync( this.pipeSocket.Socket, command );

			if (!send.IsSuccess && send.ErrorCode < 0) this.pipeSocket.IsSocketError = true;
			return send;
		}


		/// <inheritdoc cref="ISecs.SendByCommand(byte, byte, SecsValue, bool)"/>
		public async Task<OperateResult> SendByCommandAsync( byte stream, byte function, SecsValue data, bool back ) => await SendByCommandAsync( stream, function, data.ToSourceBytes( this.stringEncoding ), back );


		/// <inheritdoc cref="ISecs.ReadSecsMessage(byte, byte, byte[], bool)"/>
		public async Task<OperateResult<SecsMessage>> ReadSecsMessageAsync( byte stream, byte function, byte[] data, bool back )
		{
			uint identityQA = (uint)this.incrementCount.GetCurrentValue( );
			lock (identityQAs) identityQAs.Add( identityQA );

			OperateResult<byte[]> read = await ReadFromCoreServerAsync( Secs1.BuildHSMSMessage( DeviceID, stream, function, 0, identityQA, data, back ) );
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<SecsMessage>( read );

			return OperateResult.CreateSuccessResult( new SecsMessage( read.Content, 4 ) { StringEncoding = this.stringEncoding } );
		}
		/// <inheritdoc cref="ISecs.ReadSecsMessage(byte, byte, SecsValue, bool)"/>
		public async Task<OperateResult<SecsMessage>> ReadSecsMessageAsync( byte stream, byte function, SecsValue data, bool back ) => await ReadSecsMessageAsync( stream, function, data.ToSourceBytes( this.stringEncoding ), back );
#endif
		#endregion

		#region Event Handle

		/// <summary>
		/// Secs消息接收的事件
		/// </summary>
		/// <param name="sender">数据的发送方</param>
		/// <param name="secsMessage">消息内容</param>
		public delegate void OnSecsMessageReceivedDelegate( object sender, SecsMessage secsMessage );

		/// <summary>
		/// 当接收到非应答消息的时候触发的事件
		/// </summary>
		public event OnSecsMessageReceivedDelegate OnSecsMessageReceived;

		#endregion

		#region Object Override

		/// <inheritdoc/>
		public override string ToString( ) => $"SecsHsms[{IpAddress}:{Port}]";

		#endregion

		#region Private Member

		private Encoding stringEncoding = Encoding.Default;
		private SoftIncrementCount incrementCount;                                   // 自增的消息号信息
		private List<uint> identityQAs = new List<uint>( );

		#endregion
	}
}
