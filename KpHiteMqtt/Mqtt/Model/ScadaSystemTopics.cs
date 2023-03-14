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
        public static string MqttTsModelData_Publish = "/HiteScada/service/property/post";

        public static string MqttCmdReply_Publish = "/HiteScada/event/cmd_reply";

        public static string MqttCmd_Subscribe = "/HiteScada/event/cmd";

        public static string MqttTsModelDataReply_Subscribe = "/HiteScada/service/property/post_reply";


    }
}
