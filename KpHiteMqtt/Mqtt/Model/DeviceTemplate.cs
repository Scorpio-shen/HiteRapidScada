using HslCommunication.MQTT;
using KpCommon.Extend;
using KpHiteMqtt.Mqtt.Model.Enum;
using Scada;
using Scada.Data.Entities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;

namespace KpHiteMqtt.Mqtt.Model
{
    public class DeviceTemplate
    {
        public DeviceTemplate() { }
        /// <summary>
        /// 连接参数
        /// </summary>
        public MqttConnectionOptions ConnectionOptions { get; set; } = new MqttConnectionOptions();
        /// <summary>
        /// 订阅Topic(服务端的指令)
        /// </summary>
        public List<string> SubscribeTopics { get;set; } = new List<string>()
        {
            //默认添加系统自带Topic

        };
        /// <summary>
        /// 发布Topic(物模型数据)
        /// </summary>
        public List<string> PublishTopics { get;set; } = new List<string> { };
        /// <summary>
        /// 物模型集合
        /// </summary>
        public List<Property> Properties { get; set; } = new List<Property>();
        /// <summary>
        /// 物模型关联的输入通道集合
        /// </summary>
        public List<InCnl> InCnls { get; set; } = new List<InCnl>();
        /// <summary>
        /// 物模型关联的输出通道集合
        /// </summary>
        public List<CtrlCnl> CtrlCnls { get; set; } = new List<CtrlCnl>();
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
                ConnectionOptions = new MqttConnectionOptions()
                {
                    ClientId = connectionElement.GetAttrAsString("ClientId"),
                    ServerIpAddress = connectionElement.GetAttrAsString("IpAddress"),
                    Port = connectionElement.GetAttrAsInt("Port"),
                    UserName = connectionElement.GetAttrAsString("UserName"),
                    Password = connectionElement.GetAttrAsString("Password"),
                    UseTsl = connectionElement.GetAttrAsBool("UseTsl"),
                    DeviceSn = connectionElement.GetAttrAsString("DeviceSn"),
                    KeepAliveSendInterval = TimeSpan.FromSeconds(connectionElement.GetAttrAsInt("KeepAliveSendInterval")),
                };
            //物模型
            Properties = new List<Property>();
            foreach(XmlElement property in rootElement.SelectSingleNode("Properties").ChildNodes)
            {
                #region 基础参数
                var temp = new Property
                {
                    Id = property.GetAttrAsInt("Id"),
                    Identifier = property.GetAttrAsString("Identifier"),
                    Name = property.GetAttrAsString("Name"),
                    DataType = property.GetAttrAsEnum<DataTypeEnum>("DataType", DataTypeEnum.Int32),
                    Description = property.GetAttrAsString("Description"),
                    //ArrayLength = property.GetAttrAsInt("ArrayLength"),
                    IsReadOnly = property.GetAttrAsBool("IsReadOnly"),
                    Unit = property.GetAttrAsString("Unit"),
                    CnlNum = property.GetAttrAsInt("CnlNum"),
                    CtrlCnlNum = property.GetAttrAsInt("CtrlCnlNum"),
                    DataSpecsList = new List<DataSpecs>(),
                    DataArraySpecs = new DataArraySpecs()
                };
                #endregion

                #region 数组参数
                var dataArraySpecsNode = property.SelectSingleNode("DataArraySpecs");
                var dataArraySpecsElement = dataArraySpecsNode as XmlElement;
                temp.DataArraySpecs.DataType = dataArraySpecsElement.GetAttrAsEnum("DataType", ArrayDataTypeEnum.Int32);
                temp.DataArraySpecs.ArrayLength = dataArraySpecsElement.GetAttrAsInt("ArrayLength");
                //数组内部参数
                var arraySpecsesNode = dataArraySpecsNode.SelectSingleNode("ArraySpecses");
                temp.DataArraySpecs.ArraySpecs = new List<ArraySpecs>();
                foreach (XmlElement arraySpecsElement in arraySpecsesNode.ChildNodes)
                {
                    var arraySpec = new ArraySpecs();
                    arraySpec.InCnlNum = arraySpecsElement.GetAttrAsInt("InCnlNum");
                    arraySpec.CtrlCnlNum = arraySpecsElement.GetAttrAsInt("CtrlCnlNum");
                    //数组内部Json参数
                    arraySpec.DataSpecs = new List<DataSpecs>();

                    foreach(XmlElement dataspecElem in arraySpecsElement.SelectSingleNode("DataSpecs").SelectNodes("Spec"))
                    {
                        arraySpec.DataSpecs.Add(new DataSpecs()
                        {
                            DataType = dataspecElem.GetAttrAsEnum<StructDataTypeEnum>("DataType"),
                            Identifier = dataspecElem.GetAttrAsString("Identifier"),
                            ParameterName = dataspecElem.GetAttrAsString("ParameterName"),
                            Unit = dataspecElem.GetAttrAsString("Unit"),
                            InCnlNum = dataspecElem.GetAttrAsInt("InCnlNum"),
                            CtrlCnlNum = dataspecElem.GetAttrAsInt("CtrlCnlNum")
                        });
                    }
                    temp.DataArraySpecs.ArraySpecs.Add(arraySpec);
                }

                //json参数
                temp.DataArraySpecs.DataSpecs = new List<DataSpecs>();
                var dataSpecsesNode = dataArraySpecsNode.SelectSingleNode("DataSpecses");
                foreach(XmlElement dataspecElem in dataSpecsesNode.ChildNodes)
                {
                    temp.DataArraySpecs.DataSpecs.Add(new DataSpecs
                    {
                        DataType = dataspecElem.GetAttrAsEnum<StructDataTypeEnum>("DataType"),
                        Identifier = dataspecElem.GetAttrAsString("Identifier"),
                        ParameterName = dataspecElem.GetAttrAsString("ParameterName"),
                        Unit = dataspecElem.GetAttrAsString("Unit"),
                        InCnlNum = dataspecElem.GetAttrAsInt("InCnlNum"),
                        CtrlCnlNum = dataspecElem.GetAttrAsInt("CtrlCnlNum")
                    });
                }
                #endregion

                foreach (XmlElement dataspecElem in property.SelectSingleNode("DataSpecs").ChildNodes)
                {
                    temp.DataSpecsList.Add(new DataSpecs
                    {
                        DataType = dataspecElem.GetAttrAsEnum<StructDataTypeEnum>("DataType"),
                        Identifier = dataspecElem.GetAttrAsString("Identifier"),
                        ParameterName = dataspecElem.GetAttrAsString("ParameterName"),
                        Unit = dataspecElem.GetAttrAsString("Unit"),
                        InCnlNum = dataspecElem.GetAttrAsInt("InCnlNum"),
                        CtrlCnlNum = dataspecElem.GetAttrAsInt("CtrlCnlNum")
                    });
                }
                Properties.Add(temp);
            }

