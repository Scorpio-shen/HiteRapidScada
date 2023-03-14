namespace KpHiteMqtt.Mqtt.View
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
            this.toolStrip = new System.Windows.Forms.ToolStrip();
            this.btnSave = new System.Windows.Forms.ToolStripButton();
            this.btnSaveAs = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.btnImport = new System.Windows.Forms.ToolStripButton();
            this.btnExport = new System.Windows.Forms.ToolStripButton();
            this.imageList = new System.Windows.Forms.ImageList(this.components);
            this.saveFileDialog = new System.Windows.Forms.SaveFileDialog();
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.dgvProperty = new System.Windows.Forms.DataGridView();
            this.ctxMenuViewOperate = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsmEditModel = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmDeleteModel = new System.Windows.Forms.ToolStripMenuItem();
            this.btnAddModel = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.txtPassword = new System.Windows.Forms.TextBox();
            this.txtUserName = new System.Windows.Forms.TextBox();
            this.label_Password = new System.Windows.Forms.Label();
            this.label_UseName = new System.Windows.Forms.Label();
            this.txtAliveInterval = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.txtPort = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txtServerIp = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.txtClientId = new System.Windows.Forms.TextBox();
            this.lblClientId = new System.Windows.Forms.Label();
            this.bdsProperty = new System.Windows.Forms.BindingSource(this.components);
            this.btnAddTopic = new System.Windows.Forms.Button();
            this.columnCheck = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.columnId = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.columnName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.columnIdentifier = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.columnDataType = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.columnUnit = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.column_Description = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.toolStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvProperty)).BeginInit();
            this.ctxMenuViewOperate.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.bdsProperty)).BeginInit();
            this.SuspendLayout();
            // 
            // toolStrip
            // 
            this.toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnSave,
            this.btnSaveAs,
            this.toolStripSeparator1,
            this.toolStripSeparator2,
            this.btnImport,
            this.btnExport});
            this.toolStrip.Location = new System.Drawing.Point(0, 0);
            this.toolStrip.Name = "toolStrip";
            this.toolStrip.Size = new System.Drawing.Size(970, 25);
            this.toolStrip.TabIndex = 1;
            this.toolStrip.Text = "toolStrip1";
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
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
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
            // dgvProperty
            // 
            this.dgvProperty.AllowUserToAddRows = false;
            this.dgvProperty.AllowUserToDeleteRows = false;
            this.dgvProperty.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvProperty.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvProperty.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.columnCheck,
            this.columnId,
            this.columnName,
            this.columnIdentifier,
            this.columnDataType,
            this.columnUnit,
            this.column_Description});
            this.dgvProperty.ContextMenuStrip = this.ctxMenuViewOperate;
            this.dgvProperty.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvProperty.Location = new System.Drawing.Point(3, 17);
            this.dgvProperty.Name = "dgvProperty";
            this.dgvProperty.RowTemplate.Height = 23;
            this.dgvProperty.Size = new System.Drawing.Size(964, 545);
            this.dgvProperty.TabIndex = 2;
            // 
            // ctxMenuViewOperate
            // 
            this.ctxMenuViewOperate.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmEditModel,
            this.tsmDeleteModel});
            this.ctxMenuViewOperate.Name = "ctxMenuViewOperate";
            this.ctxMenuViewOperate.Size = new System.Drawing.Size(101, 48);
            // 
            // tsmEditModel
            // 
            this.tsmEditModel.Name = "tsmEditModel";
            this.tsmEditModel.Size = new System.Drawing.Size(100, 22);
            this.tsmEditModel.Text = "编辑";
            this.tsmEditModel.Click += new System.EventHandler(this.tsmEditModel_Click);
            // 
            // tsmDeleteModel
            // 
            this.tsmDeleteModel.Name = "tsmDeleteModel";
            this.tsmDeleteModel.Size = new System.Drawing.Size(100, 22);
            this.tsmDeleteModel.Text = "删除";
            this.tsmDeleteModel.Click += new System.EventHandler(this.tsmDeleteModel_Click);
            // 
            // btnAddModel
            // 
            this.btnAddModel.Location = new System.Drawing.Point(885, 201);
            this.btnAddModel.Name = "btnAddModel";
            this.btnAddModel.Size = new System.Drawing.Size(82, 32);
            this.btnAddModel.TabIndex = 4;
            this.btnAddModel.Text = "添加物模型";
            this.btnAddModel.UseVisualStyleBackColor = true;
            this.btnAddModel.Click += new System.EventHandler(this.btnAddModel_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.dgvProperty);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.groupBox1.Location = new System.Drawing.Point(0, 239);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(970, 565);
            this.groupBox1.TabIndex = 5;
            this.groupBox1.TabStop = false;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.txtPassword);
            this.groupBox2.Controls.Add(this.txtUserName);
            this.groupBox2.Controls.Add(this.label_Password);
            this.groupBox2.Controls.Add(this.label_UseName);
            this.groupBox2.Controls.Add(this.txtAliveInterval);
            this.groupBox2.Controls.Add(this.label4);
            this.groupBox2.Controls.Add(this.txtPort);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Controls.Add(this.txtServerIp);
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Controls.Add(this.txtClientId);
            this.groupBox2.Controls.Add(this.lblClientId);
            this.groupBox2.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBox2.Location = new System.Drawing.Point(0, 25);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(970, 166);
            this.groupBox2.TabIndex = 6;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Mqtt服务器连接参数";
            // 
            // txtPassword
            // 
            this.txtPassword.Location = new System.Drawing.Point(568, 61);
            this.txtPassword.Name = "txtPassword";
            this.txtPassword.Size = new System.Drawing.Size(129, 21);
            this.txtPassword.TabIndex = 12;
            // 
            // txtUserName
            // 
            this.txtUserName.Location = new System.Drawing.Point(568, 24);
            this.txtUserName.Name = "txtUserName";
            this.txtUserName.Size = new System.Drawing.Size(129, 21);
            this.txtUserName.TabIndex = 13;
            // 
            // label_Password
            // 
            this.label_Password.Font = new System.Drawing.Font("宋体", 9F);
            this.label_Password.Location = new System.Drawing.Point(492, 64);
            this.label_Password.Name = "label_Password";
            this.label_Password.Size = new System.Drawing.Size(70, 14);
            this.label_Password.TabIndex = 10;
            this.label_Password.Text = "Password:";
            this.label_Password.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label_UseName
            // 
            this.label_UseName.Font = new System.Drawing.Font("宋体", 9F);
            this.label_UseName.Location = new System.Drawing.Point(492, 27);
            this.label_UseName.Name = "label_UseName";
            this.label_UseName.Size = new System.Drawing.Size(70, 14);
            this.label_UseName.TabIndex = 11;
            this.label_UseName.Text = "UserName:";
            this.label_UseName.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtAliveInterval
            // 
            this.txtAliveInterval.Location = new System.Drawing.Point(568, 102);
            this.txtAliveInterval.Name = "txtAliveInterval";
            this.txtAliveInterval.Size = new System.Drawing.Size(129, 21);
            this.txtAliveInterval.TabIndex = 7;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(485, 106);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(77, 12);
            this.label4.TabIndex = 6;
            this.label4.Text = "心跳间隔(s):";
            // 
            // txtPort
            // 
            this.txtPort.Location = new System.Drawing.Point(189, 102);
            this.txtPort.Name = "txtPort";
            this.txtPort.Size = new System.Drawing.Size(129, 21);
            this.txtPort.TabIndex = 5;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(136, 106);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(47, 12);
            this.label2.TabIndex = 4;
            this.label2.Text = "端口号:";
            // 
            // txtServerIp
            // 
            this.txtServerIp.Location = new System.Drawing.Point(189, 61);
            this.txtServerIp.Name = "txtServerIp";
            this.txtServerIp.Size = new System.Drawing.Size(129, 21);
            this.txtServerIp.TabIndex = 3;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(100, 66);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(83, 12);
            this.label1.TabIndex = 2;
            this.label1.Text = "Mqtt服务器IP:";
            // 
            // txtClientId
            // 
            this.txtClientId.Location = new System.Drawing.Point(189, 24);
            this.txtClientId.Name = "txtClientId";
            this.txtClientId.Size = new System.Drawing.Size(129, 21);
            this.txtClientId.TabIndex = 1;
            // 
            // lblClientId
            // 
            this.lblClientId.AutoSize = true;
            this.lblClientId.Location = new System.Drawing.Point(124, 28);
            this.lblClientId.Name = "lblClientId";
            this.lblClientId.Size = new System.Drawing.Size(59, 12);
            this.lblClientId.TabIndex = 0;
            this.lblClientId.Text = "ClientId:";
            // 
            // btnAddTopic
            // 
            this.btnAddTopic.Location = new System.Drawing.Point(797, 201);
            this.btnAddTopic.Name = "btnAddTopic";
            this.btnAddTopic.Size = new System.Drawing.Size(82, 32);
            this.btnAddTopic.TabIndex = 7;
            this.btnAddTopic.Text = "添加Topic";
            this.btnAddTopic.UseVisualStyleBackColor = true;
            this.btnAddTopic.Click += new System.EventHandler(this.btnAddTopic_Click);
            // 
            // columnCheck
            // 
            this.columnCheck.FillWeight = 20F;
            this.columnCheck.HeaderText = "";
            this.columnCheck.Name = "columnCheck";
            this.columnCheck.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.columnCheck.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            // 
            // columnId
            // 
            this.columnId.DataPropertyName = "Id";
            this.columnId.HeaderText = "Id";
            this.columnId.Name = "columnId";
            this.columnId.Visible = false;
            // 
            // columnName
            // 
            this.columnName.DataPropertyName = "Name";
            this.columnName.HeaderText = "名称";
            this.columnName.Name = "columnName";
            // 
            // columnIdentifier
            // 
            this.columnIdentifier.DataPropertyName = "Identifier";
            this.columnIdentifier.HeaderText = "标识符";
            this.columnIdentifier.Name = "columnIdentifier";
            // 
            // columnDataType
            // 
            this.columnDataType.DataPropertyName = "DataType";
            this.columnDataType.HeaderText = "数据类型";
            this.columnDataType.Name = "columnDataType";
            // 
            // columnUnit
            // 
            this.columnUnit.DataPropertyName = "Unit";
            this.columnUnit.HeaderText = "单位";
            this.columnUnit.Name = "columnUnit";
            // 
            // column_Description
            // 
            this.column_Description.DataPropertyName = "Description";
            this.column_Description.HeaderText = "描述";
            this.column_Description.Name = "column_Description";
            // 
            // FrmDevTemplate
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(970, 804);
            this.Controls.Add(this.btnAddTopic);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.btnAddModel);
            this.Controls.Add(this.toolStrip);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FrmDevTemplate";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = " ";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FrmDevTemplate_FormClosing);
            this.Load += new System.EventHandler(this.FrmDevTemplate_Load);
            this.toolStrip.ResumeLayout(false);
            this.toolStrip.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvProperty)).EndInit();
            this.ctxMenuViewOperate.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.bdsProperty)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolStrip toolStrip;
        private System.Windows.Forms.ToolStripButton btnSave;
        private System.Windows.Forms.ToolStripButton btnSaveAs;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.SaveFileDialog saveFileDialog;
        private System.Windows.Forms.OpenFileDialog openFileDialog;
        private System.Windows.Forms.ImageList imageList;
        private System.Windows.Forms.ToolStripButton btnImport;
        private System.Windows.Forms.ToolStripButton btnExport;
        private System.Windows.Forms.DataGridView dgvProperty;
        private System.Windows.Forms.ContextMenuStrip ctxMenuViewOperate;
        private System.Windows.Forms.ToolStripMenuItem tsmEditModel;
        private System.Windows.Forms.ToolStripMenuItem tsmDeleteModel;
        private System.Windows.Forms.Button btnAddModel;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label lblClientId;
        private System.Windows.Forms.TextBox txtClientId;
        private System.Windows.Forms.TextBox txtAliveInterval;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtPort;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtServerIp;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtPassword;
        private System.Windows.Forms.TextBox txtUserName;
        private System.Windows.Forms.Label label_Password;
        private System.Windows.Forms.Label label_UseName;
        private System.Windows.Forms.BindingSource bdsProperty;
        private System.Windows.Forms.Button btnAddTopic;
        private System.Windows.Forms.DataGridViewCheckBoxColumn columnCheck;
        private System.Windows.Forms.DataGridViewTextBoxColumn columnId;
        private System.Windows.Forms.DataGridViewTextBoxColumn columnName;
        private System.Windows.Forms.DataGridViewTextBoxColumn columnIdentifier;
        private System.Windows.Forms.DataGridViewTextBoxColumn columnDataType;
        private System.Windows.Forms.DataGridViewTextBoxColumn columnUnit;
        private System.Windows.Forms.DataGridViewTextBoxColumn column_Description;
    }
}