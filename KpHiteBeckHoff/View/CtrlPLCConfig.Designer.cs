namespace KpHiteBeckHoff.View
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
            this.txtIPAddress = new System.Windows.Forms.TextBox();
            this.txtPort = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txtAmsPort = new System.Windows.Forms.TextBox();
            this.txtSenderNetId = new System.Windows.Forms.TextBox();
            this.label15 = new System.Windows.Forms.Label();
            this.txtTargetNetId = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.chkAutoAmsNetId = new System.Windows.Forms.CheckBox();
            this.label6 = new System.Windows.Forms.Label();
            this.chkTagCache = new System.Windows.Forms.CheckBox();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.txtAmsPort);
            this.groupBox1.Controls.Add(this.txtSenderNetId);
            this.groupBox1.Controls.Add(this.label15);
            this.groupBox1.Controls.Add(this.txtTargetNetId);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.chkAutoAmsNetId);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.chkTagCache);
            this.groupBox1.Controls.Add(this.txtPort);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.txtIPAddress);
            this.groupBox1.Controls.Add(this.label1);
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
            this.label1.Location = new System.Drawing.Point(62, 38);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(47, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "IP地址:";
            // 
            // txtIPAddress
            // 
            this.txtIPAddress.Location = new System.Drawing.Point(115, 34);
            this.txtIPAddress.Name = "txtIPAddress";
            this.txtIPAddress.Size = new System.Drawing.Size(179, 21);
            this.txtIPAddress.TabIndex = 1;
            // 
            // txtPort
            // 
            this.txtPort.Location = new System.Drawing.Point(407, 34);
            this.txtPort.Name = "txtPort";
            this.txtPort.Size = new System.Drawing.Size(73, 21);
            this.txtPort.TabIndex = 3;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(372, 38);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(29, 12);
            this.label2.TabIndex = 2;
            this.label2.Text = "端口";
            // 
            // txtAmsPort
            // 
            this.txtAmsPort.Location = new System.Drawing.Point(222, 107);
            this.txtAmsPort.Name = "txtAmsPort";
            this.txtAmsPort.Size = new System.Drawing.Size(72, 21);
            this.txtAmsPort.TabIndex = 27;
            this.txtAmsPort.Text = "851";
            // 
            // txtSenderNetId
            // 
            this.txtSenderNetId.Location = new System.Drawing.Point(407, 69);
            this.txtSenderNetId.Name = "txtSenderNetId";
            this.txtSenderNetId.Size = new System.Drawing.Size(179, 21);
            this.txtSenderNetId.TabIndex = 24;
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(318, 73);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(83, 12);
            this.label15.TabIndex = 23;
            this.label15.Text = "Sender NetId:";
            // 
            // txtTargetNetId
            // 
            this.txtTargetNetId.Location = new System.Drawing.Point(115, 69);
            this.txtTargetNetId.Name = "txtTargetNetId";
            this.txtTargetNetId.Size = new System.Drawing.Size(179, 21);
            this.txtTargetNetId.TabIndex = 22;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(26, 73);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(83, 12);
            this.label5.TabIndex = 21;
            this.label5.Text = "Target NetId:";
            // 
            // chkAutoAmsNetId
            // 
            this.chkAutoAmsNetId.AutoSize = true;
            this.chkAutoAmsNetId.Location = new System.Drawing.Point(19, 110);
            this.chkAutoAmsNetId.Name = "chkAutoAmsNetId";
            this.chkAutoAmsNetId.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.chkAutoAmsNetId.Size = new System.Drawing.Size(108, 16);
            this.chkAutoAmsNetId.TabIndex = 26;
            this.chkAutoAmsNetId.Text = ":自动AMS NetId";
            this.chkAutoAmsNetId.UseVisualStyleBackColor = true;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.ForeColor = System.Drawing.SystemColors.ControlText;
            this.label6.Location = new System.Drawing.Point(157, 111);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(59, 12);
            this.label6.TabIndex = 28;
            this.label6.Text = "Ams Port:";
            // 
            // chkTagCache
            // 
            this.chkTagCache.AutoSize = true;
            this.chkTagCache.Checked = true;
            this.chkTagCache.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkTagCache.Location = new System.Drawing.Point(329, 107);
            this.chkTagCache.Name = "chkTagCache";
            this.chkTagCache.Size = new System.Drawing.Size(72, 16);
            this.chkTagCache.TabIndex = 25;
            this.chkTagCache.Text = "标签缓存";
            this.chkTagCache.UseVisualStyleBackColor = true;
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
        private System.Windows.Forms.TextBox txtAmsPort;
        private System.Windows.Forms.TextBox txtSenderNetId;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.TextBox txtTargetNetId;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.CheckBox chkAutoAmsNetId;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.CheckBox chkTagCache;
        private System.Windows.Forms.TextBox txtPort;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtIPAddress;
        private System.Windows.Forms.Label label1;
    }
}
