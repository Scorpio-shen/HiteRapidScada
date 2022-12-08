using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KpCommon.Model
{
    /// <summary>
    /// 定义KpModel中各种ReadOnly值
    /// </summary>
    public class DefineReadOnlyValues
    {
        /// <summary>
        /// KpModels中一个Device中最大能定义的点数（超过该数,数据传输会有问题，转换成的byte字节数超过两个字节所能表示的最大值65535）
        /// </summary>
        public static readonly int MaxTagCount = 1200;
        /// <summary>
        /// 读取数据时默认超时时间(毫秒)
        /// </summary>
        public static readonly int DefaultRequestTimeOut = 2000;
        /// <summary>
        /// 连接超时（毫秒）
        /// </summary>
        public static readonly int DefaultConnectTimeOut = 3000;
    }
}
