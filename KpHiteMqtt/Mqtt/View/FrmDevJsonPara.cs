using KpCommon.Extend;
using KpHiteMqtt.Mqtt.Model;
using KpHiteMqtt.Mqtt.Model.Enum;
using KpHiteMqtt.Mqtt.View;
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

namespace KpHiteModbus.Modbus.View
{
    public partial class FrmDevJsonPara : Form
    {
        private DataSpecs _dataSpecs;
        private bool _isreadonly;
        private bool _showChannels;
        List<InCnl> _allInCnls;
        List<CtrlCnl> _allCtrlCnls;
        
        public FrmDevJsonPara()
        {
            InitializeComponent();
        }

        public void InitParameters(DataSpecs dataSpecs,bool showChannels)
        {
            _dataSpecs = dataSpecs;
            _showChannels = showChannels;
            _allInCnls = FrmDevTemplate.AllInCnls;
            _allCtrlCnls = FrmDevTemplate.AllCtrlCnls;
            _isreadonly = FrmDevModel.IsReadOnly;

            if(_isreadonly )
            {
                lblOutputChannel.Visible = false;
                cbxOutputChannels.Visible = false;
            }
            else
            {
                lblOutputChannel.Visible = true;
                cbxOutputChannels.Visible = true;
            }

            if (!_showChannels)
            {
                lblInputChannel.Visible = false;
                cbxInputChannels.Visible = false;
                lblOutputChannel.Visible = false;
                cbxOutputChannels.Visible = false;
            }
        }

        private void FrmDevJsonPara_Load(object sender, EventArgs e)
        {
            //combobox绑定数据源
            Dictionary<string, StructDataTypeEnum> keyValueDataTypeEnums = new Dictionary<string, StructDataTypeEnum>();
            foreach (StructDataTypeEnum type in Enum.GetValues(typeof(StructDataTypeEnum)))
                keyValueDataTypeEnums.Add(type.ToString(), type);
            BindingSource bdsDataType = new BindingSource();
            bdsDataType.DataSource = keyValueDataTypeEnums;
            cbxDataType.DataSource = bdsDataType;
            cbxDataType.DisplayMember = "Key";
            cbxDataType.ValueMember = "Value";

            var bdsInCnls = new BindingSource();
            bdsInCnls.DataSource = _allInCnls;
            cbxInputChannels.DataSource = bdsInCnls;
            cbxInputChannels.DisplayMember = "Name";
            cbxInputChannels.ValueMember = "CnlNum";

            var bdsOutCnls = new BindingSource();
            bdsOutCnls.DataSource = _allCtrlCnls;

            cbxOutputChannels.DataSource = bdsOutCnls;
            cbxOutputChannels.DisplayMember = "Name";
            cbxOutputChannels.ValueMember = "CtrlCnlNum";
            //绑定控件
            txtName.AddDataBindings(_dataSpecs, nameof(_dataSpecs.ParameterName));
            txtIdentifier.AddDataBindings(_dataSpecs,nameof(_dataSpecs.Identifier));
            txtUnit.AddDataBindings(_dataSpecs, nameof(_dataSpecs.Unit));   
            cbxDataType.AddDataBindings(_dataSpecs, nameof(_dataSpecs.DataType));
            cbxInputChannels.AddDataBindings(_dataSpecs, nameof(_dataSpecs.InCnlNum));
            cbxOutputChannels.AddDataBindings(_dataSpecs, nameof(_dataSpecs.CtrlCnlNum));

        }

        private void btnConfirm_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult= DialogResult.Cancel;
        }
    }
}
