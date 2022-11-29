using KpHiteModbus.Modbus.Model;
using KpHiteModbus.Modbus.Model.EnumType;
using Scada.Extend;
using Scada.KPModel.Extend;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace KpHiteModbus.Modbus.View
{
    public partial class CtrlPLCConfig : UserControl
    {
        public event ModbusConfigChangedEventHandler ConfigChanged;
        private ConnectionOptions connectionOptions;
        public ConnectionOptions ConnectionOptions
        {
            get=>connectionOptions;
            set
            {
                IsShowProps = true;
                connectionOptions = value;
                BindControls(value);
                IsShowProps = false;
            }
        }

        /// <summary>
        /// 是否是展示Options内容,只是展示内容不触发控件事件
        /// </summary>
        public bool IsShowProps { get; set; } = false;
        public CtrlPLCConfig()
        {
            InitializeComponent();
        }

        private void CtrlPLCConfig_Load(object sender, EventArgs e)
        {
            //Combobox绑定数据源
            var keyValueConnectionEnums = new Dictionary<string, ModbusConnectionTypeEnum>();
            foreach (ModbusConnectionTypeEnum type in Enum.GetValues(typeof(ModbusConnectionTypeEnum)))
                keyValueConnectionEnums.Add(type.GetDescription(), type);

            BindingSource bindingSource = new BindingSource();
            bindingSource.DataSource = keyValueConnectionEnums;
            cbxConnectionType.DataSource = bindingSource;
            cbxConnectionType.DisplayMember = "Key";
            cbxConnectionType.ValueMember = "Value";



            var keyValueModeEnums = new Dictionary<string, ModbusModeEnum>();
            foreach (ModbusModeEnum type in Enum.GetValues(typeof(ModbusModeEnum)))
                keyValueModeEnums.Add(type.ToString(), type);

            BindingSource bindingSource2 = new BindingSource();
            bindingSource2.DataSource = keyValueModeEnums;
            cbxMode.DataSource = bindingSource2;
            cbxMode.DisplayMember = "Key";
            cbxMode.ValueMember = "Value";

        }

        private void BindControls(ConnectionOptions options)
        {
            if (options == null)
                return;
            txtStation.AddDataBindings( options, nameof(options.Station));
            txtParams.AddDataBindings(options, nameof(options.ConsoleParamsStr));
            cbxConnectionType.AddDataBindings( options, nameof(options.ConnectionType));
            cbxMode.AddDataBindings( options, nameof(options.ModbusMode));
        }

        private void OnConfigChanged(object sender)
        {
            ConfigChanged?.Invoke(sender, new  ModbusConfigChangedEventArgs
            {
                ConnectionOptions = ConnectionOptions
            });
        }

        #region 控件事件
        private void btnParamSetting_Click(object sender, EventArgs e)
        {
            FrmParaSet frm = new FrmParaSet(ConnectionOptions,OnConfigChanged);
            frm.StartPosition = FormStartPosition.CenterParent;
            frm.ShowDialog();
        }

        private void txtStation_TextChanged(object sender, EventArgs e)
        {
            if (IsShowProps)
                return;

            OnConfigChanged(sender);
        }

        private void cbxConnectionType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (IsShowProps)
                return;

            var connectionType = cbxConnectionType.SelectedValue as ModbusConnectionTypeEnum?;
            if(connectionType == null)
                return;
            switch (connectionType)
            {
                case ModbusConnectionTypeEnum.SerialPort:
                case ModbusConnectionTypeEnum.RTUASCIIOverUdp:
                case ModbusConnectionTypeEnum.RTUASCIIOverTcp:
                    cbxMode.Enabled = true;
                    break;
                case ModbusConnectionTypeEnum.TcpIP:
                case ModbusConnectionTypeEnum.Udp:
                    cbxMode.Enabled = false;
                    break;
            }
            OnConfigChanged(sender);
        }

        private void cbxMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (IsShowProps)
                return;

            OnConfigChanged(sender);
        }
        #endregion

    }
}
