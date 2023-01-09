using HslCommunication.Core.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using HslCommunication.BasicFramework;

namespace HslCommunication.Enthernet
{
	/// <summary>
	/// 转发过程中的中间会话信息
	/// </summary>
	public class ForwardSession : AppSession
	{
		/// <summary>
		/// 实例化一个默认的对象
		/// </summary>
		public ForwardSession( )
		{
			this.ServerBuffer = new byte[2048];
			this.BytesBuffer  = new byte[2048];
		}

		/// <summary>
		/// 指定客户端的 socket 来实例化一个对象
		/// </summary>
		/// <param name="socket">客户端的socket</param>
		public ForwardSession( Socket socket ) : base( socket )
		{
			this.ServerBuffer = new byte[2048];
			this.BytesBuffer  = new byte[2048];
		}

		/// <summary>
		/// 连接服务端的socket
		/// </summary>
		public Socket ServerSocket { get; set; }

		/// <summary>
		/// 服务端的缓存数据信息
		/// </summary>
		public byte[] ServerBuffer { get; set; }

		/// <inheritdoc/>
		public override string ToString( )
		{
			return $"Server[{ServerSocket.RemoteEndPoint}] Local[{IpEndPoint}] Online:{SoftBasic.GetTimeSpanDescription(DateTime.Now - OnlineTime)}";
		}

	}
}
