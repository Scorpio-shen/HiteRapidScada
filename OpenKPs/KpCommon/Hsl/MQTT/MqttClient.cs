using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HslCommunication.Core.Net;
using HslCommunication.Core;
using HslCommunication;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using HslCommunication.BasicFramework;
using System.Security.Cryptography;
using HslCommunication.Core.Security;
using static HslCommunication.MQTT.MqttClient;
#if !NET35 && !NET20
using System.Threading.Tasks;
#endif

namespace HslCommunication.MQTT
{
	/// <summary>
	/// Mqtt协议的客户端实现，支持订阅消息，发布消息，详细的使用例子参考api文档<br />
	/// The client implementation of the Mqtt protocol supports subscription messages and publishing messages. For detailed usage examples, refer to the api documentation. 
	/// </summary>
	/// <remarks>
	/// 这是一个MQTT的客户端实现，参照MQTT协议的3.1.1版本设计实现的。服务器可以是其他的组件提供的，其他的可以参考示例<br />
	/// This is an MQTT client implementation, designed and implemented with reference to version 3.1.1 of the MQTT protocol. The server can be provided by other components.
	/// </remarks>
	/// <example>
	/// <code lang="cs" source="HslCommunication_Net45.Test\Documentation\Samples\MQTT\MQTTClient.cs" region="Test" title="简单的实例化" />
	/// <code lang="cs" source="HslCommunication_Net45.Test\Documentation\Samples\MQTT\MQTTClient.cs" region="Test2" title="带用户名密码的实例化" />
	/// <code lang="cs" source="HslCommunication_Net45.Test\Documentation\Samples\MQTT\MQTTClient.cs" region="Test3" title="连接示例" />
	/// <code lang="cs" source="HslCommunication_Net45.Test\Documentation\Samples\MQTT\MQTTClient.cs" region="Test4" title="发布示例" />
	/// <code lang="cs" source="HslCommunication_Net45.Test\Documentation\Samples\MQTT\MQTTClient.cs" region="Test5" title="订阅示例" />
	/// <code lang="cs" source="HslCommunication_Net45.Test\Documentation\Samples\MQTT\MQTTClient.cs" region="Test8" title="网络重连示例" />
	/// 当我们在一个多窗体的客户端里使用了<see cref="MqttClient"/>类，可能很多界面都需要订阅主题，显示一些实时数据信息。只由主窗体来订阅再把数据传递给子窗体却不是很容易操作。
	/// 所以在hsl里提供了更加便捷的操作方法。方便在每个子窗体界面中，订阅，显示，取消订阅操作。核心代码如下：
	/// <code lang="cs" source="HslCommunication_Net45.Test\Documentation\Samples\MQTT\MQTTClient.cs" region="Test9" title="子窗体订阅操作" />
	/// 以下的例子是DEMO程序的一个例子代码，也可以作为参考
	/// <code lang="cs" source="TestProject\HslCommunicationDemo\MQTT\FormMqttSubscribe.cs" region="Sample" title="DEMO子窗体" />
	/// </example>
	public class MqttClient : NetworkXBase, IDisposable
	{
		#region Constructor

		/// <summary>
		/// 实例化一个默认的对象
		/// </summary>
		/// <param name="options">配置信息</param>
		public MqttClient( MqttConnectionOptions options )
		{
			this.connectionOptions = options;
			this.incrementCount    = new SoftIncrementCount( ushort.MaxValue, 1 );
			this.listLock          = new object( );
			this.publishMessages   = new List<MqttPublishMessage>( );
			this.subscribeTopics   = new Dictionary<string, SubscribeTopic>( );
			this.activeTime        = DateTime.Now;
			this.subscribeLock      = new object( );
			this.connectLock       = new object( );
		}

		#endregion

		#region Connect DisConnect

