using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestConsole
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(String.Join(",", Enum.GetNames(typeof(ModbusRegisterType))));
            foreach(ModbusRegisterType value in Enum.GetValues(typeof(ModbusRegisterType)))
            {
                Console.WriteLine((int)value);
            }
            //Console.WriteLine(String.Join(",",Enum.GetValues(typeof(ModbusRegisterType))));

            Console.ReadKey();
        }
    }

    public enum ModbusRegisterType
    {
        [Description("离散输入")]
        DiscreteInputs = 1,

        [Description("线圈")]
        Coils = 0,

        [Description("输入寄存器")]
        InputRegisters = 3,

        [Description("保持寄存器")]
        HoldingRegisters = 4
    }
}
