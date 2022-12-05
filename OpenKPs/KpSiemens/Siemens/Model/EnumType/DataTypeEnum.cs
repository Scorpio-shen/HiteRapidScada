using KpCommon.Attributes;

namespace KpSiemens.Siemens.Model
{
    /// <summary>
    /// 定义通讯点数据类型
    /// </summary>
    public enum DataTypeEnum
    {
        /// <summary>
        /// String类型（占据字节长度2+字符串长度）第一个字节为该字符串总长度，第二个字节为当前存储的有效字符数量。
        /// </summary>
        [DataTypeByteCount(2)]
        String,
        [DataTypeByteCount(1)]
        Bool,
        /// <summary>
        /// 单个字节
        /// </summary>
        [DataTypeByteCount(1)]
        Byte,
        /// <summary>
        /// 对应ushort类型
        /// </summary>
        [DataTypeByteCount(2)]
        Word,
        /// <summary>
        /// 对应uint
        /// </summary>
        [DataTypeByteCount(4)]
        DWord,
        /// <summary>
        /// 对应short
        /// </summary>
        [DataTypeByteCount(2)]
        Int,
        /// <summary>
        /// 对应Int
        /// </summary>
        [DataTypeByteCount(4)]
        DInt,
        /// <summary>
        /// 对应float
        /// </summary>
        [DataTypeByteCount(4)]
        Real,

        [DataTypeByteCount(1)]
        Char
    }

    
}
