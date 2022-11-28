using Scada.Comm.Devices.Modbus.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KpMyModbus.Modbus.Protocol
{
    public class TagInfo
    {
        public Tag Tag { get; set; }
        public MyTagGroup MyTagGroup { get; set; }
        public ushort Address { get; set; }
        public int Signal { get; set; }
        //public string AddressRange
        //{
        //    get
        //    {
        //        return ModbusUtils.GetAddressRange(Address,)
        //    }
        //}
        public string Caption
        {
            get
            {
                return (string.IsNullOrEmpty(Tag.Name) ? KpPhrases.DefElemName : Tag.Name);
            }
        }
    }
}
