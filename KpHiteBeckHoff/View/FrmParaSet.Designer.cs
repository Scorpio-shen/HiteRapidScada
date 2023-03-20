namespace KpHiteBeckHoff.View
{
    partial class FrmParaSet
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
            this.txtIPAddress = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.txtPort = new System.Windows.Forms.TextBox();
            this.cbxSerialPortName = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.cbxBaudRate = new System.Windows.Forms.ComboBox();
            this.label5 = new System.Windows.Forms.Label();
            this.cbxDataBits = new System.Windows.Forms.ComboBox();
            this.label6 = new System.Windows.Forms.Label();
            this.cbxParity = new System.Windows.Forms.ComboBox();
            this.label7 = new System.Windows.Forms.Label();
            this.cbxStopBits = new System.Windows.Forms.ComboBox();
            this.gbxTcp = new System.Windows.Forms.GroupBox();
            this.gbxSerial = new System.Windows.Forms.GroupBox();
            this.btnConfirm = new System.Windows.Forms.Button();
            this.btnCancle = new System.Windows.Forms.Button();
            this.gbxPLCOptions = new System.Windows.Forms.GroupBox();
            this.txtSlot = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.gbxTcp.SuspendLayout();
            this.gbxSerial.SuspendLayout();
            this.gbxPLCOptions.SuspendLayout();
            this.SuspendLayout();
            // 
            // txtIPAddress
            // 
            this.txtIPAddress.Location = new System.Drawing.Point(76, 42);
            this.txtIPAddress.Name = "txtIPAddress";
            this.txtIPAddress.Size = new System.Drawing.Size(121, 21);
            this.txtIPAddress.TabIndex = 40;
            this.txtIPAddress.Text = "127.0.0.1";
            this.txtIPAddress.TextChanged += new System.EventHandler(this.ControlChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(29, 46);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(41, 12);
            this.label1.TabIndex = 39;
            this.label1.Text = "IP地址";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(223, 46);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(41, 12);
            this.label2.TabIndex = 37;
            this.label2.Text = "端口号";
            // 
            // txtPort
            // 
            this.txtPort.Location = new System.Drawing.Point(270, 42);
            this.txtPort.Name = "txtPort";
            this.txtPort.Size = new System.Drawing.Size(40, 21);
            this.txtPort.TabIndex = 38;
            this.txtPort.Text = "102";
            this.txtPort.TextChanged += new System.EventHandler(this.ControlChanged);
            // 
            // cbxSerialPortName
            // 
            this.cbxSerialPortName.FormattingEnabled = true;
            this.cbxSerialPortName.Location = new System.Drawing.Point(76, 52);
            this.cbxSerialPortName.Name = "cbxSerialPortName";
            this.cbxSerialPortName.Size = new System.Drawing.Size(121, 20);
            this.cbxSerialPortName.TabIndex = 41;
            this.cbxSerialPortName.SelectedIndexChanged += new System.EventHandler(this.ControlChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(29, 55);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(41, 12);
            this.label3.TabIndex = 42;
            this.label3.Text = "串口号";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(29, 91);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(41, 12);
            this.label4.TabIndex = 44;
            this.label4.Text = "波特率";
            // 
            // cbxBaudRate
            // 
            this.cbxBaudRate.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbxBaudRate.FormattingEnabled = true;
            this.cbxBaudRate.Items.AddRange(new object[] {
            "300",
            "600",
            "1200",
            "2400",
            "4800",
            "9600",
            "14400",
            "19200"});
            this.cbxBaudRate.Location = new System.Drawing.Point(76, 88);
            this.cbxBaudRate.Name = "cbxBaudRate";
            this.cbxBaudRate.Size = new System.Drawing.Size(121, 20);
            this.cbxBaudRate.TabIndex = 43;
            this.cbxBaudRate.SelectedIndexChanged += new System.EventHandler(this.ControlChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(29, 133);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(41, 12);
            this.label5.TabIndex = 46;
            this.label5.Text = "数据位";
            // 
            // cbxDataBits
            // 
            this.cbxDataBits.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbxDataBits.FormattingEnabled = true;
            this.cbxDataBits.Items.AddRange(new object[] {
            "8",
            "7"});
            this.cbxDataBits.Location = new System.Drawing.Point(76, 130);
            this.cbxDataBits.Name = "cbxDataBits";
            this.cbxDataBits.Size = new System.Drawing.Size(121, 20);
            this.cbxDataBits.TabIndex = 45;
            this.cbxDataBits.SelectedIndexChanged += new System.EventHandler(this.ControlChanged);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(223, 55);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(41, 12);
            this.label6.TabIndex = 48;
            this.label6.Text = "校验位";
            // 
            // cbxParity
            // 
            this.cbxParity.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbxParity.FormattingEnabled = true;
            this.cbxParity.Items.AddRange(new object[] {
            "无",
            "奇",
            "偶"});
            this.cbxParity.Location = new System.Drawing.Point(270, 52);
            this.cbxParity.Name = "cbxParity";
            this.cbxParity.Size = new System.Drawing.Size(121, 20);
            this.cbxParity.TabIndex = 47;
            this.cbxParity.SelectedIndexChanged += new System.EventHandler(this.ControlChanged);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(223, 94);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(41, 12);
            this.label7.TabIndex = 50;
            this.label7.Text = "停止位";
            // 
            // cbxStopBits
            // 
            this.cbxStopBits.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbxStopBits.FormattingEnabled = true;
            this.cbxStopBits.Items.AddRange(new object[] {
            "1",
            "1.5",
            "2"});
            this.cbxStopBits.Location = new System.Drawing.Point(270, 91);
            this.cbxStopBits.Name = "cbxStopBits";
            this.cbxStopBits.Size = new System.Drawing.Size(121, 20);
            this.cbxStopBits.TabIndex = 49;
            this.cbxStopBits.SelectedIndexChanged += new System.EventHandler(this.ControlChanged);
            // 
            // gbxTcp
            // 
            this.gbxTcp.Controls.Add(this.txtIPAddress);
            this.gbxTcp.Controls.Add(this.txtPort);
            this.gbxTcp.Controls.Add(this.label2);
            this.gbxTcp.Controls.Add(this.label1);
            this.gbxTcp.Location = new System.Drawing.Point(0, 0);
            this.gbxTcp.Name = "gbxTcp";
            this.gbxTcp.Size = new System.Drawing.Size(399, 100);
            this.gbxTcp.TabIndex = 51;
            this.gbxTcp.TabStop = false;
            this.gbxTcp.Text = "Tcp/Udp设置";
            // 
            // gbxSerial
            // 
            this.gbxSerial.Controls.Add(this.cbxSerialPortName);
            this.gbxSerial.Controls.Add(this.label3);
            this.gbxSerial.Controls.Add(this.label7);
            this.gbxSerial.Controls.Add(this.cbxBaudRate);
            this.gbxSerial.Controls.Add(this.cbxStopBits);
            this.gbxSerial.Controls.Add(this.label4);
            this.gbxSerial.Controls.Add(this.label6);
            this.gbxSerial.Controls.Add(this.cbxDataBits);
            this.gbxSerial.Controls.Add(this.cbxParity);
            this.gbxSerial.Controls.Add(this.label5);
            this.gbxSerial.Location = new System.Drawing.Point(0, 100);
            this.gbxSerial.Name = "gbxSerial";
            this.gbxSerial.Size = new System.Drawing.Size(399, 171);
            this.gbxSerial.TabIndex = 52;
            this.gbxSerial.TabStop = false;
            this.gbxSerial.Text = "串口设置";
            // 
            // btnConfirm
            // 
            this.btnConfirm.Location = new System.Drawing.Point(295, 280);
            this.btnConfirm.Name = "btnConfirm";
            this.btnConfirm.Size = new System.Drawing.Size(96, 38);
            this.btnConfirm.TabIndex = 53;
            this.btnConfirm.Text = "确认";
            this.btnConfirm.UseVisualStyleBackColor = true;
            this.btnConfirm.Click += new System.EventHandler(this.btnConfirm_Click);
            // 
            // btnCancle
            // 
            this.btnCancle.Location = new System.Drawing.Point(405, 280);
            this.btnCancle.Name = "btnCancle";
            this.btnCancle.Size = new System.Drawing.Size(96, 38);
            this.btnCancle.TabIndex = 54;
            this.btnCancle.Text = "取消";
            this.btnCancle.UseVisualStyleBackColor = true;
            this.btnCancle.Click += new System.EventHandler(this.btnCancle_Click);
            // 
            // gbxPLCOptions
            // 
            this.gbxPLCOptions.Controls.Add(this.txtSlot);
            this.gbxPLCOptions.Controls.Add(this.label9);
            this.gbxPLCOptions.Location = new System.Drawing.Point(405, 0);
            this.gbxPLCOptions.Name = "gbxPLCOptions";
            this.gbxPLCOptions.Size = new System.Drawing.Size(341, 271);
            this.gbxPLCOptions.TabIndex = 56;
            this.gbxPLCOptions.TabStop = false;
            this.gbxPLCOptions.Text = "PLC参数";
            // 
            // txtSlot
            // 
            this.txtSlot.Location = new System.Drawing.Point(89, 43);
            this.txtSlot.Name = "txtSlot";
            this.txtSlot.Size = new System.Drawing.Size(58, 21);
            this.txtSlot.TabIndex = 43;
            this.txtSlot.Text = "0";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(54, 47);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(29, 12);
            this.label9.TabIndex = 42;
            this.label9.Text = "Slot";
            // 
            // FrmParaSet
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(757, 330);
            this.Controls.Add(this.gbxPLCOptions);
            this.Controls.Add(this.btnCancle);
            this.Controls.Add(this.btnConfirm);
            this.Controls.Add(this.gbxSerial);
            this.Controls.Add(this.gbxTcp);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FrmParaSet";
            this.ShowIcon = false;
            this.Text = "参数设置";
            this.Load += new System.EventHandler(this.FrmParaSet_Load);
            this.gbxTcp.ResumeLayout(false);
            this.gbxTcp.PerformLayout();
            this.gbxSerial.ResumeLayout(false);
            this.gbxSerial.PerformLayout();
            this.gbxPLCOptions.ResumeLayout(false);
            this.gbxPLCOptions.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TextBox txtIPAddress;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtPort;
        private System.Windows.Forms.ComboBox cbxSerialPortName;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox cbxBaudRate;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ComboBox cbxDataBits;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.ComboBox cbxParity;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.ComboBox cbxStopBits;
        private System.Windows.Forms.GroupBox gbxTcp;
        private System.Windows.Forms.GroupBox gbxSerial;
        private System.Windows.Forms.Button btnConfirm;
        private System.Windows.Forms.Button btnCancle;
        private System.Windows.Forms.GroupBox gbxPLCOptions;
        private System.Windows.Forms.TextBox txtSlot;
        private System.Windows.Forms.Label label9;
    }
}