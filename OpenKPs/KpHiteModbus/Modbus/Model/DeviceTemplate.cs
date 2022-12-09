using KpCommon.Model;
using KpHiteModbus.Modbus.Model;
using Newtonsoft.Json;
using Scada;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace KpHiteModbus.Modbus.Model
{
    public class DeviceTemplate : TemplateUnit<ModbusTagGroup,Tag,ConnectionOptions>
    {

        public DeviceTemplate() : base()
        {
            
        }
        protected override void LoadFromXml(XmlNode rootNode)
        {
            XmlNode tagGroupsNode = rootNode.SelectSingleNode("TagGroups");
            if(tagGroupsNode != null)
            {
                XmlNodeList tagGroupsNodes = tagGroupsNode.SelectNodes("TagGroup");
                foreach(XmlElement tagGroupElem in tagGroupsNodes)
                {
                    var tagGroup = new ModbusTagGroup();
                    tagGroup.LoadFromXml(tagGroupElem);

                    if(tagGroup.TagCount >= 0)
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
                ConnectionOptions = new ConnectionOptions();
                ConnectionOptions.LoadFromXml(optionNode as XmlElement);
            }
        }

        protected override void SaveToXml(XmlElement rootElement)
        {
            XmlElement tagGroupElem = rootElement.AppendElem("TagGroups");
            foreach (ModbusTagGroup tagGroup in TagGroups)
                tagGroup.SaveToXml(tagGroupElem.AppendElem("TagGroup"));

            //XmlElement cmdsElem = rootElement.AppendElem("Cmds");
            //foreach( SiemensCmdGroup cmd in Cmds)
            //{
            //    cmd.SaveToXml(cmdsElem.AppendElem("Cmd"));
            //}

            XmlElement optionElem = rootElement.AppendElem("ConnectionOptions");
            ConnectionOptions.SaveToXml(optionElem);
        }

        //protected override void LoadFromJson(string json)
        //{
        //    this = JsonConvert.DeserializeObject<DeviceTemplate>(json);
        //}

        //protected override string SaveToJson()
        //{
        //    throw new NotImplementedException();
        //}

        public ModbusTagGroup CreateTagGroup(RegisterTypeEnum registerType = RegisterTypeEnum.HoldingRegisters)
        {
            var tagGroup = new ModbusTagGroup(registerType);
            return tagGroup;
        }

        public ConnectionOptions CreateOptions()
        {
            var option = new ConnectionOptions()
            {
                Station = 1,
                IPAddress = "127.0.0.1",
                Port = 502,
                BaudRate = 9600,
                DataBits = 8,
                Parity = System.IO.Ports.Parity.None,
                StopBits = System.IO.Ports.StopBits.One,
            };
            ConnectionOptions = option;
            return option;
        }

        
    }
}
