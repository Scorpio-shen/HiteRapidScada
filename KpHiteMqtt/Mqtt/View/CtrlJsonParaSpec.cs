using KpHiteMqtt.Mqtt.ViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KpHiteMqtt.Mqtt.View
{
    public partial class CtrlJsonParaSpec : UserControl
    {
        public CtrlJsonParaSpecViewModel ViewModel { get; set; }
        public CtrlJsonParaSpec()
        {
            InitializeComponent();
        }

        private void lblOutputChannel_Click(object sender, EventArgs e)
        {

        }
    }
}
