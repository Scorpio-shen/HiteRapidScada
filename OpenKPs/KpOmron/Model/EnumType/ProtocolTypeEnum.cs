using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KpOmron.Model.EnumType
{
    /// <summary>
    /// 协议类型
    /// </summary>
    public enum ProtocolTypeEnum
    {
        [Description("Fins-Tcp")]
        FinsTcp,
        [Description("Fins Udp")]
        FinsUdp,
        [Description("CIP")]
        EtherNetIP_CIP,
        [Description("Connected CIP")]
        ConnectedCIP,
        [Description("HostLink Serial")]
        HostLinkSerial,
        [Description("HostLink OverTcp")]
        HostLinkOverTcp,
        [Description("HostLink C-Mode")]
        HostLinkCMode,
        [Description("CMode OverTcp")]
        CModeOverTcp

    }
}
