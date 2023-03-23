using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KpHiteMqtt.Mqtt.Model.Request
{
    public class MqttMonitorDataSet
    {
        public string Version { get; set; }
        public List<MonitorData> Data { get; set; }
    }
}
