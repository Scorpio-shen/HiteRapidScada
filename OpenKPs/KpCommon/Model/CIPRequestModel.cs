using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KpCommon.Model
{
    /// <summary>
    /// 基于CIP协议请求Model
    /// </summary>
    public class CIPRequestModel
    {
        /// <summary>
        /// 请求地址Tag数组
        /// </summary>
        public List<string> Addresses { get; set; }
        /// <summary>
        /// 请求字节长度
        /// </summary>
        public List<ushort> Lengths { get; set; }
    }
}
