using KpCommon.Attributes;
using KpCommon.Extend;
using KpHiteModbus.Modbus.Model;
using System.IO.Ports;

namespace KpHiteModbus.Modbus.Extend
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

        public static string GetDescription(this Parity parity)
        {
            string desc = "无";
            switch (parity)
            {
                case Parity.Even:
                    desc = "偶";
                    break;
                case Parity.Odd:
                    desc = "奇";
                    break;
            }

            return desc;    
        }

        public static string GetDescription(this StopBits stopBits)
        {
            string desc = "1";
            switch (stopBits)
            {
                case StopBits.One:
                    break;
                    case StopBits.Two:
                    desc = "2";
                    break;
                case StopBits.OnePointFive:
                    desc = "1.5";
                    break;
            }

            return desc;
        }
    }
}
