using KpOmron.Model.EnumType;
using System.ComponentModel;

namespace KpMelsec.ViewModel
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
        private DataTypeEnum datatype = DataTypeEnum.UInt;
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
            get=> $"{StartAddress}~{StartAddress + AddressIncrement * (TagCount - 1)}";
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
