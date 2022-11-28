using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Scada.Client;
using Scada.UI;

namespace Scada.Server.Modules.TDEngineExport
{
	internal partial class FrmDBExportConfig : Form
	{
		private FrmDBExportConfig()
		{
			this.InitializeComponent();
			this.config = null;
			this.configCopy = null;
			this.modified = false;
			this.changing = false;
			this.selExpDest = null;
			this.selExpDestNode = null;
		}

		private bool Modified
		{
			get
			{
				return this.modified;
			}
			set
			{
				this.modified = value;
				this.btnSave.Enabled = this.modified;
				this.btnCancel.Enabled = this.modified;
			}
		}

		public static void ShowDialog(AppDirs appDirs, ServerComm serverComm)
		{
			new FrmDBExportConfig
			{
				appDirs = appDirs,
				serverComm = serverComm
			}.ShowDialog();
		}

		private TreeNode NewExpDestNode(ModConfig.ExportDestination expDest)
		{
			TreeNode treeNode = new TreeNode(expDest.DataSource.Name);
			treeNode.Tag = expDest;
			string selectedImageKey;
			if (expDest.DataSource.DBType == DBType.TDEngine)
			{
				selectedImageKey = "TDEngine.png";
			}
			else
			{
				selectedImageKey = "";
			}
			treeNode.ImageKey = (treeNode.SelectedImageKey = selectedImageKey);
			return treeNode;
		}

		private void ConfigToControls()
		{
			this.selExpDest = null;
			this.selExpDestNode = null;
			this.treeView.BeginUpdate();
			this.treeView.Nodes.Clear();
			foreach (ModConfig.ExportDestination expDest in this.config.ExportDestinations)
			{
				this.treeView.Nodes.Add(this.NewExpDestNode(expDest));
			}
			this.treeView.ExpandAll();
			this.treeView.EndUpdate();
			if (this.treeView.Nodes.Count > 0)
			{
				this.treeView.SelectedNode = this.treeView.Nodes[0];
			}
			this.SetControlsEnabled();
		}

		private void ShowSelectedExportParams()
		{
			if (this.selExpDest != null)
			{
				this.changing = true;
				this.tabControl.SelectedIndex = 0;
				DataSource dataSource = this.selExpDest.DataSource;
				this.txtServer.Text = dataSource.Server;
				this.txtDatabase.Text = dataSource.Database;
				this.txtUser.Text = dataSource.User;
				this.txtPassword.Text = dataSource.Password;
				this.txtConnectionString.Text = dataSource.CfgFilePath;
				ModConfig.ExportParams exportParams = this.selExpDest.ExportParams;
				this.ctrlExportCurDataQuery.Export = exportParams.ExportCurData;
				this.ctrlExportCurDataQuery.Query = exportParams.ExportCurDataQuery;
				this.ctrlExportCurDataQuery.ExportCnls = exportParams.ExportCurDataCnls;
				this.ctrlExportArcDataQuery.Export = exportParams.ExportArcData;
				this.ctrlExportArcDataQuery.Query = exportParams.ExportArcDataQuery;
				this.ctrlExportArcDataQuery.ExportCnls = exportParams.ExportArcDataCnls;
				this.ctrlExportEventQuery.Export = exportParams.ExportEvents;
				this.ctrlExportEventQuery.Query = exportParams.ExportEventQuery;
				this.ctrlExportEventQuery.ExportCnls = exportParams.ExportEventCnls;
				ScadaUiUtils.SetValue(this.numMaxQueueSize, exportParams.MaxQueueSize);
				this.changing = false;
			}
		}

		private void SetConnControlsBackColor(KnownColor connParamsColor, KnownColor connStrColor)
		{
			this.txtServer.BackColor = (this.txtDatabase.BackColor = (this.txtUser.BackColor = (this.txtPassword.BackColor = Color.FromKnownColor(connParamsColor))));
			this.txtConnectionString.BackColor = Color.FromKnownColor(connStrColor);
		}

		private void SetConnectionString()
		{
		}

		private void SetControlsEnabled()
		{
			if (this.selExpDest == null)
			{
				this.btnDelDataSource.Enabled = false;
				this.btnManualExport.Enabled = false;
				this.lblInstruction.Visible = true;
				this.tabControl.Visible = false;
				return;
			}
			this.btnDelDataSource.Enabled = true;
			this.btnManualExport.Enabled = true;
			this.lblInstruction.Visible = false;
			this.tabControl.Visible = true;
		}

