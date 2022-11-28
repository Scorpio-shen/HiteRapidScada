using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KpMyModbus.Modbus.Protocol
{
    public enum TagDataType
    {
        Undefined,
        UShort,
        Short,
        UInt,
        Int,
        ULong,
        Long,
        Float,
        Double,
        Bool
    }
}
