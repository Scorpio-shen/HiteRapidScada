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
using Scada.UI;

namespace Scada.Comm.Devices.Mqtt.UI
{
    public partial class FrmMqttSubCmds : Form
    {
        // 定义五中类型的数据
        private bool modified; // 窗口对应的值被修改 此值置位true
        private string strOldTopic = "";
        private int nOldQosLevel = -1;
        private int nOldOutputChannel = -1;
        private int nOldCmdType = -1;
        public AppDirs appDirs;
        public int nKpNum;

        public List<MqttSubCmd> SubCmds;
        int nSelIndex ;

        public static bool ShowDialog(ref MqttSubCmd subCmd, AppDirs appDirs,int KpNum, List<MqttSubCmd> SubCmds, int nSelIndex)
        {
            if (subCmd == null)
                throw new ArgumentNullException("subCmd");

            FrmMqttSubCmds form = new FrmMqttSubCmds();
            form.appDirs = appDirs;
            form.nKpNum = KpNum;
            form.SubCmds = SubCmds;
            form.nSelIndex = nSelIndex;

            form.SettingsToControls(subCmd);

            if (form.ShowDialog() == DialogResult.OK)
            {
                form.ControlsToSettings(subCmd);
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
        private void SettingsToControls(MqttSubCmd subCmd)
        {
            if (Localization.UseRussian)
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
            nOldQosLevel = comboBox_QosLevel.SelectedIndex = (int)subCmd.QosLevel;

            strOldTopic = textBox_Topic.Text = subCmd.TopicName;
            LoadConfigCtrlCnl();
            comboBox_Channel.Text = subCmd.NumCnlCtrl.ToString();

            nOldOutputChannel = subCmd.NumCnlCtrl;

            nOldCmdType = comboBox_CmdType.SelectedIndex = (int)subCmd.CmdType;

            modified = false;
        }

        private void ControlsToSettings(MqttSubCmd subCmd)
        {
            subCmd.QosLevel = (StriderMqtt.MqttQos)comboBox_QosLevel.SelectedIndex;

            subCmd.TopicName = textBox_Topic.Text;

            subCmd.NumCnlCtrl = int.Parse(comboBox_Channel.Text);

            subCmd.CmdType = (CmdType)comboBox_CmdType.SelectedIndex;



            if (comboBox_QosLevel.SelectedIndex != nOldQosLevel 
                || subCmd.TopicName != strOldTopic 
                || int.Parse(comboBox_Channel.Text) != nOldOutputChannel 
                || comboBox_CmdType.SelectedIndex != nOldCmdType )
            {
                modified = true;
            }

        }

        public FrmMqttSubCmds()
        {
            InitializeComponent();
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {

            for (int index = 0; index < SubCmds.Count; index++)
            {
                if (nSelIndex != index)
                {
                    if (textBox_Topic.Text == SubCmds[index].TopicName)
                    {
                        MessageBox.Show(string.Format(Localization.UseRussian ?
                            "{0} 命名重复" : "The Topic Name {0} was Repeated", textBox_Topic.Text));

                        return;
                    }
                }
            }

            this.DialogResult = DialogResult.OK;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {

        }

        private void FrmMqttSubCmds_Load(object sender, EventArgs e)
        {
            Translator.TranslateForm(this, "Scada.Comm.Devices.Mqtt.UI.FrmMqttSubCmds");

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
                if (cnl.KPNum != nKpNum)
                    comboBox_Channel.Items.Add(cnl.CtrlCnlNum);
            }
        }
    }
}
