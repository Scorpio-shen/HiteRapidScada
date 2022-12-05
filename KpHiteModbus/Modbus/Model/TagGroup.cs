using KpHiteModbus.Modbus.Extend;
using Scada;
using Scada.Data.Tables;
using Scada.Extend;
using Scada.KPModel;
using Scada.KPModel.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace KpHiteModbus.Modbus.Model
{
    public class ModbusTagGroup : GroupUnit<Tag>
    {
        public int MaxTagCount = ushort.MaxValue;

        #region 构造函数
        public ModbusTagGroup()
        {
            Tags = new List<Tag>();
        }

        public ModbusTagGroup(RegisterTypeEnum registerType)
        {
            Tags = new List<Tag>();
            RegisterType = registerType;
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

        private RegisterTypeEnum registerType;
        /// <summary>
        /// 寄存器类型
        /// </summary>
        public RegisterTypeEnum RegisterType
        {
            get=>registerType;
            set
            {
                var oldType = registerType;
                registerType = value;
                SetRegisterType(registerType,oldType);
                OnPropertyChanged(nameof(RegisterType));
            }
        }
        private void SetRegisterType(RegisterTypeEnum registerType,RegisterTypeEnum oldType)
        {
            if(TagCount > 0)
            {
                var newType = registerType;
                bool setBool = false;
                bool setUShort = false;
                bool setReadWritefalse = false;
                if(oldType == RegisterTypeEnum.Coils || oldType == RegisterTypeEnum.DiscretesInputs)
                {
                    if(newType == RegisterTypeEnum.InputRegisters || newType == RegisterTypeEnum.HoldingRegisters)
                    {
                        //切换寄存器类型,数据类型变更ushort
                        setUShort = true;
                    }
                }
                else
                {
                    if(newType == RegisterTypeEnum.Coils || newType == RegisterTypeEnum.DiscretesInputs)
                    {
                        //切换寄存器类型,数据类型变更bool
                        setBool = true;
                    }
                }
                //不允许写入
                if(newType == RegisterTypeEnum.DiscretesInputs || newType == RegisterTypeEnum.InputRegisters)
                    setReadWritefalse = true;


                Tags.ForEach(t =>
                {
                    if (setBool)
                        t.DataType = DataTypeEnum.Bool;
                    else if (setUShort)
                        t.DataType = DataTypeEnum.UShort;
                    if (setReadWritefalse)
                        t.CanWriteBool = false;
                    t.RegisterType = registerType;
                });
            }
        }
        private double maxrequestbytelength;

        /// <summary>
        /// 限制一组Tag最大请求数据字节长度
        /// </summary>
        public override double MaxRequestByteLength
        {
            get
            {
                if(maxrequestbytelength <= 0)
                    maxrequestbytelength = TagGroupDefaultValues.MaxAddressLength;
                return maxrequestbytelength;
            }
            set => maxrequestbytelength = value;
        }

        //private int requestlength;
        ///// <summary>
        ///// 请求寄存器个数
        ///// </summary>
        //public int RequestLength
        //{
        //    get => requestlength;
        //    set=> requestlength = value;
        //}
        #endregion

        #region 载入存储Xml
        public override void LoadFromXml(XmlElement tagElem)
        {
            if (tagElem == null)
                throw new ArgumentNullException("TagGroupElement");

            Name = tagElem.GetAttribute("Name");
            StartKpTagIndex = tagElem.GetAttrAsInt("StartKpTagIndex");
            Active = tagElem.GetAttrAsBool("Active",true);
            RegisterType = tagElem.GetAttrAsEnum("RegisterType", RegisterTypeEnum.HoldingRegisters);
            MaxRequestByteLength = tagElem.GetAttrAsDouble("MaxRequestByteLength");
            //RequestLength = tagElem.GetAttrAsInt("RequestLength");
            if(MaxRequestByteLength == 0)
                MaxRequestByteLength = TagGroupDefaultValues.MaxAddressLength;

            XmlNodeList nodes = tagElem.SelectNodes("Tag");
            int maxTagCount = MaxTagCount;
            RegisterTypeEnum type = RegisterType;
            foreach(XmlElement element in nodes)
            {
                if(TagCount > MaxTagCount)
                    break;

                var tagID = element.GetAttrAsInt("TagID");
                var name = element.GetAttribute("Name");
                var dataType = element.GetAttrAsEnum("DataType", DataTypeEnum.Bool);
                var address = element.GetAttrAsString("Address");
                var length = element.GetAttrAsInt("Length");
                var canwrite = element.GetAttrAsString("CanWrite").ToByte();
                var tag = Tag.CreateNewTag(tagID:tagID, tagname:name,dataType:dataType,registerType:RegisterType,address:address,canwrite: canwrite > 0, length:length);
                Tags.Add(tag);
            }

        }

        public override void SaveToXml(XmlElement tagGroupElement)
        {
            if(tagGroupElement == null)
                throw new ArgumentNullException("TagGroupElement");

            tagGroupElement.SetAttribute("Name", Name);
            tagGroupElement.SetAttribute("StartKpTagIndex", StartKpTagIndex);
            tagGroupElement.SetAttribute("Active", Active);
            tagGroupElement.SetAttribute("RegisterType", RegisterType);
            tagGroupElement.SetAttribute("MaxRequestByteLength", MaxRequestByteLength);
            //tagGroupElement.SetAttribute("RequestLength", RequestLength);
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

        #region 批量添加点集合
        public override bool CheckAndAddTags(List<Tag> addTags, out string errorMsg, bool needClear = false)
        {
            errorMsg= string.Empty;
            //将原有对象先拷贝
            bool result = true;
            var tagsOld = new List<Tag>();
            tagsOld.AddRange(Tags.Select(t => t.Clone() as Tag));

            if (needClear)
                Tags.Clear();
            Tags.AddRange(addTags);
            SortTags();
            //验证是否超出最大点数限制
            if ((TagCount + StartKpTagIndex > DefineMaxValues.MaxTagCount))
            {
                Tags.Clear();
                Tags.AddRange(tagsOld);
                errorMsg = "超出点数限制!";
                result = false;
            }
            var model = GetRequestModel();
            int byteCount = default;
            //验证是否超出最大寄存器个数限制
            if(RegisterType == RegisterTypeEnum.Coils || RegisterType == RegisterTypeEnum.DiscretesInputs)
            {
                //验证数据类型是否符合规范
                if(addTags.Any(t=>t.DataType != DataTypeEnum.Bool))
                {
                    errorMsg = "存在不符合规范的数据类型!";
                    return false;
                }
                //一个寄存器代表一个bit
                byteCount = model.Length / 8;
                if (model.Length % 8 > 0)
                    byteCount += 1;
            }
            else
            {
                //一个寄存器表示两个byte
                byteCount = model.Length * 2;
            }
            if (byteCount > MaxRequestByteLength)
            {
                Tags.Clear();
                Tags.AddRange(tagsOld);
                errorMsg = "数据超出范围!";
                result = false;
            }
            RefreshTagIndex();
            return result;
        }

        public TagGroupRequestModel GetRequestModel()
        {
            var model = new TagGroupRequestModel();
            var functionCode = RegisterType.GetFunctionCode();
            model.Address = $"x={functionCode};{StartAddress}";
            model.StartAddress = StartAddress;
            if (TagCount == 0)
                model.Length = 0;
            else
            {
                var tagLast = Tags.Last();
                if (double.TryParse(tagLast.Address, out double address))
                {

                    if (RegisterType == RegisterTypeEnum.Coils || RegisterType == RegisterTypeEnum.DiscretesInputs)
                    {
                        var length = (ushort)(address - StartAddress);  //获取首尾长度间隔
                        model.Length+= length;
                        model.Length += 1;
                    }
                    else
                    {
                        //根据最后一个Tag的数据类型,占据的字节数
                        var lastByteCount = tagLast.DataType.GetByteCount();
                        if (tagLast.DataType == DataTypeEnum.String)
                            lastByteCount += tagLast.Length;

                        //获取总请求字节长度
                        var length = address - StartAddress;
                        //除以2看需要几个寄存器
                        var regCount = (ushort)(lastByteCount / 2 + lastByteCount % 2);
                        model.Length += (ushort)length;
                        model.Length += regCount;

                    }
                }
                else
                    model.Length = 0;
            }

            return model;
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

                int addressOffSet = (int)(tag.Address.ToDouble() - StartAddress); //地址偏移
                if (RegisterType == RegisterTypeEnum.Coils || RegisterType == RegisterTypeEnum.DiscretesInputs)
                {
                    if(tag.DataType != DataTypeEnum.Bool)
                        return result;

                    //除以8
                    var skipByte = addressOffSet / 8;
                    var selectBit = addressOffSet % 8;
                    if (skipByte + 1 > Data.Length)
                        return result;
                    var bitArray = new BitArray(Data.Skip(skipByte).Take(1).ToArray());
                    result = bitArray[selectBit] ? 1d : 0d;
                    return result;
                }
                else
                {
                    var skipByte = addressOffSet * 2;
                    var byteCount = tag.DataType.GetByteCount();
                    if(skipByte + byteCount > Data.Length) 
                        return result;
                    byte[] buf = Data.Skip(skipByte).Take(byteCount).Reverse().ToArray();
                    switch (tag.DataType)
                    {
                        case DataTypeEnum.Byte:
                            return buf[0];
                        case DataTypeEnum.UShort:
                            return BitConverter.ToUInt16(buf, 0);
                        case DataTypeEnum.Short:
                            return BitConverter.ToInt16(buf, 0);
                        case DataTypeEnum.UInt:
                            return BitConverter.ToUInt32(buf, 0);
                        case DataTypeEnum.Int:
                            return BitConverter.ToInt32(buf, 0);
                        case DataTypeEnum.ULong:
                            return BitConverter.ToUInt64(buf, 0);
                        case DataTypeEnum.Long:
                            return BitConverter.ToInt64(buf, 0);
                        case DataTypeEnum.Float:
                            return BitConverter.ToSingle(buf, 0);
                        case DataTypeEnum.Double:
                            return BitConverter.ToDouble(buf, 0);
                        case DataTypeEnum.String:
                            //取实际内容部分
                            try
                            {
                                if(skipByte + byteCount + tag.Length > Data.Length) 
                                    return result;
                                buf = Data.Skip(skipByte + byteCount).Take(tag.Length).ToArray();
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

            }
            catch
            {
                return null;
            }
        }
    }
}
