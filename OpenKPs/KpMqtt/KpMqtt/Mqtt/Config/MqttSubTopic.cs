﻿namespace Scada.Comm.Devices.Mqtt.Config
{
    /// <summary>
    /// Represents a subscription to a topic.
    /// </summary>
    public class MqttSubTopic : MqttTopic
    {
        public int TagIndex { get; set; }
    }
}
