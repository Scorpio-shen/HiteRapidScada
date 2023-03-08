using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KpHiteMqtt.Mqtt.Model
{
    public class ArrayChannelModel
    {
        /// <summary>
        /// 数组索引
        /// </summary>
        [Description("数组索引")]
        public int ArrayIndex { get; set; }
        /// <summary>
        /// 输入通道号
        /// </summary>
        [Description("输入通道号")]
        public int InCnlNum { get; set; }
        /// <summary>
        /// 输出通道号
        /// </summary>
        [Description("输出通道号")]
        public int CtrlCnlNum { get;set; }
    }
}
