using KpCommon.Extend;
using KpSiemens.Siemens.Model;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace KpSiemens.Siemens.View
{
    public partial class CtrlPLCConfig : UserControl
    {
        public event PLCConfigChangedEventHandler PLCConfigChanged;
        private PLCConnectionOptions connectionOptions;
        public PLCConnectionOptions ConnectionOptions
        {
            get=>connectionOptions;
            set
            {
                IsShowCmdGroup = true;
                connectionOptions = value;
                BindControls(connectionOptions);
                IsShowCmdGroup = false;
            }
        }

        /// <summary>
        /// 是否是展示Options内容,只是展示内容不触发控件事件
        /// </summary>
        public bool IsShowCmdGroup { get; set; } = false;
        public CtrlPLCConfig()
        {
            InitializeComponent();
        }

        private void CtrlPLCConfig_Load(object sender, EventArgs e)
        {
            //控件绑定DataSource
            var keyValueMemoryEnums = new Dictionary<string, SiemensPLCTypeEnum>();
            foreach (SiemensPLCTypeEnum type in Enum.GetValues(typeof(SiemensPLCTypeEnum)))
                keyValueMemoryEnums.Add(type.ToString(), type);

            BindingSource bindingSource = new BindingSource();
            bindingSource.DataSource = keyValueMemoryEnums;
            cbxPLCType.DataSource = bindingSource;
            cbxPLCType.DisplayMember = "Key";
            cbxPLCType.ValueMember = "Value";

            //txtDestTASP.Visible = false;
            //lblDestTASP.Visible = false;

            
        }

        private void BindControls(PLCConnectionOptions options)
        {
            if (options == null)
                return;
            txtIPAddress.AddDataBindings(ConnectionOptions, nameof(ConnectionOptions.IPAddress));
            cbxPLCType.AddDataBindings(ConnectionOptions, nameof(ConnectionOptions.SiemensPLCTypeEnum));


            //txtIPAddress.DataBindings.Clear();
            //cbxPLCType.DataBindings.Clear();

            //txtIPAddress.DataBindings.Add(nameof(txtIPAddress.Text), ConnectionOptions, nameof(ConnectionOptions.IPAddress),false,DataSourceUpdateMode.OnPropertyChanged);
            //cbxPLCType.DataBindings.Add(nameof(cbxPLCType.SelectedValue), ConnectionOptions, nameof(ConnectionOptions.SiemensPLCTypeEnum), false, DataSourceUpdateMode.OnPropertyChanged);
        }

        private void cbxPLCType_SelectedIndexChanged(object sender, EventArgs e)
        {
            var plcType = cbxPLCType.SelectedValue as SiemensPLCTypeEnum?;
            if (plcType == null)
                return;

            //switch (plcType)
            //{
            //    case SiemensPLCTypeEnum.S200Smart:
            //        txtRack.Visible = false;
            //        txtSlot.Visible = false;
            //        txtConnectionType.Visible = false;
            //        txtLocalTASP.Visible = false;
            //        txtDestTASP.Visible = false;

            //        lblRack.Visible = false;
            //        lblSlot.Visible = false;
            //        lblConnectionType.Visible = false;
            //        lblLocalTASP.Visible = false;
            //        lblDestTASP.Visible = false;
            //        break;
            //    case SiemensPLCTypeEnum.S200:
            //        txtRack.Visible = false;
            //        txtSlot.Visible = false;
            //        txtConnectionType.Visible = false;
            //        txtLocalTASP.Visible = true;
            //        txtDestTASP.Visible = true;

            //        lblRack.Visible = false;
            //        lblSlot.Visible = false;
            //        lblConnectionType.Visible = false;
            //        lblLocalTASP.Visible = true;
            //        lblDestTASP.Visible = true;
            //        break;
            //    default:
            //        txtRack.Visible = true;
            //        txtSlot.Visible = true;
            //        txtConnectionType.Visible = true;
            //        txtLocalTASP.Visible = true;
            //        txtDestTASP.Visible = false;

            //        lblRack.Visible = true;
            //        lblSlot.Visible = true;
            //        lblConnectionType.Visible = true;
            //        lblLocalTASP.Visible = true;
            //        lblDestTASP.Visible = false;
            //        break;
            //}

            if (IsShowCmdGroup)
                return;
            OnPLCConfigChanged(sender, PLCConnectionOptionsEnum.ConnectionType);
        }

        private void txtIPAddress_TextChanged(object sender, EventArgs e)
        {
            if (IsShowCmdGroup)
                return;
            OnPLCConfigChanged(sender, PLCConnectionOptionsEnum.IPAddress);
        }

        private void OnPLCConfigChanged(object sender, PLCConnectionOptionsEnum modifyType)
        {
            PLCConfigChanged?.Invoke(sender, new PLCConfigChangedEventArgs
            {
                ModifyType = modifyType,
                ViewModel = ConnectionOptions
            });
        }

        private void txtPort_TextChanged(object sender, EventArgs e)
        {
            if (IsShowCmdGroup)
                return;
            OnPLCConfigChanged(sender, PLCConnectionOptionsEnum.Port);
        }

        private void txtRack_TextChanged(object sender, EventArgs e)
        {
            if (IsShowCmdGroup)
                return;
            OnPLCConfigChanged(sender, PLCConnectionOptionsEnum.Rack);
        }

        private void txtSlot_TextChanged(object sender, EventArgs e)
        {
            if (IsShowCmdGroup)
                return;
            OnPLCConfigChanged(sender, PLCConnectionOptionsEnum.Slot);
        }

        private void txtConnectionType_TextChanged(object sender, EventArgs e)
        {
            if (IsShowCmdGroup)
                return;
            OnPLCConfigChanged(sender, PLCConnectionOptionsEnum.ConnectionType);
        }

        private void txtLocalTASP_TextChanged(object sender, EventArgs e)
        {
            if (IsShowCmdGroup)
                return;
            OnPLCConfigChanged(sender, PLCConnectionOptionsEnum.LocalTASP);
        }

        private void txtDestTASP_TextChanged(object sender, EventArgs e)
        {
            if (IsShowCmdGroup)
                return;
            OnPLCConfigChanged(sender, PLCConnectionOptionsEnum.LocalTASP);
        }

        private void btnAdvanceSetting_Click(object sender, EventArgs e)
        {
            FrmAdvancedSetting frmAdvancedSetting = new FrmAdvancedSetting(ConnectionOptions, OnPLCConfigChanged);
            frmAdvancedSetting.StartPosition = FormStartPosition.CenterParent;
            frmAdvancedSetting.ShowDialog();
        }

        /// <summary>
        /// 属性值展示
        /// </summary>
        //private void ShowProps(PLCConnectionOptions options)
        //{
        //    ConnectionOptions.IPAddress = options.IPAddress;
        //    ConnectionOptions.Port = options.Port;
        //    ConnectionOptions.Rack = options.Rack;
        //    ConnectionOptions.Slot = options.Slot;
        //    ConnectionOptions.ConnectionType = options.ConnectionType;
        //    ConnectionOptions.SiemensPLCTypeEnum = options.SiemensPLCTypeEnum;
        //    txtConnectionType.Text = options.ConnectionType.ToString();
        //    txtLocalTASP.Text = options.LocalTSAP.ToString();
        //}
    }
}
