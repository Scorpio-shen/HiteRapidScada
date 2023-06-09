﻿using HslCommunication.Core;
using HslCommunication.Reflection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
#if !NET35 && !NET20
using System.Threading.Tasks;
#endif

namespace HslCommunication.LogNet
{
	/// <summary>
	/// 日志存储类的基类，提供一些基础的服务
	/// </summary>
	/// <remarks>
	/// 基于此类可以实现任意的规则的日志存储规则，欢迎大家补充实现，本组件实现了3个日志类
	/// <list type="number">
	/// <item>单文件日志类 <see cref="LogNetSingle"/></item>
	/// <item>根据文件大小的类 <see cref="LogNetFileSize"/></item>
	/// <item>根据时间进行存储的类 <see cref="LogNetDateTime"/></item>
	/// </list>
	/// </remarks>
	public abstract class LogNetBase : IDisposable
	{
		#region Constructor

		/// <summary>
		/// 实例化一个日志对象<br />
		/// Instantiate a log object
		/// </summary>
		public LogNetBase( )
		{
			m_fileSaveLock = new SimpleHybirdLock( );
			m_simpleHybirdLock = new SimpleHybirdLock( );
			m_WaitForSave = new Queue<HslMessageItem>( );
			filtrateKeyword = new List<string>( );
			filtrateLock = new SimpleHybirdLock( );
		}

		#endregion

		#region Private Member

		/// <summary>
		/// 文件存储的锁
		/// </summary>
		protected SimpleHybirdLock m_fileSaveLock;                                             // 文件的锁
		private HslMessageDegree m_messageDegree = HslMessageDegree.DEBUG;                     // 默认的存储规则
		private Queue<HslMessageItem> m_WaitForSave;                                           // 待存储数据的缓存
		private SimpleHybirdLock m_simpleHybirdLock;                                           // 缓存列表的锁
		private int m_SaveStatus = 0;                                                          // 存储状态
		private List<string> filtrateKeyword;                                                  // 需要过滤的存储对象
		private SimpleHybirdLock filtrateLock;                                                 // 过滤列表的锁
		private string lastLogSaveFileName = string.Empty;                                     // 上一次存储的文件的名字

		#endregion

		#region Event Handle

		/// <inheritdoc cref="ILogNet.BeforeSaveToFile"/>
		public event EventHandler<HslEventArgs> BeforeSaveToFile = null;

		private void OnBeforeSaveToFile( HslEventArgs args ) => BeforeSaveToFile?.Invoke( this, args );

		#endregion

		#region Public Member

		/// <inheritdoc cref="ILogNet.LogSaveMode"/>
		public LogSaveMode LogSaveMode { get; protected set; }

		/// <inheritdoc cref="ILogNet.LogNetStatistics"/>
		public LogStatistics LogNetStatistics { get; set; }

		/// <inheritdoc cref="ILogNet.ConsoleOutput"/>
		public bool ConsoleOutput { get; set; }

		/// <inheritdoc cref="ILogNet.LogThreadID"/>
		public bool LogThreadID { get; set; } = true;

		/// <inheritdoc cref="ILogNet.LogStxAsciiCode"/>
		public bool LogStxAsciiCode { get; set; } = true;

		/// <inheritdoc cref="ILogNet.HourDeviation"/>
		public int HourDeviation { get; set; } = 0;

		#endregion

		#region Log Method

		/// <inheritdoc cref="ILogNet.WriteDebug(string)"/>
		[HslMqttApi]
		public void WriteDebug( string text ) => WriteDebug( string.Empty, text );

		/// <inheritdoc cref="ILogNet.WriteDebug(string, string)"/>
		[HslMqttApi(ApiTopic = "WriteDebugKeyWord" )]
		public void WriteDebug( string keyWord, string text ) => RecordMessage( HslMessageDegree.DEBUG, keyWord, text );

		/// <inheritdoc cref="ILogNet.WriteInfo(string)"/>
		[HslMqttApi]
		public void WriteInfo( string text ) => WriteInfo( string.Empty, text );

		/// <inheritdoc cref="ILogNet.WriteInfo(string, string)"/>
		[HslMqttApi( ApiTopic = "WriteInfoKeyWord" )]
		public void WriteInfo( string keyWord, string text ) => RecordMessage( HslMessageDegree.INFO, keyWord, text );

		/// <inheritdoc cref="ILogNet.WriteWarn(string)"/>
		[HslMqttApi]
		public void WriteWarn( string text ) => WriteWarn( string.Empty, text );

