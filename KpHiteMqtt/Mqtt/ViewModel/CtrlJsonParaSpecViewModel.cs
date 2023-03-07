using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KpHiteMqtt.Mqtt.ViewModel
{
    public class CtrlJsonParaSpecViewModel
    {
        public string Name { get; set; }
        public int InputChannel { get; set; }
        public bool InputChannelVisable { get; set; }
        public int OutputChannel { get; set; }
        public bool OutputChannelVisable { get; set; }
    }
}
