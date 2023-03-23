using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HslCommunication.Core.Net;
using HslCommunication.Core.IMessage;
using HslCommunication.Core;
using System.Net.Sockets;
#if !NET35 && !NET20
using System.Threading.Tasks;
#endif

namespace HslCommunication.Profinet.OpenProtocol
{
    /// <summary>
    /// 开放以太网协议，在拧紧枪中应用广泛，本通信支持基本的问答机制以及订阅机制，支持完全自定义的参数指定及数据读取。<br />
    /// Open Ethernet protocol, widely used in tightening guns, this communication supports basic question answering mechanism and subscription mechanism, supports fully customized parameter specification and data reading.
    /// </summary>
    /// <remarks>
    /// 自定义的读取使用<see cref="ReadCustomer(int, int, int, int, List{string})"/>来实现，如果是订阅的数据，使用<see cref="OnReceivedOpenMessage"/>绑定自己的方法触发。更详细的示例参考：http://api.hslcommunication.cn<br />
    /// Custom reads are implemented using <see cref="ReadCustomer(int, int, int, int, List{string})"/>, and if it is subscribed data, use <see cref="OnReceivedOpenMessage"/> 
    /// bind your own method to trigger it. For a more detailed example, refer to: http://api.hslcommunication.cn
    /// </remarks>
    /// <example>
    /// <code lang="cs" source="HslCommunication_Net45.Test\Documentation\Samples\Profinet\OpenProtocolNetSample.cs" region="Usage" title="连接及自定义读取使用" />
    /// <code lang="cs" source="HslCommunication_Net45.Test\Documentation\Samples\Profinet\OpenProtocolNetSample.cs" region="Usage2" title="便捷的读取示例" />
    /// <code lang="cs" source="HslCommunication_Net45.Test\Documentation\Samples\Profinet\OpenProtocolNetSample.cs" region="Usage3" title="订阅事件处理操作" />
    /// </example>
    public class OpenProtocolNet : NetworkDoubleBase
	{
		#region Constructor

		/// <summary>
		/// 实例化一个默认的对象<br />
		/// Instantiate a default object
		/// </summary>
		public OpenProtocolNet( ) 
		{
			this.ByteTransform            = new RegularByteTransform( );
			this.UseServerActivePush      = true;
			this.timer                    = new System.Threading.Timer( new System.Threading.TimerCallback( ThreadKeepAlive ), null, 10_000, 10_000 );
			this.parameterSetMessages     = new ParameterSetMessages( this );
			this.jobMessage               = new JobMessage( this );
			this.tighteningResultMessages = new TighteningResultMessages( this );
			this.toolMessages             = new ToolMessages( this );
			this.timeMessages             = new TimeMessages( this );
		}

		/// <summary>
		/// 使用指定的IP地址及端口来初始化对象<br />
		/// Use the specified IP address and port to initialize the object
		/// </summary>
		/// <param name="ipAddress">Ip地址</param>
		/// <param name="port">端口号</param>
		public OpenProtocolNet( string ipAddress, int port = 4545 ) : this( )
		{
			this.IpAddress     = ipAddress;
			this.Port          = port;
		}

		/// <inheritdoc/>
		protected override INetMessage GetNewNetMessage( ) => new OpenProtocolMessage( );

		private void ThreadKeepAlive( object state )
		{
			Core.Pipe.PipeSocket pipeSocket = this.GetPipeSocket( );

			if (pipeSocket != null)
			{
				if (pipeSocket.IsSocketError == false)
				{
					OperateResult<byte[]> command = BuildReadCommand( mid: 9999, revison: 1, stationId: -1, spindleId: -1, parameters: null );
					if (!command.IsSuccess) return;

					Send( pipeSocket.Socket, command.Content );
				}
			}
		}

		#endregion

		#region Override NetworkDoubleBase

