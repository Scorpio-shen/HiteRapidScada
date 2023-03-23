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
        [DisplayName("数组索引")]
        public int ArrayIndex { get; set; }
        /// <summary>
        /// 输入通道号
        /// </summary>
        [DisplayName("输入通道号")]
        public string InCnlNum { get; set; }
        /// <summary>
        /// 输出通道号
        /// </summary>
        [DisplayName("输出通道号")]
        public string CtrlCnlNum { get;set; }
    }
}
