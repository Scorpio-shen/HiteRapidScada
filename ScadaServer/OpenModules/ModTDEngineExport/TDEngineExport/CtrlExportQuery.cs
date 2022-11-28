using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Scada.Server.Modules.TDEngineExport
{

	internal class CtrlExportQuery : UserControl
	{
		public CtrlExportQuery()
		{
			this.InitializeComponent();
			this.lbExportRemark.Text = "备注：导出通道用半角逗号分隔,连续通道用'-'分隔,例如：100,200-205,300,不填导出全部";
		}

		public bool Export
		{
			get
			{
				return this.chkExport.Checked;
			}
			set
			{
				this.chkExport.CheckedChanged -= this.chkExport_CheckedChanged;
				this.chkExport.Checked = value;
				this.chkExport.CheckedChanged += this.chkExport_CheckedChanged;
				this.SetQueryBackColor();
			}
		}

		public string Query
		{
			get
			{
				return this.txtQuery.Text;
			}
			set
			{
				this.txtQuery.TextChanged -= this.txtQuery_TextChanged;
				this.txtQuery.Text = value;
				this.txtQuery.TextChanged += this.txtQuery_TextChanged;
			}
		}

		public string ExportCnls
		{
			get
			{
				return this.tbExportCnls.Text;
			}
			set
			{
				this.tbExportCnls.TextChanged -= this.tbExportCnls_TextChanged;
				this.tbExportCnls.Text = value;
				this.tbExportCnls.TextChanged += this.tbExportCnls_TextChanged;
			}
		}

		private void SetQueryBackColor()
		{
			this.txtQuery.BackColor = Color.FromKnownColor(this.chkExport.Checked ? KnownColor.Window : KnownColor.Control);
			this.tbExportCnls.BackColor = Color.FromKnownColor(this.chkExport.Checked ? KnownColor.Window : KnownColor.Control);
		}

		private void OnPropChanged()
		{
			if (this.PropChanged != null)
			{
				this.PropChanged(this, EventArgs.Empty);
			}
		}

		[Category("Property Changed")]
		public event EventHandler PropChanged;

		private void CtrlExportQuery_Load(object sender, EventArgs e)
		{
			this.SetQueryBackColor();
		}

		private void chkExport_CheckedChanged(object sender, EventArgs e)
		{
			this.SetQueryBackColor();
			this.OnPropChanged();
		}

		private void txtQuery_TextChanged(object sender, EventArgs e)
		{
			this.Export = (this.txtQuery.Text != "");
			this.OnPropChanged();
		}

		private void tbExportCnls_TextChanged(object sender, EventArgs e)
		{
			this.OnPropChanged();
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
			this.txtQuery = new TextBox();
			this.lblQuery = new Label();
			this.chkExport = new CheckBox();
			this.tbExportCnls = new TextBox();
			this.lbExportRemark = new Label();
			this.lbExportCnls = new Label();
			base.SuspendLayout();
			this.txtQuery.Location = new Point(0, 33);
			this.txtQuery.Multiline = true;
			this.txtQuery.Name = "txtQuery";
			this.txtQuery.ScrollBars = ScrollBars.Both;
			this.txtQuery.Size = new Size(465, 184);
			this.txtQuery.TabIndex = 2;
			this.txtQuery.WordWrap = false;
			this.txtQuery.TextChanged += this.txtQuery_TextChanged;
			this.lblQuery.AutoSize = true;
			this.lblQuery.Location = new Point(-3, 18);
			this.lblQuery.Name = "lblQuery";
			this.lblQuery.Size = new Size(23, 12);
			this.lblQuery.TabIndex = 1;
			this.lblQuery.Text = "SQL";
			this.chkExport.AutoSize = true;
			this.chkExport.Location = new Point(0, 0);
			this.chkExport.Name = "chkExport";
			this.chkExport.Size = new Size(48, 16);
			this.chkExport.TabIndex = 0;
			this.chkExport.Text = "导出";
			this.chkExport.UseVisualStyleBackColor = true;
			this.chkExport.CheckedChanged += this.chkExport_CheckedChanged;
			this.tbExportCnls.Location = new Point(0, 239);
			this.tbExportCnls.Multiline = true;
			this.tbExportCnls.Name = "tbExportCnls";
			this.tbExportCnls.Size = new Size(465, 69);
			this.tbExportCnls.TabIndex = 3;
			this.tbExportCnls.TextChanged += this.tbExportCnls_TextChanged;
			this.lbExportRemark.AutoSize = true;
			this.lbExportRemark.Font = new Font("宋体", 8f, FontStyle.Regular, GraphicsUnit.Point, 134);
			this.lbExportRemark.Location = new Point(4, 315);
			this.lbExportRemark.Name = "lbExportRemark";
			this.lbExportRemark.Size = new Size(27, 11);
			this.lbExportRemark.TabIndex = 4;
			this.lbExportRemark.Text = "备注";
			this.lbExportCnls.AutoSize = true;
			this.lbExportCnls.Location = new Point(3, 224);
			this.lbExportCnls.Name = "lbExportCnls";
			this.lbExportCnls.Size = new Size(53, 12);
			this.lbExportCnls.TabIndex = 5;
			this.lbExportCnls.Text = "导出通道";
			base.AutoScaleDimensions = new SizeF(6f, 12f);
			base.AutoScaleMode = AutoScaleMode.Font;
			base.Controls.Add(this.lbExportCnls);
			base.Controls.Add(this.lbExportRemark);
			base.Controls.Add(this.tbExportCnls);
			base.Controls.Add(this.txtQuery);
			base.Controls.Add(this.lblQuery);
			base.Controls.Add(this.chkExport);
			base.Name = "CtrlExportQuery";
			base.Size = new Size(465, 328);
			base.Load += this.CtrlExportQuery_Load;
			base.ResumeLayout(false);
			base.PerformLayout();
		}


		private IContainer components;

		private TextBox txtQuery;

		private Label lblQuery;

		private CheckBox chkExport;

		private TextBox tbExportCnls;

		private Label lbExportRemark;

		private Label lbExportCnls;
	}
}
