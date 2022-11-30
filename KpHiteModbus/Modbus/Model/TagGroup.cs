using KpHiteModbus.Modbus.Extend;
using Scada;
using Scada.Extend;
using Scada.KPModel;
using System;
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

        private int requestlength;
        /// <summary>
        /// 请求寄存器个数
        /// </summary>
        public int RequestLength
        {
            get => requestlength;
            set=> requestlength = value;
        }
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
            RequestLength = tagElem.GetAttrAsInt("RequestLength");
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
            tagGroupElement.SetAttribute("RequestLength", RequestLength);
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
        public override bool CheckAndAddTags(List<Tag> addTags, bool needClear = false)
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

        public TagGroupRequestModel GetRequestModel()
        {
            var model = new TagGroupRequestModel();
            var functionCode = GetFunctionCode(RegisterType);
            model.Address = $"x={RegisterType};{StartAddress}";
            if (TagCount == 0)
                model.Length = 0;
            else
            {
                var tag = Tags.Last();
                if (double.TryParse(tag.Address, out double address))
                {
                    model.Length = (ushort)(address + tag.DataType.GetByteCount());
                    if (tag.Length > 0)
                        model.Length += (ushort)tag.Length;
                }
                else
                    model.Length = 0;
            }

            return model;
        }

        private byte GetFunctionCode(RegisterTypeEnum register,bool iswrite = false,bool ismultiple = true)
        {
            byte code = default;
            if (!iswrite)
            {
                switch (register)
                {
                    case RegisterTypeEnum.Coils:
                        code = FunctionCodes.ReadCoils;
                        break;
                    case RegisterTypeEnum.DiscretesInputs:
                        code = FunctionCodes.ReadDiscreteInputs;
                        break;
                    case RegisterTypeEnum.HoldingRegisters:
                        code = FunctionCodes.ReadHoldingRegisters;
                        break;
                    case RegisterTypeEnum.InputRegisters:
                        code = FunctionCodes.ReadInputRegisters;
                        break;
                }
            }
            else
            {
                switch (register)
                {
                    case RegisterTypeEnum.Coils:
                        if (!ismultiple)
                            code = FunctionCodes.WriteSingleCoil;
                        else
                            code = FunctionCodes.WriteMultipleCoils;
                        break;
                    case RegisterTypeEnum.HoldingRegisters:
                        if (!ismultiple)
                            code = FunctionCodes.WriteSingleRegister;
                        else
                            code = FunctionCodes.WriteMultipleRegisters;
                        break;
                }
            }

            return code;
        }
        #endregion

    }
}
