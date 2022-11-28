using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KpMyModbus.Modbus.Protocol
{
    /// <summary>
    /// Modbus寄存器枚举
    /// </summary>
    public enum ModbusRegisterType
    {
        [Description("离散输入")]
        DiscreteInputs = 2,

        [Description("线圈")]
        Coils = 1,

        [Description("输入寄存器")]
        InputRegisters = 4,

        [Description("保持寄存器")]
        HoldingRegisters = 3
    }
}
