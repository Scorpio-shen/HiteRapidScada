using Scada.KPModel.Attributes;
using Scada.KPModel.InterFace;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scada.KPModel
{
    /// <summary>
    /// 通讯点最小单元
    /// </summary>
    public abstract class DataUnit : IDataUnit
    {
        #region 属性
        private int tagid;
        /// <summary>
        /// TagGroup中序号，组中唯一，按顺序排列
        /// </summary>
        [DisplayName("序号")]
        [ExcelHeaderSort(0)]
        public int TagID
        {
            get => tagid;
            set
            {
                tagid = value;
                OnPropertyChanged(nameof(TagID));
            }
        }

        private string name;
        /// <summary>
        /// 点名
        /// </summary>
        [DisplayName("名称")]
        [ExcelHeaderSort(1)]
        public string Name
        {
            get => name;
            set
            {
                name = value;
                OnPropertyChanged(Name);
            }
        }

        //[DisplayName("数据类型")]
        //[ExcelHeaderSort(2)]
        public abstract string DataTypeDesc { get; set; }

        private string address;
        /// <summary>
        /// 地址
        /// </summary>
        [DisplayName("地址")]
        [ExcelHeaderSort(3)]

        public string Address
        {
            get => address;
            set
            {
                if (double.TryParse(value, out double valueAddress))
                {
                    if (valueAddress >= 0d)
                    {
                        address = valueAddress.ToString();
                        OnPropertyChanged(nameof(Address));

                    }
                        
                }
            }
        }

        private int length;
        /// <summary>
        /// 数据类型是string或其他数组类型加上长度
        /// </summary>
        [DisplayName("长度")]
        [ExcelHeaderSort(4)]
        public int Length
        {
            get => length;
            set
            {
                length = value;
                OnPropertyChanged(nameof(Length));
            }
        }

        protected byte canwrite;
        /// <summary>
        /// 是否支持写入(0只可读,1可读可写)
        /// </summary>
        [DisplayName("是否支持写入")]
        [ExcelHeaderSort(5)]
        public virtual byte CanWrite
        {
            get => canwrite;
            set
            {
                canwrite = value;
                OnPropertyChanged(nameof(CanWrite));
            }
        }
        #endregion

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string proName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(proName));
        }

        #region 抽象或者虚方法
        /// <summary>
        /// 无引用对象字段的浅拷贝方式
        /// </summary>
        /// <returns></returns>
        public virtual object Clone()
        {
            return MemberwiseClone();
        }

        public virtual int CompareTo(IDataUnit other)
        {
            if (double.TryParse(Address, out double currentAddress))
            {
                if (double.TryParse(other.Address, out double otherAddress))
                {
                    return currentAddress.CompareTo(otherAddress);
                }
            }

            return 0;
        }

        public abstract void SetCmdData(double cmdVal);
        #endregion
    }
}
