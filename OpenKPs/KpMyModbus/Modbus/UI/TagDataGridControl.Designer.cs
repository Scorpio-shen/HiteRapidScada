namespace KpMyModbus.Modbus.UI
{
    partial class TagDataGridControl
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
            this.dgvTagItems = new System.Windows.Forms.DataGridView();
            this.numTagCount = new System.Windows.Forms.NumericUpDown();
            this.numStartAddress = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.bdsTags = new System.Windows.Forms.BindingSource(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.dgvTagItems)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numTagCount)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numStartAddress)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.bdsTags)).BeginInit();
            this.SuspendLayout();
            // 
            // dgvTagItems
            // 
            this.dgvTagItems.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvTagItems.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvTagItems.Location = new System.Drawing.Point(3, 74);
            this.dgvTagItems.Name = "dgvTagItems";
            this.dgvTagItems.RowTemplate.Height = 23;
            this.dgvTagItems.Size = new System.Drawing.Size(470, 298);
            this.dgvTagItems.TabIndex = 0;
            // 
            // numTagCount
            // 
            this.numTagCount.Location = new System.Drawing.Point(172, 47);
            this.numTagCount.Name = "numTagCount";
            this.numTagCount.Size = new System.Drawing.Size(141, 21);
            this.numTagCount.TabIndex = 1;
            this.numTagCount.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // numStartAddress
            // 
            this.numStartAddress.Location = new System.Drawing.Point(3, 47);
            this.numStartAddress.Name = "numStartAddress";
            this.numStartAddress.Size = new System.Drawing.Size(142, 21);
            this.numStartAddress.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 32);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(53, 12);
            this.label1.TabIndex = 3;
            this.label1.Text = "起始地址";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(170, 32);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 12);
            this.label2.TabIndex = 4;
            this.label2.Text = "通讯点数";
            // 
            // TagDataGridControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.numStartAddress);
            this.Controls.Add(this.numTagCount);
            this.Controls.Add(this.dgvTagItems);
            this.Name = "TagDataGridControl";
            this.Size = new System.Drawing.Size(476, 375);
            ((System.ComponentModel.ISupportInitialize)(this.dgvTagItems)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numTagCount)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numStartAddress)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.bdsTags)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataGridView dgvTagItems;
        private System.Windows.Forms.NumericUpDown numTagCount;
        private System.Windows.Forms.NumericUpDown numStartAddress;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.BindingSource bdsTags;
    }
}
