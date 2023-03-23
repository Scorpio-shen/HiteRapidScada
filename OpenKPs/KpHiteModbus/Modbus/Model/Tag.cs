using KpCommon.Attributes;
using KpCommon.Model;
using System;
using System.ComponentModel;

namespace KpHiteModbus.Modbus.Model
{
    /// <summary>
    /// 通讯点
    /// </summary>
    public class Tag : DataUnit
    {
        public Tag()
        {

        }


        public static Tag CreateNewTag(int tagID = default, string tagname = "", DataTypeEnum dataType = DataTypeEnum.Byte, RegisterTypeEnum registerType = RegisterTypeEnum.HoldingRegisters, string address = "", bool canwrite = false,int length = default)
        {
            return new Tag()
            {
                TagID = tagID,
                Name = tagname,
                DataType = dataType,
                Address = address,
                CanWrite = (byte)(canwrite ? 1 : 0),
                RegisterType = registerType,
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
                //if (Enum.TryParse(value, out DataTypeEnum valueEnum))
                //    DataType = valueEnum;
                foreach (DataTypeEnum dataType in Enum.GetValues(typeof(DataTypeEnum)))
                {
                    if (value.Equals(dataType.ToString(), StringComparison.OrdinalIgnoreCase))
                    {
                        DataType = dataType;
                        break;
                    }
                }
            }
        }


        public dynamic Data { get; set; }

        public RegisterTypeEnum RegisterType { get; set; }

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
                case DataTypeEnum.UShort:
                    Data = (ushort)cmdVal;
                    break;
                case DataTypeEnum.Short:
                    Data = (short)cmdVal;
                    break;
                case DataTypeEnum.UInt:
                    Data = (uint)cmdVal;
                    break;
                case DataTypeEnum.Int:
                    Data = (int)cmdVal;
                    break;
                case DataTypeEnum.ULong:
                    Data = (ulong)cmdVal;
                    break;
                case DataTypeEnum.Long:
                    Data = (long)cmdVal;
                    break;
                case DataTypeEnum.Float:
                    Data = (float)cmdVal;
                    break;
                case DataTypeEnum.Double:
                    Data = cmdVal;
                    break;
                case DataTypeEnum.Bool:
                    Data = cmdVal > 0 ? true : false;
                    break;
                default:
                    Data = (double)cmdVal;
                    break;
            }
        }
        #endregion
    }
}
