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
    public partial class FrmMqttSubTopics : Form
    {
        // 常规订阅主题 只配置2个变量
        private bool modified; // 窗口对应的值被修改 此值置位true
        private string strOldTopic = "";
        private int nOldQosLevel = -1;
        int nSelIndex;
        List<MqttSubTopic> SubTopics;
        public FrmMqttSubTopics()
        {
            InitializeComponent();
        }
        public static bool ShowDialog(ref MqttSubTopic subTopic, List<MqttSubTopic> SubTopics, int nSelIndex)
        {
            if (subTopic == null)
                throw new ArgumentNullException("subTopic");

            FrmMqttSubTopics form = new FrmMqttSubTopics();
            form.SubTopics = SubTopics;
            form.nSelIndex = nSelIndex;
            form.SettingsToControls(subTopic);

            if (form.ShowDialog() == DialogResult.OK )
            {
                form.ControlsToSettings(subTopic);
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
        private void SettingsToControls(MqttSubTopic subTopic)
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
            nOldQosLevel = comboBox_QosLevel.SelectedIndex = (int)subTopic.QosLevel;

            strOldTopic = textBox_Topic.Text = subTopic.TopicName;

            modified = false;
        }

        private void ControlsToSettings(MqttSubTopic subTopic)
        {
            subTopic.QosLevel = (StriderMqtt.MqttQos)comboBox_QosLevel.SelectedIndex  ;

            subTopic.TopicName = textBox_Topic.Text ;

            if(comboBox_QosLevel.SelectedIndex != nOldQosLevel ||
                subTopic.TopicName != strOldTopic)
            {
                modified = true;
            }

        }
        private void buttonOK_Click(object sender, EventArgs e)
        {
            // 除了自己之外的内容不能重复
            for (int index = 0; index < SubTopics.Count; index++)
            {
                if (nSelIndex != index)
                {
                    if (textBox_Topic.Text == SubTopics[index].TopicName)
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

        private void FrmMqttSubTopics_Load(object sender, EventArgs e)
        {
            Translator.TranslateForm(this, "Scada.Comm.Devices.Mqtt.UI.FrmMqttSubTopics");

        }
    }
}
