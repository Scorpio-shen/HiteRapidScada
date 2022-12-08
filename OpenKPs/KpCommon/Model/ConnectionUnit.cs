using KpCommon.InterFace;
using System.ComponentModel;

namespace KpCommon.Model
{
    public class ConnectionUnit : IConnectionUnit
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName = null)
        {
            var eventHandler = PropertyChanged;
            if (eventHandler != null)
            {
                eventHandler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
