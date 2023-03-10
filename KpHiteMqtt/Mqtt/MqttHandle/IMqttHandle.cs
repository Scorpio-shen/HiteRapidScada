﻿using KpHiteMqtt.Mqtt.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KpHiteMqtt.Mqtt.MqttHandle
{
    public interface IMqttHandle
    {
        DeviceTemplate DeviceTemplate { get; set; }
        Dictionary<int, InputChannelModel> InputChannelModels { get; set; }
        HiteMqttClient MqttClient { get; set; }

        bool HandleTopic(string topic);

        void Handle(string topic, string content);
    }

    public abstract class MqttHandleBase : IMqttHandle
    {
        protected abstract string Topic { get; }
        public DeviceTemplate DeviceTemplate {get; set;}
        public Dictionary<int, InputChannelModel> InputChannelModels { get; set; }
        public HiteMqttClient MqttClient { get; set; }

        public abstract void Handle(string topic, string content);

        public bool HandleTopic(string topic)
        {
            return Topic.Equals(topic);
        }
    }
}