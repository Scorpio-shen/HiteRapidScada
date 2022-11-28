using Scada;
using Scada.Extend;
using System;
using System.ComponentModel;
using System.Security.AccessControl;
using System.Xml;

namespace KpSiemens.Siemens.Model
{
    public class PLCConnectionOptions : INotifyPropertyChanged
    {
        private string ipaddress;
        public string IPAddress
        {
            get => ipaddress;
            set
            {
                ipaddress = value;
                OnPropertyChanged(nameof(IPAddress));
            }
        }
        

        private SiemensPLCTypeEnum siemensPLCType = SiemensPLCTypeEnum.S1200;

        public SiemensPLCTypeEnum SiemensPLCTypeEnum
        {
            get => siemensPLCType;
            set
            {
                siemensPLCType = value;
                OnPropertyChanged(nameof(SiemensPLCTypeEnum));
            }
        }

        private int port = 102;
        public int Port
        {
            get => port;
            set
            {
                port = value;
                OnPropertyChanged(nameof(Port));
            }
        }
        private byte rack;
        public byte Rack
        {
            get => rack;
            set
            {
                rack = value;
                OnPropertyChanged(nameof(Rack));
            }
        }
        private byte slot;
        public byte Slot
        {
            get => slot;
            set
            {
                slot = value;
                OnPropertyChanged(nameof(Slot));
            }
        }
        private byte connectiontype = 1;
        public byte ConnectionType
        {
            get => connectiontype;
            set
            {
                connectiontype = value;
                OnPropertyChanged(nameof(ConnectionType));
            }
        }
        private int localtasp = 258;
        public int LocalTSAP
        {
            get => localtasp;
            set
            {
                localtasp = value;
                OnPropertyChanged(nameof(LocalTSAP));
            }
        }

        private int desttasp;
        public int DestTASP
        {
            get => desttasp;
            set
            {
                desttasp = value;
                OnPropertyChanged(nameof(DestTASP));
            }
        }

        public void LoadFromXml(XmlElement optionElement)
        {
            if (optionElement == null)
                throw new ArgumentNullException("OptionElement");

            IPAddress = optionElement.GetAttrAsString("IPAddress");
            Port = optionElement.GetAttrAsInt("Port");
            Rack = optionElement.GetAttrAsString("Rack").ToByte();
            Slot = optionElement.GetAttrAsString("Slot").ToByte();
            DestTASP = optionElement.GetAttrAsInt("DestTASP");
            LocalTSAP = optionElement.GetAttrAsInt("LocalTSAP");
            ConnectionType = optionElement.GetAttrAsString("ConnectionType").ToByte();
            SiemensPLCTypeEnum = optionElement.GetAttrAsEnum("SiemensPLCTypeEnum", SiemensPLCTypeEnum.S1200);
        }

        public void SaveToXml(XmlElement optionElement)
        {
            if (optionElement == null)
                throw new ArgumentNullException("OptionElement");

            optionElement.SetAttribute("IPAddress", IPAddress);
            optionElement.SetAttribute("Port",Port);
            optionElement.SetAttribute("Rack",Rack);
            optionElement.SetAttribute("Slot", Slot);
            optionElement.SetAttribute("DestTASP", DestTASP);
            optionElement.SetAttribute("LocalTSAP", LocalTSAP);
            optionElement.SetAttribute("ConnectionType", ConnectionType);
            optionElement.SetAttribute("SiemensPLCTypeEnum", SiemensPLCTypeEnum);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName = null)
        {
            var eventHandler = PropertyChanged;
            if (eventHandler != null)
            {
                eventHandler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
    /// <summary>
    /// PLC类型枚举
    /// </summary>
    public enum SiemensPLCTypeEnum
    {
        /// <summary>
        /// 200系统，需要额外配置以太网模块
        /// </summary>
        S200 = 6,
        /// <summary>
        /// 200的smart系列
        /// </summary>
        S200Smart = 5,
        /// <summary>
        /// 300系列
        /// </summary>
        S300 = 2,
        /// <summary>
        /// 400系列
        /// </summary>
        S400 = 3,
        /// <summary>
        /// 1200系列
        /// </summary>
        S1200 = 1,
        /// <summary>
        /// 1500系列PLC
        /// </summary>
        S1500 = 4,
       
    }
}
