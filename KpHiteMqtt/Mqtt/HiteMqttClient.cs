using HslCommunication;
using HslCommunication.MQTT;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils;
using static HslCommunication.MQTT.MqttClient;

namespace KpHiteMqtt.Mqtt
{
    public delegate void MqttMessageReceived(string topic, string content);
    public class HiteMqttClient
    {
        private MqttClient mqttClient;
        private Log.WriteLineDelegate _writeToLog;
        public HiteMqttClient(MqttConnectionOptions mqttConnectionOptions,Log.WriteLineDelegate writeToLog)
        {
            _writeToLog = writeToLog;
            mqttClient = new MqttClient(mqttConnectionOptions);
            mqttClient.OnClientConnected += MqttClient_OnClientConnected;
            mqttClient.OnNetworkError += MqttClient_OnNetworkError;
            mqttClient.OnMqttMessageReceived += MqttClient_OnMqttMessageReceived;
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
        public OperateResult ConnectServer()
        {
            var result = mqttClient.ConnectServer();
            return result;
        }

        public void DisConnect()
        {
            if (mqttClient == null)
                return;
            if (!IsConnected)
                return;

            mqttClient.ConnectClose();
        }
        #endregion

        #region Mqtt事件
        /// <summary>
        /// 消息接收
        /// </summary>
        /// <param name="client"></param>
        /// <param name="topic"></param>
        /// <param name="payload"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void MqttClient_OnMqttMessageReceived(MqttClient client, string topic, byte[] payload)
        {
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
            _writeToLog?.Invoke($"HiteMqttClient:MqttClient_OnMqttMessageReceived,Mqtt数据接收,Topic:{topic},{content}");
            OnMqttMessageReceived?.Invoke(topic, content);
        }

        private void MqttClient_OnNetworkError(object sender, EventArgs e)
        {
            _writeToLog?.Invoke($"HiteMqttClient:MqttClient_OnNetworkError,Mqtt网络异常");
            OnMqttDisConnected?.Invoke(sender, e);
        }

        private void MqttClient_OnClientConnected(MqttClient client)
        {
            _writeToLog?.Invoke($"HiteMqttClient:MqttClient_OnClientConnected,Mqtt成功连接,连接参数{client.ConnectionOptions.ToJsonString()}");
            OnMqttConnected?.Invoke(client, new EventArgs());
        }
        #endregion

        #region 数据Publish
        public async Task<bool> PublishAsync(string topic, byte[] payload,MqttQualityOfServiceLevel mqttQuality = MqttQualityOfServiceLevel.AtMostOnce,bool retain = false)
        {
            if(!IsConnected)
                return false;
            var result = await mqttClient.PublishMessageAsync(new MqttApplicationMessage
            {
                Topic = topic,
                Payload = payload,
                QualityOfServiceLevel = mqttQuality,
                Retain = retain
            });
            return result.IsSuccess;
        }
        #endregion

        #region 订阅Topic
        public async Task<bool> Subscribe(string topic)
        {
            var topics = mqttClient.SubcribeTopics;
            if (topics.Contains(topic))
                return true;
            var result = await mqttClient.SubscribeMessageAsync(topic);

            return result.IsSuccess;
        }
        #endregion
    }
}
