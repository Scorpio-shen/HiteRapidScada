namespace KpMelsec.View
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
            this.txtParams = new System.Windows.Forms.TextBox();
            this.btnParamSetting = new System.Windows.Forms.Button();
            this.label7 = new System.Windows.Forms.Label();
            this.cbxConnectionType = new System.Windows.Forms.ComboBox();
            this.txtStation = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.txtSA2 = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txtDA2 = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.txtSID = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.txtSID);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.txtDA2);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.txtSA2);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.txtParams);
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
            // txtParams
            // 
            this.txtParams.BackColor = System.Drawing.SystemColors.Control;
            this.txtParams.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtParams.Location = new System.Drawing.Point(15, 86);
            this.txtParams.Multiline = true;
            this.txtParams.Name = "txtParams";
            this.txtParams.Size = new System.Drawing.Size(647, 89);
            this.txtParams.TabIndex = 22;
            // 
            // btnParamSetting
            // 
            this.btnParamSetting.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(225)))), ((int)(((byte)(225)))), ((int)(((byte)(225)))));
            this.btnParamSetting.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(173)))), ((int)(((byte)(173)))), ((int)(((byte)(173)))));
            this.btnParamSetting.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnParamSetting.Location = new System.Drawing.Point(548, 45);
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
            this.label7.Location = new System.Drawing.Point(364, 33);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(29, 12);
            this.label7.TabIndex = 18;
            this.label7.Text = "连接";
            // 
            // cbxConnectionType
            // 
            this.cbxConnectionType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbxConnectionType.FormattingEnabled = true;
            this.cbxConnectionType.Location = new System.Drawing.Point(399, 29);
            this.cbxConnectionType.Name = "cbxConnectionType";
            this.cbxConnectionType.Size = new System.Drawing.Size(126, 20);
            this.cbxConnectionType.TabIndex = 17;
            this.cbxConnectionType.SelectedIndexChanged += new System.EventHandler(this.cbxConnectionType_SelectedIndexChanged);
            // 
            // txtStation
            // 
            this.txtStation.Location = new System.Drawing.Point(98, 29);
            this.txtStation.Name = "txtStation";
            this.txtStation.Size = new System.Drawing.Size(76, 21);
            this.txtStation.TabIndex = 5;
            this.txtStation.TextChanged += new System.EventHandler(this.txtStation_TextChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(51, 33);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(41, 12);
            this.label1.TabIndex = 4;
            this.label1.Text = "站地址";
            // 
            // txtSA2
            // 
            this.txtSA2.Location = new System.Drawing.Point(273, 59);
            this.txtSA2.Name = "txtSA2";
            this.txtSA2.Size = new System.Drawing.Size(76, 21);
            this.txtSA2.TabIndex = 24;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(190, 63);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(77, 12);
            this.label2.TabIndex = 23;
            this.label2.Text = "上位机单元号";
            // 
            // txtDA2
            // 
            this.txtDA2.Location = new System.Drawing.Point(98, 59);
            this.txtDA2.Name = "txtDA2";
            this.txtDA2.Size = new System.Drawing.Size(76, 21);
            this.txtDA2.TabIndex = 26;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(33, 63);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(59, 12);
            this.label4.TabIndex = 25;
            this.label4.Text = "PLC单元号";
            // 
            // txtSID
            // 
            this.txtSID.Location = new System.Drawing.Point(273, 29);
            this.txtSID.Name = "txtSID";
            this.txtSID.Size = new System.Drawing.Size(76, 21);
            this.txtSID.TabIndex = 30;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(202, 33);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(65, 12);
            this.label5.TabIndex = 29;
            this.label5.Text = "设备标识号";
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
        private System.Windows.Forms.TextBox txtParams;
        private System.Windows.Forms.TextBox txtSID;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txtDA2;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtSA2;
        private System.Windows.Forms.Label label2;
    }
}
