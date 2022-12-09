using KpCommon.Attributes;

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
        Real,
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
