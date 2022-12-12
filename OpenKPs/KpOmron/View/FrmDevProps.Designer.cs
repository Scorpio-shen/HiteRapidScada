namespace KpOmron.View
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
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOK = new System.Windows.Forms.Button();
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.gbDevice.SuspendLayout();
            this.SuspendLayout();
            // 
            // gbDevice
            // 
            this.gbDevice.Controls.Add(this.btnBrowseDevTemplate);
            this.gbDevice.Controls.Add(this.btnCreateDevTemplate);
            this.gbDevice.Controls.Add(this.btnEditDevTemplate);
            this.gbDevice.Controls.Add(this.txtDevTemplate);
            this.gbDevice.Controls.Add(this.lblDevTemplate);
            this.gbDevice.Location = new System.Drawing.Point(12, 47);
            this.gbDevice.Name = "gbDevice";
            this.gbDevice.Padding = new System.Windows.Forms.Padding(10, 3, 10, 9);
            this.gbDevice.Size = new System.Drawing.Size(259, 60);
            this.gbDevice.TabIndex = 5;
            this.gbDevice.TabStop = false;
            this.gbDevice.Text = "Device";
            // 
            // btnBrowseDevTemplate
            // 
            this.btnBrowseDevTemplate.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnBrowseDevTemplate.Image = ((System.Drawing.Image)(resources.GetObject("btnBrowseDevTemplate.Image")));
            this.btnBrowseDevTemplate.Location = new System.Drawing.Point(174, 30);
            this.btnBrowseDevTemplate.Name = "btnBrowseDevTemplate";
            this.btnBrowseDevTemplate.Size = new System.Drawing.Size(20, 18);
            this.btnBrowseDevTemplate.TabIndex = 2;
            this.toolTip.SetToolTip(this.btnBrowseDevTemplate, "Browse for template");
            this.btnBrowseDevTemplate.UseVisualStyleBackColor = true;
            this.btnBrowseDevTemplate.Click += new System.EventHandler(this.btnBrowseDevTemplate_Click);
            // 
            // btnCreateDevTemplate
            // 
            this.btnCreateDevTemplate.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnCreateDevTemplate.Image = ((System.Drawing.Image)(resources.GetObject("btnCreateDevTemplate.Image")));
            this.btnCreateDevTemplate.Location = new System.Drawing.Point(200, 30);
            this.btnCreateDevTemplate.Name = "btnCreateDevTemplate";
            this.btnCreateDevTemplate.Size = new System.Drawing.Size(20, 18);
            this.btnCreateDevTemplate.TabIndex = 3;
            this.toolTip.SetToolTip(this.btnCreateDevTemplate, "Create new template");
            this.btnCreateDevTemplate.UseVisualStyleBackColor = true;
            this.btnCreateDevTemplate.Click += new System.EventHandler(this.btnCreateDevTemplate_Click);
            // 
            // btnEditDevTemplate
            // 
            this.btnEditDevTemplate.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnEditDevTemplate.Image = ((System.Drawing.Image)(resources.GetObject("btnEditDevTemplate.Image")));
            this.btnEditDevTemplate.Location = new System.Drawing.Point(226, 30);
            this.btnEditDevTemplate.Name = "btnEditDevTemplate";
            this.btnEditDevTemplate.Size = new System.Drawing.Size(20, 18);
            this.btnEditDevTemplate.TabIndex = 4;
            this.toolTip.SetToolTip(this.btnEditDevTemplate, "Edit template");
            this.btnEditDevTemplate.UseVisualStyleBackColor = true;
            this.btnEditDevTemplate.Click += new System.EventHandler(this.btnEditDevTemplate_Click);
            // 
            // txtDevTemplate
            // 
            this.txtDevTemplate.Location = new System.Drawing.Point(13, 30);
            this.txtDevTemplate.Name = "txtDevTemplate";
            this.txtDevTemplate.Size = new System.Drawing.Size(155, 21);
            this.txtDevTemplate.TabIndex = 1;
            this.txtDevTemplate.TextChanged += new System.EventHandler(this.txtDevTemplate_TextChanged);
            // 
            // lblDevTemplate
            // 
            this.lblDevTemplate.AutoSize = true;
            this.lblDevTemplate.Location = new System.Drawing.Point(10, 15);
            this.lblDevTemplate.Name = "lblDevTemplate";
            this.lblDevTemplate.Size = new System.Drawing.Size(95, 12);
            this.lblDevTemplate.TabIndex = 0;
            this.lblDevTemplate.Text = "Device template";
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(196, 143);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 21);
            this.btnCancel.TabIndex = 7;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(115, 143);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 21);
            this.btnOK.TabIndex = 6;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
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
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(283, 175);
            this.Controls.Add(this.gbDevice);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "FrmDevProps";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "模板配置";
            this.Load += new System.EventHandler(this.FrmDevProps_Load);
            this.gbDevice.ResumeLayout(false);
            this.gbDevice.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox gbDevice;
        private System.Windows.Forms.Button btnBrowseDevTemplate;
        private System.Windows.Forms.ToolTip toolTip;
        private System.Windows.Forms.Button btnCreateDevTemplate;
        private System.Windows.Forms.Button btnEditDevTemplate;
        private System.Windows.Forms.TextBox txtDevTemplate;
        private System.Windows.Forms.Label lblDevTemplate;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.OpenFileDialog openFileDialog;
    }
}