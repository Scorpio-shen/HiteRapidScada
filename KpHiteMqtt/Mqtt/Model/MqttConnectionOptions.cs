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
        /// <summary>
        /// 目标服务器IP
        /// </summary>
        public string ServerIpAddress { get;set; }    

        /// <summary>
        /// 本机连接的IPAddress
        /// </summary>
        public string LocalIpAddress { get; set; }
        /// <summary>
        /// 本机连接端口
        /// </summary>
        public int LocalPort { get; set; }
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