		/// <summary>
		/// 连接服务器，如果连接失败，请稍候重试。<br />
		/// Connect to the server. If the connection fails, try again later.
		/// </summary>
		/// <returns>连接是否成功</returns>
		public OperateResult ConnectServer( )
		{
			if (this.connectionOptions == null) return new OperateResult( "Optines is null" );

			OperateResult<Socket> connect = CreateSocketAndConnect( this.connectionOptions.IpAddress, this.connectionOptions.Port, this.connectionOptions.ConnectTimeout );
			if (!connect.IsSuccess) return connect;

			// 连接对象加密处理，和服务器进行交换密钥处理
			RSACryptoServiceProvider rsa = null;
			if (this.connectionOptions.UseRSAProvider)
			{
				cryptoServiceProvider = new RSACryptoServiceProvider( );

				OperateResult sendKey = Send( connect.Content, MqttHelper.BuildMqttCommand( 0xFF, null, HslSecurity.ByteEncrypt( cryptoServiceProvider.GetPEMPublicKey( ) ) ).Content );
				if (!sendKey.IsSuccess) return sendKey;

				OperateResult<byte, byte[]> key = ReceiveMqttMessage( connect.Content, 10_000 );
				if (!key.IsSuccess) return key;

				try
				{
					byte[] serverPublicToken = cryptoServiceProvider.DecryptLargeData( HslSecurity.ByteDecrypt( key.Content2 ) );
					rsa = RSAHelper.CreateRsaProviderFromPublicKey( serverPublicToken );
				}
				catch (Exception ex)
				{
					connect.Content?.Close( );
					return new OperateResult( "RSA check failed: " + ex.Message );
				}
			}

			OperateResult<byte[]> command = MqttHelper.BuildConnectMqttCommand( this.connectionOptions, "MQTT", rsa );
			if (!command.IsSuccess) return command;

			// 发送连接的报文信息
			OperateResult send = Send( connect.Content, command.Content );
			if (!send.IsSuccess) return send;

			// 接收服务器端注册返回的报文信息
			OperateResult<byte, byte[]> receive = ReceiveMqttMessage( connect.Content, 30_000 );
			if (!receive.IsSuccess) return receive;

			// 检查连接的返回状态是否正确
			OperateResult check = MqttHelper.CheckConnectBack( receive.Content1, receive.Content2 );
			if (!check.IsSuccess) { connect.Content?.Close( ); return check; }

			if (this.connectionOptions.UseRSAProvider)
			{
				string key = Encoding.UTF8.GetString( cryptoServiceProvider.Decrypt( receive.Content2.RemoveBegin( 2 ), false ) );
				this.aesCryptography = new AesCryptography( key );
			}

			this.incrementCount.ResetCurrentValue( );          // 重置消息计数
			this.closed = false;                               // 重置关闭状态

			try
			{
				connect.Content.BeginReceive( new byte[0], 0, 0, SocketFlags.None, new AsyncCallback( ReceiveAsyncCallback ), connect.Content );
			}
			catch (Exception ex)
			{
				return new OperateResult( ex.Message );
			}

			this.CoreSocket?.Close( );
			this.CoreSocket = connect.Content;
			this.IsConnected = true;
			this.OnClientConnected?.Invoke( this );
			// 开启心跳检测
			this.timerCheck?.Dispose( );
			this.activeTime = DateTime.Now;
			if (this.UseTimerCheckDropped && (int)this.connectionOptions.KeepAliveSendInterval.TotalMilliseconds > 0)
			{
				this.timerCheck = new Timer( new TimerCallback( TimerCheckServer ), null, 2000, (int)this.connectionOptions.KeepAliveSendInterval.TotalMilliseconds );
			}
			return OperateResult.CreateSuccessResult( );
		}

		/// <summary>
		/// 关闭Mqtt服务器的连接。<br />
		/// Close the connection to the Mqtt server.
		/// </summary>
		public void ConnectClose( )
		{
			lock (connectLock) { closed = true; this.IsConnected = false; }
			OperateResult<byte[]> command = MqttHelper.BuildMqttCommand( MqttControlMessage.DISCONNECT, 0x00, null, null );
			if (command.IsSuccess) Send( this.CoreSocket, command.Content );
			timerCheck?.Dispose( );
			Thread.Sleep( 20 );
			this.CoreSocket?.Close( );
		}

		#endregion

