using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KpCommon.Attributes
{
    /// <summary>
    /// 排序从0开始,从小到大排序
    /// </summary>
    public class ExcelHeaderSortAttribute : Attribute
    {
        public int SortIndex { get; set; }

        public ExcelHeaderSortAttribute(int sort)
        {
            SortIndex = sort;
        }
    }
}
