using KpHiteMqtt.Mqtt.Model.Enum;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace KpHiteMqtt.Mqtt.Model
{
    /// <summary>
    /// 物模型
    /// </summary>
    public class Property : INotifyPropertyChanged
    {
        private int index;
        public int Index
        {
            get => index;
            set
            {
                index = value;
                OnPropertyChanged(nameof(Index));
            }
        }
        /// <summary>
        /// 名称
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
        private string identifier;
        /// <summary>
        /// 标识符
        /// </summary>
        public string Identifier
        {
            get => identifier;
            set
            {
                identifier = value;
                OnPropertyChanged(nameof(Identifier));
            }
        }

        private string description;
        /// <summary>
        /// 描述
        /// </summary>
        public string Description
        {
            get => description;
            set
            {
                description = value;
                OnPropertyChanged(nameof(Description));
            }
        }

        private DataTypeEnum dataType;
        /// <summary>
        /// 数据类型
        /// </summary>
        public DataTypeEnum DataType
        {
            get => dataType;
            set
            {
                dataType = value;
                OnPropertyChanged(nameof(DataType));
                OnPropertyChanged(nameof(InputChannelVisable));
                OnPropertyChanged(nameof(OutputChannelVisable));
                OnPropertyChanged(nameof(IsStruct));
            }
        }
        private bool isreadonly;
        /// <summary>
        /// 是否是只读
        /// </summary>
        public bool IsReadOnly
        {
            get => isreadonly;
            set
            {
                isreadonly = value;
                OnPropertyChanged(nameof(IsReadOnly));
                OnPropertyChanged(nameof(OutputChannelVisable));
            }
        }
        /// <summary>
        /// 是否是结构体
        /// </summary>
        public bool IsStruct
        {
            get=>DataType == DataTypeEnum.Struct;
        }

        /// <summary>
        /// 输入通道可见性
        /// </summary>
        [JsonIgnore]
        public bool InputChannelVisable
        {
            get
            {
                return DataType == DataTypeEnum.Array || DataType == DataTypeEnum.Struct;
            }
        }
        /// <summary>
        /// 输出通道可见性
        /// </summary>
        [JsonIgnore]
        public bool OutputChannelVisable
        {
            get
            {
                return (DataType == DataTypeEnum.Array || DataType == DataTypeEnum.Struct) && !IsReadOnly;
            }
        }

        private DataArraySpecs arraySpecs = new DataArraySpecs();
        public DataArraySpecs ArraySpecs
        {
            get => arraySpecs;
            set
            {
                arraySpecs = value;
                OnPropertyChanged(nameof(ArraySpecs));
            }
        }

        private List<DataSpecs> dataspecslist = new List<DataSpecs>();
        /// <summary>
        /// Json参数集合
        /// </summary>
        public List<DataSpecs> DataSpecsList
        {
            get => dataspecslist;
            set
            {
                dataspecslist = value;
                OnPropertyChanged(nameof(DataSpecsList));
            }
        }

        private string unit;
        /// <summary>
        /// 单位
        /// </summary>
        public string Unit
        {
            get => unit;
            set
            {
                unit = value;
                OnPropertyChanged(nameof(Unit));
            }
        }

        private int cnlnum;
        /// <summary>
        /// 输入通道Num
        /// </summary>
        public int CnlNum
        {
            get => cnlnum;
            set
            {
                cnlnum = value;
                OnPropertyChanged(nameof(CnlNum));
            }
        }

        private int ctrlcnlnum;
        /// <summary>
        /// 输出通道
        /// </summary>
        public int CtrlCnlNum
        {
            get => ctrlcnlnum;
            set
            {
                ctrlcnlnum = value;
                OnPropertyChanged(nameof(CtrlCnlNum));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string proName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(proName));
        }
    }

    public class DataArraySpecs : INotifyPropertyChanged
    {
        private ArrayDataTypeEnum datatype;
        public ArrayDataTypeEnum DataType
        {
            get => datatype;
            set
            {
                datatype = value;
                OnPropertyChanged(nameof(DataType));
                OnPropertyChanged(nameof(IsStruct));
            }
        }

        public bool IsStruct
        {
            get=>DataType == ArrayDataTypeEnum.Struct;
        }

        private int arraylength;
        /// <summary>
        /// 类型为数组时数组长度
        /// </summary>
        public int ArrayLength
        {
            get => arraylength;
            set
            {
                arraylength = value;
                OnPropertyChanged(nameof(ArrayLength));
            }
        }

        private List<int> incnlums = new List<int>();
        public List<int> InCnlNums
        {
            get => incnlums;
            set
            {
                incnlums = value;
                OnPropertyChanged(nameof(InCnlNums));
            }
        }

        private List<int> ctrlcnlnums = new List<int>();
        public List<int> CtrlCnlNums
        {
            get=> ctrlcnlnums;
            set
            {
                ctrlcnlnums = value;
                OnPropertyChanged(nameof(CtrlCnlNums));
            }
        }

        private List<DataSpecs> dataSpecs = new List<DataSpecs>();
        public List<DataSpecs> DataSpecs
        {
            get => dataSpecs;
            set
            {
                dataSpecs = value;
                OnPropertyChanged(nameof(DataSpecs));
            }
        }

        public string ArrayChannelString
        {
            get
            {
                StringBuilder builder = new StringBuilder();
                for(int i = 0;i < arraylength; i++)
                {
                    builder.Append($"数组索引:{i}");
                    if (i < InCnlNums.Count)
                    {
                        builder.Append($",输入通道:{InCnlNums[i]}");
                    }

                    if(i < CtrlCnlNums.Count)
                    {
                        builder.Append($",输出通道:{CtrlCnlNums[i]}");
                    }

                    builder.Append("\r\n");
                }

                return builder.ToString();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string proName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(proName));
        }

    }
    /// <summary>
    /// Json参数
    /// </summary>
    public class DataSpecs : INotifyPropertyChanged
    {
        private string parametername;
        /// <summary>
        /// 参数名称
        /// </summary>
        public string ParameterName
        {
            get => parametername;
            set
            {
                parametername = value;  
                OnPropertyChanged(nameof(ParameterName));
            }
        }

        private string identifier;
        /// <summary>
        /// 标识符
        /// </summary>
        public string Identifier
        {
            get => identifier;
            set
            {
                identifier = value;
                OnPropertyChanged(nameof(Identifier));
            }
        }

        private StructDataTypeEnum datatype;
        /// <summary>
        /// Struct数据类型
        /// </summary>
        public StructDataTypeEnum DataType
        {
            get => datatype;
            set
            {
                datatype = value;
                OnPropertyChanged(nameof(DataType));
            }
        }

        private int cnlnum;
        /// <summary>
        /// 输入通道Num
        /// </summary>
        public int CnlNum
        {
            get => cnlnum;
            set
            {
                cnlnum = value;
                OnPropertyChanged(nameof(CnlNum));
            }
        }

        private int ctrlcnlnum;
        /// <summary>
        /// 输出通道
        /// </summary>
        public int CtrlCnlNum
        {
            get => ctrlcnlnum;
            set
            {
                ctrlcnlnum = value;
                OnPropertyChanged(nameof(CtrlCnlNum));
            }
        }

        private string unit;
        /// <summary>
        /// 单位
        /// </summary>
        public string Unit
        {
            get => unit;
            set
            {
                unit = value;
                OnPropertyChanged(nameof(Unit));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string proName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(proName));
        }
    }
}
