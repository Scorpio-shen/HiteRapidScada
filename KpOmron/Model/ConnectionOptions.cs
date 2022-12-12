using KpCommon.Extend;
using KpCommon.Model;
using KpOmron.Model.EnumType;
using Scada;
using System;
using System.IO.Ports;
using System.Text;
using System.Xml;

namespace KpOmron.Model
{
    public class ConnectionOptions : ConnectionUnit
    {
        #region 公共参数
        private byte unitnumber;
        public byte UnitNumber
        {
            get => unitnumber;
            set
            {
                unitnumber = value;
                OnPropertyChanged(nameof(UnitNumber));
            }
        }

        private byte sid;
        public byte SID
        {
            get=> sid;
            set
            {
                sid= value;
                OnPropertyChanged(nameof(SID));
            }
        }

        private byte da2;
        public byte DA2
        {
            get => da2;
            set
            {
                da2= value;
                OnPropertyChanged(nameof(DA2));
            }
        }

        private byte sa2;
        public byte SA2
        {
            get => sa2;
            set
            {
                sa2= value;
                OnPropertyChanged(nameof(SA2));
            }
        }

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
                if(ConnectionType == ConnectionTypeEnum.SerialPort)
                {
                    sb.AppendLine($"串口名:{PortName}");
                    sb.AppendLine($"波特率:{BaudRate}");
                    sb.AppendLine($"数据位:{DataBits}");
                    sb.AppendLine($"停止位:{StopBits.GetDescription()}");
                    sb.AppendLine($"校验位:{Parity.GetDescription()}");
                }
                else
                {
                    sb.AppendLine($"IP地址:{IPAddress}");
                    sb.AppendLine($"端口:{Port}");
                }

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

        private int port = 502;
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
