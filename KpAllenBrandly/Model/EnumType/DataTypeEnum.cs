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
        /// 整数（-32768~32767）
        /// </summary>
        [DataTypeByteCount(2)]
        Int,
        /// <summary>
        /// 双整数(-2147483648~2147483647)
        /// </summary>
        [DataTypeByteCount(4)]
        DInt,
        /// <summary>
        /// 长整数（有符号）
        /// </summary>
        [DataTypeByteCount(8)]
        LInt,

        /// <summary>
        /// 无符号整数
        /// </summary>
        [DataTypeByteCount(2)]
        UInt,
        /// <summary>
        /// 无符号双整数
        /// </summary>
        [DataTypeByteCount(4)]
        UDInt,
        /// <summary>
        /// 无符号长整数
        /// </summary>
        [DataTypeByteCount(8)]
        ULInt,
        ///// <summary>
        ///// 4个字节
        ///// </summary>
        [DataTypeByteCount(4)]
        Real,
        ///// <summary>
        ///// 8个字节
        ///// </summary>
        [DataTypeByteCount(8)]
        LReal,
        /// <summary>
        /// String类型
        /// </summary>
        [DataTypeByteCount(0)]
        String,
        /// <summary>
        /// 无符号
        /// </summary>
        [DataTypeByteCount(2)]
        Word,
        /// <summary>
        /// 无符号
        /// </summary>
        [DataTypeByteCount(4)]
        DWord,
        /// <summary>
        /// 无符号
        /// </summary>
        [DataTypeByteCount(8)]
        LWord,
        
    }

}
