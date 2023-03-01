
namespace Scada.Comm.Devices.Mqtt.UI
{
    partial class FrmMqttPubCmds
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
            this.label_Topic = new System.Windows.Forms.Label();
            this.textBox_Topic = new System.Windows.Forms.TextBox();
            this.label_Qos = new System.Windows.Forms.Label();
            this.comboBox_QosLevel = new System.Windows.Forms.ComboBox();
            this.label_Channel = new System.Windows.Forms.Label();
            this.comboBox_Channel = new System.Windows.Forms.ComboBox();
            this.label10 = new System.Windows.Forms.Label();
            this.buttonOK = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label_Topic
            // 
            this.label_Topic.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label_Topic.ImageAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.label_Topic.Location = new System.Drawing.Point(87, 23);
            this.label_Topic.Name = "label_Topic";
            this.label_Topic.Size = new System.Drawing.Size(49, 14);
            this.label_Topic.TabIndex = 0;
            this.label_Topic.Text = "Topic:";
            this.label_Topic.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // textBox_Topic
            // 
            this.textBox_Topic.Location = new System.Drawing.Point(157, 20);
            this.textBox_Topic.Name = "textBox_Topic";
            this.textBox_Topic.Size = new System.Drawing.Size(128, 23);
            this.textBox_Topic.TabIndex = 1;
            // 
            // label_Qos
            // 
            this.label_Qos.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label_Qos.ImageAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.label_Qos.Location = new System.Drawing.Point(66, 59);
            this.label_Qos.Name = "label_Qos";
            this.label_Qos.Size = new System.Drawing.Size(70, 14);
            this.label_Qos.TabIndex = 0;
            this.label_Qos.Text = "QosLevel:";
            this.label_Qos.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // comboBox_QosLevel
            // 
            this.comboBox_QosLevel.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_QosLevel.FormattingEnabled = true;
            this.comboBox_QosLevel.Location = new System.Drawing.Point(157, 59);
            this.comboBox_QosLevel.Name = "comboBox_QosLevel";
            this.comboBox_QosLevel.Size = new System.Drawing.Size(128, 22);
            this.comboBox_QosLevel.TabIndex = 2;
            // 
            // label_Channel
            // 
            this.label_Channel.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label_Channel.ImageAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.label_Channel.Location = new System.Drawing.Point(24, 100);
            this.label_Channel.Name = "label_Channel";
            this.label_Channel.Size = new System.Drawing.Size(112, 14);
            this.label_Channel.TabIndex = 0;
            this.label_Channel.Text = "Command Number:";
            this.label_Channel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // comboBox_Channel
            // 
            this.comboBox_Channel.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_Channel.FormattingEnabled = true;
            this.comboBox_Channel.Location = new System.Drawing.Point(157, 97);
            this.comboBox_Channel.Name = "comboBox_Channel";
            this.comboBox_Channel.Size = new System.Drawing.Size(128, 22);
            this.comboBox_Channel.TabIndex = 2;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label10.Location = new System.Drawing.Point(23, 416);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(0, 14);
            this.label10.TabIndex = 0;
            // 
            // buttonOK
            // 
            this.buttonOK.Location = new System.Drawing.Point(116, 153);
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
            this.buttonCancel.Location = new System.Drawing.Point(208, 153);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(67, 23);
            this.buttonCancel.TabIndex = 3;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // FrmMqttPubCmds
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 14F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(322, 207);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.comboBox_Channel);
            this.Controls.Add(this.comboBox_QosLevel);
            this.Controls.Add(this.textBox_Topic);
            this.Controls.Add(this.label_Channel);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.label_Qos);
            this.Controls.Add(this.label_Topic);
            this.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.Name = "FrmMqttPubCmds";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "FrmMqttPubCmds";
            this.Load += new System.EventHandler(this.FrmMqttPubCmds_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label_Topic;
        private System.Windows.Forms.TextBox textBox_Topic;
        private System.Windows.Forms.Label label_Qos;
        private System.Windows.Forms.ComboBox comboBox_QosLevel;
        private System.Windows.Forms.Label label_Channel;
        private System.Windows.Forms.ComboBox comboBox_Channel;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonCancel;
    }
}