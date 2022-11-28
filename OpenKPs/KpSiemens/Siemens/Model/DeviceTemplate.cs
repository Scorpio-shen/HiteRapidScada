using Scada;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace KpSiemens.Siemens.Model
{
    public class DeviceTemplate
    {
        public List<SiemensTagGroup> TagGroups { get; set; }    

        public PLCConnectionOptions ConnectionOptions { get; set; }

        /// <summary>
        /// 指令点数目
        /// </summary>
        public int CmdTagCount
        {
            get=> TagGroups.SelectMany(t=>t.Tags).Where(t=>t.CanWrite > 0).Count();
        }

        public List<Tag> CmdTags
        {
            get => TagGroups.SelectMany(t => t.Tags).Where(t => t.CanWrite > 0).OrderBy(t => t.TagID).ToList();
        }

        public DeviceTemplate()
        {
            TagGroups = new List<SiemensTagGroup>();
            //Cmds = new List<SiemensCmdGroup>();
            ConnectionOptions = new PLCConnectionOptions();
        }
        private void LoadFromXml(XmlNode rootNode)
        {
            XmlNode tagGroupsNode = rootNode.SelectSingleNode("TagGroups");
            if(tagGroupsNode != null)
            {
                XmlNodeList tagGroupsNodes = tagGroupsNode.SelectNodes("TagGroup");
                foreach(XmlElement tagGroupElem in tagGroupsNodes)
                {
                    var tagGroup = new SiemensTagGroup();
                    tagGroup.LoadFromXml(tagGroupElem);

                    if(tagGroup.TagCount > 0)
                    {
                        TagGroups.Add(tagGroup);
                    }
                }
            }

            //XmlNode cmdsNode = rootNode.SelectSingleNode("Cmds");
            //if (cmdsNode != null)
            //{
            //    XmlNodeList cmdNodes = cmdsNode.SelectNodes("Cmd");
            //    foreach (XmlElement cmdNode in cmdNodes)
            //    {
            //        SiemensCmdGroup cmd = new SiemensCmdGroup();
            //        cmd.LoadFromXml(cmdNode);

            //        Cmds.Add(cmd);
            //    }
            //}

            XmlNode optionNode = rootNode.SelectSingleNode("ConnectionOptions");
            if (optionNode != null)
            {
                ConnectionOptions = new PLCConnectionOptions();
                ConnectionOptions.LoadFromXml(optionNode as XmlElement);
            }
        }

        private void SaveToXml(XmlElement rootElement)
        {
            XmlElement tagGroupElem = rootElement.AppendElem("TagGroups");
            foreach (SiemensTagGroup tagGroup in TagGroups)
                tagGroup.SaveToXml(tagGroupElem.AppendElem("TagGroup"));

            //XmlElement cmdsElem = rootElement.AppendElem("Cmds");
            //foreach( SiemensCmdGroup cmd in Cmds)
            //{
            //    cmd.SaveToXml(cmdsElem.AppendElem("Cmd"));
            //}

            XmlElement optionElem = rootElement.AppendElem("ConnectionOptions");
            ConnectionOptions.SaveToXml(optionElem);
        }

        public bool Load(string fileName,out string errMsg)
        {
            try
            {
                XmlDocument document = new XmlDocument();
                document.Load(fileName);
                LoadFromXml(document.DocumentElement);
                errMsg = String.Empty;
                return true;
            }
            catch(Exception ex)
            {
                errMsg = $"DeviceTemplate_Load,载入模板异常,{ex.Message}";
                return false;
            }
        }

        public bool Save(string fileName,out string errMsg)
        {
            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                XmlDeclaration xmlDecl = xmlDoc.CreateXmlDeclaration("1.0", "utf-8", null);
                xmlDoc.AppendChild(xmlDecl);

                XmlElement rootElem = xmlDoc.CreateElement("DevTemplate");
                xmlDoc.AppendChild(rootElem);
                SaveToXml(rootElem);

                xmlDoc.Save(fileName);
                errMsg = "";
                return true;
            }
            catch(Exception ex)
            {
                errMsg = $"DeviceTemplate_Save,保存模板异常,{ex.Message}";
                return false;
            }
        }

        public SiemensTagGroup CreateTagGroup(MemoryTypeEnum memoryType = MemoryTypeEnum.DB)
        {
            var tagGroup = new SiemensTagGroup(memoryType);
            return tagGroup;
        }

        public PLCConnectionOptions CreateOptions()
        {
            var option = new PLCConnectionOptions();
            ConnectionOptions = option;
            return option;
        }

        public void RefreshTagGroupIndex()
        {
            int startIndex = 1;
            TagGroups.ForEach(t =>
            {
                t.StartKpTagIndex = startIndex;
                t.RefreshTagIndex();
                startIndex += t.TagCount;
            });
        }


        /// <summary>
        /// 获取Active TagGroup集合
        /// </summary>
        /// <returns></returns>
        public List<SiemensTagGroup> GetActiveTagGroups()
        {
            var result = new List<SiemensTagGroup>();
            foreach (var tagGroup in TagGroups)
                if (tagGroup.Active)
                    result.Add(tagGroup);

            return result;
        }
        /// <summary>
        /// 根据cmdNumber返回对应Tag点
        /// </summary>
        /// <param name="cmdNumber"></param>
        /// <returns></returns>
        public Tag FindCmd(int cmdNumber,out SiemensTagGroup tagGroup)
        {
            //var cmdTag = Cmds.SelectMany(c => c.Tags).FirstOrDefault(t => t.TagID == cmdNumber);
            //memoryType = cmdTag?.MemoryType ?? MemoryTypeEnum.DB;
            //return cmdTag;
            tagGroup = null;
            Tag result = null;
            foreach (var tg in TagGroups)
            {
                foreach(var tag in tg.Tags)
                {
                    if(tag.TagID == cmdNumber)
                    {
                        tagGroup = tg;
                        result = tag;
                    }
                }
            }

            return result;
        }
    }
}
