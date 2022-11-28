namespace Scada.Server.Modules.TDEngineExport
{
	internal partial class FrmDBExportConfig : global::System.Windows.Forms.Form
	{
		protected override void Dispose(bool disposing)
		{
			if (disposing && this.components != null)
			{
				this.components.Dispose();
			}
			base.Dispose(disposing);
		}

		private void InitializeComponent()
		{
            this.components = new System.ComponentModel.Container();
            this.treeView = new System.Windows.Forms.TreeView();
            this.ilTree = new System.Windows.Forms.ImageList(this.components);
            this.btnSave = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.tabControl = new System.Windows.Forms.TabControl();
            this.pageConnection = new System.Windows.Forms.TabPage();
            this.txtConnectionString = new System.Windows.Forms.TextBox();
            this.lblConnectionString = new System.Windows.Forms.Label();
            this.lblPassword = new System.Windows.Forms.Label();
            this.txtPassword = new System.Windows.Forms.TextBox();
            this.txtUser = new System.Windows.Forms.TextBox();
            this.lblUser = new System.Windows.Forms.Label();
            this.txtDatabase = new System.Windows.Forms.TextBox();
            this.lblDatabase = new System.Windows.Forms.Label();
            this.txtServer = new System.Windows.Forms.TextBox();
            this.lblServer = new System.Windows.Forms.Label();
            this.pageExportCurDataQuery = new System.Windows.Forms.TabPage();
            this.ctrlExportCurDataQuery = new Scada.Server.Modules.TDEngineExport.CtrlExportQuery();
            this.pageExportArcDataQuery = new System.Windows.Forms.TabPage();
            this.ctrlExportArcDataQuery = new Scada.Server.Modules.TDEngineExport.CtrlExportQuery();
            this.pageExportEventQuery = new System.Windows.Forms.TabPage();
            this.ctrlExportEventQuery = new Scada.Server.Modules.TDEngineExport.CtrlExportQuery();
            this.pageMisc = new System.Windows.Forms.TabPage();
            this.numMaxQueueSize = new System.Windows.Forms.NumericUpDown();
            this.lblMaxQueueSize = new System.Windows.Forms.Label();
            this.toolStrip = new System.Windows.Forms.ToolStrip();
            this.ddbAddDataSource = new System.Windows.Forms.ToolStripDropDownButton();
            this.miAddSqlDataSource = new System.Windows.Forms.ToolStripMenuItem();
            this.btnDelDataSource = new System.Windows.Forms.ToolStripButton();
            this.sep1 = new System.Windows.Forms.ToolStripSeparator();
            this.btnManualExport = new System.Windows.Forms.ToolStripButton();
            this.lblInstruction = new System.Windows.Forms.Label();
            this.frmAuthorInfo1 = new Scada.Server.Modules.FrmAuthorInfo();
            this.tabControl.SuspendLayout();
            this.pageConnection.SuspendLayout();
            this.pageExportCurDataQuery.SuspendLayout();
            this.pageExportArcDataQuery.SuspendLayout();
            this.pageExportEventQuery.SuspendLayout();
            this.pageMisc.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numMaxQueueSize)).BeginInit();
            this.toolStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // treeView
            // 
            this.treeView.HideSelection = false;
            this.treeView.ImageIndex = 0;
            this.treeView.ImageList = this.ilTree;
            this.treeView.Location = new System.Drawing.Point(0, 52);
            this.treeView.Margin = new System.Windows.Forms.Padding(6);
            this.treeView.Name = "treeView";
            this.treeView.SelectedImageIndex = 0;
            this.treeView.ShowRootLines = false;
            this.treeView.Size = new System.Drawing.Size(396, 720);
            this.treeView.TabIndex = 1;
            this.treeView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeView_AfterSelect);
            // 
            // ilTree
            // 
            this.ilTree.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
            this.ilTree.ImageSize = new System.Drawing.Size(16, 16);
            this.ilTree.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // btnSave
            // 
            this.btnSave.Location = new System.Drawing.Point(870, 788);
            this.btnSave.Margin = new System.Windows.Forms.Padding(6);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(150, 42);
            this.btnSave.TabIndex = 4;
            this.btnSave.Text = "保存";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(1032, 788);
            this.btnCancel.Margin = new System.Windows.Forms.Padding(6);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(150, 42);
            this.btnCancel.TabIndex = 5;
            this.btnCancel.Text = "取消";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnClose
            // 
            this.btnClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnClose.Location = new System.Drawing.Point(1194, 788);
            this.btnClose.Margin = new System.Windows.Forms.Padding(6);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(150, 42);
            this.btnClose.TabIndex = 6;
            this.btnClose.Text = "关闭";
            this.btnClose.UseVisualStyleBackColor = true;
            // 
            // tabControl
            // 
            this.tabControl.Controls.Add(this.pageConnection);
            this.tabControl.Controls.Add(this.pageExportCurDataQuery);
            this.tabControl.Controls.Add(this.pageExportArcDataQuery);
            this.tabControl.Controls.Add(this.pageExportEventQuery);
            this.tabControl.Controls.Add(this.pageMisc);
            this.tabControl.Location = new System.Drawing.Point(400, 52);
            this.tabControl.Margin = new System.Windows.Forms.Padding(6);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(970, 726);
            this.tabControl.TabIndex = 3;
            // 
            // pageConnection
            // 
            this.pageConnection.Controls.Add(this.txtConnectionString);
            this.pageConnection.Controls.Add(this.lblConnectionString);
            this.pageConnection.Controls.Add(this.lblPassword);
            this.pageConnection.Controls.Add(this.txtPassword);
            this.pageConnection.Controls.Add(this.txtUser);
            this.pageConnection.Controls.Add(this.lblUser);
            this.pageConnection.Controls.Add(this.txtDatabase);
            this.pageConnection.Controls.Add(this.lblDatabase);
            this.pageConnection.Controls.Add(this.txtServer);
            this.pageConnection.Controls.Add(this.lblServer);
            this.pageConnection.Location = new System.Drawing.Point(8, 39);
            this.pageConnection.Margin = new System.Windows.Forms.Padding(6);
            this.pageConnection.Name = "pageConnection";
            this.pageConnection.Padding = new System.Windows.Forms.Padding(6);
            this.pageConnection.Size = new System.Drawing.Size(954, 679);
            this.pageConnection.TabIndex = 3;
            this.pageConnection.Text = "连接";
            this.pageConnection.UseVisualStyleBackColor = true;
            // 
            // txtConnectionString
            // 
            this.txtConnectionString.Location = new System.Drawing.Point(12, 252);
            this.txtConnectionString.Margin = new System.Windows.Forms.Padding(6);
            this.txtConnectionString.Multiline = true;
            this.txtConnectionString.Name = "txtConnectionString";
            this.txtConnectionString.Size = new System.Drawing.Size(926, 88);
            this.txtConnectionString.TabIndex = 9;
            this.txtConnectionString.Text = "/etc/taos/";
            this.txtConnectionString.TextChanged += new System.EventHandler(this.txtConnectionString_TextChanged);
            // 
            // lblConnectionString
            // 
            this.lblConnectionString.AutoSize = true;
            this.lblConnectionString.Location = new System.Drawing.Point(6, 222);
            this.lblConnectionString.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.lblConnectionString.Name = "lblConnectionString";
            this.lblConnectionString.Size = new System.Drawing.Size(202, 24);
            this.lblConnectionString.TabIndex = 8;
            this.lblConnectionString.Text = "本地配置文件路径";
            // 
            // lblPassword
            // 
            this.lblPassword.AutoSize = true;
            this.lblPassword.Location = new System.Drawing.Point(478, 150);
            this.lblPassword.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.lblPassword.Name = "lblPassword";
            this.lblPassword.Size = new System.Drawing.Size(58, 24);
            this.lblPassword.TabIndex = 6;
            this.lblPassword.Text = "密码";
            // 
            // txtPassword
            // 
            this.txtPassword.Location = new System.Drawing.Point(484, 180);
            this.txtPassword.Margin = new System.Windows.Forms.Padding(6);
            this.txtPassword.Name = "txtPassword";
            this.txtPassword.Size = new System.Drawing.Size(454, 35);
            this.txtPassword.TabIndex = 7;
            this.txtPassword.UseSystemPasswordChar = true;
            this.txtPassword.TextChanged += new System.EventHandler(this.txtPassword_TextChanged);
            // 
            // txtUser
            // 
            this.txtUser.Location = new System.Drawing.Point(12, 180);
            this.txtUser.Margin = new System.Windows.Forms.Padding(6);
            this.txtUser.Name = "txtUser";
            this.txtUser.Size = new System.Drawing.Size(456, 35);
            this.txtUser.TabIndex = 5;
            this.txtUser.TextChanged += new System.EventHandler(this.txtUser_TextChanged);
            // 
            // lblUser
            // 
            this.lblUser.AutoSize = true;
            this.lblUser.Location = new System.Drawing.Point(6, 150);
            this.lblUser.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.lblUser.Name = "lblUser";
            this.lblUser.Size = new System.Drawing.Size(82, 24);
            this.lblUser.TabIndex = 4;
            this.lblUser.Text = "用户名";
            // 
            // txtDatabase
            // 
            this.txtDatabase.Location = new System.Drawing.Point(12, 108);
            this.txtDatabase.Margin = new System.Windows.Forms.Padding(6);
            this.txtDatabase.Name = "txtDatabase";
            this.txtDatabase.Size = new System.Drawing.Size(926, 35);
            this.txtDatabase.TabIndex = 3;
            this.txtDatabase.TextChanged += new System.EventHandler(this.txtDatabase_TextChanged);
            // 
            // lblDatabase
            // 
            this.lblDatabase.AutoSize = true;
            this.lblDatabase.Location = new System.Drawing.Point(6, 78);
            this.lblDatabase.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.lblDatabase.Name = "lblDatabase";
            this.lblDatabase.Size = new System.Drawing.Size(82, 24);
            this.lblDatabase.TabIndex = 2;
            this.lblDatabase.Text = "数据库";
            // 
            // txtServer
            // 
            this.txtServer.Location = new System.Drawing.Point(12, 36);
            this.txtServer.Margin = new System.Windows.Forms.Padding(6);
            this.txtServer.Name = "txtServer";
            this.txtServer.Size = new System.Drawing.Size(926, 35);
            this.txtServer.TabIndex = 1;
            this.txtServer.TextChanged += new System.EventHandler(this.txtServer_TextChanged);
            // 
            // lblServer
            // 
            this.lblServer.AutoSize = true;
            this.lblServer.Location = new System.Drawing.Point(6, 6);
            this.lblServer.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.lblServer.Name = "lblServer";
            this.lblServer.Size = new System.Drawing.Size(82, 24);
            this.lblServer.TabIndex = 0;
            this.lblServer.Text = "服务器";
            // 
            // pageExportCurDataQuery
            // 
            this.pageExportCurDataQuery.Controls.Add(this.ctrlExportCurDataQuery);
            this.pageExportCurDataQuery.Location = new System.Drawing.Point(8, 39);
            this.pageExportCurDataQuery.Margin = new System.Windows.Forms.Padding(6);
            this.pageExportCurDataQuery.Name = "pageExportCurDataQuery";
            this.pageExportCurDataQuery.Padding = new System.Windows.Forms.Padding(6);
            this.pageExportCurDataQuery.Size = new System.Drawing.Size(954, 679);
            this.pageExportCurDataQuery.TabIndex = 0;
            this.pageExportCurDataQuery.Text = "当前数据";
            this.pageExportCurDataQuery.UseVisualStyleBackColor = true;
            // 
            // ctrlExportCurDataQuery
            // 
            this.ctrlExportCurDataQuery.Export = false;
            this.ctrlExportCurDataQuery.ExportCnls = "";
            this.ctrlExportCurDataQuery.Location = new System.Drawing.Point(12, 12);
            this.ctrlExportCurDataQuery.Margin = new System.Windows.Forms.Padding(12);
            this.ctrlExportCurDataQuery.Name = "ctrlExportCurDataQuery";
            this.ctrlExportCurDataQuery.Query = "";
            this.ctrlExportCurDataQuery.Size = new System.Drawing.Size(930, 656);
            this.ctrlExportCurDataQuery.TabIndex = 0;
            this.ctrlExportCurDataQuery.PropChanged += new System.EventHandler(this.ctrlExportCurDataQuery_PropChanged);
            // 
            // pageExportArcDataQuery
            // 
            this.pageExportArcDataQuery.Controls.Add(this.ctrlExportArcDataQuery);
            this.pageExportArcDataQuery.Location = new System.Drawing.Point(8, 39);
            this.pageExportArcDataQuery.Margin = new System.Windows.Forms.Padding(6);
            this.pageExportArcDataQuery.Name = "pageExportArcDataQuery";
            this.pageExportArcDataQuery.Padding = new System.Windows.Forms.Padding(6);
            this.pageExportArcDataQuery.Size = new System.Drawing.Size(954, 679);
            this.pageExportArcDataQuery.TabIndex = 1;
            this.pageExportArcDataQuery.Text = "存档数据";
            this.pageExportArcDataQuery.UseVisualStyleBackColor = true;
            // 
            // ctrlExportArcDataQuery
            // 
            this.ctrlExportArcDataQuery.Export = false;
            this.ctrlExportArcDataQuery.ExportCnls = "";
            this.ctrlExportArcDataQuery.Location = new System.Drawing.Point(12, 12);
            this.ctrlExportArcDataQuery.Margin = new System.Windows.Forms.Padding(12);
            this.ctrlExportArcDataQuery.Name = "ctrlExportArcDataQuery";
            this.ctrlExportArcDataQuery.Query = "";
            this.ctrlExportArcDataQuery.Size = new System.Drawing.Size(930, 656);
            this.ctrlExportArcDataQuery.TabIndex = 1;
            this.ctrlExportArcDataQuery.PropChanged += new System.EventHandler(this.ctrlExportArcDataQuery_PropChanged);
            // 
            // pageExportEventQuery
            // 
            this.pageExportEventQuery.Controls.Add(this.ctrlExportEventQuery);
            this.pageExportEventQuery.Location = new System.Drawing.Point(8, 39);
            this.pageExportEventQuery.Margin = new System.Windows.Forms.Padding(6);
            this.pageExportEventQuery.Name = "pageExportEventQuery";
            this.pageExportEventQuery.Padding = new System.Windows.Forms.Padding(6);
            this.pageExportEventQuery.Size = new System.Drawing.Size(954, 679);
            this.pageExportEventQuery.TabIndex = 2;
            this.pageExportEventQuery.Text = "事件";
            this.pageExportEventQuery.UseVisualStyleBackColor = true;
            // 
            // ctrlExportEventQuery
            // 
            this.ctrlExportEventQuery.Export = false;
            this.ctrlExportEventQuery.ExportCnls = "";
            this.ctrlExportEventQuery.Location = new System.Drawing.Point(12, 12);
            this.ctrlExportEventQuery.Margin = new System.Windows.Forms.Padding(12);
            this.ctrlExportEventQuery.Name = "ctrlExportEventQuery";
            this.ctrlExportEventQuery.Query = "";
            this.ctrlExportEventQuery.Size = new System.Drawing.Size(930, 656);
            this.ctrlExportEventQuery.TabIndex = 2;
            this.ctrlExportEventQuery.PropChanged += new System.EventHandler(this.ctrlExportEventQuery_PropChanged);
            // 
            // pageMisc
            // 
            this.pageMisc.Controls.Add(this.numMaxQueueSize);
            this.pageMisc.Controls.Add(this.lblMaxQueueSize);
            this.pageMisc.Location = new System.Drawing.Point(8, 39);
            this.pageMisc.Margin = new System.Windows.Forms.Padding(6);
            this.pageMisc.Name = "pageMisc";
            this.pageMisc.Padding = new System.Windows.Forms.Padding(6);
            this.pageMisc.Size = new System.Drawing.Size(954, 679);
            this.pageMisc.TabIndex = 4;
            this.pageMisc.Text = "杂项";
            this.pageMisc.UseVisualStyleBackColor = true;
            // 
            // numMaxQueueSize
            // 
            this.numMaxQueueSize.Location = new System.Drawing.Point(12, 36);
            this.numMaxQueueSize.Margin = new System.Windows.Forms.Padding(6);
            this.numMaxQueueSize.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.numMaxQueueSize.Name = "numMaxQueueSize";
            this.numMaxQueueSize.Size = new System.Drawing.Size(240, 35);
            this.numMaxQueueSize.TabIndex = 1;
            this.numMaxQueueSize.ValueChanged += new System.EventHandler(this.numMaxQueueSize_ValueChanged);
            // 
            // lblMaxQueueSize
            // 
            this.lblMaxQueueSize.AutoSize = true;
            this.lblMaxQueueSize.Location = new System.Drawing.Point(6, 6);
            this.lblMaxQueueSize.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.lblMaxQueueSize.Name = "lblMaxQueueSize";
            this.lblMaxQueueSize.Size = new System.Drawing.Size(106, 24);
            this.lblMaxQueueSize.TabIndex = 0;
            this.lblMaxQueueSize.Text = "队列长度";
            // 
            // toolStrip
            // 
            this.toolStrip.ImageScalingSize = new System.Drawing.Size(32, 32);
            this.toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ddbAddDataSource,
            this.btnDelDataSource,
            this.sep1,
            this.btnManualExport});
            this.toolStrip.Location = new System.Drawing.Point(0, 0);
            this.toolStrip.Name = "toolStrip";
            this.toolStrip.Padding = new System.Windows.Forms.Padding(0, 0, 4, 0);
            this.toolStrip.Size = new System.Drawing.Size(1368, 25);
            this.toolStrip.TabIndex = 0;
            this.toolStrip.Text = "toolStrip1";
            // 
            // ddbAddDataSource
            // 
            this.ddbAddDataSource.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.ddbAddDataSource.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.miAddSqlDataSource});
            this.ddbAddDataSource.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.ddbAddDataSource.Name = "ddbAddDataSource";
            this.ddbAddDataSource.Size = new System.Drawing.Size(22, 19);
            this.ddbAddDataSource.ToolTipText = "添加";
            // 
            // miAddSqlDataSource
            // 
            this.miAddSqlDataSource.Name = "miAddSqlDataSource";
            this.miAddSqlDataSource.Size = new System.Drawing.Size(257, 44);
            this.miAddSqlDataSource.Text = "TDEngine";
            this.miAddSqlDataSource.Click += new System.EventHandler(this.miAddDataSource_Click);
            // 
            // btnDelDataSource
            // 
            this.btnDelDataSource.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnDelDataSource.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnDelDataSource.Name = "btnDelDataSource";
            this.btnDelDataSource.Size = new System.Drawing.Size(46, 19);
            this.btnDelDataSource.ToolTipText = "删除";
            this.btnDelDataSource.Click += new System.EventHandler(this.btnDelDataSource_Click);
            // 
            // sep1
            // 
            this.sep1.Name = "sep1";
            this.sep1.Size = new System.Drawing.Size(6, 25);
            // 
            // btnManualExport
            // 
            this.btnManualExport.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnManualExport.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnManualExport.Name = "btnManualExport";
            this.btnManualExport.Size = new System.Drawing.Size(46, 19);
            this.btnManualExport.ToolTipText = "手动导出";
            this.btnManualExport.Click += new System.EventHandler(this.btnManualExport_Click);
            // 
            // lblInstruction
            // 
            this.lblInstruction.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.lblInstruction.ForeColor = System.Drawing.SystemColors.GrayText;
            this.lblInstruction.Location = new System.Drawing.Point(400, 462);
            this.lblInstruction.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.lblInstruction.Name = "lblInstruction";
            this.lblInstruction.Size = new System.Drawing.Size(970, 42);
            this.lblInstruction.TabIndex = 2;
            this.lblInstruction.Text = "请添加数据源";
            this.lblInstruction.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // frmAuthorInfo1
            // 
            this.frmAuthorInfo1.Location = new System.Drawing.Point(32, 796);
            this.frmAuthorInfo1.Margin = new System.Windows.Forms.Padding(12);
            this.frmAuthorInfo1.Name = "frmAuthorInfo1";
            this.frmAuthorInfo1.Size = new System.Drawing.Size(696, 46);
            this.frmAuthorInfo1.TabIndex = 7;
            // 
            // FrmDBExportConfig
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 24F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnClose;
            this.ClientSize = new System.Drawing.Size(1368, 852);
            this.Controls.Add(this.frmAuthorInfo1);
            this.Controls.Add(this.lblInstruction);
            this.Controls.Add(this.toolStrip);
            this.Controls.Add(this.tabControl);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.treeView);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Margin = new System.Windows.Forms.Padding(6);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FrmDBExportConfig";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "TDEngine Export";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FrmDBExportConfig_FormClosing);
            this.Load += new System.EventHandler(this.FrmDBExportConfig_Load);
            this.tabControl.ResumeLayout(false);
            this.pageConnection.ResumeLayout(false);
            this.pageConnection.PerformLayout();
            this.pageExportCurDataQuery.ResumeLayout(false);
            this.pageExportArcDataQuery.ResumeLayout(false);
            this.pageExportEventQuery.ResumeLayout(false);
            this.pageMisc.ResumeLayout(false);
            this.pageMisc.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numMaxQueueSize)).EndInit();
            this.toolStrip.ResumeLayout(false);
            this.toolStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		private global::System.ComponentModel.IContainer components;

		private global::System.Windows.Forms.TreeView treeView;

		private global::System.Windows.Forms.Button btnSave;

		private global::System.Windows.Forms.Button btnCancel;

		private global::System.Windows.Forms.Button btnClose;

		private global::System.Windows.Forms.TabControl tabControl;

		private global::System.Windows.Forms.TabPage pageExportCurDataQuery;

		private global::System.Windows.Forms.TabPage pageExportArcDataQuery;

		private global::System.Windows.Forms.TabPage pageExportEventQuery;

		private global::System.Windows.Forms.ToolStrip toolStrip;

		private global::System.Windows.Forms.ToolStripDropDownButton ddbAddDataSource;

		private global::System.Windows.Forms.ToolStripMenuItem miAddSqlDataSource;

		private global::System.Windows.Forms.ToolStripButton btnDelDataSource;

		private global::System.Windows.Forms.Label lblInstruction;

		private global::Scada.Server.Modules.TDEngineExport.CtrlExportQuery ctrlExportCurDataQuery;

		private global::Scada.Server.Modules.TDEngineExport.CtrlExportQuery ctrlExportArcDataQuery;

		private global::Scada.Server.Modules.TDEngineExport.CtrlExportQuery ctrlExportEventQuery;

		private global::System.Windows.Forms.TabPage pageConnection;

		private global::System.Windows.Forms.TextBox txtServer;

		private global::System.Windows.Forms.Label lblServer;

		private global::System.Windows.Forms.TextBox txtDatabase;

		private global::System.Windows.Forms.Label lblDatabase;

		private global::System.Windows.Forms.TextBox txtPassword;

		private global::System.Windows.Forms.TextBox txtUser;

		private global::System.Windows.Forms.Label lblUser;

		private global::System.Windows.Forms.Label lblPassword;

		private global::System.Windows.Forms.Label lblConnectionString;

		private global::System.Windows.Forms.TextBox txtConnectionString;

		private global::System.Windows.Forms.ImageList ilTree;

		private global::System.Windows.Forms.ToolStripSeparator sep1;

		private global::System.Windows.Forms.ToolStripButton btnManualExport;

		private global::System.Windows.Forms.TabPage pageMisc;

		private global::System.Windows.Forms.Label lblMaxQueueSize;

		private global::System.Windows.Forms.NumericUpDown numMaxQueueSize;

		private global::Scada.Server.Modules.FrmAuthorInfo frmAuthorInfo1;
	}
}
