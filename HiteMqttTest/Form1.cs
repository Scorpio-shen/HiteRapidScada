using HslCommunication.MQTT;
using KpCommon.Helper;
using KpHiteMqtt.Mqtt;
using KpHiteMqtt.Mqtt.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HiteMqttTest
{
    public partial class Form1 : Form
    {
        DeviceTemplate deviceTemplate;
        MqttClient mqttClient;
        public Form1()
        {
            InitializeComponent();
        }

        private void btnLoad_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            var result = openFileDialog.ShowDialog();
            if(result == DialogResult.OK)
            {
                var fileName = openFileDialog.FileName;
                deviceTemplate = new DeviceTemplate();
                deviceTemplate.Load(fileName, out string errMsg);

                //连接Mqtt服务器
                deviceTemplate.ConnectionOptions.ClientId = new Guid().ToString();
                deviceTemplate.ConnectionOptions.Credentials.UserName = "ClientTest";
                deviceTemplate.ConnectionOptions.Credentials.Password = "ClientPassword";
                mqttClient = new MqttClient(deviceTemplate.ConnectionOptions);
                mqttClient.OnClientConnected += MqttClient_OnClientConnected;
                mqttClient.OnMqttMessageReceived += MqttClient_OnMqttMessageReceived;
            }
        }

        private void MqttClient_OnMqttMessageReceived(MqttClient client, string topic, byte[] payload)
        {
            var content = Encoding.UTF8.GetString(payload);
            var decodeContent = EncryptionHelper.Base64Decode(content);

            var hitemqttpayload = JsonConvert.DeserializeObject<HiteMqttPayload>(decodeContent);
            if(topic == ScadaSystemTopics.MqttTsModelData_Publish)
            {
            }
            else if(topic == ScadaSystemTopics.MqttCmdReply_Publish)
            {

            }
        }

        private void ReviceModelData(HiteMqttPayload payload)
        {
            var contents = JsonConvert.DeserializeObject<List<MqttModelContent>>(payload.Content);
            txtJsonModel.Text = JsonConvert.SerializeObject(contents,new JsonSerializerSettings
            {
                d
            });
        }

        private void MqttClient_OnClientConnected(MqttClient client)
        {
            //进行订阅
            client.SubscribeMessage(ScadaSystemTopics.MqttTsModelData_Publish);
            client.SubscribeMessage(ScadaSystemTopics.MqttCmdReply_Publish);
        }

        private void btnWrite_Click(object sender, EventArgs e)
        {
            if(mqttClient != null)
            {
                mqttClient.PublishMessage()
            }
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            if(mqttClient != null)
            {
                mqttClient.ConnectServer();

            }
        }
    }
}
