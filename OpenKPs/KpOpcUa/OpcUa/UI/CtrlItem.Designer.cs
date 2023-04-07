namespace Scada.Comm.Devices.OpcUa.UI
{
    partial class CtrlItem
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.gbItem = new System.Windows.Forms.GroupBox();
            this.chkCanWrite = new System.Windows.Forms.CheckBox();
            this.txtSignal = new System.Windows.Forms.TextBox();
            this.lblSignal = new System.Windows.Forms.Label();
            this.numArrayLen = new System.Windows.Forms.NumericUpDown();
            this.lblArrayLen = new System.Windows.Forms.Label();
            this.chkIsArray = new System.Windows.Forms.CheckBox();
            this.txtNodeID = new System.Windows.Forms.TextBox();
            this.lblNodeID = new System.Windows.Forms.Label();
            this.txtDisplayName = new System.Windows.Forms.TextBox();
            this.lblDisplayName = new System.Windows.Forms.Label();
            this.chkItemActive = new System.Windows.Forms.CheckBox();
            this.gbItem.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numArrayLen)).BeginInit();
            this.SuspendLayout();
            // 
            // gbItem
            // 
            this.gbItem.Controls.Add(this.chkCanWrite);
            this.gbItem.Controls.Add(this.txtSignal);
            this.gbItem.Controls.Add(this.lblSignal);
            this.gbItem.Controls.Add(this.numArrayLen);
            this.gbItem.Controls.Add(this.lblArrayLen);
            this.gbItem.Controls.Add(this.chkIsArray);
            this.gbItem.Controls.Add(this.txtNodeID);
            this.gbItem.Controls.Add(this.lblNodeID);
            this.gbItem.Controls.Add(this.txtDisplayName);
            this.gbItem.Controls.Add(this.lblDisplayName);
            this.gbItem.Controls.Add(this.chkItemActive);
            this.gbItem.Location = new System.Drawing.Point(0, 0);
            this.gbItem.Name = "gbItem";
            this.gbItem.Padding = new System.Windows.Forms.Padding(10, 3, 10, 9);
            this.gbItem.Size = new System.Drawing.Size(230, 376);
            this.gbItem.TabIndex = 0;
            this.gbItem.TabStop = false;
            this.gbItem.Text = "参数";
            // 
            // chkCanWrite
            // 
            this.chkCanWrite.AutoSize = true;
            this.chkCanWrite.Location = new System.Drawing.Point(13, 133);
            this.chkCanWrite.Name = "chkCanWrite";
            this.chkCanWrite.Size = new System.Drawing.Size(72, 16);
            this.chkCanWrite.TabIndex = 12;
            this.chkCanWrite.Text = "是否可写";
            this.chkCanWrite.UseVisualStyleBackColor = true;
            this.chkCanWrite.CheckedChanged += new System.EventHandler(this.chkCanWrite_CheckedChanged);
            // 
            // txtSignal
            // 
            this.txtSignal.Location = new System.Drawing.Point(13, 247);
            this.txtSignal.Name = "txtSignal";
            this.txtSignal.ReadOnly = true;
            this.txtSignal.Size = new System.Drawing.Size(100, 21);
            this.txtSignal.TabIndex = 11;
            this.txtSignal.Visible = false;
            // 
            // lblSignal
            // 
            this.lblSignal.AutoSize = true;
            this.lblSignal.Location = new System.Drawing.Point(10, 232);
            this.lblSignal.Name = "lblSignal";
            this.lblSignal.Size = new System.Drawing.Size(41, 12);
            this.lblSignal.TabIndex = 10;
            this.lblSignal.Text = "Signal";
            this.lblSignal.Visible = false;
            // 
            // numArrayLen
            // 
            this.numArrayLen.Location = new System.Drawing.Point(13, 180);
            this.numArrayLen.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.numArrayLen.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numArrayLen.Name = "numArrayLen";
            this.numArrayLen.Size = new System.Drawing.Size(100, 21);
            this.numArrayLen.TabIndex = 7;
            this.numArrayLen.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numArrayLen.ValueChanged += new System.EventHandler(this.numArrayLen_ValueChanged);
            // 
            // lblArrayLen
            // 
            this.lblArrayLen.AutoSize = true;
            this.lblArrayLen.Location = new System.Drawing.Point(10, 165);
            this.lblArrayLen.Name = "lblArrayLen";
            this.lblArrayLen.Size = new System.Drawing.Size(53, 12);
            this.lblArrayLen.TabIndex = 6;
            this.lblArrayLen.Text = "数组长度";
            // 
            // chkIsArray
            // 
            this.chkIsArray.AutoSize = true;
            this.chkIsArray.Location = new System.Drawing.Point(13, 111);
            this.chkIsArray.Name = "chkIsArray";
            this.chkIsArray.Size = new System.Drawing.Size(84, 16);
            this.chkIsArray.TabIndex = 5;
            this.chkIsArray.Text = "是否是数组";
            this.chkIsArray.UseVisualStyleBackColor = true;
            this.chkIsArray.CheckedChanged += new System.EventHandler(this.chkIsArray_CheckedChanged);
            // 
            // txtNodeID
            // 
            this.txtNodeID.Location = new System.Drawing.Point(13, 87);
            this.txtNodeID.Name = "txtNodeID";
            this.txtNodeID.ReadOnly = true;
            this.txtNodeID.Size = new System.Drawing.Size(204, 21);
            this.txtNodeID.TabIndex = 4;
            // 
            // lblNodeID
            // 
            this.lblNodeID.AutoSize = true;
            this.lblNodeID.Location = new System.Drawing.Point(10, 72);
            this.lblNodeID.Name = "lblNodeID";
            this.lblNodeID.Size = new System.Drawing.Size(47, 12);
            this.lblNodeID.TabIndex = 3;
            this.lblNodeID.Text = "Node ID";
            // 
            // txtDisplayName
            // 
            this.txtDisplayName.Location = new System.Drawing.Point(13, 51);
            this.txtDisplayName.Name = "txtDisplayName";
            this.txtDisplayName.Size = new System.Drawing.Size(204, 21);
            this.txtDisplayName.TabIndex = 2;
            this.txtDisplayName.TextChanged += new System.EventHandler(this.txtDisplayName_TextChanged);
            // 
            // lblDisplayName
            // 
            this.lblDisplayName.AutoSize = true;
            this.lblDisplayName.Location = new System.Drawing.Point(10, 36);
            this.lblDisplayName.Name = "lblDisplayName";
            this.lblDisplayName.Size = new System.Drawing.Size(29, 12);
            this.lblDisplayName.TabIndex = 1;
            this.lblDisplayName.Text = "名称";
            // 
            // chkItemActive
            // 
            this.chkItemActive.AutoSize = true;
            this.chkItemActive.Location = new System.Drawing.Point(13, 18);
            this.chkItemActive.Name = "chkItemActive";
            this.chkItemActive.Size = new System.Drawing.Size(72, 16);
            this.chkItemActive.TabIndex = 0;
            this.chkItemActive.Text = "是否激活";
            this.chkItemActive.UseVisualStyleBackColor = true;
            this.chkItemActive.CheckedChanged += new System.EventHandler(this.chkItemActive_CheckedChanged);
            // 
            // CtrlItem
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.gbItem);
            this.Name = "CtrlItem";
            this.Size = new System.Drawing.Size(230, 376);
            this.gbItem.ResumeLayout(false);
            this.gbItem.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numArrayLen)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox gbItem;
        private System.Windows.Forms.TextBox txtDisplayName;
        private System.Windows.Forms.Label lblDisplayName;
        private System.Windows.Forms.CheckBox chkItemActive;
        private System.Windows.Forms.TextBox txtNodeID;
        private System.Windows.Forms.Label lblNodeID;
        private System.Windows.Forms.CheckBox chkIsArray;
        private System.Windows.Forms.Label lblArrayLen;
        private System.Windows.Forms.NumericUpDown numArrayLen;
        private System.Windows.Forms.Label lblSignal;
        private System.Windows.Forms.TextBox txtSignal;
        private System.Windows.Forms.CheckBox chkCanWrite;
    }
}
