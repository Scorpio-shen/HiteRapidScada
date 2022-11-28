namespace Scada.Comm.Devices.Modbus.UI
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
            this.imageList = new System.Windows.Forms.ImageList(this.components);
            this.toolStrip = new System.Windows.Forms.ToolStrip();
            this.btnNew = new System.Windows.Forms.ToolStripButton();
            this.btnOpen = new System.Windows.Forms.ToolStripButton();
            this.btnSave = new System.Windows.Forms.ToolStripButton();
            this.btnSaveAs = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.btnAddElemGroup = new System.Windows.Forms.ToolStripButton();
            this.btnAddElem = new System.Windows.Forms.ToolStripButton();
            this.btnAddCmd = new System.Windows.Forms.ToolStripButton();
            this.btnMoveUp = new System.Windows.Forms.ToolStripButton();
            this.btnMoveDown = new System.Windows.Forms.ToolStripButton();
            this.btnDelete = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.btnEditSettings = new System.Windows.Forms.ToolStripButton();
            this.btnEditSettingsExt = new System.Windows.Forms.ToolStripButton();
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.saveFileDialog = new System.Windows.Forms.SaveFileDialog();
            this.cbxRegisterType = new System.Windows.Forms.ComboBox();
            this.numTagCount = new System.Windows.Forms.NumericUpDown();
            this.lblRegisterType = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.dgvTagGroup = new System.Windows.Forms.DataGridView();
            this.tagName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.TagGroupDataType = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.TagAddress = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.txtStartAddress = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txtCmdAddr = new System.Windows.Forms.TextBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.btnAddModbusCmd = new System.Windows.Forms.Button();
            this.dgvCmd = new System.Windows.Forms.DataGridView();
            this.cmdName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Address = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.CmdModbusRegisterType = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.CmdFunctionCode = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.CmdNum = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.label5 = new System.Windows.Forms.Label();
            this.cbCmdElemType = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.cbCmdRegType = new System.Windows.Forms.ComboBox();
            this.bdsTagGroups = new System.Windows.Forms.BindingSource(this.components);
            this.bdsCmds = new System.Windows.Forms.BindingSource(this.components);
            this.toolStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numTagCount)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvTagGroup)).BeginInit();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvCmd)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.bdsTagGroups)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.bdsCmds)).BeginInit();
            this.SuspendLayout();
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
            // 
            // toolStrip
            // 
            this.toolStrip.ImageScalingSize = new System.Drawing.Size(36, 36);
            this.toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnNew,
            this.btnOpen,
            this.btnSave,
            this.btnSaveAs,
            this.toolStripSeparator1,
            this.btnAddElemGroup,
            this.btnAddElem,
            this.btnAddCmd,
            this.btnMoveUp,
            this.btnMoveDown,
            this.btnDelete,
            this.toolStripSeparator2,
            this.btnEditSettings,
            this.btnEditSettingsExt});
            this.toolStrip.Location = new System.Drawing.Point(0, 0);
            this.toolStrip.Name = "toolStrip";
            this.toolStrip.Padding = new System.Windows.Forms.Padding(0, 0, 2, 0);
            this.toolStrip.Size = new System.Drawing.Size(811, 43);
            this.toolStrip.TabIndex = 0;
            this.toolStrip.Text = "toolStrip1";
            // 
            // btnNew
            // 
            this.btnNew.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnNew.Image = ((System.Drawing.Image)(resources.GetObject("btnNew.Image")));
            this.btnNew.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnNew.Name = "btnNew";
            this.btnNew.Size = new System.Drawing.Size(40, 40);
            this.btnNew.ToolTipText = "Create new template";
            // 
            // btnOpen
            // 
            this.btnOpen.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnOpen.Image = ((System.Drawing.Image)(resources.GetObject("btnOpen.Image")));
            this.btnOpen.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnOpen.Name = "btnOpen";
            this.btnOpen.Size = new System.Drawing.Size(40, 40);
            this.btnOpen.ToolTipText = "Open template";
            // 
            // btnSave
            // 
            this.btnSave.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnSave.Image = ((System.Drawing.Image)(resources.GetObject("btnSave.Image")));
            this.btnSave.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(40, 40);
            this.btnSave.ToolTipText = "Save template";
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnSaveAs
            // 
            this.btnSaveAs.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnSaveAs.Image = ((System.Drawing.Image)(resources.GetObject("btnSaveAs.Image")));
            this.btnSaveAs.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnSaveAs.Name = "btnSaveAs";
            this.btnSaveAs.Size = new System.Drawing.Size(40, 40);
            this.btnSaveAs.ToolTipText = "Save template as";
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 43);
            // 
            // btnAddElemGroup
            // 
            this.btnAddElemGroup.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnAddElemGroup.Image = ((System.Drawing.Image)(resources.GetObject("btnAddElemGroup.Image")));
            this.btnAddElemGroup.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnAddElemGroup.Name = "btnAddElemGroup";
            this.btnAddElemGroup.Size = new System.Drawing.Size(40, 40);
            this.btnAddElemGroup.ToolTipText = "Add element group";
            // 
            // btnAddElem
            // 
            this.btnAddElem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnAddElem.Image = ((System.Drawing.Image)(resources.GetObject("btnAddElem.Image")));
            this.btnAddElem.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnAddElem.Name = "btnAddElem";
            this.btnAddElem.Size = new System.Drawing.Size(40, 40);
            this.btnAddElem.ToolTipText = "Add element";
            // 
            // btnAddCmd
            // 
            this.btnAddCmd.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnAddCmd.Image = ((System.Drawing.Image)(resources.GetObject("btnAddCmd.Image")));
            this.btnAddCmd.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnAddCmd.Name = "btnAddCmd";
            this.btnAddCmd.Size = new System.Drawing.Size(40, 40);
            this.btnAddCmd.ToolTipText = "Add command";
            // 
            // btnMoveUp
            // 
            this.btnMoveUp.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnMoveUp.Image = ((System.Drawing.Image)(resources.GetObject("btnMoveUp.Image")));
            this.btnMoveUp.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnMoveUp.Name = "btnMoveUp";
            this.btnMoveUp.Size = new System.Drawing.Size(40, 40);
            this.btnMoveUp.ToolTipText = "Move up";
            // 
            // btnMoveDown
            // 
            this.btnMoveDown.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnMoveDown.Image = ((System.Drawing.Image)(resources.GetObject("btnMoveDown.Image")));
            this.btnMoveDown.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnMoveDown.Name = "btnMoveDown";
            this.btnMoveDown.Size = new System.Drawing.Size(40, 40);
            this.btnMoveDown.ToolTipText = "Move down";
            // 
            // btnDelete
            // 
            this.btnDelete.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnDelete.Image = ((System.Drawing.Image)(resources.GetObject("btnDelete.Image")));
            this.btnDelete.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(40, 40);
            this.btnDelete.ToolTipText = "Delete";
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 43);
            // 
            // btnEditSettings
            // 
            this.btnEditSettings.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnEditSettings.Image = ((System.Drawing.Image)(resources.GetObject("btnEditSettings.Image")));
            this.btnEditSettings.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnEditSettings.Name = "btnEditSettings";
            this.btnEditSettings.Size = new System.Drawing.Size(40, 40);
            this.btnEditSettings.ToolTipText = "Edit template settings";
            this.btnEditSettings.Click += new System.EventHandler(this.btnEditSettings_Click);
            // 
            // btnEditSettingsExt
            // 
            this.btnEditSettingsExt.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnEditSettingsExt.Image = ((System.Drawing.Image)(resources.GetObject("btnEditSettingsExt.Image")));
            this.btnEditSettingsExt.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnEditSettingsExt.Name = "btnEditSettingsExt";
            this.btnEditSettingsExt.Size = new System.Drawing.Size(40, 40);
            this.btnEditSettingsExt.ToolTipText = "Edit extended settings";
            this.btnEditSettingsExt.Click += new System.EventHandler(this.btnEditSettingsExt_Click);
            // 
            // openFileDialog
            // 
            this.openFileDialog.DefaultExt = "*.xml";
            this.openFileDialog.Filter = "Template Files (*.xml)|*.xml|All Files (*.*)|*.*";
            this.openFileDialog.FilterIndex = 0;
            // 
            // saveFileDialog
            // 
            this.saveFileDialog.DefaultExt = "*.xml";
            this.saveFileDialog.Filter = "Template Files (*.xml)|*.xml|All Files (*.*)|*.*";
            this.saveFileDialog.FilterIndex = 0;
            // 
            // cbxRegisterType
            // 
            this.cbxRegisterType.FormattingEnabled = true;
            this.cbxRegisterType.Location = new System.Drawing.Point(33, 71);
            this.cbxRegisterType.Name = "cbxRegisterType";
            this.cbxRegisterType.Size = new System.Drawing.Size(162, 20);
            this.cbxRegisterType.TabIndex = 1;
            this.cbxRegisterType.SelectedIndexChanged += new System.EventHandler(this.cbxRegisterType_SelectedIndexChanged);
            // 
            // numTagCount
            // 
            this.numTagCount.Location = new System.Drawing.Point(33, 114);
            this.numTagCount.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            this.numTagCount.Name = "numTagCount";
            this.numTagCount.Size = new System.Drawing.Size(85, 21);
            this.numTagCount.TabIndex = 2;
            this.numTagCount.ValueChanged += new System.EventHandler(this.numTagCount_ValueChanged);
            // 
            // lblRegisterType
            // 
            this.lblRegisterType.AutoSize = true;
            this.lblRegisterType.Location = new System.Drawing.Point(33, 56);
            this.lblRegisterType.Name = "lblRegisterType";
            this.lblRegisterType.Size = new System.Drawing.Size(65, 12);
            this.lblRegisterType.TabIndex = 3;
            this.lblRegisterType.Text = "寄存器类型";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(33, 99);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(29, 12);
            this.label1.TabIndex = 4;
            this.label1.Text = "点数";
            // 
            // dgvTagGroup
            // 
            this.dgvTagGroup.AllowUserToAddRows = false;
            this.dgvTagGroup.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvTagGroup.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvTagGroup.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.tagName,
            this.TagGroupDataType,
            this.TagAddress});
            this.dgvTagGroup.Location = new System.Drawing.Point(216, 37);
            this.dgvTagGroup.Name = "dgvTagGroup";
            this.dgvTagGroup.RowHeadersWidth = 92;
            this.dgvTagGroup.RowTemplate.Height = 23;
            this.dgvTagGroup.Size = new System.Drawing.Size(568, 154);
            this.dgvTagGroup.TabIndex = 5;
            // 
            // tagName
            // 
            this.tagName.DataPropertyName = "Name";
            this.tagName.HeaderText = "Name";
            this.tagName.MinimumWidth = 11;
            this.tagName.Name = "tagName";
            // 
            // TagGroupDataType
            // 
            this.TagGroupDataType.DataPropertyName = "DataType";
            this.TagGroupDataType.HeaderText = "DataType";
            this.TagGroupDataType.MinimumWidth = 11;
            this.TagGroupDataType.Name = "TagGroupDataType";
            this.TagGroupDataType.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.TagGroupDataType.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            // 
            // TagAddress
            // 
            this.TagAddress.DataPropertyName = "Address";
            this.TagAddress.HeaderText = "TagAddress";
            this.TagAddress.MinimumWidth = 11;
            this.TagAddress.Name = "TagAddress";
            this.TagAddress.ReadOnly = true;
            // 
            // txtStartAddress
            // 
            this.txtStartAddress.Location = new System.Drawing.Point(33, 158);
            this.txtStartAddress.Name = "txtStartAddress";
            this.txtStartAddress.Size = new System.Drawing.Size(162, 21);
            this.txtStartAddress.TabIndex = 6;
            this.txtStartAddress.TextChanged += new System.EventHandler(this.txtStartAddress_TextChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(31, 143);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 12);
            this.label2.TabIndex = 7;
            this.label2.Text = "起始地址";
            // 
            // txtCmdAddr
            // 
            this.txtCmdAddr.Location = new System.Drawing.Point(6, 40);
            this.txtCmdAddr.Name = "txtCmdAddr";
            this.txtCmdAddr.Size = new System.Drawing.Size(156, 21);
            this.txtCmdAddr.TabIndex = 8;
            this.txtCmdAddr.TextChanged += new System.EventHandler(this.txtCmdAddr_TextChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.btnAddModbusCmd);
            this.groupBox1.Controls.Add(this.dgvCmd);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.cbCmdElemType);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.cbCmdRegType);
            this.groupBox1.Controls.Add(this.txtCmdAddr);
            this.groupBox1.Location = new System.Drawing.Point(33, 228);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(763, 229);
            this.groupBox1.TabIndex = 9;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "指令";
            // 
            // btnAddModbusCmd
            // 
            this.btnAddModbusCmd.Location = new System.Drawing.Point(8, 168);
            this.btnAddModbusCmd.Name = "btnAddModbusCmd";
            this.btnAddModbusCmd.Size = new System.Drawing.Size(86, 36);
            this.btnAddModbusCmd.TabIndex = 10;
            this.btnAddModbusCmd.Text = "添加指令";
            this.btnAddModbusCmd.UseVisualStyleBackColor = true;
            this.btnAddModbusCmd.Click += new System.EventHandler(this.btnAddModbusCmd_Click);
            // 
            // dgvCmd
            // 
            this.dgvCmd.AllowUserToAddRows = false;
            this.dgvCmd.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvCmd.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvCmd.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.cmdName,
            this.Address,
            this.CmdModbusRegisterType,
            this.CmdFunctionCode,
            this.CmdNum});
            this.dgvCmd.Location = new System.Drawing.Point(183, 40);
            this.dgvCmd.Name = "dgvCmd";
            this.dgvCmd.RowHeadersWidth = 92;
            this.dgvCmd.RowTemplate.Height = 23;
            this.dgvCmd.Size = new System.Drawing.Size(568, 183);
            this.dgvCmd.TabIndex = 14;
            // 
            // cmdName
            // 
            this.cmdName.DataPropertyName = "Name";
            this.cmdName.HeaderText = "Name";
            this.cmdName.MinimumWidth = 11;
            this.cmdName.Name = "cmdName";
            // 
            // Address
            // 
            this.Address.DataPropertyName = "Address";
            this.Address.HeaderText = "Address";
            this.Address.MinimumWidth = 11;
            this.Address.Name = "Address";
            // 
            // CmdModbusRegisterType
            // 
            this.CmdModbusRegisterType.DataPropertyName = "ModbusRegisterType";
            this.CmdModbusRegisterType.HeaderText = "ModbusRegisterType";
            this.CmdModbusRegisterType.MinimumWidth = 11;
            this.CmdModbusRegisterType.Name = "CmdModbusRegisterType";
            this.CmdModbusRegisterType.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.CmdModbusRegisterType.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            // 
            // CmdFunctionCode
            // 
            this.CmdFunctionCode.DataPropertyName = "FuncCode";
            this.CmdFunctionCode.HeaderText = "FunctionCode";
            this.CmdFunctionCode.MinimumWidth = 11;
            this.CmdFunctionCode.Name = "CmdFunctionCode";
            this.CmdFunctionCode.ReadOnly = true;
            // 
            // CmdNum
            // 
            this.CmdNum.DataPropertyName = "CmdNum";
            this.CmdNum.HeaderText = "CmdNum";
            this.CmdNum.MinimumWidth = 11;
            this.CmdNum.Name = "CmdNum";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(6, 116);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(53, 12);
            this.label5.TabIndex = 13;
            this.label5.Text = "数据类型";
            // 
            // cbCmdElemType
            // 
            this.cbCmdElemType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbCmdElemType.FormattingEnabled = true;
            this.cbCmdElemType.Items.AddRange(new object[] {
            "Undefined",
            "ushort (2 bytes)",
            "short (2 bytes)",
            "uint (4 bytes)",
            "int (4 bytes)",
            "ulong (8 bytes)",
            "long (8 bytes)",
            "float (4 bytes)",
            "double (8 bytes)",
            "bool (1 bit)"});
            this.cbCmdElemType.Location = new System.Drawing.Point(8, 131);
            this.cbCmdElemType.Name = "cbCmdElemType";
            this.cbCmdElemType.Size = new System.Drawing.Size(124, 20);
            this.cbCmdElemType.TabIndex = 12;
            this.cbCmdElemType.SelectedIndexChanged += new System.EventHandler(this.cbCmdElemType_SelectedIndexChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(8, 76);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(65, 12);
            this.label4.TabIndex = 11;
            this.label4.Text = "寄存器类型";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 25);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(29, 12);
            this.label3.TabIndex = 10;
            this.label3.Text = "地址";
            // 
            // cbCmdRegType
            // 
            this.cbCmdRegType.FormattingEnabled = true;
            this.cbCmdRegType.Items.AddRange(new object[] {
            "Coils (0X)",
            "Holding Registers (4X)"});
            this.cbCmdRegType.Location = new System.Drawing.Point(8, 91);
            this.cbCmdRegType.Name = "cbCmdRegType";
            this.cbCmdRegType.Size = new System.Drawing.Size(154, 20);
            this.cbCmdRegType.TabIndex = 10;
            this.cbCmdRegType.SelectedIndexChanged += new System.EventHandler(this.cbCmdRegType_SelectedIndexChanged);
            // 
            // FrmDevTemplate
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(811, 469);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txtStartAddress);
            this.Controls.Add(this.dgvTagGroup);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.lblRegisterType);
            this.Controls.Add(this.numTagCount);
            this.Controls.Add(this.cbxRegisterType);
            this.Controls.Add(this.toolStrip);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(592, 423);
            this.Name = "FrmDevTemplate";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "MODBUS. Device Template Editor";
            this.Load += new System.EventHandler(this.FrmDevTemplate_Load);
            this.toolStrip.ResumeLayout(false);
            this.toolStrip.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numTagCount)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvTagGroup)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvCmd)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.bdsTagGroups)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.bdsCmds)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.ToolStrip toolStrip;
        private System.Windows.Forms.ToolStripButton btnSave;
        private System.Windows.Forms.ToolStripButton btnOpen;
        private System.Windows.Forms.ToolStripButton btnNew;
        private System.Windows.Forms.ToolStripButton btnAddElemGroup;
        private System.Windows.Forms.ToolStripButton btnAddCmd;
        private System.Windows.Forms.ToolStripButton btnMoveUp;
        private System.Windows.Forms.ToolStripButton btnMoveDown;
        private System.Windows.Forms.ToolStripButton btnDelete;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ImageList imageList;
        private System.Windows.Forms.OpenFileDialog openFileDialog;
        private System.Windows.Forms.SaveFileDialog saveFileDialog;
        private System.Windows.Forms.ToolStripButton btnSaveAs;
        private System.Windows.Forms.ToolStripButton btnAddElem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripButton btnEditSettings;
        private System.Windows.Forms.ToolStripButton btnEditSettingsExt;
        private System.Windows.Forms.ComboBox cbxRegisterType;
        private System.Windows.Forms.NumericUpDown numTagCount;
        private System.Windows.Forms.Label lblRegisterType;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.DataGridView dgvTagGroup;
        private System.Windows.Forms.TextBox txtStartAddress;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtCmdAddr;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox cbCmdRegType;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ComboBox cbCmdElemType;
        private System.Windows.Forms.DataGridView dgvCmd;
        private System.Windows.Forms.BindingSource bdsTagGroups;
        private System.Windows.Forms.BindingSource bdsCmds;
        private System.Windows.Forms.Button btnAddModbusCmd;
        private System.Windows.Forms.DataGridViewTextBoxColumn tagName;
        private System.Windows.Forms.DataGridViewComboBoxColumn TagGroupDataType;
        private System.Windows.Forms.DataGridViewTextBoxColumn TagAddress;
        private System.Windows.Forms.DataGridViewTextBoxColumn cmdName;
        private System.Windows.Forms.DataGridViewTextBoxColumn Address;
        private System.Windows.Forms.DataGridViewComboBoxColumn CmdModbusRegisterType;
        private System.Windows.Forms.DataGridViewTextBoxColumn CmdFunctionCode;
        private System.Windows.Forms.DataGridViewTextBoxColumn CmdNum;
    }
}