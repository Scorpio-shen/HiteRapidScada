﻿using KpHiteModbus.Modbus.Model;
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
       
        public FrmDevAddRange(ModbusTagGroup modbusTagGroup)
        {
            InitializeComponent();
            ViewModel = new FrmDevAddRangeViewModel();
            _modbusTagGroup = modbusTagGroup;
            InitControl();
        }

        private void InitControl()
        {
            //绑定存储器类
            Dictionary<string, DataTypeEnum> keyValueDataTypeEnums = new Dictionary<string, DataTypeEnum>();
            foreach (DataTypeEnum type in Enum.GetValues(typeof(DataTypeEnum)))
                keyValueDataTypeEnums.Add(type.ToString(), type);

            BindingSource bindingSourceDataType = new BindingSource();
            bindingSourceDataType.DataSource = keyValueDataTypeEnums;
            cbxDataType.DataSource = bindingSourceDataType;
            cbxDataType.DisplayMember = "Key";
            cbxDataType.ValueMember = "Value";

            //控件绑定
            txtNameReplace.DataBindings.Add(nameof(txtNameReplace.Text), ViewModel, nameof(ViewModel.NameReplace));
            numNameStartIndex.DataBindings.Add(nameof(numNameStartIndex.Value),ViewModel, nameof(ViewModel.NameStartIndex));
            txtStartAddress.DataBindings.Add(nameof(txtStartAddress.Text), ViewModel, nameof(ViewModel.StartAddress));
            numAddressIncrement.DataBindings.Add(nameof(numAddressIncrement.Value), ViewModel, nameof(ViewModel.AddressIncrement));
            numTagCount.DataBindings.Add(nameof(numTagCount.Value),ViewModel, nameof(ViewModel.TagCount));
            cbxDataType.DataBindings.Add(nameof(cbxDataType.SelectedValue), ViewModel, nameof(ViewModel.DataType));
            lblAddressOutput.DataBindings.Add(nameof(lblAddressOutput.Text),ViewModel,nameof(ViewModel.AddressOutput));
            lblNameOutput.DataBindings.Add(nameof(lblNameOutput.Text), ViewModel, nameof(ViewModel.NameOutput));
            txtLength.DataBindings.Add(nameof(txtLength.Text), ViewModel, nameof(ViewModel.Length));
            chkCanWrite.DataBindings.Add(nameof(chkCanWrite.Checked), ViewModel, nameof(ViewModel.CanWrite));

            var registerType = _modbusTagGroup.RegisterType;
            if (registerType == RegisterTypeEnum.Coils || registerType == RegisterTypeEnum.HoldingRegisters)
                chkCanWrite.Enabled = true;
            else
                chkCanWrite.Enabled = false;
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
            if(!_modbusTagGroup.CheckAndAddTags(GetTags()))
            {
                ScadaUiUtils.ShowError(TempleteKeyString.RangeOutOfMaxRequestLengthErrorMsg);
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

                if (ViewModel.DataType == DataTypeEnum.Bool)
                {
                    double dPart = address % 1; //小数部分
                    int iPart = (int)address;
                    if (dPart < 0.7)
                        dPart += 0.1d;
                    else
                    {
                        iPart++;
                        dPart = 0.0d;
                    }

                    address = iPart + dPart;
                }
                else
                    address = ViewModel.StartAddress + ViewModel.AddressIncrement * i;
                Tag tag = Model.Tag.CreateNewTag(tagname: name, dataType: ViewModel.DataType, registerType: _modbusTagGroup.RegisterType, address: address.ToString(), canwrite: (byte)(ViewModel.CanWrite ? 1 : 0),length: ViewModel.Length);
                result.Add(tag);
            }

            return result;
        }
    }
}
 