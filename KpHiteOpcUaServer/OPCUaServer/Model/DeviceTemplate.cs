using KpCommon.Extend;
using KpCommon.Model;
using Scada;
using Scada.Data.Entities;
using Scada.Data.Models;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using static Org.BouncyCastle.Math.EC.ECCurve;
using static Scada.Comm.Devices.KPView;

namespace KpHiteOpcUaServer.OPCUaServer.Model
{
    public class DeviceTemplate
    {
        /// <summary>
        /// OPCServer IP地址
        /// </summary>
        public string OPCServerIP { get; set; }
        /// <summary>
        /// 输入通道配置文件路径
        /// </summary>
        public string InputChannelPath { get; set; }
        /// <summary>
        /// 输出通道配置文件路径
        /// </summary>
        public string OutputChannelPath { get; set; }
        /// <summary>
        /// 输入通道
        /// </summary>
        public List<InCnl> InCnls { get; set; }
        /// <summary>
        /// 输出通道
        /// </summary>
        public List<CtrlCnl> CtrlCnls { get; set; }
        /// <summary>
        /// 输入通道Model Value变化
        /// </summary>
        public Action<InputChannelModel> ModelValueChanged;
        /// <summary>
        /// 外部OPC Client赋值
        /// </summary>
        public Action<OutputChannelModel> OpcClientSetValue;
        public DeviceTemplate() : base()
        {
            InCnls = new List<InCnl>();
            CtrlCnls = new List<CtrlCnl>();
        }

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
            catch (Exception ex)
            {
                errMsg = $"DeviceTemplate_Load,载入模板异常,{ex.Message}";
                return false;
            }
        }

        public bool Save(string fileName, string inputChannelPath,string outputChannelPath,out string errMsg)
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
                errMsg = $"DeviceTemplate_Save,保存模板异常,{ex.Message}";
                return false;
            }
        }

        public void LoadFromXml(XmlNode rootNode)
        {
            XmlNode tagGroupsNode = rootNode.SelectSingleNode("OPCUaServer");
            XmlElement element = tagGroupsNode as XmlElement;

            OPCServerIP = element.GetAttrAsString("OPCServerIP");
            InputChannelPath = element.GetAttrAsString("InputChannelPath");
            OutputChannelPath = element.GetAttrAsString("OutputChannelPath");

            //载入输入通道
            LoadInputChannel(InputChannelPath, rootNode as XmlElement);
            //载入输出通道
            LoadOutputChannel(OutputChannelPath, rootNode as XmlElement);

        }

        public void SaveToXml(XmlElement rootElement)
        {
            XmlElement element = rootElement.AppendElem("OPCUaServer");
            element.SetAttribute("OPCServerIP", OPCServerIP);
            element.SetAttribute("InputChannelPath", InputChannelPath);
            element.SetAttribute("OutputChannelPath", OutputChannelPath);

            //存储输入通道
            SaveInputChannel(InputChannelPath, rootElement);
            //存储输出通道
            SaveOutputChannel(OutputChannelPath, rootElement);
        }

        private void SaveInputChannel(string inputChannelPath,XmlElement rootElement)
        {
            var inputChannels = Load<InCnl>(inputChannelPath);
            XmlElement inputChannelElem = rootElement.AppendElem("InputChannels");
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

        private void SaveOutputChannel(string outputChannelPath, XmlElement rootElement)
        {
            var outputChannels = Load<CtrlCnl>(outputChannelPath);
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

        private void LoadInputChannel(string inputChannelPath,XmlElement rootElement) 
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
                                var enumValue = Enum.Parse(p.PropertyType, inputElement.GetAttrAsString(p.Name), true);
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

        private void LoadOutputChannel(string outputChannelPath,XmlElement rootElement)
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
                                var enumValue = Enum.Parse(p.PropertyType, inputElement.GetAttrAsString(p.Name), true);
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
    }
}
