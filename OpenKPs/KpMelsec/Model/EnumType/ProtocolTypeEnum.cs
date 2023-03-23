using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KpMelsec.Model.EnumType
{
    public enum ProtocolTypeEnum
    {
        [Description("Melsec CIP")]
        MelsecCIP,
        [Description("MC Qna1E Binary")]
        McQna1EBinary,
        [Description("MC Qna1E Ascii")]
        McQna1EAscii,
        [Description("MC Qna3E Binary")]
        McQna3EBinary,
        [Description("MC Qna3E Binary Udp")]
        MCQna3EBinaryUdp,
        [Description("MC Qna3E ASCII")]
        McQna3EASCII,
        [Description("MC Qna3E ASCII Udp")]
        MCQna3EASCIIUdp,
        [Description("MC R Serial Qna3E Binary")]
        MCRSerialQna3EBinary,
        [Description("Fx Serial")]
        FxSerial,
        [Description("Fx Serial Over Tcp")]
        FxSerialOverTcp,
        [Description("Fx Links 485")]
        FxLinks485,
        [Description("Fx Links Over Tcp")]
        FxLinksOverTcp,
        [Description("Qna 3c")]
        Qna3c,
        [Description("Qna 3c OverTcp")]
        Qna3cOverTcp
    }
}
