using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KpAllenBrandly.Model.EnumType
{
    public enum ProtocolTypeEnum
    {
        [Description("EtherNet/IP")]
        EtherNetIP,
        [Description("CIP")]
        MicroCip,
        //[Description("SLC")]
        //SLCNet,

    }
}
