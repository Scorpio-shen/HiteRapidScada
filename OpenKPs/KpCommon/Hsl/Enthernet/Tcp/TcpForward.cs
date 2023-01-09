using HslCommunication.Core.Net;
using HslCommunication.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
#if !NET35 && !NET20
using System.Threading.Tasks;
#endif

namespace HslCommunication.Enthernet
{
	/// <summary>
	/// 用于转发的TCP服务类，可以用来实现TCP协议的转发功能，需要指定本机端口，服务器的ip及端口信息<br />
	/// The TCP service class used for forwarding can be used to implement the forwarding function of the TCP protocol. It is necessary to specify the local port, the server's ip and port information.
	/// </summary>
	public class TcpForward : NetworkServerBase
	{
		#region Constructor

		/// <summary>
		/// 实例化一个TCP转发的对象，需要本机端口号，远程ip地址及远程端口号
		/// </summary>
		/// <param name="localPort">本机侦听的端口号</param>
		/// <param name="host">远程的IP地址</param>
		/// <param name="hostPort">远程的端口号信息</param>
		public TcpForward( int localPort, string host, int hostPort )
		{
			this._forwards = new List<ForwardSession>( );
			this.Port      = localPort;
			this._hostIp   = host;
			this._port     = hostPort;
			this.ConnectTimeOut = 5000;
		}

		#endregion

		#region Public Properties

		/// <inheritdoc cref="NetworkDoubleBase.ConnectTimeOut"/>
		[HslMqttApi( HttpMethod = "GET", Description = "Gets or sets the timeout for the connection, in milliseconds" )]
		public virtual int ConnectTimeOut
		{
			get;
			set;
		}

		/// <summary>
		/// 获取当前的用于中转数据的会话数量
		/// </summary>
		public int OnlineSessionsCount => _forwards.Count;

		#endregion

		#region Override Method

		/// <inheritdoc/>
		protected override void CloseAction( )
		{
			lock (_lock)
			{
				foreach (ForwardSession forward in _forwards)
				{
					forward.ServerSocket?.Close( );
					forward.WorkSocket?.Close( );
				}
				_forwards.Clear( );
			}
		}

		/// <inheritdoc/>
		protected override void ThreadPoolLogin( Socket socket, IPEndPoint endPoint )
		{
			// 有客户端登录了，现在要尝试连接服务器
			LogNet?.WriteInfo( ToString( ), $"Local client[{endPoint}] connected" );
			OperateResult<Socket> serverConnect = CreateSocketAndConnect( _hostIp, _port, this.ConnectTimeOut );
			if (!serverConnect.IsSuccess)
			{
				LogNet?.WriteError( ToString( ), $"Connect server failed, local client close: {serverConnect.Message}" );
				socket?.Close( );
				return;
			}
			LogNet?.WriteInfo( ToString( ), $"Connect [{_hostIp}:{_port}] success" );

			ForwardSession sessionForward = new ForwardSession( socket );
			sessionForward.ServerSocket   = serverConnect.Content;

			// 先开启server接收
			try
			{
				sessionForward.ServerSocket.BeginReceive( sessionForward.ServerBuffer, 0, sessionForward.ServerBuffer.Length, SocketFlags.None, new AsyncCallback( ServerReceiveAsync ), sessionForward );
			}
			catch ( Exception ex )
			{
				LogNet?.WriteError( ToString( ), $"Server begin receive failed, local client close. {ex.Message}" );
				socket?.Close( );
				serverConnect.Content?.Close( );
				return;
			}

			// 再开启client接收
			try
			{
				sessionForward.WorkSocket.BeginReceive( sessionForward.BytesBuffer, 0, sessionForward.BytesBuffer.Length, SocketFlags.None, new AsyncCallback( LocalReceiveAsync ), sessionForward );
			}
			catch (Exception ex)
			{
				LogNet?.WriteError( ToString( ), $"Local begin receive failed, server close. {ex.Message}" );
				socket?.Close( );
				serverConnect.Content?.Close( );
				return;
			}

			// 都开启成功添加列表
			lock (_lock)
			{
				_forwards.Add( sessionForward );
			}
		}

		#endregion

		#region Event Hander

		/// <summary>
		/// 接收消息触发的委托信息
		/// </summary>
		/// <param name="session">会话对象</param>
		/// <param name="data">原始报文数据信息</param>
		public delegate void OnMessageReceivedDelegate( ForwardSession session, byte[] data );

		/// <summary>
		/// 当接收到远程的数据触发的事件
		/// </summary>
		public event OnMessageReceivedDelegate OnRemoteMessageReceived;

		/// <summary>
		/// 当接收到客户端数据触发的事件
		/// </summary>
		public event OnMessageReceivedDelegate OnClientMessageReceive;