		#region Async Connect DisConnect
#if !NET35 && !NET20
		/// <inheritdoc cref="ConnectServer"/>
		public async Task<OperateResult> ConnectServerAsync( )
		{
			if (this.connectionOptions == null) return new OperateResult( "Optines is null" );

			OperateResult<Socket> connect = await CreateSocketAndConnectAsync( this.connectionOptions.IpAddress, this.connectionOptions.Port, this.connectionOptions.ConnectTimeout );
			if (!connect.IsSuccess) return connect;

			// 连接对象加密处理，和服务器进行交换密钥处理
			RSACryptoServiceProvider rsa = null;
			if (this.connectionOptions.UseRSAProvider)
			{
				cryptoServiceProvider = new RSACryptoServiceProvider( );

				OperateResult sendKey = await SendAsync( connect.Content, MqttHelper.BuildMqttCommand( 0xFF, null, HslSecurity.ByteEncrypt( cryptoServiceProvider.GetPEMPublicKey( ) ) ).Content );
				if (!sendKey.IsSuccess) return sendKey;

				OperateResult<byte, byte[]> key = await ReceiveMqttMessageAsync( connect.Content, 10_000 );
				if (!key.IsSuccess) return key;

				try
				{
					byte[] serverPublicToken = cryptoServiceProvider.DecryptLargeData( HslSecurity.ByteDecrypt( key.Content2 ) );
					rsa = RSAHelper.CreateRsaProviderFromPublicKey( serverPublicToken );
				}
				catch (Exception ex)
				{
					connect.Content?.Close( );
					return new OperateResult( "RSA check failed: " + ex.Message );
				}
			}

			OperateResult<byte[]> command = MqttHelper.BuildConnectMqttCommand( this.connectionOptions, "MQTT", rsa );
			if (!command.IsSuccess) return command;

			// 发送连接的报文信息
			OperateResult send = await SendAsync( connect.Content, command.Content );
			if (!send.IsSuccess) return send;

			// 接收服务器端注册返回的报文信息
			OperateResult<byte, byte[]> receive = await ReceiveMqttMessageAsync( connect.Content, 30_000 );
			if (!receive.IsSuccess) return receive;

			// 检查连接的返回状态是否正确
			OperateResult check = MqttHelper.CheckConnectBack( receive.Content1, receive.Content2 );
			if (!check.IsSuccess) { connect.Content?.Close( ); return check; }

			if (this.connectionOptions.UseRSAProvider)
			{
				string key = Encoding.UTF8.GetString( cryptoServiceProvider.Decrypt( receive.Content2.RemoveBegin( 2 ), false ) );
				this.aesCryptography = new AesCryptography( key );
			}

			this.incrementCount.ResetCurrentValue( );          // 重置消息计数
			this.closed = false;                               // 重置关闭状态

			try
			{
				connect.Content.BeginReceive( new byte[0], 0, 0, SocketFlags.None, new AsyncCallback( ReceiveAsyncCallback ), connect.Content );
			}
			catch (Exception ex)
			{
				return new OperateResult( ex.Message );
			}

			this.CoreSocket?.Close( );
			this.CoreSocket = connect.Content;
			this.OnClientConnected?.Invoke( this );
			this.IsConnected = true;
			// 开启心跳检测
			this.timerCheck?.Dispose( );
			this.activeTime = DateTime.Now;
			if (this.UseTimerCheckDropped && (int)this.connectionOptions.KeepAliveSendInterval.TotalMilliseconds > 0)
			{
				this.timerCheck = new Timer( new TimerCallback( TimerCheckServer ), null, 2000, (int)this.connectionOptions.KeepAliveSendInterval.TotalMilliseconds );
			}
			return OperateResult.CreateSuccessResult( );
		}

		/// <inheritdoc cref="ConnectClose"/>
		public async Task ConnectCloseAsync( )
		{
			lock (connectLock) { closed = true; this.IsConnected = true; }
			OperateResult<byte[]> command = MqttHelper.BuildMqttCommand( MqttControlMessage.DISCONNECT, 0x00, null, null );
			if (command.IsSuccess) await SendAsync( this.CoreSocket, command.Content );
			timerCheck?.Dispose( );
			Thread.Sleep( 20 );
			this.CoreSocket?.Close( );
		}
#endif
		#endregion

		#region Publish Message

		/// <summary>
		/// 发布一个MQTT协议的消息到服务器。该消息包含主题，负载数据，消息等级，是否保留信息。<br />
		/// Publish an MQTT protocol message to the server. The message contains the subject, payload data, message level, and whether to retain information.
		/// </summary>
		/// <param name="message">消息</param>
		/// <returns>发布结果</returns>
		/// <example>
		/// 参照 <see cref="MqttClient"/> 的示例说明。
		/// <code lang="cs" source="HslCommunication_Net45.Test\Documentation\Samples\MQTT\MQTTClient.cs" region="Test" title="简单的实例化" />
		/// <code lang="cs" source="HslCommunication_Net45.Test\Documentation\Samples\MQTT\MQTTClient.cs" region="Test4" title="发布示例" />
		/// </example>
		public OperateResult PublishMessage( MqttApplicationMessage message )
		{
			MqttPublishMessage publishMessage = new MqttPublishMessage( )
			{
				Identifier = message.QualityOfServiceLevel == MqttQualityOfServiceLevel.AtMostOnce ? 0 : (int)incrementCount.GetCurrentValue( ),
				Message = message,
			};

			OperateResult<byte[]> command = MqttHelper.BuildPublishMqttCommand( publishMessage, this.aesCryptography );
			if (!command.IsSuccess) return command;

			if (message.QualityOfServiceLevel == MqttQualityOfServiceLevel.AtMostOnce)
			{
				return Send( this.CoreSocket, command.Content );
			}
			else
			{
				AddPublishMessage( publishMessage );
				return Send( this.CoreSocket, command.Content );
			}
		}

