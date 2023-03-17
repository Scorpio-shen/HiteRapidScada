using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KpHiteMqtt.Mqtt.Model
{
    public class MqttConnectionOptions
    {
        public string ClientId { get;set; }
        public string IpAddress { get;set; }    
        /// <summary>
        /// 设备号
        /// </summary>
        public string DeviceSn { get;set; }
        public int Port { get;set; }

        public int ConnectTimeout { get; set; } = 5;
        public TimeSpan KeepAliveSendInterval { get; set; }

        public string UserName { get;set; }
        public string Password { get;set; } 

        public bool UseTsl { get;set; } = true;
    }
}
