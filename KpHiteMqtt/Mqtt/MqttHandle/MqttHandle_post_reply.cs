using KpCommon.Helper;
using KpHiteMqtt.Mqtt.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KpHiteMqtt.Mqtt.MqttHandle
{
    public class MqttHandle_post_reply : MqttHandleBase
    {
        protected override string Topic => ScadaSystemTopics.MqttTsModelDataReply_Subscribe;

        public override void Handle(string topic, string content)
        {
            try
            {
                //base64解密
                var decodeContent = EncryptionHelper.Base64Decode(content);
                var mqttpayload = JsonConvert.DeserializeObject<HiteMqttPayload>(decodeContent);
                WriteToLog($"MqttHandle_post_reply:Handle,处理消息,Topic:{topic},解密后decodeContent:{decodeContent},MqttPayload:{JsonConvert.SerializeObject(mqttpayload)}");

            }
            catch (Exception ex)
            {
                WriteToLog($"MqttHandle_post_reply:Handle,处理消息异常,{ex.Message},原始Content:{content}");
            }
        }
    }
}