		#endregion

		#region Async Publish Message
#if !NET35 && !NET20
		/// <inheritdoc cref="PublishMessage(MqttApplicationMessage)"/>
		public async Task<OperateResult> PublishMessageAsync( MqttApplicationMessage message )
		{
			MqttPublishMessage publishMessage = new MqttPublishMessage( )
			{
				Identifier = message.QualityOfServiceLevel == MqttQualityOfServiceLevel.AtMostOnce ? 0 : (int)incrementCount.GetCurrentValue( ),
				Message = message,
			};

			OperateResult<byte[]> command = MqttHelper.BuildPublishMqttCommand( publishMessage, this.aesCryptography );
			if (!command.IsSuccess) return command;

			if (message.QualityOfServiceLevel == MqttQualityOfServiceLevel.AtMostOnce)
			{
				return await SendAsync( this.CoreSocket, command.Content );
			}
			else
			{
				AddPublishMessage( publishMessage );
				return await SendAsync( this.CoreSocket, command.Content );
			}
		}
#endif
		#endregion

		#region Subscribe Message

		/// <summary>
		/// 从服务器订阅一个或多个主题信息<br />
		/// Subscribe to one or more topics from the server
		/// </summary>
		/// <param name="topic">主题信息</param>
		/// <returns>订阅结果</returns>
		/// <example>
		/// 参照 <see cref="MqttClient"/> 的示例说明。
		/// <code lang="cs" source="HslCommunication_Net45.Test\Documentation\Samples\MQTT\MQTTClient.cs" region="Test" title="简单的实例化" />
		/// <code lang="cs" source="HslCommunication_Net45.Test\Documentation\Samples\MQTT\MQTTClient.cs" region="Test5" title="订阅示例" />
		/// </example>
		public OperateResult SubscribeMessage( string topic ) => SubscribeMessage( new string[] { topic } );

		/// <inheritdoc cref="SubscribeMessage(string)"/>
		public OperateResult SubscribeMessage( string[] topics )
		{
			MqttSubscribeMessage subcribeMessage = new MqttSubscribeMessage( )
			{
				Identifier = (int)incrementCount.GetCurrentValue( ),
				Topics = topics,
			};

			return SubscribeMessage( subcribeMessage );
		}

		/// <summary>
		/// 向服务器订阅一个主题消息，可以指定订阅的主题数组，订阅的质量等级，还有消息标识符<br />
		/// To subscribe to a topic message from the server, you can specify the subscribed topic array, 
		/// the subscription quality level, and the message identifier
		/// </summary>
		/// <param name="subcribeMessage">订阅的消息本体</param>
		/// <returns>是否订阅成功</returns>
		public OperateResult SubscribeMessage( MqttSubscribeMessage subcribeMessage )
		{
			if (subcribeMessage.Topics == null) return OperateResult.CreateSuccessResult( );
			if (subcribeMessage.Topics.Length == 0) return OperateResult.CreateSuccessResult( );

			OperateResult<byte[]> command = MqttHelper.BuildSubscribeMqttCommand( subcribeMessage );
			if (!command.IsSuccess) return command;

			OperateResult send = Send( this.CoreSocket, command.Content );
			if (!send.IsSuccess) return send;

			AddSubTopics( subcribeMessage.Topics );
			return OperateResult.CreateSuccessResult( );
		}

		private void AddSubTopics( string[] topics )
		{
			lock (subscribeLock)
			{
				for (int i = 0; i < topics.Length; i++)
				{
					if (!subscribeTopics.ContainsKey( topics[i] )) subscribeTopics.Add( topics[i], new SubscribeTopic( topics[i] ) );
					subscribeTopics[topics[i]].AddSubscribeTick( );
				}
			}
		}

		/// <summary>
		/// 获取已经订阅的主题信息，方便针对不同的界面订阅不同的主题。<br />
		/// Get subscribed topic information, which is convenient for subscribing to different topics for different interfaces.
		/// </summary>
		/// <param name="topic">主题信息</param>
		/// <returns>订阅主题信息</returns>
		public SubscribeTopic GetSubscribeTopic( string topic )
		{
			SubscribeTopic subscribeTopic = null;
			lock (subscribeLock)
			{
				if (subscribeTopics.ContainsKey( topic ))
				{
					subscribeTopic = subscribeTopics[topic];
				}
			}
			return subscribeTopic;
		}

