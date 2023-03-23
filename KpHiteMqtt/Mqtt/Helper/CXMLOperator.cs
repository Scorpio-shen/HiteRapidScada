using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace KpHiteMqtt.Mqtt.Helper
{
    public class CXMLOperator
    {
        public void SerializeToXml(string path, object obj)
        {
            string xml = SerializeToXml(obj);
            if (xml != null)
            {
                FileInfo file = new FileInfo(path);

                if (!file.Directory.Exists) file.Directory.Create();

                File.WriteAllText(path, xml);
            }
        }
        public string SerializeToXml(object pObj)
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(pObj.GetType());

                StringWriter w = new StringWriter();

                serializer.Serialize(w, pObj);

                return w.GetStringBuilder().ToString();
            }
            catch (Exception ex)
            {
                return null;
            }

        }
        public T DeserializeFromXml<T>(string path)
        {
            return (T)DeserializeFromXml(path, typeof(T));
        }
        public object DeserializeFromXml(string path, Type type)
        {
            return DeserializeFromXmlContent(File.ReadAllText(path), type);
        }
        public object DeserializeFromXmlContent(string content, Type type)
        {
            StringReader reader = new StringReader(content);
            XmlSerializer ser = new XmlSerializer(type);

            return ser.Deserialize(reader);
        }

        //Debug路径下
        public T GetConfig<T>() where T : class
        {
            return GetConfig(typeof(T)) as T;
        }

        public T GetConfig<T>(string fileName) where T : class
        {
            return GetConfig(typeof(T), fileName) as T;
        }
        public object GetConfig(Type type)
        {
            return GetConfig(type, GetConfigDefaultFileName(type));
        }
        private string GetConfigDefaultFileName(Type type)
        {
            string fileName = Application.StartupPath + "\\" + type.Name + ".xml";

            return fileName;
        }
        public object GetConfig(Type type, string fileName)
        {
            object config = null;
            if (File.Exists(fileName))
            {
                config = DeserializeFromXml(fileName, type);
            }
            if (config == null) config = Activator.CreateInstance(type);

            return config;
        }
        public void SetConfig(object config)
        {
            if (config == null) return;

            SetConfig(config, GetConfigDefaultFileName(config.GetType()));
        }
        public void SetConfig(object config, string fileName)
        {
            if (config == null) return;

            SerializeToXml(fileName, config);

        }

    }
}
