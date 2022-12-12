using KpCommon.InterFace;
using KpCommon.Model.EnumType;
using System;

namespace KpCommon.Model
{
    public delegate void ConfigChangedEventHandler<T>(object sender, ConfigChangedEventArgs<T> e) where T : class,IDataUnit;

    public class ConfigChangedEventArgs<T> : EventArgs where T : class, IDataUnit
    {
        public IGroupUnit<T> TagGroup { get; set; }

        public IConnectionUnit ConnectionOptions { get; set; }
        public ModifyType ModifyType { get; set; }
    }
}
