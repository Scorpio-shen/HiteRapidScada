using KpCommon.Attributes;

namespace KpAllenBrandly.Model.EnumType
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
        /// <summary>
        /// byte 0～255
        /// </summary>
        [DataTypeByteCount(1)]
        Byte,
        /// <summary>
        /// -128 ～ 127
        /// </summary>
        [DataTypeByteCount(1)]
        SByte,
        /// <summary>
        /// 整数（-32768~32767）
        /// </summary>
        [DataTypeByteCount(2)]
        Short,
        /// <summary>
        /// 双整数(-2147483648~2147483647)
        /// </summary>
        [DataTypeByteCount(4)]
        Int,
        /// <summary>
        /// 长整数（有符号）
        /// </summary>
        [DataTypeByteCount(8)]
        Long,

        /// <summary>
        /// 无符号整数
        /// </summary>
        [DataTypeByteCount(2)]
        UShort,
        /// <summary>
        /// 无符号双整数
        /// </summary>
        [DataTypeByteCount(4)]
        UInt,
        /// <summary>
        /// 无符号长整数
        /// </summary>
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
        
    }

}
