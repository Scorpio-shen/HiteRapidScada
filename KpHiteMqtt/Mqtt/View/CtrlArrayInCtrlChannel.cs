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
        List<InCnl> _allInCnls;
        List<CtrlCnl> _allCtrlCnls;
        DataArraySpecs _arraySpecs;
        public CtrlArrayInCtrlChannel()
        {
            InitializeComponent();
        }
        public CtrlArrayInCtrlChannel(List<InCnl> allInCnls, List<CtrlCnl> allCtrlCnls,DataArraySpecs arraySpecs)
        {
            InitializeComponent();
            _allInCnls = allInCnls;
            _allCtrlCnls = allCtrlCnls;
            _arraySpecs = arraySpecs;
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
            //cbxInCnl.AddDataBindings(_arraySpecs.)

        }
    }
}