		/// <inheritdoc/>
		protected override OperateResult InitializationOnConnect( Socket socket )
		{
			// 此处的 revison 共有三种，1，2，3
			OperateResult<byte[]> command = BuildReadCommand( mid: 1, revison: 1, stationId: -1, spindleId: -1, parameters: null );
			if (!command.IsSuccess) return OperateResult.CreateFailedResult<string>( command );

			OperateResult send = Send( socket, command.Content );
			if (!send.IsSuccess) return OperateResult.CreateFailedResult<string>( send );

			OperateResult<byte[]> receive = ReceiveByMessage( socket, timeOut: this.ReceiveTimeOut, GetNewNetMessage( ) );
			if (!receive.IsSuccess) return OperateResult.CreateFailedResult<string>( receive );

			string reply = Encoding.ASCII.GetString( receive.Content );
			if (reply.Substring( 4, 4 ) == "0002")
				return base.InitializationOnConnect( socket );
			else
				return new OperateResult( "Failed:" + reply.Substring( 4, 4 ) );
		}

		/// <inheritdoc/>
		protected override OperateResult ExtraOnDisconnect( Socket socket )
		{
			OperateResult<byte[]> command = BuildReadCommand( mid: 3, revison: 1, stationId: -1, spindleId: -1, parameters: null );
			if (!command.IsSuccess) return OperateResult.CreateFailedResult<string>( command );

			return ReadFromCoreServer( socket, command.Content );
		}

#if !NET35 && !NET20
		/// <inheritdoc/>
		protected async override Task<OperateResult> InitializationOnConnectAsync( Socket socket )
		{
			// 此处的 revison 共有三种，1，2，3
			OperateResult<byte[]> command = BuildReadCommand( mid: 1, revison: 1, stationId: -1, spindleId: -1, parameters: null );
			if (!command.IsSuccess) return OperateResult.CreateFailedResult<string>( command );

			OperateResult send = await SendAsync( socket, command.Content );
			if (!send.IsSuccess) return OperateResult.CreateFailedResult<string>( send );

			OperateResult<byte[]> receive = await ReceiveByMessageAsync( socket, timeOut: this.ReceiveTimeOut, GetNewNetMessage( ) );
			if (!receive.IsSuccess) return OperateResult.CreateFailedResult<string>( receive );

			string reply = Encoding.ASCII.GetString( receive.Content );
			if (reply.Substring( 4, 4 ) == "0002")
				return await base.InitializationOnConnectAsync( socket );
			else
				return new OperateResult( "Failed:" + reply.Substring( 4, 4 ) );
		}
#endif
		private int DecideSubscribeData( int mid )
		{
			if (mid == 15 || mid == 35 || mid == 52 || mid == 61 || mid == 71 || mid == 74 || mid == 76 || mid == 91 || mid == 101)
				return mid + 1;

			if (mid == 106 || mid == 107)
				return 108;

			if (mid == 121 || mid == 122 || mid == 123 || mid == 124)
				return 125;

			if (mid == 152) return 153;
			if (mid == 211) return 212;
			if (mid == 217) return 218;
			if (mid == 221) return 222;
			if (mid == 242) return 243;
			if (mid == 251) return 252;
			if (mid == 401) return 402;
			if (mid == 421) return 422;
			return -1;
		}

		/// <inheritdoc/>
		protected override bool DecideWhetherQAMessage( Socket socket, OperateResult<byte[]> receive )
		{
			if (receive.Content.Length >= 20)
			{
				int mid = Convert.ToInt32( Encoding.ASCII.GetString( receive.Content, 4, 4 ) );
				bool ack = receive.Content[11] == 0x30;

				if (mid == 9999) return false;                   // 如果是心跳，直接返回

				int id = DecideSubscribeData( mid );
				if (id > 0)
				{
					if (ack) Send( socket, BuildReadCommand( id, 1, -1, -1, null ).Content );
					OnReceivedOpenMessage?.Invoke( this, new OpenEventArgs( Encoding.ASCII.GetString( receive.Content ).TrimEnd( '\0' ) ) );
					return false;
				}
			}
			return base.DecideWhetherQAMessage( socket, receive );
		}

