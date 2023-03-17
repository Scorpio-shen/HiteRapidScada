using HslCommunication;
using KpCommon.Helper;
using KpHiteMqtt.Mqtt.Model;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Protocol;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Utils;

namespace KpHiteMqtt.Mqtt
{
    public delegate void MqttMessageReceived(string topic, string content);
    public class HiteMqttClient
    {
        //private MqttClient mqttClient;
        private IMqttClient mqttClient;
        private Log.WriteLineDelegate _writeToLog;
        private MqttConnectionOptions _connectionOptions;
        private List<string> subscribeTopics;
        private CancellationTokenSource tokenSourcePingServer;
        private double KeepAliveSeconds;
        private DateTime lastPingTime = DateTime.Now;
        public HiteMqttClient(MqttConnectionOptions mqttConnectionOptions,Log.WriteLineDelegate writeToLog)
        {
            _writeToLog = writeToLog;
            _connectionOptions = mqttConnectionOptions;
            KeepAliveSeconds = _connectionOptions.KeepAliveSendInterval.TotalSeconds;
            subscribeTopics = new List<string>();
            var factory = new MqttFactory();
            mqttClient = factory.CreateMqttClient();
            mqttClient.ConnectedAsync += MqttClient_ConnectedAsync;
            mqttClient.DisconnectedAsync += MqttClient_DisconnectedAsync;
            mqttClient.ApplicationMessageReceivedAsync += MqttClient_ApplicationMessageReceivedAsync;
        }

        

        #region 事件
        public event MqttMessageReceived OnMqttMessageReceived;
        public event EventHandler OnMqttConnected;
        public event EventHandler OnMqttDisConnected;
        #endregion

        #region 属性
        public bool IsConnected
        {
            get=> mqttClient?.IsConnected == true;
        }
        #endregion


        #region 连接与断开
        public async Task<bool> ConnectServer()
        {
            var mqttClientOptionBulider = new MqttClientOptionsBuilder()
                    .WithTcpServer(_connectionOptions.IpAddress, _connectionOptions.Port)
                    .WithCredentials(_connectionOptions.UserName, _connectionOptions.Password)
                    .WithClientId(_connectionOptions.ClientId)
                    .WithKeepAlivePeriod(_connectionOptions.KeepAliveSendInterval.Add(TimeSpan.FromSeconds(5)));
            if (_connectionOptions.UseTsl)
                mqttClientOptionBulider.WithTls();
            var result = await mqttClient.ConnectAsync(mqttClientOptionBulider.Build());
            return result.ResultCode == MqttClientConnectResultCode.Success;
        }

        public void DisConnect()
        {
            if (tokenSourcePingServer != null && !tokenSourcePingServer.IsCancellationRequested)
            {
                tokenSourcePingServer.Cancel();
                tokenSourcePingServer = null;
            }
                
            if (!IsConnected)
                return;
            mqttClient.DisconnectAsync();
        }
        #endregion

        #region Mqtt事件
        private Task MqttClient_ApplicationMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs arg)
        {
            string topic = arg.ApplicationMessage.Topic;
            byte[] payload = arg.ApplicationMessage.Payload;
            string content = string.Empty;
            bool isSuccess = true;
            try
            {
                content = Encoding.UTF8.GetString(payload);
            }
            catch
            {
                isSuccess = false;
            }
            if (!isSuccess)
                content = "Mqtt消息转换异常";
            _writeToLog?.Invoke($"HiteMqttClient:MqttClient_ApplicationMessageReceivedAsync,Mqtt数据接收,Topic:{topic},{content}");
            OnMqttMessageReceived?.Invoke(topic, content);
            return Task.CompletedTask;
        }

        private Task MqttClient_DisconnectedAsync(MqttClientDisconnectedEventArgs arg)
        {
            _writeToLog?.Invoke($"HiteMqttClient:MqttClient_DisconnectedAsync,Mqtt断开连接，{arg.Reason}");
            OnMqttDisConnected?.Invoke(this, new EventArgs());
            if (tokenSourcePingServer != null && !tokenSourcePingServer.IsCancellationRequested)
            {
                tokenSourcePingServer.Cancel();
                tokenSourcePingServer = null;
            }
            return Task.CompletedTask;
        }

        private async Task MqttClient_ConnectedAsync(MqttClientConnectedEventArgs arg)
        {
            _writeToLog?.Invoke($"HiteMqttClient:MqttClient_ConnectedAsync,Mqtt成功连接,连接参数{_connectionOptions.ToJsonString()}");
            OnMqttConnected?.Invoke(mqttClient, new EventArgs());

            //定时Ping服务器
            await PingMqttServer();
        }
        #endregion

        #region 数据Publish
        /// <summary>
        /// 发布数据
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="payload">Base64加密后转字节数据</param>
        /// <param name="mqttQuality"></param>
        /// <param name="retain"></param>
        /// <returns></returns>
        public async Task<bool> PublishAsync(string topic, byte[] payload,MqttQualityOfServiceLevel mqttQuality = MqttQualityOfServiceLevel.AtMostOnce,bool retain = false)
        {
            if(!IsConnected)
                return false;
            var result = await mqttClient.PublishAsync(new MqttApplicationMessage
            {
                Topic = topic,
                Payload = payload,
                QualityOfServiceLevel = mqttQuality,
                Retain = retain
            });
            return result.IsSuccess;
        }

        public async Task<bool> PublishAsync(string topic,HiteMqttPayload payload, MqttQualityOfServiceLevel mqttQuality = MqttQualityOfServiceLevel.AtMostOnce, bool retain = false)
        {
            if (!IsConnected)
                return false;
            var payloadStr = JsonConvert.SerializeObject(payload);
            var result = await mqttClient.PublishAsync(new MqttApplicationMessage
            {
                Topic = topic,
                Payload = Encoding.UTF8.GetBytes(EncryptionHelper.Base64Encode(payloadStr)),
                QualityOfServiceLevel = mqttQuality,
                Retain = retain
            });
            return result.IsSuccess;
        }
        #endregion

        #region 订阅Topic
        public async Task<bool> Subscribe(string topic)
        {
       
            if (subscribeTopics.Contains(topic))
                return true;
            var result = await mqttClient.SubscribeAsync(topic);
            subscribeTopics.Add(topic);
            return true;
        }
        #endregion

        #region 定时ping服务器
        private async Task PingMqttServer()
        {
            if (tokenSourcePingServer != null)
                return;
            tokenSourcePingServer = new CancellationTokenSource();
            var pingToken = tokenSourcePingServer.Token;
            await Task.Factory.StartNew(async () =>
            {
                while (!pingToken.IsCancellationRequested)
                {
                    if (IsConnected)
                    {
                        if (DateTime.Now - lastPingTime > _connectionOptions.KeepAliveSendInterval)
                            await mqttClient.PingAsync(pingToken);
                        else
                            await Task.Delay(2 * 1000);

                    }
                    else
                        break;
                }
            }, pingToken, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }
        #endregion
    }
}
