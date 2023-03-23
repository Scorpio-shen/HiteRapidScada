using KpCommon.Extend;
using KpCommon.Model;
using KpHiteModbus.Modbus.Extend;
using KpHiteModbus.Modbus.Model.EnumType;
using Scada;
using System;
using System.ComponentModel;
using System.IO.Ports;
using System.Text;
using System.Xml;

namespace KpHiteModbus.Modbus.Model
{
    public class ConnectionOptions : ConnectionUnit
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
                OnPropertyChanged(nameof(ConsoleParamsStr));
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
                OnPropertyChanged(nameof(ConsoleParamsStr));
            }
        }

        public string ConsoleParamsStr
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                if(ConnectionType == ModbusConnectionTypeEnum.SerialPort)
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

        //public override void LoadFromXml(XmlElement optionElement)
        //{
        //    if (optionElement == null)
        //        throw new ArgumentNullException("OptionElement");

        //    foreach(var p in GetType().GetProperties())
        //    {
        //        if (!p.CanWrite)
        //            return;
        //        if(p.PropertyType == typeof(bool))
        //        {
        //            p.SetValue(this, optionElement.GetAttrAsBool(p.Name));
        //        }
        //        else if(p.PropertyType == typeof(byte))
        //        {
        //            p.SetValue(this, optionElement.GetAttrAsByte(p.Name));
        //        }
        //        else if(p.PropertyType == typeof(string))
        //        {
        //            p.SetValue(this, optionElement.GetAttrAsString(p.Name));
        //        }
        //        else if(p.PropertyType == typeof(int))
        //        {
        //            p.SetValue(this, optionElement.GetAttrAsInt(p.Name));
        //        }
        //        else if (p.PropertyType.IsEnum)
        //        {
        //            try
        //            {
        //                var enumValue = Enum.Parse(p.PropertyType, optionElement.GetAttrAsString(p.Name), true);
        //                p.SetValue(this, enumValue);
        //            }
        //            catch
        //            {
                        
        //            }
        //        }
        //    }
        //    Station = optionElement.GetAttrAsString("Station").ToByte();
        //    ConnectionType = optionElement.GetAttrAsEnum("ConnectionType", ModbusConnectionTypeEnum.TcpIP);
        //    ModbusMode = optionElement.GetAttrAsEnum("ModbusMode", ModbusModeEnum.Rtu);

        //    IPAddress = optionElement.GetAttrAsString("IPAddress");
        //    Port = optionElement.GetAttrAsInt("Port");

        //    PortName = optionElement.GetAttrAsString("PortName");
        //    BaudRate = optionElement.GetAttrAsInt("BaudRate");
        //    DataBits = optionElement.GetAttrAsInt("DataBits");
        //    StopBits = optionElement.GetAttrAsEnum("StopBits", StopBits.None);
        //    Parity = optionElement.GetAttrAsEnum("Parity", Parity.None);
        //}
        //public override void SaveToXml(XmlElement optionElement)
        //{
        //    if (optionElement == null)
        //        throw new ArgumentNullException("OptionElement");

        //    optionElement.SetAttribute("Station", Station);
        //    optionElement.SetAttribute("ConnectionType", ConnectionType);
        //    optionElement.SetAttribute("ModbusMode", ModbusMode);

        //    optionElement.SetAttribute("IPAddress", IPAddress);
        //    optionElement.SetAttribute("Port", Port);

        //    optionElement.SetAttribute("PortName", PortName);
        //    optionElement.SetAttribute("BaudRate", BaudRate);
        //    optionElement.SetAttribute("DataBits", DataBits);
        //    optionElement.SetAttribute("StopBits", StopBits);
        //    optionElement.SetAttribute("Parity", Parity);
        //}
    }
}
