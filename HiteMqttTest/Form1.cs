using HslCommunication.MQTT;
using KpCommon.Extend;
using KpCommon.Helper;
using KpHiteMqtt.Mqtt;
using KpHiteMqtt.Mqtt.Model;
using KpHiteMqtt.Mqtt.Model.Response;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
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
        DataModel dataModel;
        public Form1()
        {
            InitializeComponent();
            dataModel = new DataModel(this);
            txtJsonModel.AddDataBindings(dataModel, nameof(dataModel.ShowContent));
            txtCmdReply.AddDataBindings(dataModel,nameof(dataModel.CmdReply));
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
                ReviceModelData(hitemqttpayload);
            }
            else if(topic == ScadaSystemTopics.MqttCmdReply_Publish)
            {
                ReceiveCmdReply(hitemqttpayload);
            }
        }

        private void ReviceModelData(HiteMqttPayload payload)
        {
            var contents = JsonConvert.DeserializeObject<List<MqttModelContent>>(payload.Content);
            dataModel.ShowContent = JsonConvert.SerializeObject(contents);

        }

        private void ReceiveCmdReply(HiteMqttPayload payload)
        {
            var content = JsonConvert.DeserializeObject<MqttResponse>(payload.Content);
            var cmdResponses = JsonConvert.DeserializeObject<List<MqttCmdResponse>>(content.Content);
            dataModel.CmdReply = JsonConvert.SerializeObject(cmdResponses);
        }

        private void MqttClient_OnClientConnected(MqttClient client)
        {
            Debug.WriteLine("Mqtt连接成功!");
            //进行订阅
            client.SubscribeMessage(ScadaSystemTopics.MqttTsModelData_Publish);
            client.SubscribeMessage(ScadaSystemTopics.MqttCmdReply_Publish);
        }

        private async void btnWrite_Click(object sender, EventArgs e)
        {
            if (mqttClient != null)
            {
                var modelContents = JsonConvert.DeserializeObject<List<MqttModelContent>>(txtModelWrite.Text);   
                await PublishData(modelContents,ScadaSystemTopics.MqttCmd_Subscribe);
            }
        }

        private async Task PublishData(List<MqttModelContent> modelContents,string topic)
        {
            try
            {
                HiteMqttPayload mqttPayload = new HiteMqttPayload();
                mqttPayload.MessageId = SnowflakeHelper.GetNewId().ToString();
                mqttPayload.Time = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
                mqttPayload.Content = JsonConvert.SerializeObject(modelContents);
                var payload = JsonConvert.SerializeObject(mqttPayload);
                var result = await PublishAsync(
                    topic: topic,
                    payload: Encoding.UTF8.GetBytes(EncryptionHelper.Base64Encode(payload)),
                    mqttQuality: MqttQualityOfServiceLevel.AtMostOnce);

                Debug.WriteLine($"Form1:PublishData,发布数据,Content:{payload},Result:{result}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Form1:PublishData,发布数据异常,{ex.Message}");
            }
        }

        public async Task<bool> PublishAsync(string topic, byte[] payload, MqttQualityOfServiceLevel mqttQuality = MqttQualityOfServiceLevel.AtMostOnce, bool retain = false)
        {
            if (!mqttClient.IsConnected)
                return false;
            var result = await mqttClient.PublishMessageAsync(new MqttApplicationMessage
            {
                Topic = topic,
                Payload = payload,
                QualityOfServiceLevel = mqttQuality,
                Retain = retain
            });
            return result.IsSuccess;
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            if(mqttClient != null)
            {
                var result = mqttClient.ConnectServer();
                Debug.WriteLine(result.IsSuccess);
            }
        }
    }

    public class DataModel : INotifyPropertyChanged
    {
        Control _form;

        public DataModel(Control form)
        {
            _form = form;   
        }

        private string showcontent;
        public string ShowContent
        {
            get => showcontent;
            set
            {
                showcontent = value;
                OnPropertyChanged(nameof(ShowContent));
            }
        }

        public string cmdreply;
        public string CmdReply
        {
            get => cmdreply;
            set
            {
                cmdreply = value;
                OnPropertyChanged(nameof(CmdReply));
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string proName)
        {
            if (_form.InvokeRequired)
            {
                _form.Invoke(new Action(() =>
                {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(proName));
                }));
            }
            
        }
    }
}
