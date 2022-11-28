using Scada.KPModel.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KpHiteModbus.Modbus.Model
{
    /// <summary>
    /// 定义通讯点数据类型
    /// </summary>
    public enum DataTypeEnum
    {
        /// <summary>
        /// 一个Bit
        /// </summary>
        [DataTypeByteCount(1)]
        Bool,
        ///// <summary>
        ///// 单个字节
        ///// </summary>
        [DataTypeByteCount(1)]
        Byte,
        /// <summary>
        /// 2个字节
        /// </summary>
        [DataTypeByteCount(2)]
        Short,
        ///// <summary>
        ///// 2个字节
        ///// </summary>
        [DataTypeByteCount(2)]
        UShort,
        ///// <summary>
        ///// 4个字节
        ///// </summary>
        [DataTypeByteCount(4)]
        Int,
        ///// <summary>
        ///// 4个字节
        ///// </summary>
        [DataTypeByteCount(4)]
        UInt,
        ///// <summary>
        ///// 8个字节
        ///// </summary>
        [DataTypeByteCount(8)]
        Long,
        ///// <summary>
        ///// 8个字节
        ///// </summary>
        [DataTypeByteCount(8)]
        ULong,
        ///// <summary>
        ///// 4个字节
        ///// </summary>
        [DataTypeByteCount(4)]
        Float,
        ///// <summary>
        ///// 8个字节
        ///// </summary>
        [DataTypeByteCount(8)]
        Double,
        /// <summary>
        /// String类型
        /// </summary>
        [DataTypeByteCount(0)]
        String,
        //[DataTypeByteCount(1)]
        //Bool,
        ///// <summary>
        ///// 单个字节
        ///// </summary>
        //[DataTypeByteCount(1)]
        //Byte,
        ///// <summary>
        ///// 对应ushort类型
        ///// </summary>
        //[DataTypeByteCount(2)]
        //Word,
        ///// <summary>
        ///// 对应uint
        ///// </summary>
        //[DataTypeByteCount(4)]
        //DWord,
        ///// <summary>
        ///// 对应short
        ///// </summary>
        //[DataTypeByteCount(2)]
        //Int,
        ///// <summary>
        ///// 对应Int
        ///// </summary>
        //[DataTypeByteCount(4)]
        //DInt,
        ///// <summary>
        ///// 对应float
        ///// </summary>
        //[DataTypeByteCount(4)]
        //Real,

        //[DataTypeByteCount(1)]
        //Char
    }

}