            //物模型关联输入、输出通道号
            InCnls = new List<InCnl>();
            LoadInputChannel(rootElement);
            CtrlCnls = new List<CtrlCnl>();
            LoadOutputChannel(rootElement);

            //Topics
            var publishTopicNode = rootElement.SelectSingleNode("PublishTopics");
            foreach(XmlElement publishtopic in publishTopicNode.ChildNodes)
            {
                PublishTopics.Add(publishtopic.InnerText);
            }

            var subscribeTopicNode = rootElement.SelectSingleNode("SubscribeTopics");
            foreach(XmlElement subscribetopic in subscribeTopicNode.ChildNodes)
            {
                SubscribeTopics.Add(subscribetopic.InnerText);
            }

            //判断是否有加载系统自带Topic
            if (!PublishTopics.Contains(ScadaSystemTopics.MqttTsModelData_Publish))
                PublishTopics.Add(ScadaSystemTopics.MqttTsModelData_Publish);

            if (!PublishTopics.Contains(ScadaSystemTopics.MqttCmdReply_Publish))
                PublishTopics.Add(ScadaSystemTopics.MqttCmdReply_Publish);

            if (!SubscribeTopics.Contains(ScadaSystemTopics.MqttTsModelDataReply_Subscribe))
                SubscribeTopics.Add(ScadaSystemTopics.MqttTsModelDataReply_Subscribe);