		/// <summary>
		/// 取消订阅多个主题信息，取消之后，当前的订阅数据就不在接收到，除非服务器强制推送。<br />
		/// Unsubscribe from multiple topic information. After cancellation, the current subscription data will not be received unless the server forces it to push it.
		/// </summary>
		/// <param name="topics">主题信息</param>
		/// <returns>取消订阅结果</returns>
		/// <example>
		/// 参照 <see cref="MqttClient"/> 的示例说明。
		/// <code lang="cs" source="HslCommunication_Net45.Test\Documentation\Samples\MQTT\MQTTClient.cs" region="Test" title="简单的实例化" />
		/// <code lang="cs" source="HslCommunication_Net45.Test\Documentation\Samples\MQTT\MQTTClient.cs" region="Test7" title="订阅示例" />
		/// </example>
		public OperateResult UnSubscribeMessage( string[] topics )
		{
			MqttSubscribeMessage subcribeMessage = new MqttSubscribeMessage( )
			{
				Identifier = (int)incrementCount.GetCurrentValue( ),
				Topics = topics,
			};

			OperateResult<byte[]> command = MqttHelper.BuildUnSubscribeMqttCommand( subcribeMessage );
			if (!command.IsSuccess) return command;

			OperateResult send = Send( this.CoreSocket, command.Content );
			if (!send.IsSuccess) return send;

			RemoveSubTopics( topics );
			return OperateResult.CreateSuccessResult( );
		}

		/// <summary>
		/// 取消订阅指定的主题信息，取消之后，就不再接收当前主题的数据，除非服务器强制推送<br />
		/// Unsubscribe from the specified topic information. After cancellation, the data of the current topic will no longer be received unless the server forces push
		/// </summary>
		/// <param name="topic">主题信息</param>
		/// <returns>取消订阅结果</returns>
		/// <example>
		/// 参照 <see cref="MqttClient"/> 的示例说明。
		/// <code lang="cs" source="HslCommunication_Net45.Test\Documentation\Samples\MQTT\MQTTClient.cs" region="Test" title="简单的实例化" />
		/// <code lang="cs" source="HslCommunication_Net45.Test\Documentation\Samples\MQTT\MQTTClient.cs" region="Test7" title="订阅示例" />
		/// </example>
		public OperateResult UnSubscribeMessage( string topic ) => UnSubscribeMessage( new string[] { topic } );

		private bool RemoveSubTopics( string[] topics )
		{
			bool remove = true;
			lock (subscribeLock)
			{
				for (int i = 0; i < topics.Length; i++)
				{
					if (subscribeTopics.ContainsKey( topics[i] ))
					{
						subscribeTopics.Remove( topics[i] );
					}
				}
			}
			return remove;
		}

		#endregion

		#region Async Subscribe Message
#if !NET35 && !NET20
		/// <inheritdoc cref="SubscribeMessage(string)"/>
		public async Task<OperateResult> SubscribeMessageAsync( string topic ) => await SubscribeMessageAsync( new string[] { topic } );

		/// <inheritdoc cref="SubscribeMessage(string[])"/>
		public async Task<OperateResult> SubscribeMessageAsync( string[] topics )
		{
			if (topics == null) return OperateResult.CreateSuccessResult( );
			if (topics.Length == 0) return OperateResult.CreateSuccessResult( );

			MqttSubscribeMessage subcribeMessage = new MqttSubscribeMessage( )
			{
				Identifier = (int)incrementCount.GetCurrentValue( ),
				Topics = topics,
			};

			OperateResult<byte[]> command = MqttHelper.BuildSubscribeMqttCommand( subcribeMessage );
			if (!command.IsSuccess) return command;

			AddSubTopics( topics );
			return await SendAsync( this.CoreSocket, command.Content );
		}

		/// <inheritdoc cref="UnSubscribeMessage(string[])"/>
		public async Task<OperateResult> UnSubscribeMessageAsync( string[] topics )
		{
			MqttSubscribeMessage subcribeMessage = new MqttSubscribeMessage( )
			{
				Identifier = (int)incrementCount.GetCurrentValue( ),
				Topics = topics,
			};

			OperateResult<byte[]> command = MqttHelper.BuildUnSubscribeMqttCommand( subcribeMessage );
			RemoveSubTopics( topics );
			return await SendAsync( this.CoreSocket, command.Content );
		}

		/// <inheritdoc cref="UnSubscribeMessage(string)"/>
		public async Task<OperateResult> UnSubscribeMessageAsync( string topic ) => await UnSubscribeMessageAsync( new string[] { topic } );

#endif
		#endregion

		#region Private Method

