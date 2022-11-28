using KpHiteModbus.Modbus.Model;
using System;
using System.ComponentModel;

namespace KpHiteModbus.Modbus.ViewModel
{
    public class FrmDevAddRangeViewModel : INotifyPropertyChanged
    {
        private string namereplace;
        public string NameReplace
        {
            get => namereplace;
            set
            {
                namereplace = value;
                OnPropertyChanged(nameof(NameReplace));
            }
        }
        private int namestartindex;
        public int NameStartIndex
        {
            get => namestartindex;
            set
            {
                namestartindex = value;
                OnPropertyChanged(nameof(NameStartIndex));
            }
        }
        private double startaddress;
        public double StartAddress
        {
            get => startaddress;
            set
            {
                startaddress = value;
                OnPropertyChanged(nameof(StartAddress));
            }
        }
        private double addressincrement;
        public double AddressIncrement
        {
            get => addressincrement;
            set
            {
                addressincrement = value;
                OnPropertyChanged(AddressOutput);
            }
        }
        private int tagcount = 1;
        public int TagCount
        {
            get=> tagcount;
            set
            {
                tagcount = value;
                OnPropertyChanged(nameof(TagCount));
            }
        }
        private DataTypeEnum datatype = DataTypeEnum.Byte;
        public DataTypeEnum DataType
        {
            get => datatype;
            set
            {
                datatype = value;
                OnPropertyChanged(nameof(DataType));
            }
        }
        public string NameOutput
        {
            get => $"{NameReplace}{NameStartIndex}" + "~" + $"{NameReplace}{NameStartIndex + (TagCount -1)}";
        }
        public string AddressOutput
        {
            get
            {
                double address = StartAddress;
                if (DataType == DataTypeEnum.Bool)
                {
                    for (int i = 0; i < TagCount; i++)
                    {
                        double dPart = Math.Round((address % 1),1); //小数部分
                        int iPart = (int)address;
                        if (dPart < 0.7)
                            dPart += 0.1d;
                        else
                        {
                            iPart++;
                            dPart = 0.0d;
                        }

                        address = iPart + dPart;
                    }

                    return $"{StartAddress}~{address}";
                }
                else
                    return $"{StartAddress}~{StartAddress + AddressIncrement * (TagCount-1)}";
            }
        }

        private int length;
        public int Length
        {
            get => length;
            set
            {
                length = value;
                OnPropertyChanged(nameof(Length));
            }
        }

        public bool canwrite;
        public bool CanWrite
        {
            get => canwrite;
            set
            {
                canwrite = value;
                OnPropertyChanged(nameof(CanWrite));
            }
        }

        //private bool lengthvisable = false;
        //public bool LengthVisable
        //{
        //    get => lengthvisable;
        //    set
        //    {
        //        lengthvisable = value;
        //        OnPropertyChanged(nameof(LengthVisable));
        //    }
        //}
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string proName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(proName));
        }
    }
}
