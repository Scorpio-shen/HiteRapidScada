using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KpHiteModbus.Modbus.Model
{
    /// <summary>
    /// Modbus寄存器类型
    /// </summary>
    public enum RegisterTypeEnum
    {
        [Description("线圈")]
        Coils,
        [Description("离散输入")]
        DiscretesInputs,
        [Description("输入寄存器")]
        InputRegisters,
        [Description("保持寄存器")]
        HoldingRegisters
    }
}