		private void OnMqttNetworkError(  )
		{
			if (closed) { LogNet?.WriteDebug( ToString( ), "Closed" ); return; }
			if (Interlocked.CompareExchange( ref isReConnectServer, 1, 0 ) == 0)
			{
				try
				{
					this.IsConnected = false;
					this.timerCheck?.Dispose( );
					this.timerCheck = null;
					if (OnNetworkError == null)
					{
						// 网络异常，系统准备在10秒后自动重新连接。
						LogNet?.WriteInfo( ToString( ), "The network is abnormal, and the system is ready to automatically reconnect after 10 seconds." );
						while (true)
						{
							// 每隔10秒重连
							for (int i = 0; i < 10; i++)
							{
								Thread.Sleep( 1_000 );
								LogNet?.WriteInfo( ToString( ), $"Wait for {10-i} second to connect to the server ..." );
								if (closed) { LogNet?.WriteDebug( ToString( ), "Closed" ); Interlocked.Exchange( ref isReConnectServer, 0 ); return; }
							}
							lock (connectLock)
							{
								if (closed) { LogNet?.WriteDebug( ToString( ), "Closed" ); Interlocked.Exchange( ref isReConnectServer, 0 ); return; }
								OperateResult connect = ConnectServer( );
								if (connect.IsSuccess)
								{
									// 连接成功后，可以在下方break之前进行订阅，或是数据初始化操作
									LogNet?.WriteInfo( ToString( ), "Successfully connected to the server!" );
									break;
								}
								LogNet?.WriteInfo( ToString( ), "The connection failed. Prepare to reconnect after 10 seconds." );
								if (closed) { LogNet?.WriteDebug( ToString( ), "Closed" ); Interlocked.Exchange( ref isReConnectServer, 0 ); return; }
							}
						}
					}
					else
					{
						OnNetworkError?.Invoke( this, new EventArgs( ) );
					}
					Interlocked.Exchange( ref isReConnectServer, 0 );
				}
				catch
				{
					Interlocked.Exchange( ref isReConnectServer, 0 );
					throw;
				}
			}
			else
			{
				//LogNet?.WriteDebug( ToString( ), "Current is connecting, this time cancel." );
			}
		}

#if NET35 || NET20
		private void ReceiveAsyncCallback( IAsyncResult ar )
#else
		private async void ReceiveAsyncCallback( IAsyncResult ar )
#endif
		{
			if (ar.AsyncState is Socket socket)
			{
				try
				{
					socket.EndReceive( ar );
				}
				catch (ObjectDisposedException)
				{
					socket?.Close( );
					LogNet?.WriteDebug( ToString( ), "Closed" );
					return;
				}
				catch (Exception ex)
				{
					socket?.Close( );
					LogNet?.WriteDebug( ToString( ), "ReceiveCallback Failed:" + ex.Message );
					OnMqttNetworkError( );
					return;
				}

				if (closed) { LogNet?.WriteDebug( ToString( ), "Closed" ); return; }

#if NET35 || NET20
				OperateResult<byte, byte[]> read = ReceiveMqttMessage( socket, 30_000 );
#else
				OperateResult<byte, byte[]> read = await ReceiveMqttMessageAsync( socket, 30_000 );
#endif
				if (!read.IsSuccess)
				{
					OnMqttNetworkError( );
					return;
				}

				byte mqttCode = read.Content1;
				byte[] data   = read.Content2;

				int code = mqttCode >> 4;
				if      (code == MqttControlMessage.PUBACK) LogNet?.WriteDebug( ToString( ), $"Code[{mqttCode:X2}] Publish Ack: {SoftBasic.ByteToHexString( data, ' ' )}" ); // Qos1的发布确认
				else if (code == MqttControlMessage.PUBREC) // Qos2的发布收到
				{
					Send( socket, MqttHelper.BuildMqttCommand( MqttControlMessage.PUBREL, 0x02, data, new byte[0] ).Content );
					LogNet?.WriteDebug( ToString( ), $"Code[{mqttCode:X2}] Publish Rec: {SoftBasic.ByteToHexString( data, ' ' )}" );
				}
				else if (code == MqttControlMessage.PUBCOMP)  LogNet?.WriteDebug( ToString( ), $"Code[{mqttCode:X2}] Publish Complete: {SoftBasic.ByteToHexString( data, ' ' )}" );  // Qos2的发布完成
				else if (code == MqttControlMessage.PINGRESP) { activeTime = DateTime.Now; LogNet?.WriteDebug( ToString( ), $"Heart Code Check!" ); }                                // 心跳响应
				else if (code == MqttControlMessage.SUBACK)   LogNet?.WriteDebug( ToString( ), $"Code[{mqttCode:X2}] Subscribe Ack: {SoftBasic.ByteToHexString( data, ' ' )}" );
				else if (code == MqttControlMessage.UNSUBACK) LogNet?.WriteDebug( ToString( ), $"Code[{mqttCode:X2}] UnSubscribe Ack: {SoftBasic.ByteToHexString( data, ' ' )}" );
				else if (code == MqttControlMessage.PUBLISH)  ExtraPublishData( mqttCode, data );                                                                                    // 收到订阅反馈
				else LogNet?.WriteDebug( ToString( ), $"Code[{mqttCode:X2}] {SoftBasic.ByteToHexString( data, ' ' )}" );                                                             // 其他未知的指令

				try
				{
					socket.BeginReceive( new byte[0], 0, 0, SocketFlags.None, new AsyncCallback( ReceiveAsyncCallback ), socket );
				}
				catch (Exception ex)
				{
					socket?.Close( );
					LogNet?.WriteDebug( ToString( ), "BeginReceive Failed:" + ex.Message );
					OnMqttNetworkError( );
				}
			}
		}
		private void ExtraPublishData( byte mqttCode, byte[] data )
		{
			activeTime = DateTime.Now;
			OperateResult<string, byte[]> extra = MqttHelper.ExtraMqttReceiveData( mqttCode, data, this.aesCryptography );
			if (!extra.IsSuccess) { LogNet?.WriteDebug( ToString( ), extra.Message ); return; }

			OnMqttMessageReceived?.Invoke( this, extra.Content1, extra.Content2 );

			// 触发子订阅的消息
			SubscribeTopic subscribeTopic = GetSubscribeTopic( extra.Content1 );
			subscribeTopic?.TriggerSubscription( this, extra.Content1, extra.Content2 );
		}

