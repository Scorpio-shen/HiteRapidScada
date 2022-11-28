using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scada.KPModel
{
    /// <summary>
    /// 导出Excel时Header属性排序
    /// </summary>
    public class ExcelHeaderSortIndexAttribute : Attribute
    {
        /// <summary>
        /// 序号
        /// </summary>
        public int Index { get; set; }
        public ExcelHeaderSortIndexAttribute(int index)
        {
            Index = index;
        }
    }
}
