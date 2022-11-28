using KpSiemens.Siemens.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KpSiemens.Siemens.View
{
    public partial class FrmAdvancedSetting : Form
    {
        PLCConnectionOptions _connectionOptions = null;
        Action<object, PLCConnectionOptionsEnum> _OnPLCConfigChanged = null;
        bool isLoad = false;
        //public FrmAdvancedSetting()
        //{
        //    InitializeComponent();
        //}

        public FrmAdvancedSetting(PLCConnectionOptions connectionOptions, Action<object, PLCConnectionOptionsEnum> OnPLCConfigChanged)
        {
            InitializeComponent();
            _connectionOptions = connectionOptions;
            _OnPLCConfigChanged = OnPLCConfigChanged;   
        }

        private void FrmAdvancedSetting_Load(object sender, EventArgs e)
        {
            isLoad = true;
            BindControls(_connectionOptions);
            ShowControlsByType(_connectionOptions.SiemensPLCTypeEnum);
            isLoad = false;
        }

        private void BindControls(PLCConnectionOptions options)
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

            if (options == null)
                return;
            txtIPAddress.DataBindings.Clear();
            txtPort.DataBindings.Clear();
            txtRack.DataBindings.Clear();
            txtSlot.DataBindings.Clear();
            cbxPLCType.DataBindings.Clear();
            txtConnectionType.DataBindings.Clear();
            txtLocalTASP.DataBindings.Clear();
            txtDestTASP.DataBindings.Clear();

            txtIPAddress.DataBindings.Add(nameof(txtIPAddress.Text), options, nameof(options.IPAddress));
            txtPort.DataBindings.Add(nameof(txtPort.Text), options, nameof(options.Port));
            txtRack.DataBindings.Add(nameof(txtRack.Text), options, nameof(options.Rack));
            txtSlot.DataBindings.Add(nameof(txtSlot.Text), options, nameof(options.Slot));
            cbxPLCType.DataBindings.Add(nameof(cbxPLCType.SelectedValue), options, nameof(options.SiemensPLCTypeEnum));
            txtConnectionType.DataBindings.Add(nameof(txtConnectionType.Text), options, nameof(options.ConnectionType));
            txtLocalTASP.DataBindings.Add(nameof(txtLocalTASP.Text), options, nameof(options.LocalTSAP));
            txtDestTASP.DataBindings.Add(nameof(txtDestTASP.Text), options, nameof(options.DestTASP));
        }

        private void ShowControlsByType(SiemensPLCTypeEnum plcType)
        {
            switch (plcType)
            {
                case SiemensPLCTypeEnum.S200Smart:
                    txtRack.Visible = false;
                    txtSlot.Visible = false;
                    txtConnectionType.Visible = false;
                    txtLocalTASP.Visible = false;
                    txtDestTASP.Visible = false;

                    lblRack.Visible = false;
                    lblSlot.Visible = false;
                    lblConnectionType.Visible = false;
                    lblLocalTASP.Visible = false;
                    lblDestTASP.Visible = false;
                    break;
                case SiemensPLCTypeEnum.S200:
                    txtRack.Visible = false;
                    txtSlot.Visible = false;
                    txtConnectionType.Visible = false;
                    txtLocalTASP.Visible = true;
                    txtDestTASP.Visible = true;

                    lblRack.Visible = false;
                    lblSlot.Visible = false;
                    lblConnectionType.Visible = false;
                    lblLocalTASP.Visible = true;
                    lblDestTASP.Visible = true;
                    break;
                default:
                    txtRack.Visible = true;
                    txtSlot.Visible = true;
                    txtConnectionType.Visible = true;
                    txtLocalTASP.Visible = true;
                    txtDestTASP.Visible = false;

                    lblRack.Visible = true;
                    lblSlot.Visible = true;
                    lblConnectionType.Visible = true;
                    lblLocalTASP.Visible = true;
                    lblDestTASP.Visible = false;
                    break;
            }
        }

        private void btnConfirm_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }

        #region 控件事件
        private void btnCancle_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void txtPort_TextChanged(object sender, EventArgs e)
        {
            if (isLoad)
                return;
            _OnPLCConfigChanged?.Invoke(sender, PLCConnectionOptionsEnum.Port);
        }

        private void txtRack_TextChanged(object sender, EventArgs e)
        {
            if (isLoad)
                return;
            _OnPLCConfigChanged?.Invoke(sender, PLCConnectionOptionsEnum.Rack);
        }

        private void txtSlot_TextChanged(object sender, EventArgs e)
        {
            if (isLoad)
                return;
            _OnPLCConfigChanged?.Invoke(sender, PLCConnectionOptionsEnum.Slot);
        }

        private void txtConnectionType_TextChanged(object sender, EventArgs e)
        {
            if (isLoad)
                return;
            _OnPLCConfigChanged?.Invoke(sender, PLCConnectionOptionsEnum.ConnectionType);
        }

        private void txtLocalTASP_TextChanged(object sender, EventArgs e)
        {
            if (isLoad)
                return;
            _OnPLCConfigChanged?.Invoke(sender, PLCConnectionOptionsEnum.LocalTASP);
        }

        private void txtDestTASP_TextChanged(object sender, EventArgs e)
        {
            if (isLoad)
                return;
            _OnPLCConfigChanged?.Invoke(sender, PLCConnectionOptionsEnum.DestTASP);
        }
        #endregion

    }
}
