using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Scada.Client;
using Scada.UI;

namespace Scada.Server.Modules.TDEngineExport
{
	internal partial class FrmManualExport : Form
	{
		private FrmManualExport()
		{
			this.InitializeComponent();
		}

		public static bool ShowDialog(ServerComm serverComm, List<ModConfig.ExportDestination> expDests, ModConfig.ExportDestination selExpDest, ref int curDataCtrlCnlNum, ref int arcDataCtrlCnlNum, ref int eventsCtrlCnlNum)
		{
			FrmManualExport frmManualExport = new FrmManualExport();
			frmManualExport.ServerComm = serverComm;
			foreach (ModConfig.ExportDestination exportDestination in expDests)
			{
				int selectedIndex = frmManualExport.cbDataSource.Items.Add(exportDestination.DataSource);
				if (exportDestination == selExpDest)
				{
					frmManualExport.cbDataSource.SelectedIndex = selectedIndex;
				}
			}
			frmManualExport.CurDataCtrlCnlNum = curDataCtrlCnlNum;
			frmManualExport.ArcDataCtrlCnlNum = arcDataCtrlCnlNum;
			frmManualExport.EventsCtrlCnlNum = eventsCtrlCnlNum;
			if (frmManualExport.ShowDialog() == DialogResult.OK)
			{
				curDataCtrlCnlNum = frmManualExport.CurDataCtrlCnlNum;
				arcDataCtrlCnlNum = frmManualExport.ArcDataCtrlCnlNum;
				eventsCtrlCnlNum = frmManualExport.EventsCtrlCnlNum;
				return true;
			}
			return false;
		}

		private ServerComm ServerComm { get; set; }

		private int CurDataCtrlCnlNum
		{
			get
			{
				return Convert.ToInt32(this.numCurDataCtrlCnlNum.Value);
			}
			set
			{
				ScadaUiUtils.SetValue(this.numCurDataCtrlCnlNum, value);
			}
		}

		private int ArcDataCtrlCnlNum
		{
			get
			{
				return Convert.ToInt32(this.numArcDataCtrlCnlNum.Value);
			}
			set
			{
				ScadaUiUtils.SetValue(this.numArcDataCtrlCnlNum, value);
			}
		}

		private int EventsCtrlCnlNum
		{
			get
			{
				return Convert.ToInt32(this.numEventsCtrlCnlNum.Value);
			}
			set
			{
				ScadaUiUtils.SetValue(this.numEventsCtrlCnlNum, value);
			}
		}

		private void FrmManualExport_Load(object sender, EventArgs e)
		{
			if (!Localization.UseRussian)
			{
				Translator.TranslateForm(this, "Scada.Server.Modules.TDEngineExport.FrmManualExport", null, new ContextMenuStrip[0]);
			}
			if (this.cbDataSource.SelectedIndex < 0 && this.cbDataSource.Items.Count > 0)
			{
				this.cbDataSource.SelectedIndex = 0;
			}
			this.gbCurData.Enabled = (this.gbArcData.Enabled = (this.gbEvents.Enabled = (this.cbDataSource.Items.Count > 0)));
			this.dtpArcDataDate.Value = (this.dtpEventsDate.Value = (this.dtpArcDataTime.Value = DateTime.Today));
			if (this.ServerComm == null)
			{
				this.btnExportCurData.Enabled = false;
				this.btnExportArcData.Enabled = false;
				this.btnExportEvents.Enabled = false;
			}
		}

		private void numCurDataCtrlCnlNum_ValueChanged(object sender, EventArgs e)
		{
			this.btnExportCurData.Enabled = (this.numCurDataCtrlCnlNum.Value > 0m);
		}

		private void numArcDataCtrlCnlNum_ValueChanged(object sender, EventArgs e)
		{
			this.dtpArcDataDate.Enabled = (this.dtpArcDataTime.Enabled = (this.btnExportArcData.Enabled = (this.numArcDataCtrlCnlNum.Value > 0m)));
		}

		private void numEventsCtrlCnlNum_ValueChanged(object sender, EventArgs e)
		{
			this.dtpEventsDate.Enabled = (this.btnExportEvents.Enabled = (this.numEventsCtrlCnlNum.Value > 0m));
		}

		private void btnExport_Click(object sender, EventArgs e)
		{
			string text = this.cbDataSource.Text;
			int num;
			if (sender == this.btnExportArcData)
			{
				num = this.ArcDataCtrlCnlNum;
				DateTime dateTime = this.dtpArcDataDate.Value.Date.Add(this.dtpArcDataTime.Value.TimeOfDay);
				text = text + "\n" + ScadaUtils.XmlValToStr(dateTime);
			}
			else if (sender == this.btnExportEvents)
			{
				num = this.EventsCtrlCnlNum;
				DateTime date = this.dtpEventsDate.Value.Date;
				text = text + "\n" + ScadaUtils.XmlValToStr(date);
			}
			else
			{
				num = this.CurDataCtrlCnlNum;
			}
			byte[] bytes = Encoding.Default.GetBytes(text);
			bool flag;
			if (this.ServerComm.SendBinaryCommand(0, num, bytes, out flag))
			{
				ScadaUiUtils.ShowInfo(ModPhrases.CmdSentSuccessfully);
				return;
			}
			ScadaUiUtils.ShowError(this.ServerComm.ErrMsg);
		}
	}
}
