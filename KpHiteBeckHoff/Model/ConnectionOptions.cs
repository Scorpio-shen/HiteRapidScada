using KpCommon.Extend;
using KpCommon.Model;
using KpHiteBeckHoff.Model.EnumType;
using System.IO.Ports;
using System.Text;

namespace KpHiteBeckHoff.Model
{
    public class ConnectionOptions : ConnectionUnit
    {
        #region 公共参数

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

        private string targetnetid;

        public string TaggetNetId
        {
            get => targetnetid;
            set
            {
                targetnetid = value;
                OnPropertyChanged(nameof(TaggetNetId));
            }
        }

        private string sendernetid;
        public string SenderNetId
        {
            get => sendernetid;
            set
            {
                sendernetid = value;
                OnPropertyChanged(nameof(SenderNetId));
            }
        }

        private bool autoamsnetid;
        public bool AutoAmsNetId
        {
            get => autoamsnetid;
            set
            {
                autoamsnetid = value;
                OnPropertyChanged(nameof(AutoAmsNetId));
            }
        }

        private string amsport;
        public string AmsPort
        {
            get => amsport;
            set
            {
                amsport = value;
                OnPropertyChanged(nameof(AmsPort));
            }
        }

        private bool tagcache = true;
        public bool TagCache
        {
            get => tagcache;
            set
            {
                tagcache = value;   
                OnPropertyChanged(nameof(TagCache));
            }
        }
        #endregion

        //#region SerialPort方法
        //private string portname;
        //public string PortName
        //{
        //    get => portname;
        //    set
        //    {
        //        portname = value;
        //        OnPropertyChanged(nameof(PortName));
        //        OnPropertyChanged(nameof(ConsoleParamsStr));
        //    }
        //}

        //private int baudrate;
        //public int BaudRate
        //{
        //    get => baudrate;
        //    set
        //    {
        //        baudrate = value;
        //        OnPropertyChanged(nameof(BaudRate));
        //        OnPropertyChanged(nameof(ConsoleParamsStr));
        //    }
        //}

        //private int databits;
        //public int DataBits
        //{
        //    get => databits;
        //    set
        //    {
        //        databits = value;
        //        OnPropertyChanged(nameof(DataBits));
        //        OnPropertyChanged(nameof(ConsoleParamsStr));
        //    }
        //}

        //private StopBits stopbits;
        //public StopBits StopBits
        //{
        //    get => stopbits;
        //    set
        //    {
        //        stopbits = value;
        //        OnPropertyChanged(nameof(StopBits));
        //        OnPropertyChanged(nameof(ConsoleParamsStr));
        //    }
        //}

        //private Parity parity;
        //public Parity Parity
        //{
        //    get => parity;
        //    set
        //    {
        //        parity = value;
        //        OnPropertyChanged(nameof(Parity));
        //        OnPropertyChanged(nameof(ConsoleParamsStr));
        //    }
        //}
        //#endregion
    }
}
