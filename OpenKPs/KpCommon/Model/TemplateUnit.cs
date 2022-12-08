using KpCommon.InterFace;
using Scada;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace KpCommon.Model
{
    public abstract class TemplateUnit<G, D, C> : ITemplateUnit<G, D, C> where G : IGroupUnit<D> where D : class, IDataUnit where C : IConnectionUnit, new()
    {

        #region 属性
        /// <summary>
        /// 通讯点集合
        /// </summary>
        public List<G> TagGroups { get; set; }
        /// <summary>
        /// 连接配置
        /// </summary>
        public C ConnectionOptions { get; set; }
        /// <summary>
        /// 指令点数目
        /// </summary>
        public int CmdTagCount
        {
            get => TagGroups.SelectMany(t => t.Tags).Where(t => t.CanWrite > 0).Count();
        }

        /// <summary>
        /// 可以下发指令测点
        /// </summary>
        public List<D> CmdTags
        {
            get => TagGroups.SelectMany(t => t.Tags).Where(t => t.CanWrite > 0).OrderBy(t => t.TagID).ToList();
        }
        #endregion


        public TemplateUnit()
        {
            TagGroups = new List<G>();
            ConnectionOptions = new C();
        }

        #region 保存载入xml
        protected abstract void LoadFromXml(XmlNode rootNode);

        protected abstract void SaveToXml(XmlElement rootElement);
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
                errMsg = $"DeviceTemplate_Save,保存模板异常,{ex.Message}";
                return false;
            }
        }
        #endregion
        /// <summary>
        /// 刷新所有组以及组内每个Tag的Index
        /// </summary>
        public virtual void RefreshTagGroupIndex()
        {
            int startIndex = 1;
            TagGroups.ForEach(t =>
            {
                t.StartKpTagIndex = startIndex;
                t.SortTags();
                startIndex += t.TagCount;
            });
        }
        /// <summary>
        /// 获取Active TagGroup集合
        /// </summary>
        /// <returns></returns>
        public virtual List<G> GetActiveTagGroups()
        {
            var result = new List<G>();
            foreach (var tagGroup in TagGroups)
                if (tagGroup.Active)
                    result.Add(tagGroup);

            return result;
        }
        /// <summary>
        /// 根据cmdNumber返回对应Tag点
        /// </summary>
        /// <param name="cmdNumber"></param>
        /// <param name="tagGroup"></param>
        /// <returns></returns>
        public virtual D FindCmd(int cmdNumber,out G tagGroup)
        {
            tagGroup = default;
            D result = null;
            foreach (var tg in TagGroups)
            {
                foreach (var tag in tg.Tags)
                {
                    if (tag.TagID == cmdNumber)
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
