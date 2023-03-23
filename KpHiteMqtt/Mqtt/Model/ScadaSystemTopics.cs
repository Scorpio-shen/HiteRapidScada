using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KpHiteMqtt.Mqtt.Model
{
    /// <summary>
    /// 系统自带Topic
    /// </summary>
    public static class ScadaSystemTopics
    {
        /// <summary>
        /// 监控数据上报
        /// </summary>
        public static string MqttTsModelData_Publish = "HiteScada/service/{设备号}/system/MonitorData";
        /// <summary>
        /// 接收指令（回复）
        /// </summary>
        public static string MqttCmdReply_Publish = "HiteScada/event/{设备号}/system/WriteData_reply";
        /// <summary>
        /// 接收指令
        /// </summary>
        public static string MqttCmd_Subscribe = "HiteScada/event/{设备号}/system/WriteData";
        /// <summary>
        /// 监控数据上报回复
        /// </summary>
        public static string MqttTsModelDataReply_Subscribe = "HiteScada/service/{设备号}/system/MonitorData_reply";


    }
}