		/// <inheritdoc cref="ILogNet.WriteWarn(string, string)"/>
		[HslMqttApi( ApiTopic = "WriteWarnKeyWord" )]
		public void WriteWarn( string keyWord, string text ) => RecordMessage( HslMessageDegree.WARN, keyWord, text );

		/// <inheritdoc cref="ILogNet.WriteError(string)"/>
		[HslMqttApi]
		public void WriteError( string text ) => WriteError( string.Empty, text );

		/// <inheritdoc cref="ILogNet.WriteError(string, string)"/>
		[HslMqttApi( ApiTopic = "WriteErrorKeyWord" )]
		public void WriteError( string keyWord, string text ) => RecordMessage( HslMessageDegree.ERROR, keyWord, text );

		/// <inheritdoc cref="ILogNet.WriteFatal(string)"/>
		[HslMqttApi]
		public void WriteFatal( string text ) => WriteFatal( string.Empty, text );

		/// <inheritdoc cref="ILogNet.WriteFatal(string, string)"/>
		[HslMqttApi( ApiTopic = "WriteFatalKeyWord" )]
		public void WriteFatal( string keyWord, string text ) => RecordMessage( HslMessageDegree.FATAL, keyWord, text );

		/// <inheritdoc cref="ILogNet.WriteException(string, Exception)"/>
		public void WriteException( string keyWord, Exception ex ) => WriteException( keyWord, string.Empty, ex );

		/// <inheritdoc cref="ILogNet.WriteException(string, string, Exception)"/>
		public void WriteException( string keyWord, string text, Exception ex ) => RecordMessage( HslMessageDegree.FATAL, keyWord, LogNetManagment.GetSaveStringFromException( text, ex ) );

		/// <inheritdoc cref="ILogNet.RecordMessage(HslMessageDegree, string, string)"/>
		public void RecordMessage( HslMessageDegree degree, string keyWord, string text ) => WriteToFile( degree, keyWord, text );

		/// <inheritdoc cref="ILogNet.WriteDescrition(string)"/>
		[HslMqttApi]
		public void WriteDescrition( string description )
		{
			if (string.IsNullOrEmpty( description )) return;

			// 和上面的文本之间追加一行空行
			StringBuilder stringBuilder = new StringBuilder( "\u0002" );
			stringBuilder.Append( Environment.NewLine );
			stringBuilder.Append( "\u0002/" );

			int count = 118 - CalculateStringOccupyLength( description );
			if (count >= 8)
			{
				int count_1 = (count - 8) / 2;
				AppendCharToStringBuilder( stringBuilder, '*', count_1 );
				stringBuilder.Append( "   " );
				stringBuilder.Append( description );
				stringBuilder.Append( "   " );
				if (count % 2 == 0)
				{
					AppendCharToStringBuilder( stringBuilder, '*', count_1 );
				}
				else
				{
					AppendCharToStringBuilder( stringBuilder, '*', count_1 + 1 );
				}
			}
			else if (count >= 2)
			{
				int count_1 = (count - 2) / 2;
				AppendCharToStringBuilder( stringBuilder, '*', count_1 );
				stringBuilder.Append( description );
				if (count % 2 == 0)
				{
					AppendCharToStringBuilder( stringBuilder, '*', count_1 );
				}
				else
				{
					AppendCharToStringBuilder( stringBuilder, '*', count_1 + 1 );
				}
			}
			else
			{
				stringBuilder.Append( description );
			}

			stringBuilder.Append( "/" );
			stringBuilder.Append( Environment.NewLine );
			RecordMessage( HslMessageDegree.None, string.Empty, stringBuilder.ToString( ) );
		}

		/// <inheritdoc cref="ILogNet.WriteAnyString(string)"/>
		[HslMqttApi]
		public void WriteAnyString( string text ) => RecordMessage( HslMessageDegree.None, string.Empty, text );

		/// <inheritdoc cref="ILogNet.WriteNewLine"/>
		[HslMqttApi]
		public void WriteNewLine( ) => RecordMessage( HslMessageDegree.None, string.Empty, "\u0002" + Environment.NewLine );

		/// <inheritdoc cref="ILogNet.SetMessageDegree(HslMessageDegree)"/>
		public void SetMessageDegree( HslMessageDegree degree ) => m_messageDegree = degree;

		#endregion

		#region Filtrate Keyword

