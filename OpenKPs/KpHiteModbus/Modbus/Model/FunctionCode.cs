using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KpHiteModbus.Modbus.Model
{
    public class FunctionCodes
    {
        /// <summary>
        /// 离散输入
        /// </summary>
        public const byte ReadDiscreteInputs = 0x02;

        /// <summary>
        /// 线圈
        /// </summary>
        public const byte ReadCoils = 0x01;

        /// <summary>
        /// 输入寄存器
        /// </summary>
        public const byte ReadInputRegisters = 0x04;

        /// <summary>
        /// 保持寄存器
        /// </summary>
        public const byte ReadHoldingRegisters = 0x03;

        /// <summary>
        /// 写单个线圈
        /// </summary>
        public const byte WriteSingleCoil = 0x05;

        /// <summary>
        /// 写单个寄存器
        /// </summary>
        public const byte WriteSingleRegister = 0x06;

        /// <summary>
        /// 写多个线圈
        /// </summary>
        public const byte WriteMultipleCoils = 0x0F;

        /// <summary>
        /// 写多个寄存器
        /// </summary>
        public const byte WriteMultipleRegisters = 0x10;
    }
}
