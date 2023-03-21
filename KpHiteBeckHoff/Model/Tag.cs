using KpHiteBeckHoff.Extend;
using KpHiteBeckHoff.Model.EnumType;
using KpCommon.Attributes;
using KpCommon.Model;
using Scada;
using System;
using System.Collections;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Collections.Generic;

namespace KpHiteBeckHoff.Model
{
    /// <summary>
    /// 通讯点
    /// </summary>
    public class Tag : DataUnit
    {
        public Tag()
        {

        }


        public static Tag CreateNewTag(int tagID = default, 
            string tagname = "", 
            DataTypeEnum dataType = DataTypeEnum.UShort, 
            string address = "", 
            bool canwrite = false,
            int length = default,
            int index = 0
            /*Tag parent = null*/)
        {
            return new Tag()
            {
                TagID = tagID,
                Name = tagname,
                DataType = dataType,
                Address = address,
                CanWrite = (byte)(canwrite ? 1 : 0),
                Length = length,
                Index= index,
                //ParentTag = parent  
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

        /// <summary>
        /// 偏移量
        /// </summary>
        public int Index { get; set; }
        /// <summary>
        /// 是否是数组类型
        /// </summary>
        public bool IsArray
        {
            get
            {
                if (DataType == DataTypeEnum.String)
                    return false;
                else
                {
                    return Length > 0;
                }
            }
        }

        /// <summary>
        /// 当类型为数组类型时，指向的数组Tag
        /// </summary>
        //public Tag ParentTag { get; set; }
        /// <summary>
        /// 数组类型的子Tag
        /// </summary>
        //public List<Tag> ChildRenTags { get; set; }

        /// <summary>
        /// 读取数据
        /// </summary>
        public byte[] ReadData { get; set; }

        /// <summary>
        /// 对应通道标识号(数组类型时为集合，非数组类型则单个数据)
        /// </summary>
        public List<int> Signals { get; set; }
        #endregion


        #region 覆写或实现基类方法

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
                case DataTypeEnum.SByte:
                    Data = (sbyte)cmdVal;
                    break;
                case DataTypeEnum.Short:
                    Data = (short)cmdVal;
                    break;
                case DataTypeEnum.Int:
                    Data = (int)cmdVal;
                    break;
                case DataTypeEnum.Long:
                    Data = (long)cmdVal;
                    break;
                case DataTypeEnum.UShort:
                    Data = (ushort)cmdVal;
                    break;
                case DataTypeEnum.UInt:
                    Data = (uint)cmdVal;
                    break;
                case DataTypeEnum.ULong:
                    Data = (ulong)cmdVal;
                    break;
                case DataTypeEnum.Float:
                    Data = (float)cmdVal;
                    break;
                case DataTypeEnum.Double:
                    Data = cmdVal;
                    break;
                default:
                    Data = cmdVal;
                    break;
            }
        }
        #endregion

        #region 获取Tag值
        public double? GetTagVal()
        {
            double? result = null;
            try
            {
                result = GetTagVal(ReadData);
            }
            catch 
            {
                result = null;
            }

            return result;  
        }

        private double? GetTagVal(byte[] buffer)
        {
            double? result = null;
            try
            {
                if(buffer == null || buffer.Length == 0)
                    return result;
                switch(DataType)
                {
                    case DataTypeEnum.Bool:
                        result = buffer[0] > 0 ? 1d : 0d;
                        break;
                    case DataTypeEnum.Byte:
                        result = buffer[0];
                        break;
                    case DataTypeEnum.SByte:
                        result = buffer[0];
                        break;
                    case DataTypeEnum.Short:
                        result = BitConverter.ToInt16(buffer, 0);
                        break;
                    case DataTypeEnum.Int:
                        result = BitConverter.ToInt32(buffer, 0);   
                        break;
                    case DataTypeEnum.Long:
                        result = BitConverter.ToInt64(buffer,0);
                        break;
                    case DataTypeEnum.UShort:
                        result = BitConverter.ToUInt16(buffer, 0);
                        break;
                    case DataTypeEnum.UInt:
                        result = BitConverter.ToUInt32(buffer, 0);
                        break;
                    case DataTypeEnum.ULong:
                        result = BitConverter.ToUInt64(buffer, 0);  
                        break;
                    case DataTypeEnum.Float:
                        result = BitConverter.ToSingle(buffer, 0);
                        break;
                    case DataTypeEnum.Double:
                        result = BitConverter.ToDouble(buffer, 0);
                        break;
                    case DataTypeEnum.String:
                        //跳过头两个字节,获取int类型有效长度,在读取字符串
                        var length = BitConverter.ToInt32(buffer, 2);
                        var buf = buffer.Skip(2 + 4).Take(length).ToArray();
                        result = ScadaUtils.EncodeAscii(Encoding.ASCII.GetString(buf));
                        break;
                }
            }
            catch
            {
                result = null;
            }
            return result;
        }
        #endregion
    }
}
