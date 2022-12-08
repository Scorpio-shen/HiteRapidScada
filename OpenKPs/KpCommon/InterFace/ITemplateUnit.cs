using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace KpCommon.InterFace
{
    public interface ITemplateUnit<G, D, C> where G : IGroupUnit<D> where D : class, IDataUnit where C : IConnectionUnit, new()
    {
        #region 属性
        /// <summary>
        /// 通讯点集合
        /// </summary>
        List<G> TagGroups { get; set; }
        /// <summary>
        /// 连接配置
        /// </summary>
        C ConnectionOptions { get; set; }
        /// <summary>
        /// 指令点数目
        /// </summary>
        int CmdTagCount { get;}

        /// <summary>
        /// 可以下发指令测点
        /// </summary>
        List<D> CmdTags { get; }
        #endregion

        #region 保存载入xml
        bool Load(string fileName, out string errMsg);

        bool Save(string fileName, out string errMsg);
        #endregion

        /// <summary>
        /// 刷新所有组以及组内每个Tag的Index
        /// </summary>
        void RefreshTagGroupIndex();
        /// <summary>
        /// 获取Active TagGroup集合
        /// </summary>
        /// <returns></returns>
        List<G> GetActiveTagGroups();
        /// <summary>
        /// 根据cmdNumber返回对应Tag点
        /// </summary>
        /// <param name="cmdNumber"></param>
        /// <param name="tagGroup"></param>
        /// <returns></returns>
        D FindCmd(int cmdNumber, out G tagGroup);
    }
}
