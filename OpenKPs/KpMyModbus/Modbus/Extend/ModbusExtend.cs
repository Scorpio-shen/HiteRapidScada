using HslCommunication.ModBus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KpMyModbus.Modbus.Extend
{
    public static class ModbusExtend
    {
        public static void Write(this IModbus modbus,dynamic data)
        {
            if(modbus is ModbusTcpNet modbusTcpNet)
            {
                modbusTcpNet.Write(data);
            }
        }
    }
}
