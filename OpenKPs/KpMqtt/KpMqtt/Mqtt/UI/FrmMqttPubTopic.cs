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
    public partial class FrmMqttPubTopic : Form
    {
        // 定义五中类型的数据
        private bool modified; // 窗口对应的值被修改 此值置位true
        private string strOldTopic = "";
        private int nOldQosLevel = -1;
        private int nOldInputChannel = -1;
        private int nOldBehavorIndex = -1;
        private int nOldRetainIndex = -1;

        private string strOldPrefix = "";
        private string strOldsuffix = "";

        private AppDirs appDirs;                 // 应用程序目录
        private int nKpNum;
        List<MqttPubTopic> PubTopics;
        int nSelIndex;
        public static bool ShowDialog(ref MqttPubTopic pubTopic,AppDirs appdirs,int KpNum, List<MqttPubTopic> PubTopics,int nSelIndex)
        {
            if (pubTopic == null)
                throw new ArgumentNullException("pubTopic");

            FrmMqttPubTopic form = new FrmMqttPubTopic();
            form.appDirs = appdirs;
            form.nKpNum = KpNum;
            form.nSelIndex = nSelIndex;
            form.PubTopics = PubTopics;
            form.SettingsToControls(pubTopic);

            if (form.ShowDialog() == DialogResult.OK)
            {
                form.ControlsToSettings(pubTopic);
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
        private void SettingsToControls(MqttPubTopic pubTopic)
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
            nOldQosLevel = comboBox_QosLevel.SelectedIndex = (int)pubTopic.QosLevel;

            strOldTopic = textBox_Topic.Text = pubTopic.TopicName;

            LoadConfigInCnl();

            comboBox_Channel.Text = pubTopic.NumCnl.ToString();
            nOldInputChannel = pubTopic.NumCnl;

            comboBox_Behavior.Text = pubTopic.PubBehavior.ToString();
            nOldBehavorIndex = comboBox_Behavior.SelectedIndex;

            comboBox_retain.Text = pubTopic.Retain.ToString();
            nOldRetainIndex = comboBox_retain.SelectedIndex;


            strOldPrefix =  textBox_Prefix.Text = pubTopic.Prefix;
            strOldsuffix = textBox_suffix.Text = pubTopic.Suffix;


            modified = false;
        }

        private void ControlsToSettings(MqttPubTopic pubTopic)
        {
            pubTopic.QosLevel = (StriderMqtt.MqttQos)comboBox_QosLevel.SelectedIndex;

            pubTopic.TopicName = textBox_Topic.Text;

            pubTopic.NumCnl = int.Parse(comboBox_Channel.Text);

            pubTopic.PubBehavior = (PubBehavior)comboBox_Behavior.SelectedIndex;

            pubTopic.Retain = bool.Parse(comboBox_retain.Text);

            pubTopic.Prefix = textBox_Prefix.Text;

            pubTopic.Suffix =  textBox_suffix.Text;

            if (comboBox_QosLevel.SelectedIndex != nOldQosLevel
                || pubTopic.TopicName != strOldTopic
                || int.Parse(comboBox_Channel.Text) != nOldInputChannel
                || nOldBehavorIndex != comboBox_Behavior.SelectedIndex
                || nOldRetainIndex != comboBox_retain.SelectedIndex
                || strOldPrefix != textBox_Prefix.Text
                || strOldsuffix != textBox_suffix.Text
                 )
            {
                modified = true;
            }

        }


        public FrmMqttPubTopic()
        {
            InitializeComponent();
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            // 除了自己之外的内容不能重复
           // int nSelIndex = selNode.Index;
            for (int index = 0; index < PubTopics.Count; index++)
            {
                if (nSelIndex != index)
                {
                    if (textBox_Topic.Text == PubTopics[index].TopicName)
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

        private void FrmMqttPubTopic_Load(object sender, EventArgs e)
        {
            Translator.TranslateForm(this, "Scada.Comm.Devices.Mqtt.UI.FrmMqttPubTopic");

        }
        private void LoadConfigInCnl()
        {
            string strbaseXml = "";
            int nIndex = appDirs.ConfigDir.IndexOf("Instances");
            strbaseXml = appDirs.ConfigDir.Substring(0, nIndex) + "BaseXML\\InCnl.xml";

            List<InCnl> listInCnl = new List<InCnl>();
            CXMLOperator operatorXml = new CXMLOperator();

            listInCnl = operatorXml.GetConfig<List<InCnl>>(strbaseXml);

            foreach (InCnl cnl in listInCnl)
            {
                if (cnl.KPNum != nKpNum)
                    comboBox_Channel.Items.Add(cnl.CnlNum);
            }
        }
    }
}
