namespace KpHiteModbus.Modbus.View
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
            this.cbxMode = new System.Windows.Forms.ComboBox();
            this.txtParams = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.btnParamSetting = new System.Windows.Forms.Button();
            this.label7 = new System.Windows.Forms.Label();
            this.cbxConnectionType = new System.Windows.Forms.ComboBox();
            this.txtStation = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.cbxMode);
            this.groupBox1.Controls.Add(this.txtParams);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.btnParamSetting);
            this.groupBox1.Controls.Add(this.label7);
            this.groupBox1.Controls.Add(this.cbxConnectionType);
            this.groupBox1.Controls.Add(this.txtStation);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Location = new System.Drawing.Point(0, 0);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(665, 181);
            this.groupBox1.TabIndex = 13;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "PLC参数配置";
            // 
            // cbxMode
            // 
            this.cbxMode.FormattingEnabled = true;
            this.cbxMode.Location = new System.Drawing.Point(401, 30);
            this.cbxMode.Name = "cbxMode";
            this.cbxMode.Size = new System.Drawing.Size(126, 20);
            this.cbxMode.TabIndex = 23;
            this.cbxMode.SelectedIndexChanged += new System.EventHandler(this.cbxMode_SelectedIndexChanged);
            // 
            // txtParams
            // 
            this.txtParams.BackColor = System.Drawing.SystemColors.Control;
            this.txtParams.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtParams.Location = new System.Drawing.Point(15, 60);
            this.txtParams.Multiline = true;
            this.txtParams.Name = "txtParams";
            this.txtParams.Size = new System.Drawing.Size(647, 115);
            this.txtParams.TabIndex = 22;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(366, 33);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(29, 12);
            this.label2.TabIndex = 21;
            this.label2.Text = "模式";
            // 
            // btnParamSetting
            // 
            this.btnParamSetting.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(225)))), ((int)(((byte)(225)))), ((int)(((byte)(225)))));
            this.btnParamSetting.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(173)))), ((int)(((byte)(173)))), ((int)(((byte)(173)))));
            this.btnParamSetting.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnParamSetting.Location = new System.Drawing.Point(551, 24);
            this.btnParamSetting.Name = "btnParamSetting";
            this.btnParamSetting.Size = new System.Drawing.Size(111, 30);
            this.btnParamSetting.TabIndex = 19;
            this.btnParamSetting.Text = "参数设置";
            this.btnParamSetting.UseVisualStyleBackColor = false;
            this.btnParamSetting.Click += new System.EventHandler(this.btnParamSetting_Click);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(175, 33);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(29, 12);
            this.label7.TabIndex = 18;
            this.label7.Text = "连接";
            // 
            // cbxConnectionType
            // 
            this.cbxConnectionType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbxConnectionType.FormattingEnabled = true;
            this.cbxConnectionType.Location = new System.Drawing.Point(210, 30);
            this.cbxConnectionType.Name = "cbxConnectionType";
            this.cbxConnectionType.Size = new System.Drawing.Size(126, 20);
            this.cbxConnectionType.TabIndex = 17;
            this.cbxConnectionType.SelectedIndexChanged += new System.EventHandler(this.cbxConnectionType_SelectedIndexChanged);
            // 
            // txtStation
            // 
            this.txtStation.Location = new System.Drawing.Point(69, 29);
            this.txtStation.Name = "txtStation";
            this.txtStation.Size = new System.Drawing.Size(76, 21);
            this.txtStation.TabIndex = 5;
            this.txtStation.TextChanged += new System.EventHandler(this.txtStation_TextChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(22, 33);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(41, 12);
            this.label1.TabIndex = 4;
            this.label1.Text = "站地址";
            // 
            // CtrlPLCConfig
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.groupBox1);
            this.Name = "CtrlPLCConfig";
            this.Size = new System.Drawing.Size(665, 181);
            this.Load += new System.EventHandler(this.CtrlPLCConfig_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.ComboBox cbxConnectionType;
        private System.Windows.Forms.TextBox txtStation;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnParamSetting;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtParams;
        private System.Windows.Forms.ComboBox cbxMode;
    }
}
