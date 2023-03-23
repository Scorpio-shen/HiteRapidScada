using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KpHiteMqtt.Mqtt.Model
{
    public class ArrayChannelMap
    {
        public int Index { get; set; }  
        public string Identifier { get; set; }
        public int InputChannel { get; set; }
        public int OutputChannel { get; set; }

        public Dictionary<string,Parameter> Parameters { get; set; }
    }

    public class Parameter
    {
        public int InputChannel { get;set; }
        public int OutputChannel { get; set; }
    }
}
