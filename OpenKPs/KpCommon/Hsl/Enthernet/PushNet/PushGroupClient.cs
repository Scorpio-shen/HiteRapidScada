using HslCommunication.Core.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HslCommunication.Core;




namespace HslCommunication.Enthernet
{
	/// <summary>
	/// 订阅分类的核心组织对象
	/// </summary>
	public class PushGroupClient : IDisposable
	{
		#region Constructor

		/// <summary>
		/// 实例化一个默认的对象
		/// </summary>
		public PushGroupClient()
		{
			appSessions = new List<AppSession>( );
			simpleHybird = new SimpleHybirdLock( );
		}

		#endregion

		#region Public Method

		/// <summary>
		/// 新增一个订阅的会话
		/// </summary>
		/// <param name="session">会话</param>
		public void AddPushClient(AppSession session)
		{
			simpleHybird.Enter( );
			appSessions.Add( session );
			simpleHybird.Leave( );
		}

		/// <summary>
		/// 移除一个订阅的会话
		/// </summary>
		/// <param name="clientID">客户端唯一的ID信息</param>
		public bool RemovePushClient( string clientID )
		{
			bool result = false;
			simpleHybird.Enter( );
			for (int i = 0; i < appSessions.Count; i++)
			{
				if(appSessions[i].ClientUniqueID == clientID)
				{
					appSessions[i].WorkSocket?.Close( );
					appSessions.RemoveAt( i );
					result = true;
					break;
				}
			}
			simpleHybird.Leave( );

			return result;
		}

		/// <summary>
		/// 使用固定的发送方法将数据发送出去
		/// </summary>
		/// <param name="content">数据内容</param>
		/// <param name="send">指定的推送方法</param>
		public void PushString( string content, Action<AppSession, string> send )
		{
			simpleHybird.Enter( );

			System.Threading.Interlocked.Increment( ref pushTimesCount );
			for (int i = 0; i < appSessions.Count; i++)
			{
				send( appSessions[i], content );
			}
			simpleHybird.Leave( );
		}

		/// <summary>
		/// 移除并关闭所有的客户端
		/// </summary>
		public int RemoveAllClient( )
		{
			int result = 0;
			simpleHybird.Enter( );

			for (int i = 0; i < appSessions.Count; i++)
			{
				appSessions[i].WorkSocket?.Close( );
			}

			result = appSessions.Count;

			appSessions.Clear( );
			simpleHybird.Leave( );

			return result;
		}

		/// <summary>
		/// 获取是否推送过数据
		/// </summary>
		/// <returns>True代表有，False代表没有</returns>
		public bool HasPushedContent( )
		{
			return pushTimesCount > 0L;
		}

		#endregion

		#region Private Member

		private List<AppSession> appSessions;               // 所有的客户端信息
		private SimpleHybirdLock simpleHybird;              // 列表的锁
		private long pushTimesCount = 0L;                   // 推送的次数总和

		#endregion
		
		#region IDisposable Support

		private bool disposedValue = false; // 要检测冗余调用

		/// <summary>
		/// 释放当前的程序所占用的资源
		/// </summary>
		/// <param name="disposing">是否释放资源</param>
		protected virtual void Dispose( bool disposing )
		{
			if (!disposedValue)
			{
				if (disposing)
				{
				}

				simpleHybird.Enter( );
				appSessions.ForEach( m => m.WorkSocket?.Close( ) );
				appSessions.Clear( );
				simpleHybird.Leave( );
				simpleHybird.Dispose( );
				disposedValue = true;
			}
		}

		/// <summary>
		/// 释放当前的对象所占用的资源
		/// </summary>
		public void Dispose()
		{
			// 请勿更改此代码。将清理代码放入以上 Dispose(bool disposing) 中。
			Dispose( true );
			// GC.SuppressFinalize(this);
		}

		#endregion

		#region Object Override

		/// <inheritdoc/>
		public override string ToString() => "PushGroupClient";
		
		#endregion
	}
}
