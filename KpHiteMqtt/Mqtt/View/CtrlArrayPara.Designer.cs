namespace KpHiteMqtt.Mqtt.View
{
    partial class CtrlArrayPara
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
            this.lblArrayLength = new System.Windows.Forms.Label();
            this.txtArrayLength = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.cbxDataType = new System.Windows.Forms.ComboBox();
            this.ctrlJsonPara = new KpHiteMqtt.Mqtt.View.CtrlJsonPara();
            this.txtArrayChannel = new System.Windows.Forms.TextBox();
            this.btnImport = new System.Windows.Forms.Button();
            this.btnExport = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // lblArrayLength
            // 
            this.lblArrayLength.AutoSize = true;
            this.lblArrayLength.Location = new System.Drawing.Point(11, 17);
            this.lblArrayLength.Name = "lblArrayLength";
            this.lblArrayLength.Size = new System.Drawing.Size(53, 12);
            this.lblArrayLength.TabIndex = 0;
            this.lblArrayLength.Text = "数组长度";
            // 
            // txtArrayLength
            // 
            this.txtArrayLength.Location = new System.Drawing.Point(11, 41);
            this.txtArrayLength.Name = "txtArrayLength";
            this.txtArrayLength.Size = new System.Drawing.Size(99, 21);
            this.txtArrayLength.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(114, 17);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(53, 12);
            this.label1.TabIndex = 2;
            this.label1.Text = "数据类型";
            // 
            // cbxDataType
            // 
            this.cbxDataType.FormattingEnabled = true;
            this.cbxDataType.Location = new System.Drawing.Point(116, 42);
            this.cbxDataType.Name = "cbxDataType";
            this.cbxDataType.Size = new System.Drawing.Size(121, 20);
            this.cbxDataType.TabIndex = 3;
            // 
            // ctrlJsonPara
            // 
            this.ctrlJsonPara._allctrlcnls = null;
            this.ctrlJsonPara.AutoScroll = true;
            this.ctrlJsonPara.BackColor = System.Drawing.Color.WhiteSmoke;
            this.ctrlJsonPara.Location = new System.Drawing.Point(11, 207);
            this.ctrlJsonPara.Name = "ctrlJsonPara";
            this.ctrlJsonPara.Size = new System.Drawing.Size(515, 195);
            this.ctrlJsonPara.TabIndex = 4;
            // 
            // txtArrayChannel
            // 
            this.txtArrayChannel.Location = new System.Drawing.Point(11, 117);
            this.txtArrayChannel.Multiline = true;
            this.txtArrayChannel.Name = "txtArrayChannel";
            this.txtArrayChannel.Size = new System.Drawing.Size(515, 84);
            this.txtArrayChannel.TabIndex = 5;
            // 
            // btnImport
            // 
            this.btnImport.Location = new System.Drawing.Point(117, 81);
            this.btnImport.Name = "btnImport";
            this.btnImport.Size = new System.Drawing.Size(79, 31);
            this.btnImport.TabIndex = 6;
            this.btnImport.Text = "导入";
            this.btnImport.UseVisualStyleBackColor = true;
            this.btnImport.Click += new System.EventHandler(this.btnImport_Click);
            // 
            // btnExport
            // 
            this.btnExport.Location = new System.Drawing.Point(202, 81);
            this.btnExport.Name = "btnExport";
            this.btnExport.Size = new System.Drawing.Size(79, 31);
            this.btnExport.TabIndex = 7;
            this.btnExport.Text = "导出";
            this.btnExport.UseVisualStyleBackColor = true;
            this.btnExport.Click += new System.EventHandler(this.btnExport_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(10, 90);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(101, 12);
            this.label2.TabIndex = 8;
            this.label2.Text = "数组通道匹配关系";
            // 
            // CtrlArrayPara
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.label2);
            this.Controls.Add(this.btnExport);
            this.Controls.Add(this.btnImport);
            this.Controls.Add(this.txtArrayChannel);
            this.Controls.Add(this.ctrlJsonPara);
            this.Controls.Add(this.cbxDataType);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txtArrayLength);
            this.Controls.Add(this.lblArrayLength);
            this.Name = "CtrlArrayPara";
            this.Size = new System.Drawing.Size(540, 413);
            this.Load += new System.EventHandler(this.CtrlArrayPara_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblArrayLength;
        private System.Windows.Forms.TextBox txtArrayLength;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cbxDataType;
        private CtrlJsonPara ctrlJsonPara;
        private System.Windows.Forms.TextBox txtArrayChannel;
        private System.Windows.Forms.Button btnImport;
        private System.Windows.Forms.Button btnExport;
        private System.Windows.Forms.Label label2;
    }
}
