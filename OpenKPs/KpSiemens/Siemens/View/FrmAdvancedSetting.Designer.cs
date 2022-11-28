namespace KpSiemens.Siemens.View
{
    partial class FrmAdvancedSetting
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
            this.txtDestTASP = new System.Windows.Forms.TextBox();
            this.lblDestTASP = new System.Windows.Forms.Label();
            this.txtConnectionType = new System.Windows.Forms.TextBox();
            this.txtLocalTASP = new System.Windows.Forms.TextBox();
            this.lblLocalTASP = new System.Windows.Forms.Label();
            this.lblConnectionType = new System.Windows.Forms.Label();
            this.txtSlot = new System.Windows.Forms.TextBox();
            this.lblSlot = new System.Windows.Forms.Label();
            this.txtRack = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.lblRack = new System.Windows.Forms.Label();
            this.txtPort = new System.Windows.Forms.TextBox();
            this.btnConfirm = new System.Windows.Forms.Button();
            this.btnCancle = new System.Windows.Forms.Button();
            this.label7 = new System.Windows.Forms.Label();
            this.cbxPLCType = new System.Windows.Forms.ComboBox();
            this.txtIPAddress = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // txtDestTASP
            // 
            this.txtDestTASP.Location = new System.Drawing.Point(328, 112);
            this.txtDestTASP.Name = "txtDestTASP";
            this.txtDestTASP.Size = new System.Drawing.Size(40, 21);
            this.txtDestTASP.TabIndex = 32;
            this.txtDestTASP.Text = "258";
            this.txtDestTASP.TextChanged += new System.EventHandler(this.txtDestTASP_TextChanged);
            // 
            // lblDestTASP
            // 
            this.lblDestTASP.AutoSize = true;
            this.lblDestTASP.Location = new System.Drawing.Point(263, 116);
            this.lblDestTASP.Name = "lblDestTASP";
            this.lblDestTASP.Size = new System.Drawing.Size(53, 12);
            this.lblDestTASP.TabIndex = 31;
            this.lblDestTASP.Text = "DestTASP";
            // 
            // txtConnectionType
            // 
            this.txtConnectionType.Location = new System.Drawing.Point(103, 111);
            this.txtConnectionType.Name = "txtConnectionType";
            this.txtConnectionType.Size = new System.Drawing.Size(40, 21);
            this.txtConnectionType.TabIndex = 30;
            this.txtConnectionType.Text = "1";
            this.txtConnectionType.TextChanged += new System.EventHandler(this.txtConnectionType_TextChanged);
            // 
            // txtLocalTASP
            // 
            this.txtLocalTASP.Location = new System.Drawing.Point(328, 75);
            this.txtLocalTASP.Name = "txtLocalTASP";
            this.txtLocalTASP.Size = new System.Drawing.Size(40, 21);
            this.txtLocalTASP.TabIndex = 29;
            this.txtLocalTASP.Text = "258";
            this.txtLocalTASP.TextChanged += new System.EventHandler(this.txtLocalTASP_TextChanged);
            // 
            // lblLocalTASP
            // 
            this.lblLocalTASP.AutoSize = true;
            this.lblLocalTASP.Location = new System.Drawing.Point(263, 79);
            this.lblLocalTASP.Name = "lblLocalTASP";
            this.lblLocalTASP.Size = new System.Drawing.Size(59, 12);
            this.lblLocalTASP.TabIndex = 28;
            this.lblLocalTASP.Text = "LocalTASP";
            // 
            // lblConnectionType
            // 
            this.lblConnectionType.AutoSize = true;
            this.lblConnectionType.Location = new System.Drawing.Point(8, 115);
            this.lblConnectionType.Name = "lblConnectionType";
            this.lblConnectionType.Size = new System.Drawing.Size(89, 12);
            this.lblConnectionType.TabIndex = 27;
            this.lblConnectionType.Text = "ConnectionType";
            // 
            // txtSlot
            // 
            this.txtSlot.Location = new System.Drawing.Point(201, 111);
            this.txtSlot.Name = "txtSlot";
            this.txtSlot.Size = new System.Drawing.Size(40, 21);
            this.txtSlot.TabIndex = 26;
            this.txtSlot.Text = "0";
            this.txtSlot.TextChanged += new System.EventHandler(this.txtSlot_TextChanged);
            // 
            // lblSlot
            // 
            this.lblSlot.AutoSize = true;
            this.lblSlot.Location = new System.Drawing.Point(166, 115);
            this.lblSlot.Name = "lblSlot";
            this.lblSlot.Size = new System.Drawing.Size(29, 12);
            this.lblSlot.TabIndex = 25;
            this.lblSlot.Text = "Slot";
            // 
            // txtRack
            // 
            this.txtRack.Location = new System.Drawing.Point(201, 74);
            this.txtRack.Name = "txtRack";
            this.txtRack.Size = new System.Drawing.Size(40, 21);
            this.txtRack.TabIndex = 24;
            this.txtRack.Text = "0";
            this.txtRack.TextChanged += new System.EventHandler(this.txtRack_TextChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(56, 78);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(41, 12);
            this.label2.TabIndex = 21;
            this.label2.Text = "端口号";
            // 
            // lblRack
            // 
            this.lblRack.AutoSize = true;
            this.lblRack.Location = new System.Drawing.Point(166, 78);
            this.lblRack.Name = "lblRack";
            this.lblRack.Size = new System.Drawing.Size(29, 12);
            this.lblRack.TabIndex = 23;
            this.lblRack.Text = "Rack";
            // 
            // txtPort
            // 
            this.txtPort.Location = new System.Drawing.Point(103, 74);
            this.txtPort.Name = "txtPort";
            this.txtPort.Size = new System.Drawing.Size(40, 21);
            this.txtPort.TabIndex = 22;
            this.txtPort.Text = "102";
            this.txtPort.TextChanged += new System.EventHandler(this.txtPort_TextChanged);
            // 
            // btnConfirm
            // 
            this.btnConfirm.Location = new System.Drawing.Point(103, 154);
            this.btnConfirm.Name = "btnConfirm";
            this.btnConfirm.Size = new System.Drawing.Size(75, 32);
            this.btnConfirm.TabIndex = 33;
            this.btnConfirm.Text = "确认";
            this.btnConfirm.UseVisualStyleBackColor = true;
            this.btnConfirm.Click += new System.EventHandler(this.btnConfirm_Click);
            // 
            // btnCancle
            // 
            this.btnCancle.Location = new System.Drawing.Point(201, 154);
            this.btnCancle.Name = "btnCancle";
            this.btnCancle.Size = new System.Drawing.Size(75, 32);
            this.btnCancle.TabIndex = 34;
            this.btnCancle.Text = "取消";
            this.btnCancle.UseVisualStyleBackColor = true;
            this.btnCancle.Click += new System.EventHandler(this.btnCancle_Click);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(203, 25);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(47, 12);
            this.label7.TabIndex = 38;
            this.label7.Text = "PLC型号";
            // 
            // cbxPLCType
            // 
            this.cbxPLCType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbxPLCType.Enabled = false;
            this.cbxPLCType.FormattingEnabled = true;
            this.cbxPLCType.Location = new System.Drawing.Point(256, 22);
            this.cbxPLCType.Name = "cbxPLCType";
            this.cbxPLCType.Size = new System.Drawing.Size(121, 20);
            this.cbxPLCType.TabIndex = 37;
            // 
            // txtIPAddress
            // 
            this.txtIPAddress.Enabled = false;
            this.txtIPAddress.Location = new System.Drawing.Point(58, 21);
            this.txtIPAddress.Name = "txtIPAddress";
            this.txtIPAddress.Size = new System.Drawing.Size(121, 21);
            this.txtIPAddress.TabIndex = 36;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 25);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(41, 12);
            this.label1.TabIndex = 35;
            this.label1.Text = "IP地址";
            // 
            // FrmAdvancedSetting
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(391, 199);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.cbxPLCType);
            this.Controls.Add(this.txtIPAddress);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnCancle);
            this.Controls.Add(this.btnConfirm);
            this.Controls.Add(this.txtDestTASP);
            this.Controls.Add(this.lblDestTASP);
            this.Controls.Add(this.txtConnectionType);
            this.Controls.Add(this.txtLocalTASP);
            this.Controls.Add(this.lblLocalTASP);
            this.Controls.Add(this.lblConnectionType);
            this.Controls.Add(this.txtSlot);
            this.Controls.Add(this.lblSlot);
            this.Controls.Add(this.txtRack);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.lblRack);
            this.Controls.Add(this.txtPort);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FrmAdvancedSetting";
            this.ShowIcon = false;
            this.Text = "高级参数设置";
            this.Load += new System.EventHandler(this.FrmAdvancedSetting_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtDestTASP;
        private System.Windows.Forms.Label lblDestTASP;
        private System.Windows.Forms.TextBox txtConnectionType;
        private System.Windows.Forms.TextBox txtLocalTASP;
        private System.Windows.Forms.Label lblLocalTASP;
        private System.Windows.Forms.Label lblConnectionType;
        private System.Windows.Forms.TextBox txtSlot;
        private System.Windows.Forms.Label lblSlot;
        private System.Windows.Forms.TextBox txtRack;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label lblRack;
        private System.Windows.Forms.TextBox txtPort;
        private System.Windows.Forms.Button btnConfirm;
        private System.Windows.Forms.Button btnCancle;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.ComboBox cbxPLCType;
        private System.Windows.Forms.TextBox txtIPAddress;
        private System.Windows.Forms.Label label1;
    }
}