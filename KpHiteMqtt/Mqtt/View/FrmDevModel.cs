using KpCommon.Extend;
using KpHiteMqtt.Mqtt.Model;
using KpHiteMqtt.Mqtt.Model.Enum;
using Scada.Data.Entities;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Windows.Forms;

namespace KpHiteMqtt.Mqtt.View
{
    public partial class FrmDevModel : Form
    {
        private List<InCnl> _allInCnls;
        private List<CtrlCnl> _allCtrlCnls;
        private Property _property;

        private static bool isreadonly = false;
        public static bool IsReadOnly
        {
            get => isreadonly;
        }

        public FrmDevModel(Property property)
        {
            InitializeComponent();
            _allCtrlCnls = FrmDevTemplate.AllCtrlCnls;
            _allInCnls = FrmDevTemplate.AllInCnls;
            _property = property;
            _property.PropertyChanged += _property_PropertyChanged;
            InitDataBings();
        }

        private void _property_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals(nameof(Property.IsReadOnly)))
            {
                isreadonly = _property.IsReadOnly;
            }
        }

        private void InitDataBings()
        {
            //输入通道
            BindingSource bdsInputChannels = new BindingSource();
            bdsInputChannels.DataSource = _allInCnls;
            cbxInputChannels.DataSource = bdsInputChannels;
            cbxInputChannels.DisplayMember = "CnlNum";
            cbxInputChannels.ValueMember = "Name";

            //输出通道
            BindingSource bdsOutputChannels = new BindingSource();
            bdsOutputChannels.DataSource = _allCtrlCnls;
            cbxOutputChannels.DataSource = bdsOutputChannels;
            cbxOutputChannels.DisplayMember = "CtrlCnlNum";
            cbxOutputChannels.ValueMember = "Name";
            //绑定存储器类
            Dictionary<string, DataTypeEnum> keyValueDataTypeEnums = new Dictionary<string, DataTypeEnum>();
            foreach (DataTypeEnum type in Enum.GetValues(typeof(DataTypeEnum)))
                keyValueDataTypeEnums.Add(type.ToString(), type);

            BindingSource bindingSource = new BindingSource();
            bindingSource.DataSource = keyValueDataTypeEnums;
            cbxDataType.DataSource = bindingSource;
            cbxDataType.DisplayMember = "Key";
            cbxDataType.ValueMember = "Value";


            //绑定控件
            txtName.AddDataBindings(_property, nameof(Property.Name));
            txtIdentifier.AddDataBindings(_property, nameof(Property.Identifier));
            rdbReadOnlyR.AddDataBindings(_property,nameof(Property.IsReadOnly));
            cbxInputChannels.AddDataBindings(_property, nameof(Property.CnlNum));
            cbxInputChannels.AddVisableDataBindings(_property,nameof(Property.InputChannelVisable));
            lblInputChannel.AddVisableDataBindings(_property, nameof(Property.InputChannelVisable));
            cbxOutputChannels.AddDataBindings(_property, nameof(Property.CtrlCnlNum));
            cbxOutputChannels.AddVisableDataBindings(_property, nameof(_property.OutputChannelVisable));
            lblOutputChannel.AddVisableDataBindings(_property, nameof(_property.OutputChannelVisable));
            txtDescription.AddDataBindings(_property, nameof(Property.Description));
            txtUnit.AddDataBindings(_property,nameof(_property.Unit));
            ctrlJsonPara.AddVisableDataBindings(_property, nameof(_property.IsStruct));
            cbxDataType.AddDataBindings(_property, nameof(_property.DataType));
            ctrlArrayPara.AddVisableDataBindings(_property, nameof(_property.IsArray));
            //控件参数赋值
            //数组
            ctrlArrayPara.InitCtrlArrayPara(_property.ArraySpecs);
            //Json参数
            ctrlJsonPara.InitPara(_property.DataSpecsList);
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
