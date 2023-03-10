using Scada.Data.Entities;
using System;

namespace KpHiteMqtt.Mqtt.Model
{
    public class InputChannelModel
    {

        public InCnl InCnl { get; set; }
        private object value;
        public object Value
        {
            get { return value; }
            set
            {
                if (this.value != value)
                {
                    this.value = value;
                    OnValueChanged();
                }
            }
        }

        public event Action<InputChannelModel> ValueChanged;
        public void OnValueChanged()
        {
            ValueChanged?.Invoke(this);
        }
    }
    /// <summary>
    /// OPC外部给通道赋值
    /// </summary>
    public class OutputChannelModel
    {
        /// <summary>
        /// 控制通道号
        /// </summary>
        public int CtrlCnlNum { get; set; }
        public object Value { get; set; }

    }
}
