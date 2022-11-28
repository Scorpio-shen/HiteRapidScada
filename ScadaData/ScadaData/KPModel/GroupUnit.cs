using Scada.KPModel.InterFace;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Scada.KPModel
{
    public abstract class GroupUnit<T> where T : class,IDataUnit
    {
        #region 属性
        /// <summary>
        /// 唯一ID
        /// </summary>
        //public int ID { get; set; }
        /// <summary>
        /// 点名
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 起始地址
        /// </summary>
        public virtual double StartAddress
        {
            get
            {
                double address = default;
                if (Tags?.Count > 0)
                {
                    var tag = Tags.FirstOrDefault();
                    if (!string.IsNullOrEmpty(tag.Address))
                        double.TryParse(tag.Address, out address);
                }

                return address;
            }
        }


        /// <summary>
        /// 是否激活
        /// </summary>
        public bool Active { get; set; }

        /// <summary>
        /// DB块号
        /// </summary>
        public int DBNum { get; set; } = 1;

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
        public abstract double MaxAddressLength { get; set; }
        #endregion

        #region 抽象或虚方法

        #region 存储载入XML
        /// <summary>
        /// 存储
        /// </summary>
        /// <param name="xmlElement"></param>
        public abstract void SaveToXml(XmlElement xmlElement);
        /// <summary>
        /// 载入
        /// </summary>
        /// <param name="xmlElement"></param>
        public abstract void LoadFromXml(XmlElement xmlElement);
        #endregion

        #region 地址与索引刷新
        /// <summary>
        /// 刷新测点地址
        /// </summary>
        public virtual void RefreshTagAddress()
        {
            if (StartKpTagIndex >= 1)
            {
                for (int i = 0; i < Tags.Count; i++)
                {
                    var tag = Tags[i];
                    tag.TagID = StartKpTagIndex + i;
                }
            }
        }
        /// <summary>
        /// 刷新测点ID
        /// </summary>
        public virtual void RefreshTagIndex()
        {
            if (Tags == null || Tags.Count == 0)
                return;
            SortTags();
        }

        /// <summary>
        /// 对测点进行排序
        /// </summary>
        private void SortTags()
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
        public abstract bool CheckAndAddTags(List<T> addTags, bool needClear = false);
        #endregion

        #endregion
    }
}
