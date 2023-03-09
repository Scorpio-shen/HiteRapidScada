namespace KpHiteMqtt.Mqtt.View
{
    partial class FrmDevModel
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
            this.label1 = new System.Windows.Forms.Label();
            this.txtName = new System.Windows.Forms.TextBox();
            this.txtIdentifier = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.cbxDataType = new System.Windows.Forms.ComboBox();
            this.rdbReadOnlyRW = new System.Windows.Forms.RadioButton();
            this.rdbReadOnlyR = new System.Windows.Forms.RadioButton();
            this.label5 = new System.Windows.Forms.Label();
            this.txtDescription = new System.Windows.Forms.TextBox();
            this.lblInputChannel = new System.Windows.Forms.Label();
            this.cbxInputChannels = new System.Windows.Forms.ComboBox();
            this.cbxOutputChannels = new System.Windows.Forms.ComboBox();
            this.lblOutputChannel = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.btnConfirm = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.txtUnit = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.ctrlArrayPara = new KpHiteMqtt.Mqtt.View.CtrlArrayPara();
            this.ctrlJsonPara = new KpHiteMqtt.Mqtt.View.CtrlJsonPara();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(10, 11);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(29, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "名称";
            // 
            // txtName
            // 
            this.txtName.Location = new System.Drawing.Point(12, 26);
            this.txtName.Name = "txtName";
            this.txtName.Size = new System.Drawing.Size(341, 21);
            this.txtName.TabIndex = 1;
            // 
            // txtIdentifier
            // 
            this.txtIdentifier.Location = new System.Drawing.Point(12, 75);
            this.txtIdentifier.Name = "txtIdentifier";
            this.txtIdentifier.Size = new System.Drawing.Size(341, 21);
            this.txtIdentifier.TabIndex = 3;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(10, 60);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(41, 12);
            this.label2.TabIndex = 2;
            this.label2.Text = "标识符";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(10, 110);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(53, 12);
            this.label3.TabIndex = 4;
            this.label3.Text = "数据类型";
            // 
            // cbxDataType
            // 
            this.cbxDataType.FormattingEnabled = true;
            this.cbxDataType.Location = new System.Drawing.Point(12, 125);
            this.cbxDataType.Name = "cbxDataType";
            this.cbxDataType.Size = new System.Drawing.Size(341, 20);
            this.cbxDataType.TabIndex = 6;
            // 
            // rdbReadOnlyRW
            // 
            this.rdbReadOnlyRW.AutoSize = true;
            this.rdbReadOnlyRW.Checked = true;
            this.rdbReadOnlyRW.Location = new System.Drawing.Point(6, 20);
            this.rdbReadOnlyRW.Name = "rdbReadOnlyRW";
            this.rdbReadOnlyRW.Size = new System.Drawing.Size(47, 16);
            this.rdbReadOnlyRW.TabIndex = 7;
            this.rdbReadOnlyRW.TabStop = true;
            this.rdbReadOnlyRW.Text = "读写";
            this.rdbReadOnlyRW.UseVisualStyleBackColor = true;
            // 
            // rdbReadOnlyR
            // 
            this.rdbReadOnlyR.AutoSize = true;
            this.rdbReadOnlyR.Location = new System.Drawing.Point(68, 20);
            this.rdbReadOnlyR.Name = "rdbReadOnlyR";
            this.rdbReadOnlyR.Size = new System.Drawing.Size(47, 16);
            this.rdbReadOnlyR.TabIndex = 8;
            this.rdbReadOnlyR.Text = "只读";
            this.rdbReadOnlyR.UseVisualStyleBackColor = true;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(14, 318);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(29, 12);
            this.label5.TabIndex = 10;
            this.label5.Text = "描述";
            // 
            // txtDescription
            // 
            this.txtDescription.Location = new System.Drawing.Point(12, 333);
            this.txtDescription.Multiline = true;
            this.txtDescription.Name = "txtDescription";
            this.txtDescription.Size = new System.Drawing.Size(540, 111);
            this.txtDescription.TabIndex = 11;
            // 
            // lblInputChannel
            // 
            this.lblInputChannel.AutoSize = true;
            this.lblInputChannel.Location = new System.Drawing.Point(10, 223);
            this.lblInputChannel.Name = "lblInputChannel";
            this.lblInputChannel.Size = new System.Drawing.Size(53, 12);
            this.lblInputChannel.TabIndex = 12;
            this.lblInputChannel.Text = "输入通道";
            // 
            // cbxInputChannels
            // 
            this.cbxInputChannels.FormattingEnabled = true;
            this.cbxInputChannels.Location = new System.Drawing.Point(12, 238);
            this.cbxInputChannels.Name = "cbxInputChannels";
            this.cbxInputChannels.Size = new System.Drawing.Size(98, 20);
            this.cbxInputChannels.TabIndex = 13;
            // 
            // cbxOutputChannels
            // 
            this.cbxOutputChannels.FormattingEnabled = true;
            this.cbxOutputChannels.Location = new System.Drawing.Point(127, 238);
            this.cbxOutputChannels.Name = "cbxOutputChannels";
            this.cbxOutputChannels.Size = new System.Drawing.Size(98, 20);
            this.cbxOutputChannels.TabIndex = 15;
            // 
            // lblOutputChannel
            // 
            this.lblOutputChannel.AutoSize = true;
            this.lblOutputChannel.Location = new System.Drawing.Point(125, 223);
            this.lblOutputChannel.Name = "lblOutputChannel";
            this.lblOutputChannel.Size = new System.Drawing.Size(53, 12);
            this.lblOutputChannel.TabIndex = 14;
            this.lblOutputChannel.Text = "输出通道";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.rdbReadOnlyR);
            this.groupBox1.Controls.Add(this.rdbReadOnlyRW);
            this.groupBox1.Location = new System.Drawing.Point(12, 160);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(341, 54);
            this.groupBox1.TabIndex = 17;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "读写属性";
            // 
            // btnConfirm
            // 
            this.btnConfirm.Location = new System.Drawing.Point(135, 894);
            this.btnConfirm.Name = "btnConfirm";
            this.btnConfirm.Size = new System.Drawing.Size(93, 33);
            this.btnConfirm.TabIndex = 18;
            this.btnConfirm.Text = "确定";
            this.btnConfirm.UseVisualStyleBackColor = true;
            this.btnConfirm.Click += new System.EventHandler(this.btnConfirm_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(272, 894);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(93, 33);
            this.btnCancel.TabIndex = 19;
            this.btnCancel.Text = "取消";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // txtUnit
            // 
            this.txtUnit.Location = new System.Drawing.Point(12, 285);
            this.txtUnit.Name = "txtUnit";
            this.txtUnit.Size = new System.Drawing.Size(341, 21);
            this.txtUnit.TabIndex = 21;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(10, 270);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(29, 12);
            this.label4.TabIndex = 20;
            this.label4.Text = "单位";
            // 
            // ctrlArrayPara
            // 
            this.ctrlArrayPara.Location = new System.Drawing.Point(12, 455);
            this.ctrlArrayPara.Name = "ctrlArrayPara";
            this.ctrlArrayPara.Size = new System.Drawing.Size(540, 413);
            this.ctrlArrayPara.TabIndex = 22;
            // 
            // ctrlJsonPara
            // 
            this.ctrlJsonPara._allctrlcnls = null;
            this.ctrlJsonPara.AutoScroll = true;
            this.ctrlJsonPara.BackColor = System.Drawing.Color.WhiteSmoke;
            this.ctrlJsonPara.Location = new System.Drawing.Point(12, 455);
            this.ctrlJsonPara.Name = "ctrlJsonPara";
            this.ctrlJsonPara.Size = new System.Drawing.Size(540, 413);
            this.ctrlJsonPara.TabIndex = 16;
            // 
            // FrmDevModel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(569, 948);
            this.Controls.Add(this.ctrlArrayPara);
            this.Controls.Add(this.txtUnit);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnConfirm);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.ctrlJsonPara);
            this.Controls.Add(this.cbxOutputChannels);
            this.Controls.Add(this.lblOutputChannel);
            this.Controls.Add(this.cbxInputChannels);
            this.Controls.Add(this.lblInputChannel);
            this.Controls.Add(this.txtDescription);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.cbxDataType);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.txtIdentifier);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txtName);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FrmDevModel";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtName;
        private System.Windows.Forms.TextBox txtIdentifier;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox cbxDataType;
        private System.Windows.Forms.RadioButton rdbReadOnlyRW;
        private System.Windows.Forms.RadioButton rdbReadOnlyR;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txtDescription;
        private System.Windows.Forms.Label lblInputChannel;
        private System.Windows.Forms.ComboBox cbxInputChannels;
        private System.Windows.Forms.ComboBox cbxOutputChannels;
        private System.Windows.Forms.Label lblOutputChannel;
        private CtrlJsonPara ctrlJsonPara;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button btnConfirm;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.TextBox txtUnit;
        private System.Windows.Forms.Label label4;
        private CtrlArrayPara ctrlArrayPara;
    }
}