		/// <inheritdoc cref="ILogNet.FiltrateKeyword(string)"/>
		[HslMqttApi]
		public void FiltrateKeyword( string keyword )
		{
			filtrateLock.Enter( );
			if (!filtrateKeyword.Contains( keyword ))
			{
				filtrateKeyword.Add( keyword );
			}
			filtrateLock.Leave( );
		}

		/// <inheritdoc cref="ILogNet.RemoveFiltrate(string)"/>
		[HslMqttApi]
		public void RemoveFiltrate( string keyword )
		{
			filtrateLock.Enter( );
			if (filtrateKeyword.Contains( keyword ))
			{
				filtrateKeyword.Remove( keyword );
			}
			filtrateLock.Leave( );
		}

		#endregion

		#region File Write

		private void WriteToFile( HslMessageDegree degree, string keyword, string text )
		{
			// 过滤事件
			if (degree <= m_messageDegree)
			{
				// 需要记录数据
				HslMessageItem item = GetHslMessageItem( degree, keyword, text );
				AddItemToCache( new HslMessageItem[] { item } );
			}
		}

		private void AddItemToCache( HslMessageItem[] items )
		{
			m_simpleHybirdLock.Enter( );
			foreach (HslMessageItem item in items)
				m_WaitForSave.Enqueue( item );
			m_simpleHybirdLock.Leave( );
			StartSaveFile( );
		}

		private void StartSaveFile( )
		{
			if (Interlocked.CompareExchange( ref m_SaveStatus, 1, 0 ) == 0)
			{
				ThreadPool.QueueUserWorkItem( new WaitCallback( ThreadPoolSaveFile ), null );  // 启动存储
			}
		}

		private HslMessageItem GetAndRemoveLogItem( )
		{
			HslMessageItem result = null;

			m_simpleHybirdLock.Enter( );

			result = m_WaitForSave.Count > 0 ? m_WaitForSave.Dequeue( ) : null;

			m_simpleHybirdLock.Leave( );

			return result;
		}

		private void ConsoleWriteLog( HslMessageItem log )
		{
			if      (log.Degree == HslMessageDegree.DEBUG) Console.ForegroundColor = ConsoleColor.DarkGray;
			else if (log.Degree == HslMessageDegree.INFO)  Console.ForegroundColor = ConsoleColor.White;
			else if (log.Degree == HslMessageDegree.WARN)  Console.ForegroundColor = ConsoleColor.Yellow;
			else if (log.Degree == HslMessageDegree.ERROR) Console.ForegroundColor = ConsoleColor.Red;
			else if (log.Degree == HslMessageDegree.FATAL) Console.ForegroundColor = ConsoleColor.DarkRed;
			else Console.ForegroundColor = ConsoleColor.White;

			Console.WriteLine( HslMessageFormate( log, false ) );
		}

		private void ThreadPoolSaveFile( object obj )
		{
			// 获取需要存储的日志
			HslMessageItem current = GetAndRemoveLogItem( );
			// 进入文件操作的锁
			m_fileSaveLock.Enter( );

			// 获取要存储的文件名称，并判断是否生成了新的文件
			string logSaveFileName = GetFileSaveName( );
			bool createNewLogFile = false;

			if (!string.IsNullOrEmpty( logSaveFileName ))
			{
				if (logSaveFileName != this.lastLogSaveFileName)
				{
					createNewLogFile = true;
					this.lastLogSaveFileName = logSaveFileName;
				}
				// 保存
				StreamWriter sw = null;
				try
				{
					sw = new StreamWriter( logSaveFileName, true, Encoding.UTF8 );
					while (current != null)
					{
						if (ConsoleOutput) ConsoleWriteLog( current );
						// 触发事件
						OnBeforeSaveToFile( new HslEventArgs( ) { HslMessage = current } );
						LogNetStatistics?.StatisticsAdd( );

						// 检查是否需要真的进行存储
						bool isSave = true;
						filtrateLock.Enter( );
						isSave = !filtrateKeyword.Contains( current.KeyWord );
						filtrateLock.Leave( );

						// 检查是否被设置为强制不存储
						if (current.Cancel) isSave = false;

						// 如果需要存储的就过滤掉
						if (isSave)
						{
							sw.Write( HslMessageFormate( current, true ) );
							sw.Write( Environment.NewLine );
							sw.Flush( );
						}

						current = GetAndRemoveLogItem( );
					}
				}
				catch (Exception ex)
				{
					// 如果存储失败，则加入缓存里面去，等待下次再次尝试
					AddItemToCache( new HslMessageItem[]{ current, new HslMessageItem( )
					{
						Degree = HslMessageDegree.FATAL,
						Text   = LogNetManagment.GetSaveStringFromException( "LogNetSelf", ex ),
						Time   = DateTime.Now.AddHours( HourDeviation ),
					} } );
				}
				finally
				{
					sw?.Dispose( );
				}
			}
			else
			{
				while (current != null)
				{
					if (ConsoleOutput) ConsoleWriteLog( current );
					// 触发事件
					OnBeforeSaveToFile( new HslEventArgs( ) { HslMessage = current } );
					current = GetAndRemoveLogItem( );
				}
			}

			// 释放锁
			m_fileSaveLock.Leave( );
			Interlocked.Exchange( ref m_SaveStatus, 0 );
			OnWriteCompleted( createNewLogFile );

			// 再次检测锁是否释放完成
			if (m_WaitForSave.Count > 0)
			{
				StartSaveFile( );
			}
		}

