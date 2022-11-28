using KpHiteModbus.Modbus.Model.EnumType;
using Scada;
using Scada.Extend;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO.Ports;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TrackBar;

namespace KpHiteModbus.Modbus.Model
{
    public class ConnectionOptions : INotifyPropertyChanged
    {
        #region 公共参数
        private byte station;
        public byte Station
        {
            get => station;
            set
            {
                station = value;
                OnPropertyChanged(nameof(Station));
            }
        }

        private ModbusConnectionTypeEnum connectiontype;
        public ModbusConnectionTypeEnum ConnectionType
        {
            get => connectiontype;
            set
            {
                connectiontype = value;
                OnPropertyChanged(nameof(ConnectionType));
            }
        }

        private ModbusModeEnum modbusmode;
        public ModbusModeEnum ModbusMode
        {
            get => modbusmode;
            set
            {
                modbusmode = value;
                OnPropertyChanged(nameof(ModbusMode));
            }
        }
        #endregion

        #region TcpIP、Udp方式
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

        private int port = 502;
        public int Port
        {
            get => port;
            set
            {
                port = value;
                OnPropertyChanged(nameof(Port));
            }
        }
        #endregion

        #region SerialPort方法
        private string portname;
        public string PortName
        {
            get => portname;
            set
            {
                portname = value;
                OnPropertyChanged(nameof(PortName));
            }
        }

        private int baudrate;
        public int BaudRate
        {
            get => baudrate;
            set
            {
                baudrate = value;
                OnPropertyChanged(nameof(BaudRate));
            }
        }

        private int databits;
        public int DataBits
        {
            get => databits;
            set
            {
                databits = value;
                OnPropertyChanged(nameof(DataBits));
            }
        }

        private StopBits stopbits;
        public StopBits StopBits
        {
            get => stopbits;
            set
            {
                stopbits = value;
                OnPropertyChanged(nameof(StopBits));
            }
        }

        private Parity parity;
        public Parity Parity
        {
            get => parity;
            set
            {
                parity = value;
                OnPropertyChanged(nameof(Parity));
            }
        }
        #endregion

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName = null)
        {
            var eventHandler = PropertyChanged;
            if (eventHandler != null)
            {
                eventHandler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public virtual void LoadFromXml(XmlElement optionElement)
        {
            if (optionElement == null)
                throw new ArgumentNullException("OptionElement");
            Station = optionElement.GetAttrAsString("Station").ToByte();
            ConnectionType = optionElement.GetAttrAsEnum("ConnectionType", ModbusConnectionTypeEnum.TcpIP);
            ModbusMode = optionElement.GetAttrAsEnum("ModbusMode", ModbusModeEnum.Rtu);

            IPAddress = optionElement.GetAttrAsString("IPAddress");
            Port = optionElement.GetAttrAsInt("Port");

            PortName = optionElement.GetAttrAsString("PortName");
            BaudRate = optionElement.GetAttrAsInt("BaudRate");
            DataBits = optionElement.GetAttrAsInt("DataBits");
            StopBits = optionElement.GetAttrAsEnum("StopBits", StopBits.None);
            Parity = optionElement.GetAttrAsEnum("Parity", Parity.None);
        }
        public virtual void SaveToXml(XmlElement optionElement)
        {
            if (optionElement == null)
                throw new ArgumentNullException("OptionElement");

            optionElement.SetAttribute("Station", Station);
            optionElement.SetAttribute("ConnectionType", ConnectionType);
            optionElement.SetAttribute("ModbusMode", ModbusMode);

            optionElement.SetAttribute("IPAddress", IPAddress);
            optionElement.SetAttribute("Port", Port);

            optionElement.SetAttribute("PortName", PortName);
            optionElement.SetAttribute("BaudRate", BaudRate);
            optionElement.SetAttribute("DataBits", DataBits);
            optionElement.SetAttribute("StopBits", StopBits);
            optionElement.SetAttribute("Parity", Parity);
        }
    }

    //public class ConnectionOptionsByTcp : ConnectionOptions
    //{
        

    //    public override void LoadFromXml(XmlElement optionElement)
    //    {
    //        base.LoadFromXml(optionElement);

           
    //    }

    //    public override void SaveToXml(XmlElement optionElement)
    //    {
    //        base.SaveToXml(optionElement);

    //        optionElement.SetAttribute("IPAddress", IPAddress);
    //        optionElement.SetAttribute("Port", Port);
    //    }
    //}

    //public class ConnectionOptionsBySerial : ConnectionOptions
    //{
        

    //    public override void LoadFromXml(XmlElement optionElement)
    //    {
    //        base.LoadFromXml(optionElement);

           
    //    }

    //    public override void SaveToXml(XmlElement optionElement)
    //    {
    //        base.SaveToXml(optionElement);

            
    //    }
    //}
}
