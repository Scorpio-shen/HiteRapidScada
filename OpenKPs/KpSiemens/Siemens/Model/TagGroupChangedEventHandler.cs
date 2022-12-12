using KpSiemens.Siemens.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KpSiemens.Siemens.Model
{
    public delegate void TagGroupChangedEventHandler(object sender, TagGroupChangedEventArgs e);
    //public delegate void CmdGroupChangedEventHandler(object sender, CmdGroupChangedEventArgs e);
    public delegate void PLCConfigChangedEventHandler(object sender, PLCConfigChangedEventArgs e);
    public class TagGroupChangedEventArgs : EventArgs
    {
        public SiemensTagGroup ViewModel { get; set; }
        public ModifyType ModifyType { get; set; }
    }
    public class PLCConfigChangedEventArgs : EventArgs
    {
        public PLCConnectionOptions ViewModel { get; set; }
        public PLCConnectionOptionsEnum ModifyType { get; set; }
    }
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
    }

    public enum PLCConnectionOptionsEnum
    {
        IPAddress,
        Port,
        Rack,
        Slot,
        PLCType,
        ConnectionType,
        LocalTASP,
        DestTASP
    }
}