		private bool SaveConfig()
		{
			if (!this.Modified)
			{
				return true;
			}
			string text;
			if (this.config.Save(out text))
			{
				this.Modified = false;
				return true;
			}
			ScadaUiUtils.ShowError(text);
			return false;
		}

		private void FrmDBExportConfig_Load(object sender, EventArgs e)
		{
			string text;
			if (!Localization.UseRussian)
			{
				if (Localization.LoadDictionaries(this.appDirs.LangDir, "ModTDEngineExport", out text))
				{
					Translator.TranslateForm(this, "Scada.Server.Modules.TDEngineExport.FrmDBExportConfig", null, new ContextMenuStrip[0]);
				}
				else
				{
					ScadaUiUtils.ShowError(text);
				}
			}
			this.lblInstruction.Top = this.treeView.Top;
			this.config = new ModConfig(this.appDirs.ConfigDir);
			if (File.Exists(this.config.FileName) && !this.config.Load(out text))
			{
				ScadaUiUtils.ShowError(text);
			}
			this.configCopy = this.config.Clone();
			this.ConfigToControls();
			this.Modified = false;
		}

		private void FrmDBExportConfig_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (this.Modified)
			{
				DialogResult dialogResult = MessageBox.Show(ModPhrases.SaveModSettingsConfirm, CommonPhrases.QuestionCaption, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
				if (dialogResult != DialogResult.Yes)
				{
					if (dialogResult != DialogResult.No)
					{
						e.Cancel = true;
					}
				}
				else if (!this.SaveConfig())
				{
					e.Cancel = true;
					return;
				}
			}
		}

		private void treeView_AfterSelect(object sender, TreeViewEventArgs e)
		{
			TreeNode node = e.Node;
			this.selExpDest = (node.Tag as ModConfig.ExportDestination);
			this.selExpDestNode = ((this.selExpDest == null) ? null : node);
			this.ShowSelectedExportParams();
		}

		private void miAddDataSource_Click(object sender, EventArgs e)
		{
			DataSource dataSource = null;
			if (sender == this.miAddSqlDataSource)
			{
				dataSource = new TDEngineDataSource();
			}
			if (dataSource != null)
			{
				ModConfig.ExportDestination exportDestination = new ModConfig.ExportDestination(dataSource, new ModConfig.ExportParams());
				TreeNode treeNode = this.NewExpDestNode(exportDestination);
				int num = this.config.ExportDestinations.BinarySearch(exportDestination);
				if (num >= 0)
				{
					num++;
				}
				else
				{
					num = ~num;
				}
				this.config.ExportDestinations.Insert(num, exportDestination);
				this.treeView.Nodes.Insert(num, treeNode);
				this.treeView.SelectedNode = treeNode;
				this.SetConnectionString();
				this.SetControlsEnabled();
				this.Modified = true;
			}
		}

		private void btnDelDataSource_Click(object sender, EventArgs e)
		{
			if (this.selExpDestNode != null)
			{
				TreeNode prevNode = this.selExpDestNode.PrevNode;
				TreeNode nextNode = this.selExpDestNode.NextNode;
				int index = this.selExpDestNode.Index;
				this.config.ExportDestinations.RemoveAt(index);
				this.treeView.Nodes.RemoveAt(index);
				this.treeView.SelectedNode = ((nextNode == null) ? prevNode : nextNode);
				if (this.treeView.SelectedNode == null)
				{
					this.selExpDest = null;
					this.selExpDestNode = null;
				}
				this.SetControlsEnabled();
				this.Modified = true;
			}
		}

		private void btnManualExport_Click(object sender, EventArgs e)
		{
			int curDataCtrlCnlNum = this.config.CurDataCtrlCnlNum;
			int arcDataCtrlCnlNum = this.config.ArcDataCtrlCnlNum;
			int eventsCtrlCnlNum = this.config.EventsCtrlCnlNum;
			if (FrmManualExport.ShowDialog(this.serverComm, this.config.ExportDestinations, this.selExpDest, ref curDataCtrlCnlNum, ref arcDataCtrlCnlNum, ref eventsCtrlCnlNum) && (this.config.CurDataCtrlCnlNum != curDataCtrlCnlNum || this.config.ArcDataCtrlCnlNum != arcDataCtrlCnlNum || this.config.EventsCtrlCnlNum != eventsCtrlCnlNum))
			{
				this.config.CurDataCtrlCnlNum = curDataCtrlCnlNum;
				this.config.ArcDataCtrlCnlNum = arcDataCtrlCnlNum;
				this.config.EventsCtrlCnlNum = eventsCtrlCnlNum;
				this.Modified = true;
			}
		}

