using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KpHiteMqtt.Mqtt.Model
{
    /// <summary>
    /// Mqtt数据模型
    /// </summary>
    public class HiteMqttDataModel
    {
        public MqttData data { get; set; }
        public string time { get; set; }
    }
    /// <summary>
    /// 数据内容
    /// </summary>
    public class MqttData
    {
        public string id { get; set; }
        public string name { get; set; }
        public string value { get; set; }
    }
}
