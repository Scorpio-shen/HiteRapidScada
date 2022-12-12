using KpCommon.Attributes;
using KpCommon.Model;
using KpOmron.Model.EnumType;
using System;
using System.ComponentModel;

namespace KpOmron.Model
{
    /// <summary>
    /// 通讯点
    /// </summary>
    public class Tag : DataUnit
    {
        public Tag()
        {

        }


        public static Tag CreateNewTag(int tagID = default, string tagname = "", DataTypeEnum dataType = DataTypeEnum.UInt, MemoryTypeEnum memoryType = MemoryTypeEnum.D, string address = "", bool canwrite = false,int length = default)
        {
            return new Tag()
            {
                TagID = tagID,
                Name = tagname,
                DataType = dataType,
                Address = address,
                CanWrite = (byte)(canwrite ? 1 : 0),
                MemoryType = memoryType,
                Length = length
            };
        }

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
        [ExcelHeaderSort(2)]
        public override string DataTypeDesc
        {
            get => DataType.ToString();
            set
            {
                if (Enum.TryParse(value, out DataTypeEnum valueEnum))
                    DataType = valueEnum;

            }
        }


        public dynamic Data { get; set; }

        public MemoryTypeEnum MemoryType { get; set; }

        public bool CanWriteBool
        {
            get => CanWrite > 0;
            set
            {
                CanWrite = (byte)(value ? 1 : 0);
                OnPropertyChanged(nameof(CanWriteBool));
            }
        }
        #endregion


        #region 覆写或实现基类方法

        public override void SetCmdData(double cmdVal)
        {
            switch (DataType)
            {
                case DataTypeEnum.Bool:
                    Data = cmdVal > 0 ? true : false;
                    break;
                case DataTypeEnum.Int: 
                    Data = (short)cmdVal; 
                    break;
                    case DataTypeEnum.DInt: 
                    Data = (int)cmdVal; 
                    break;
                case DataTypeEnum.LInt:
                    Data = (long)cmdVal;
                    break;
                case DataTypeEnum.UInt:
                case DataTypeEnum.Word:
                    Data = (ushort)cmdVal;
                    break;
                case DataTypeEnum.UDInt:
                case DataTypeEnum.DWord:
                    Data = (uint)cmdVal;
                    break;
                case DataTypeEnum.ULInt:
                case DataTypeEnum.LWord:
                    Data = (ulong)cmdVal;
                    break;
                case DataTypeEnum.Real:
                    Data = (float)cmdVal;
                    break;
                case DataTypeEnum.LReal:
                    Data = cmdVal;
                    break;
                default:
                    Data = cmdVal;
                    break;
            }
        }
        #endregion
    }
}
