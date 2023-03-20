using KpHiteBeckHoff.Model;
using KpHiteBeckHoff.Model.EnumType;
using KpCommon.Extend;
using KpCommon.Model;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace KpHiteBeckHoff.View
{
    public partial class CtrlPLCConfig : UserControl
    {
        public event ConfigChangedEventHandler<Tag> ConfigChanged;
        private ConnectionOptions connectionOptions;
        public ConnectionOptions ConnectionOptions
        {
            get=>connectionOptions;
            set
            {
                IsShowProps = true;
                connectionOptions = value;
                BindControls(value);
                IsShowProps = false;
            }
        }

        /// <summary>
        /// 是否是展示Options内容,只是展示内容不触发控件事件
        /// </summary>
        public bool IsShowProps { get; set; } = false;
        public CtrlPLCConfig()
        {
            InitializeComponent();
        }

        private void CtrlPLCConfig_Load(object sender, EventArgs e)
        {
            //Combobox绑定数据源
            //var keyValueConnectionEnums = new Dictionary<string, ProtocolTypeEnum>();
            //foreach (ProtocolTypeEnum type in Enum.GetValues(typeof(ProtocolTypeEnum)))
            //    keyValueConnectionEnums.Add(type.GetDescription(), type);

            //BindingSource bindingSource = new BindingSource();
            //bindingSource.DataSource = keyValueConnectionEnums;
            //cbxProtocolType.DataSource = bindingSource;
            //cbxProtocolType.DisplayMember = "Key";
            //cbxProtocolType.ValueMember = "Value";
        }

        private void BindControls(ConnectionOptions options)
        {
            if (options == null)
                return;
            
            txtIPAddress.AddDataBindings(options,nameof(options.IPAddress));
            txtPort.AddDataBindings(options, nameof(options.Port));
            txtTargetNetId.AddDataBindings(options, nameof(options.TaggetNetId));
            txtSenderNetId.AddDataBindings(options, nameof(options.SenderNetId));
            chkAutoAmsNetId.AddDataBindings(options, nameof(options.AutoAmsNetId));
            txtAmsPort.AddDataBindings(options, nameof(options.AmsPort));
            chkTagCache.AddDataBindings(options, nameof(options.TagCache));

            options.PropertyChanged += Options_PropertyChanged;
        }

        private void Options_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            OnConfigChanged(ConnectionOptions);
        }

        private void OnConfigChanged(object sender)
        {
            ConfigChanged?.Invoke(sender, new ConfigChangedEventArgs<Tag>
            {
                ConnectionOptions = ConnectionOptions
            });
        }

        #region 控件事件
        private void btnParamSetting_Click(object sender, EventArgs e)
        {
            FrmParaSet frm = new FrmParaSet(ConnectionOptions,OnConfigChanged);
            frm.StartPosition = FormStartPosition.CenterParent;
            frm.ShowDialog();
        }
        #endregion


    }
}
