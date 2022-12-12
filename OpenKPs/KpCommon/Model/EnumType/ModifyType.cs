using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KpCommon.Model.EnumType
{
    /// <summary>
    /// 修改那个部分
    /// </summary>
    public enum ModifyType
    {
        GroupName,
        IsActive,
        MemoryType,
        TagCount,
        DBNum,
        Tags, //绑定到datagridview的Tags集合变化
        Connection
    }
}
