using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Scada.Comm.Devices.Mqtt.Config;
using System.IO;
using StriderMqtt;
using Scada.UI;

namespace Scada.Comm.Devices.Mqtt.UI
{
    public partial class FrmMqttServerConfig : Form
    {
        // 定义五中类型的数据
        private bool modified; // 窗口对应的值被修改 此值置位true
        private string strOldHostname = "";
        private string strOldClientID = "";
        private int nOldPort = -1;
        private string strOldUserName = "";
        private string strOldPassword = "";
        public FrmMqttServerConfig()
        {
            InitializeComponent();
        }

        public static bool ShowDialog(ref MqttConnectionArgs connectConfig)
        {
            if (connectConfig == null)
                throw new ArgumentNullException("connectConfig");

            FrmMqttServerConfig form = new FrmMqttServerConfig();
          

            form.SettingsToControls(connectConfig);

            if (form.ShowDialog() == DialogResult.OK)
            {
                form.ControlsToSettings(connectConfig);
                if (form.modified == true)
                    return true;
                else return false;
            }
            else
            {
                return false;
            }
        }
        private void SettingsToControls(MqttConnectionArgs subJs)
        {

            strOldClientID = textBox_ClientID.Text = subJs.ClientId;

            strOldHostname = textBox_Hostname.Text = subJs.Hostname;


            textBox_Port.Text = subJs.Port.ToString();
            nOldPort = subJs.Port;


            strOldUserName = textBox_UserName.Text = subJs.Username;
            strOldPassword = textBox_Password.Text = subJs.Password;




            modified = false;
        }

        private void ControlsToSettings(MqttConnectionArgs subJs)
        {
            subJs.ClientId = textBox_ClientID.Text ;

            subJs.Hostname = textBox_Hostname.Text ;


            subJs.Port  = int.Parse(textBox_Port.Text);


            subJs.Username = textBox_UserName.Text ;
            subJs.Password = textBox_Password.Text ;



            if (subJs.Hostname != strOldHostname
                || subJs.ClientId != strOldClientID
                || subJs.Port != nOldPort
                || subJs.Username != strOldUserName
                || subJs.Password != strOldPassword)
            {
                modified = true;
            }

        }

        private void buttonOK_Click(object sender, EventArgs e)
        {

        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {

        }

        private void FrmMqttServerConfig_Load(object sender, EventArgs e)
        {
            Translator.TranslateForm(this, "Scada.Comm.Devices.Mqtt.UI.FrmMqttServerConfig");


        }


    }
}
