using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KpHiteModbus.Modbus.Model
{
    public class ModbusRegisterMaxDefine
    {
        /// <summary>
        /// 定义读取Coil或者DI寄存器时，限制一次最大请求寄存器长度,Hsl 读取超过255会报异常
        /// </summary>
        //public readonly static ushort MaxCoilOrDiRequestLength = 200;

        /// <summary>
        /// 定义读取HR或者IR寄存器，限制一次最大请求寄存器长度
        /// </summary>
        //public readonly static ushort MaxHrOrIRRequestLength = 2000;
        /// <summary>
        /// 定义读取Coil或者DI寄存器时，限制分包请求线程最大个数
        /// </summary>
        //public readonly static int MaxCoilOrDiRequestThreadCount = 5;
    }
}
