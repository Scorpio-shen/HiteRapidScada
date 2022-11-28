using KpMyModbus.Modbus.Protocol;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KpMyModbus.Modbus.ViewModel
{
    public class TagGroupControlViewModel : INotifyPropertyChanged
    {
        private string tagGroupName;
        public string TagGroupName
        {
            get => tagGroupName;
            set
            {
                tagGroupName = value;
                OnPropertyChanged(nameof(TagGroupName));
            }
        }

        private ModbusRegisterType  modbusRegisterType;
        public ModbusRegisterType ModbusRegisterType
        {
            get => modbusRegisterType;
            set
            {
                modbusRegisterType = value;
                OnPropertyChanged(nameof(ModbusRegisterType));
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

        private int tagstartAddress = 1;
        public int TagStartAddess
        {
            get => tagstartAddress;
            set
            {
                tagstartAddress = value;
                OnPropertyChanged(nameof(TagStartAddess));
            }
        }

        private int tagCount = 1;
        public int TagCount
        {
            get => tagCount;
            set
            {
                tagCount = value;
                OnPropertyChanged(nameof(TagCount));
            }
        }




        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string proName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(proName));
        }
    }
}
