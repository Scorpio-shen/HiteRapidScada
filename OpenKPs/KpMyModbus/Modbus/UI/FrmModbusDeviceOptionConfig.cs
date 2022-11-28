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
    public partial class FrmModbusDeviceOptionConfig : Form
    {
        private DeviceTemplate _deviceTemplate;
        public FrmModbusDeviceOptionConfig(DeviceTemplate deviceTemplate)
        {
            InitializeComponent();
            _deviceTemplate = deviceTemplate;

            LoadFromTemplete(_deviceTemplate);
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            _deviceTemplate.ConnectionOptions.ServerPort = txtPort.Text;
            _deviceTemplate.ConnectionOptions.ServerIpAddress = txtIpAddress.Text;
            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void LoadFromTemplete(DeviceTemplate template)
        {
            if(template.ConnectionOptions != null)
            {
                txtIpAddress.Text = template.ConnectionOptions.ServerIpAddress;
                txtPort.Text = template.ConnectionOptions.ServerPort;
            }
        }
    }
}
