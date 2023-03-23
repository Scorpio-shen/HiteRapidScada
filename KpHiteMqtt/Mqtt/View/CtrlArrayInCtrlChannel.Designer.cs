namespace KpHiteMqtt.Mqtt.View
{
    partial class CtrlArrayInCtrlChannel
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
            this.cbxInCnl = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.cbxCtrlCnl = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // cbxInCnl
            // 
            this.cbxInCnl.FormattingEnabled = true;
            this.cbxInCnl.Location = new System.Drawing.Point(25, 29);
            this.cbxInCnl.Name = "cbxInCnl";
            this.cbxInCnl.Size = new System.Drawing.Size(121, 20);
            this.cbxInCnl.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(25, 11);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(53, 12);
            this.label1.TabIndex = 1;
            this.label1.Text = "输入通道";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(168, 11);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 12);
            this.label2.TabIndex = 3;
            this.label2.Text = "输出通道";
            // 
            // cbxCtrlCnl
            // 
            this.cbxCtrlCnl.FormattingEnabled = true;
            this.cbxCtrlCnl.Location = new System.Drawing.Point(168, 29);
            this.cbxCtrlCnl.Name = "cbxCtrlCnl";
            this.cbxCtrlCnl.Size = new System.Drawing.Size(121, 20);
            this.cbxCtrlCnl.TabIndex = 2;
            // 
            // CtrlArrayInCtrlChannel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.label2);
            this.Controls.Add(this.cbxCtrlCnl);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.cbxInCnl);
            this.Name = "CtrlArrayInCtrlChannel";
            this.Size = new System.Drawing.Size(532, 66);
            this.Load += new System.EventHandler(this.CtrlArrayInCtrlChannel_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox cbxInCnl;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox cbxCtrlCnl;
    }
}
