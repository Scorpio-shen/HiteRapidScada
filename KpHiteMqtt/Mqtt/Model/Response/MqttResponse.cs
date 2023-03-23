using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KpHiteMqtt.Mqtt.Model.Response
{
    /// <summary>
    /// 执行Cmd指令后的响应
    /// </summary>
    public class MqttResponse
    {
        /// <summary>
        /// 200执行成功,400执行失败
        /// </summary>
        public int Code { get; set; }
        /// <summary>
        /// 响应消息
        /// </summary>
        public string Message { get; set; }
        /// <summary>
        /// 响应消息内容
        /// </summary>
        public string Content { get; set; }
    }


}
