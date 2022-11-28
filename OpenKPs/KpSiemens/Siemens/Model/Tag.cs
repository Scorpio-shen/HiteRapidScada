using Scada.KPModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KpSiemens.Siemens.Model
{
    /// <summary>
    /// 通讯点
    /// </summary>
    public class Tag: DataUnit
    {
        #region 构造函数
        public Tag()
        {

        }

        public static Tag CreateNewTag(int tagID = default, string tagname = "", DataTypeEnum dataType = DataTypeEnum.Byte, MemoryTypeEnum memoryType = MemoryTypeEnum.DB, string address = "", byte canwrite = 1,int length = default)
        {
            return new Tag()
            {
                TagID = tagID,
                Name = tagname,
                DataType = dataType,
                Address = address,
                CanWrite = canwrite,
                MemoryType = memoryType,
                Length = length
            };
        }
        #endregion

        #region 属性
        private DataTypeEnum dataType;
        /// <summary>
        /// 数据类型
        /// </summary>

        public DataTypeEnum DataType
        {
            get => dataType;
            set
            {
                dataType = value;
                OnPropertyChanged(nameof(DataType));
            }
        }

        [DisplayName("数据类型")]
        public override string DataTypeDesc
        {
            get => DataType.ToString();
            set
            {
                if (Enum.TryParse(value, out DataTypeEnum valueEnum))
                    DataType = valueEnum;

            }
        }


        private bool canwritebool;
        public bool CanWriteBool
        {
            get => canwritebool;
            set
            {
                canwritebool = value;
                OnPropertyChanged(nameof(CanWriteBool));
            }
        }

        public dynamic Data { get; set; }

        public MemoryTypeEnum MemoryType { get; set; }
        #endregion

        #region 覆写基类抽象方法或者虚方法
        /// <summary>
        /// 写数据赋值
        /// </summary>
        /// <param name="cmdVal"></param>
        public override void SetCmdData(double cmdVal)
        {
            switch (DataType)
            {
                case DataTypeEnum.Bool:
                    Data = cmdVal > 0 ? true : false;
                    break;
                case DataTypeEnum.Byte:
                    Data = (byte)cmdVal;
                    break;
                case DataTypeEnum.Word:
                    Data = (ushort)cmdVal;
                    break;
                case DataTypeEnum.DWord:
                    Data = (uint)cmdVal;
                    break;
                case DataTypeEnum.Int:
                    Data = (short)cmdVal;
                    break;
                case DataTypeEnum.DInt:
                    Data = (int)cmdVal;
                    break;
                case DataTypeEnum.Real:
                    Data = (float)cmdVal;
                    break;
                case DataTypeEnum.Char:
                    Data = (char)cmdVal;
                    break;
                    //case DataTypeEnum.String:

                    //    break;
                    //case DataTypeEnum.Short:
                    //    Data = (short)cmdVal;
                    //    break;
                    //case DataTypeEnum.UShort:
                    //    Data = (ushort)cmdVal;
                    //    break;
                    //case DataTypeEnum.Int:
                    //    Data = (int)cmdVal;
                    //    break;
                    //case DataTypeEnum.UInt:
                    //    Data = (uint)cmdVal;
                    //    break;
                    //    case DataTypeEnum.Long:
                    //    Data = (long)cmdVal;
                    //    break ;
                    //    case DataTypeEnum.ULong:
                    //    Data = (ulong)cmdVal;
                    //    break;
                    //case DataTypeEnum.Float:
                    //    Data = (float)cmdVal;
                    //    break;
                    //case DataTypeEnum.Double:
                    //    Data = cmdVal;
                    //    break;
            }
        }
        #endregion

    }
}
