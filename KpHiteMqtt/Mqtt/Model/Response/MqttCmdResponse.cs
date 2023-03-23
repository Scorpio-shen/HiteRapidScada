using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KpHiteMqtt.Mqtt.Model.Response
{
    public class MqttCmdResponse
    {
        /// <summary>
        /// 执行是否成功
        /// </summary>
        public bool IsSuccess { get;set; }
        /// <summary>
        /// 消息
        /// </summary>
        public string Message { get;set; }
        /// <summary>
        /// 对物模型Id
        /// </summary>
        public string Id { get;set; }   
        /// <summary>
        /// 物模型标识符
        /// </summary>
        public string Identifier { get;set; }
    }
}
