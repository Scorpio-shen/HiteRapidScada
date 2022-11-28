using KpMyModbus.Modbus.Protocol;
using KpMyModbus.Modbus.ViewModel;
using Scada.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KpMyModbus.Modbus.UI
{
    public partial class TagCmdControl : UserControl
    {
        [Category("Property Changed")]
        public event ObjectChangedEventHandler ObjectChanged;
        public TagCmdControlViewModel ViewModel { get; set; }

        private ModbusCmd modbusCmd;
        public ModbusCmd ModbusCmd
        {
            get=>modbusCmd;
            set
            {
                modbusCmd = value;
                ShowCmdProps(modbusCmd);
            }
        }
        public TagCmdControl()
        {
            InitializeComponent();
            ViewModel = new TagCmdControlViewModel();
            txtCmdName.DataBindings.Add(nameof(txtCmdName.Text),ViewModel,nameof(ViewModel.CmdName));
            txtCmdFuncCode.DataBindings.Add(nameof(txtCmdFuncCode.Text), ViewModel, nameof(ViewModel.FunctionCode));
            numCmdAddress.DataBindings.Add(nameof(numCmdAddress.Value), ViewModel, nameof(ViewModel.TagAddress));
            numCmdNum.DataBindings.Add(nameof(numCmdNum.Value), ViewModel, nameof(ViewModel.CmdNum));
            numCmdTagCnt.DataBindings.Add(nameof(numCmdTagCnt.Value), ViewModel, nameof(ViewModel.TagCount));
        }

        private void ShowCmdProps(ModbusCmd cmd)
        {
            ShowFuncCode(cmd);
            if(cmd == null)
            {
                ViewModel.CmdName = String.Empty;
                ViewModel.TagAddress = 1;
                ViewModel.TagCount = 1;
                ViewModel.CmdNum = 1;
                cbCmdTagType.SelectedIndex = 0;
                cbGrRegisterType.SelectedIndex = 0;
            }
            else
            {
                ViewModel.CmdName = cmd.Name;
                ViewModel.TagAddress = cmd.Address;
                ViewModel.TagCount = cmd.TagCount;
                ViewModel.CmdNum = cmd.CmdNum;
                cbCmdTagType.SelectedIndex = (int)cmd.TagDataType;
                cbGrRegisterType.SelectedIndex = (int)cmd.ModbusRegisterType;
            }
        }

        private void ShowFuncCode(ModbusCmd cmd)
        {
            ViewModel.FunctionCode = cmd == null ? "" :
                string.Format("{0} ({1}H)", modbusCmd.FuncCode, modbusCmd.FuncCode.ToString("X2"));
        }

        private void OnOjbectChanged(object changeArgument)
        {
            ObjectChanged?.Invoke(this, new ObjectChangedEventArgs(modbusCmd, changeArgument));
        }
    }
}
