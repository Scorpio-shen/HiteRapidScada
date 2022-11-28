using HslCommunication.Profinet.Siemens;
using KpSiemens.Siemens.Model;
using System;

namespace KpSiemens.Siemens.Extend
{
    public static class SiemensPLCTypeEnumExtend
    {
        public static SiemensPLCS ToHslSiemensPLCS(this SiemensPLCTypeEnum siemensPLCType)
        {
            SiemensPLCS result = SiemensPLCS.S1200;
            Enum.TryParse(siemensPLCType.ToString(), out result);
            return result;
        }
    }
}
