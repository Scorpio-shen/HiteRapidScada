using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KpAllenBrandly.Model
{
    public class ABRequestModel
    {
        /// <summary>
        /// 请求地址Tag数组
        /// </summary>
        public string[] Addresses { get; set; }
        /// <summary>
        /// 请求字节长度
        /// </summary>
        public int[] Lengths { get; set; }
    }
}
