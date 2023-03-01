using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KpHiteMqtt.Config.Mqtt
{
    public abstract class MqttTopic
    {
        /// <summary>
        /// Gets or sets the topic name.
        /// </summary>
        public string TopicName { get; set; }

        /// <summary>
        /// The QOS levels are a way of guaranteeing message delivery and they  refer to the connection between a broker and a client.
        /// QOS 0 – Once (not guaranteed)
        /// QOS 1 – At Least Once(guaranteed)
        /// QOS 2 – Only Once(guaranteed)
        /// </summary>
        public MqttQos QosLevel { get; set; }
    }
}
