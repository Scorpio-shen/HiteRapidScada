using System;
using System.Collections.Generic;
using System.Linq;

namespace HslCommunication.Core.Pipe
{
	/// <summary>
	/// 管道的基础类对象
	/// </summary>
	public class PipeBase :IDisposable
	{
		/// <summary>
		/// 实例化一个默认的对象
		/// </summary>
		public PipeBase( )
		{
			this.hybirdLock = new SimpleHybirdLock( );
		}

		/// <inheritdoc cref="SimpleHybirdLock.Enter()"/>
		public bool PipeLockEnter( ) => this.hybirdLock.Enter( );

		/// <inheritdoc cref="SimpleHybirdLock.Leave"/>
		public bool PipeLockLeave( ) => this.hybirdLock.Leave( );

		/// <inheritdoc cref="IDisposable.Dispose"/>
		public virtual void Dispose( )
		{
			this.hybirdLock?.Dispose( );
		}

		private SimpleHybirdLock hybirdLock;                     // 通道交互的锁，通道里要求每个通信都是唯一的
	}
}
