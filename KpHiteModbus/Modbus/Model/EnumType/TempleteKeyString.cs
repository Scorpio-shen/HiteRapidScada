using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KpHiteModbus.Modbus.Model
{
    public class TempleteKeyString
    {
        public static readonly string DialogFilterStr = "Template Files(*.xml)|*.xml|All Files(*.*)|*.*";

        public static readonly string DefaultTagGroupName = "测点组";

        public static readonly string DefaultCmdGroupName = "指令组";

        public static readonly string OpenExcelFilterStr = "导入文件(*.xls)|*.xls|导入文件(*.xlsx)|*.xlsx|All Files(*.*)|*.*";
        public static readonly string SaveExcelFilterStr = "导出文件(*.xls)|*.xls|导出文件(*.xlsx)|*.xlsx|All Files(*.*)|*.*";

        public static readonly string RangeOutOfMaxRequestLengthErrorMsg = "不允许超过最大数据请求长度!";
    }
}
