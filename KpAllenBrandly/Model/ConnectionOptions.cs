using KpCommon.Extend;
using KpCommon.Model;
using KpAllenBrandly.Model.EnumType;
using System.IO.Ports;
using System.Text;

namespace KpAllenBrandly.Model
{
    public class ConnectionOptions : ConnectionUnit
    {
        #region 公共参数
        //private ConnectionTypeEnum connectiontype;
        //public ConnectionTypeEnum ConnectionType
        //{
        //    get => connectiontype;
        //    set
        //    {
        //        connectiontype = value;
        //        OnPropertyChanged(nameof(ConnectionType));
        //        OnPropertyChanged(nameof(ConsoleParamsStr));
        //    }
        //}

        private ProtocolTypeEnum protocolType;
        public ProtocolTypeEnum ProtocolType
        {
            get => protocolType;
            set
            {
                protocolType = value;
                OnPropertyChanged(nameof(ProtocolType));
                OnPropertyChanged(nameof(ConsoleParamsStr));
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

        private bool newversionmessage;
        public bool NewVersionMessage
        {
            get => newversionmessage;
            set { newversionmessage = value; OnPropertyChanged(nameof(NewVersionMessage)); }
        }

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


        private bool sumcheck;
        public bool SumCheck
        {
            get => sumcheck;
            set
            {
                sumcheck = value;
                OnPropertyChanged(nameof(SumCheck));
            }
        }

        private int format;
        public int Format
        {
            get => format;
            set
            {
                format = value;
                OnPropertyChanged(nameof(Format));
            }
        }


        public string ConsoleParamsStr
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine($"IP地址:{IPAddress}");
                sb.AppendLine($"端口:{Port}");

                return sb.ToString();
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
                OnPropertyChanged(nameof(ConsoleParamsStr));
            }
        }

        private int port = 6000;
        public int Port
        {
            get => port;
            set
            {
                port = value;
                OnPropertyChanged(nameof(Port));
                OnPropertyChanged(nameof(ConsoleParamsStr));
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
                OnPropertyChanged(nameof(ConsoleParamsStr));
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
                OnPropertyChanged(nameof(ConsoleParamsStr));
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
                OnPropertyChanged(nameof(ConsoleParamsStr));
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
                OnPropertyChanged(nameof(ConsoleParamsStr));
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
                OnPropertyChanged(nameof(ConsoleParamsStr));
            }
        }
        #endregion
    }
}
