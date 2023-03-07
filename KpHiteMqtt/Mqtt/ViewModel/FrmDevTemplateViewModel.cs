using HslCommunication.MQTT;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KpHiteMqtt.Mqtt.ViewModel
{
    public class FrmDevTemplateViewModel : INotifyPropertyChanged
    {
        private string clientId;
        public string ClientID
        {
            get => clientId;
            set
            {
                clientId = value;
                OnPropertyChanged(nameof(ClientID));
            }
        }

        private string mqttserverip;
        public string MqttServerIp
        {
            get => mqttserverip;
            set
            {
                mqttserverip = value;
                OnPropertyChanged(nameof(MqttServerIp));    
            }
        }

        private int mqttserverport;
        public int MqttServerPort
        {
            get => mqttserverport;
            set
            {
                mqttserverport = value;
                OnPropertyChanged(nameof(MqttServerPort));
            }
        }

        private string username;
        public string UserName
        {
            get => username;
            set
            {
                username = value;
                OnPropertyChanged(nameof(UserName));
            }
        }

        private string password;
        public string Password
        {
            get => password;
            set
            {
                password = value;
                OnPropertyChanged(nameof(Password));
            }
        }

        private double heartinterval;
        public double HeartInterval
        {
            get => heartinterval;
            set
            {
                heartinterval = value;
                OnPropertyChanged(nameof(HeartInterval));
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string proName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(proName));
        }
    }

}
