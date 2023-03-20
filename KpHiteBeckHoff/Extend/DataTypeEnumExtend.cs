using KpHiteBeckHoff.Model.EnumType;
using KpCommon.Attributes;
using KpCommon.Extend;

namespace KpHiteBeckHoff.Extend
{
    public static class DataTypeEnumExtend
    {
        /// <summary>
        /// 获取数据类型枚举对应的字节长度
        /// </summary>
        /// <param name="dataTypeEnum"></param>
        /// <returns></returns>
        public static int GetByteCount(this DataTypeEnum dataTypeEnum)
        {
            var attr = dataTypeEnum.GetAttribute<DataTypeByteCountAttribute>();
            if (attr != null)
                return attr.ByteCount;
            else
                return default;
        }
    }
}
