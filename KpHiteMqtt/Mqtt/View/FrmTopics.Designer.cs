namespace KpHiteMqtt.Mqtt.View
{
    partial class FrmTopics
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
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.TreeNode treeNode1 = new System.Windows.Forms.TreeNode("订阅");
            System.Windows.Forms.TreeNode treeNode2 = new System.Windows.Forms.TreeNode("发布");
            this.tvTopics = new System.Windows.Forms.TreeView();
            this.btnSubscribe = new System.Windows.Forms.Button();
            this.txtTopic = new System.Windows.Forms.TextBox();
            this.btnConfirm = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.ctmsTvTopics = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tspDeleteNode = new System.Windows.Forms.ToolStripMenuItem();
            this.ctmsTvTopics.SuspendLayout();
            this.SuspendLayout();
            // 
            // tvTopics
            // 
            this.tvTopics.ContextMenuStrip = this.ctmsTvTopics;
            this.tvTopics.Location = new System.Drawing.Point(13, 51);
            this.tvTopics.Name = "tvTopics";
            treeNode1.Name = "nodeSubscribe";
            treeNode1.Text = "订阅";
            treeNode2.Name = "nodePublish";
            treeNode2.Text = "发布";
            this.tvTopics.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
            treeNode1,
            treeNode2});
            this.tvTopics.Size = new System.Drawing.Size(384, 166);
            this.tvTopics.TabIndex = 0;
            // 
            // btnSubscribe
            // 
            this.btnSubscribe.Location = new System.Drawing.Point(322, 7);
            this.btnSubscribe.Name = "btnSubscribe";
            this.btnSubscribe.Size = new System.Drawing.Size(75, 38);
            this.btnSubscribe.TabIndex = 1;
            this.btnSubscribe.Text = "订阅";
            this.btnSubscribe.UseVisualStyleBackColor = true;
            this.btnSubscribe.Click += new System.EventHandler(this.btnSubscribe_Click);
            // 
            // txtTopic
            // 
            this.txtTopic.Location = new System.Drawing.Point(13, 16);
            this.txtTopic.Name = "txtTopic";
            this.txtTopic.Size = new System.Drawing.Size(303, 21);
            this.txtTopic.TabIndex = 3;
            // 
            // btnConfirm
            // 
            this.btnConfirm.Location = new System.Drawing.Point(120, 223);
            this.btnConfirm.Name = "btnConfirm";
            this.btnConfirm.Size = new System.Drawing.Size(78, 37);
            this.btnConfirm.TabIndex = 4;
            this.btnConfirm.Text = "确定";
            this.btnConfirm.UseVisualStyleBackColor = true;
            this.btnConfirm.Click += new System.EventHandler(this.btnConfirm_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(204, 223);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(78, 37);
            this.btnCancel.TabIndex = 5;
            this.btnCancel.Text = "取消";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // ctmsTvTopics
            // 
            this.ctmsTvTopics.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tspDeleteNode});
            this.ctmsTvTopics.Name = "ctmsTvTopics";
            this.ctmsTvTopics.Size = new System.Drawing.Size(101, 26);
            // 
            // tspDeleteNode
            // 
            this.tspDeleteNode.Name = "tspDeleteNode";
            this.tspDeleteNode.Size = new System.Drawing.Size(100, 22);
            this.tspDeleteNode.Text = "删除";
            this.tspDeleteNode.Click += new System.EventHandler(this.tspDeleteNode_Click);
            // 
            // FrmTopics
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(409, 272);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnConfirm);
            this.Controls.Add(this.txtTopic);
            this.Controls.Add(this.btnSubscribe);
            this.Controls.Add(this.tvTopics);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FrmTopics";
            this.ShowIcon = false;
            this.Text = "FrmTopics";
            this.Load += new System.EventHandler(this.FrmTopics_Load);
            this.ctmsTvTopics.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TreeView tvTopics;
        private System.Windows.Forms.Button btnSubscribe;
        private System.Windows.Forms.TextBox txtTopic;
        private System.Windows.Forms.Button btnConfirm;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.ContextMenuStrip ctmsTvTopics;
        private System.Windows.Forms.ToolStripMenuItem tspDeleteNode;
    }
}