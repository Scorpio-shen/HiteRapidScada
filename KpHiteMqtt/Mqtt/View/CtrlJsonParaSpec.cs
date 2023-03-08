using KpCommon.Extend;
using KpHiteMqtt.Mqtt.Model;
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
        DataSpecs _dataSpecs;
        public CtrlJsonParaSpec()
        {
            InitializeComponent();
        }
        public CtrlJsonParaSpec(DataSpecs dataSpecs)
        {
            InitializeComponent();
            _dataSpecs = dataSpecs;
            //绑定控件
            lblParaName.AddDataBindings(_dataSpecs, nameof(DataSpecs.ParameterName));
            lblInputChannel.AddDataBindings(_dataSpecs, nameof(DataSpecs.CnlNum));
            lblOutputChannel.AddDataBindings(_dataSpecs,nameof(DataSpecs.CtrlCnlNum));
        }

        private void linkEdit_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {

        }

        private void linkDelete_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {

        }
    }
}
