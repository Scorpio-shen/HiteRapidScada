using HslCommunication.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KpCommon.Hsl.Profinet.Omron.InterFace
{
    //
    // 摘要:
    //     HostLink的接口实现
    public interface IHostLink : IReadWriteDevice, IReadWriteNet
    {
        byte ICF { get; set; }

        byte DA2 { get; set; }

        byte SA2 { get; set; }

        byte SID { get; set; }

        byte ResponseWaitTime { get; set; }

        byte UnitNumber { get; set; }

        int ReadSplits { get; set; }
    }

}
