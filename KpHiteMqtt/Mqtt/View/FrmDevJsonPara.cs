using KpCommon.Extend;
using KpHiteMqtt.Mqtt.Model;
using KpHiteMqtt.Mqtt.Model.Enum;
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
        public FrmDevJsonPara()
        {
            InitializeComponent();
        }

        public FrmDevJsonPara(DataSpecs dataSpecs)
        {
            InitializeComponent();
            _dataSpecs = dataSpecs;
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
            //绑定控件
            txtName.AddDataBindings(_dataSpecs, nameof(_dataSpecs.ParameterName));
            txtIdentifier.AddDataBindings(_dataSpecs,nameof(_dataSpecs.Identifier));
            txtUnit.AddDataBindings(_dataSpecs, nameof(_dataSpecs.Unit));
            cbxDataType.AddDataBindings(_dataSpecs, nameof(_dataSpecs.DataType));


        }
    }
}
