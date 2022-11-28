using KpHiteModbus.Modbus.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KpHiteModbus.Modbus.View
{
    public partial class FrmParaSet : Form
    {
        ConnectionOptions _connectionOptions;
        public FrmParaSet(ConnectionOptions connectionOptions)
        {
            InitializeComponent();
            _connectionOptions = connectionOptions;
        }

        private void FrmParaSet_Load(object sender, EventArgs e)
        {
            gbxSerial.Enabled = _connectionOptions.ConnectionType == Model.EnumType.ModbusConnectionTypeEnum.SerialPort;
            gbxTcp.Enabled = _connectionOptions.ConnectionType != Model.EnumType.ModbusConnectionTypeEnum.SerialPort;
            //获取所有串口
            cbxSerialPortName.Items.AddRange(SerialPort.GetPortNames());

            BindControl();
        }

        private void BindControl()
        {
            txtIPAddress.DataBindings.Add(nameof(txtIPAddress.Text), _connectionOptions, nameof(_connectionOptions.IPAddress));
            txtPort.DataBindings.Add(nameof(txtPort.Text),_connectionOptions, nameof(_connectionOptions.Port));

            cbxSerialPortName.DataBindings.Add(nameof(cbxSerialPortName.SelectedValue), _connectionOptions, nameof(_connectionOptions.PortName));
            cbxBaudRate.DataBindings.Add(nameof(cbxBaudRate.SelectedValue),_connectionOptions, nameof(_connectionOptions.BaudRate));
            cbxDataBits.DataBindings.Add(nameof(cbxDataBits.SelectedValue), _connectionOptions, nameof(_connectionOptions.DataBits));
            cbxParity.DataBindings.Add(nameof(cbxParity.SelectedValue), _connectionOptions, nameof(_connectionOptions.Parity));
            cbxStopBits.DataBindings.Add(nameof(cbxStopBits.SelectedValue), _connectionOptions, nameof(_connectionOptions.StopBits));
        }
    }
}
