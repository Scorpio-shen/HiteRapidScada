using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Scada.Comm.Devices.Mqtt.Config;
using Scada.UI;


namespace Scada.Comm.Devices.Mqtt.UI
{
    public partial class FrmMqttPubCmds : Form
    {
        // 定义五中类型的数据
        private bool modified; // 窗口对应的值被修改 此值置位true
        private string strOldTopic = "";
        private int nOldQosLevel = -1;
        private int nOldOutputChannel = -1;
        private AppDirs appDirs;                 // 应用程序目录
        private int nKpNum;
        List<MqttPubCmd> PubCmds;
        int nSelIndex;
        public static bool ShowDialog(ref MqttPubCmd pubCmd, AppDirs appDirs,int KpNum, List<MqttPubCmd> PubCmds, int nIndex)
        {
            if (pubCmd == null)
                throw new ArgumentNullException("pubCmd");

            FrmMqttPubCmds form = new FrmMqttPubCmds();
            form.appDirs = appDirs;
            form.nKpNum = KpNum;
            form.PubCmds = PubCmds;
            form.nSelIndex = nIndex;
            form.SettingsToControls(pubCmd);
            if (form.ShowDialog() == DialogResult.OK)
            {
                form.ControlsToSettings(pubCmd);

                if (form.modified == true)
                {
                    

                    return true;
                }

                else return false;
            }
            else
            {
                return false;
            }
        }
        private void SettingsToControls(MqttPubCmd pubCmd)
        {
            // 在这里添加
       
            if(Localization.UseRussian)
            {              
                comboBox_QosLevel.Items.Insert(0, "最多一次");
                comboBox_QosLevel.Items.Insert(1, "至少一次");
                comboBox_QosLevel.Items.Insert(2, "只有一次");

            }
            else
            {
                comboBox_QosLevel.Items.Insert(0, "AtMostOnce");
                comboBox_QosLevel.Items.Insert(1, "AtLeastOnce");
                comboBox_QosLevel.Items.Insert(2, "ExactlyOnce");
            }
               
            nOldQosLevel = comboBox_QosLevel.SelectedIndex = (int)pubCmd.QosLevel;

            strOldTopic = textBox_Topic.Text = pubCmd.TopicName;
            LoadConfigCtrlCnl();
            comboBox_Channel.Text = pubCmd.NumCmd.ToString();

            nOldOutputChannel = pubCmd.NumCmd;



            modified = false;
        }

        private void ControlsToSettings(MqttPubCmd pubCmd)
        {
            pubCmd.QosLevel = (StriderMqtt.MqttQos)comboBox_QosLevel.SelectedIndex;

            pubCmd.TopicName = textBox_Topic.Text;

            pubCmd.NumCmd = int.Parse(comboBox_Channel.Text);

            if (comboBox_QosLevel.SelectedIndex != nOldQosLevel 
                || pubCmd.TopicName != strOldTopic 
                || int.Parse(comboBox_Channel.Text) != nOldOutputChannel 
                 )
            {
                modified = true;
            }

        }

        public FrmMqttPubCmds()
        {
            InitializeComponent();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {

        }

        private void FrmMqttPubCmds_Load(object sender, EventArgs e)
        {
            // 加载配置文件 读取配置信息
            Translator.TranslateForm(this, "Scada.Comm.Devices.Mqtt.UI.FrmMqttPubCmds");

        }
        private void LoadConfigCtrlCnl()
        {
            string strbaseXml = "";
            int nIndex = appDirs.ConfigDir.IndexOf("Instances");
            strbaseXml = appDirs.ConfigDir.Substring(0, nIndex) + "BaseXML\\CtrlCnl.xml";

            List<CtrlCnl> listCtrlCnl = new List<CtrlCnl>();
            CXMLOperator operatorXml = new CXMLOperator();

            listCtrlCnl = operatorXml.GetConfig<List<CtrlCnl>>(strbaseXml);

            foreach (CtrlCnl cnl in listCtrlCnl)
            {
                if (cnl.KPNum == nKpNum)
                    comboBox_Channel.Items.Add(cnl.CmdNum);
            }
        }
        private void buttonOK_Click(object sender, EventArgs e)
        {

            for (int index = 0; index < PubCmds.Count; index++)
            {
                if (nSelIndex != index)
                {
                    if (textBox_Topic.Text == PubCmds[index].TopicName)
                    {

                        MessageBox.Show(string.Format(Localization.UseRussian ? 
                            "{0} 命名重复" : "The Topic Name {0} was Repeated",textBox_Topic.Text));
                             
                        
                        return;
                    }
                }
            }
          
            this.DialogResult = DialogResult.OK;
        }
    }
}
