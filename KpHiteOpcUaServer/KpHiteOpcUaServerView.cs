using Scada.Comm.Devices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KpHiteOpcUaServer
{
    public class KpHiteOpcUaServerView : KPView
    {
        public override string KPDescr => "Hite Opc ua Server驱动v1.0";

        public KpHiteOpcUaServerView() : base(0) 
        { 
        }

        public KpHiteOpcUaServerView(int number) : base(number)
        {
            CanShowProps = true;
        }

        public override KPCnlPrototypes DefaultCnls => base.DefaultCnls;

        public override void ShowProps()
        {
            
        }
    }
}