            if (!SubscribeTopics.Contains(ScadaSystemTopics.MqttCmd_Subscribe))
                SubscribeTopics.Add(ScadaSystemTopics.MqttCmd_Subscribe);


        }

        private void LoadInputChannel(XmlElement rootElement)
        {
            XmlNode inputChannelNode = rootElement.SelectSingleNode("InputChannels");
            if (inputChannelNode != null)
            {
                XmlNodeList inputChannelNodes = inputChannelNode.SelectNodes("InputChannel");
                foreach (XmlElement inputElement in inputChannelNodes)
                {
                    InCnl t = Activator.CreateInstance(typeof(InCnl)) as InCnl;
                    foreach (var p in typeof(InCnl).GetProperties())
                    {
                        if (!p.CanWrite)
                            continue;
                        if (t == null)
                            return;

                        if (p.PropertyType == typeof(bool))
                        {
                            p.SetValue(t, inputElement.GetAttrAsBool(p.Name), null);
                        }
                        else if (p.PropertyType == typeof(byte))
                        {
                            p.SetValue(t, inputElement.GetAttrAsByte(p.Name), null);
                        }
                        else if (p.PropertyType == typeof(string))
                        {
                            p.SetValue(t, inputElement.GetAttrAsString(p.Name), null);
                        }
                        else if (p.PropertyType == typeof(int))
                        {
                            p.SetValue(t, inputElement.GetAttrAsInt(p.Name), null);
                        }
                        else if (p.PropertyType.IsEnum)
                        {
                            try
                            {
                                var enumValue = System.Enum.Parse(p.PropertyType, inputElement.GetAttrAsString(p.Name), true);
                                p.SetValue(t, enumValue, null);
                            }
                            catch
                            {

                            }
                        }
                    }
                    InCnls.Add(t);
                }
            }
        }

