﻿namespace Scada.Comm.Devices.rttask.UI
{
    partial class FrmDevProps
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmDevProps));
            this.gbDevice = new System.Windows.Forms.GroupBox();
            this.btnBrowseDevTemplate = new System.Windows.Forms.Button();
            this.btnCreateDevTemplate = new System.Windows.Forms.Button();
            this.btnEditDevTemplate = new System.Windows.Forms.Button();
            this.txtDevTemplate = new System.Windows.Forms.TextBox();
            this.lblDevTemplate = new System.Windows.Forms.Label();
            this.gbCommLine = new System.Windows.Forms.GroupBox();
            this.cbTransMode = new System.Windows.Forms.ComboBox();
            this.lblTransMode = new System.Windows.Forms.Label();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.gbDevice.SuspendLayout();
            this.gbCommLine.SuspendLayout();
            this.SuspendLayout();
            // 
            // gbDevice
            // 
            this.gbDevice.Controls.Add(this.btnBrowseDevTemplate);
            this.gbDevice.Controls.Add(this.btnCreateDevTemplate);
            this.gbDevice.Controls.Add(this.btnEditDevTemplate);
            this.gbDevice.Controls.Add(this.txtDevTemplate);
            this.gbDevice.Controls.Add(this.lblDevTemplate);
            this.gbDevice.Location = new System.Drawing.Point(16, 97);
            this.gbDevice.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.gbDevice.Name = "gbDevice";
            this.gbDevice.Padding = new System.Windows.Forms.Padding(13, 3, 13, 12);
            this.gbDevice.Size = new System.Drawing.Size(345, 75);
            this.gbDevice.TabIndex = 1;
            this.gbDevice.TabStop = false;
            this.gbDevice.Text = "Device";
            // 
            // btnBrowseDevTemplate
            // 
            this.btnBrowseDevTemplate.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnBrowseDevTemplate.Image = ((System.Drawing.Image)(resources.GetObject("btnBrowseDevTemplate.Image")));
            this.btnBrowseDevTemplate.Location = new System.Drawing.Point(232, 37);
            this.btnBrowseDevTemplate.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btnBrowseDevTemplate.Name = "btnBrowseDevTemplate";
            this.btnBrowseDevTemplate.Size = new System.Drawing.Size(27, 23);
            this.btnBrowseDevTemplate.TabIndex = 2;
            this.toolTip.SetToolTip(this.btnBrowseDevTemplate, "Browse for template");
            this.btnBrowseDevTemplate.UseVisualStyleBackColor = true;
            this.btnBrowseDevTemplate.Click += new System.EventHandler(this.btnBrowseDevTemplate_Click);
            // 
            // btnCreateDevTemplate
            // 
            this.btnCreateDevTemplate.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnCreateDevTemplate.Image = ((System.Drawing.Image)(resources.GetObject("btnCreateDevTemplate.Image")));
            this.btnCreateDevTemplate.Location = new System.Drawing.Point(267, 37);
            this.btnCreateDevTemplate.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btnCreateDevTemplate.Name = "btnCreateDevTemplate";
            this.btnCreateDevTemplate.Size = new System.Drawing.Size(27, 23);
            this.btnCreateDevTemplate.TabIndex = 3;
            this.toolTip.SetToolTip(this.btnCreateDevTemplate, "Create new template");
            this.btnCreateDevTemplate.UseVisualStyleBackColor = true;
            this.btnCreateDevTemplate.Click += new System.EventHandler(this.btnCreateDevTemplate_Click);
            // 
            // btnEditDevTemplate
            // 
            this.btnEditDevTemplate.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnEditDevTemplate.Image = ((System.Drawing.Image)(resources.GetObject("btnEditDevTemplate.Image")));
            this.btnEditDevTemplate.Location = new System.Drawing.Point(301, 37);
            this.btnEditDevTemplate.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btnEditDevTemplate.Name = "btnEditDevTemplate";
            this.btnEditDevTemplate.Size = new System.Drawing.Size(27, 23);
            this.btnEditDevTemplate.TabIndex = 4;
            this.toolTip.SetToolTip(this.btnEditDevTemplate, "Edit template");
            this.btnEditDevTemplate.UseVisualStyleBackColor = true;
            this.btnEditDevTemplate.Click += new System.EventHandler(this.btnEditDevTemplate_Click);
            // 
            // txtDevTemplate
            // 
            this.txtDevTemplate.Location = new System.Drawing.Point(17, 37);
            this.txtDevTemplate.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.txtDevTemplate.Name = "txtDevTemplate";
            this.txtDevTemplate.Size = new System.Drawing.Size(205, 25);
            this.txtDevTemplate.TabIndex = 1;
            this.txtDevTemplate.TextChanged += new System.EventHandler(this.txtDevTemplate_TextChanged);
            // 
            // lblDevTemplate
            // 
            this.lblDevTemplate.AutoSize = true;
            this.lblDevTemplate.Location = new System.Drawing.Point(13, 18);
            this.lblDevTemplate.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblDevTemplate.Name = "lblDevTemplate";
            this.lblDevTemplate.Size = new System.Drawing.Size(127, 15);
            this.lblDevTemplate.TabIndex = 0;
            this.lblDevTemplate.Text = "Device template";
            // 
            // gbCommLine
            // 
            this.gbCommLine.Controls.Add(this.cbTransMode);
            this.gbCommLine.Controls.Add(this.lblTransMode);
            this.gbCommLine.Location = new System.Drawing.Point(16, 14);
            this.gbCommLine.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.gbCommLine.Name = "gbCommLine";
            this.gbCommLine.Padding = new System.Windows.Forms.Padding(13, 3, 13, 12);
            this.gbCommLine.Size = new System.Drawing.Size(345, 76);
            this.gbCommLine.TabIndex = 0;
            this.gbCommLine.TabStop = false;
            this.gbCommLine.Text = "Communication line";
            // 
            // cbTransMode
            // 
            this.cbTransMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbTransMode.FormattingEnabled = true;
            this.cbTransMode.Items.AddRange(new object[] {
            "Modbus RTU",
            "Modbus ASCII",
            "Modbus TCP"});
            this.cbTransMode.Location = new System.Drawing.Point(17, 37);
            this.cbTransMode.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.cbTransMode.Name = "cbTransMode";
            this.cbTransMode.Size = new System.Drawing.Size(309, 23);
            this.cbTransMode.TabIndex = 1;
            this.cbTransMode.SelectedIndexChanged += new System.EventHandler(this.control_Changed);
            // 
            // lblTransMode
            // 
            this.lblTransMode.AutoSize = true;
            this.lblTransMode.Location = new System.Drawing.Point(13, 18);
            this.lblTransMode.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblTransMode.Name = "lblTransMode";
            this.lblTransMode.Size = new System.Drawing.Size(71, 15);
            this.lblTransMode.TabIndex = 0;
            this.lblTransMode.Text = "Protocol";
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(153, 179);
            this.btnOK.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(100, 27);
            this.btnOK.TabIndex = 2;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(261, 179);
            this.btnCancel.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(100, 27);
            this.btnCancel.TabIndex = 3;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // openFileDialog
            // 
            this.openFileDialog.DefaultExt = "*.xml";
            this.openFileDialog.Filter = "Template Files (*.xml)|*.xml|All Files (*.*)|*.*";
            this.openFileDialog.FilterIndex = 0;
            // 
            // FrmDevProps
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(377, 219);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.gbCommLine);
            this.Controls.Add(this.gbDevice);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FrmDevProps";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Device {0} Properties";
            this.Load += new System.EventHandler(this.FrmDevProps_Load);
            this.gbDevice.ResumeLayout(false);
            this.gbDevice.PerformLayout();
            this.gbCommLine.ResumeLayout(false);
            this.gbCommLine.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox gbDevice;
        private System.Windows.Forms.TextBox txtDevTemplate;
        private System.Windows.Forms.Label lblDevTemplate;
        private System.Windows.Forms.GroupBox gbCommLine;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.ComboBox cbTransMode;
        private System.Windows.Forms.Label lblTransMode;
        private System.Windows.Forms.OpenFileDialog openFileDialog;
        private System.Windows.Forms.Button btnBrowseDevTemplate;
        private System.Windows.Forms.Button btnCreateDevTemplate;
        private System.Windows.Forms.Button btnEditDevTemplate;
        private System.Windows.Forms.ToolTip toolTip;
    }
}