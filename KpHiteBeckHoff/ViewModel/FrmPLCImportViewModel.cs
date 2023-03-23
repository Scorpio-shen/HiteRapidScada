using HslCommunication.Profinet.AllenBradley;
using KpHiteBeckHoff.Model;
using KpHiteBeckHoff.Model.EnumType;
using System.Collections.Generic;
using System.ComponentModel;

namespace KpHiteBeckHoff.ViewModel
{
    public class FrmPLCImportViewModel : INotifyPropertyChanged
    {
        private string ipaddress;
        public string IpAddress
        {
            get { return ipaddress; }
            set { ipaddress = value; OnPropertyChanged(nameof(IpAddress)); }
        }
        private int port;
        public int Port
        {
            get { return port; }
            set
            {
                port= value; OnPropertyChanged(nameof(Port));
            }
        }
        private ProtocolTypeEnum protocoltype;

        public ProtocolTypeEnum ProtocolType
        {
            get => protocoltype;
            set
            {
                protocoltype = value;
                OnPropertyChanged(nameof(ProtocolType));
            }
        }

        public AllenBradleyNet AllenBradleyNet { get; set; }

        private bool isconnected;
        public bool IsConnected
        {
            get => isconnected;
            set
            {
                isconnected = value;
                OnPropertyChanged(nameof(IsConnected));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string proName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(proName));
        }
        /// <summary>
        /// Tag点集合
        /// </summary>
        public List<Tag> Tags { get; set; }
        /// <summary>
        /// 存储数组Tag的父Tag
        /// </summary>
        public List<Tag> ParentTags { get; set; }
    }
}
