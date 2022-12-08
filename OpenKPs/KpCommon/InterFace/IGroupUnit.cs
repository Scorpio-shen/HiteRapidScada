using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Xml;

namespace KpCommon.InterFace
{
    public interface IGroupUnit<T>:INotifyPropertyChanged where T : class,IDataUnit 
    {
        #region 属性
        /// <summary>
        /// 点名
        /// </summary>
        string Name { get; set; }
        /// <summary>
        /// 起始地址
        /// </summary>
        int StartAddress { get; }

        /// <summary>
        /// 是否激活
        /// </summary>
        bool Active { get; set; }
        /// <summary>
        /// DB块号
        /// </summary>
        int DBNum { get; set; }
        /// <summary>
        /// 起始点在所有Group包含的Tag的起始索引
        /// </summary>
        int StartKpTagIndex { get; set; }
        /// <summary>
        /// 测点集合
        /// </summary>
        List<T> Tags { get; set; }
        /// <summary>
        /// 测点数
        /// </summary>
        int TagCount { get; }
        /// <summary>
        /// 存储每次读取到的数据
        /// </summary>
        byte[] Data { get; set; }
        /// <summary>
        /// 最大地址长度（限制配置点时防止超出最大地址长度）
        /// </summary>
        double MaxRequestByteLength { get; set; }
        #endregion

        #region 方法
        /// <summary>
        /// 通知属性变更
        /// </summary>
        /// <param name="proName"></param>
        void OnPropertyChanged(string proName);

        #region 存储载入XML
        /// <summary>
        /// 存储
        /// </summary>
        void SaveToXml(XmlElement xmlElement);
        /// <summary>
        /// 载入
        /// </summary>
        /// <param name="xmlElement"></param>
        void LoadFromXml(XmlElement xmlElement);
        #endregion

        #region 地址与索引刷新
        /// <summary>
        /// 刷新测点地址
        /// </summary>
        void RefreshTagIndex();
        /// <summary>
        /// 刷新测点ID
        /// </summary>
        void SortTags();
        #endregion

        #region 批量添加点集合
        /// <summary>
        /// 批量添加Tags集合到当前group对象Tags集合中，超出了最大请求地址限制,则恢复成原有集合
        /// </summary>
        /// <param name="addTags"></param>
        bool CheckAndAddTags(List<T> addTags, out string errogMsg, bool needClear = false);
        #endregion
        #endregion


    }
}
