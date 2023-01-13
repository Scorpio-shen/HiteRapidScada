using KpCommon.Extend;
using KpAllenBrandly.Model;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Windows.Forms;

namespace KpAllenBrandly.View
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
        }

        private void FrmParaSet_Load(object sender, EventArgs e)
        {
            isLoad = true;
            //初始默认右侧PLC参数都不可见，不可设置
            txtSlot.Visible = false;
            //chkNewVersionMessage.Visible = false;
            //txtStation.Visible = false;
            //chkSumCheck.Visible = false;
            //cbxFormat.Visible = false;


            switch (_connectionOptions.ProtocolType)
            {
                case Model.EnumType.ProtocolTypeEnum.MicroCip:
                    txtSlot.Visible = true;
                    gbxSerial.Enabled = false;
                    gbxTcp.Enabled = true;
                    break;
                case Model.EnumType.ProtocolTypeEnum.EtherNetIP:
                    txtSlot.Visible = true;
                    gbxSerial.Enabled = false;
                    gbxTcp.Enabled = true;
                    break;
                case Model.EnumType.ProtocolTypeEnum.ConnectedCIP:
                    txtSlot.Visible = false;
                    gbxSerial.Enabled = false;
                    gbxTcp.Enabled = true;
                    break;
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

            //cbxSerialPortName.AddDataBindings(_connectionOptions, nameof(_connectionOptions.PortName));
            //cbxBaudRate.AddDataBindings( _connectionOptions, nameof(_connectionOptions.BaudRate));
            //cbxDataBits.AddDataBindings(_connectionOptions, nameof(_connectionOptions.DataBits));
            //cbxParity.AddDataBindings(_connectionOptions, nameof(_connectionOptions.Parity));
            //cbxStopBits.AddDataBindings(_connectionOptions, nameof(_connectionOptions.StopBits));
        }

        private void btnConfirm_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }

        private void btnCancle_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void ControlChanged(object sender,EventArgs e)
        {
            if(isLoad)
                return;
            _onConfigChanged?.Invoke(sender);
        }
    }
}
