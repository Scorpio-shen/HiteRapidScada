using KpCommon.Extend;
using KpOmron.Model;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Windows.Forms;

namespace KpOmron.View
{
    public partial class FrmParaSet : Form
    {
        ConnectionOptions _connectionOptions;
        Action<object> _onConfigChanged;
        bool isLoad = false;
        public FrmParaSet(ConnectionOptions connectionOptions,Action<object> OnConfigChanged)
        {
            InitializeComponent();
            _connectionOptions = connectionOptions;
            _onConfigChanged = OnConfigChanged;

            BindEvent(Controls);
        }

        private void FrmParaSet_Load(object sender, EventArgs e)
        {
            isLoad = true;

            if(_connectionOptions.ProtocolType == Model.EnumType.ProtocolTypeEnum.HostLinkSerial || _connectionOptions.ProtocolType == Model.EnumType.ProtocolTypeEnum.HostLinkCMode)
            {
                gbxSerial.Enabled = true;
                gbxTcp.Enabled = false;
            }
            else
            {
                gbxSerial.Enabled = false;
                gbxTcp.Enabled = true;
            }

            #region 下拉框绑定数据源
            //获取所有串口
            cbxSerialPortName.Items.AddRange(SerialPort.GetPortNames());

            //数据位和停止位下拉列表绑定数据源
            var dicParity = new Dictionary<string, Parity>()
            {
                { "无",Parity.None },
                { "奇",Parity.Odd },
                { "偶",Parity.Even },
            };

            var bindSourceParity = new BindingSource();
            bindSourceParity.DataSource = dicParity;
            cbxParity.DataSource = bindSourceParity;
            cbxParity.DisplayMember = "Key";
            cbxParity.ValueMember = "Value";

            var dicStopBit = new Dictionary<string, StopBits>()
            {
                {"1",StopBits.One },
                {"1.5",StopBits.OnePointFive },
                {"2",StopBits.Two },
            };
            var bindSourceStopBit = new BindingSource();
            bindSourceStopBit.DataSource = dicStopBit;
            cbxStopBits.DataSource = bindSourceStopBit;
            cbxStopBits.DisplayMember = "Key";
            cbxStopBits.ValueMember = "Value";
            #endregion

            BindControl();

            isLoad = false;
        }


        private void BindControl()
        {
            txtIPAddress.AddDataBindings(_connectionOptions, nameof(_connectionOptions.IPAddress));
            txtPort.AddDataBindings( _connectionOptions, nameof(_connectionOptions.Port));

            cbxSerialPortName.AddDataBindings(_connectionOptions, nameof(_connectionOptions.PortName));
            cbxBaudRate.AddDataBindings( _connectionOptions, nameof(_connectionOptions.BaudRate));
            cbxDataBits.AddDataBindings(_connectionOptions, nameof(_connectionOptions.DataBits));
            cbxParity.AddDataBindings(_connectionOptions, nameof(_connectionOptions.Parity));
            cbxStopBits.AddDataBindings(_connectionOptions, nameof(_connectionOptions.StopBits));

            txtStation.AddDataBindings(_connectionOptions, nameof(_connectionOptions.UnitNumber));
            txtSlot.AddDataBindings(_connectionOptions, nameof(_connectionOptions.Slot));
            txtSA1.AddDataBindings(_connectionOptions, nameof(_connectionOptions.SA1));
            txtSID.AddDataBindings(_connectionOptions,nameof(_connectionOptions.SID));
            txtSA2.AddDataBindings(_connectionOptions, nameof(_connectionOptions.SA2));
            txtDA2.AddDataBindings(_connectionOptions, nameof(_connectionOptions.DA2));
        }

        private void btnConfirm_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }

        private void btnCancle_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void BindEvent(Control.ControlCollection collection)
        {
            foreach(Control control in collection)
            {
                if (control.HasChildren)
                    BindEvent(control.Controls);
                else
                {
                    if(control is ComboBox comboBox)
                    {
                        comboBox.SelectedIndexChanged += ControlChanged;
                    }
                    else if(control is TextBox textBox)
                    {
                        textBox.TextChanged += ControlChanged;
                    }
                }
            }
        }

        private void ControlChanged(object sender,EventArgs e)
        {
            if(isLoad)
                return;
            _onConfigChanged?.Invoke(sender);
        }
    }
}
