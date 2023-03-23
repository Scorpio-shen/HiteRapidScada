
namespace Scada.Comm.Devices.Mqtt.UI
{
    partial class FrmMqttPubTopic
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
            this.label_PubBehavior = new System.Windows.Forms.Label();
            this.comboBox_Behavior = new System.Windows.Forms.ComboBox();
            this.label_Retain = new System.Windows.Forms.Label();
            this.comboBox_retain = new System.Windows.Forms.ComboBox();
            this.label_Prefix = new System.Windows.Forms.Label();
            this.textBox_Prefix = new System.Windows.Forms.TextBox();
            this.label_Suffix = new System.Windows.Forms.Label();
            this.textBox_suffix = new System.Windows.Forms.TextBox();
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
            // label_Qos
            // 
            this.label_Qos.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label_Qos.Location = new System.Drawing.Point(66, 61);
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
            this.label_Channel.Location = new System.Drawing.Point(31, 99);
            this.label_Channel.Name = "label_Channel";
            this.label_Channel.Size = new System.Drawing.Size(105, 14);
            this.label_Channel.TabIndex = 0;
            this.label_Channel.Text = "Input Channel:";
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
            // label_PubBehavior
            // 
            this.label_PubBehavior.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label_PubBehavior.Location = new System.Drawing.Point(45, 137);
            this.label_PubBehavior.Name = "label_PubBehavior";
            this.label_PubBehavior.Size = new System.Drawing.Size(91, 14);
            this.label_PubBehavior.TabIndex = 0;
            this.label_PubBehavior.Text = "PubBehavior:";
            this.label_PubBehavior.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // comboBox_Behavior
            // 
            this.comboBox_Behavior.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_Behavior.FormattingEnabled = true;
            this.comboBox_Behavior.Items.AddRange(new object[] {
            "OnChange",
            "OnAlways"});
            this.comboBox_Behavior.Location = new System.Drawing.Point(157, 135);
            this.comboBox_Behavior.Name = "comboBox_Behavior";
            this.comboBox_Behavior.Size = new System.Drawing.Size(128, 22);
            this.comboBox_Behavior.TabIndex = 2;
            // 
            // label_Retain
            // 
            this.label_Retain.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label_Retain.Location = new System.Drawing.Point(80, 175);
            this.label_Retain.Name = "label_Retain";
            this.label_Retain.Size = new System.Drawing.Size(56, 14);
            this.label_Retain.TabIndex = 0;
            this.label_Retain.Text = "Retain:";
            this.label_Retain.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // comboBox_retain
            // 
            this.comboBox_retain.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_retain.FormattingEnabled = true;
            this.comboBox_retain.Items.AddRange(new object[] {
            "true",
            "false"});
            this.comboBox_retain.Location = new System.Drawing.Point(157, 173);
            this.comboBox_retain.Name = "comboBox_retain";
            this.comboBox_retain.Size = new System.Drawing.Size(128, 22);
            this.comboBox_retain.TabIndex = 2;
            // 
            // label_Prefix
            // 
            this.label_Prefix.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label_Prefix.Location = new System.Drawing.Point(80, 213);
            this.label_Prefix.Name = "label_Prefix";
            this.label_Prefix.Size = new System.Drawing.Size(56, 14);
            this.label_Prefix.TabIndex = 0;
            this.label_Prefix.Text = "Prefix:";
            this.label_Prefix.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // textBox_Prefix
            // 
            this.textBox_Prefix.Location = new System.Drawing.Point(157, 211);
            this.textBox_Prefix.Name = "textBox_Prefix";
            this.textBox_Prefix.Size = new System.Drawing.Size(128, 23);
            this.textBox_Prefix.TabIndex = 1;
            // 
            // label_Suffix
            // 
            this.label_Suffix.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label_Suffix.Location = new System.Drawing.Point(80, 251);
            this.label_Suffix.Name = "label_Suffix";
            this.label_Suffix.Size = new System.Drawing.Size(56, 14);
            this.label_Suffix.TabIndex = 0;
            this.label_Suffix.Text = "Suffix:";
            this.label_Suffix.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // textBox_suffix
            // 
            this.textBox_suffix.Location = new System.Drawing.Point(157, 250);
            this.textBox_suffix.Name = "textBox_suffix";
            this.textBox_suffix.Size = new System.Drawing.Size(128, 23);
            this.textBox_suffix.TabIndex = 1;
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
            this.buttonOK.Location = new System.Drawing.Point(176, 296);
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
            this.buttonCancel.Location = new System.Drawing.Point(268, 296);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(67, 23);
            this.buttonCancel.TabIndex = 3;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // FrmMqttPubTopic
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 14F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(378, 352);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.comboBox_retain);
            this.Controls.Add(this.comboBox_Behavior);
            this.Controls.Add(this.comboBox_Channel);
            this.Controls.Add(this.label_Retain);
            this.Controls.Add(this.comboBox_QosLevel);
            this.Controls.Add(this.label_PubBehavior);
            this.Controls.Add(this.textBox_suffix);
            this.Controls.Add(this.textBox_Prefix);
            this.Controls.Add(this.label_Suffix);
            this.Controls.Add(this.textBox_Topic);
            this.Controls.Add(this.label_Prefix);
            this.Controls.Add(this.label_Channel);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.label_Qos);
            this.Controls.Add(this.label_Topic);
            this.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.Name = "FrmMqttPubTopic";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "FrmMqttPubTopic";
            this.Load += new System.EventHandler(this.FrmMqttPubTopic_Load);
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
        private System.Windows.Forms.Label label_PubBehavior;
        private System.Windows.Forms.ComboBox comboBox_Behavior;
        private System.Windows.Forms.Label label_Retain;
        private System.Windows.Forms.ComboBox comboBox_retain;
        private System.Windows.Forms.Label label_Prefix;
        private System.Windows.Forms.TextBox textBox_Prefix;
        private System.Windows.Forms.Label label_Suffix;
        private System.Windows.Forms.TextBox textBox_suffix;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonCancel;
    }
}