        private void LoadOutputChannel( XmlElement rootElement)
        {
            XmlNode outputChannelNode = rootElement.SelectSingleNode("OutputChannels");
            if (outputChannelNode != null)
            {
                XmlNodeList outputChannelNodes = outputChannelNode.SelectNodes("OutputChannel");
                foreach (XmlElement inputElement in outputChannelNodes)
                {
                    CtrlCnl t = Activator.CreateInstance(typeof(CtrlCnl)) as CtrlCnl;
                    foreach (var p in typeof(CtrlCnl).GetProperties())
                    {
                        if (!p.CanWrite)
                            continue;
                        if (t == null)
                            return;

                        if (p.PropertyType == typeof(bool))
                        {
                            p.SetValue(t, inputElement.GetAttrAsBool(p.Name), null);
                        }
                        else if (p.PropertyType == typeof(byte))
                        {
                            p.SetValue(t, inputElement.GetAttrAsByte(p.Name), null);
                        }
                        else if (p.PropertyType == typeof(string))
                        {
                            p.SetValue(t, inputElement.GetAttrAsString(p.Name), null);
                        }
                        else if (p.PropertyType == typeof(int))
                        {
                            p.SetValue(t, inputElement.GetAttrAsInt(p.Name), null);
                        }
                        else if (p.PropertyType.IsEnum)
                        {
                            try
                            {
                                var enumValue = System.Enum.Parse(p.PropertyType, inputElement.GetAttrAsString(p.Name), true);
                                p.SetValue(t, enumValue, null);
                            }
                            catch
                            {

                            }
                        }
                    }
                    CtrlCnls.Add(t);
                }
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
            connectElement.SetAttribute("IpAddress", ConnectionOptions.ServerIpAddress);
            connectElement.SetAttribute("Port", ConnectionOptions.Port.ToString());
            connectElement.SetAttribute("UserName", ConnectionOptions.UserName);
            connectElement.SetAttribute("Password", ConnectionOptions.Password);
            connectElement.SetAttribute("UseTsl", ConnectionOptions.UseTsl);
            connectElement.SetAttribute("KeepAliveSendInterval", ConnectionOptions.KeepAliveSendInterval.TotalSeconds);
            connectElement.SetAttribute("DeviceSn", ConnectionOptions.DeviceSn);
            //物模型
            var propertyElement = rootElement.AppendElem("Properties");
            
            foreach(var property in Properties)
            {
                #region 基础参数
                var pElement = propertyElement.AppendElem("Property");
                pElement.SetAttribute("Id", property.Id);
                pElement.SetAttribute("Name", property.Name);
                pElement.SetAttribute("Identifier", property.Identifier);
                pElement.SetAttribute("Description", property.Description);
                pElement.SetAttribute("DataType", property.DataType);
                pElement.SetAttribute("IsReadOnly", property.IsReadOnly);
                //pElement.SetAttribute("ArrayLength", property.ArrayLength);
                pElement.SetAttribute("CnlNum", property.CnlNum.ToString());
                pElement.SetAttribute("CtrlCnlNum", property.CtrlCnlNum.ToString());
                pElement.SetAttribute("Unit", property.Unit);
                #endregion

                #region 数组类型参数
                //数组参数
                var dataArraySpecsElement = pElement.AppendElem("DataArraySpecs");
                dataArraySpecsElement.SetAttribute("DataType", property.DataArraySpecs.DataType);
                dataArraySpecsElement.SetAttribute("ArrayLength", property.DataArraySpecs.ArrayLength);

                var arraySpecsesElement = dataArraySpecsElement.AppendElem("ArraySpecses");
                foreach(var arraySpecs in property.DataArraySpecs.ArraySpecs)
                {
                    var arraySpecsElement = arraySpecsesElement.AppendElem("ArraySpecs");
                    arraySpecsElement.SetAttribute("InCnlNum", arraySpecs.InCnlNum);
                    arraySpecsElement.SetAttribute("CtrlCnlNum", arraySpecs.CtrlCnlNum);
                    //数组内部Json参数
                    var arraySpecsElements = arraySpecsElement.AppendElem("DataSpecs");
                    foreach (var data in arraySpecs.DataSpecs)
                    {
                        var specElement = arraySpecsElements.AppendElem("Spec");
                        specElement.SetAttribute("Identifier", data.Identifier);
                        specElement.SetAttribute("ParameterName", data.ParameterName);
                        specElement.SetAttribute("DataType", data.DataType);
                        specElement.SetAttribute("Unit", data.Unit);
                        specElement.SetAttribute("InCnlNum", data.InCnlNum);
                        specElement.SetAttribute("CtrlCnlNum", data.CtrlCnlNum);
                    }
                }


                //Json参数
                var dataSpecsElement = dataArraySpecsElement.AppendElem("DataSpecses");
                foreach(var data in property.DataArraySpecs.DataSpecs)
                {
                    var dataElement = dataSpecsElement.AppendElem("DataSpecs");
                    dataElement.SetAttribute("Identifier", data.Identifier);
                    dataElement.SetAttribute("ParameterName", data.ParameterName);
                    dataElement.SetAttribute("DataType", data.DataType);
                    dataElement.SetAttribute("Unit", data.Unit);
                    dataElement.SetAttribute("InCnlNum", data.InCnlNum);
                    dataElement.SetAttribute("CtrlCnlNum", data.CtrlCnlNum);
                }
                #endregion

                #region Json参数
                //Json参数
                var specsElements = pElement.AppendElem("DataSpecs");
                foreach (var dataspec in property.DataSpecsList)
                {
                    var specElement = specsElements.AppendElem("Spec");
                    specElement.SetAttribute("Identifier", dataspec.Identifier);
                    specElement.SetAttribute("ParameterName", dataspec.ParameterName);
                    specElement.SetAttribute("DataType", dataspec.DataType);
                    specElement.SetAttribute("Unit", dataspec.Unit);
                    specElement.SetAttribute("InCnlNum", dataspec.InCnlNum);
                    specElement.SetAttribute("CtrlCnlNum", dataspec.CtrlCnlNum);
                }
                #endregion
            }

            //输入、输出通道
            SaveInputChannel(InCnls,rootElement);
            SaveOutputChannel(CtrlCnls,rootElement);

            //Topics
            var publishTopicElement = rootElement.AppendElem("PublishTopics");
            foreach(var publishTopic in PublishTopics)
            {
                var element = publishTopicElement.AppendElem("PublishTopic",publishTopic);
            }
            var subscribeTopicElement = rootElement.AppendElem("SubscribeTopics");
            foreach (var subscribetopic in SubscribeTopics)
            {
                subscribeTopicElement.AppendElem("SubscribeTopic", subscribetopic);
            }

        }
        private void SaveInputChannel(List<InCnl> inputChannels, XmlElement rootElement)
        {
            var inputChannelElem = rootElement.AppendElem("InputChannels");
            foreach (var inputChannel in inputChannels)
            {
                XmlElement inputElem = inputChannelElem.AppendElem("InputChannel");
                foreach (var tagProperty in inputChannel.GetType().GetProperties())
                {
                    if (!tagProperty.CanWrite)
                        continue;
                    inputElem.SetAttribute(tagProperty.Name, tagProperty.GetValue(inputChannel));
                }
            }
        }

        private void SaveOutputChannel(List<CtrlCnl> outputChannels, XmlElement rootElement)
        {
            XmlElement outputChannelElem = rootElement.AppendElem("OutputChannels");
            foreach (var outputChannel in outputChannels)
            {
                XmlElement outputElem = outputChannelElem.AppendElem("OutputChannel");
                foreach (var tagProperty in outputChannel.GetType().GetProperties())
                {
                    if (!tagProperty.CanWrite)
                        continue;
                    outputElem.SetAttribute(tagProperty.Name, tagProperty.GetValue(outputChannel));
                }
            }
        }

        #endregion

        private List<T> Load<T>(string fileName)
        {
            List<T> list;
            XmlSerializer serializer = new XmlSerializer(typeof(List<T>));

            using (XmlReader reader = XmlReader.Create(fileName))
            {
                list = (List<T>)serializer.Deserialize(reader);
            }

            return list;
        }

        #region 获取关联的输入、输出通道号
        /// <summary>
        /// 获取物模型关联输入通道号
        /// </summary>
        /// <returns></returns>
        public List<int> GetInCnls()
        {
            List<int> incnls = new List<int>();
            Properties.ForEach(p =>
            {
                if (p.DataType == DataTypeEnum.Array)
                {
                    if(p.DataArraySpecs.IsStruct)
                    {
                        incnls.AddRange(p.DataArraySpecs.ArraySpecs.SelectMany(asp=>asp.DataSpecs.Select(d=>d.InCnlNum)));
                    }
                    else
                    {
                        incnls.AddRange(p.DataArraySpecs.ArraySpecs.Select(asp=>asp.InCnlNum));
                    }
                }
                else if (p.DataType == DataTypeEnum.Struct)
                {
                    incnls.AddRange(p.DataSpecsList.Select(ds => ds.InCnlNum));
                }
                else
                {
                    incnls.Add(p.CnlNum);
                }
            });

            return incnls;
        }
        /// <summary>
        /// 获取物模型关联的输出通道号
        /// </summary>
        /// <returns></returns>
        public List<int> GetCtrlCnls()
        {
            List<int> ctrlcnls = new List<int>();
            
            Properties.ForEach(p =>
            {
                if (p.IsReadOnly)
                    return;
                if (p.DataType == DataTypeEnum.Array)
                {
                    if (p.DataArraySpecs.IsStruct)
                    {
                        ctrlcnls.AddRange(p.DataArraySpecs.ArraySpecs.SelectMany(asp => asp.DataSpecs.Select(d=>d.CtrlCnlNum)));
                    }
                    else
                    {
                        ctrlcnls.AddRange(p.DataArraySpecs.ArraySpecs.Select(asp => asp.CtrlCnlNum));
                    }
                }
                else if (p.DataType == DataTypeEnum.Struct)
                {
                    ctrlcnls.AddRange(p.DataSpecsList.Select(ds => ds.CtrlCnlNum));
                }
                else
                {
                    ctrlcnls.Add(p.CtrlCnlNum);
                }
            });

            return ctrlcnls;
        }
        #endregion
    }
}
