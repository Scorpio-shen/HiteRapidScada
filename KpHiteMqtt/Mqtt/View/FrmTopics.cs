using KpHiteMqtt.Mqtt.Model;
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
        public FrmTopics(DeviceTemplate deviceTemplate)
        {
            InitializeComponent();
            _deviceTemplate = deviceTemplate;
            nodePublish = tvTopics.Nodes["nodePublish"];
            nodeSubscribe = tvTopics.Nodes["nodeSubscribe"];
        }

        private void FrmTopics_Load(object sender, EventArgs e)
        {
            foreach(var publish in _deviceTemplate.PublishTopics)
            {
                nodePublish.Nodes.Add(publish);
            }

            foreach(var subscribe in _deviceTemplate.SubscribeTopics)
            {
                nodeSubscribe.Nodes.Add(subscribe);
            }
        }

        private void btnSubscribe_Click(object sender, EventArgs e)
        {
            string topic = txtTopic.Text;
            nodeSubscribe.Nodes.Add(topic);
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
    }
}
