using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KpMyModbus.Modbus.Protocol
{
    /// <summary>
    /// Modbus通讯点
    /// </summary>
    public class Tag
    {
        //public ModbusRegisterType RegisterType { get; set; }
        public Tag()
        {

        }
        public Tag(string name,TagDataType tagDataType)
        {
            Name = name;
            DataType = tagDataType;
        }
        public string Name { get; set; }

        public TagDataType DataType { get; set; }

        public byte[] Data { get; set; }    

        public int ByteCount { get => GetByteCount(); }

        private int GetByteCount()
        {
            int byteCount = default;
            switch (DataType)
            {
                case TagDataType.Bool: byteCount = 1; break;
                case TagDataType.UShort:
                case TagDataType.Short:
                    byteCount = 2; 
                    break;
                case TagDataType.Int:
                case TagDataType.UInt:
                case TagDataType.Float:
                    byteCount = 4;
                    break;
                    case TagDataType.ULong:
                    case TagDataType.Double:
                case TagDataType.Long:
                    byteCount = 8;
                    break;
            }

            return byteCount;
        }
    }
}
