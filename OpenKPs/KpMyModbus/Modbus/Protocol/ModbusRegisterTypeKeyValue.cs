using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KpMyModbus.Modbus.Protocol
{
    public class ModbusRegisterTypeKeyValue
    {
        public string ModbusRegisterTypeName { get; set; }
        //public int ModbusRegisterTypeValue { get; set; }

        public ModbusRegisterType ModbusRegisterType { get; set; }
    }
}
