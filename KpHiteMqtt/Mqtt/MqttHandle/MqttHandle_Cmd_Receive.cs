using KpHiteMqtt.Mqtt.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KpHiteMqtt.Mqtt.MqttHandle
{
    /// <summary>
    /// Mqtt下发指令接收
    /// </summary>
    public class MqttHandle_Cmd_Receive : MqttHandleBase
    {
        protected override string Topic => "/HiteScada/ScadaTest/event/cmd";
        /// <summary>
        /// 通道
        /// </summary>
        public Action<int, object> SetCtrlCnlValue;
        
        public override void Handle(string topic, string content)
        {
            try
            {
                var payload = JsonConvert.DeserializeObject<HiteMqttPayload>(content);
                var mqttContents = JsonConvert.DeserializeObject<List<MqttContent>>(payload.Content);

                foreach(var mqttcontent in mqttContents)
                {
                    if(mqttcontent.DataTypeValue == Model.Enum.DataTypeEnum.Array)
                    {

                    }
                    else if(mqttcontent.DataTypeValue == Model.Enum.DataTypeEnum.Struct)
                    {

                    }
                    else
                    {

                    }
                }
            }
            catch (Exception ex)
            {

            }
        }
    }
}
