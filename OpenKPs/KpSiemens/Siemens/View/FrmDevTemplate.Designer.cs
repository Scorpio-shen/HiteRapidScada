namespace KpSiemens.Siemens.View
{
    partial class FrmDevTemplate
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmDevTemplate));
            System.Windows.Forms.TreeNode treeNode1 = new System.Windows.Forms.TreeNode("变量集合");
            this.toolStrip = new System.Windows.Forms.ToolStrip();
            this.btnNew = new System.Windows.Forms.ToolStripButton();
            this.btnOpen = new System.Windows.Forms.ToolStripButton();
            this.btnSave = new System.Windows.Forms.ToolStripButton();
            this.btnSaveAs = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.btnAddTagGroup = new System.Windows.Forms.ToolStripButton();
            this.btnMoveUp = new System.Windows.Forms.ToolStripButton();
            this.btnMoveDown = new System.Windows.Forms.ToolStripButton();
            this.btnDelete = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.btnEditSettings = new System.Windows.Forms.ToolStripButton();
            this.btnEditSettingsExt = new System.Windows.Forms.ToolStripButton();
            this.btnImport = new System.Windows.Forms.ToolStripButton();
            this.btnExport = new System.Windows.Forms.ToolStripButton();
            this.gbDevTemplate = new System.Windows.Forms.GroupBox();
            this.treeView = new System.Windows.Forms.TreeView();
            this.imageList = new System.Windows.Forms.ImageList(this.components);
            this.saveFileDialog = new System.Windows.Forms.SaveFileDialog();
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.ctrlPLCConfig = new KpSiemens.Siemens.View.CtrlPLCConfig();
            this.ctrlRead = new KpSiemens.Siemens.View.CtrlRead();
            this.toolStrip.SuspendLayout();
            this.gbDevTemplate.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolStrip
            // 
            this.toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnNew,
            this.btnOpen,
            this.btnSave,
            this.btnSaveAs,
            this.toolStripSeparator1,
            this.btnAddTagGroup,
            this.btnMoveUp,
            this.btnMoveDown,
            this.btnDelete,
            this.toolStripSeparator2,
            this.btnEditSettings,
            this.btnEditSettingsExt,
            this.btnImport,
            this.btnExport});
            this.toolStrip.Location = new System.Drawing.Point(0, 0);
            this.toolStrip.Name = "toolStrip";
            this.toolStrip.Size = new System.Drawing.Size(970, 25);
            this.toolStrip.TabIndex = 1;
            this.toolStrip.Text = "toolStrip1";
            // 
            // btnNew
            // 
            this.btnNew.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnNew.Image = ((System.Drawing.Image)(resources.GetObject("btnNew.Image")));
            this.btnNew.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnNew.Name = "btnNew";
            this.btnNew.Size = new System.Drawing.Size(23, 22);
            this.btnNew.ToolTipText = "Create new template";
            this.btnNew.Click += new System.EventHandler(this.btnNew_Click);
            // 
            // btnOpen
            // 
            this.btnOpen.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnOpen.Image = ((System.Drawing.Image)(resources.GetObject("btnOpen.Image")));
            this.btnOpen.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnOpen.Name = "btnOpen";
            this.btnOpen.Size = new System.Drawing.Size(23, 22);
            this.btnOpen.ToolTipText = "Open template";
            this.btnOpen.Click += new System.EventHandler(this.btnOpen_Click);
            // 
            // btnSave
            // 
            this.btnSave.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnSave.Image = ((System.Drawing.Image)(resources.GetObject("btnSave.Image")));
            this.btnSave.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(23, 22);
            this.btnSave.ToolTipText = "Save template";
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnSaveAs
            // 
            this.btnSaveAs.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnSaveAs.Image = ((System.Drawing.Image)(resources.GetObject("btnSaveAs.Image")));
            this.btnSaveAs.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnSaveAs.Name = "btnSaveAs";
            this.btnSaveAs.Size = new System.Drawing.Size(23, 22);
            this.btnSaveAs.ToolTipText = "Save template as";
            this.btnSaveAs.Click += new System.EventHandler(this.btnSaveAs_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // btnAddTagGroup
            // 
            this.btnAddTagGroup.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnAddTagGroup.Image = ((System.Drawing.Image)(resources.GetObject("btnAddTagGroup.Image")));
            this.btnAddTagGroup.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnAddTagGroup.Name = "btnAddTagGroup";
            this.btnAddTagGroup.Size = new System.Drawing.Size(23, 22);
            this.btnAddTagGroup.ToolTipText = "Add tag group";
            this.btnAddTagGroup.Click += new System.EventHandler(this.btnAddTagGroup_Click);
            // 
            // btnMoveUp
            // 
            this.btnMoveUp.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnMoveUp.Image = ((System.Drawing.Image)(resources.GetObject("btnMoveUp.Image")));
            this.btnMoveUp.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnMoveUp.Name = "btnMoveUp";
            this.btnMoveUp.Size = new System.Drawing.Size(23, 22);
            this.btnMoveUp.ToolTipText = "Move up";
            this.btnMoveUp.Click += new System.EventHandler(this.btnMoveUp_Click);
            // 
            // btnMoveDown
            // 
            this.btnMoveDown.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnMoveDown.Image = ((System.Drawing.Image)(resources.GetObject("btnMoveDown.Image")));
            this.btnMoveDown.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnMoveDown.Name = "btnMoveDown";
            this.btnMoveDown.Size = new System.Drawing.Size(23, 22);
            this.btnMoveDown.ToolTipText = "Move down";
            this.btnMoveDown.Click += new System.EventHandler(this.btnMoveDown_Click);
            // 
            // btnDelete
            // 
            this.btnDelete.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnDelete.Image = ((System.Drawing.Image)(resources.GetObject("btnDelete.Image")));
            this.btnDelete.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(23, 22);
            this.btnDelete.ToolTipText = "Delete";
            this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
            // 
            // btnEditSettings
            // 
            this.btnEditSettings.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnEditSettings.Image = ((System.Drawing.Image)(resources.GetObject("btnEditSettings.Image")));
            this.btnEditSettings.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnEditSettings.Name = "btnEditSettings";
            this.btnEditSettings.Size = new System.Drawing.Size(23, 22);
            this.btnEditSettings.ToolTipText = "Edit template settings";
            // 
            // btnEditSettingsExt
            // 
            this.btnEditSettingsExt.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnEditSettingsExt.Image = ((System.Drawing.Image)(resources.GetObject("btnEditSettingsExt.Image")));
            this.btnEditSettingsExt.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnEditSettingsExt.Name = "btnEditSettingsExt";
            this.btnEditSettingsExt.Size = new System.Drawing.Size(23, 22);
            this.btnEditSettingsExt.ToolTipText = "Edit extended settings";
            // 
            // btnImport
            // 
            this.btnImport.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnImport.Image = ((System.Drawing.Image)(resources.GetObject("btnImport.Image")));
            this.btnImport.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnImport.Name = "btnImport";
            this.btnImport.Size = new System.Drawing.Size(23, 22);
            this.btnImport.Text = "toolStripButton1";
            this.btnImport.ToolTipText = "导入";
            this.btnImport.Click += new System.EventHandler(this.btnImport_Click);
            // 
            // btnExport
            // 
            this.btnExport.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnExport.Image = ((System.Drawing.Image)(resources.GetObject("btnExport.Image")));
            this.btnExport.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnExport.Name = "btnExport";
            this.btnExport.Size = new System.Drawing.Size(23, 22);
            this.btnExport.Text = "toolStripButton1";
            this.btnExport.ToolTipText = "导出";
            this.btnExport.Click += new System.EventHandler(this.btnExport_Click);
            // 
            // gbDevTemplate
            // 
            this.gbDevTemplate.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.gbDevTemplate.Controls.Add(this.treeView);
            this.gbDevTemplate.Location = new System.Drawing.Point(12, 28);
            this.gbDevTemplate.Name = "gbDevTemplate";
            this.gbDevTemplate.Padding = new System.Windows.Forms.Padding(10, 3, 10, 9);
            this.gbDevTemplate.Size = new System.Drawing.Size(275, 562);
            this.gbDevTemplate.TabIndex = 2;
            this.gbDevTemplate.TabStop = false;
            this.gbDevTemplate.Text = "Device template";
            // 
            // treeView
            // 
            this.treeView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.treeView.HideSelection = false;
            this.treeView.ImageIndex = 0;
            this.treeView.ImageList = this.imageList;
            this.treeView.Location = new System.Drawing.Point(13, 18);
            this.treeView.Name = "treeView";
            treeNode1.ImageKey = "group.png";
            treeNode1.Name = "tagGroupNode";
            treeNode1.SelectedImageKey = "group.png";
            treeNode1.Text = "变量集合";
            this.treeView.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
            treeNode1});
            this.treeView.SelectedImageIndex = 0;
            this.treeView.ShowRootLines = false;
            this.treeView.Size = new System.Drawing.Size(249, 533);
            this.treeView.TabIndex = 0;
            this.treeView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeView_AfterSelect);
            // 
            // imageList
            // 
            this.imageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList.ImageStream")));
            this.imageList.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList.Images.SetKeyName(0, "group.png");
            this.imageList.Images.SetKeyName(1, "group_inactive.png");
            this.imageList.Images.SetKeyName(2, "elem.png");
            this.imageList.Images.SetKeyName(3, "cmds.png");
            this.imageList.Images.SetKeyName(4, "cmd.png");
            this.imageList.Images.SetKeyName(5, "export.png");
            this.imageList.Images.SetKeyName(6, "import excel.png");
            // 
            // openFileDialog
            // 
            this.openFileDialog.FileName = "openFileDialog";
            // 
            // ctrlPLCConfig
            // 
            this.ctrlPLCConfig.ConnectionOptions = null;
            this.ctrlPLCConfig.IsShowCmdGroup = false;
            this.ctrlPLCConfig.Location = new System.Drawing.Point(293, 28);
            this.ctrlPLCConfig.Name = "ctrlPLCConfig";
            this.ctrlPLCConfig.Size = new System.Drawing.Size(665, 123);
            this.ctrlPLCConfig.TabIndex = 15;
            this.ctrlPLCConfig.PLCConfigChanged += new KpSiemens.Siemens.Model.PLCConfigChangedEventHandler(this.ctrlPLCConfig_PLCConfigChanged);
            // 
            // ctrlRead
            // 
            this.ctrlRead.IsShowTagGroup = false;
            this.ctrlRead.Location = new System.Drawing.Point(293, 157);
            this.ctrlRead.Name = "ctrlRead";
            this.ctrlRead.SiemensTagGroup = null;
            this.ctrlRead.Size = new System.Drawing.Size(665, 433);
            this.ctrlRead.TabIndex = 13;
            this.ctrlRead.TagGroupChanged += new KpSiemens.Siemens.Model.TagGroupChangedEventHandler(this.ctrlRead_TagGroupChanged);
            // 
            // FrmDevTemplate
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(970, 596);
            this.Controls.Add(this.ctrlPLCConfig);
            this.Controls.Add(this.ctrlRead);
            this.Controls.Add(this.gbDevTemplate);
            this.Controls.Add(this.toolStrip);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FrmDevTemplate";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "模板配置";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FrmDevTemplate_FormClosing);
            this.Load += new System.EventHandler(this.FrmDevTemplate_Load);
            this.toolStrip.ResumeLayout(false);
            this.toolStrip.PerformLayout();
            this.gbDevTemplate.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolStrip toolStrip;
        private System.Windows.Forms.ToolStripButton btnNew;
        private System.Windows.Forms.ToolStripButton btnOpen;
        private System.Windows.Forms.ToolStripButton btnSave;
        private System.Windows.Forms.ToolStripButton btnSaveAs;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton btnAddTagGroup;
        private System.Windows.Forms.ToolStripButton btnMoveUp;
        private System.Windows.Forms.ToolStripButton btnMoveDown;
        private System.Windows.Forms.ToolStripButton btnDelete;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripButton btnEditSettings;
        private System.Windows.Forms.ToolStripButton btnEditSettingsExt;
        private System.Windows.Forms.GroupBox gbDevTemplate;
        private System.Windows.Forms.TreeView treeView;
        private System.Windows.Forms.SaveFileDialog saveFileDialog;
        private System.Windows.Forms.OpenFileDialog openFileDialog;
        private CtrlRead ctrlRead;
        private System.Windows.Forms.ImageList imageList;
        private CtrlPLCConfig ctrlPLCConfig;
        private System.Windows.Forms.ToolStripButton btnImport;
        private System.Windows.Forms.ToolStripButton btnExport;
    }
}