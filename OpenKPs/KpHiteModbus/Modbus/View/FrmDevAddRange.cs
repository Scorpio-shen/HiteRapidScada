﻿using KpCommon.Extend;
using KpHiteModbus.Modbus.Model;
using KpHiteModbus.Modbus.ViewModel;
using Scada.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;

namespace KpHiteModbus.Modbus.View
{
    public partial class FrmDevAddRange : Form
    {
        FrmDevAddRangeViewModel ViewModel { get; set; }
        readonly ModbusTagGroup _modbusTagGroup;
        BindingSource bindSourceDataTypeOnlyBool;
        BindingSource bindSourceDataTypeExceptBool;

        public FrmDevAddRange(ModbusTagGroup modbusTagGroup)
        {
            InitializeComponent();
            ViewModel = new FrmDevAddRangeViewModel();
            _modbusTagGroup = modbusTagGroup;
            InitControl();
        }

        private void InitControl()
        {
            Dictionary<string, DataTypeEnum> keyValueDataTypeEnumsOnlyBool = new Dictionary<string, DataTypeEnum>();
            keyValueDataTypeEnumsOnlyBool.Add(DataTypeEnum.Bool.ToString(), DataTypeEnum.Bool);
            bindSourceDataTypeOnlyBool = new BindingSource();
            bindSourceDataTypeOnlyBool.DataSource = keyValueDataTypeEnumsOnlyBool;

            Dictionary<string, DataTypeEnum> keyValueDataTypeEnumsExceptBool = new Dictionary<string, DataTypeEnum>();
            foreach (DataTypeEnum type in Enum.GetValues(typeof(DataTypeEnum)))
                if (type != DataTypeEnum.Bool)
                    keyValueDataTypeEnumsExceptBool.Add(type.ToString(), type);
            bindSourceDataTypeExceptBool = new BindingSource();
            bindSourceDataTypeExceptBool.DataSource = keyValueDataTypeEnumsExceptBool;

            //绑定存储器类
            //Dictionary<string, DataTypeEnum> keyValueDataTypeEnums = new Dictionary<string, DataTypeEnum>();
            //foreach (DataTypeEnum type in Enum.GetValues(typeof(DataTypeEnum)))
            //    keyValueDataTypeEnums.Add(type.ToString(), type);

            //BindingSource bindingSourceDataType = new BindingSource();
            //bindingSourceDataType.DataSource = keyValueDataTypeEnums;

            if (_modbusTagGroup.RegisterType == RegisterTypeEnum.Coils || _modbusTagGroup.RegisterType == RegisterTypeEnum.DiscretesInputs)
                cbxDataType.DataSource = bindSourceDataTypeOnlyBool;
            else
                cbxDataType.DataSource = bindSourceDataTypeExceptBool;
            //cbxDataType.DataSource = bindingSourceDataType;
            cbxDataType.DisplayMember = "Key";
            cbxDataType.ValueMember = "Value";

            //控件绑定
            txtNameReplace.AddDataBindings(ViewModel, nameof(ViewModel.NameReplace));
            txtLength.AddDataBindings(ViewModel, nameof(ViewModel.Length));
            numNameStartIndex.AddDataBindings(ViewModel, nameof(ViewModel.NameStartIndex));
            txtStartAddress.AddDataBindings( ViewModel, nameof(ViewModel.StartAddress));
            numAddressIncrement.AddDataBindings(ViewModel, nameof(ViewModel.AddressIncrement));
            numTagCount.AddDataBindings( ViewModel, nameof(ViewModel.TagCount));
            cbxDataType.AddDataBindings(ViewModel, nameof(ViewModel.DataType));
            lblAddressOutput.AddDataBindings(ViewModel, nameof(ViewModel.AddressOutput));
            lblNameOutput.AddDataBindings(ViewModel, nameof(ViewModel.NameOutput));
            txtLength.AddDataBindings(ViewModel, nameof(ViewModel.Length));
            chkCanWrite.AddDataBindings(ViewModel, nameof(ViewModel.CanWrite));


            var registerType = _modbusTagGroup.RegisterType;
            if (registerType == RegisterTypeEnum.Coils || registerType == RegisterTypeEnum.HoldingRegisters)
            {
                chkCanWrite.Enabled = true;
                chkCanWrite.Visible = true;
            }
            else
            {
                chkCanWrite.Enabled = false;
                chkCanWrite.Visible = false;
            }
                
        }

        private void btnConfirm_Click(object sender, EventArgs e)
        {
            //校验输入
            if (string.IsNullOrEmpty(ViewModel.NameReplace))
            {
                ScadaUiUtils.ShowWarning($"通配符不能为空!");
                return;
            }

            if (string.IsNullOrEmpty(txtStartAddress.Text))
            {
                ScadaUiUtils.ShowWarning($"起始地址不能为空!");
                return;
            }

            //验证是否超出最大地址长度
            if(!_modbusTagGroup.CheckAndAddTags(GetTags(),out string errorMsg))
            {
                ScadaUiUtils.ShowError(errorMsg);
                return;
            }

            DialogResult = DialogResult.OK;
        }

        private void btnCancle_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void txtStartAddress_Validating(object sender, CancelEventArgs e)
        {
            var text = ((TextBox)sender).Text;
            if (!double.TryParse(text,out _))
                e.Cancel = true;
        }

        private void txtLength_Validating(object sender, CancelEventArgs e)
        {
            var text = ((TextBox)sender).Text;
            if (!double.TryParse(text, out _))
                e.Cancel = true;
        }

        private void cbxDataType_SelectedIndexChanged(object sender, EventArgs e)
        {
            var type = cbxDataType.SelectedValue as DataTypeEnum?;
            if (type == null)
                return;

           lblLength.Visible = txtLength.Visible = type == DataTypeEnum.String;
        }

        private List<Tag> GetTags()
        {
            List<Tag> result = new List<Tag>();
            var address = ViewModel.StartAddress;
            for (int i = 0; i < ViewModel.TagCount; i++)
            {
                var name = $"{ViewModel.NameReplace}{ViewModel.NameStartIndex + i}";
                address = ViewModel.StartAddress + ViewModel.AddressIncrement * i;
                Tag tag = Model.Tag.CreateNewTag(tagname: name, dataType: ViewModel.DataType, registerType: _modbusTagGroup.RegisterType, address: address.ToString(), canwrite:ViewModel.CanWrite,length: ViewModel.Length);
                result.Add(tag);
            }

            return result;
        }
    }
}
 