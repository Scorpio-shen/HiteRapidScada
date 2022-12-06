using KpHiteModbus.Modbus.Model.EnumType;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KpHiteModbus.Modbus.Model
{


    public delegate void ModbusConfigChangedEventHandler(object sender, ModbusConfigChangedEventArgs e);

    public class ModbusConfigChangedEventArgs : EventArgs
    {
        public ModbusTagGroup TagGroup { get; set; }

        public ConnectionOptions ConnectionOptions { get; set; }
        public ModifyType ChangeType { get; set; }
    }


}