		private string HslMessageFormate( HslMessageItem hslMessage, bool writeFile )
		{
			StringBuilder stringBuilder = new StringBuilder( );
			if (hslMessage.Degree != HslMessageDegree.None)
			{
				if (writeFile && LogStxAsciiCode) stringBuilder.Append( "\u0002" );
				stringBuilder.Append( "[" );
				stringBuilder.Append( LogNetManagment.GetDegreeDescription( hslMessage.Degree ) );
				stringBuilder.Append( "] " );

				stringBuilder.Append( hslMessage.Time.ToString( "yyyy-MM-dd HH:mm:ss.fff" ) );
				if (LogThreadID)
				{
					stringBuilder.Append( " Thread:[" );
					stringBuilder.Append( hslMessage.ThreadId.ToString( "D3" ) );
					stringBuilder.Append( "] " );
				}

				if (!string.IsNullOrEmpty( hslMessage.KeyWord ))
				{
					stringBuilder.Append( hslMessage.KeyWord );
					stringBuilder.Append( " : " );
				}
			}
			stringBuilder.Append( hslMessage.Text );

			return stringBuilder.ToString( );
		}

		#endregion

		#region Object Override

		/// <inheritdoc/>
		public override string ToString( ) => $"LogNetBase[{LogSaveMode}]";

		#endregion

		#region Helper Method

		/// <inheritdoc/>
		protected virtual string GetFileSaveName( ) => string.Empty;

		/// <summary>
		/// 当写入文件完成的时候触发，这时候已经释放了文件的句柄了。<br />
		/// Triggered when writing to the file is complete, and the file handle has been released.
		/// </summary>
		protected virtual void OnWriteCompleted( bool createNewLogFile ) { }

		private HslMessageItem GetHslMessageItem( HslMessageDegree degree, string keyWord, string text )
		{
			if (HourDeviation == 0)
				return new HslMessageItem( )
				{
					KeyWord  = keyWord,
					Degree   = degree,
					Text     = text,
					ThreadId = Thread.CurrentThread.ManagedThreadId,
				};
			else
				return new HslMessageItem( )
				{
					KeyWord    = keyWord,
					Degree     = degree,
					Text       = text,
					ThreadId   = Thread.CurrentThread.ManagedThreadId,
					Time       = DateTime.Now.AddHours( HourDeviation )
				};
		}

		private int CalculateStringOccupyLength( string str )
		{
			if (string.IsNullOrEmpty( str )) return 0;
			int result = 0;
			for (int i = 0; i < str.Length; i++)
			{
				if (str[i] >= 0x4e00 && str[i] <= 0x9fbb)
				{
					result += 2;
				}
				else
				{
					result += 1;
				}
			}
			return result;
		}

		private void AppendCharToStringBuilder( StringBuilder sb, char c, int count )
		{
			for (int i = 0; i < count; i++)
			{
				sb.Append( c );
			}
		}

		#endregion

		#region IDisposable Support

		private bool disposedValue = false; // 要检测冗余调用

		/// <summary>
		/// 释放资源
		/// </summary>
		/// <param name="disposing">是否初次调用</param>
		protected virtual void Dispose( bool disposing )
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					BeforeSaveToFile = null;
					m_simpleHybirdLock.Dispose( );
					m_WaitForSave.Clear( );
					m_fileSaveLock.Dispose( );
				}
				disposedValue = true;
			}
		}

		/// <inheritdoc cref="IDisposable.Dispose"/>
		public void Dispose( )
		{
			// 请勿更改此代码。将清理代码放入以上 Dispose(bool disposing) 中。
			Dispose( true );
			// GC.SuppressFinalize(this);
		}

		#endregion
	}
}
