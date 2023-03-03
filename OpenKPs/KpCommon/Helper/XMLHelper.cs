using System;
using System.Data;
using System.IO;
using System.Xml;

namespace KpCommon.Helper
{
    public class XMLHelper
    {
        /// <summary>
        /// 将xml转为Datable
        /// </summary>
        public static DataTable XmlToDataTable(string xmlStr)
        {
            if (!string.IsNullOrEmpty(xmlStr))
            {
                StringReader StrStream = null;
                XmlTextReader Xmlrdr = null;
                try
                {
                    DataSet ds = new DataSet();
                    //读取字符串中的信息  
                    StrStream = new StringReader(xmlStr);
                    //获取StrStream中的数据  
                    Xmlrdr = new XmlTextReader(StrStream);
                    //ds获取Xmlrdr中的数据                 
                    ds.ReadXml(Xmlrdr);
                    return ds.Tables[0];
                }
                catch (Exception e)
                {
                    return null;
                }
                finally
                {
                    //释放资源  
                    if (Xmlrdr != null)
                    {
                        Xmlrdr.Close();
                        StrStream.Close();
                        StrStream.Dispose();
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// 将datatable转为xml 
        /// </summary>
        public static void DataTableToXml(DataTable vTable,string savePath)
        {
            if (!Directory.Exists(savePath))
            {
                Directory.CreateDirectory(savePath);
            }
            string xml = savePath + @"\编组信息表.xml";
            //如果文件DataTable.xml存在则直接删除
            if (File.Exists(xml))
            {
                File.Delete(xml);
            }
            vTable.WriteXml(savePath + @"\编组信息表.xml");
        }
    }
}
