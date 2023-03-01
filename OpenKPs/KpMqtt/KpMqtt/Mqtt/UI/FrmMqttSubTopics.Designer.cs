
namespace Scada.Comm.Devices.Mqtt.UI
{
    partial class FrmMqttSubTopics
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
            this.label_QosLevel = new System.Windows.Forms.Label();
            this.comboBox_QosLevel = new System.Windows.Forms.ComboBox();
            this.label10 = new System.Windows.Forms.Label();
            this.buttonOK = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label_Topic
            // 
            this.label_Topic.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
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
            // label_QosLevel
            // 
            this.label_QosLevel.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label_QosLevel.Location = new System.Drawing.Point(66, 61);
            this.label_QosLevel.Name = "label_QosLevel";
            this.label_QosLevel.Size = new System.Drawing.Size(70, 14);
            this.label_QosLevel.TabIndex = 0;
            this.label_QosLevel.Text = "QosLevel:";
            this.label_QosLevel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
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
            this.buttonOK.Location = new System.Drawing.Point(141, 104);
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
            this.buttonCancel.Location = new System.Drawing.Point(233, 104);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(67, 23);
            this.buttonCancel.TabIndex = 3;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // FrmMqttSubTopics
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 14F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(378, 148);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.comboBox_QosLevel);
            this.Controls.Add(this.textBox_Topic);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.label_QosLevel);
            this.Controls.Add(this.label_Topic);
            this.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.Name = "FrmMqttSubTopics";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "FrmMqttSubTopic";
            this.Load += new System.EventHandler(this.FrmMqttSubTopics_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label_Topic;
        private System.Windows.Forms.TextBox textBox_Topic;
        private System.Windows.Forms.Label label_QosLevel;
        private System.Windows.Forms.ComboBox comboBox_QosLevel;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonCancel;
    }
}