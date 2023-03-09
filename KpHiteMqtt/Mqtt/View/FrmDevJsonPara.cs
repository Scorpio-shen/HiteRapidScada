﻿using KpCommon.Extend;
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
        List<InCnl> _allInCnls;
        List<CtrlCnl> _allCtrlCnls;
        
        public FrmDevJsonPara()
        {
            InitializeComponent();
        }

        public void InitParameters(DataSpecs dataSpecs)
        {
            _dataSpecs = dataSpecs;
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
            cbxInputChannels.DisplayMember = "CnlNum";
            cbxInputChannels.ValueMember = "CnlNum";

            var bdsOutCnls = new BindingSource();
            bdsOutCnls.DataSource = _allCtrlCnls;

            cbxOutputChannels.DataSource = bdsOutCnls;
            cbxOutputChannels.DisplayMember = "CtrlCnlNum";
            cbxOutputChannels.ValueMember = "CtrlCnlNum";
            //绑定控件
            txtName.AddDataBindings(_dataSpecs, nameof(_dataSpecs.ParameterName));
            txtIdentifier.AddDataBindings(_dataSpecs,nameof(_dataSpecs.Identifier));
            txtUnit.AddDataBindings(_dataSpecs, nameof(_dataSpecs.Unit));   
            cbxDataType.AddDataBindings(_dataSpecs, nameof(_dataSpecs.DataType));
            cbxInputChannels.AddDataBindings(_dataSpecs, nameof(_dataSpecs.CnlNum));
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
