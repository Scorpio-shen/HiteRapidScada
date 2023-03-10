using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KpHiteMqtt.Mqtt.MqttHandle
{
    public class MqttHandle_post_reply : MqttHandleBase
    {
        protected override string Topic => "/HiteScada/ScadaTest/modeldata/post_reply";

        public override void Handle(string topic, string content)
        {
            throw new NotImplementedException();
        }
    }
}
