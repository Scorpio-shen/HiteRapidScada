namespace KpHiteMqtt.Mqtt.View
{
    partial class CtrlJsonPara
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
            this.linkAddPara = new System.Windows.Forms.LinkLabel();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.panelPara = new System.Windows.Forms.Panel();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // linkAddPara
            // 
            this.linkAddPara.AutoSize = true;
            this.linkAddPara.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.linkAddPara.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            this.linkAddPara.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(26)))), ((int)(((byte)(128)))), ((int)(((byte)(210)))));
            this.linkAddPara.Location = new System.Drawing.Point(71, 17);
            this.linkAddPara.Name = "linkAddPara";
            this.linkAddPara.Size = new System.Drawing.Size(65, 12);
            this.linkAddPara.TabIndex = 0;
            this.linkAddPara.TabStop = true;
            this.linkAddPara.Text = "+ 新增参数";
            this.linkAddPara.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkAddPara_LinkClicked);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label1.Location = new System.Drawing.Point(6, 17);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(59, 12);
            this.label1.TabIndex = 1;
            this.label1.Text = "Json对象";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.linkAddPara);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBox1.Location = new System.Drawing.Point(0, 0);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(540, 43);
            this.groupBox1.TabIndex = 3;
            this.groupBox1.TabStop = false;
            // 
            // panelPara
            // 
            this.panelPara.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelPara.Location = new System.Drawing.Point(0, 43);
            this.panelPara.Name = "panelPara";
            this.panelPara.Size = new System.Drawing.Size(540, 170);
            this.panelPara.TabIndex = 4;
            // 
            // CtrlJsonPara
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.BackColor = System.Drawing.Color.WhiteSmoke;
            this.Controls.Add(this.panelPara);
            this.Controls.Add(this.groupBox1);
            this.Name = "CtrlJsonPara";
            this.Size = new System.Drawing.Size(540, 213);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.LinkLabel linkAddPara;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Panel panelPara;
    }
}
