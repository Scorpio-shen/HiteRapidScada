using KpHiteMqtt.Mqtt.Model;
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

namespace KpHiteMqtt.Mqtt.View
{
    public partial class FrmTopics : Form
    {
        DeviceTemplate _deviceTemplate;
        TreeNode nodeSubscribe;
        TreeNode nodePublish;
        List<string> systemTopics = new List<string>();
        public FrmTopics(DeviceTemplate deviceTemplate)
        {
            InitializeComponent();
            _deviceTemplate = deviceTemplate;

            nodePublish = tvTopics.Nodes["nodePublish"];
            nodeSubscribe = tvTopics.Nodes["nodeSubscribe"];

            //添加Scada自带Topics

            //nodePublish.Nodes.Add(ScadaSystemTopics.MqttTsModelData_Publish);
            //nodePublish.Nodes.Add(ScadaSystemTopics.MqttCmdReply_Publish);

            //nodeSubscribe.Nodes.Add(ScadaSystemTopics.MqttTsModelDataReply_Subscribe);
            //nodeSubscribe.Nodes.Add(ScadaSystemTopics.MqttCmd_Subscribe);

            systemTopics.AddRange(new List<string>
            {
                ScadaSystemTopics.MqttTsModelData_Publish,
                ScadaSystemTopics.MqttCmdReply_Publish,
                ScadaSystemTopics.MqttTsModelDataReply_Subscribe,
                ScadaSystemTopics.MqttCmd_Subscribe
            });

           
            
        }

        private void FrmTopics_Load(object sender, EventArgs e)
        {
            foreach(var publish in _deviceTemplate.PublishTopics)
            {
                nodePublish.Nodes.Add(publish,publish);
            }

            foreach(var subscribe in _deviceTemplate.SubscribeTopics)
            {
                nodeSubscribe.Nodes.Add(subscribe,subscribe);
            }
        }

        private void btnSubscribe_Click(object sender, EventArgs e)
        {
            string topic = txtTopic.Text;
            //判断是否已存在该Topic
            if (nodeSubscribe.Nodes.ContainsKey(topic))
            {
                ScadaUiUtils.ShowError("该订阅Topic已存在!");
                return;
            }
            nodeSubscribe.Nodes.Add(topic, topic);
            //自动添加回复Topic
            var replyTopic = topic + "_reply";
            nodePublish.Nodes.Add(replyTopic, replyTopic);
        }

        private void btnPublish_Click(object sender, EventArgs e)
        {
            string topic = txtTopic.Text;
            nodePublish.Nodes.Add(topic);   
        }

        private void btnConfirm_Click(object sender, EventArgs e)
        {
            _deviceTemplate.PublishTopics.Clear();
            _deviceTemplate.SubscribeTopics.Clear();

            foreach(TreeNode publish in nodePublish.Nodes)
            {
                _deviceTemplate.PublishTopics.Add(publish.Text);
            }
            
            foreach(TreeNode subscribe in nodeSubscribe.Nodes)
            {
                _deviceTemplate.SubscribeTopics.Add(subscribe.Text);
            }
            DialogResult = DialogResult.OK;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void tspDeleteNode_Click(object sender, EventArgs e)
        {
            var selectNode = tvTopics.SelectedNode;
            if (selectNode == null)
            {
                ScadaUiUtils.ShowError("当前未选中任何节点!");
                return;
            }

            //判断是否选中不能删除节点
            if (selectNode == nodePublish || selectNode == nodeSubscribe)
            {
                return;
            }

            if (systemTopics.Contains(selectNode.Text))
            {
                ScadaUiUtils.ShowError("当前选中Topic是系统默认Topic，不允许删除!");
                return;
            }

            if(MessageBox.Show("是否确定要删除该Topic", "删除", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }
            if (selectNode.Parent == nodePublish)
            {
                nodePublish.Nodes.Remove(selectNode);
            }
            else if (selectNode.Parent == nodeSubscribe)
            {
                nodeSubscribe.Nodes.Remove(selectNode);
            }
            
            
        }
    }
}
