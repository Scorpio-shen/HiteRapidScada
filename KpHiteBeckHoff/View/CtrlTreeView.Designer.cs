namespace KpHiteBeckHoff.View
{
    partial class CtrlTreeView
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CtrlTreeView));
            this.trvSource = new System.Windows.Forms.TreeView();
            this.btnMoveAll = new System.Windows.Forms.Button();
            this.btnMove = new System.Windows.Forms.Button();
            this.trvTarget = new System.Windows.Forms.TreeView();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsmDeleteTreeNode = new System.Windows.Forms.ToolStripMenuItem();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // trvSource
            // 
            this.trvSource.AllowDrop = true;
            this.trvSource.Location = new System.Drawing.Point(3, 3);
            this.trvSource.Name = "trvSource";
            this.trvSource.Size = new System.Drawing.Size(307, 387);
            this.trvSource.TabIndex = 1;
            this.trvSource.MouseClick += new System.Windows.Forms.MouseEventHandler(this.trvSource_MouseClick);
            // 
            // btnMoveAll
            // 
            this.btnMoveAll.Location = new System.Drawing.Point(316, 183);
            this.btnMoveAll.Name = "btnMoveAll";
            this.btnMoveAll.Size = new System.Drawing.Size(75, 23);
            this.btnMoveAll.TabIndex = 5;
            this.btnMoveAll.Text = ">>";
            this.btnMoveAll.UseVisualStyleBackColor = true;
            this.btnMoveAll.Click += new System.EventHandler(this.btnMoveAll_Click);
            // 
            // btnMove
            // 
            this.btnMove.Location = new System.Drawing.Point(316, 154);
            this.btnMove.Name = "btnMove";
            this.btnMove.Size = new System.Drawing.Size(75, 23);
            this.btnMove.TabIndex = 4;
            this.btnMove.Text = ">";
            this.btnMove.UseVisualStyleBackColor = true;
            this.btnMove.Click += new System.EventHandler(this.btnMove_Click);
            // 
            // trvTarget
            // 
            this.trvTarget.AllowDrop = true;
            this.trvTarget.ContextMenuStrip = this.contextMenuStrip1;
            this.trvTarget.Location = new System.Drawing.Point(397, 3);
            this.trvTarget.Name = "trvTarget";
            this.trvTarget.Size = new System.Drawing.Size(307, 387);
            this.trvTarget.TabIndex = 6;
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.ImageScalingSize = new System.Drawing.Size(36, 36);
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmDeleteTreeNode});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(101, 26);
            // 
            // tsmDeleteTreeNode
            // 
            this.tsmDeleteTreeNode.Name = "tsmDeleteTreeNode";
            this.tsmDeleteTreeNode.Size = new System.Drawing.Size(100, 22);
            this.tsmDeleteTreeNode.Text = "删除";
            this.tsmDeleteTreeNode.Click += new System.EventHandler(this.tsmDeleteTreeNode_Click);
            // 
            // imageList1
            // 
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "brackets_Square_16xMD.png");
            this.imageList1.Images.SetKeyName(1, "Class_489.png");
            this.imageList1.Images.SetKeyName(2, "Enum_582.png");
            this.imageList1.Images.SetKeyName(3, "Method_636.png");
            this.imageList1.Images.SetKeyName(4, "Module_648.png");
            this.imageList1.Images.SetKeyName(5, "Structure_507.png");
            this.imageList1.Images.SetKeyName(6, "VirtualMachine.png");
            // 
            // CtrlTreeView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.trvTarget);
            this.Controls.Add(this.btnMoveAll);
            this.Controls.Add(this.btnMove);
            this.Controls.Add(this.trvSource);
            this.Name = "CtrlTreeView";
            this.Size = new System.Drawing.Size(713, 397);
            this.Load += new System.EventHandler(this.CtrlTreeView_Load);
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TreeView trvSource;
        private System.Windows.Forms.Button btnMoveAll;
        private System.Windows.Forms.Button btnMove;
        private System.Windows.Forms.TreeView trvTarget;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem tsmDeleteTreeNode;
        private System.Windows.Forms.ImageList imageList1;
    }
}
