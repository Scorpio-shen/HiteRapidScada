using KpAllenBrandly.Extend;
using KpAllenBrandly.Model.EnumType;
using KpCommon.Extend;
using KpCommon.Model;
using Scada;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace KpAllenBrandly.Model
{
    public class TagGroup : GroupUnit<Tag>
    {
        public int MaxTagCount = ushort.MaxValue;

        #region 构造函数
        public TagGroup()
        {
            Tags = new List<Tag>();
            ParentTags= new List<Tag>();
            Active = true;
            StartKpTagIndex = -1;
        }

        //public TagGroup(MemoryTypeEnum registerType)
        //{
        //    Tags = new List<Tag>();
        //    MemoryType = registerType;
        //    Active = true;
        //    StartKpTagIndex = -1;
        //}
        #endregion

        #region 属性
        /// <summary>
        /// 是否全部支持写入
        /// </summary>
        public bool AllCanWrite
        {
            get => Tags?.Count > 0 && Tags.All(t => t.CanWrite > 0);
        }

        //private MemoryTypeEnum memoryType;
        ///// <summary>
        ///// 寄存器类型
        ///// </summary>
        //public MemoryTypeEnum MemoryType
        //{
        //    get=>memoryType;
        //    set
        //    {
        //        memoryType = value;
        //        SetMemoryType(memoryType);
        //        OnPropertyChanged(nameof(MemoryType));
        //    }
        //}
        //private void SetMemoryType(MemoryTypeEnum registerType)=> Tags.ForEach(t => t.MemoryType = memoryType);
        private int maxrequestbytelength;

        /// <summary>
        /// 限制一组Tag最大请求数据字节长度
        /// </summary>
        public override int MaxRequestByteLength
        {
            get
            {
                if(maxrequestbytelength <= 0)
                    maxrequestbytelength = TagGroupDefaultValues.MaxAddressRequestLength;
                return maxrequestbytelength;
            }
            set => maxrequestbytelength = value;
        }

        //private int requestlength;
        ///// <summary>
        ///// 请求寄存器个数
        ///// </summary>
        //public int RequestLength
        //{
        //    get => requestlength;
        //    set=> requestlength = value;
        //}

        /// <summary>
        /// 存储数组类型Tag的父Tag
        /// </summary>
        public List<Tag> ParentTags { get; set; }
        #endregion

        #region 载入存储Xml
        public override void SaveToXml(XmlElement tagGroupElement)
        {
            base.SaveToXml(tagGroupElement);
            //存储ParentTag
            foreach(var tag in ParentTags)
            {
                XmlElement tagElem = tagGroupElement.AppendElem("ParentTags");
                foreach (var tagProperty in tag.GetType().GetProperties())
                {
                    if (!tagProperty.CanWrite)
                        continue;
                    tagElem.SetAttribute(tagProperty.Name, tagProperty.GetValue(tag));
                }
            }
        }

        public override void SaveTagsToXml(XmlElement tagGroupElement, List<Tag> tags)
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
                //添加ParentTagId属性
                tagElem.SetAttribute("ParentTagID", tag.ParentTag?.TagID ?? -1);
            }
        }

        public override void LoadFromXml(XmlElement tagGroupElem)
        {
            XmlNodeList tagNodes = tagGroupElem.SelectNodes("ParentTags");
            foreach (XmlElement tagElem in tagNodes)
            {
                Tag t = Activator.CreateInstance(typeof(Tag)) as Tag;
                foreach (var p in typeof(Tag).GetProperties())
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

                ParentTags.Add(t);
            }
            base.LoadFromXml(tagGroupElem);
            
        }

        public override void LoadTagsFromXml(XmlElement tagGroupElem)
        {
            XmlNodeList tagNodes = tagGroupElem.SelectNodes("Tag");
            foreach (XmlElement tagElem in tagNodes)
            {
                Tag t = Activator.CreateInstance(typeof(Tag)) as Tag;
                foreach (var p in typeof(Tag).GetProperties())
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

                //获取父Tag
                var parentId = tagElem.GetAttrAsInt("ParentTagID");
                var parentTag = ParentTags.FirstOrDefault(p=>p.TagID == parentId);
                if (parentTag != null)
                {
                    t.ParentTag = parentTag;
                }
                Tags.Add(t);
            }
        }
        #endregion

        #region 批量添加点集合
        public override bool CheckAndAddTags(List<Tag> addTags, out string errorMsg, bool needClear = false)
        {
            errorMsg= string.Empty;
            //将原有对象先拷贝
            bool result = true;
            var tagsOld = new List<Tag>();
            tagsOld.AddRange(Tags.Select(t => t.Clone() as Tag));

            if (needClear)
                Tags.Clear();
            //判断是否有重复的点名
            foreach(var add in addTags)
            {
                if (Tags.Any(t => t.Name.Equals(add.Name)))
                {
                    Tags.Clear();
                    Tags.AddRange(tagsOld);
                    errorMsg = $"名称:{add.Name}重复!";
                    result = false;
                    RefreshTagIndex();
                    return result;
                }
            }
            Tags.AddRange(addTags);
            //SortTags();
            //验证是否超出最大点数限制
            if ((TagCount + StartKpTagIndex > DefineReadOnlyValues.MaxTagCount))
            {
                Tags.Clear();
                Tags.AddRange(tagsOld);
                errorMsg = "超出点数限制!";
                result = false;
                RefreshTagIndex();
                return result;
            }
            var model = GetRequestModel();
            int byteCount = default;
            //验证是否超出最大地址限制
            if (byteCount > MaxRequestByteLength)
            {
                Tags.Clear();
                Tags.AddRange(tagsOld);
                errorMsg = "数据超出范围!";
                result = false;
                RefreshTagIndex(); 
                return result;
            }
            RefreshTagIndex();
            OnPropertyChanged(nameof(TagCount));
            return result;
        }

        public void RefreshParentTagId()
        {
            int tagId = 1;
            foreach(var tag in ParentTags)
            {
                tag.TagID = tagId;
                tagId++;
            }
        }

        public ABRequestModel GetRequestModel()
        {
            var model = new ABRequestModel();
            List<string> addresses = new List<string>();
            List<int> lengths = new List<int>();
            List<int> byteCounts = new List<int>();


            model.Addresses = addresses;
            model.Lengths = lengths;
            model.BytesCount= byteCounts;

            foreach(var tag in Tags)
            {
                if(tag.IsArray)
                {
                    var parentTag = tag.ParentTag;
                    //获取tag的Parent
                    if(parentTag != null && !addresses.Any(a=>a.Equals(parentTag.Name)))
                    {
                        addresses.Add(parentTag.Name);
                        lengths.Add(parentTag.Length);
                        //获取对应字节数
                        int byteCount = default;
                        switch(parentTag.DataType)
                        {
                            case DataTypeEnum.Bool:
                                {
                                    byteCount += parentTag.Length / 8;
                                    if ((parentTag.Length % 8) > 0)
                                        byteCount++;
                                }
                                break;
                            default:
                                byteCount = parentTag.DataType.GetByteCount() * parentTag.Length;
                                break;
                        }

                        byteCounts.Add(byteCount);
                    }
                }
                else
                {
                    if (!addresses.Any(a => a.Equals(tag.Name)))
                    {
                        addresses.Add(tag.Name);
                        lengths.Add(1);
                        int byteCount = default;    
                        switch(tag.DataType)
                        {
                            case DataTypeEnum.String:
                                {
                                    byteCount = 2+ 4 + tag.Length;
                                }
                                break;
                            default:
                                byteCount = tag.DataType.GetByteCount();
                                break;
                        }

                        byteCounts.Add(byteCount);
                    }
                }
            }

            return model;   
        }

        #endregion

        #region 设置Tag是否支持写入
        public void SetTagCanWrite(bool canwrite)
        {
            Tags.ForEach(t => t.CanWriteBool = canwrite);
        }
        #endregion


        /// <summary>
        /// 
        /// </summary>
        /// <param name="index">tag索引</param>
        /// <param name="byteIndex">字节索引</param>
        /// <returns></returns>
        //public double? GetTagVal(int index,ref int byteIndex)
        //{
        //    double? result = null;
        //    try
        //    {
        //        if (TagCount == 0 || index >= TagCount)
        //            return result;
        //        var tag = Tags[index];
        //        if(tag == null) 
        //            return result;

        //        if(Data == null || Data.Length == 0) 
        //            return result;

        //        if (byteIndex >= Data.Length)
        //            return result;

        //        if (tag.IsArray)
        //        {
                    
        //        }
        //        else
        //        {
        //            var byteCount = tag.DataType.GetByteCount();
        //            if(Data.Length < byteIndex + byteCount)
        //                return result;

        //            byte[] buf = Data.Skip(byteIndex).Take(byteCount).ToArray();
        //            switch(tag.DataType)
        //            {
        //                case DataTypeEnum.Bool:

        //                    break;
        //            }
        //        }
        //    }
        //}
    }
}
