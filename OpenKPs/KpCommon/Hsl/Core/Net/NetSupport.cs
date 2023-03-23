using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using HslCommunication.BasicFramework;
using HslCommunication.Enthernet;
using HslCommunication.LogNet;
#if !NET35 && !NET20
using System.Threading.Tasks;
#endif

namespace HslCommunication.Core
{
	/*******************************************************************************
	 * 
	 *    网络通信类的基础类，提供所有相关的基础方法和功能
	 *
	 *    Network communication base class of the class, provides the basis of all relevant methods and functions
	 * 
	 *******************************************************************************/

	#region Network Helper

	/// <summary>
	/// 静态的方法支持类，提供一些网络的静态支持，支持从套接字从同步接收指定长度的字节数据，并支持报告进度。<br />
	/// The static method support class provides some static support for the network, supports receiving byte data of a specified length from the socket from synchronization, and supports reporting progress.
	/// </summary>
	/// <remarks>
	/// 在接收指定数量的字节数据的时候，如果一直接收不到，就会发生假死的状态。接收的数据时保存在内存里的，不适合大数据块的接收。
	/// </remarks>
	public static class NetSupport
	{
		/// <summary>
		/// Socket传输中的缓冲池大小<br />
		/// Buffer pool size in socket transmission
		/// </summary>
		internal const int SocketBufferSize = 16 * 1024;

		/// <summary>
		/// 根据接收数据的长度信息，合理的分割出单次的长度信息
		/// </summary>
		/// <param name="length">要接收数据的总长度信息</param>
		/// <returns>本次接收数据的长度</returns>
		internal static int GetSplitLengthFromTotal( int length )
		{
			if (length < 1024)              return length;
			if (length <= 1024 * 8)         return 1024 * 2;
			if (length <= 1024 * 32)        return 1024 * 8;
			if (length <= 1024 * 256)       return 1024 * 32;
			if (length <= 1024 * 1024 )     return 1024 * 256;
			if (length <= 1024 * 1024 * 8 ) return 1024 * 1024;
			return 1024 * 1024 * 2;
		}

		/// <summary>
		/// 从socket的网络中读取数据内容，需要指定数据长度和超时的时间，为了防止数据太大导致接收失败，所以此处接收到新的数据之后就更新时间。<br />
		/// To read the data content from the socket network, you need to specify the data length and timeout period. In order to prevent the data from being too large and cause the reception to fail, the time is updated after new data is received here.
		/// </summary>
		/// <param name="socket">网络套接字</param>
		/// <param name="receive">接收的长度</param>
		/// <param name="reportProgress">当前接收数据的进度报告，有些协议支持传输非常大的数据内容，可以给与进度提示的功能</param>
		/// <returns>最终接收的指定长度的byte[]数据</returns>
		internal static byte[] ReadBytesFromSocket( Socket socket, int receive, Action<long, long> reportProgress = null )
		{
			byte[] bytes_receive = new byte[receive];
			ReceiveBytesFromSocket( socket, bytes_receive, 0, receive, reportProgress );
			return bytes_receive;
		}

		/// <summary>
		/// 从socket的网络中读取数据内容，需要指定数据长度和超时的时间，为了防止数据太大导致接收失败，所以此处接收到新的数据之后就更新时间。<br />
		/// To read the data content from the socket network, you need to specify the data length and timeout period. In order to prevent the data from being too large and cause the reception to fail, the time is updated after new data is received here.
		/// </summary>
		/// <param name="socket">网络套接字</param>
		/// <param name="buffer">缓存的字节数组</param>
		/// <param name="offset">偏移信息</param>
		/// <param name="length">接收长度</param>
		/// <param name="reportProgress">当前接收数据的进度报告，有些协议支持传输非常大的数据内容，可以给与进度提示的功能</param>
		/// <exception cref="RemoteCloseException">远程关闭的异常信息</exception>
		internal static void ReceiveBytesFromSocket( Socket socket, byte[] buffer, int offset, int length, Action<long, long> reportProgress = null )
		{
			int count_receive = 0;
			while (count_receive < length)
			{
				// 分割成16KB来接收数据
				int receive_length = Math.Min( length - count_receive, SocketBufferSize );
				int count = socket.Receive( buffer, count_receive + offset, receive_length, SocketFlags.None );
				count_receive += count;

				if (count == 0) throw new RemoteCloseException( );
				reportProgress?.Invoke( count_receive, length );
			}
		}

		/// <summary>
		/// 从socket的网络中读取数据内容，然后写入到流中
		/// </summary>
		/// <param name="socket">网络套接字</param>
		/// <param name="stream">等待写入的流</param>
		/// <param name="length">长度信息</param>
		/// <param name="reportProgress">当前接收数据的进度报告，有些协议支持传输非常大的数据内容，可以给与进度提示的功能</param>
		/// <exception cref="RemoteCloseException">远程关闭的异常信息</exception>
		internal static void ReceiveBytesFromSocket( Socket socket, Stream stream, int length, Action<long, long> reportProgress = null )
		{
			int count_receive = 0;
			byte[] buffer = new byte[GetSplitLengthFromTotal( length )];
			while (count_receive < length)
			{
				// 根据length来自动匹配分割的长度接收数据
				int count = socket.Receive( buffer, 0, buffer.Length, SocketFlags.None );
				stream.Write( buffer, 0, count );
				count_receive += count;

				if (count == 0) throw new RemoteCloseException( );
				reportProgress?.Invoke( count_receive, length );
			}
		}

