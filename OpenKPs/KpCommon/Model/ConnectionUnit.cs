using KpCommon.Extend;
using KpCommon.InterFace;
using Scada;
using System;
using System.ComponentModel;
using System.IO.Ports;
using System.Net;
using System.Xml;

namespace KpCommon.Model
{
    public class ConnectionUnit : IConnectionUnit
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName = null)
        {
            var eventHandler = PropertyChanged;
            if (eventHandler != null)
            {
                eventHandler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public virtual void LoadFromXml(XmlElement optionElement)
        {
            if (optionElement == null)
                throw new ArgumentNullException("OptionElement");

            foreach (var p in GetType().GetProperties( System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance))
            {
                if (!p.CanWrite)
                    continue;
                if (p.PropertyType == typeof(bool))
                {
                    p.SetValue(this, optionElement.GetAttrAsBool(p.Name),null);
                }
                else if (p.PropertyType == typeof(byte))
                {
                    p.SetValue(this, optionElement.GetAttrAsByte(p.Name), null);
                }
                else if (p.PropertyType == typeof(string))
                {
                    p.SetValue(this, optionElement.GetAttrAsString(p.Name), null);
                }
                else if (p.PropertyType == typeof(int))
                {
                    p.SetValue(this, optionElement.GetAttrAsInt(p.Name), null);
                }
                else if (p.PropertyType.IsEnum)
                {
                    try
                    {
                        var enumValue = Enum.Parse(p.PropertyType, optionElement.GetAttrAsString(p.Name), true);
                        p.SetValue(this, enumValue, null);
                    }
                    catch
                    {

                    }
                }
            }
        }

        public virtual void SaveToXml(XmlElement optionElement)
        {
            if (optionElement == null)
                throw new ArgumentNullException("OptionElement");

            foreach(var p in GetType().GetProperties())
            {
                if (!p.CanWrite)
                    continue;
                //p.SetValue(this,p.GetValue(this,null),null);
                optionElement.SetAttribute(p.Name, p.GetValue(this, null));
            }
            //optionElement.SetAttribute("Station", Station);
            //optionElement.SetAttribute("ConnectionType", ConnectionType);
            //optionElement.SetAttribute("ModbusMode", ModbusMode);

            //optionElement.SetAttribute("IPAddress", IPAddress);
            //optionElement.SetAttribute("Port", Port);

            //optionElement.SetAttribute("PortName", PortName);
            //optionElement.SetAttribute("BaudRate", BaudRate);
            //optionElement.SetAttribute("DataBits", DataBits);
            //optionElement.SetAttribute("StopBits", StopBits);
            //optionElement.SetAttribute("Parity", Parity);
        }
    }
}
