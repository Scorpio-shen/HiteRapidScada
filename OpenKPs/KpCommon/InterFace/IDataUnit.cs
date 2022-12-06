using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KpCommon.InterFace
{
    public interface IDataUnit : IComparable<IDataUnit>, INotifyPropertyChanged, ICloneable
    {
        #region 属性
        /// <summary>
        /// TagGroup中序号，组中唯一，按顺序排列
        /// </summary>
        int TagID { get; set; }

        /// <summary>
        /// 点名
        /// </summary>
        string Name { get; set; }

        string DataTypeDesc { get; set; }

        /// <summary>
        /// 地址
        /// </summary>

        string Address { get; set; }

        /// <summary>
        /// 数据类型是string或其他数组类型加上长度
        /// </summary>
        int Length { get; set; }

        /// <summary>
        /// 是否支持写入(0只可读,1只可写，2可读可写)
        /// </summary>
        byte CanWrite { get; set; }
        #endregion

        #region 方法
        void OnPropertyChanged(string proName);
        void SetCmdData(double cmdVal);
        #endregion
    }
}
