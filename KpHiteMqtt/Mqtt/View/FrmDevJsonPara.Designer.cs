namespace KpHiteModbus.Modbus.View
{
    partial class FrmDevJsonPara
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
            this.cbxOutputChannels = new System.Windows.Forms.ComboBox();
            this.lblOutputChannel = new System.Windows.Forms.Label();
            this.cbxInputChannels = new System.Windows.Forms.ComboBox();
            this.lblInputChannel = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.rdbReadOnlyR = new System.Windows.Forms.RadioButton();
            this.rdbReadOnlyRW = new System.Windows.Forms.RadioButton();
            this.cbxDataType = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.txtIdentifier = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txtName = new System.Windows.Forms.TextBox();
            this.txtUnit = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // cbxOutputChannels
            // 
            this.cbxOutputChannels.FormattingEnabled = true;
            this.cbxOutputChannels.Location = new System.Drawing.Point(127, 275);
            this.cbxOutputChannels.Name = "cbxOutputChannels";
            this.cbxOutputChannels.Size = new System.Drawing.Size(98, 20);
            this.cbxOutputChannels.TabIndex = 27;
            // 
            // lblOutputChannel
            // 
            this.lblOutputChannel.AutoSize = true;
            this.lblOutputChannel.Location = new System.Drawing.Point(125, 260);
            this.lblOutputChannel.Name = "lblOutputChannel";
            this.lblOutputChannel.Size = new System.Drawing.Size(53, 12);
            this.lblOutputChannel.TabIndex = 26;
            this.lblOutputChannel.Text = "输出通道";
            // 
            // cbxInputChannels
            // 
            this.cbxInputChannels.FormattingEnabled = true;
            this.cbxInputChannels.Location = new System.Drawing.Point(12, 275);
            this.cbxInputChannels.Name = "cbxInputChannels";
            this.cbxInputChannels.Size = new System.Drawing.Size(98, 20);
            this.cbxInputChannels.TabIndex = 25;
            // 
            // lblInputChannel
            // 
            this.lblInputChannel.AutoSize = true;
            this.lblInputChannel.Location = new System.Drawing.Point(10, 260);
            this.lblInputChannel.Name = "lblInputChannel";
            this.lblInputChannel.Size = new System.Drawing.Size(53, 12);
            this.lblInputChannel.TabIndex = 24;
            this.lblInputChannel.Text = "输入通道";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(10, 203);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(53, 12);
            this.label4.TabIndex = 23;
            this.label4.Text = "读写类型";
            // 
            // rdbReadOnlyR
            // 
            this.rdbReadOnlyR.AutoSize = true;
            this.rdbReadOnlyR.Location = new System.Drawing.Point(75, 228);
            this.rdbReadOnlyR.Name = "rdbReadOnlyR";
            this.rdbReadOnlyR.Size = new System.Drawing.Size(47, 16);
            this.rdbReadOnlyR.TabIndex = 22;
            this.rdbReadOnlyR.TabStop = true;
            this.rdbReadOnlyR.Text = "只读";
            this.rdbReadOnlyR.UseVisualStyleBackColor = true;
            // 
            // rdbReadOnlyRW
            // 
            this.rdbReadOnlyRW.AutoSize = true;
            this.rdbReadOnlyRW.Location = new System.Drawing.Point(12, 228);
            this.rdbReadOnlyRW.Name = "rdbReadOnlyRW";
            this.rdbReadOnlyRW.Size = new System.Drawing.Size(47, 16);
            this.rdbReadOnlyRW.TabIndex = 21;
            this.rdbReadOnlyRW.TabStop = true;
            this.rdbReadOnlyRW.Text = "读写";
            this.rdbReadOnlyRW.UseVisualStyleBackColor = true;
            // 
            // cbxDataType
            // 
            this.cbxDataType.FormattingEnabled = true;
            this.cbxDataType.Location = new System.Drawing.Point(12, 124);
            this.cbxDataType.Name = "cbxDataType";
            this.cbxDataType.Size = new System.Drawing.Size(341, 20);
            this.cbxDataType.TabIndex = 20;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(10, 109);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(53, 12);
            this.label3.TabIndex = 19;
            this.label3.Text = "数据类型";
            // 
            // txtIdentifier
            // 
            this.txtIdentifier.Location = new System.Drawing.Point(12, 74);
            this.txtIdentifier.Name = "txtIdentifier";
            this.txtIdentifier.Size = new System.Drawing.Size(341, 21);
            this.txtIdentifier.TabIndex = 18;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(10, 59);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(41, 12);
            this.label2.TabIndex = 17;
            this.label2.Text = "标识符";
            // 
            // txtName
            // 
            this.txtName.Location = new System.Drawing.Point(12, 25);
            this.txtName.Name = "txtName";
            this.txtName.Size = new System.Drawing.Size(341, 21);
            this.txtName.TabIndex = 16;
            // 
            // txtUnit
            // 
            this.txtUnit.Location = new System.Drawing.Point(12, 165);
            this.txtUnit.Name = "txtUnit";
            this.txtUnit.Size = new System.Drawing.Size(341, 21);
            this.txtUnit.TabIndex = 29;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(10, 150);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(29, 12);
            this.label1.TabIndex = 28;
            this.label1.Text = "单位";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(10, 9);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(29, 12);
            this.label5.TabIndex = 30;
            this.label5.Text = "名称";
            // 
            // FrmDevJsonPara
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(370, 450);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.txtUnit);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.cbxOutputChannels);
            this.Controls.Add(this.lblOutputChannel);
            this.Controls.Add(this.cbxInputChannels);
            this.Controls.Add(this.lblInputChannel);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.rdbReadOnlyR);
            this.Controls.Add(this.rdbReadOnlyRW);
            this.Controls.Add(this.cbxDataType);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.txtIdentifier);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txtName);
            this.Name = "FrmDevJsonPara";
            this.Text = "FrmDevJsonPara";
            this.Load += new System.EventHandler(this.FrmDevJsonPara_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox cbxOutputChannels;
        private System.Windows.Forms.Label lblOutputChannel;
        private System.Windows.Forms.ComboBox cbxInputChannels;
        private System.Windows.Forms.Label lblInputChannel;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.RadioButton rdbReadOnlyR;
        private System.Windows.Forms.RadioButton rdbReadOnlyRW;
        private System.Windows.Forms.ComboBox cbxDataType;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtIdentifier;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtName;
        private System.Windows.Forms.TextBox txtUnit;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label5;
    }
}