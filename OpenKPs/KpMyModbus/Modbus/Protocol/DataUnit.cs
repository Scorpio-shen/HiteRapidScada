using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KpMyModbus.Modbus.Protocol
{
    public abstract class DataUnit
    {
        /// <summary>
        /// 限制创建无参数对象的构造函数
        /// </summary>
        protected DataUnit()
            : this(ModbusRegisterType.DiscreteInputs)
        {
        }

        /// <summary>
        /// Конструктор
        /// </summary>
        public DataUnit(ModbusRegisterType  registerType)
        {
            Name = "";
            ModbusRegisterType = registerType;
            Address = 0;

            FuncCode = 0;
            RespByteCnt = 0;
        }

        public string Name { get; set; }

        public ModbusRegisterType  ModbusRegisterType { get; set; }

        public ushort Address { get; set; }

        public byte FuncCode { get; protected set; }
        public byte ExcFuncCode { get; protected set; }

        /// <summary>
        /// Получить длину ADU ответа на запрос
        /// </summary>
        public int RespAduLen { get; protected set; }
        public int RequestByteCount
        {
            get => GetRequestByteLength();
        }


        /// <summary>
        /// Получить количество байт, которое указывается в ответе
        /// </summary>
        public byte RespByteCnt { get; protected set; }

        /// <summary>
        /// Gets the maximum number of elements.
        /// </summary>
        public int MaxElemCnt
        {
            get
            {
                return GetMaxElemCnt(ModbusRegisterType);
            }
        }

        /// <summary>
        /// Gets the default element type.
        /// </summary>
        public TagDataType DefElemType
        {
            get
            {
                return GetDefElemType(ModbusRegisterType);
            }
        }

        /// <summary>
        /// Получить признак, что для блока данных или его элементов разрешено использование типов
        /// </summary>
        public virtual bool TagDataTypeEnabled
        {
            get
            {
                return ModbusRegisterType == ModbusRegisterType.InputRegisters || ModbusRegisterType == ModbusRegisterType.HoldingRegisters;
            }
        }

        public virtual bool ByteOrderEnabled
        {
            get
            {
                return ModbusRegisterType == ModbusRegisterType.InputRegisters || ModbusRegisterType == ModbusRegisterType.HoldingRegisters;
            }
        }

        /// <summary>
        /// Gets the maximum number of elements depending on the data table type.
        /// </summary>
        public virtual int GetMaxElemCnt(ModbusRegisterType registerType)
        {
            return registerType == ModbusRegisterType.DiscreteInputs || registerType == ModbusRegisterType.Coils ? 2000 : 125;
        }

        /// <summary>
        /// Gets the element type depending on the data table type.
        /// </summary>
        public virtual TagDataType GetDefElemType(ModbusRegisterType tableType)
        {
            return tableType == ModbusRegisterType.DiscreteInputs || tableType == ModbusRegisterType.Coils ?
                TagDataType.Bool : TagDataType.UShort;
        }
        /// <summary>
        /// 获取请求数据字节长度
        /// </summary>
        /// <returns></returns>
        protected abstract int GetRequestByteLength();
        /// <summary>
        /// 响应数据解析赋值
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <param name="errMsg"></param>
        /// <returns></returns>
        public virtual bool DecodeResponseContent(byte[] buffer,out string errMsg)
        {
            errMsg = string.Empty;
            bool result = false;
            return result;
        }
    }
}