		#endregion

		/// <summary>
		/// 使用自定义的命令读取数据，需要指定每个参数信息，然后返回字符串数据内容，根据实际的功能码，解析出实际的数据信息<br />
		/// To use a custom command to read data, you need to specify each parameter information, then return the string data content, and parse the actual data information according to the actual function code
		/// </summary>
		/// <param name="mid">The MID is four bytes long and is specified by four ASCII digits(‘0’…’9’). The MID describes how to interpret the message.</param>
		/// <param name="revison">The revision of the MID is specified by three ASCII digits(‘0’…’9’).The MID revision is unique per MID and is used in case several versions are available for the same MID. </param>
		/// <param name="stationId">The station the message is addressed to in the case of controller with multi-station configuration.The station ID is 1 byte long and is specified by one ASCII digit(‘0’…’9’). </param>
		/// <param name="spindleId">The spindle the message is addressed to in the case several spindles are connected to the same controller. The spindle ID is 2 bytes long and is specified by two ASCII digits (‘0’…’9’). </param>
		/// <param name="parameters">The Data Field is ASCII data representing the data. The data contains a list of parameters depending on the MID.Each parameter is represented with an ID and the parameter value. </param>
		/// <returns></returns>
		public OperateResult<string> ReadCustomer( int mid, int revison, int stationId, int spindleId, List<string> parameters )
		{
			if (parameters != null) parameters = new List<string>( );
			OperateResult<byte[]> command = BuildReadCommand( mid, revison, stationId, spindleId, parameters );
			if (!command.IsSuccess) return OperateResult.CreateFailedResult<string>( command );

			OperateResult<byte[]> read = ReadFromCoreServer( command.Content );
			if(!read.IsSuccess) return OperateResult.CreateFailedResult<string>( read );

			OperateResult check = CheckRequestReplyMessages( read.Content );
			if (check.IsSuccess) return OperateResult.CreateFailedResult<string>( check );

			return OperateResult.CreateSuccessResult( Encoding.ASCII.GetString( read.Content ) );
		}
#if !NET35 && !NET20
		/// <inheritdoc cref="ReadCustomer(int, int, int, int, List{string})"/>
		public async Task<OperateResult<string>> ReadCustomerAsync( int mid, int revison, int stationId, int spindleId, List<string> parameters )
		{
			if (parameters != null) parameters = new List<string>( );
			OperateResult<byte[]> command = BuildReadCommand( mid, revison, stationId, spindleId, parameters );
			if (!command.IsSuccess) return OperateResult.CreateFailedResult<string>( command );

			OperateResult<byte[]> read = await ReadFromCoreServerAsync( command.Content );
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<string>( read );

			OperateResult check = CheckRequestReplyMessages( read.Content );
			if (check.IsSuccess) return OperateResult.CreateFailedResult<string>( check );

			return OperateResult.CreateSuccessResult( Encoding.ASCII.GetString( read.Content ) );
		}
#endif
		#region Public Properties

		/// <summary>
		/// 参数集合操作的相关属性，可以用来获取参数ID列表，设置数据等操作。<br />
		/// The properties related to parameter collection operations can be used to obtain parameter ID lists, set data, and other operations.
		/// </summary>
		public ParameterSetMessages ParameterSetMessages => this.parameterSetMessages;

		/// <summary>
		/// 任务消息的相关属性，可以用来获取任务的数据，订阅任务，取消订阅任务，选择任务，启动任务。<br />
		/// The relevant properties of task messages can be used to obtain task data, subscribe to tasks, unsubscribe tasks, select tasks, and start tasks.
		/// </summary>
		public JobMessage JobMessage => this.jobMessage;

