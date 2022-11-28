using KpMyModbus.Modbus.Protocol;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KpMyModbus.Modbus.ViewModel
{
    public class TagCmdControlViewModel : INotifyPropertyChanged
    {
        private string cmdname;
        public string CmdName 
        {
            get=>cmdname;
            set
            {
                cmdname = value;
                OnPropertyChanged(nameof(CmdName));
            }
        }
        private ModbusRegisterType modbusRegisterType;
        public ModbusRegisterType ModbusRequestType
        {
            get=> modbusRegisterType;
            set
            {
                modbusRegisterType = value;
                OnPropertyChanged(nameof(ModbusRequestType));
            }
        }

        private bool cmdMultiple;
        public bool CmdMultiple
        {
            get => cmdMultiple;
            set
            {
                cmdMultiple = value;
                OnPropertyChanged(nameof(CmdMultiple));
            }
        }

        private string functionCode;
        public string FunctionCode
        {
            get => functionCode;
            set
            {
                functionCode = value;
                OnPropertyChanged(nameof(FunctionCode));
            }
        }

        private int tagAddress = 1;
        public int TagAddress
        {
            get => tagAddress;
            set
            {
                tagAddress = value;
                OnPropertyChanged(nameof(TagAddress));
            }
        }

        private int elementType;
        public int ElementType
        {
            get => elementType;
            set
            {
                elementType = value;
                OnPropertyChanged(nameof(ElementType));
            }
        }

        private int tagCount =1;
        public int TagCount
        {
            get => tagCount;
            set
            {
                tagCount = value;
                OnPropertyChanged(nameof(TagCount));
            }
        }


        private int cmdNum = 1;
        public int CmdNum
        {
            get => cmdNum;
            set
            {
                cmdNum = value;
                OnPropertyChanged(nameof(CmdNum));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string proName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(proName));
        }
    }
}
