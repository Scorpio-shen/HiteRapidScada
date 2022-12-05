namespace KpSiemens.Siemens.View
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
            this.lblTagCount = new System.Windows.Forms.Label();
            this.txtMaxAddressLength = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.chkAllCanWrite = new System.Windows.Forms.CheckBox();
            this.btnAddRange = new System.Windows.Forms.Button();
            this.lblDbNum = new System.Windows.Forms.Label();
            this.numDbNum = new System.Windows.Forms.NumericUpDown();
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
            this.cbxMemoryType = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.bdsTags = new System.Windows.Forms.BindingSource(this.components);
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numDbNum)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvTags)).BeginInit();
            this.cmsTags.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.bdsTags)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.lblTagCount);
            this.groupBox1.Controls.Add(this.txtMaxAddressLength);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.chkAllCanWrite);
            this.groupBox1.Controls.Add(this.btnAddRange);
            this.groupBox1.Controls.Add(this.lblDbNum);
            this.groupBox1.Controls.Add(this.numDbNum);
            this.groupBox1.Controls.Add(this.chkActive);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.txtGroupName);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.dgvTags);
            this.groupBox1.Controls.Add(this.cbxMemoryType);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Location = new System.Drawing.Point(0, 0);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(654, 433);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "参数配置";
            // 
            // lblTagCount
            // 
            this.lblTagCount.AutoSize = true;
            this.lblTagCount.Location = new System.Drawing.Point(301, 68);
            this.lblTagCount.Name = "lblTagCount";
            this.lblTagCount.Size = new System.Drawing.Size(11, 12);
            this.lblTagCount.TabIndex = 16;
            this.lblTagCount.Text = "0";
            // 
            // txtMaxAddressLength
            // 
            this.txtMaxAddressLength.Enabled = false;
            this.txtMaxAddressLength.Location = new System.Drawing.Point(470, 64);
            this.txtMaxAddressLength.Name = "txtMaxAddressLength";
            this.txtMaxAddressLength.Size = new System.Drawing.Size(65, 21);
            this.txtMaxAddressLength.TabIndex = 15;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(411, 62);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 24);
            this.label2.TabIndex = 14;
            this.label2.Text = "最大数据\r\n请求长度";
            // 
            // chkAllCanWrite
            // 
            this.chkAllCanWrite.AutoSize = true;
            this.chkAllCanWrite.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkAllCanWrite.Location = new System.Drawing.Point(552, 37);
            this.chkAllCanWrite.Name = "chkAllCanWrite";
            this.chkAllCanWrite.Size = new System.Drawing.Size(72, 16);
            this.chkAllCanWrite.TabIndex = 13;
            this.chkAllCanWrite.Text = "全部勾选";
            this.chkAllCanWrite.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.chkAllCanWrite.UseVisualStyleBackColor = true;
            this.chkAllCanWrite.CheckedChanged += new System.EventHandler(this.chkAllCanWrite_CheckedChanged);
            // 
            // btnAddRange
            // 
            this.btnAddRange.Location = new System.Drawing.Point(552, 60);
            this.btnAddRange.Name = "btnAddRange";
            this.btnAddRange.Size = new System.Drawing.Size(87, 29);
            this.btnAddRange.TabIndex = 12;
            this.btnAddRange.Text = "批量添加";
            this.btnAddRange.UseVisualStyleBackColor = true;
            this.btnAddRange.Click += new System.EventHandler(this.btnAddRange_Click);
            // 
            // lblDbNum
            // 
            this.lblDbNum.AutoSize = true;
            this.lblDbNum.Location = new System.Drawing.Point(244, 40);
            this.lblDbNum.Name = "lblDbNum";
            this.lblDbNum.Size = new System.Drawing.Size(53, 12);
            this.lblDbNum.TabIndex = 11;
            this.lblDbNum.Text = "DB块号码";
            // 
            // numDbNum
            // 
            this.numDbNum.Location = new System.Drawing.Point(303, 35);
            this.numDbNum.Name = "numDbNum";
            this.numDbNum.Size = new System.Drawing.Size(76, 21);
            this.numDbNum.TabIndex = 10;
            this.numDbNum.ValueChanged += new System.EventHandler(this.numDbNum_ValueChanged);
            // 
            // chkActive
            // 
            this.chkActive.AutoSize = true;
            this.chkActive.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkActive.Location = new System.Drawing.Point(414, 39);
            this.chkActive.Name = "chkActive";
            this.chkActive.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.chkActive.Size = new System.Drawing.Size(72, 16);
            this.chkActive.TabIndex = 9;
            this.chkActive.Text = "是否激活";
            this.chkActive.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkActive.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.chkActive.UseVisualStyleBackColor = true;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(256, 68);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(41, 12);
            this.label4.TabIndex = 8;
            this.label4.Text = "测点数";
            // 
            // txtGroupName
            // 
            this.txtGroupName.Location = new System.Drawing.Point(89, 35);
            this.txtGroupName.Name = "txtGroupName";
            this.txtGroupName.Size = new System.Drawing.Size(126, 21);
            this.txtGroupName.TabIndex = 6;
            this.txtGroupName.TextChanged += new System.EventHandler(this.TxtGroupName_TextChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(42, 38);
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
            this.dgvTags.Size = new System.Drawing.Size(642, 326);
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
            // 
            // dgvTagCanWrite
            // 
            this.dgvTagCanWrite.DataPropertyName = "CanWriteBool";
            this.dgvTagCanWrite.FillWeight = 50F;
            this.dgvTagCanWrite.HeaderText = "支持写入";
            this.dgvTagCanWrite.Name = "dgvTagCanWrite";
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
            // cbxMemoryType
            // 
            this.cbxMemoryType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbxMemoryType.FormattingEnabled = true;
            this.cbxMemoryType.Location = new System.Drawing.Point(89, 62);
            this.cbxMemoryType.Name = "cbxMemoryType";
            this.cbxMemoryType.Size = new System.Drawing.Size(126, 20);
            this.cbxMemoryType.TabIndex = 1;
            this.cbxMemoryType.SelectedIndexChanged += new System.EventHandler(this.cbxMemoryType_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(18, 66);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(65, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "存储器选择";
            // 
            // CtrlRead
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.groupBox1);
            this.Name = "CtrlRead";
            this.Size = new System.Drawing.Size(654, 433);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numDbNum)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvTags)).EndInit();
            this.cmsTags.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.bdsTags)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label lblDbNum;
        private System.Windows.Forms.NumericUpDown numDbNum;
        private System.Windows.Forms.CheckBox chkActive;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtGroupName;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.DataGridView dgvTags;
        private System.Windows.Forms.ComboBox cbxMemoryType;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ContextMenuStrip cmsTags;
        private System.Windows.Forms.ToolStripMenuItem deleteTStripItem;
        private System.Windows.Forms.Button btnAddRange;
        private System.Windows.Forms.CheckBox chkAllCanWrite;
        private System.Windows.Forms.DataGridViewTextBoxColumn dgvTagID;
        private System.Windows.Forms.DataGridViewTextBoxColumn dgvTagName;
        private System.Windows.Forms.DataGridViewTextBoxColumn dgvTagDataType;
        private System.Windows.Forms.DataGridViewTextBoxColumn dgvTagStringLength;
        private System.Windows.Forms.DataGridViewTextBoxColumn dgvTagAddress;
        private System.Windows.Forms.DataGridViewCheckBoxColumn dgvTagCanWrite;
        private System.Windows.Forms.BindingSource bdsTags;
        private System.Windows.Forms.TextBox txtMaxAddressLength;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label lblTagCount;
    }
}
