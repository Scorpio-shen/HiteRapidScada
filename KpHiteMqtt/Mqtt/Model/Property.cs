using KpHiteMqtt.Mqtt.Model.Enum;
using Newtonsoft.Json;
using Scada.Data.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
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
        private int id;
        public int Id
        {
            get => id;
            set
            {
                id = value;
                OnPropertyChanged(nameof(Id));
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
                OnPropertyChanged(nameof(IsArray));
            }
        }
        private bool isreadonly;
        /// <summary>
        /// 是否是只读
        /// </summary>
        [JsonIgnore]    
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
        [JsonIgnore]
        public bool IsStruct
        {
            get=>DataType == DataTypeEnum.Struct;
        }
        /// <summary>
        /// 是否是数组类型
        /// </summary>
        [JsonIgnore]
        public bool IsArray
        {
            get => DataType == DataTypeEnum.Array;
        }

        /// <summary>
        /// 输入通道可见性
        /// </summary>
        [JsonIgnore]
        public bool InputChannelVisable
        {
            get
            {
                return DataType != DataTypeEnum.Array && DataType != DataTypeEnum.Struct;
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
                return DataType != DataTypeEnum.Array && DataType != DataTypeEnum.Struct && !IsReadOnly;
            }
        }
        private DataArraySpecs dataArraySpecs = new DataArraySpecs();
        public DataArraySpecs DataArraySpecs
        {
            get => dataArraySpecs;
            set
            {
                dataArraySpecs = value;
                OnPropertyChanged(nameof(DataArraySpecs));
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
        private string identifier;
        public string Identifier
        {
            get=>identifier;
            set
            {
                identifier = value;
                OnPropertyChanged(nameof(Identifier));
                OnPropertyChanged(nameof(ArrayChannelString));
            }
        }
        private ArrayDataTypeEnum datatype;
        public ArrayDataTypeEnum DataType
        {
            get => datatype;
            set
            {
                datatype = value;
                OnPropertyChanged(nameof(DataType));
                OnPropertyChanged(nameof(IsStruct));
                OnPropertyChanged(nameof(ArrayChannelString));
            }
        }
        [JsonIgnore]
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
                //if(arraylength < arraySpecs.Count)
                //{
                //    do
                //    {
                //        arraySpecs.RemoveAt(arraySpecs.Count - 1);

                //    }
                //    while (arraylength < arraySpecs.Count && arraySpecs.Count > 0);
                //}
                //else if(arraylength > arraySpecs.Count)
                //{
                //    var count = arraySpecs.Count;
                //    for(int i = 0;i < arraylength - count; i++)
                //    {
                //        arraySpecs.Add(new Model.ArraySpecs
                //        {
                //            DataSpecs = this.DataSpecs.Select(d => d.Clone() as DataSpecs).ToList(),
                //        });
                //    }
                //}
                OnPropertyChanged(nameof(ArrayLength));
                OnPropertyChanged(nameof(ArrayChannelString));
            }
        }

        private List<ArraySpecs> arraySpecs = new List<ArraySpecs>();
        public List<ArraySpecs> ArraySpecs
        {
            get => arraySpecs;
            set
            {
                arraySpecs = value;
                OnPropertyChanged(nameof(ArraySpecs));
                OnPropertyChanged(nameof(ArrayChannelString));
            }
        }

        private List<DataSpecs> dataSpecs = new List<DataSpecs>();
        /// <summary>
        /// 用于存储数组统一Json参数
        /// </summary>
        [JsonIgnore]
        public List<DataSpecs> DataSpecs
        {
            get => dataSpecs;
            set
            {
                dataSpecs = value;
                OnPropertyChanged(nameof(DataSpecs));
            }
        }

        public void SetArrayDataSpecs()
        {
            foreach (var spec in arraySpecs)
            {
                List<DataSpecs> removeSpecs = new List<DataSpecs>(); //要移除的参数
                List<DataSpecs> addSpecs = new List<DataSpecs>();    //要添加的参数
                                                                     //遍历原有的参数，获取需要移除的参数
                foreach (var olddataSpecs in spec.DataSpecs)
                {
                    if (!dataSpecs.Any(d => d.Identifier.Equals(olddataSpecs.Identifier)))
                    {
                        //需要移除
                        removeSpecs.Add(olddataSpecs);
                    }
                }

                foreach (var newdataSpecs in dataSpecs)
                {
                    if (!spec.DataSpecs.Any(d => d.Identifier.Equals(newdataSpecs.Identifier)))
                    {
                        //需要添加
                        addSpecs.Add(newdataSpecs.Clone() as DataSpecs);
                    }
                }

                foreach (var re in removeSpecs)
                {
                    spec.DataSpecs.Remove(re);
                }

                spec.DataSpecs.AddRange(addSpecs);
            }
            OnPropertyChanged(nameof(ArrayChannelString));
        }
        [JsonIgnore]
        public string ArrayChannelString
        {
            get => GetArrayChannelString(Identifier);
        }
        private string GetArrayChannelString(string identifier)
        {
            List<ArrayChannelMap> maps = new List<ArrayChannelMap>();
            for (int i = 0; i < arraylength; i++)
            {
                var map = new ArrayChannelMap()
                {
                    Index = i,
                    Identifier = identifier + $"[{i}]",
                };
                if (i < ArraySpecs.Count)
                {
                    var arrayspec = ArraySpecs[i];
                    map.InputChannel = arrayspec.InCnlNum;
                    map.OutputChannel = arrayspec.CtrlCnlNum;

                    if (DataType == ArrayDataTypeEnum.Struct)
                    {
                        
                        map.Parameters = new Dictionary<string, Parameter>();
                        foreach (var specs in arrayspec.DataSpecs)
                        {
                            map.Parameters.Add(specs.Identifier, new Parameter
                            {
                                InputChannel = specs.InCnlNum,
                                OutputChannel = specs.InCnlNum,
                            });
                        }
                    }
                }
                

                maps.Add(map);
            }
            return ConvertJsonString(JsonConvert.SerializeObject(maps));

        }

        private string ConvertJsonString(string str)
        {
            //格式化json字符串
            JsonSerializer serializer = new JsonSerializer();
            TextReader tr = new StringReader(str);
            JsonTextReader jtr = new JsonTextReader(tr);
            object obj = serializer.Deserialize(jtr);
            if (obj != null)
            {
                StringWriter textWriter = new StringWriter();
                JsonTextWriter jsonWriter = new JsonTextWriter(textWriter)
                {
                    Formatting = Formatting.Indented,
                    Indentation = 4,
                    IndentChar = ' '
                };
                serializer.Serialize(jsonWriter, obj);
                return textWriter.ToString();
            }
            else
            {
                return str;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string proName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(proName));
        }

    }

    public class ArraySpecs : INotifyPropertyChanged
    {
        private int incnlnum;
        public int InCnlNum
        {
            get => incnlnum;
            set
            {
                incnlnum = value;
                OnPropertyChanged(nameof(InCnlNum));         
            }
        }

        private int ctrlcnlnum;
        public int CtrlCnlNum
        {
            get=>ctrlcnlnum;
            set
            {
                ctrlcnlnum = value;
                OnPropertyChanged(nameof(CtrlCnlNum));
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

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string proName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(proName));
        }
    }
    /// <summary>
    /// Json参数
    /// </summary>
    public class DataSpecs : INotifyPropertyChanged,ICloneable
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

        private int incnlnum;
        /// <summary>
        /// 输入通道Num
        /// </summary>
        public int InCnlNum
        {
            get => incnlnum;
            set
            {
                incnlnum = value;
                OnPropertyChanged(nameof(InCnlNum));
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

        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}
