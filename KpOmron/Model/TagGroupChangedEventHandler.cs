using KpOmron.Model;
using KpOmron.Model.EnumType;
using System;

namespace KpOmron.Model
{
    public delegate void ConfigChangedEventHandler(object sender, ConfigChangedEventArgs e);

    public class ConfigChangedEventArgs : EventArgs
    {
        public TagGroup TagGroup { get; set; }

        public ConnectionOptions ConnectionOptions { get; set; }
        public ModifyType ModifyType { get; set; }
    }
}