		private void TimerCheckServer( object obj )
		{
			if (this.CoreSocket != null)
			{
				if ((DateTime.Now - activeTime).TotalSeconds > this.connectionOptions.KeepAliveSendInterval.TotalSeconds * 3)
				{
					// 3个心跳周期没有接收到数据
					OnMqttNetworkError( );
				}
				else
				{
					if (!Send( this.CoreSocket, MqttHelper.BuildMqttCommand( MqttControlMessage.PINGREQ, 0x00, new byte[0], new byte[0] ).Content ).IsSuccess)
						OnMqttNetworkError( );
				}
			}
		}

		#endregion

		#region Publish Message

		private void AddPublishMessage( MqttPublishMessage publishMessage )
		{

		}

		#endregion

		#region Event Handle

		/// <summary>
		/// 当接收到Mqtt订阅的信息的时候触发<br />
		/// Triggered when receiving Mqtt subscription information
		/// </summary>
		/// <param name="client">收到消息时候的client实例对象</param>
		/// <param name="topic">主题信息</param>
		/// <param name="payload">负载数据</param>
		public delegate void MqttMessageReceiveDelegate( MqttClient client, string topic, byte[] payload );

		/// <summary>
		/// 当接收到Mqtt订阅的信息的时候触发
		/// </summary>
		public event MqttMessageReceiveDelegate OnMqttMessageReceived;

		/// <summary>
		/// 当网络发生异常的时候触发的事件，用户应该在事件里进行重连服务器
		/// </summary>
		public event EventHandler OnNetworkError;

		/// <summary>
		/// 连接服务器成功的委托<br />
		/// Connection server successfully delegated
		/// </summary>
		public delegate void OnClientConnectedDelegate( MqttClient client );

		/// <summary>
		/// 当客户端连接成功触发事件，就算是重新连接服务器后，也是会触发的<br />
		/// The event is triggered when the client is connected successfully, even after reconnecting to the server.
		/// </summary>
		public event OnClientConnectedDelegate OnClientConnected;

		#endregion

		#region IDispose

		/// <summary>
		/// 释放当前的对象
		/// </summary>
		/// <param name="disposing"></param>
		protected virtual void Dispose( bool disposing )
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					this.incrementCount?.Dispose( );
					this.timerCheck?.Dispose( );
					this.OnClientConnected = null;
					this.OnMqttMessageReceived = null;
					this.OnNetworkError = null;
				}

