using KpCommon.Extend;
using KpHiteModbus.Modbus.View;
using KpHiteMqtt.Mqtt.Model;
using KpHiteMqtt.Mqtt.ViewModel;
using Scada.UI;
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
        private bool _showchannels;
        DataSpecs _dataSpecs;
        public event Action<DataSpecs> OnDeleteJsonPara;
        public event Action<DataSpecs> OnModifyJsonPara;
        public CtrlJsonParaSpec()
        {
            InitializeComponent();
        }
        public CtrlJsonParaSpec(DataSpecs dataSpecs,bool showchannels)
        {
            InitializeComponent();
            _dataSpecs = dataSpecs;
            _showchannels = showchannels;
            //绑定控件
            lblParaName.AddDataBindings(_dataSpecs, nameof(DataSpecs.ParameterName));
            lblInputChannel.AddDataBindings(_dataSpecs, nameof(DataSpecs.InCnlNum));
            lblOutputChannel.AddDataBindings(_dataSpecs,nameof(DataSpecs.CtrlCnlNum));
            lblIdentifier.AddDataBindings(_dataSpecs, nameof(DataSpecs.Identifier));

            if (!_showchannels)
            {
                lblInput.Visible = false;
                lblInputChannel.Visible = false;
                lbloutput.Visible = false;
                lblOutputChannel.Visible = false;
            }
        }

        private void linkEdit_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            FrmDevJsonPara frm = new FrmDevJsonPara();
            frm.InitParameters(_dataSpecs,_showchannels);
            frm.ShowDialog();
            OnModifyJsonPara?.Invoke(_dataSpecs);
        }

        private void linkDelete_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (MessageBox.Show("是否确认要删除该参数?", "参数删除", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                OnDeleteJsonPara?.Invoke(_dataSpecs);
            }
        }

        private void CtrlJsonParaSpec_Load(object sender, EventArgs e)
        {

        }
    }
}
