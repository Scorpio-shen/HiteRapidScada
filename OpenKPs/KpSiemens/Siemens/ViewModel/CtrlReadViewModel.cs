using KpSiemens.Siemens.Model;
using System.ComponentModel;

namespace KpSiemens.Siemens.ViewModel
{
    /// <summary>
    /// ViewModel
    /// </summary>
    public class CtrlReadViewModel : INotifyPropertyChanged
    {
        private string groupname = string.Empty;
        /// <summary>
        /// 点组名称
        /// </summary>
        public string GroupName
        {
            get => groupname;
            set
            {
                groupname = value;
                OnPropertyChanged(nameof(GroupName));
            }
        }
        private string startaddress;
        /// <summary>
        /// 起始地址
        /// </summary>
        public string StartAddress
        {
            get => startaddress;
            set
            {
                startaddress = value;
                OnPropertyChanged(nameof(StartAddress));
            }
        }

        private bool active;
        /// <summary>
        /// 是否激活
        /// </summary>
        public bool Active
        {
            get => active;
            set
            {
                active = value;
                OnPropertyChanged(nameof(Active));
            }
        }

        private MemoryTypeEnum memoryType;
        /// <summary>
        /// 存储器类型
        /// </summary>
        public MemoryTypeEnum MemoryType
        {
            get => memoryType;
            set
            {
                memoryType = value;
                OnPropertyChanged(nameof(MemoryType));
            }
        }

        private int tagCount;
        /// <summary>
        /// 测点数
        /// </summary>
        public int TagCount
        {
            get => tagCount;
            set
            {
                tagCount = value;
                OnPropertyChanged(nameof(TagCount));
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

        private bool dbnumvisable;
        /// <summary>
        /// DB块号可见性
        /// </summary>
        public bool DBNumVisable
        {
            get => dbnumvisable;
            set
            {
                dbnumvisable = value;
                OnPropertyChanged(nameof(DBNumVisable));
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string proName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(proName));
        }
    }
}
