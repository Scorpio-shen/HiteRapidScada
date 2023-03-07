using HslCommunication.MQTT;
using KpHiteMqtt.Mqtt.Model.Enum;
using Scada;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace KpHiteMqtt.Mqtt.Model
{
    public class DeviceTemplate
    {
        public DeviceTemplate() { }
        /// <summary>
        /// 连接参数
        /// </summary>
        public MqttConnectionOptions ConnectionOptions { get; set; }
        /// <summary>
        /// 物模型集合
        /// </summary>
        public List<Property> Properties { get; set; }

        #region 保存载入XML
        public bool Load(string fileName, out string errMsg)
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
                errMsg = $"模板载入异常,{ex.Message}";
                return false;
            }
        }

        private void LoadFromXml(XmlElement rootElement)
        {
            //连接参数
            var connectionNode = rootElement.SelectSingleNode("ConnectionOptions");
            var connectionElement = connectionNode as XmlElement;
            if (ConnectionOptions == null)
                ConnectionOptions = new MqttConnectionOptions()
                {
                    ClientId = connectionElement.GetAttrAsString("ClientId"),
                    IpAddress = connectionElement.GetAttrAsString("IpAddress"),
                    Port = connectionElement.GetAttrAsInt("Port"),
                    Credentials = new MqttCredential
                    {
                        UserName = connectionElement.GetAttrAsString("UserName"),
                        Password = connectionElement.GetAttrAsString("Password")
                    },
                    KeepAliveSendInterval = TimeSpan.FromSeconds(connectionElement.GetAttrAsInt("KeepAliveSendInterval")),
                };
            //物模型
            Properties = new List<Property>();
            foreach(XmlElement property in rootElement.SelectSingleNode("Properties").ChildNodes)
            {
                var temp = new Property
                {
                    Identifier = property.GetAttrAsString("Identifier"),
                    Name = property.GetAttrAsString("Name"),
                    DataType = property.GetAttrAsEnum<DataTypeEnum>("DataType", DataTypeEnum.Int32),
                    Description = property.GetAttrAsString("Description"),
                    ArrayLength = property.GetAttrAsInt("ArrayLength"),
                    IsReadOnly = property.GetAttrAsBool("IsReadOnly"),
                    Unit = property.GetAttrAsString("Unit"),
                    DataSpecsList = new List<DataSpecs>()
                };
                foreach(XmlElement dataspecElem in property.SelectSingleNode("DataSpecs").ChildNodes)
                {
                    temp.DataSpecsList.Add(new DataSpecs
                    {
                        DataType = dataspecElem.GetAttrAsEnum<StructDataTypeEnum>("DataType"),
                        Identifier = dataspecElem.GetAttrAsString("Identifier"),
                        ParameterName = dataspecElem.GetAttrAsString("ParameterName"),
                        Unit = dataspecElem.GetAttrAsString("Unit"),
                    });
                }
                Properties.Add(temp);
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
            catch(Exception ex)
            {
                errMsg = $"保存模板异常,{ex.Message}";
                return false;
            }
        }

        private void SaveToXml(XmlElement rootElement)
        {
            //连接参数
            var connectElement = rootElement.AppendElem("ConnectionOptions");

            connectElement.SetAttribute("ClientId", ConnectionOptions.ClientId);
            connectElement.SetAttribute("IpAddress", ConnectionOptions.IpAddress);
            connectElement.SetAttribute("Port", ConnectionOptions.Port.ToString());
            connectElement.SetAttribute("UserName", ConnectionOptions.Credentials.UserName);
            connectElement.SetAttribute("Password", ConnectionOptions.Credentials.Password);
            connectElement.SetAttribute("KeepAliveSendInterval", ConnectionOptions.KeepAliveSendInterval.TotalSeconds);
            //物模型
            var propertyElement = rootElement.AppendElem("Properties");
            
            foreach(var property in Properties)
            {
                var pElement = propertyElement.AppendElem("Property");
                pElement.SetAttribute("Name", property.Name);
                pElement.SetAttribute("Identifier", property.Identifier);
                pElement.SetAttribute("Description", property.Description);
                pElement.SetAttribute("DataType", property.DataType);
                pElement.SetAttribute("IsReadOnly", property.IsReadOnly);
                pElement.SetAttribute("ArrayLength", property.ArrayLength);
                pElement.SetAttribute("Unit", property.Unit);
                var specsElements = pElement.AppendElem("DataSpecs");
                foreach (var dataspecs in property.DataSpecsList)
                {
                    var specElement = specsElements.AppendElem("Spec");
                    specElement.SetAttribute("Identifier", dataspecs.Identifier);
                    specElement.SetAttribute("ParameterName", dataspecs.ParameterName);
                    specElement.SetAttribute("DataType", dataspecs.DataType);
                    specElement.SetAttribute("Unit", dataspecs.Unit);
                }
            }

        }


        #endregion

    }
}