		/// <summary>
		/// 拧紧结果消息的操作属性
		/// </summary>
		public TighteningResultMessages TighteningResultMessages => this.tighteningResultMessages;

		/// <summary>
		/// 工具消息的操作属性
		/// </summary>
		public ToolMessages ToolMessages => this.toolMessages;

		/// <summary>
		/// 时间消息的属性
		/// </summary>
		public TimeMessages TimeMessages => this.timeMessages;

		#endregion


		#region Private Member

		private System.Threading.Timer timer;
		private ParameterSetMessages parameterSetMessages;
		private JobMessage jobMessage;
		private TighteningResultMessages tighteningResultMessages;
		private ToolMessages toolMessages;
		private TimeMessages timeMessages;

		/// <summary>
		/// 当接收到OpenProtocol协议消息触发的事件
		/// </summary>
		public event EventHandler<OpenEventArgs> OnReceivedOpenMessage;

		#endregion

		#region Object Override

		/// <inheritdoc/>
		public override string ToString( )
		{
			return $"OpenProtocolNet[{IpAddress}:{Port}]";
		}


		#endregion

		/// <summary>
		/// 构建一个读取的初始报文
		/// </summary>
		/// <param name="mid">The MID is four bytes long and is specified by four ASCII digits(‘0’…’9’). The MID describes how to interpret the message.</param>
		/// <param name="revison">The revision of the MID is specified by three ASCII digits(‘0’…’9’).The MID revision is unique per MID and is used in case several versions are available for the same MID. </param>
		/// <param name="stationId">The station the message is addressed to in the case of controller with multi-station configuration.The station ID is 1 byte long and is specified by one ASCII digit(‘0’…’9’). </param>
		/// <param name="spindleId">The spindle the message is addressed to in the case several spindles are connected to the same controller. The spindle ID is 2 bytes long and is specified by two ASCII digits (‘0’…’9’). </param>
		/// <param name="parameters">The Data Field is ASCII data representing the data. The data contains a list of parameters depending on the MID.Each parameter is represented with an ID and the parameter value. </param>
		/// <returns>原始字节的报文信息</returns>
		public static OperateResult<byte[]> BuildReadCommand( int mid, int revison, int stationId, int spindleId, List<string> parameters )
		{
			if (mid < 0 || mid > 9999) return new OperateResult<byte[]>( "Mid must be between 0 - 9999" );
			if (revison < 0 || revison > 999) return new OperateResult<byte[]>( "revison must be between 0 - 999" );
			if (stationId > 9) return new OperateResult<byte[]>( "stationId must be between 0 - 9" );
			if (spindleId > 99) return new OperateResult<byte[]>( "spindleId must be between 0 - 99" );

			int count = 0;
			if (parameters != null)
				parameters.ForEach( m => count += m.Length );

			StringBuilder sb = new StringBuilder( );
			sb.Append( (20 + count).ToString( "D4" ) );
			sb.Append( mid.ToString( "D4" ) );
			sb.Append( revison.ToString( "D3" ) );
			sb.Append( '0' );                                // No ack flag: 0
			sb.Append( stationId < 0 ? " " : stationId.ToString( "D1" ) );
			sb.Append( spindleId < 0 ? "  " : spindleId.ToString( "D2" ) );
			sb.Append( ' ' );
			sb.Append( ' ' );
			sb.Append( ' ' );
			sb.Append( ' ' );
			sb.Append( ' ' );

			if (parameters != null)
				for (int i = 0; i < parameters.Count; i++)
				{
					sb.Append( parameters[i] );
				}
			sb.Append( '\0' );
			return OperateResult.CreateSuccessResult( Encoding.ASCII.GetBytes( sb.ToString( ) ) );
		}

