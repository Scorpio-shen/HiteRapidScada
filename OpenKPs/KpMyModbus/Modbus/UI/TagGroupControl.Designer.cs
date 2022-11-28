namespace KpMyModbus.Modbus.UI
{
    partial class TagGroupControl
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
            this.txtGrName = new System.Windows.Forms.TextBox();
            this.lblGrName = new System.Windows.Forms.Label();
            this.lblGrTableType = new System.Windows.Forms.Label();
            this.cbGrRegisterType = new System.Windows.Forms.ComboBox();
            this.txtGrFuncCode = new System.Windows.Forms.TextBox();
            this.lblGrFuncCode = new System.Windows.Forms.Label();
            this.lblGrElemCnt = new System.Windows.Forms.Label();
            this.numGrTagCnt = new System.Windows.Forms.NumericUpDown();
            this.numGrAddress = new System.Windows.Forms.NumericUpDown();
            this.lblGrAddress = new System.Windows.Forms.Label();
            this.gbTagGroup.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numGrTagCnt)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numGrAddress)).BeginInit();
            this.SuspendLayout();
            // 
            // gbTagGroup
            // 
            this.gbTagGroup.Controls.Add(this.txtGrFuncCode);
            this.gbTagGroup.Controls.Add(this.lblGrFuncCode);
            this.gbTagGroup.Controls.Add(this.lblGrElemCnt);
            this.gbTagGroup.Controls.Add(this.numGrTagCnt);
            this.gbTagGroup.Controls.Add(this.numGrAddress);
            this.gbTagGroup.Controls.Add(this.lblGrAddress);
            this.gbTagGroup.Controls.Add(this.lblGrTableType);
            this.gbTagGroup.Controls.Add(this.cbGrRegisterType);
            this.gbTagGroup.Controls.Add(this.txtGrName);
            this.gbTagGroup.Controls.Add(this.lblGrName);
            this.gbTagGroup.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gbTagGroup.Location = new System.Drawing.Point(0, 0);
            this.gbTagGroup.Name = "gbTagGroup";
            this.gbTagGroup.Size = new System.Drawing.Size(642, 488);
            this.gbTagGroup.TabIndex = 0;
            this.gbTagGroup.TabStop = false;
            this.gbTagGroup.Text = "TagGroup Parameter";
            // 
            // txtGrName
            // 
            this.txtGrName.Location = new System.Drawing.Point(31, 85);
            this.txtGrName.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
            this.txtGrName.Name = "txtGrName";
            this.txtGrName.Size = new System.Drawing.Size(587, 38);
            this.txtGrName.TabIndex = 4;
            // 
            // lblGrName
            // 
            this.lblGrName.AutoSize = true;
            this.lblGrName.Location = new System.Drawing.Point(31, 48);
            this.lblGrName.Margin = new System.Windows.Forms.Padding(7, 0, 7, 0);
            this.lblGrName.Name = "lblGrName";
            this.lblGrName.Size = new System.Drawing.Size(68, 27);
            this.lblGrName.TabIndex = 3;
            this.lblGrName.Text = "Name";
            // 
            // lblGrTableType
            // 
            this.lblGrTableType.AutoSize = true;
            this.lblGrTableType.Location = new System.Drawing.Point(31, 133);
            this.lblGrTableType.Margin = new System.Windows.Forms.Padding(7, 0, 7, 0);
            this.lblGrTableType.Name = "lblGrTableType";
            this.lblGrTableType.Size = new System.Drawing.Size(264, 27);
            this.lblGrTableType.TabIndex = 5;
            this.lblGrTableType.Text = "ModbusRegisterType";
            // 
            // cbGrRegisterType
            // 
            this.cbGrRegisterType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbGrRegisterType.FormattingEnabled = true;
            this.cbGrRegisterType.Items.AddRange(new object[] {
            "Discretes Inputs (1X)",
            "Coils (0X)",
            "Input Registers (3X)",
            "Holding Registers (4X)"});
            this.cbGrRegisterType.Location = new System.Drawing.Point(31, 170);
            this.cbGrRegisterType.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
            this.cbGrRegisterType.Name = "cbGrRegisterType";
            this.cbGrRegisterType.Size = new System.Drawing.Size(587, 35);
            this.cbGrRegisterType.TabIndex = 6;
            // 
            // txtGrFuncCode
            // 
            this.txtGrFuncCode.Location = new System.Drawing.Point(31, 252);
            this.txtGrFuncCode.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
            this.txtGrFuncCode.Name = "txtGrFuncCode";
            this.txtGrFuncCode.ReadOnly = true;
            this.txtGrFuncCode.Size = new System.Drawing.Size(284, 38);
            this.txtGrFuncCode.TabIndex = 13;
            // 
            // lblGrFuncCode
            // 
            this.lblGrFuncCode.AutoSize = true;
            this.lblGrFuncCode.Location = new System.Drawing.Point(31, 215);
            this.lblGrFuncCode.Margin = new System.Windows.Forms.Padding(7, 0, 7, 0);
            this.lblGrFuncCode.Name = "lblGrFuncCode";
            this.lblGrFuncCode.Size = new System.Drawing.Size(194, 27);
            this.lblGrFuncCode.TabIndex = 12;
            this.lblGrFuncCode.Text = "Function code";
            // 
            // lblGrElemCnt
            // 
            this.lblGrElemCnt.AutoSize = true;
            this.lblGrElemCnt.Location = new System.Drawing.Point(31, 385);
            this.lblGrElemCnt.Margin = new System.Windows.Forms.Padding(7, 0, 7, 0);
            this.lblGrElemCnt.Name = "lblGrElemCnt";
            this.lblGrElemCnt.Size = new System.Drawing.Size(138, 27);
            this.lblGrElemCnt.TabIndex = 16;
            this.lblGrElemCnt.Text = "Tag count";
            // 
            // numGrTagCnt
            // 
            this.numGrTagCnt.Location = new System.Drawing.Point(31, 422);
            this.numGrTagCnt.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
            this.numGrTagCnt.Maximum = new decimal(new int[] {
            2000,
            0,
            0,
            0});
            this.numGrTagCnt.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numGrTagCnt.Name = "numGrTagCnt";
            this.numGrTagCnt.Size = new System.Drawing.Size(289, 38);
            this.numGrTagCnt.TabIndex = 17;
            this.numGrTagCnt.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // numGrAddress
            // 
            this.numGrAddress.Location = new System.Drawing.Point(31, 337);
            this.numGrAddress.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
            this.numGrAddress.Maximum = new decimal(new int[] {
            65536,
            0,
            0,
            0});
            this.numGrAddress.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numGrAddress.Name = "numGrAddress";
            this.numGrAddress.Size = new System.Drawing.Size(289, 38);
            this.numGrAddress.TabIndex = 15;
            this.numGrAddress.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // lblGrAddress
            // 
            this.lblGrAddress.AutoSize = true;
            this.lblGrAddress.Location = new System.Drawing.Point(31, 300);
            this.lblGrAddress.Margin = new System.Windows.Forms.Padding(7, 0, 7, 0);
            this.lblGrAddress.Name = "lblGrAddress";
            this.lblGrAddress.Size = new System.Drawing.Size(250, 27);
            this.lblGrAddress.TabIndex = 14;
            this.lblGrAddress.Text = "Start tag address";
            // 
            // TagGroupControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(14F, 27F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.gbTagGroup);
            this.Name = "TagGroupControl";
            this.Size = new System.Drawing.Size(642, 488);
            this.gbTagGroup.ResumeLayout(false);
            this.gbTagGroup.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numGrTagCnt)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numGrAddress)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox gbTagGroup;
        private System.Windows.Forms.TextBox txtGrFuncCode;
        private System.Windows.Forms.Label lblGrFuncCode;
        private System.Windows.Forms.Label lblGrElemCnt;
        private System.Windows.Forms.NumericUpDown numGrTagCnt;
        private System.Windows.Forms.NumericUpDown numGrAddress;
        private System.Windows.Forms.Label lblGrAddress;
        private System.Windows.Forms.Label lblGrTableType;
        private System.Windows.Forms.ComboBox cbGrRegisterType;
        private System.Windows.Forms.TextBox txtGrName;
        private System.Windows.Forms.Label lblGrName;
    }
}
