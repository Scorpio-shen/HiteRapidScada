using KpMyModbus.Modbus.Protocol;
using Scada;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace KpMyModbus.Modbus
{
    public class DeviceTemplate
    {
        public List<MyTagGroup> TagGroups { get; set; }
        public List<ModbusCmd> ModbusCmds { get; set; }
        /// <summary>
        /// Modbus连接参数配置
        /// </summary>
        public ModbusConnectionOptions ConnectionOptions { get; set; }

        public DeviceTemplate()
        {
            TagGroups = new List<MyTagGroup>();
            ModbusCmds = new List<ModbusCmd>();
            ConnectionOptions = new ModbusConnectionOptions();
        }

        protected virtual void SetToDefault()
        {
            TagGroups.Clear();
            ModbusCmds.Clear();
        }

        #region 从XML文件载入或存储到XML文件
        public bool Load(string fileName, out string errMsg)
        {
            SetToDefault();
            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(fileName);
                LoadFromXml(xmlDoc.DocumentElement);
                errMsg = String.Empty;
                return true;
            }
            catch (Exception ex)
            {
                errMsg = ModbusPhrases.LoadTemplateError + ":" + Environment.NewLine + ex.Message;
                return false;
            }
        }

        public bool Save(string fileName, out string errMsg)
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
            catch (Exception ex)
            {
                errMsg = ModbusPhrases.SaveTemplateError + ":" + Environment.NewLine + ex.Message;
                return false;
            }
        }

        public void CopyFrom(DeviceTemplate srcTemplate)
        {
            if (srcTemplate == null)
                throw new ArgumentNullException("源模板为空");

            TagGroups.Clear();
            TagGroups.AddRange(srcTemplate.TagGroups);

            ModbusCmds.Clear();
            ModbusCmds.AddRange(srcTemplate.ModbusCmds);

            ConnectionOptions = srcTemplate.ConnectionOptions;

        }
        protected virtual void LoadFromXml(XmlNode rootNode)
        {
            XmlNode tagGroupNode = rootNode.SelectSingleNode("TagGroups");
            if (tagGroupNode != null)
            {
                XmlNodeList tagGroupNodes = tagGroupNode.SelectNodes("TagGroup");
                int kpTagIndex = 0;
                foreach (XmlElement tagGroupElemnt in tagGroupNodes)
                {
                    MyTagGroup tagGroup = CreateTagGroup(tagGroupElemnt.GetAttrAsEnum<ModbusRegisterType>("modbusRegisterType"));
                    tagGroup.StartKPTagIndex = kpTagIndex;
                    tagGroup.StartSignal = kpTagIndex + 1;
                    tagGroup.LoadFromXml(tagGroupElemnt);

                    if (tagGroup.Tags.Count > 0)
                    {
                        TagGroups.Add(tagGroup);
                        kpTagIndex += tagGroup.Tags.Count;
                    }
                }
            }

            XmlNode cmdsNode = rootNode.SelectSingleNode("Cmds");
            if (cmdsNode != null)
            {
                XmlNodeList cmdNodes = cmdsNode.SelectNodes("Cmd");
                foreach (XmlElement cmdNode in cmdNodes)
                {
                    ModbusCmd cmd = CreateModbusCmd(cmdNode.GetAttrAsEnum<ModbusRegisterType>("modbusRegisterType"),
                        cmdNode.GetAttrAsBool("multiple"));
                    cmd.LoadFromXml(cmdNode);

                    if (cmd.CmdNum > 0)
                        ModbusCmds.Add(cmd);
                }
            }

            XmlNode optionNode = rootNode.SelectSingleNode("ModbusOption");
            if (optionNode != null)
            {
                ConnectionOptions.ServerIpAddress = optionNode.Attributes["ServerIpAddress"].Value;
                ConnectionOptions.ServerPort = optionNode.Attributes["ServerPort"].Value;
            }
        }

        protected virtual void SaveToXml(XmlElement rootElem)
        {
            XmlElement tagGroupElem = rootElem.AppendElem("TagGroups");
            foreach (MyTagGroup tagGroup in TagGroups)
            {
                tagGroup.SaveToXml(tagGroupElem.AppendElem("TagGroup"));
            }

            XmlElement cmdsElem = rootElem.AppendElem("Cmds");
            foreach (ModbusCmd cmd in ModbusCmds)
                cmd.SaveToXml(cmdsElem.AppendElem("Cmd"));

            XmlElement optionElem = rootElem.AppendElem("ModbusOption");
            optionElem.SetAttribute("ServerIpAddress", ConnectionOptions.ServerIpAddress);
            optionElem.SetAttribute("ServerPort", ConnectionOptions.ServerPort);
        }
        #endregion

        public ModbusCmd FindCmd(int cmdNum)
        {
            foreach(ModbusCmd cmd in ModbusCmds)
            {
                if (cmd.CmdNum == cmdNum)
                    return cmd;
            }

            return null;
        }

        public virtual MyTagGroup CreateTagGroup(ModbusRegisterType modbusRegisterType)
        {
            return new MyTagGroup(modbusRegisterType);
        }

        public virtual ModbusCmd CreateModbusCmd(ModbusRegisterType registerType,bool multiple)
        {
            return new ModbusCmd(registerType, multiple);
        }

        /// <summary>
        /// 获取Active的TagGroup
        /// </summary>
        /// <returns></returns>
        public List<MyTagGroup> GetActiveTagGroups()
        {
            var reuslt = new List<MyTagGroup>();
            foreach(var tagGroup in TagGroups)
                if(tagGroup.Active)
                    reuslt.Add(tagGroup);

            return reuslt;
        }
    }
}