		private static string GetErrorText( int code )
		{
			switch (code)
			{
				case 1: return "Invalid data";
				case 2: return "Parameter set ID not present";
				case 3: return "Parameter set can not be set.";
				case 4: return "Parameter set not running";
				case 06: return "VIN upload subscription already exists";
				case 07: return "VIN upload subscription does not exists";
				case 08: return "VIN input source not granted";
				case 09: return "Last tightening result subscription already exists";
				case 10: return "Last tightening result subscription does not exist";
				case 11: return "Alarm subscription already exists";
				case 12: return "Alarm subscription does not exist";
				case 13: return "Parameter set selection subscription already exists";
				case 14: return "Parameter set selection subscription does not exist";
				case 15: return "Tightening ID requested not found";
				case 16: return "Connection rejected protocol busy";
				case 17: return "Job ID not present";
				case 18: return "Job info subscription already exists";
				case 19: return "Job info subscription does not exist";
				case 20: return "Job can not be set";
				case 21: return "Job not running";
				case 22: return "Not possible to execute dynamic Job request";
				case 23: return "Job batch decrement failed";
				case 30: return "Controller is not a sync Master/station controller";
				case 31: return "Multi-spindle status subscription already exists";
				case 32: return "Multi-spindle status subscription does not exist";
				case 33: return "Multi-spindle result subscription already exists";
				case 34: return "Multi-spindle result subscription does not exist";
				case 40: return "Job line control info subscription already exists";
				case 41: return "Job line control info subscription does not exist";
				case 42: return "Identifier input source not granted";
				case 43: return "Multiple identifiers work order subscription already exists";
				case 44: return "Multiple identifiers work order subscription does not exist";
				case 50: return "Status external monitored inputs subscription already exists";
				case 51: return "Status external monitored inputs subscription does not exist";
				case 52: return "IO device not connected";
				case 53: return "Faulty IO device ID";
				case 58: return "No alarm present";
				case 59: return "Tool currently in use";
				case 60: return "No histogram available";
				case 70: return "Calibration failed";
				case 79: return "Command failed";
				case 80: return "Audi emergency status subscription exists";
				case 81: return "Audi emergency status subscription does not exist";
				case 82: return "Automatic/Manual mode subscribe already exist";
				case 83: return "Automatic/Manual mode subscribe does not exist";
				case 84: return "The relay function subscription already exists";
				case 85: return "The relay function subscription does not exist";
				case 86: return "The selector socket info subscription already exist";
				case 87: return "The selector socket info subscription does not exist";
				case 88: return "The digin info subscription already exist";
				case 89: return "The digin info subscription does not exist";
				case 90: return "Lock at bach done subscription already exist";
				case 91: return "Lock at bach done subscription does not exist";
				case 92: return "Open protocol commands disabled";
				case 93: return "Open protocol commands disabled subscription already exists";
				case 94: return "Open protocol commands disabled subscription does not exist";
				case 95: return "Reject request, PowerMACS is in manual mode";
				case 96: return "Client already connected";
				case 97: return "MID revision unsupported";
				case 98: return "Controller internal request timeout";
				case 99: return "Unknown MID";
				default: return StringResources.Language.UnknownError;
			}
		}

		/// <summary>
		/// 检查请求返回的消息是否合法的
		/// </summary>
		/// <param name="reply">返回的消息</param>
		/// <returns>是否合法的结果对象</returns>
		public static OperateResult CheckRequestReplyMessages( byte[] reply )
		{
			try
			{
				if (Encoding.ASCII.GetString( reply, 4, 4 ) == "0004")
				{
					string mid = Encoding.ASCII.GetString( reply, 20, 4 );
					int code = Convert.ToInt32( Encoding.ASCII.GetString( reply, 24, 2 ) );

					if (code == 0) return OperateResult.CreateSuccessResult( );
					return new OperateResult( code, $"The request MID {mid} Select parameter set failed: " + GetErrorText( code ) );
				}
				return OperateResult.CreateSuccessResult( );
			}
			catch( Exception e )
			{
				return new OperateResult( e.Message );
			}
		}
	}
}
