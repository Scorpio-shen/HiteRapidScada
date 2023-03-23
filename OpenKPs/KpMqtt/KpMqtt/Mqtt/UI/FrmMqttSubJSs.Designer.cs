
namespace Scada.Comm.Devices.Mqtt.UI
{
    partial class FrmMqttSubJSs
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
            this.label_Number = new System.Windows.Forms.Label();
            this.textBox_CnlCnt = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.textBox_JSHandlePath = new System.Windows.Forms.TextBox();
            this.buttonOK = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.textBox_Lable = new System.Windows.Forms.TextBox();
            this.button_Add = new System.Windows.Forms.Button();
            this.label_TagName = new System.Windows.Forms.Label();
            this.panelJS = new System.Windows.Forms.Panel();
            this.button_Delete = new System.Windows.Forms.Button();
            this.label_JSFileName = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label_Topic
            // 
            this.label_Topic.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label_Topic.Location = new System.Drawing.Point(94, 23);
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
            this.label_QosLevel.Location = new System.Drawing.Point(73, 65);
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
            this.comboBox_QosLevel.Location = new System.Drawing.Point(157, 61);
            this.comboBox_QosLevel.Name = "comboBox_QosLevel";
            this.comboBox_QosLevel.Size = new System.Drawing.Size(128, 22);
            this.comboBox_QosLevel.TabIndex = 2;
            // 
            // label_Number
            // 
            this.label_Number.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label_Number.Location = new System.Drawing.Point(87, 107);
            this.label_Number.Name = "label_Number";
            this.label_Number.Size = new System.Drawing.Size(56, 14);
            this.label_Number.TabIndex = 0;
            this.label_Number.Text = "Number:";
            this.label_Number.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // textBox_CnlCnt
            // 
            this.textBox_CnlCnt.Location = new System.Drawing.Point(157, 101);
            this.textBox_CnlCnt.Name = "textBox_CnlCnt";
            this.textBox_CnlCnt.Size = new System.Drawing.Size(128, 23);
            this.textBox_CnlCnt.TabIndex = 1;
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
            // textBox_JSHandlePath
            // 
            this.textBox_JSHandlePath.Location = new System.Drawing.Point(157, 142);
            this.textBox_JSHandlePath.Multiline = true;
            this.textBox_JSHandlePath.Name = "textBox_JSHandlePath";
            this.textBox_JSHandlePath.Size = new System.Drawing.Size(128, 26);
            this.textBox_JSHandlePath.TabIndex = 1;
            // 
            // buttonOK
            // 
            this.buttonOK.Location = new System.Drawing.Point(179, 442);
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
            this.buttonCancel.Location = new System.Drawing.Point(271, 442);
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
            // textBox_Lable
            // 
            this.textBox_Lable.Location = new System.Drawing.Point(88, 196);
            this.textBox_Lable.Name = "textBox_Lable";
            this.textBox_Lable.Size = new System.Drawing.Size(86, 23);
            this.textBox_Lable.TabIndex = 9;
            // 
            // button_Add
            // 
            this.button_Add.Location = new System.Drawing.Point(180, 194);
            this.button_Add.Name = "button_Add";
            this.button_Add.Size = new System.Drawing.Size(76, 26);
            this.button_Add.TabIndex = 8;
            this.button_Add.Text = "Add";
            this.button_Add.UseVisualStyleBackColor = true;
            this.button_Add.Click += new System.EventHandler(this.button_Add_Click);
            // 
            // label_TagName
            // 
            this.label_TagName.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label_TagName.Location = new System.Drawing.Point(20, 200);
            this.label_TagName.Name = "label_TagName";
            this.label_TagName.Size = new System.Drawing.Size(63, 14);
            this.label_TagName.TabIndex = 7;
            this.label_TagName.Text = "TagName:";
            this.label_TagName.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // panelJS
            // 
            this.panelJS.Location = new System.Drawing.Point(26, 239);
            this.panelJS.Name = "panelJS";
            this.panelJS.Size = new System.Drawing.Size(312, 180);
            this.panelJS.TabIndex = 10;
            // 
            // button_Delete
            // 
            this.button_Delete.Location = new System.Drawing.Point(262, 194);
            this.button_Delete.Name = "button_Delete";
            this.button_Delete.Size = new System.Drawing.Size(76, 26);
            this.button_Delete.TabIndex = 8;
            this.button_Delete.Text = "Delete";
            this.button_Delete.UseVisualStyleBackColor = true;
            this.button_Delete.Click += new System.EventHandler(this.button_Delete_Click);
            // 
            // label_JSFileName
            // 
            this.label_JSFileName.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label_JSFileName.Location = new System.Drawing.Point(52, 149);
            this.label_JSFileName.Name = "label_JSFileName";
            this.label_JSFileName.Size = new System.Drawing.Size(91, 14);
            this.label_JSFileName.TabIndex = 0;
            this.label_JSFileName.Text = "JS FileName:";
            this.label_JSFileName.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // FrmMqttSubJSs
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 14F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(359, 472);
            this.Controls.Add(this.panelJS);
            this.Controls.Add(this.textBox_Lable);
            this.Controls.Add(this.button_Delete);
            this.Controls.Add(this.button_Add);
            this.Controls.Add(this.label_TagName);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.comboBox_QosLevel);
            this.Controls.Add(this.textBox_JSHandlePath);
            this.Controls.Add(this.textBox_CnlCnt);
            this.Controls.Add(this.textBox_Topic);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.label_Number);
            this.Controls.Add(this.label_QosLevel);
            this.Controls.Add(this.label_JSFileName);
            this.Controls.Add(this.label_Topic);
            this.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.Name = "FrmMqttSubJSs";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "FrmMqttMqttSubJSs";
            this.Load += new System.EventHandler(this.FrmMqttSubJSs_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label_Topic;
        private System.Windows.Forms.TextBox textBox_Topic;
        private System.Windows.Forms.Label label_QosLevel;
        private System.Windows.Forms.ComboBox comboBox_QosLevel;
        private System.Windows.Forms.Label label_Number;
        private System.Windows.Forms.TextBox textBox_CnlCnt;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.TextBox textBox_JSHandlePath;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.OpenFileDialog openFileDialog;
        private System.Windows.Forms.TextBox textBox_Lable;
        private System.Windows.Forms.Button button_Add;
        private System.Windows.Forms.Label label_TagName;
        private System.Windows.Forms.Panel panelJS;
        private System.Windows.Forms.Button button_Delete;
        private System.Windows.Forms.Label label_JSFileName;
    }
}