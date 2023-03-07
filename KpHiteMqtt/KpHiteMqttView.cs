using KpHiteMqtt.Mqtt.View;
using Scada.Comm.Devices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KpHiteMqtt
{
    public class KpHiteMqttView : KPView
    {
        public override string KPDescr => "Hite Mqtt驱动v1.0";
        public KpHiteMqttView() : base(0)
        {

        }

        public KpHiteMqttView(int number) : base(number)
        {

        }



        public override void ShowProps()
        {
            FrmDevProps frm = new FrmDevProps(Number, AppDirs, KPProps);
            frm.ShowDialog();

        }
    }
}
