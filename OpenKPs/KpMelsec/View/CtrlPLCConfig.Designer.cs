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
            this.label1 = new System.Windows.Forms.Label();
            this.cbxProtocolType = new System.Windows.Forms.ComboBox();
            this.txtParams = new System.Windows.Forms.TextBox();
            this.btnParamSetting = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.cbxProtocolType);
            this.groupBox1.Controls.Add(this.txtParams);
            this.groupBox1.Controls.Add(this.btnParamSetting);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Location = new System.Drawing.Point(0, 0);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(665, 181);
            this.groupBox1.TabIndex = 13;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "PLC参数配置";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(155, 52);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(53, 12);
            this.label1.TabIndex = 28;
            this.label1.Text = "协议类型";
            // 
            // cbxProtocolType
            // 
            this.cbxProtocolType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbxProtocolType.FormattingEnabled = true;
            this.cbxProtocolType.Location = new System.Drawing.Point(214, 48);
            this.cbxProtocolType.Name = "cbxProtocolType";
            this.cbxProtocolType.Size = new System.Drawing.Size(143, 20);
            this.cbxProtocolType.TabIndex = 27;
            // 
            // txtParams
            // 
            this.txtParams.BackColor = System.Drawing.SystemColors.Control;
            this.txtParams.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtParams.Location = new System.Drawing.Point(6, 86);
            this.txtParams.Multiline = true;
            this.txtParams.Name = "txtParams";
            this.txtParams.Size = new System.Drawing.Size(656, 89);
            this.txtParams.TabIndex = 26;
            // 
            // btnParamSetting
            // 
            this.btnParamSetting.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(225)))), ((int)(((byte)(225)))), ((int)(((byte)(225)))));
            this.btnParamSetting.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(173)))), ((int)(((byte)(173)))), ((int)(((byte)(173)))));
            this.btnParamSetting.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnParamSetting.Location = new System.Drawing.Point(374, 43);
            this.btnParamSetting.Name = "btnParamSetting";
            this.btnParamSetting.Size = new System.Drawing.Size(111, 30);
            this.btnParamSetting.TabIndex = 25;
            this.btnParamSetting.Text = "参数设置";
            this.btnParamSetting.UseVisualStyleBackColor = false;
            this.btnParamSetting.Click += new System.EventHandler(this.btnParamSetting_Click);
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
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cbxProtocolType;
        private System.Windows.Forms.TextBox txtParams;
        private System.Windows.Forms.Button btnParamSetting;
    }
}
