using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KpMelsec.Model.EnumType
{
    public enum ConnectionTypeEnum
    {
        [Description("SerialPort")]
        SerialPort,
        [Description("TcpIP")]
        TcpIP
    }
}
