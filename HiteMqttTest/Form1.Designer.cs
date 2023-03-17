namespace HiteMqttTest
{
    partial class Form1
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

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.btnLoad = new System.Windows.Forms.Button();
            this.txtJsonModel = new System.Windows.Forms.TextBox();
            this.btnWrite = new System.Windows.Forms.Button();
            this.txtModelWrite = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.btnConnect = new System.Windows.Forms.Button();
            this.txtCmdReply = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.btnTest = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnLoad
            // 
            this.btnLoad.Location = new System.Drawing.Point(75, 28);
            this.btnLoad.Name = "btnLoad";
            this.btnLoad.Size = new System.Drawing.Size(131, 54);
            this.btnLoad.TabIndex = 0;
            this.btnLoad.Text = "载入模板";
            this.btnLoad.UseVisualStyleBackColor = true;
            this.btnLoad.Click += new System.EventHandler(this.btnLoad_Click);
            // 
            // txtJsonModel
            // 
            this.txtJsonModel.Location = new System.Drawing.Point(646, 46);
            this.txtJsonModel.Multiline = true;
            this.txtJsonModel.Name = "txtJsonModel";
            this.txtJsonModel.Size = new System.Drawing.Size(711, 447);
            this.txtJsonModel.TabIndex = 1;
            // 
            // btnWrite
            // 
            this.btnWrite.Location = new System.Drawing.Point(75, 156);
            this.btnWrite.Name = "btnWrite";
            this.btnWrite.Size = new System.Drawing.Size(131, 59);
            this.btnWrite.TabIndex = 2;
            this.btnWrite.Text = "写入";
            this.btnWrite.UseVisualStyleBackColor = true;
            this.btnWrite.Click += new System.EventHandler(this.btnWrite_Click);
            // 
            // txtModelWrite
            // 
            this.txtModelWrite.Location = new System.Drawing.Point(212, 28);
            this.txtModelWrite.Multiline = true;
            this.txtModelWrite.Name = "txtModelWrite";
            this.txtModelWrite.Size = new System.Drawing.Size(326, 187);
            this.txtModelWrite.TabIndex = 3;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(210, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(89, 12);
            this.label1.TabIndex = 4;
            this.label1.Text = "写入Mqtt数据值";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(644, 28);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(77, 12);
            this.label2.TabIndex = 5;
            this.label2.Text = "接收Mqtt数据";
            // 
            // btnConnect
            // 
            this.btnConnect.Location = new System.Drawing.Point(75, 88);
            this.btnConnect.Name = "btnConnect";
            this.btnConnect.Size = new System.Drawing.Size(131, 62);
            this.btnConnect.TabIndex = 6;
            this.btnConnect.Text = "连接Mqtt服务器";
            this.btnConnect.UseVisualStyleBackColor = true;
            this.btnConnect.Click += new System.EventHandler(this.btnConnect_Click);
            // 
            // txtCmdReply
            // 
            this.txtCmdReply.Location = new System.Drawing.Point(212, 264);
            this.txtCmdReply.Multiline = true;
            this.txtCmdReply.Name = "txtCmdReply";
            this.txtCmdReply.Size = new System.Drawing.Size(326, 187);
            this.txtCmdReply.TabIndex = 7;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(210, 239);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(113, 12);
            this.label3.TabIndex = 8;
            this.label3.Text = "写入Mqtt数据值回复";
            // 
            // btnTest
            // 
            this.btnTest.Location = new System.Drawing.Point(75, 264);
            this.btnTest.Name = "btnTest";
            this.btnTest.Size = new System.Drawing.Size(131, 59);
            this.btnTest.TabIndex = 9;
            this.btnTest.Text = "测试连接";
            this.btnTest.UseVisualStyleBackColor = true;
            this.btnTest.Click += new System.EventHandler(this.btnTest_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1369, 521);
            this.Controls.Add(this.btnTest);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.txtCmdReply);
            this.Controls.Add(this.btnConnect);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txtModelWrite);
            this.Controls.Add(this.btnWrite);
            this.Controls.Add(this.txtJsonModel);
            this.Controls.Add(this.btnLoad);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnLoad;
        private System.Windows.Forms.TextBox txtJsonModel;
        private System.Windows.Forms.Button btnWrite;
        private System.Windows.Forms.TextBox txtModelWrite;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnConnect;
        private System.Windows.Forms.TextBox txtCmdReply;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button btnTest;
    }
}

