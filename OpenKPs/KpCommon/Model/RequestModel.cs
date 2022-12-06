using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KpCommon.Model
{
    public class RequestModel
    {
        /// <summary>
        /// 请求起始地址如M100,DB10等
        /// </summary>
        public virtual string Address { get; set; }
        /// <summary>
        /// 请求字节长度
        /// </summary>
        public virtual ushort Length { get; set; }
    }
}
