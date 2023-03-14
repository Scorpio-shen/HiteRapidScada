using KpHiteModbus.Modbus.View;
using KpHiteMqtt.Mqtt.Model;
using Scada.Data.Entities;
using Scada.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KpHiteMqtt.Mqtt.View
{
    public partial class CtrlJsonPara : UserControl
    {
        private List<DataSpecs> _dataSpecsList;
        private List<InCnl> _allincnls { get;set; }
        public List<CtrlCnl> _allctrlcnls { get;set; }

        public List<DataSpecs> DataSpecsList
        {
            get => _dataSpecsList;
            set
            {
                _dataSpecsList = value;
                //OnDataSpecsChanged(_dataSpecsList);
            }
        }

        public event Action<List<DataSpecs>> DataSpecsChanged;

        private bool _showjsonchannels;
        public CtrlJsonPara()
        {
            InitializeComponent();
        }

        #region 参数初始化
        public void InitPara(List<DataSpecs> dataSpecsList,bool showjsonchannels)
        {
            DataSpecsList = dataSpecsList;
            _allincnls = FrmDevTemplate.AllInCnls;
            _allctrlcnls = FrmDevTemplate.AllCtrlCnls;
            _showjsonchannels = showjsonchannels;
            ShowJsonPara();
        }
        #endregion
        #region 添加Json参数
        private void linkAddPara_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            FrmDevJsonPara frm = new FrmDevJsonPara();
            frm.StartPosition = FormStartPosition.CenterParent;
            var dataSpecs = new DataSpecs();
            frm.InitParameters(dataSpecs, _showjsonchannels);
            if (frm.ShowDialog() != DialogResult.OK)
                return;
            //判断该辨识符的参数是否已存在
            if (DataSpecsList.Any(d => d.Identifier.Equals(dataSpecs.Identifier)))
            {
                ScadaUiUtils.ShowError($"标识符:{dataSpecs.Identifier}已存在!");
                return;
            }
            DataSpecsList.Add(dataSpecs);
            ShowJsonPara();
            OnDataSpecsChanged(DataSpecsList);
        }
        #endregion

        #region 刷新Json参数数量显示
        private void ShowJsonPara()
        {
            panelPara.Controls.Clear();
            Point startPoint = new Point(4, 6);
            foreach (DataSpecs dataSpecs in DataSpecsList)
            {
                var control = new CtrlJsonParaSpec(dataSpecs, _showjsonchannels)
                {
                    Location = startPoint
                };
                control.OnDeleteJsonPara += Control_OnDeleteJsonPara;
                control.OnModifyJsonPara += Control_OnModifyJsonPara;
                panelPara.Controls.Add(control);
                startPoint.Y += 50;
            }
        }

        private void Control_OnModifyJsonPara(DataSpecs obj)
        {
            OnDataSpecsChanged(DataSpecsList);
        }

        private void Control_OnDeleteJsonPara(DataSpecs dataSpecs)
        {
            //要删除此参数
            if(DataSpecsList.Any(d => d.Identifier.Equals(dataSpecs.Identifier)))
            {
                DataSpecsList.Remove(dataSpecs);
                ShowJsonPara();
                OnDataSpecsChanged(DataSpecsList);
            }

        }
        #endregion

        #region 触发事件
        private void OnDataSpecsChanged(List<DataSpecs> dataSpecs)
        {
            DataSpecsChanged?.Invoke(dataSpecs);
        }
        #endregion
    }
}
