using KpCommon.Extend;
using KpOmron.Model;
using KpOmron.Model.EnumType;
using KpOmron.ViewModel;
using Scada.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Security.Principal;
using System.Windows.Forms;

namespace KpOmron.View
{
    public partial class FrmDevAddRange : Form
    {
        FrmDevAddRangeViewModel ViewModel { get; set; }
        readonly TagGroup _tagGroup;
        BindingSource bindSourceDataTypeOnlyBool;
        BindingSource bindSourceDataTypeExceptBool;

        public FrmDevAddRange(TagGroup tagGroup)
        {
            InitializeComponent();
            ViewModel = new FrmDevAddRangeViewModel();
            _tagGroup = tagGroup;
            InitControl();
        }

        private void InitControl()
        {
            Dictionary<string, DataTypeEnum> keyValueDataTypeEnums = new Dictionary<string, DataTypeEnum>();
            foreach (DataTypeEnum type in Enum.GetValues(typeof(DataTypeEnum)))
                keyValueDataTypeEnums.Add(type.ToString(), type);

            BindingSource bindingSourceDataType = new BindingSource();
            bindingSourceDataType.DataSource = keyValueDataTypeEnums;
            cbxDataType.DataSource = bindingSourceDataType;
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


            chkCanWrite.Enabled = true;
            chkCanWrite.Visible = true;

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
            if(!_tagGroup.CheckAndAddTags(GetTags(),out string errorMsg))
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
                if (i > 0)
                {
                    if (ViewModel.DataType == DataTypeEnum.Bool)
                    {

                        double dPart = address % 1; //小数部分
                        int iPart = (int)address;
                        if (dPart + ViewModel.AddressIncrement < 0.15d)
                            dPart += ViewModel.AddressIncrement;
                        else
                        {
                            iPart++;
                            dPart = 0.0d;
                        }

                        address = iPart + dPart;
                    }
                    else
                        address = ViewModel.StartAddress + ViewModel.AddressIncrement * i;
                }
                
                Tag tag = Model.Tag.CreateNewTag(tagname: name, dataType: ViewModel.DataType, memoryType: _tagGroup.MemoryType, address: address.ToString(), canwrite:ViewModel.CanWrite,length: ViewModel.Length);
                result.Add(tag);
            }

            return result;
        }
    }
}
 