using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Threading;
using HslCommunication.Core;
using HslCommunication.Core.Net;
using HslCommunication.LogNet;
#if !NET35 && !NET20
using System.Threading.Tasks;
#endif

namespace HslCommunication.Enthernet
{
	/// <summary>
	/// 用于服务器支持软件全自动更新升级的类<br />
	/// Class for server support software full automatic update and upgrade
	/// </summary>
	public sealed class NetSoftUpdateServer : NetworkServerBase
	{
		#region Constructor

		/// <summary>
		/// 实例化一个默认的对象<br />
		/// Instantiate a default object
		/// </summary>
		/// <param name="updateExeFileName">更新程序的名称</param>
		public NetSoftUpdateServer( string updateExeFileName = "软件自动更新.exe" )
		{
			this.updateExeFileName = updateExeFileName;
		}

		#endregion

		#region Private Member

		private string m_FilePath = @"C:\HslCommunication";
		private string updateExeFileName;                     // 软件更新的声明

		#endregion

		/// <summary>
		/// 系统升级时客户端所在的目录，默认为C:\HslCommunication
		/// </summary>
		public string FileUpdatePath
		{
			get { return m_FilePath; }
			set { m_FilePath = value; }
		}

#if NET35 || NET20
		/// <inheritdoc/>
		protected override void ThreadPoolLogin( Socket socket, IPEndPoint endPoint )
		{
			try
			{
				OperateResult<byte[]> receive = Receive( socket, 4 );
				if (!receive.IsSuccess)
				{
					LogNet?.WriteError( ToString( ), "Receive Failed: " + receive.Message );
					return;
				}

				byte[] ReceiveByte = receive.Content;
				int Protocol = BitConverter.ToInt32(ReceiveByte, 0);

				if (Protocol == 0x1001 || Protocol == 0x1002)
				{
					// 安装系统和更新系统
					if (Protocol == 0x1001)
					{
						LogNet?.WriteInfo(ToString(), StringResources.Language.SystemInstallOperater + ((IPEndPoint)socket.RemoteEndPoint).Address.ToString());
					}
					else
					{
						LogNet?.WriteInfo( ToString( ), StringResources.Language.SystemUpdateOperater + ((IPEndPoint)socket.RemoteEndPoint).Address.ToString());
					}

					if (Directory.Exists(FileUpdatePath))
					{
						List<string> Files = GetAllFiles( FileUpdatePath, LogNet );

						for (int i = Files.Count - 1; i >= 0; i--)
						{
							FileInfo finfo = new FileInfo(Files[i]);
							if (finfo.Length > 200000000)
							{
								Files.RemoveAt(i);
							}
							if (Protocol == 0x1002)
							{
								if (finfo.Name == this.updateExeFileName)
								{
									Files.RemoveAt(i);
								}
							}
						}
						string[] files = Files.ToArray();

						socket.BeginReceive(new byte[4], 0, 4, SocketFlags.None, new AsyncCallback(ReceiveCallBack), socket);

						socket.Send(BitConverter.GetBytes(files.Length));
						for (int i = 0; i < files.Length; i++)
						{
							// 传送数据包含了本次数据大小，文件数据大小，文件名（带后缀）
							FileInfo finfo = new FileInfo(files[i]);
							string fileName = finfo.FullName.Replace( m_FilePath, "" );
							if (fileName.StartsWith( "\\" )) fileName = fileName.Substring( 1 );
							byte[] ByteName = Encoding.Unicode.GetBytes(fileName);

							int First = 4 + 4 + ByteName.Length;
							byte[] FirstSend = new byte[First];

							FileStream fs = new FileStream(files[i], FileMode.Open, FileAccess.Read);

							Array.Copy(BitConverter.GetBytes(First), 0, FirstSend, 0, 4);
							Array.Copy(BitConverter.GetBytes((int)fs.Length), 0, FirstSend, 4, 4);
							Array.Copy(ByteName, 0, FirstSend, 8, ByteName.Length);

							socket.Send(FirstSend);
							Thread.Sleep(10);

							byte[] buffer = new byte[4096];
							int sended = 0;
							while (sended < fs.Length)
							{
								int count = fs.Read(buffer, 0, 4096);
								socket.Send(buffer, 0, count, SocketFlags.None);
								sended += count;
							}

							fs.Close();
							fs.Dispose();

							Thread.Sleep(20);
						}
					}
					else
					{
						socket.Send( BitConverter.GetBytes( 0 ) );
						socket?.Close( );
					}
				}
				else
				{
					// 兼容原先版本的更新，新的验证方式无需理会
					socket.Send(BitConverter.GetBytes(10000f));
					Thread.Sleep(20);
					socket?.Close();
				}
			}
			catch (Exception ex)
			{
				Thread.Sleep(20);
				socket?.Close();
				LogNet?.WriteException( ToString( ), StringResources.Language.FileSendClientFailed, ex);
			}
		}
#else
		/// <inheritdoc/>
		protected async override void ThreadPoolLogin( Socket socket, IPEndPoint endPoint )
		{
			try
			{
				OperateResult<byte[]> receive = await ReceiveAsync( socket, 4 );
				if (!receive.IsSuccess)
				{
					LogNet?.WriteError( ToString( ), "Receive Failed: " + receive.Message );
					return;
				}

				byte[] ReceiveByte = receive.Content;
				int Protocol = BitConverter.ToInt32( ReceiveByte, 0 );

				if (Protocol == 0x1001 || Protocol == 0x1002)
				{
					// 安装系统和更新系统
					if (Protocol == 0x1001)
					{
						LogNet?.WriteInfo( ToString( ), StringResources.Language.SystemInstallOperater + ((IPEndPoint)socket.RemoteEndPoint).Address.ToString( ) );
					}
					else
					{
						LogNet?.WriteInfo( ToString( ), StringResources.Language.SystemUpdateOperater + ((IPEndPoint)socket.RemoteEndPoint).Address.ToString( ) );
					}

					if (Directory.Exists( FileUpdatePath ))
					{
						List<string> Files = GetAllFiles( FileUpdatePath, LogNet );

						for (int i = Files.Count - 1; i >= 0; i--)
						{
							FileInfo finfo = new FileInfo( Files[i] );
							if (finfo.Length > 200000000)
							{
								Files.RemoveAt( i );
							}
							if (Protocol == 0x1002)
							{
								if (finfo.Name == this.updateExeFileName)
								{
									Files.RemoveAt( i );
								}
							}
						}
						string[] files = Files.ToArray( );

						socket.BeginReceive( new byte[4], 0, 4, SocketFlags.None, new AsyncCallback( ReceiveCallBack ), socket );

						await SendAsync( socket, BitConverter.GetBytes( files.Length ) );
						for (int i = 0; i < files.Length; i++)
						{
							// 传送数据包含了本次数据大小，文件数据大小，文件名（带后缀）
							FileInfo finfo = new FileInfo( files[i] );
							string fileName = finfo.FullName.Replace( m_FilePath, "" );
							if (fileName.StartsWith( "\\" )) fileName = fileName.Substring( 1 );
							byte[] ByteName = Encoding.Unicode.GetBytes( fileName );

							int First = 4 + 4 + ByteName.Length;
							byte[] FirstSend = new byte[First];

							FileStream fs = new FileStream( files[i], FileMode.Open, FileAccess.Read );

							Array.Copy( BitConverter.GetBytes( First ), 0, FirstSend, 0, 4 );
							Array.Copy( BitConverter.GetBytes( (int)fs.Length ), 0, FirstSend, 4, 4 );
							Array.Copy( ByteName, 0, FirstSend, 8, ByteName.Length );

							await SendAsync( socket, FirstSend );
							Thread.Sleep( 10 );

							byte[] buffer = new byte[4096 * 10];
							int sended = 0;
							while (sended < fs.Length)
							{
								int count = await fs.ReadAsync( buffer, 0, buffer.Length );

								await SendAsync( socket, buffer, 0, count );
								sended += count;
							}

							fs.Close( );
							fs.Dispose( );

							Thread.Sleep( 20 );
						}
					}
					else
					{
						await SendAsync( socket, BitConverter.GetBytes( 0 ) );
						socket?.Close( );
					}
				}
				else
				{
					// 兼容原先版本的更新，新的验证方式无需理会
					await SendAsync( socket, BitConverter.GetBytes( 10000f ) );
					Thread.Sleep( 20 );
					socket?.Close( );
				}
			}
			catch (Exception ex)
			{
				Thread.Sleep( 20 );
				socket?.Close( );
				LogNet?.WriteException( ToString( ), StringResources.Language.FileSendClientFailed, ex );
			}
		}
#endif

		private void ReceiveCallBack(IAsyncResult ir)
		{
			if (ir.AsyncState is Socket socket)
			{
				try
				{
					socket.EndReceive(ir);
				}
				catch(Exception ex)
				{
					LogNet?.WriteException( ToString( ), ex);
				}
				finally
				{
					socket?.Close();
				}
			}
		}

		/// <summary>
		/// 获取所有的文件信息
		/// </summary>
		/// <param name="dircPath">目标路径</param>
		/// <param name="logNet">日志信息</param>
		/// <returns>文件名的列表</returns>
		public static List<string> GetAllFiles( string dircPath, ILogNet logNet )
		{
			List<string> fileList = new List<string>( );

			try
			{
				fileList.AddRange( Directory.GetFiles( dircPath ) );
			}
			catch(Exception ex)
			{
				logNet?.WriteWarn( "GetAllFiles", ex.Message );
			}
			foreach (var item in Directory.GetDirectories( dircPath ))
				fileList.AddRange( GetAllFiles( item, logNet ) );
			return fileList;
		}

		#region Object Override

		/// <inheritdoc/>
		public override string ToString( ) => $"NetSoftUpdateServer[{Port}]";

		#endregion

	}
}
