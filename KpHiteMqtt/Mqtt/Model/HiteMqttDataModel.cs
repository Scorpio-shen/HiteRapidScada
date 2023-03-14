using KpHiteMqtt.Mqtt.Model.Enum;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KpHiteMqtt.Mqtt.Model
{
    /// <summary>
    /// Mqtt数据模型
    /// </summary>
    public class HiteMqttPayload
    {
        public string MessageId { get; set; }
        public string Content { get; set; }
        public string Time { get; set; }
    }
    /// <summary>
    /// Mqtt物模型数据内容
    /// </summary>
    public class MqttModelContent
    {
        /// <summary>
        /// 是否是只读
        /// </summary>
        [JsonIgnore]
        public bool IsReadOnly { get; set; }
        /// <summary>
        /// 输入通道号
        /// </summary>
        [JsonIgnore]
        public int? InCnlNum { get; set; } = null;
        /// <summary>
        /// 输出通道号
        /// </summary>
        [JsonIgnore]
        public int? CtrlCnlNum { get; set; } = null;
        /// <summary>
        /// Id
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 标识符
        /// </summary>
        public string Identifier { get; set; }
        /// <summary>
        /// 数据值
        /// </summary>
        public string Value { get; set; }

        public string DataType
        {
            get => DataTypeValue.ToString();
        }
        /// <summary>
        /// 存储实际类型值
        /// </summary>
        [JsonIgnore]
        public DataTypeEnum DataTypeValue { get; set; }
        public List<MqttModelSpecs> Parameters { get; set; }
    }
    /// <summary>
    /// Mqtt物模型Json参数
    /// </summary>
    public class MqttModelSpecs
    {
        [JsonIgnore]
        public int? InCnlNum { get; set; } = null;

        [JsonIgnore]
        public int? CtrlCnlNum { get; set; } = null;
        public string ParameterName { get; set; }
        public string Identifier { get; set; }
        public string Value { get; set; }
        public string DataType
        {
            get => DataTypeValue.ToString();
        }
        [JsonIgnore]
        public StructDataTypeEnum DataTypeValue { get; set; }
    }


    
}
