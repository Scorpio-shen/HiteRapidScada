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

        public FrmDevModel(Property property,List<InCnl> allInCnls,List<CtrlCnl> allCtrlCnls)
        {
            InitializeComponent();
            _allCtrlCnls = allCtrlCnls;
            _allInCnls = allInCnls;
            _property = property;


            InitDataBings();
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
            rdbReadOnlyRW.AddDataBindings(_property, nameof(Property.IsReadOnly), true);
            rdbReadOnlyR.AddDataBindings(_property,nameof(Property.IsReadOnly), true);
            cbxInputChannels.AddDataBindings(_property, nameof(Property.CnlNum));
            cbxInputChannels.DataBindings.Add(nameof(cbxInputChannels.Visible), _property, nameof(_property.InputChannelVisable), false, DataSourceUpdateMode.OnPropertyChanged);
            lblInputChannel.AddVisableDataBindings(_property, nameof(_property.InputChannelVisable));
            cbxOutputChannels.AddDataBindings(_property, nameof(Property.CtrlCnlNum));
            cbxOutputChannels.DataBindings.Add(nameof(cbxOutputChannels.Visible), _property, nameof(_property.OutputChannelVisable), false, DataSourceUpdateMode.OnPropertyChanged);
            lblOutputChannel.AddVisableDataBindings(_property, nameof(_property.OutputChannelVisable));
            txtDescription.AddDataBindings(_property, nameof(Property.Description));
            ctrlJsonPara.DataBindings.Add(nameof(ctrlJsonPara.Visible), _property, nameof(_property.IsStruct), false, DataSourceUpdateMode.OnPropertyChanged);

            
        }

    }
}
