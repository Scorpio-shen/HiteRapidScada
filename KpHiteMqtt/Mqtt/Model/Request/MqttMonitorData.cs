using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KpHiteMqtt.Mqtt.Model.Request
{
    public class MqttMonitorData
    {

        public string time { get;set; }

        public List<MonitorData> Data { get; set; }
    }

    public class MonitorData
    {
        public string name { get; set; }
        public string value { get; set; }
    }
}
