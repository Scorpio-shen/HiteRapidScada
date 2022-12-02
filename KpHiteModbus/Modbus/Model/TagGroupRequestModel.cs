using Scada.KPModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KpHiteModbus.Modbus.Model
{
    /// <summary>
    /// 西门子数据请求Model
    /// </summary>
    public class TagGroupRequestModel : RequestModel
    {
        public int StartAddress { get; set; }
    }
}
