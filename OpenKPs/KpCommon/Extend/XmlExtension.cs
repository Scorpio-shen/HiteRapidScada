using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace KpCommon.Extend
{
    public static class XmlExtension
    {
        public static byte GetAttrAsByte(this XmlElement xmlElem, string attrName, byte defaultVal = default)
        {
            try
            {
                return xmlElem.HasAttribute(attrName) ?
                    byte.Parse(xmlElem.GetAttribute(attrName)) : defaultVal;
            }
            catch (FormatException ex)
            {
                throw ex;
            }
        }
    }
}