				disposedValue = true;
			}
		}

		/// <inheritdoc cref="IDisposable.Dispose"/>
		public void Dispose( )
		{
			// 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
			Dispose( disposing: true );
			GC.SuppressFinalize( this );
		}

		#endregion

		#region Private Member

		private DateTime activeTime;                                          // 激活时间
		private int isReConnectServer = 0;                                    // 是否重连服务器中
		private List<MqttPublishMessage> publishMessages;                     // 缓存的等待处理的Qos大于0的消息
		private object listLock;                                              // 缓存的处理的消息的队列
		private Dictionary<string, SubscribeTopic> subscribeTopics;           // 缓存的等待处理的订阅的消息内容
		private object connectLock;                                           // 连接服务器的锁
		private object subscribeLock;                                          // 订阅的主题的队列锁
		private SoftIncrementCount incrementCount;                            // 自增的数据id对象
		private bool closed = false;                                          // 客户端是否关闭
		private MqttConnectionOptions connectionOptions;                      // 连接服务器时的配置信息
		private Timer timerCheck;                                             // 定时器，用来心跳校验的
		private bool disposedValue;
		private RSACryptoServiceProvider cryptoServiceProvider = null;        // 账户验证加密对象
		private AesCryptography aesCryptography = null;                       // 数据请求加密对象

		#endregion

		#region Public Properties

		/// <summary>
		/// 获取当前的连接配置参数信息<br />
		/// Get current connection configuration parameter information
		/// </summary>
		public MqttConnectionOptions ConnectionOptions => this.connectionOptions;

		/// <summary>
		/// 获取或设置是否启动定时器去检测当前客户端是否超时掉线。默认为 <c>True</c><br />
		/// Get or set whether to start the timer to detect whether the current client timeout and disconnection. Default is <c>True</c>
		/// </summary>
		public bool UseTimerCheckDropped { get; set; } = true;

		/// <summary>
		/// 获取或设置当前的服务器连接是否成功，定时获取本属性可用于实时更新连接状态信息。<br />
		/// Get or set whether the current server connection is successful or not. 
		/// This property can be obtained regularly and can be used to update the connection status information in real time.
		/// </summary>
		public bool IsConnected { get; private set; } = false;

		/// <summary>
		/// 获取当前的客户端对象已经订阅的所有的Topic信息<br />
		/// Get all Topic information that the current client object has subscribed to
		/// </summary>
		public string[] SubcribeTopics 
		{
			get
			{
				lock (subscribeLock)
					return subscribeTopics.Keys.ToArray( );
			}
		}
		#endregion

		#region Object Override

		/// <inheritdoc/>
		public override string ToString( ) => $"MqttClient[{this.connectionOptions.IpAddress}:{this.connectionOptions.Port}]";

		#endregion
	}

	/// <summary>
	/// 订阅的主题信息<br />
	/// Subscribed topic information
	/// </summary>
	public class SubscribeTopic
	{
		/// <summary>
		/// 使用指定的主题初始化<br />
		/// Initialize with the specified theme
		/// </summary>
		/// <param name="topic">主题信息</param>
		public SubscribeTopic( string topic )
		{
			this.Topic = topic;
		}

		/// <summary>
		/// 主题信息<br />
		/// topic information
		/// </summary>
		public string Topic { get; set; }

		/// <summary>
		/// 主题被订阅的次数<br />
		/// The number of times the topic was subscribed
		/// </summary>
		public long SubscribeTick => this.subscribeTick;


		private long subscribeTick = 0;                                  // 当前主题被订阅的次数

		/// <summary>
		/// 当接收到Mqtt订阅的信息的时候触发的事件<br />
		/// Event triggered when a message subscribed to by Mqtt is received
		/// </summary>
		public event MqttMessageReceiveDelegate OnMqttMessageReceived;

		/// <summary>
		/// 使用指定的参数，触发订阅主题的事件<br />
		/// Using the specified parameters, trigger the event of the subscribed topic
		/// </summary>
		/// <param name="client">客户端会话信息</param>
		/// <param name="topic">主题信息</param>
		/// <param name="payload">数据负载</param>
		public void TriggerSubscription( MqttClient client, string topic, byte[] payload )
		{
			OnMqttMessageReceived?.Invoke( client, topic, payload );
		}

		/// <summary>
		/// 增加一个订阅的计数信息，不需要手动调用，在 <see cref="MqttClient"/> 订阅主题之后，会自动增加<br />
		/// Add a subscription count information, no need to manually call it, it will be automatically added after <see cref="MqttClient"/> subscribes to the topic
		/// </summary>
		/// <returns>返回增加计数后的值</returns>
		public long AddSubscribeTick( )
		{
			return Interlocked.Increment( ref this.subscribeTick );
		}

		/// <summary>
		/// 减少一个订阅的计数信息，用户可以手动调用，比如判断是否是最后一次移除，然后决定是否通过 <see cref="MqttClient"/> 取消订阅主题<br />
		/// Reduce the count information of a subscription, the user can call it manually, such as judging whether it is the last removal, 
		/// and then decide whether to unsubscribe the topic through <see cref="MqttClient"/>
		/// </summary>
		/// <returns>返回减少计数后的值</returns>
		public long RemoveSubscribeTick( )
		{
			return Interlocked.Decrement( ref this.subscribeTick );
		}

		/// <inheritdoc/>
		public override string ToString( ) => $"SubscribeTopic[{Topic}]";
	}
}
