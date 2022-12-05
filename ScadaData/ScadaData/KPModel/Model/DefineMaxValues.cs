using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scada.KPModel.Model
{
    /// <summary>
    /// 定义KpModel中各种最大值限定
    /// </summary>
    public class DefineMaxValues
    {
        /// <summary>
        /// KpModels中一个Device中最大能定义的点数（超过该数,数据传输会有问题，转换成的byte字节数超过两个字节所能表示的最大值65535）
        /// </summary>
        public static readonly int MaxTagCount = 1200;
    }
}
