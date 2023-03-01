
namespace Scada.Comm.Devices.Mqtt.UI
{
    partial class FrmMqttServerConfig
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
            this.label_Hostname = new System.Windows.Forms.Label();
            this.textBox_Hostname = new System.Windows.Forms.TextBox();
            this.label_ClientID = new System.Windows.Forms.Label();
            this.label_Port = new System.Windows.Forms.Label();
            this.textBox_Port = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.textBox_Password = new System.Windows.Forms.TextBox();
            this.buttonOK = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.label_Password = new System.Windows.Forms.Label();
            this.textBox_UserName = new System.Windows.Forms.TextBox();
            this.label_UseName = new System.Windows.Forms.Label();
            this.textBox_ClientID = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // label_Hostname
            // 
            this.label_Hostname.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label_Hostname.Location = new System.Drawing.Point(66, 23);
            this.label_Hostname.Name = "label_Hostname";
            this.label_Hostname.Size = new System.Drawing.Size(70, 14);
            this.label_Hostname.TabIndex = 0;
            this.label_Hostname.Text = "Hostname:";
            this.label_Hostname.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // textBox_Hostname
            // 
            this.textBox_Hostname.Location = new System.Drawing.Point(156, 20);
            this.textBox_Hostname.Name = "textBox_Hostname";
            this.textBox_Hostname.Size = new System.Drawing.Size(129, 23);
            this.textBox_Hostname.TabIndex = 1;
            // 
            // label_ClientID
            // 
            this.label_ClientID.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label_ClientID.Location = new System.Drawing.Point(66, 62);
            this.label_ClientID.Name = "label_ClientID";
            this.label_ClientID.Size = new System.Drawing.Size(70, 14);
            this.label_ClientID.TabIndex = 0;
            this.label_ClientID.Text = "ClientID:";
            this.label_ClientID.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label_Port
            // 
            this.label_Port.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label_Port.Location = new System.Drawing.Point(94, 101);
            this.label_Port.Name = "label_Port";
            this.label_Port.Size = new System.Drawing.Size(42, 14);
            this.label_Port.TabIndex = 0;
            this.label_Port.Text = "Port:";
            this.label_Port.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // textBox_Port
            // 
            this.textBox_Port.Location = new System.Drawing.Point(156, 98);
            this.textBox_Port.Name = "textBox_Port";
            this.textBox_Port.Size = new System.Drawing.Size(129, 23);
            this.textBox_Port.TabIndex = 1;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label10.Location = new System.Drawing.Point(23, 162);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(0, 14);
            this.label10.TabIndex = 0;
            // 
            // textBox_Password
            // 
            this.textBox_Password.Location = new System.Drawing.Point(156, 176);
            this.textBox_Password.Name = "textBox_Password";
            this.textBox_Password.Size = new System.Drawing.Size(129, 23);
            this.textBox_Password.TabIndex = 1;
            // 
            // buttonOK
            // 
            this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonOK.Location = new System.Drawing.Point(184, 235);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(67, 23);
            this.buttonOK.TabIndex = 3;
            this.buttonOK.Text = "OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(276, 235);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(67, 23);
            this.buttonCancel.TabIndex = 3;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // openFileDialog
            // 
            this.openFileDialog.DefaultExt = "*.js";
            this.openFileDialog.FileName = "openFileDialog";
            this.openFileDialog.Filter = "Js Files (*.js)|*.js|All Files (*.*)|*.*";
            // 
            // label_Password
            // 
            this.label_Password.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label_Password.Location = new System.Drawing.Point(66, 179);
            this.label_Password.Name = "label_Password";
            this.label_Password.Size = new System.Drawing.Size(70, 14);
            this.label_Password.TabIndex = 0;
            this.label_Password.Text = "Password:";
            this.label_Password.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // textBox_UserName
            // 
            this.textBox_UserName.Location = new System.Drawing.Point(156, 137);
            this.textBox_UserName.Name = "textBox_UserName";
            this.textBox_UserName.Size = new System.Drawing.Size(129, 23);
            this.textBox_UserName.TabIndex = 1;
            // 
            // label_UseName
            // 
            this.label_UseName.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label_UseName.Location = new System.Drawing.Point(66, 140);
            this.label_UseName.Name = "label_UseName";
            this.label_UseName.Size = new System.Drawing.Size(70, 14);
            this.label_UseName.TabIndex = 0;
            this.label_UseName.Text = "UserName:";
            this.label_UseName.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // textBox_ClientID
            // 
            this.textBox_ClientID.Location = new System.Drawing.Point(156, 59);
            this.textBox_ClientID.Name = "textBox_ClientID";
            this.textBox_ClientID.Size = new System.Drawing.Size(129, 23);
            this.textBox_ClientID.TabIndex = 1;
            // 
            // FrmMqttServerConfig
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 14F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(378, 284);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.textBox_Password);
            this.Controls.Add(this.textBox_UserName);
            this.Controls.Add(this.textBox_ClientID);
            this.Controls.Add(this.textBox_Port);
            this.Controls.Add(this.textBox_Hostname);
            this.Controls.Add(this.label_Password);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.label_UseName);
            this.Controls.Add(this.label_Port);
            this.Controls.Add(this.label_ClientID);
            this.Controls.Add(this.label_Hostname);
            this.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.Name = "FrmMqttServerConfig";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "FrmMqttServerConfig";
            this.Load += new System.EventHandler(this.FrmMqttServerConfig_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label_Hostname;
        private System.Windows.Forms.TextBox textBox_Hostname;
        private System.Windows.Forms.Label label_ClientID;
        private System.Windows.Forms.Label label_Port;
        private System.Windows.Forms.TextBox textBox_Port;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.TextBox textBox_Password;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.OpenFileDialog openFileDialog;
        private System.Windows.Forms.Label label_Password;
        private System.Windows.Forms.TextBox textBox_UserName;
        private System.Windows.Forms.Label label_UseName;
        private System.Windows.Forms.TextBox textBox_ClientID;
    }
}