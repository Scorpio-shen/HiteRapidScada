using KpCommon.Extend;
using KpCommon.InterFace;
using NPOI.SS.Formula.Functions;
using Scada;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Xml;

namespace KpCommon.Model
{
    public abstract class GroupUnit<T>: IGroupUnit<T> where T : class,IDataUnit
    {
        #region 属性
        /// <summary>
        /// 唯一ID
        /// </summary>
        //public int ID { get; set; }
        /// <summary>
        /// 点名
        /// </summary>
        private string name;
        public string Name
        {
            get => name;
            set
            {
                name = value;
                OnPropertyChanged(nameof(Name));
            }
        }
        /// <summary>
        /// 起始地址
        /// </summary>
        public virtual int StartAddress
        {
            get
            {
                int address = default;
                if (Tags?.Count > 0)
                {
                    var tag = Tags.FirstOrDefault();
                    if (!string.IsNullOrEmpty(tag.Address))
                        int.TryParse(tag.Address, out address);
                }

                return address;
            }
        }


        /// <summary>
        /// 是否激活
        /// </summary>
        private bool active;
        public bool Active
        {
            get => active;
            set
            {
                active = value;
                OnPropertyChanged(nameof(Active));
            }
        }

        private int dbnum = 1;
        /// <summary>
        /// DB块号
        /// </summary>
        public int DBNum
        {
            get => dbnum;
            set
            {
                dbnum = value;
                OnPropertyChanged(nameof(DBNum));
            }
        }

        /// <summary>
        /// 起始点在所有Group包含的Tag的起始索引
        /// </summary>
        public int StartKpTagIndex { get; set; }

        /// <summary>
        /// 测点集合
        /// </summary>
        public List<T> Tags { get; set; }
        /// <summary>
        /// 测点数
        /// </summary>
        public int TagCount
        {
            get => Tags?.Count ?? default;
        }
        /// <summary>
        /// 存储每次读取到的数据
        /// </summary>
        public byte[] Data { get; set; }
        /// <summary>
        /// 最大地址长度（限制配置点时防止超出最大地址长度）
        /// </summary>
        public abstract int MaxRequestByteLength { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string proName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(proName));
        }
        #endregion

        #region 抽象或虚方法

        #region 存储载入XML
        /// <summary>
        /// 存储
        /// </summary>
        /// <param name="xmlElement"></param>
        public virtual void SaveToXml(XmlElement tagGroupElement)
        {
            if (tagGroupElement == null)
                throw new ArgumentNullException("TagGroupElement");

            foreach (var p in GetType().GetProperties())
            {
                if(!p.CanWrite)
                    continue;
                //p.SetValue(this, p.GetValue(this, null), null);
                if (p.Name.Equals(nameof(Tags)))
                {
                    SaveTagsToXml(tagGroupElement, Tags);
                    continue;
                }
                tagGroupElement.SetAttribute(p.Name,p.GetValue(this, null));
            }
        }
        /// <summary>
        /// 存储Tags集合
        /// </summary>
        /// <param name="tagGroupElement"></param>
        /// <param name="tags"></param>
        public virtual void SaveTagsToXml(XmlElement tagGroupElement, List<T> tags)
        {
            foreach (var tag in tags)
            {
                XmlElement tagElem = tagGroupElement.AppendElem("Tag");
                foreach (var tagProperty in tag.GetType().GetProperties())
                {
                    if (!tagProperty.CanWrite)
                        continue;
                    tagElem.SetAttribute(tagProperty.Name, tagProperty.GetValue(tag));
                }
            }
        }
        /// <summary>
        /// 载入
        /// </summary>
        /// <param name="xmlElement"></param>
        public virtual void LoadFromXml(XmlElement tagGroupElem)
        {
            if (tagGroupElem == null)
                throw new ArgumentNullException("TagGroupElement");

            foreach (var p in GetType().GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance))
            {
                if (!p.CanWrite)
                    continue;
                if(p.Name.Equals(nameof(Tags)))
                {
                    LoadTagsFromXml(tagGroupElem);
                    continue;
                }

                if (p.PropertyType == typeof(bool))
                {
                    p.SetValue(this, tagGroupElem.GetAttrAsBool(p.Name), null);
                }
                else if (p.PropertyType == typeof(byte))
                {
                    p.SetValue(this, tagGroupElem.GetAttrAsByte(p.Name), null);
                }
                else if (p.PropertyType == typeof(string))
                {
                    p.SetValue(this, tagGroupElem.GetAttrAsString(p.Name), null);
                }
                else if (p.PropertyType == typeof(int))
                {
                    p.SetValue(this, tagGroupElem.GetAttrAsInt(p.Name), null);
                }
                else if (p.PropertyType.IsEnum)
                {
                    try
                    {
                        var enumValue = Enum.Parse(p.PropertyType, tagGroupElem.GetAttrAsString(p.Name), true);
                        p.SetValue(this, enumValue, null);
                    }
                    catch
                    {

                    }
                }
            }
        }

        public virtual void LoadTagsFromXml(XmlElement tagGroupElem)
        {
            XmlNodeList tagNodes = tagGroupElem.SelectNodes("Tag");
            foreach(XmlElement tagElem in tagNodes)
            {
                T t = Activator.CreateInstance(typeof(T)) as T;
                foreach (var p in typeof(T).GetProperties())
                {
                    if (!p.CanWrite)
                        continue;
                    if (t == null)
                        return;

                    if (p.PropertyType == typeof(bool))
                    {
                        p.SetValue(t, tagElem.GetAttrAsBool(p.Name), null);
                    }
                    else if (p.PropertyType == typeof(byte))
                    {
                        p.SetValue(t, tagElem.GetAttrAsByte(p.Name), null);
                    }
                    else if (p.PropertyType == typeof(string))
                    {
                        p.SetValue(t, tagElem.GetAttrAsString(p.Name), null);
                    }
                    else if (p.PropertyType == typeof(int))
                    {
                        p.SetValue(t, tagElem.GetAttrAsInt(p.Name), null);
                    }
                    else if (p.PropertyType.IsEnum)
                    {
                        try
                        {
                            var enumValue = Enum.Parse(p.PropertyType, tagElem.GetAttrAsString(p.Name), true);
                            p.SetValue(t, enumValue, null);
                        }
                        catch
                        {

                        }
                    }
                    
                }

                Tags.Add(t);
            }
        }
        #endregion

        #region 地址与索引刷新
        /// <summary>
        /// 刷新测点地址
        /// </summary>
        public virtual void RefreshTagIndex()
        {
            if (StartKpTagIndex >= 1)
            {
                for (int i = 0; i < Tags.Count; i++)
                {
                    var tag = Tags[i];
                    tag.TagID = StartKpTagIndex + i;
                }
            }

            OnPropertyChanged(nameof(TagCount));
        }
        /// <summary>
        /// 刷新测点ID
        /// </summary>
        public virtual void SortTags()
        {
            if (Tags?.Count > 0)
                Tags.Sort();
        }
        #endregion

        #region 批量添加点集合
        /// <summary>
        /// 批量添加Tags集合到当前group对象Tags集合中，超出了最大请求地址限制,则恢复成原有集合
        /// </summary>
        /// <param name="addTags"></param>
        public abstract bool CheckAndAddTags(List<T> addTags,out string errogMsg, bool needClear = false);
        #endregion

        #endregion
    }
}
