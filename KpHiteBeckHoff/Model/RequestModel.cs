using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KpHiteBeckHoff.Model
{
    public class RequestModel
    {
        /// <summary>
        /// 请求地址Tag数组
        /// </summary>
        public List<string> Addresses { get; set; }
        /// <summary>
        /// 请求字节长度
        /// </summary>
        public List<ushort> Lengths { get; set; }

        /// <summary>
        /// 每个Tag占据字节数
        /// </summary>
        //public List<int> BytesCount { get; set; }
    }
}
