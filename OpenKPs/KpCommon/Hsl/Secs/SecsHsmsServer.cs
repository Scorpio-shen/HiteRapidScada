using HslCommunication.BasicFramework;
using HslCommunication.Core.IMessage;
using HslCommunication.Core.Net;
using HslCommunication.Secs.Helper;
using HslCommunication.Secs.Message;
using HslCommunication.Secs.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
#if !NET20 && !NET35
using System.Threading.Tasks;
#endif

namespace HslCommunication.Secs
{
	/// <summary>
	/// Secs Hsms的虚拟服务器，可以用来模拟Secs设备，等待客户端的连接，自定义响应客户端的数据
	/// </summary>
	/// <remarks>
	/// </remarks>
	/// <example>
	/// 下面就看看基本的操作内容
	/// <code lang="cs" source="HslCommunication_Net45.Test\Documentation\Samples\Secs\SecsGemSample.cs" region="Server Sample" title="基本的使用" />
	/// 关于<see cref="SecsValue"/>类型，可以非常灵活的实例化，参考下面的示例代码
	/// <code lang="cs" source="HslCommunication_Net45.Test\Documentation\Samples\Secs\SecsGemSample.cs" region="Sample2" title="SecsValue说明" />
	/// </example>
	public class SecsHsmsServer : NetworkAuthenticationServerBase
	{
		#region Constructor

		/// <summary>
		/// 实例化一个默认的对象
		/// </summary>
		public SecsHsmsServer( )
		{
			this.incrementCount = new SoftIncrementCount( uint.MaxValue );
		}

		#endregion

		/// <summary>
		/// 获取或设置用于字符串解析的编码信息
		/// </summary>
		public Encoding StringEncoding { get => this.stringEncoding; set => this.stringEncoding = value; }

		#region NetServer Support

		/// <summary>
		/// 从远程Socket异步接收的数据信息
		/// </summary>
		/// <param name="ar">异步接收的对象</param>
#if NET20 || NET35
		protected override void SocketAsyncCallBack( IAsyncResult ar )
#else
		protected override async void SocketAsyncCallBack( IAsyncResult ar )
#endif
		{
			if (ar.AsyncState is AppSession session)
			{
				try
				{
					int receiveCount = session.WorkSocket.EndReceive( ar );
					OperateResult<byte[]> read1;

					if (!Authorization.asdniasnfaksndiqwhawfskhfaiw( )) { RemoveClient( session ); return; };

#if NET20 || NET35
					read1 = ReceiveByMessage( session.WorkSocket, 5000, new SecsHsmsMessage( ) );
#else
					read1 = await ReceiveByMessageAsync( session.WorkSocket, 5000, new SecsHsmsMessage( ) );
#endif
					if (!read1.IsSuccess) { RemoveClient( session ); return; };

					SecsMessage secsMessage = new SecsMessage( read1.Content, 4 );
					secsMessage.StringEncoding = this.stringEncoding;

					LogNet?.WriteDebug( ToString( ), $"[{session.IpEndPoint}] Tcp {StringResources.Language.Receive}：{secsMessage}" );

					session.UpdateHeartTime( );
					// 触发事件
					if (secsMessage.StreamNo == 0 && secsMessage.FunctionNo == 0 && secsMessage.BlockNo % 2 == 1)
						Send( session.WorkSocket, Secs1.BuildHSMSMessage( ushort.MaxValue, 0, 0, (ushort)(secsMessage.BlockNo + 1), secsMessage.MessageID, null, false ) );

					RaiseDataReceived( this, session, secsMessage );
					session.WorkSocket.BeginReceive( new byte[0], 0, 0, SocketFlags.None, new AsyncCallback( SocketAsyncCallBack ), session );
				}
				catch (Exception ex)
				{
					RemoveClient( session, $"SocketAsyncCallBack Exception -> {ex.Message}" );
				}
			}
		}

		#endregion

		#region Event Handler

