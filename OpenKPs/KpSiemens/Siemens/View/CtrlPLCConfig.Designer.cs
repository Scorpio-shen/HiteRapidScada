namespace KpSiemens.Siemens.View
{
    partial class CtrlPLCConfig
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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label7 = new System.Windows.Forms.Label();
            this.cbxPLCType = new System.Windows.Forms.ComboBox();
            this.txtIPAddress = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.btnAdvanceSetting = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.btnAdvanceSetting);
            this.groupBox1.Controls.Add(this.label7);
            this.groupBox1.Controls.Add(this.cbxPLCType);
            this.groupBox1.Controls.Add(this.txtIPAddress);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Location = new System.Drawing.Point(0, 0);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(665, 123);
            this.groupBox1.TabIndex = 13;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "PLC参数配置";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(261, 60);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(47, 12);
            this.label7.TabIndex = 18;
            this.label7.Text = "PLC型号";
            // 
            // cbxPLCType
            // 
            this.cbxPLCType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbxPLCType.FormattingEnabled = true;
            this.cbxPLCType.Location = new System.Drawing.Point(314, 57);
            this.cbxPLCType.Name = "cbxPLCType";
            this.cbxPLCType.Size = new System.Drawing.Size(121, 20);
            this.cbxPLCType.TabIndex = 17;
            this.cbxPLCType.SelectedIndexChanged += new System.EventHandler(this.cbxPLCType_SelectedIndexChanged);
            // 
            // txtIPAddress
            // 
            this.txtIPAddress.Location = new System.Drawing.Point(116, 56);
            this.txtIPAddress.Name = "txtIPAddress";
            this.txtIPAddress.Size = new System.Drawing.Size(121, 21);
            this.txtIPAddress.TabIndex = 5;
            this.txtIPAddress.TextChanged += new System.EventHandler(this.txtIPAddress_TextChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(71, 60);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(41, 12);
            this.label1.TabIndex = 4;
            this.label1.Text = "IP地址";
            // 
            // btnAdvanceSetting
            // 
            this.btnAdvanceSetting.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(225)))), ((int)(((byte)(225)))), ((int)(((byte)(225)))));
            this.btnAdvanceSetting.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(173)))), ((int)(((byte)(173)))), ((int)(((byte)(173)))));
            this.btnAdvanceSetting.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAdvanceSetting.Location = new System.Drawing.Point(489, 42);
            this.btnAdvanceSetting.Name = "btnAdvanceSetting";
            this.btnAdvanceSetting.Size = new System.Drawing.Size(90, 48);
            this.btnAdvanceSetting.TabIndex = 19;
            this.btnAdvanceSetting.Text = "高级参数设置";
            this.btnAdvanceSetting.UseVisualStyleBackColor = false;
            this.btnAdvanceSetting.Click += new System.EventHandler(this.btnAdvanceSetting_Click);
            // 
            // CtrlPLCConfig
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.groupBox1);
            this.Name = "CtrlPLCConfig";
            this.Size = new System.Drawing.Size(665, 123);
            this.Load += new System.EventHandler(this.CtrlPLCConfig_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.ComboBox cbxPLCType;
        private System.Windows.Forms.TextBox txtIPAddress;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnAdvanceSetting;
    }
}
