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
            RegisterType = registerType;
            Tags = new List<Tag>();
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
                registerType = value;
                SetRegisterType(registerType);
            }
        }
        private void SetRegisterType(RegisterTypeEnum registerType) => Tags.ForEach(t => t.RegisterType = registerType);
        public override double MaxAddressLength { get; set; }
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
            MaxAddressLength = tagElem.GetAttrAsDouble("MaxAddressLength");
            if(MaxAddressLength == 0)
                MaxAddressLength = TagGroupDefaultValues.MaxAddressLength;

            XmlNodeList nodes = tagElem.SelectNodes("Tag");
            int maxTagCount = MaxTagCount;
            RegisterTypeEnum type = RegisterType;
            foreach(XmlElement element in nodes)
            {
                if(TagCount > MaxTagCount)
                    break;

                var tagID = element.GetAttrAsInt("TagID");
                var name = tagElem.GetAttribute("Name");
                var dataType = tagElem.GetAttrAsEnum("DataType", DataTypeEnum.Bool);
                var address = tagElem.GetAttrAsString("Address");
                var length = tagElem.GetAttrAsInt("Length");
                var canwrite = tagElem.GetAttrAsString("CanWrite").ToByte();
                var tag = Tag.CreateNewTag(tagID:tagID, tagname:name,dataType:dataType,registerType:RegisterType,address:address,canwrite:canwrite,length:length);
                Tags.Add(tag);
            }

        }

        public override void SaveToXml(XmlElement xmlElement)
        {
            throw new NotImplementedException();
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
            if (model.Length > MaxAddressLength)
            {
                Tags.Clear();
                Tags.AddRange(tagsOld);
                result = false;
            }

            return result;
        }

        public TagGroupRequestModel GetRequestModel()
        {
            throw new Exception();
        }
        #endregion

    }
}
