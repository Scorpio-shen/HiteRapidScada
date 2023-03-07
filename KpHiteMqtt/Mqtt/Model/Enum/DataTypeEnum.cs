using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KpHiteMqtt.Mqtt.Model.Enum
{
    /// <summary>
    /// 数据类型枚举
    /// </summary>
    public enum DataTypeEnum
    {
        [Description("int32(整数型)")]
        Int32,
        [Description("float(单精度浮点型)")]
        Float,
        [Description("double(双精度浮点型)")]
        Double,
        [Description("bool(布尔型)")]
        Bool,
        [Description("text(字符串)")]
        Text,
        [Description("array(数组)")]
        Array,
        [Description("struct(结构体)")]
        Struct
    }

    public enum StructDataTypeEnum
    {
        [Description("int32(整数型)")]
        Int32,
        [Description("float(单精度浮点型)")]
        Float,
        [Description("double(双精度浮点型)")]
        Double,
        [Description("bool(布尔型)")]
        Bool,
    }
}