		/// <summary>
		/// 创建一个新的socket对象并连接到远程的地址，需要指定远程终结点，超时时间（单位是毫秒），如果需要绑定本地的IP或是端口，传入 local对象<br />
		/// To create a new socket object and connect to the remote address, you need to specify the remote endpoint, 
		/// the timeout period (in milliseconds), if you need to bind the local IP or port, pass in the local object
		/// </summary>
		/// <param name="endPoint">连接的目标终结点</param>
		/// <param name="timeOut">连接的超时时间</param>
		/// <param name="local">如果需要绑定本地的IP地址，就需要设置当前的对象</param>
		/// <returns>返回套接字的封装结果对象</returns>
		/// <example>
		/// <code lang="cs" source="HslCommunication_Net45.Test\Documentation\Samples\Core\NetworkBase.cs" region="CreateSocketAndConnectExample" title="创建连接示例" />
		/// </example>
		internal static OperateResult<Socket> CreateSocketAndConnect( IPEndPoint endPoint, int timeOut, IPEndPoint local = null )
		{
			int connectCount = 0;
			while (true)
			{
				connectCount++;
				var socket = new Socket( endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp );
				HslTimeOut connectTimeout = HslTimeOut.HandleTimeOutCheck( socket, timeOut );
				try
				{
					if (local != null) socket.Bind( local );
					socket.Connect( endPoint );
					connectTimeout.IsSuccessful = true;
					return OperateResult.CreateSuccessResult( socket );
				}
				catch (Exception ex)
				{
					// 如果连接一次出现了立即失败的情况，那就马上立即重试一次连接
					// If the connection fails immediately, try to retry the connection immediately
					socket?.Close( );
					connectTimeout.IsSuccessful = true;
					if (connectTimeout.GetConsumeTime( ) < TimeSpan.FromMilliseconds( 500 ) && connectCount < 2) { Thread.Sleep( 100 ); continue; }

					if (connectTimeout.IsTimeout)
					{
						return new OperateResult<Socket>( string.Format( StringResources.Language.ConnectTimeout, endPoint, timeOut ) + " ms" );
					}
					else
					{
						return new OperateResult<Socket>( $"Socket Connect {endPoint} Exception -> " + ex.Message );
					}
				}
			}
		}

		/// <inheritdoc cref="IReadWriteDevice.ReadFromCoreServer(IEnumerable{byte[]})"/>
		public static OperateResult<byte[]> ReadFromCoreServer( IEnumerable<byte[]> send, Func<byte[], OperateResult<byte[]>> funcRead )
		{
			List<byte> array = new List<byte>( );
			foreach (byte[] data in send)
			{
				OperateResult<byte[]> read = funcRead.Invoke( data );
				if (!read.IsSuccess) return read;

				if (read.Content != null) array.AddRange( read.Content );
			}
			return OperateResult.CreateSuccessResult( array.ToArray( ) );
		}

#if !NET20 && !NET35
		/// <inheritdoc cref="CreateSocketAndConnect(IPEndPoint, int, IPEndPoint)"/>
		internal static async Task<OperateResult<Socket>> CreateSocketAndConnectAsync( IPEndPoint endPoint, int timeOut, IPEndPoint local = null )
		{
			int connectCount = 0;
			while (true)
			{
				connectCount++;
				var socket = new Socket( endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp );
				HslTimeOut connectTimeout = HslTimeOut.HandleTimeOutCheck( socket, timeOut );

				try
				{
					if (local != null) socket.Bind( local );
					await Task.Factory.FromAsync( socket.BeginConnect( endPoint, null, socket ), socket.EndConnect );
					connectTimeout.IsSuccessful = true;
					return OperateResult.CreateSuccessResult( socket );
				}
				catch (Exception ex)
				{
					connectTimeout.IsSuccessful = true;
					socket?.Close( );

					// 如果连接一次出现了立即失败的情况，那就马上立即重试一次连接
					// If the connection fails immediately, try to retry the connection immediately
					if (connectTimeout.GetConsumeTime( ) < TimeSpan.FromMilliseconds( 500 ) && connectCount < 2) { await Task.Delay( 100 ); continue; }

					if (connectTimeout.IsTimeout)
					{
						return new OperateResult<Socket>( string.Format( StringResources.Language.ConnectTimeout, endPoint, timeOut ) + " ms" );
					}
					else
					{
						return new OperateResult<Socket>( $"Socket Exception -> {ex.Message}" );
					}
				}
			}
		}

		/// <inheritdoc cref="IReadWriteDevice.ReadFromCoreServer(IEnumerable{byte[]})"/>
		public static async Task<OperateResult<byte[]>> ReadFromCoreServerAsync( IEnumerable<byte[]> send, Func<byte[], Task<OperateResult<byte[]>>> funcRead )
		{
			List<byte> array = new List<byte>( );
			foreach ( byte[] data in send )
			{
				OperateResult<byte[]> read = await funcRead.Invoke( data );
				if (!read.IsSuccess) return read;

				if (read.Content != null) array.AddRange( read.Content );
			}
			return OperateResult.CreateSuccessResult( array.ToArray( ) );
		}
#endif

	}

	#endregion
	
}
