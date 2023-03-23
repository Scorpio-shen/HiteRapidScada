using KpCommon.Model;
using KpMelsec.Model.EnumType;
using Scada;
using System.Xml;

namespace KpMelsec.Model
{
    public class DeviceTemplate : TemplateUnit<TagGroup,Tag,ConnectionOptions>
    {

        public DeviceTemplate() : base()
        {
            //TagGroups = new List<ModbusTagGroup>();
            //ConnectionOptions = new ConnectionOptions();
        }
        protected override void LoadFromXml(XmlNode rootNode)
        {
            XmlNode tagGroupsNode = rootNode.SelectSingleNode("TagGroups");
            if(tagGroupsNode != null)
            {
                XmlNodeList tagGroupsNodes = tagGroupsNode.SelectNodes("TagGroup");
                foreach(XmlElement tagGroupElem in tagGroupsNodes)
                {
                    var tagGroup = new TagGroup();
                    tagGroup.LoadFromXml(tagGroupElem);

                    if(tagGroup.TagCount >= 0)
                    {
                        TagGroups.Add(tagGroup);
                    }
                }
            }

            XmlNode optionNode = rootNode.SelectSingleNode("ConnectionOptions");
            if (optionNode != null)
            {
                ConnectionOptions = new ConnectionOptions();
                ConnectionOptions.LoadFromXml(optionNode as XmlElement);
            }
        }

        protected override void SaveToXml(XmlElement rootElement)
        {
            XmlElement tagGroupElem = rootElement.AppendElem("TagGroups");
            foreach (TagGroup tagGroup in TagGroups)
                tagGroup.SaveToXml(tagGroupElem.AppendElem("TagGroup"));


            XmlElement optionElem = rootElement.AppendElem("ConnectionOptions");
            ConnectionOptions.SaveToXml(optionElem);
        }

        //public bool Load(string fileName,out string errMsg)
        //{
        //    try
        //    {
        //        XmlDocument document = new XmlDocument();
        //        document.Load(fileName);
        //        LoadFromXml(document.DocumentElement);
        //        errMsg = String.Empty;
        //        return true;
        //    }
        //    catch(Exception ex)
        //    {
        //        errMsg = $"DeviceTemplate_Load,载入模板异常,{ex.Message}";
        //        return false;
        //    }
        //}

        //public bool Save(string fileName,out string errMsg)
        //{
        //    try
        //    {
        //        XmlDocument xmlDoc = new XmlDocument();
        //        XmlDeclaration xmlDecl = xmlDoc.CreateXmlDeclaration("1.0", "utf-8", null);
        //        xmlDoc.AppendChild(xmlDecl);

        //        XmlElement rootElem = xmlDoc.CreateElement("DevTemplate");
        //        xmlDoc.AppendChild(rootElem);
        //        SaveToXml(rootElem);

        //        xmlDoc.Save(fileName);
        //        errMsg = "";
        //        return true;
        //    }
        //    catch(Exception ex)
        //    {
        //        errMsg = $"DeviceTemplate_Save,保存模板异常,{ex.Message}";
        //        return false;
        //    }
        //}

        public TagGroup CreateTagGroup(MemoryTypeEnum memoryType = MemoryTypeEnum.D)
        {
            var tagGroup = new TagGroup(memoryType);
            return tagGroup;
        }

        public ConnectionOptions CreateOptions()
        {
            var option = new ConnectionOptions()
            {
                IPAddress = "127.0.0.1",
                Port = 6000,
            };
            ConnectionOptions = option;
            return option;
        }

        //public void RefreshTagGroupIndex()
        //{
        //    int startIndex = 1;
        //    TagGroups.ForEach(t =>
        //    {
        //        t.StartKpTagIndex = startIndex;
        //        t.SortTags();
        //        startIndex += t.TagCount;
        //    });
        //}


        ///// <summary>
        ///// 获取Active TagGroup集合
        ///// </summary>
        ///// <returns></returns>
        //public List<ModbusTagGroup> GetActiveTagGroups()
        //{
        //    var result = new List<ModbusTagGroup>();
        //    foreach (var tagGroup in TagGroups)
        //        if (tagGroup.Active)
        //            result.Add(tagGroup);

        //    return result;
        //}
        /// <summary>
        /// 根据cmdNumber返回对应Tag点
        /// </summary>
        /// <param name="cmdNumber"></param>
        /// <returns></returns>
        //public Tag FindCmd(int cmdNumber,out ModbusTagGroup tagGroup)
        //{
        //    //var cmdTag = Cmds.SelectMany(c => c.Tags).FirstOrDefault(t => t.TagID == cmdNumber);
        //    //memoryType = cmdTag?.MemoryType ?? MemoryTypeEnum.DB;
        //    //return cmdTag;
        //    tagGroup = null;
        //    Tag result = null;
        //    foreach (var tg in TagGroups)
        //    {
        //        foreach(var tag in tg.Tags)
        //        {
        //            if(tag.TagID == cmdNumber)
        //            {
        //                tagGroup = tg;
        //                result = tag;
        //            }
        //        }
        //    }

        //    return result;
        //}
    }
}
