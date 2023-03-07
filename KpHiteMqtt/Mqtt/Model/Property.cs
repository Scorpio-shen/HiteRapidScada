using KpHiteMqtt.Mqtt.Model.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KpHiteMqtt.Mqtt.Model
{
    /// <summary>
    /// 物模型
    /// </summary>
    public class Property
    { 
        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 标识符
        /// </summary>
        public string Identifier { get; set; }  
        /// <summary>
        /// 描述
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// 数据类型
        /// </summary>
        public DataTypeEnum DataType { get; set; }
        /// <summary>
        /// 是否是只读
        /// </summary>
        public bool IsReadOnly { get; set; }
        /// <summary>
        /// 类型为数组时数组长度
        /// </summary>
        public int ArrayLength { get; set; }
        /// <summary>
        /// Json参数集合
        /// </summary>
        public List<DataSpecs> DataSpecsList { get; set; }
        /// <summary>
        /// 单位
        /// </summary>
        public string Unit { get; set; }
    }
    /// <summary>
    /// Json参数
    /// </summary>
    public class DataSpecs
    {
        /// <summary>
        /// 参数名称
        /// </summary>
        public string ParameterName { get; set; }
        /// <summary>
        /// 标识符
        /// </summary>
        public string Identifier { get; set; }
        /// <summary>
        /// Struct数据类型
        /// </summary>
        public StructDataTypeEnum DataType { get; set; }    
        /// <summary>
        /// 单位
        /// </summary>
        public string Unit { get; set; }
    }
}