		#endregion

		#region Socket Receive Async

		private void RemoveSession( ForwardSession session, string message )
		{
			bool remove = false;
			lock (_lock)
			{
				remove = _forwards.Remove( session );
			}
			if (remove)
			{
				if (!string.IsNullOrEmpty( message )) 
					LogNet?.WriteInfo( ToString( ), message );
				session.WorkSocket?.Close( );
				session.ServerSocket?.Close( );
			}
		}

		private void ServerReceiveAsync(IAsyncResult ar )
		{
			if(ar.AsyncState is ForwardSession session)
			{
				int length = 0;
				try
				{
					length = session.ServerSocket.EndReceive( ar );
				}
				catch (ObjectDisposedException)
				{
					// 通常是远程客户端自己关闭的
					RemoveSession( session, string.Empty );
				}
				catch( Exception ex)
				{
					RemoveSession( session, $"Server socket endreceive failed: {ex.Message}" );
					return;
				}

				if ( length == 0)
				{
					RemoveSession( session, $"Server socket [{_hostIp}:{_port}], local closed" );
					return;
				}
				byte[] buffer = session.ServerBuffer.SelectBegin( length );

				// 服务端重新开启接收
				try
				{
					session.ServerSocket.BeginReceive( session.ServerBuffer, 0, session.ServerBuffer.Length, SocketFlags.None, new AsyncCallback( ServerReceiveAsync ), session );
				}
				catch (Exception ex)
				{
					RemoveSession( session, $"Server socket beginReceive failed, local client close. {ex.Message}" );
					return;
				}

				// 发送给客户端
				LogBuffer( "Remote->Client", buffer );
				OnRemoteMessageReceived?.Invoke( session, buffer );
				try
				{
					session.WorkSocket.Send( buffer );
				}
				catch ( Exception ex)
				{
					RemoveSession( session, $"Local send failed, server closed: {ex.Message}" );
					return;
				}
			}
		}

		private void LocalReceiveAsync( IAsyncResult ar )
		{
			if (ar.AsyncState is ForwardSession session)
			{
				int length = 0;
				try
				{
					length = session.WorkSocket.EndReceive( ar );
				}
				catch (Exception ex)
				{
					RemoveSession( session, $"local socket endreceive failed: {ex.Message}" );
					return;
				}

				if (length == 0)
				{
					RemoveSession( session, $"local socket closed[{session.IpEndPoint}], server[{_hostIp}:{_port}] closed" );
					return;
				}
				byte[] buffer = session.BytesBuffer.SelectBegin( length );

				// 客户端重新开启接收
				try
				{
					session.WorkSocket.BeginReceive( session.BytesBuffer, 0, session.BytesBuffer.Length, SocketFlags.None, new AsyncCallback( LocalReceiveAsync ), session );
				}
				catch (Exception ex)
				{
					RemoveSession( session, $"local socket beginReceive failed, server socket close. {ex.Message}" );
					return;
				}

				// 发送给服务端
				LogBuffer( "Client->Remote", buffer );
				OnClientMessageReceive?.Invoke( session, buffer );
				try
				{
					session.ServerSocket.Send( buffer );
				}
				catch (Exception ex)
				{
					RemoveSession( session, $"Server send failed, local closed: {ex.Message}" );
					return;
				}
			}
		}

		#endregion

		#region Private Method

		private void LogBuffer( string info, byte[] buffer )
		{
			if (LogMsgFormatBinary)
				LogNet?.WriteInfo( ToString( ), $"[{info}] {HslCommunication.BasicFramework.SoftBasic.ByteToHexString( buffer, ' ' )}" );
			else
				LogNet?.WriteInfo( ToString( ), $"[{info}] {HslCommunication.BasicFramework.SoftBasic.GetAsciiStringRender( buffer )}" );
		}

		#endregion


		/// <inheritdoc cref="NetworkDoubleBase.LogMsgFormatBinary"/>
		public bool LogMsgFormatBinary { get; set; } = true;

		/// <summary>
		/// 获取所有的会话信息
		/// </summary>
		/// <returns>方便用于显示</returns>
		public ForwardSession[] GetSessionInfos( )
		{
			ForwardSession[] infos;
			lock (_lock)
			{
				infos = _forwards.ToArray( );
			}
			return infos;
		}

		#region Object Override

		/// <inheritdoc/>
		public override string ToString( ) => $"TcpForward[{Port}->{_hostIp}:{_port}]";

		#endregion

		#region Private Member

		private string _hostIp = string.Empty;             // 服务端的
		private int _port = 0;                             // 服务端的端口信息
		private List<ForwardSession> _forwards;            // 当前连接的所有的对象信息
		private object _lock = new object( );

		#endregion
	}
}