		private void txtServer_TextChanged(object sender, EventArgs e)
		{
			if (!this.changing && this.selExpDest != null && this.selExpDestNode != null)
			{
				this.selExpDest.DataSource.Server = this.txtServer.Text;
				this.selExpDestNode.Text = this.selExpDest.DataSource.Name;
				this.SetConnectionString();
				this.Modified = true;
			}
		}

		private void txtDatabase_TextChanged(object sender, EventArgs e)
		{
			if (!this.changing && this.selExpDest != null)
			{
				this.selExpDest.DataSource.Database = this.txtDatabase.Text;
				this.SetConnectionString();
				this.Modified = true;
			}
		}

		private void txtUser_TextChanged(object sender, EventArgs e)
		{
			if (!this.changing && this.selExpDest != null)
			{
				this.selExpDest.DataSource.User = this.txtUser.Text;
				this.SetConnectionString();
				this.Modified = true;
			}
		}

		private void txtPassword_TextChanged(object sender, EventArgs e)
		{
			if (!this.changing && this.selExpDest != null)
			{
				this.selExpDest.DataSource.Password = this.txtPassword.Text;
				this.SetConnectionString();
				this.Modified = true;
			}
		}

		private void txtConnectionString_TextChanged(object sender, EventArgs e)
		{
			if (!this.changing && this.selExpDest != null)
			{
				this.selExpDest.DataSource.CfgFilePath = this.txtConnectionString.Text;
				this.SetConnControlsBackColor(KnownColor.Control, KnownColor.Window);
				this.Modified = true;
			}
		}

		private void ctrlExportCurDataQuery_PropChanged(object sender, EventArgs e)
		{
			if (!this.changing && this.selExpDest != null)
			{
				this.selExpDest.ExportParams.ExportCurData = this.ctrlExportCurDataQuery.Export;
				this.selExpDest.ExportParams.ExportCurDataQuery = this.ctrlExportCurDataQuery.Query;
				this.selExpDest.ExportParams.ExportCurDataCnls = this.ctrlExportCurDataQuery.ExportCnls;
				this.Modified = true;
			}
		}

		private void ctrlExportArcDataQuery_PropChanged(object sender, EventArgs e)
		{
			if (!this.changing && this.selExpDest != null)
			{
				this.selExpDest.ExportParams.ExportArcData = this.ctrlExportArcDataQuery.Export;
				this.selExpDest.ExportParams.ExportArcDataQuery = this.ctrlExportArcDataQuery.Query;
				this.selExpDest.ExportParams.ExportArcDataCnls = this.ctrlExportArcDataQuery.ExportCnls;
				this.Modified = true;
			}
		}

		private void ctrlExportEventQuery_PropChanged(object sender, EventArgs e)
		{
			if (!this.changing && this.selExpDest != null)
			{
				this.selExpDest.ExportParams.ExportEvents = this.ctrlExportEventQuery.Export;
				this.selExpDest.ExportParams.ExportEventQuery = this.ctrlExportEventQuery.Query;
				this.selExpDest.ExportParams.ExportEventCnls = this.ctrlExportEventQuery.ExportCnls;
				this.Modified = true;
			}
		}

		private void numMaxQueueSize_ValueChanged(object sender, EventArgs e)
		{
			if (!this.changing && this.selExpDest != null)
			{
				this.selExpDest.ExportParams.MaxQueueSize = Convert.ToInt32(this.numMaxQueueSize.Value);
				this.Modified = true;
			}
		}

		private void btnSave_Click(object sender, EventArgs e)
		{
			this.SaveConfig();
		}

		private void btnCancel_Click(object sender, EventArgs e)
		{
			this.config = this.configCopy;
			this.configCopy = this.config.Clone();
			this.ConfigToControls();
			this.Modified = false;
		}

		private AppDirs appDirs;

		private ServerComm serverComm;

		private ModConfig config;

		private ModConfig configCopy;

		private bool modified;

		private bool changing;

		private ModConfig.ExportDestination selExpDest;

		private TreeNode selExpDestNode;
	}
}
