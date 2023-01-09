using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using HslCommunication.BasicFramework;

namespace HslCommunication.Core.Net
{
	/// <summary>
	/// 当前的网络会话信息，还包含了一些客户端相关的基本的参数信息<br />
	/// The current network session information also contains some basic parameter information related to the client
	/// </summary>
	public class AppSession : SessionBase
	{
		#region Constructor

		/// <inheritdoc cref="SessionBase.SessionBase()"/>
		public AppSession( )
		{
			ClientUniqueID = SoftBasic.GetUniqueStringByGuidAndRandom( );
		}

		/// <inheritdoc cref="SessionBase.SessionBase(Socket)"/>
		public AppSession( Socket socket ) : base( socket )
		{
			ClientUniqueID = SoftBasic.GetUniqueStringByGuidAndRandom( );
		}

		#endregion

		/// <summary>
		/// 远程对象的别名信息<br />
		/// Alias information for remote objects
		/// </summary>
		public string LoginAlias { get; set; }

		/// <summary>
		/// 客户端唯一的标识，在NetPushServer及客户端类里有使用<br />
		/// The unique identifier of the client, used in the NetPushServer and client classes
		/// </summary>
		public string ClientUniqueID { get; private set; }

		/// <summary>
		/// UDP通信中的远程端<br />
		/// Remote side in UDP communication
		/// </summary>
		internal EndPoint UdpEndPoint = null;

		/// <summary>
		/// 数据内容缓存<br />
		/// data content cache
		/// </summary>
		internal byte[] BytesBuffer { get; set; }

		/// <summary>
		/// 用于关键字分类使用<br />
		/// Used for keyword classification
		/// </summary>
		internal string KeyGroup { get; set; }

		/// <summary>
		/// 当前会话绑定的自定义的对象内容<br />
		/// The content of the custom object bound to the current session
		/// </summary>
		public object Tag { get; set; }

		#region Object Override

		/// <inheritdoc/>
		public override bool Equals( object obj ) => ReferenceEquals( this, obj );

		/// <inheritdoc/>
		public override string ToString( ) => string.IsNullOrEmpty( LoginAlias ) ? $"AppSession[{IpEndPoint}]" : $"AppSession[{IpEndPoint}] [{LoginAlias}]";

		/// <inheritdoc/>
		public override int GetHashCode( ) => base.GetHashCode( );

		#endregion

	}
}
