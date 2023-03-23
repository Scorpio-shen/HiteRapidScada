using KpHiteMqtt.Mqtt.Model.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KpHiteMqtt.Mqtt.ViewModel
{
    public class FrmDevModelViewModel
    {
        public string Name { get;set; }
        public DataTypeEnum DataType { get; set; }

        public bool IsReadOnly { get; set; }
        public int InputChannel { get; set; }
        public bool InputChannelVisable { get; set; }
        public bool OutputChannelVisable { get; set; }
        public int OutputChannel { get; set; }
        public string Description { get;set; }
    }
}
