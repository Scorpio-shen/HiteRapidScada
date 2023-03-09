using KpCommon.Extend;
using KpHiteMqtt.Mqtt.Model;
using Scada.Data.Entities;
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
    public partial class CtrlArrayInCtrlChannel : UserControl
    {
        private InCnl _inCnl;
        private CtrlCnl _ctrlCnl;
        private List<InCnl> _allInCnls;
        private List<CtrlCnl> _allCtrlCnls;
        public CtrlArrayInCtrlChannel()
        {
            InitializeComponent();
        }
        public CtrlArrayInCtrlChannel(InCnl inCnl,CtrlCnl ctrlCnl,List<InCnl> allInCnls,List<CtrlCnl> allCtrlCnls)
        {
            InitializeComponent();
            _inCnl = inCnl;
            _ctrlCnl = ctrlCnl;
            _allInCnls = allInCnls.Select(ic=> ic.InCnlCopy()).ToList();
            _allCtrlCnls = allCtrlCnls.Select(cc=>cc.CtrlCnlCopy()).ToList();
        }

        private void CtrlArrayInCtrlChannel_Load(object sender, EventArgs e)
        {
            //Combobox控件绑定
            BindingSource bdsInCnl = new BindingSource();
            bdsInCnl.DataSource = _allInCnls;
            cbxInCnl.DataSource = bdsInCnl;
            cbxInCnl.DisplayMember = "Name";
            cbxInCnl.ValueMember = "CnlNum";

            BindingSource bdsCtrlCnl = new BindingSource();
            bdsCtrlCnl.DataSource = _allCtrlCnls;
            cbxCtrlCnl.DataSource = bdsCtrlCnl;
            cbxCtrlCnl.DisplayMember = "Name";
            cbxCtrlCnl.ValueMember = "CnlNum";
            //控件绑定
            cbxInCnl.AddDataBindings(_inCnl, nameof(_inCnl.CnlNum));
            cbxCtrlCnl.AddDataBindings(_ctrlCnl, nameof(_ctrlCnl.CtrlCnlNum));

        }
    }
}
