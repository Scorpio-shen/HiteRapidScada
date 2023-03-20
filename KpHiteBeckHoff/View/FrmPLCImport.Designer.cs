namespace KpHiteBeckHoff.View
{
    partial class FrmPLCImport
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
            this.ctrlTreeView = new KpHiteBeckHoff.View.CtrlTreeView();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.btnRefresh = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.cbxProtocolType = new System.Windows.Forms.ComboBox();
            this.btnDisConnect = new System.Windows.Forms.Button();
            this.btnConnect = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.txtPort = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.txtIPAddress = new System.Windows.Forms.TextBox();
            this.btnCancle = new System.Windows.Forms.Button();
            this.btnConfirm = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // ctrlTreeView
            // 
            this.ctrlTreeView.Location = new System.Drawing.Point(6, 20);
            this.ctrlTreeView.Name = "ctrlTreeView";
            this.ctrlTreeView.Size = new System.Drawing.Size(713, 397);
            this.ctrlTreeView.TabIndex = 0;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.ctrlTreeView);
            this.groupBox1.Location = new System.Drawing.Point(12, 72);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(781, 420);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.btnRefresh);
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Controls.Add(this.cbxProtocolType);
            this.groupBox2.Controls.Add(this.btnDisConnect);
            this.groupBox2.Controls.Add(this.btnConnect);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Controls.Add(this.txtPort);
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Controls.Add(this.txtIPAddress);
            this.groupBox2.Location = new System.Drawing.Point(12, 12);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(781, 54);
            this.groupBox2.TabIndex = 2;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "连接参数";
            // 
            // btnRefresh
            // 
            this.btnRefresh.Location = new System.Drawing.Point(504, 12);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(85, 36);
            this.btnRefresh.TabIndex = 31;
            this.btnRefresh.Text = "刷新";
            this.btnRefresh.UseVisualStyleBackColor = true;
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(287, 25);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(53, 12);
            this.label3.TabIndex = 30;
            this.label3.Text = "协议类型";
            // 
            // cbxProtocolType
            // 
            this.cbxProtocolType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbxProtocolType.FormattingEnabled = true;
            this.cbxProtocolType.Location = new System.Drawing.Point(346, 20);
            this.cbxProtocolType.Name = "cbxProtocolType";
            this.cbxProtocolType.Size = new System.Drawing.Size(143, 20);
            this.cbxProtocolType.TabIndex = 29;
            // 
            // btnDisConnect
            // 
            this.btnDisConnect.Location = new System.Drawing.Point(686, 12);
            this.btnDisConnect.Name = "btnDisConnect";
            this.btnDisConnect.Size = new System.Drawing.Size(85, 36);
            this.btnDisConnect.TabIndex = 5;
            this.btnDisConnect.Text = "断开";
            this.btnDisConnect.UseVisualStyleBackColor = true;
            this.btnDisConnect.Click += new System.EventHandler(this.btnDisConnect_Click);
            // 
            // btnConnect
            // 
            this.btnConnect.Location = new System.Drawing.Point(595, 12);
            this.btnConnect.Name = "btnConnect";
            this.btnConnect.Size = new System.Drawing.Size(85, 36);
            this.btnConnect.TabIndex = 4;
            this.btnConnect.Text = "连接";
            this.btnConnect.UseVisualStyleBackColor = true;
            this.btnConnect.Click += new System.EventHandler(this.btnConnect_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(172, 25);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(35, 12);
            this.label2.TabIndex = 3;
            this.label2.Text = "端口:";
            // 
            // txtPort
            // 
            this.txtPort.Location = new System.Drawing.Point(213, 19);
            this.txtPort.Name = "txtPort";
            this.txtPort.Size = new System.Drawing.Size(68, 21);
            this.txtPort.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 22);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(47, 12);
            this.label1.TabIndex = 1;
            this.label1.Text = "IP地址:";
            // 
            // txtIPAddress
            // 
            this.txtIPAddress.Location = new System.Drawing.Point(59, 19);
            this.txtIPAddress.Name = "txtIPAddress";
            this.txtIPAddress.Size = new System.Drawing.Size(107, 21);
            this.txtIPAddress.TabIndex = 0;
            // 
            // btnCancle
            // 
            this.btnCancle.Location = new System.Drawing.Point(376, 498);
            this.btnCancle.Name = "btnCancle";
            this.btnCancle.Size = new System.Drawing.Size(96, 38);
            this.btnCancle.TabIndex = 56;
            this.btnCancle.Text = "取消";
            this.btnCancle.UseVisualStyleBackColor = true;
            this.btnCancle.Click += new System.EventHandler(this.btnCancle_Click);
            // 
            // btnConfirm
            // 
            this.btnConfirm.Location = new System.Drawing.Point(266, 498);
            this.btnConfirm.Name = "btnConfirm";
            this.btnConfirm.Size = new System.Drawing.Size(96, 38);
            this.btnConfirm.TabIndex = 55;
            this.btnConfirm.Text = "确认";
            this.btnConfirm.UseVisualStyleBackColor = true;
            this.btnConfirm.Click += new System.EventHandler(this.btnConfirm_Click);
            // 
            // FrmPLCImport
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(811, 547);
            this.Controls.Add(this.btnCancle);
            this.Controls.Add(this.btnConfirm);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FrmPLCImport";
            this.ShowIcon = false;
            this.Text = "PLC导入";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FrmPLCImport_FormClosing);
            this.Load += new System.EventHandler(this.FrmPLCImport_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private CtrlTreeView ctrlTreeView;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtIPAddress;
        private System.Windows.Forms.Button btnDisConnect;
        private System.Windows.Forms.Button btnConnect;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtPort;
        private System.Windows.Forms.Button btnCancle;
        private System.Windows.Forms.Button btnConfirm;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox cbxProtocolType;
        private System.Windows.Forms.Button btnRefresh;
    }
}