using KpSiemens.Siemens.Extend;
using Scada;
using Scada.Extend;
using Scada.KPModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace KpSiemens.Siemens.Model
{
    public class SiemensTagGroup : GroupUnit<Tag>
    {
        public int MaxTagCount = ushort.MaxValue;

        #region 构造函数
        public SiemensTagGroup()
        {
            Tags = new List<Tag>();
        }

        public SiemensTagGroup(MemoryTypeEnum memoryTypeEnum)
        {
            Tags = new List<Tag>();
            MemoryType = memoryTypeEnum;
            Active = true;
            StartKpTagIndex = -1;
        }
        #endregion

        #region 属性
        /// <summary>
        /// 是否全部支持写入
        /// </summary>
        public bool AllCanWrite
        {
            get => Tags?.Count > 0 && Tags.All(t => t.CanWrite > 0);
        }

        private MemoryTypeEnum memoryType;
        /// <summary>
        /// 存储器类型
        /// </summary>
        public MemoryTypeEnum MemoryType
        {
            get => memoryType;
            set
            {
                memoryType = value;
                SetMemoryType(memoryType);
                OnPropertyChanged(nameof(MemoryType));
            }
        }
        /// <summary>
        /// 最大地址长度（限制配置点时防止超出最大地址长度）
        /// </summary>
        public override double MaxRequestByteLength { get; set; }
        #endregion

        #region 存储载入XML
        public override void LoadFromXml(XmlElement tagGroupElement)
        {
            if (tagGroupElement == null)
                throw new ArgumentNullException("TagGroupElement");

            Name = tagGroupElement.GetAttribute("Name");
            StartKpTagIndex = tagGroupElement.GetAttrAsInt("StartKpTagIndex");
            Active = tagGroupElement.GetAttrAsBool("Active", true);
            MemoryType = tagGroupElement.GetAttrAsEnum("MemoryType", MemoryTypeEnum.DB);
            DBNum = tagGroupElement.GetAttrAsInt("DBNum");
            MaxRequestByteLength = tagGroupElement.GetAttrAsDouble("MaxRequestByteLength");
            if (MaxRequestByteLength == 0)
                MaxRequestByteLength = TagGroupDefaultValues.MaxAddressLength;
            XmlNodeList tagNodes = tagGroupElement.SelectNodes("Tag");
            int maxTagCount = MaxTagCount;
            MemoryTypeEnum memoryType = MemoryType;

            foreach (XmlElement tagElem in tagNodes)
            {
                if (Tags.Count > maxTagCount)
                    break;

                var tagID = tagElem.GetAttrAsInt("TagID");
                var name = tagElem.GetAttribute("Name");
                var dataType = tagElem.GetAttrAsEnum("DataType", DataTypeEnum.Bool);
                var address = tagElem.GetAttrAsString("Address");
                var length = tagElem.GetAttrAsInt("Length");
                var canwrite = tagElem.GetAttrAsString("CanWrite").ToByte();
                var tag = Tag.CreateNewTag(tagID, name, dataType, memoryType, address, canwrite,length);
                Tags.Add(tag);
            }
        }

        public override void SaveToXml(XmlElement tagGroupElement)
        {
            if (tagGroupElement == null)
                throw new ArgumentNullException("TagGroupElement");

            tagGroupElement.SetAttribute("Active", Active);
            tagGroupElement.SetAttribute("MemoryType", MemoryType);
            tagGroupElement.SetAttribute("StartKpTagIndex", StartKpTagIndex);
            tagGroupElement.SetAttribute("Name", Name);
            tagGroupElement.SetAttribute("DBNum", DBNum);
            tagGroupElement.SetAttribute("MaxRequestByteLength", MaxRequestByteLength);
            foreach (Tag tag in Tags)
            {
                XmlElement tagElem = tagGroupElement.AppendElem("Tag");
                tagElem.SetAttribute("TagID", tag.TagID);
                tagElem.SetAttribute("Name", tag.Name);
                tagElem.SetAttribute("DataType", tag.DataType);
                tagElem.SetAttribute("Address", tag.Address);
                tagElem.SetAttribute("Length", tag.Length);
                tagElem.SetAttribute("CanWrite", tag.CanWrite);
            }
        }
        #endregion

        #region 设置Tag是否支持写入
        public void SetTagCanWrite(bool canwrite)
        {
            Tags.ForEach(t => t.CanWriteBool = canwrite);
        }
        #endregion

        #region 批量添加点集合
        /// <summary>
        /// 批量添加Tags集合到当前group对象Tags集合中，超出了最大请求地址限制,则恢复成原有集合
        /// </summary>
        /// <param name="addTags"></param>
        public override bool CheckAndAddTags(List<Tag> addTags,bool needClear = false)
        {
            //将原有对象先拷贝
            bool result = true;
            var tagsOld = new List<Tag>();
            tagsOld.AddRange(Tags.Select(t => t.Clone() as Tag));

            if (needClear)
                Tags.Clear();
            Tags.AddRange(addTags);
           RefreshTagAddress();

            var model = GetRequestModel();
            //验证是否超出最大地址限制
            if (model.Length > MaxRequestByteLength)
            {
                Tags.Clear();
                Tags.AddRange(tagsOld);
                result = false;
            }

            return result;
        }
        #endregion

        public double? GetTagVal(int index)
        {
            double? result = null;
            try
            {
                if (TagCount == 0 || index >= Tags.Count)
                    return result;
                var tag = Tags[index];
                if (tag == null)
                    return result;
                if (Data == null || Data.Length == 0)
                    return result;

                double address = tag.Address.ToDouble() - StartAddress;
                double dPart = address % 1; //小数部分
                int iPart = (int)address;   //整数部分

                if (tag.DataType == DataTypeEnum.Bool)
                {
                    if (Data.Length >= iPart + 1)
                    {
                        var bitArray = new BitArray(Data.Skip(iPart).Take(1).ToArray());
                        if (iPart < 8)
                        {
                            int bitIndex = (int)(dPart * 10);
                            result = bitArray.Get(bitIndex) ? 1.0 : 0.0;
                        }
                    }
                    return result;
                }
                var byteCount = tag.DataType.GetByteCount();
                if (Data.Length < iPart + byteCount)    //超出数据长度
                    return result;
                byte[] buf = Data.Skip(iPart).Take(byteCount).Reverse().ToArray();
                switch (tag.DataType)
                {
                    //case DataTypeEnum.UShort:
                    //    return BitConverter.ToUInt16(buf, 0);
                    //case DataTypeEnum.Short:
                    //    return BitConverter.ToInt16(buf, 0);
                    //case DataTypeEnum.UInt:
                    //    return BitConverter.ToUInt32(buf, 0);
                    //case DataTypeEnum.Int:
                    //    return BitConverter.ToInt32(buf, 0);
                    //case DataTypeEnum.ULong:
                    //    return BitConverter.ToUInt64(buf, 0);
                    //case DataTypeEnum.Long:
                    //    return BitConverter.ToInt64(buf, 0);
                    //case DataTypeEnum.Float:
                    //    return BitConverter.ToSingle(buf, 0);
                    //case DataTypeEnum.Double:
                    //    return BitConverter.ToDouble(buf, 0);
                    case DataTypeEnum.Byte:
                        return buf[0];
                    case DataTypeEnum.Word:
                        return BitConverter.ToUInt16(buf, 0);
                    case DataTypeEnum.DWord:
                        return BitConverter.ToUInt32(buf, 0);
                    case DataTypeEnum.Int:
                        return BitConverter.ToInt16(buf, 0);
                    case DataTypeEnum.DInt:
                        return BitConverter.ToInt32(buf, 0);
                    case DataTypeEnum.Real:
                        return BitConverter.ToSingle(buf, 0);
                    case DataTypeEnum.Char:
                        return BitConverter.ToChar(buf, 0);
                    case DataTypeEnum.String:
                        //取实际内容部分
                        try
                        {
                            buf = Data.Skip(iPart + byteCount).Take(tag.Length).ToArray();
                            var str = Encoding.ASCII.GetString(buf).TrimEnd('\0');
                            return ScadaUtils.EncodeAscii(str);
                        }
                        catch
                        {
                            return null;
                        }
                }

                return result;
            }
            catch
            {
                return null;
            }
        }

        private void SetMemoryType(MemoryTypeEnum memoryType) => Tags.ForEach(t => t.MemoryType = memoryType);

        public TagGroupRequestModel GetRequestModel()
        {
            var model = new TagGroupRequestModel();
            if(MemoryType == MemoryTypeEnum.DB)
                model.Address =$"{MemoryType}{DBNum}.{StartAddress}";
            else
                model.Address = $"{MemoryType}{StartAddress}";
            if (TagCount == 0)
                model.Length = 0;
            else
            {
                var tag = Tags.Last();
                if (double.TryParse(tag.Address, out double address))
                {
                    var length = address + tag.DataType.GetByteCount() - StartAddress;
                    double dPart = length % 1; //小数部分
                    int iPart = (int)length;   //整数部分

                    model.Length += (ushort)iPart;

                    if (dPart > 0)
                        model.Length += 1;
                    if (tag.Length > 0)
                        model.Length += (ushort)tag.Length;
                }
                else
                    model.Length = 0;
            }

            return model;
        }

    }
    /// <summary>
    /// 西门子数据请求Model
    /// </summary>
    public class TagGroupRequestModel : RequestModel
    {
    }
}
