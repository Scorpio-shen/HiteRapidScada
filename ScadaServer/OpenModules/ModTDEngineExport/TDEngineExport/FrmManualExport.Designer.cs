namespace Scada.Server.Modules.TDEngineExport
{
	internal partial class FrmManualExport : global::System.Windows.Forms.Form
	{
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
            this.cbDataSource = new System.Windows.Forms.ComboBox();
            this.lblDataSource = new System.Windows.Forms.Label();
            this.gbCurData = new System.Windows.Forms.GroupBox();
            this.numCurDataCtrlCnlNum = new System.Windows.Forms.NumericUpDown();
            this.lblCurDataCtrlCnlNum = new System.Windows.Forms.Label();
            this.btnExportCurData = new System.Windows.Forms.Button();
            this.gbArcData = new System.Windows.Forms.GroupBox();
            this.lblArcDataDateTime = new System.Windows.Forms.Label();
            this.dtpArcDataTime = new System.Windows.Forms.DateTimePicker();
            this.dtpArcDataDate = new System.Windows.Forms.DateTimePicker();
            this.numArcDataCtrlCnlNum = new System.Windows.Forms.NumericUpDown();
            this.lblArcDataCtrlCnlNum = new System.Windows.Forms.Label();
            this.btnExportArcData = new System.Windows.Forms.Button();
            this.gbEvents = new System.Windows.Forms.GroupBox();
            this.lblEventsDate = new System.Windows.Forms.Label();
            this.dtpEventsDate = new System.Windows.Forms.DateTimePicker();
            this.numEventsCtrlCnlNum = new System.Windows.Forms.NumericUpDown();
            this.lblEventsCtrlCnlNum = new System.Windows.Forms.Label();
            this.btnExportEvents = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOK = new System.Windows.Forms.Button();
            this.gbCurData.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numCurDataCtrlCnlNum)).BeginInit();
            this.gbArcData.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numArcDataCtrlCnlNum)).BeginInit();
            this.gbEvents.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numEventsCtrlCnlNum)).BeginInit();
            this.SuspendLayout();
            // 
            // cbDataSource
            // 
            this.cbDataSource.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbDataSource.FormattingEnabled = true;
            this.cbDataSource.Location = new System.Drawing.Point(24, 46);
            this.cbDataSource.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.cbDataSource.Name = "cbDataSource";
            this.cbDataSource.Size = new System.Drawing.Size(744, 32);
            this.cbDataSource.TabIndex = 1;
            // 
            // lblDataSource
            // 
            this.lblDataSource.AutoSize = true;
            this.lblDataSource.Location = new System.Drawing.Point(18, 16);
            this.lblDataSource.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.lblDataSource.Name = "lblDataSource";
            this.lblDataSource.Size = new System.Drawing.Size(82, 24);
            this.lblDataSource.TabIndex = 0;
            this.lblDataSource.Text = "数据源";
            // 
            // gbCurData
            // 
            this.gbCurData.Controls.Add(this.numCurDataCtrlCnlNum);
            this.gbCurData.Controls.Add(this.lblCurDataCtrlCnlNum);
            this.gbCurData.Controls.Add(this.btnExportCurData);
            this.gbCurData.Location = new System.Drawing.Point(24, 96);
            this.gbCurData.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.gbCurData.Name = "gbCurData";
            this.gbCurData.Padding = new System.Windows.Forms.Padding(20, 6, 20, 18);
            this.gbCurData.Size = new System.Drawing.Size(748, 124);
            this.gbCurData.TabIndex = 2;
            this.gbCurData.TabStop = false;
            this.gbCurData.Text = "当前数据";
            // 
            // numCurDataCtrlCnlNum
            // 
            this.numCurDataCtrlCnlNum.Location = new System.Drawing.Point(26, 60);
            this.numCurDataCtrlCnlNum.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.numCurDataCtrlCnlNum.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.numCurDataCtrlCnlNum.Name = "numCurDataCtrlCnlNum";
            this.numCurDataCtrlCnlNum.Size = new System.Drawing.Size(140, 35);
            this.numCurDataCtrlCnlNum.TabIndex = 1;
            this.numCurDataCtrlCnlNum.ValueChanged += new System.EventHandler(this.numCurDataCtrlCnlNum_ValueChanged);
            // 
            // lblCurDataCtrlCnlNum
            // 
            this.lblCurDataCtrlCnlNum.AutoSize = true;
            this.lblCurDataCtrlCnlNum.Location = new System.Drawing.Point(20, 30);
            this.lblCurDataCtrlCnlNum.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.lblCurDataCtrlCnlNum.Name = "lblCurDataCtrlCnlNum";
            this.lblCurDataCtrlCnlNum.Size = new System.Drawing.Size(106, 24);
            this.lblCurDataCtrlCnlNum.TabIndex = 0;
            this.lblCurDataCtrlCnlNum.Text = "输出通道";
            // 
            // btnExportCurData
            // 
            this.btnExportCurData.Location = new System.Drawing.Point(178, 58);
            this.btnExportCurData.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.btnExportCurData.Name = "btnExportCurData";
            this.btnExportCurData.Size = new System.Drawing.Size(150, 42);
            this.btnExportCurData.TabIndex = 2;
            this.btnExportCurData.Text = "导出";
            this.btnExportCurData.UseVisualStyleBackColor = true;
            this.btnExportCurData.Click += new System.EventHandler(this.btnExport_Click);
            // 
            // gbArcData
            // 
            this.gbArcData.Controls.Add(this.lblArcDataDateTime);
            this.gbArcData.Controls.Add(this.dtpArcDataTime);
            this.gbArcData.Controls.Add(this.dtpArcDataDate);
            this.gbArcData.Controls.Add(this.numArcDataCtrlCnlNum);
            this.gbArcData.Controls.Add(this.lblArcDataCtrlCnlNum);
            this.gbArcData.Controls.Add(this.btnExportArcData);
            this.gbArcData.Location = new System.Drawing.Point(24, 230);
            this.gbArcData.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.gbArcData.Name = "gbArcData";
            this.gbArcData.Padding = new System.Windows.Forms.Padding(20, 6, 20, 18);
            this.gbArcData.Size = new System.Drawing.Size(748, 124);
            this.gbArcData.TabIndex = 3;
            this.gbArcData.TabStop = false;
            this.gbArcData.Text = "存档数据";
            // 
            // lblArcDataDateTime
            // 
            this.lblArcDataDateTime.AutoSize = true;
            this.lblArcDataDateTime.Location = new System.Drawing.Point(172, 30);
            this.lblArcDataDateTime.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.lblArcDataDateTime.Name = "lblArcDataDateTime";
            this.lblArcDataDateTime.Size = new System.Drawing.Size(130, 24);
            this.lblArcDataDateTime.TabIndex = 2;
            this.lblArcDataDateTime.Text = "日期和时间";
            // 
            // dtpArcDataTime
            // 
            this.dtpArcDataTime.CustomFormat = "";
            this.dtpArcDataTime.Format = System.Windows.Forms.DateTimePickerFormat.Time;
            this.dtpArcDataTime.Location = new System.Drawing.Point(390, 60);
            this.dtpArcDataTime.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.dtpArcDataTime.MaxDate = new System.DateTime(2100, 12, 31, 0, 0, 0, 0);
            this.dtpArcDataTime.MinDate = new System.DateTime(1990, 1, 1, 0, 0, 0, 0);
            this.dtpArcDataTime.Name = "dtpArcDataTime";
            this.dtpArcDataTime.ShowUpDown = true;
            this.dtpArcDataTime.Size = new System.Drawing.Size(166, 35);
            this.dtpArcDataTime.TabIndex = 4;
            // 
            // dtpArcDataDate
            // 
            this.dtpArcDataDate.CustomFormat = "";
            this.dtpArcDataDate.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dtpArcDataDate.Location = new System.Drawing.Point(178, 60);
            this.dtpArcDataDate.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.dtpArcDataDate.Name = "dtpArcDataDate";
            this.dtpArcDataDate.Size = new System.Drawing.Size(196, 35);
            this.dtpArcDataDate.TabIndex = 3;
            // 
            // numArcDataCtrlCnlNum
            // 
            this.numArcDataCtrlCnlNum.Location = new System.Drawing.Point(26, 60);
            this.numArcDataCtrlCnlNum.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.numArcDataCtrlCnlNum.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.numArcDataCtrlCnlNum.Name = "numArcDataCtrlCnlNum";
            this.numArcDataCtrlCnlNum.Size = new System.Drawing.Size(140, 35);
            this.numArcDataCtrlCnlNum.TabIndex = 1;
            this.numArcDataCtrlCnlNum.ValueChanged += new System.EventHandler(this.numArcDataCtrlCnlNum_ValueChanged);
            // 
            // lblArcDataCtrlCnlNum
            // 
            this.lblArcDataCtrlCnlNum.AutoSize = true;
            this.lblArcDataCtrlCnlNum.Location = new System.Drawing.Point(20, 30);
            this.lblArcDataCtrlCnlNum.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.lblArcDataCtrlCnlNum.Name = "lblArcDataCtrlCnlNum";
            this.lblArcDataCtrlCnlNum.Size = new System.Drawing.Size(106, 24);
            this.lblArcDataCtrlCnlNum.TabIndex = 0;
            this.lblArcDataCtrlCnlNum.Text = "输出通道";
            // 
            // btnExportArcData
            // 
            this.btnExportArcData.Location = new System.Drawing.Point(572, 58);
            this.btnExportArcData.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.btnExportArcData.Name = "btnExportArcData";
            this.btnExportArcData.Size = new System.Drawing.Size(150, 42);
            this.btnExportArcData.TabIndex = 5;
            this.btnExportArcData.Text = "导出";
            this.btnExportArcData.UseVisualStyleBackColor = true;
            this.btnExportArcData.Click += new System.EventHandler(this.btnExport_Click);
            // 
            // gbEvents
            // 
            this.gbEvents.Controls.Add(this.lblEventsDate);
            this.gbEvents.Controls.Add(this.dtpEventsDate);
            this.gbEvents.Controls.Add(this.numEventsCtrlCnlNum);
            this.gbEvents.Controls.Add(this.lblEventsCtrlCnlNum);
            this.gbEvents.Controls.Add(this.btnExportEvents);
            this.gbEvents.Location = new System.Drawing.Point(24, 366);
            this.gbEvents.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.gbEvents.Name = "gbEvents";
            this.gbEvents.Padding = new System.Windows.Forms.Padding(20, 6, 20, 18);
            this.gbEvents.Size = new System.Drawing.Size(748, 124);
            this.gbEvents.TabIndex = 4;
            this.gbEvents.TabStop = false;
            this.gbEvents.Text = "事件";
            // 
            // lblEventsDate
            // 
            this.lblEventsDate.AutoSize = true;
            this.lblEventsDate.Location = new System.Drawing.Point(172, 30);
            this.lblEventsDate.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.lblEventsDate.Name = "lblEventsDate";
            this.lblEventsDate.Size = new System.Drawing.Size(58, 24);
            this.lblEventsDate.TabIndex = 2;
            this.lblEventsDate.Text = "日期";
            // 
            // dtpEventsDate
            // 
            this.dtpEventsDate.CustomFormat = "";
            this.dtpEventsDate.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dtpEventsDate.Location = new System.Drawing.Point(178, 60);
            this.dtpEventsDate.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.dtpEventsDate.MaxDate = new System.DateTime(2100, 12, 31, 0, 0, 0, 0);
            this.dtpEventsDate.MinDate = new System.DateTime(1990, 1, 1, 0, 0, 0, 0);
            this.dtpEventsDate.Name = "dtpEventsDate";
            this.dtpEventsDate.Size = new System.Drawing.Size(196, 35);
            this.dtpEventsDate.TabIndex = 3;
            // 
            // numEventsCtrlCnlNum
            // 
            this.numEventsCtrlCnlNum.Location = new System.Drawing.Point(26, 60);
            this.numEventsCtrlCnlNum.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.numEventsCtrlCnlNum.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.numEventsCtrlCnlNum.Name = "numEventsCtrlCnlNum";
            this.numEventsCtrlCnlNum.Size = new System.Drawing.Size(140, 35);
            this.numEventsCtrlCnlNum.TabIndex = 1;
            this.numEventsCtrlCnlNum.ValueChanged += new System.EventHandler(this.numEventsCtrlCnlNum_ValueChanged);
            // 
            // lblEventsCtrlCnlNum
            // 
            this.lblEventsCtrlCnlNum.AutoSize = true;
            this.lblEventsCtrlCnlNum.Location = new System.Drawing.Point(20, 30);
            this.lblEventsCtrlCnlNum.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.lblEventsCtrlCnlNum.Name = "lblEventsCtrlCnlNum";
            this.lblEventsCtrlCnlNum.Size = new System.Drawing.Size(106, 24);
            this.lblEventsCtrlCnlNum.TabIndex = 0;
            this.lblEventsCtrlCnlNum.Text = "输出通道";
            // 
            // btnExportEvents
            // 
            this.btnExportEvents.Location = new System.Drawing.Point(390, 58);
            this.btnExportEvents.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.btnExportEvents.Name = "btnExportEvents";
            this.btnExportEvents.Size = new System.Drawing.Size(150, 42);
            this.btnExportEvents.TabIndex = 4;
            this.btnExportEvents.Text = "导出";
            this.btnExportEvents.UseVisualStyleBackColor = true;
            this.btnExportEvents.Click += new System.EventHandler(this.btnExport_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(622, 500);
            this.btnCancel.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(150, 42);
            this.btnCancel.TabIndex = 6;
            this.btnCancel.Text = "取消";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // btnOK
            // 
            this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOK.Location = new System.Drawing.Point(460, 500);
            this.btnOK.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(150, 42);
            this.btnOK.TabIndex = 5;
            this.btnOK.Text = "确认";
            this.btnOK.UseVisualStyleBackColor = true;
            // 
            // FrmManualExport
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 24F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(796, 564);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.gbEvents);
            this.Controls.Add(this.gbArcData);
            this.Controls.Add(this.gbCurData);
            this.Controls.Add(this.lblDataSource);
            this.Controls.Add(this.cbDataSource);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FrmManualExport";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "手动导出";
            this.Load += new System.EventHandler(this.FrmManualExport_Load);
            this.gbCurData.ResumeLayout(false);
            this.gbCurData.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numCurDataCtrlCnlNum)).EndInit();
            this.gbArcData.ResumeLayout(false);
            this.gbArcData.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numArcDataCtrlCnlNum)).EndInit();
            this.gbEvents.ResumeLayout(false);
            this.gbEvents.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numEventsCtrlCnlNum)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		private global::System.ComponentModel.IContainer components;

		private global::System.Windows.Forms.ComboBox cbDataSource;

		private global::System.Windows.Forms.Label lblDataSource;

		private global::System.Windows.Forms.GroupBox gbCurData;

		private global::System.Windows.Forms.Button btnExportCurData;

		private global::System.Windows.Forms.NumericUpDown numCurDataCtrlCnlNum;

		private global::System.Windows.Forms.Label lblCurDataCtrlCnlNum;

		private global::System.Windows.Forms.GroupBox gbArcData;

		private global::System.Windows.Forms.NumericUpDown numArcDataCtrlCnlNum;

		private global::System.Windows.Forms.Label lblArcDataCtrlCnlNum;

		private global::System.Windows.Forms.Button btnExportArcData;

		private global::System.Windows.Forms.DateTimePicker dtpArcDataTime;

		private global::System.Windows.Forms.DateTimePicker dtpArcDataDate;

		private global::System.Windows.Forms.Label lblArcDataDateTime;

		private global::System.Windows.Forms.GroupBox gbEvents;

		private global::System.Windows.Forms.DateTimePicker dtpEventsDate;

		private global::System.Windows.Forms.NumericUpDown numEventsCtrlCnlNum;

		private global::System.Windows.Forms.Label lblEventsCtrlCnlNum;

		private global::System.Windows.Forms.Button btnExportEvents;

		private global::System.Windows.Forms.Label lblEventsDate;

		private global::System.Windows.Forms.Button btnCancel;

		private global::System.Windows.Forms.Button btnOK;
	}
}
