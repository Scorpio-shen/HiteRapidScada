using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KpHiteModbus.Modbus.Model.EnumType
{
    public enum ModbusConnectionTypeEnum
    {
        [Description("SerialPort")]
        SerialPort,
        [Description("TcpIP")]
        TcpIP,
        [Description("Udp")]
        Udp,
        [Description("RTUASCIIOverTcp")]
        RTUASCIIOverTcp,
        [Description("RTUASCIIOverUdp")]
        RTUASCIIOverUdp,
    }
}