		/// <summary>
		/// 当接收到来自客户的Secs信息时触发的对象<br />
		/// Object fired when Secs information from client is received
		/// </summary>
		/// <param name="sender">触发的服务器对象</param>
		/// <param name="session">消息的会话对象信息</param>
		/// <param name="message">实际的数据信息</param>
		public delegate void SecsMessageReceivedDelegate( object sender, AppSession session, SecsMessage message );

		/// <summary>
		/// 接收到数据的时候就触发的事件，示例详细参考API文档信息<br />
		/// An event that is triggered when data is received
		/// </summary>
		/// <remarks>
		/// 事件共有三个参数，sender指服务器本地的对象，为 <see cref="SecsHsmsServer"/> 对象，session 指会话对象，网为 <see cref="AppSession"/>，message 为收到的原始数据 <see cref="SecsMessage"/> 对象
		/// </remarks>
		/// <example>
		/// </example>
		public event SecsMessageReceivedDelegate OnSecsMessageReceived;

		/// <summary>
		/// 触发一个数据接收的事件信息<br />
		/// Event information that triggers a data reception
		/// </summary>
		/// <param name="source">数据的发送方</param>
		/// <param name="session">消息的会话对象信息</param>
		/// <param name="message">实际的数据信息</param>
		private void RaiseDataReceived( object source, AppSession session, SecsMessage message ) => OnSecsMessageReceived?.Invoke( source, session, message );

		#endregion

		#region Public Method

		/// <summary>
		/// 向指定的会话信息发送SECS格式的原始字节数据信息，session 为当前的会话对象，receiveMessage为接收到数据，后续的参数才是真实的返回数据
		/// </summary>
		/// <param name="session">当前的会话对象</param>
		/// <param name="receiveMessage">接收到的Secs数据</param>
		/// <param name="stream">功能码1</param>
		/// <param name="function">功能码2</param>
		/// <param name="data">原始的字节数据</param>
		/// <returns>是否发送成功</returns>
		public OperateResult SendByCommand( AppSession session, SecsMessage receiveMessage, byte stream, byte function, byte[] data )
		{
			byte[] command = Secs1.BuildHSMSMessage( receiveMessage.DeviceID, stream, function, 0, receiveMessage.MessageID, data, false );
			return Send( session.WorkSocket, command );
		}

		/// <inheritdoc cref="SendByCommand(AppSession, SecsMessage, byte, byte, byte[])"/>
		public OperateResult SendByCommand( AppSession session, SecsMessage receiveMessage, byte stream, byte function, SecsValue data ) => 
			SendByCommand( session, receiveMessage, stream, function, data == null ? new byte[0] : data.ToSourceBytes( this.stringEncoding ) );

		/// <summary>
		/// 发布数据到所有的在线客户端信息
		/// </summary>
		/// <param name="stream">功能码1</param>
		/// <param name="function">功能码2</param>
		/// <param name="data">数据对象</param>
		/// <returns>是否发送成功</returns>
		public OperateResult PublishSecsMessage( byte stream, byte function, SecsValue data )
		{
			AppSession[] sessions = GetOnlineSessions;
			for (int i = 0; i < sessions.Length; i++)
			{
				byte[] command = Secs1.BuildHSMSMessage( 0x01, stream, function, 0, (uint)this.incrementCount.GetCurrentValue( ), data.ToSourceBytes( this.stringEncoding ), false );
				OperateResult send = Send( sessions[i].WorkSocket, command );

				if (!send.IsSuccess) return send;
			}
			return OperateResult.CreateSuccessResult( );
		}

		#endregion

		#region Private Member

		private Encoding stringEncoding = Encoding.Default;                          // 字符串的编码信息
		private SoftIncrementCount incrementCount;                                   // 自增的消息号信息

		#endregion

		#region Object Override

		/// <inheritdoc/>
		public override string ToString( ) => $"SecsHsmsServer[{Port}]";

		#endregion


	}
}
