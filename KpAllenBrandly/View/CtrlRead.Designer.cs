namespace KpAllenBrandly.View
{
    partial class CtrlRead
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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.btnPLCImport = new System.Windows.Forms.Button();
            this.lblTagCount = new System.Windows.Forms.Label();
            this.chkAllCanWrite = new System.Windows.Forms.CheckBox();
            this.txtMaxAddressLength = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.btnAddRange = new System.Windows.Forms.Button();
            this.chkActive = new System.Windows.Forms.CheckBox();
            this.label4 = new System.Windows.Forms.Label();
            this.txtGroupName = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.dgvTags = new System.Windows.Forms.DataGridView();
            this.dgvTagID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dgvTagName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dgvTagDataType = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dgvTagStringLength = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dgvTagAddress = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dgvTagCanWrite = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.cmsTags = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.deleteTStripItem = new System.Windows.Forms.ToolStripMenuItem();
            this.bdsTags = new System.Windows.Forms.BindingSource(this.components);
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvTags)).BeginInit();
            this.cmsTags.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.bdsTags)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.btnPLCImport);
            this.groupBox1.Controls.Add(this.lblTagCount);
            this.groupBox1.Controls.Add(this.chkAllCanWrite);
            this.groupBox1.Controls.Add(this.txtMaxAddressLength);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.btnAddRange);
            this.groupBox1.Controls.Add(this.chkActive);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.txtGroupName);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.dgvTags);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Location = new System.Drawing.Point(0, 0);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(665, 433);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "参数配置";
            // 
            // btnPLCImport
            // 
            this.btnPLCImport.Location = new System.Drawing.Point(367, 57);
            this.btnPLCImport.Name = "btnPLCImport";
            this.btnPLCImport.Size = new System.Drawing.Size(87, 29);
            this.btnPLCImport.TabIndex = 18;
            this.btnPLCImport.Text = "PLC导入";
            this.btnPLCImport.UseVisualStyleBackColor = true;
            this.btnPLCImport.Click += new System.EventHandler(this.btnPLCImport_Click);
            // 
            // lblTagCount
            // 
            this.lblTagCount.AutoSize = true;
            this.lblTagCount.Location = new System.Drawing.Point(319, 33);
            this.lblTagCount.Name = "lblTagCount";
            this.lblTagCount.Size = new System.Drawing.Size(11, 12);
            this.lblTagCount.TabIndex = 17;
            this.lblTagCount.Text = "0";
            // 
            // chkAllCanWrite
            // 
            this.chkAllCanWrite.AutoSize = true;
            this.chkAllCanWrite.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkAllCanWrite.Location = new System.Drawing.Point(540, 30);
            this.chkAllCanWrite.Name = "chkAllCanWrite";
            this.chkAllCanWrite.Size = new System.Drawing.Size(72, 16);
            this.chkAllCanWrite.TabIndex = 16;
            this.chkAllCanWrite.Text = "全部勾选";
            this.chkAllCanWrite.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.chkAllCanWrite.UseVisualStyleBackColor = true;
            this.chkAllCanWrite.CheckedChanged += new System.EventHandler(this.chkAllCanWrite_CheckedChanged);
            // 
            // txtMaxAddressLength
            // 
            this.txtMaxAddressLength.Location = new System.Drawing.Point(89, 62);
            this.txtMaxAddressLength.Name = "txtMaxAddressLength";
            this.txtMaxAddressLength.Size = new System.Drawing.Size(65, 21);
            this.txtMaxAddressLength.TabIndex = 15;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(30, 59);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 24);
            this.label2.TabIndex = 14;
            this.label2.Text = "最大数据\r\n请求长度";
            // 
            // btnAddRange
            // 
            this.btnAddRange.Location = new System.Drawing.Point(265, 57);
            this.btnAddRange.Name = "btnAddRange";
            this.btnAddRange.Size = new System.Drawing.Size(87, 29);
            this.btnAddRange.TabIndex = 12;
            this.btnAddRange.Text = "批量添加";
            this.btnAddRange.UseVisualStyleBackColor = true;
            this.btnAddRange.Click += new System.EventHandler(this.btnAddRange_Click);
            // 
            // chkActive
            // 
            this.chkActive.AutoSize = true;
            this.chkActive.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkActive.Location = new System.Drawing.Point(414, 29);
            this.chkActive.Name = "chkActive";
            this.chkActive.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.chkActive.Size = new System.Drawing.Size(72, 16);
            this.chkActive.TabIndex = 9;
            this.chkActive.Text = "是否激活";
            this.chkActive.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkActive.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.chkActive.UseVisualStyleBackColor = true;
            this.chkActive.CheckedChanged += new System.EventHandler(this.chkActive_CheckedChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(274, 33);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(41, 12);
            this.label4.TabIndex = 8;
            this.label4.Text = "测点数";
            // 
            // txtGroupName
            // 
            this.txtGroupName.Location = new System.Drawing.Point(89, 27);
            this.txtGroupName.Name = "txtGroupName";
            this.txtGroupName.Size = new System.Drawing.Size(126, 21);
            this.txtGroupName.TabIndex = 6;
            this.txtGroupName.TextChanged += new System.EventHandler(this.TxtGroupName_TextChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(42, 31);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(41, 12);
            this.label3.TabIndex = 5;
            this.label3.Text = "组名称";
            // 
            // dgvTags
            // 
            this.dgvTags.AllowUserToAddRows = false;
            this.dgvTags.AllowUserToDeleteRows = false;
            this.dgvTags.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvTags.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvTags.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dgvTagID,
            this.dgvTagName,
            this.dgvTagDataType,
            this.dgvTagStringLength,
            this.dgvTagAddress,
            this.dgvTagCanWrite});
            this.dgvTags.ContextMenuStrip = this.cmsTags;
            this.dgvTags.Location = new System.Drawing.Point(6, 98);
            this.dgvTags.Name = "dgvTags";
            this.dgvTags.RowTemplate.Height = 23;
            this.dgvTags.Size = new System.Drawing.Size(653, 326);
            this.dgvTags.TabIndex = 2;
            this.dgvTags.CellValidating += new System.Windows.Forms.DataGridViewCellValidatingEventHandler(this.dgvTags_CellValidating);
            this.dgvTags.CurrentCellChanged += new System.EventHandler(this.dgvTags_CurrentCellChanged);
            // 
            // dgvTagID
            // 
            this.dgvTagID.DataPropertyName = "TagID";
            this.dgvTagID.FillWeight = 50F;
            this.dgvTagID.HeaderText = "序号";
            this.dgvTagID.Name = "dgvTagID";
            this.dgvTagID.ReadOnly = true;
            // 
            // dgvTagName
            // 
            this.dgvTagName.DataPropertyName = "Name";
            this.dgvTagName.HeaderText = "名称";
            this.dgvTagName.Name = "dgvTagName";
            // 
            // dgvTagDataType
            // 
            this.dgvTagDataType.DataPropertyName = "DataType";
            this.dgvTagDataType.HeaderText = "数据类型";
            this.dgvTagDataType.Name = "dgvTagDataType";
            this.dgvTagDataType.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvTagDataType.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // dgvTagStringLength
            // 
            this.dgvTagStringLength.DataPropertyName = "Length";
            this.dgvTagStringLength.HeaderText = "数据长度";
            this.dgvTagStringLength.Name = "dgvTagStringLength";
            // 
            // dgvTagAddress
            // 
            this.dgvTagAddress.DataPropertyName = "Address";
            this.dgvTagAddress.HeaderText = "地址";
            this.dgvTagAddress.Name = "dgvTagAddress";
            this.dgvTagAddress.Visible = false;
            // 
            // dgvTagCanWrite
            // 
            this.dgvTagCanWrite.DataPropertyName = "CanWriteBool";
            this.dgvTagCanWrite.FillWeight = 50F;
            this.dgvTagCanWrite.HeaderText = "读写属性";
            this.dgvTagCanWrite.Name = "dgvTagCanWrite";
            this.dgvTagCanWrite.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            // 
            // cmsTags
            // 
            this.cmsTags.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.deleteTStripItem});
            this.cmsTags.Name = "cmsTags";
            this.cmsTags.Size = new System.Drawing.Size(101, 26);
            // 
            // deleteTStripItem
            // 
            this.deleteTStripItem.Name = "deleteTStripItem";
            this.deleteTStripItem.Size = new System.Drawing.Size(100, 22);
            this.deleteTStripItem.Text = "删除";
            this.deleteTStripItem.Click += new System.EventHandler(this.deleteTStripItem_Click);
            // 
            // CtrlRead
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.groupBox1);
            this.Name = "CtrlRead";
            this.Size = new System.Drawing.Size(665, 433);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvTags)).EndInit();
            this.cmsTags.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.bdsTags)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox chkActive;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtGroupName;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.DataGridView dgvTags;
        private System.Windows.Forms.ContextMenuStrip cmsTags;
        private System.Windows.Forms.ToolStripMenuItem deleteTStripItem;
        private System.Windows.Forms.Button btnAddRange;
        private System.Windows.Forms.BindingSource bdsTags;
        private System.Windows.Forms.TextBox txtMaxAddressLength;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckBox chkAllCanWrite;
        private System.Windows.Forms.Label lblTagCount;
        private System.Windows.Forms.DataGridViewTextBoxColumn dgvTagID;
        private System.Windows.Forms.DataGridViewTextBoxColumn dgvTagName;
        private System.Windows.Forms.DataGridViewTextBoxColumn dgvTagDataType;
        private System.Windows.Forms.DataGridViewTextBoxColumn dgvTagStringLength;
        private System.Windows.Forms.DataGridViewTextBoxColumn dgvTagAddress;
        private System.Windows.Forms.DataGridViewCheckBoxColumn dgvTagCanWrite;
        private System.Windows.Forms.Button btnPLCImport;
    }
}
