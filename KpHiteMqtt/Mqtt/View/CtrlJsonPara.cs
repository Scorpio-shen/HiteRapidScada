using KpHiteMqtt.Mqtt.Model;
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
        public List<DataSpecs> DataSpecs { get;set; }
        public CtrlJsonPara()
        {
            InitializeComponent();
        }

        #region 添加Json参数
        private void linkAddPara_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {

            ShowJsonPara();
        }
        #endregion

        #region 刷新Json参数数量显示
        private void ShowJsonPara()
        {
            panelPara.Controls.Clear();
            Point startPoint = new Point(4, 6);
            foreach (DataSpecs dataSpecs in DataSpecs)
            {
                var control = new CtrlJsonParaSpec(dataSpecs)
                {
                    Location = startPoint
                };
                panelPara.Controls.Add(control);
                startPoint.Y += 50;
            }
        }
        #endregion

    }
}
