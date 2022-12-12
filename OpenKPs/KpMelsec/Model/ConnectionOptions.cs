using KpCommon.Extend;
using KpCommon.Model;
using KpMelsec.Model.EnumType;
using System.IO.Ports;
using System.Text;

namespace KpMelsec.Model
{
    public class ConnectionOptions : ConnectionUnit
    {
        #region 公共参数
        private ConnectionTypeEnum connectiontype;
        public ConnectionTypeEnum ConnectionType
        {
            get => connectiontype;
            set
            {
                connectiontype = value;
                OnPropertyChanged(nameof(ConnectionType));
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
        #endregion

    }
}
