using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;
using System.Net.Sockets;
using System.Net;
using HslCommunication.Core.Net;
#if !NET20 && !NET35
using System.Threading.Tasks;
#endif


namespace HslCommunication.Core.Pipe
{
	/// <summary>
	/// 基于网络通信的管道信息，可以设置额外的一些参数信息，例如连接超时时间，读取超时时间等等。<br />
	/// Based on the pipe information of network communication, some additional parameter information can be set, such as connection timeout time, read timeout time and so on.
	/// </summary>
	public class PipeSocket : PipeBase, IDisposable
	{
		/// <summary>
		/// 实例化一个默认的对象<br />
		/// Instantiate a default object
		/// </summary>
		public PipeSocket( )
		{

		}

		/// <summary>
		/// 通过指定的IP地址和端口号来实例化一个对象<br />
		/// Instantiate an object with the specified IP address and port number
		/// </summary>
		/// <param name="ipAddress">IP地址信息</param>
		/// <param name="port">端口号</param>
		public PipeSocket( string ipAddress, int port )
		{
			this.ipAddress = ipAddress;
			this._port      = new int[] { port };
		}

		/// <summary>
		/// 获取当前的连接状态是否发生了异常，如果发生了异常，返回 False<br />
		/// Gets whether an exception has occurred in the current connection state, and returns False if an exception has occurred
		/// </summary>
		/// <returns>如果有异常，返回 True, 否则返回 False</returns>
		public bool IsConnectitonError( )
		{
			return IsSocketError || this.socket == null;
		}

		/// <inheritdoc cref="NetworkDoubleBase.LocalBinding"/>
		public IPEndPoint LocalBinding { get; set; }

		/// <inheritdoc cref="NetworkDoubleBase.IpAddress"/>
		public string IpAddress
		{
			get => this.ipAddress;
			set => ipAddress = HslHelper.GetIpAddressFromInput( value );
		}

		/// <inheritdoc cref="NetworkDoubleBase.Port"/>
		public int Port
		{
			get
			{
				if (this._port.Length == 1)
				{
					return this._port[0];
				}
				else
				{
					int index = this.indexPort;
					if (index < 0 || index >= this._port.Length) index = 0;
					return this._port[index];
				}
			}
			set
			{
				if (this._port.Length == 1)
				{
					this._port[0] = value;
				}
				else
				{
					int index = this.indexPort;
					if (index < 0 || index >= this._port.Length) index = 0;
					_port[index] = value;
				}
			}
		}

		/// <summary>
		/// 指示长连接的套接字是否处于错误的状态<br />
		/// Indicates if the long-connected socket is in the wrong state
		/// </summary>
		public bool IsSocketError { get; set; }

		/// <summary>
		/// 获取或设置当前的客户端用于服务器连接的套接字。<br />
		/// Gets or sets the socket currently used by the client for server connection.
		/// </summary>
		public Socket Socket
		{
			get => this.socket;
			set => this.socket = value;
		}

		/// <inheritdoc cref="NetworkDoubleBase.ReceiveTimeOut"/>
		public int ConnectTimeOut
		{
			get => this.connectTimeOut;
			set => this.connectTimeOut = value;
		}

		/// <inheritdoc cref="NetworkDoubleBase.ReceiveTimeOut"/>
		public int ReceiveTimeOut
		{
			get => this.receiveTimeOut;
			set => this.receiveTimeOut = value;
		}

		/// <inheritdoc cref="NetworkDoubleBase.SleepTime"/>
		public int SleepTime
		{
			get => this.sleepTime;
			set => this.sleepTime = value;
		}

		/// <inheritdoc cref="IDisposable.Dispose"/>
		public override void Dispose( )
		{
			base.Dispose( );
			this.socket?.Close( );
		}

		/// <summary>
		/// 设置多个可选的端口号信息，例如在三菱的PLC里，支持配置多个端口号，当一个网络发生异常时，立即切换端口号连接读写，提升系统的稳定性<br />
		/// Set multiple optional port number information. For example, in Mitsubishi PLC, it supports to configure multiple port numbers. 
		/// When an abnormality occurs in a network, the port number is immediately switched to connect to read and write to improve the stability of the system.
		/// </summary>
		/// <param name="ports">端口号数组信息</param>
		public void SetMultiPorts( int[] ports )
		{
			if (ports?.Length > 0)
			{
				this._port = ports;
				this.indexPort = -1;
			}
		}

		/// <summary>
		/// 获取当前的远程连接信息，如果端口号设置了可选的数组，那么每次获取对象就会发生端口号切换的操作。<br />
		/// Get the current remote connection information. If the port number is set to an optional array, the port number switching operation will occur every time the object is obtained.
		/// </summary>
		/// <returns>远程连接的对象</returns>
		public IPEndPoint GetConnectIPEndPoint( )
		{
			if (_port.Length == 1) return new IPEndPoint( IPAddress.Parse( IpAddress ), _port[0] );

			if (this.indexPort < this._port.Length - 1)
				this.indexPort++;
			else
				this.indexPort = 0;

			int port = _port[this.indexPort];
			return new IPEndPoint( IPAddress.Parse( IpAddress ), port );
		}

		/// <inheritdoc/>
		public override string ToString( ) => $"PipeSocket[{ipAddress}:{Port}]";

		private string ipAddress = "127.0.0.1";            // IP地址信息
		private int[] _port = new int[1] { 2000 };         // 端口号数组信息
		private int indexPort = -1;                        // 端口的索引
		private Socket socket;                             // 核心的通信的方法
		private int receiveTimeOut = 5000;                 // 接收数据超时的时间
		private int connectTimeOut = 10_000;               // 连接设备超时的时间
		private int sleepTime = 0;                         // 设备请求应答时，中间休眠的时间，如果设置为0，不需要休眠
	}
}
