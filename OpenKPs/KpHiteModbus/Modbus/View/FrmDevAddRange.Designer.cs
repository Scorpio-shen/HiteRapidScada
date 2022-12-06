namespace KpHiteModbus.Modbus.View
{
    partial class FrmDevAddRange
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
            this.txtNameReplace = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.txtStartAddress = new System.Windows.Forms.TextBox();
            this.numAddressIncrement = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.numTagCount = new System.Windows.Forms.NumericUpDown();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.txtLength = new System.Windows.Forms.TextBox();
            this.lblLength = new System.Windows.Forms.Label();
            this.numNameStartIndex = new System.Windows.Forms.NumericUpDown();
            this.label7 = new System.Windows.Forms.Label();
            this.cbxDataType = new System.Windows.Forms.ComboBox();
            this.label5 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.btnCancle = new System.Windows.Forms.Button();
            this.btnConfirm = new System.Windows.Forms.Button();
            this.lblAddressOutput = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.lblNameOutput = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.chkCanWrite = new System.Windows.Forms.CheckBox();
            this.label10 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.numAddressIncrement)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numTagCount)).BeginInit();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numNameStartIndex)).BeginInit();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // txtNameReplace
            // 
            this.txtNameReplace.Location = new System.Drawing.Point(92, 19);
            this.txtNameReplace.Name = "txtNameReplace";
            this.txtNameReplace.Size = new System.Drawing.Size(83, 21);
            this.txtNameReplace.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(21, 22);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(65, 12);
            this.label1.TabIndex = 1;
            this.label1.Text = "名称通配符";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(33, 62);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 12);
            this.label2.TabIndex = 3;
            this.label2.Text = "起始地址";
            // 
            // txtStartAddress
            // 
            this.txtStartAddress.Location = new System.Drawing.Point(92, 58);
            this.txtStartAddress.Name = "txtStartAddress";
            this.txtStartAddress.Size = new System.Drawing.Size(83, 21);
            this.txtStartAddress.TabIndex = 2;
            this.txtStartAddress.Validating += new System.ComponentModel.CancelEventHandler(this.txtStartAddress_Validating);
            // 
            // numAddressIncrement
            // 
            this.numAddressIncrement.DecimalPlaces = 1;
            this.numAddressIncrement.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.numAddressIncrement.Location = new System.Drawing.Point(92, 97);
            this.numAddressIncrement.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            this.numAddressIncrement.Name = "numAddressIncrement";
            this.numAddressIncrement.Size = new System.Drawing.Size(72, 21);
            this.numAddressIncrement.TabIndex = 4;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(33, 101);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(53, 12);
            this.label3.TabIndex = 5;
            this.label3.Text = "地址增量";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(205, 101);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(29, 12);
            this.label4.TabIndex = 7;
            this.label4.Text = "数目";
            // 
            // numTagCount
            // 
            this.numTagCount.Location = new System.Drawing.Point(240, 97);
            this.numTagCount.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            this.numTagCount.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numTagCount.Name = "numTagCount";
            this.numTagCount.Size = new System.Drawing.Size(72, 21);
            this.numTagCount.TabIndex = 6;
            this.numTagCount.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.chkCanWrite);
            this.groupBox1.Controls.Add(this.label10);
            this.groupBox1.Controls.Add(this.txtLength);
            this.groupBox1.Controls.Add(this.lblLength);
            this.groupBox1.Controls.Add(this.numNameStartIndex);
            this.groupBox1.Controls.Add(this.label7);
            this.groupBox1.Controls.Add(this.cbxDataType);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.txtStartAddress);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.numAddressIncrement);
            this.groupBox1.Controls.Add(this.txtNameReplace);
            this.groupBox1.Controls.Add(this.numTagCount);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBox1.Location = new System.Drawing.Point(0, 0);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(364, 178);
            this.groupBox1.TabIndex = 8;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "参数";
            // 
            // txtLength
            // 
            this.txtLength.Location = new System.Drawing.Point(240, 138);
            this.txtLength.Name = "txtLength";
            this.txtLength.Size = new System.Drawing.Size(72, 21);
            this.txtLength.TabIndex = 15;
            this.txtLength.Validating += new System.ComponentModel.CancelEventHandler(this.txtLength_Validating);
            // 
            // lblLength
            // 
            this.lblLength.AutoSize = true;
            this.lblLength.Location = new System.Drawing.Point(205, 141);
            this.lblLength.Name = "lblLength";
            this.lblLength.Size = new System.Drawing.Size(29, 12);
            this.lblLength.TabIndex = 14;
            this.lblLength.Text = "长度";
            // 
            // numNameStartIndex
            // 
            this.numNameStartIndex.Location = new System.Drawing.Point(240, 19);
            this.numNameStartIndex.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            this.numNameStartIndex.Name = "numNameStartIndex";
            this.numNameStartIndex.Size = new System.Drawing.Size(72, 21);
            this.numNameStartIndex.TabIndex = 13;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(181, 22);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(53, 12);
            this.label7.TabIndex = 11;
            this.label7.Text = "起始序号";
            // 
            // cbxDataType
            // 
            this.cbxDataType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbxDataType.FormattingEnabled = true;
            this.cbxDataType.Location = new System.Drawing.Point(92, 138);
            this.cbxDataType.Name = "cbxDataType";
            this.cbxDataType.Size = new System.Drawing.Size(107, 20);
            this.cbxDataType.TabIndex = 10;
            this.cbxDataType.SelectedIndexChanged += new System.EventHandler(this.cbxDataType_SelectedIndexChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(33, 141);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(53, 12);
            this.label5.TabIndex = 9;
            this.label5.Text = "数据类型";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.btnCancle);
            this.groupBox2.Controls.Add(this.btnConfirm);
            this.groupBox2.Controls.Add(this.lblAddressOutput);
            this.groupBox2.Controls.Add(this.label9);
            this.groupBox2.Controls.Add(this.lblNameOutput);
            this.groupBox2.Controls.Add(this.label6);
            this.groupBox2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox2.Location = new System.Drawing.Point(0, 178);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(364, 151);
            this.groupBox2.TabIndex = 9;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "输出";
            // 
            // btnCancle
            // 
            this.btnCancle.Location = new System.Drawing.Point(191, 106);
            this.btnCancle.Name = "btnCancle";
            this.btnCancle.Size = new System.Drawing.Size(75, 33);
            this.btnCancle.TabIndex = 16;
            this.btnCancle.Text = "取消";
            this.btnCancle.UseVisualStyleBackColor = true;
            this.btnCancle.Click += new System.EventHandler(this.btnCancle_Click);
            // 
            // btnConfirm
            // 
            this.btnConfirm.Location = new System.Drawing.Point(87, 106);
            this.btnConfirm.Name = "btnConfirm";
            this.btnConfirm.Size = new System.Drawing.Size(75, 33);
            this.btnConfirm.TabIndex = 15;
            this.btnConfirm.Text = "确认";
            this.btnConfirm.UseVisualStyleBackColor = true;
            this.btnConfirm.Click += new System.EventHandler(this.btnConfirm_Click);
            // 
            // lblAddressOutput
            // 
            this.lblAddressOutput.AutoSize = true;
            this.lblAddressOutput.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblAddressOutput.Location = new System.Drawing.Point(139, 80);
            this.lblAddressOutput.Name = "lblAddressOutput";
            this.lblAddressOutput.Size = new System.Drawing.Size(127, 14);
            this.lblAddressOutput.TabIndex = 14;
            this.lblAddressOutput.Text = "DB30_10~DB30_20";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label9.Location = new System.Drawing.Point(88, 80);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(45, 14);
            this.label9.TabIndex = 13;
            this.label9.Text = "地址:";
            // 
            // lblNameOutput
            // 
            this.lblNameOutput.AutoSize = true;
            this.lblNameOutput.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblNameOutput.Location = new System.Drawing.Point(139, 37);
            this.lblNameOutput.Name = "lblNameOutput";
            this.lblNameOutput.Size = new System.Drawing.Size(127, 14);
            this.lblNameOutput.TabIndex = 12;
            this.lblNameOutput.Text = "DB30_10~DB30_20";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label6.Location = new System.Drawing.Point(88, 37);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(45, 14);
            this.label6.TabIndex = 11;
            this.label6.Text = "名称:";
            // 
            // chkCanWrite
            // 
            this.chkCanWrite.AutoSize = true;
            this.chkCanWrite.Checked = true;
            this.chkCanWrite.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkCanWrite.Location = new System.Drawing.Point(240, 62);
            this.chkCanWrite.Name = "chkCanWrite";
            this.chkCanWrite.Size = new System.Drawing.Size(15, 14);
            this.chkCanWrite.TabIndex = 19;
            this.chkCanWrite.UseVisualStyleBackColor = true;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(181, 61);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(53, 12);
            this.label10.TabIndex = 18;
            this.label10.Text = "支持写入";
            // 
            // FrmDevAddRange
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(364, 329);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.MaximizeBox = false;
            this.Name = "FrmDevAddRange";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "批量添加";
            ((System.ComponentModel.ISupportInitialize)(this.numAddressIncrement)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numTagCount)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numNameStartIndex)).EndInit();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TextBox txtNameReplace;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtStartAddress;
        private System.Windows.Forms.NumericUpDown numAddressIncrement;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.NumericUpDown numTagCount;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.ComboBox cbxDataType;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label lblAddressOutput;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label lblNameOutput;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Button btnCancle;
        private System.Windows.Forms.Button btnConfirm;
        private System.Windows.Forms.NumericUpDown numNameStartIndex;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox txtLength;
        private System.Windows.Forms.Label lblLength;
        private System.Windows.Forms.CheckBox chkCanWrite;
        private System.Windows.Forms.Label label10;
    }
}