using KpOmron.Model;
using KpOmron.Model.EnumType;
using System;

namespace KpOmron.Model
{
    public delegate void TagGroupChangedEventHandler(object sender, TagGroupChangedEventArgs e);
    public delegate void PLCConfigChangedEventHandler(object sender, PLCConfigChangedEventArgs e);
    public class TagGroupChangedEventArgs : EventArgs
    {
        public TagGroup TagGroup { get; set; }
        public ModifyType ModifyType { get; set; }
    }

    //public class CmdGroupChangedEventArgs : EventArgs
    //{
    //    public SiemensCmdGroup ViewModel { get; set; }
    //    public ModifyType ModifyType { get; set; }
    //}

    public class PLCConfigChangedEventArgs : EventArgs
    {
        public ConnectionOptions Options { get; set; }
        public PLCConnectionOptionsEnum ModifyType { get; set; }
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
