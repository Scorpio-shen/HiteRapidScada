using HslCommunication;
using HslCommunication.Profinet.AllenBradley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KpCommon.Hsl.Profinet.AllenBradley.InterFace
{
    public interface IAbReadWriteCip : IReadWriteCip
    {
        OperateResult<byte[]> Read(string[] address, int[] length);

        OperateResult<byte[]> ReadSegment(string address, int startIndex, int length);
    }
}
