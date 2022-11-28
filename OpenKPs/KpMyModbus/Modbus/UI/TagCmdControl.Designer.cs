namespace KpMyModbus.Modbus.UI
{
    partial class TagCmdControl
    {
        /// <summary> 
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region 组件设计器生成的代码

        /// <summary> 
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.gbTagGroup = new System.Windows.Forms.GroupBox();
            this.lblCmdNum = new System.Windows.Forms.Label();
            this.numCmdNum = new System.Windows.Forms.NumericUpDown();
            this.lblCmdElemType = new System.Windows.Forms.Label();
            this.cbCmdTagType = new System.Windows.Forms.ComboBox();
            this.lblCmdElemCnt = new System.Windows.Forms.Label();
            this.numCmdTagCnt = new System.Windows.Forms.NumericUpDown();
            this.txtCmdFuncCode = new System.Windows.Forms.TextBox();
            this.lblGrFuncCode = new System.Windows.Forms.Label();
            this.numCmdAddress = new System.Windows.Forms.NumericUpDown();
            this.lblGrAddress = new System.Windows.Forms.Label();
            this.lblGrTableType = new System.Windows.Forms.Label();
            this.cbGrRegisterType = new System.Windows.Forms.ComboBox();
            this.txtCmdName = new System.Windows.Forms.TextBox();
            this.lblGrName = new System.Windows.Forms.Label();
            this.gbTagGroup.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numCmdNum)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numCmdTagCnt)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numCmdAddress)).BeginInit();
            this.SuspendLayout();
            // 
            // gbTagGroup
            // 
            this.gbTagGroup.Controls.Add(this.lblCmdNum);
            this.gbTagGroup.Controls.Add(this.numCmdNum);
            this.gbTagGroup.Controls.Add(this.lblCmdElemType);
            this.gbTagGroup.Controls.Add(this.cbCmdTagType);
            this.gbTagGroup.Controls.Add(this.lblCmdElemCnt);
            this.gbTagGroup.Controls.Add(this.numCmdTagCnt);
            this.gbTagGroup.Controls.Add(this.txtCmdFuncCode);
            this.gbTagGroup.Controls.Add(this.lblGrFuncCode);
            this.gbTagGroup.Controls.Add(this.numCmdAddress);
            this.gbTagGroup.Controls.Add(this.lblGrAddress);
            this.gbTagGroup.Controls.Add(this.lblGrTableType);
            this.gbTagGroup.Controls.Add(this.cbGrRegisterType);
            this.gbTagGroup.Controls.Add(this.txtCmdName);
            this.gbTagGroup.Controls.Add(this.lblGrName);
            this.gbTagGroup.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gbTagGroup.Location = new System.Drawing.Point(0, 0);
            this.gbTagGroup.Margin = new System.Windows.Forms.Padding(1, 1, 1, 1);
            this.gbTagGroup.Name = "gbTagGroup";
            this.gbTagGroup.Padding = new System.Windows.Forms.Padding(1, 1, 1, 1);
            this.gbTagGroup.Size = new System.Drawing.Size(280, 299);
            this.gbTagGroup.TabIndex = 1;
            this.gbTagGroup.TabStop = false;
            this.gbTagGroup.Text = "Command Parameter";
            // 
            // lblCmdNum
            // 
            this.lblCmdNum.AutoSize = true;
            this.lblCmdNum.Location = new System.Drawing.Point(11, 224);
            this.lblCmdNum.Name = "lblCmdNum";
            this.lblCmdNum.Size = new System.Drawing.Size(89, 12);
            this.lblCmdNum.TabIndex = 20;
            this.lblCmdNum.Text = "Command number";
            // 
            // numCmdNum
            // 
            this.numCmdNum.Location = new System.Drawing.Point(11, 239);
            this.numCmdNum.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            this.numCmdNum.Name = "numCmdNum";
            this.numCmdNum.Size = new System.Drawing.Size(124, 21);
            this.numCmdNum.TabIndex = 21;
            this.numCmdNum.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // lblCmdElemType
            // 
            this.lblCmdElemType.AutoSize = true;
            this.lblCmdElemType.Location = new System.Drawing.Point(11, 178);
            this.lblCmdElemType.Name = "lblCmdElemType";
            this.lblCmdElemType.Size = new System.Drawing.Size(83, 12);
            this.lblCmdElemType.TabIndex = 16;
            this.lblCmdElemType.Text = "Tag Data type";
            // 
            // cbCmdTagType
            // 
            this.cbCmdTagType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbCmdTagType.FormattingEnabled = true;
            this.cbCmdTagType.Items.AddRange(new object[] {
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
            this.cbCmdTagType.Location = new System.Drawing.Point(11, 192);
            this.cbCmdTagType.Name = "cbCmdTagType";
            this.cbCmdTagType.Size = new System.Drawing.Size(124, 20);
            this.cbCmdTagType.TabIndex = 17;
            // 
            // lblCmdElemCnt
            // 
            this.lblCmdElemCnt.AutoSize = true;
            this.lblCmdElemCnt.Location = new System.Drawing.Point(144, 178);
            this.lblCmdElemCnt.Name = "lblCmdElemCnt";
            this.lblCmdElemCnt.Size = new System.Drawing.Size(59, 12);
            this.lblCmdElemCnt.TabIndex = 18;
            this.lblCmdElemCnt.Text = "Tag count";
            // 
            // numCmdTagCnt
            // 
            this.numCmdTagCnt.Location = new System.Drawing.Point(147, 192);
            this.numCmdTagCnt.Maximum = new decimal(new int[] {
            2000,
            0,
            0,
            0});
            this.numCmdTagCnt.Name = "numCmdTagCnt";
            this.numCmdTagCnt.Size = new System.Drawing.Size(124, 21);
            this.numCmdTagCnt.TabIndex = 19;
            this.numCmdTagCnt.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // txtCmdFuncCode
            // 
            this.txtCmdFuncCode.Location = new System.Drawing.Point(11, 112);
            this.txtCmdFuncCode.Name = "txtCmdFuncCode";
            this.txtCmdFuncCode.ReadOnly = true;
            this.txtCmdFuncCode.Size = new System.Drawing.Size(124, 21);
            this.txtCmdFuncCode.TabIndex = 13;
            // 
            // lblGrFuncCode
            // 
            this.lblGrFuncCode.AutoSize = true;
            this.lblGrFuncCode.Location = new System.Drawing.Point(11, 96);
            this.lblGrFuncCode.Name = "lblGrFuncCode";
            this.lblGrFuncCode.Size = new System.Drawing.Size(83, 12);
            this.lblGrFuncCode.TabIndex = 12;
            this.lblGrFuncCode.Text = "Function code";
            // 
            // numCmdAddress
            // 
            this.numCmdAddress.Location = new System.Drawing.Point(11, 150);
            this.numCmdAddress.Maximum = new decimal(new int[] {
            65536,
            0,
            0,
            0});
            this.numCmdAddress.Name = "numCmdAddress";
            this.numCmdAddress.Size = new System.Drawing.Size(124, 21);
            this.numCmdAddress.TabIndex = 15;
            this.numCmdAddress.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // lblGrAddress
            // 
            this.lblGrAddress.AutoSize = true;
            this.lblGrAddress.Location = new System.Drawing.Point(11, 133);
            this.lblGrAddress.Name = "lblGrAddress";
            this.lblGrAddress.Size = new System.Drawing.Size(71, 12);
            this.lblGrAddress.TabIndex = 14;
            this.lblGrAddress.Text = "Tag address";
            // 
            // lblGrTableType
            // 
            this.lblGrTableType.AutoSize = true;
            this.lblGrTableType.Location = new System.Drawing.Point(11, 59);
            this.lblGrTableType.Name = "lblGrTableType";
            this.lblGrTableType.Size = new System.Drawing.Size(113, 12);
            this.lblGrTableType.TabIndex = 5;
            this.lblGrTableType.Text = "ModbusRegisterType";
            // 
            // cbGrRegisterType
            // 
            this.cbGrRegisterType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbGrRegisterType.FormattingEnabled = true;
            this.cbGrRegisterType.Items.AddRange(new object[] {
            "Coils (0X)",
            "Holding Registers (4X)"});
            this.cbGrRegisterType.Location = new System.Drawing.Point(11, 76);
            this.cbGrRegisterType.Name = "cbGrRegisterType";
            this.cbGrRegisterType.Size = new System.Drawing.Size(254, 20);
            this.cbGrRegisterType.TabIndex = 6;
            // 
            // txtCmdName
            // 
            this.txtCmdName.Location = new System.Drawing.Point(11, 38);
            this.txtCmdName.Name = "txtCmdName";
            this.txtCmdName.Size = new System.Drawing.Size(254, 21);
            this.txtCmdName.TabIndex = 4;
            // 
            // lblGrName
            // 
            this.lblGrName.AutoSize = true;
            this.lblGrName.Location = new System.Drawing.Point(13, 21);
            this.lblGrName.Name = "lblGrName";
            this.lblGrName.Size = new System.Drawing.Size(29, 12);
            this.lblGrName.TabIndex = 3;
            this.lblGrName.Text = "Name";
            // 
            // TagCmdControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.gbTagGroup);
            this.Margin = new System.Windows.Forms.Padding(1, 1, 1, 1);
            this.Name = "TagCmdControl";
            this.Size = new System.Drawing.Size(280, 299);
            this.gbTagGroup.ResumeLayout(false);
            this.gbTagGroup.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numCmdNum)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numCmdTagCnt)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numCmdAddress)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox gbTagGroup;
        private System.Windows.Forms.Label lblCmdNum;
        private System.Windows.Forms.NumericUpDown numCmdNum;
        private System.Windows.Forms.Label lblCmdElemType;
        private System.Windows.Forms.ComboBox cbCmdTagType;
        private System.Windows.Forms.Label lblCmdElemCnt;
        private System.Windows.Forms.NumericUpDown numCmdTagCnt;
        private System.Windows.Forms.TextBox txtCmdFuncCode;
        private System.Windows.Forms.Label lblGrFuncCode;
        private System.Windows.Forms.NumericUpDown numCmdAddress;
        private System.Windows.Forms.Label lblGrAddress;
        private System.Windows.Forms.Label lblGrTableType;
        private System.Windows.Forms.ComboBox cbGrRegisterType;
        private System.Windows.Forms.TextBox txtCmdName;
        private System.Windows.Forms.Label lblGrName;
    }
}
