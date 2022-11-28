using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace Scada.Server.Modules
{
	public class FrmAuthorInfo : UserControl
	{
		public FrmAuthorInfo()
		{
			this.InitializeComponent();
		}

		private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			
		}

		private void linkLabel1_LinkClicked_1(object sender, LinkLabelLinkClickedEventArgs e)
		{
			
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && this.components != null)
			{
				this.components.Dispose();
			}
			base.Dispose(disposing);
		}

		private void InitializeComponent()
		{
            this.plAuthor = new System.Windows.Forms.Panel();
            this.lbAuthor = new System.Windows.Forms.Label();
            this.lbAuthorName = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.linkLabel1 = new System.Windows.Forms.LinkLabel();
            this.llbQQNumber = new System.Windows.Forms.LinkLabel();
            this.plAuthor.SuspendLayout();
            this.SuspendLayout();
            // 
            // plAuthor
            // 
            this.plAuthor.Controls.Add(this.linkLabel1);
            this.plAuthor.Controls.Add(this.label1);
            this.plAuthor.Controls.Add(this.llbQQNumber);
            this.plAuthor.Controls.Add(this.lbAuthor);
            this.plAuthor.Controls.Add(this.lbAuthorName);
            this.plAuthor.Controls.Add(this.label3);
            this.plAuthor.Dock = System.Windows.Forms.DockStyle.Fill;
            this.plAuthor.Location = new System.Drawing.Point(0, 0);
            this.plAuthor.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.plAuthor.Name = "plAuthor";
            this.plAuthor.Size = new System.Drawing.Size(696, 46);
            this.plAuthor.TabIndex = 4;
            // 
            // lbAuthor
            // 
            this.lbAuthor.AutoSize = true;
            this.lbAuthor.Location = new System.Drawing.Point(28, 10);
            this.lbAuthor.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.lbAuthor.Name = "lbAuthor";
            this.lbAuthor.Size = new System.Drawing.Size(0, 48);
            this.lbAuthor.TabIndex = 0;
            // 
            // lbAuthorName
            // 
            this.lbAuthorName.AutoSize = true;
            this.lbAuthorName.Location = new System.Drawing.Point(152, 10);
            this.lbAuthorName.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.lbAuthorName.Name = "lbAuthorName";
            this.lbAuthorName.Size = new System.Drawing.Size(0, 48);
            this.lbAuthorName.TabIndex = 1;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(244, 10);
            this.label3.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(0, 48);
            this.label3.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(450, 10);
            this.label1.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(0, 48);
            this.label1.TabIndex = 5;
            // 
            // linkLabel1
            // 
            this.linkLabel1.AutoSize = true;
            this.linkLabel1.Location = new System.Drawing.Point(556, 10);
            this.linkLabel1.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.linkLabel1.Name = "linkLabel1";
            this.linkLabel1.Size = new System.Drawing.Size(0, 48);
            this.linkLabel1.TabIndex = 6;
            // 
            // llbQQNumber
            // 
            this.llbQQNumber.AutoSize = true;
            this.llbQQNumber.Location = new System.Drawing.Point(302, 10);
            this.llbQQNumber.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.llbQQNumber.Name = "llbQQNumber";
            this.llbQQNumber.Size = new System.Drawing.Size(0, 48);
            this.llbQQNumber.TabIndex = 4;
            // 
            // FrmAuthorInfo
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 24F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.plAuthor);
            this.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.Name = "FrmAuthorInfo";
            this.Size = new System.Drawing.Size(696, 46);
            this.plAuthor.ResumeLayout(false);
            this.plAuthor.PerformLayout();
            this.ResumeLayout(false);

		}

		private IContainer components;
        private LinkLabel linkLabel1;
        private Label label1;
        private LinkLabel llbQQNumber;
        private Label lbAuthor;
        private Label lbAuthorName;
        private Label label3;

        private Panel plAuthor;
	}
}
