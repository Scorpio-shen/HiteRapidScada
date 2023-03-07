using KpCommon.Extend;
using KpCommon.Model;
using KpCommon.Model.EnumType;
using KpHiteMqtt.Mqtt.Model;
using KpHiteMqtt.Mqtt.ViewModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Scada;
using Scada.Comm;
using Scada.UI;
using System;
using System.IO;
using System.Windows.Forms;

namespace KpHiteMqtt.Mqtt.View
{
    public partial class FrmDevTemplate : Form
    {
        const string NewFileName = "KpHiteModbus_NewTemplate.xml";
        string _fileName;                                   //载入已定义模板时文件名称或者新建的模板文件名称
        AppDirs _appDirs;
        DeviceTemplate deviceTemplate;                      //模板文件
        
        public FrmDevTemplateViewModel ViewModel { get; set; }

        private bool modified;
        public bool Modified
        {
            get { return modified; }
            set
            {
                modified = value;
                btnSave.Enabled = modified;
            }
        }

        public string TemplateFileName { get => _fileName; }
        public FrmDevTemplate(AppDirs appDirs,string fileName)
        {
            InitializeComponent();
            _appDirs = appDirs;
            _fileName = fileName;

            ViewModel = new FrmDevTemplateViewModel();

            //绑定控件
            txtClientId.AddDataBindings(ViewModel, nameof(ViewModel.ClientID));
            txtServerIp.AddDataBindings(ViewModel,nameof(ViewModel.MqttServerIp));
            txtPort.AddDataBindings(ViewModel, nameof(ViewModel.MqttServerPort));
            txtUserName.AddDataBindings(ViewModel,nameof (ViewModel.UserName));
            txtPassword.AddDataBindings(ViewModel, nameof(ViewModel.Password));
            txtAliveInterval.AddDataBindings(ViewModel, nameof(ViewModel.HeartInterval));
        }

        private void FrmDevTemplate_Load(object sender, EventArgs e)
        {
            openFileDialog.SetFilter(TempleteKeyString.DialogFilterStr);
            saveFileDialog.SetFilter(TempleteKeyString.DialogFilterStr);

            openFileDialog.InitialDirectory = _appDirs.ConfigDir;
            saveFileDialog.InitialDirectory = _appDirs.ConfigDir;

            Modified = false;

            //判断模板文件是否载入原模板
            if (!string.IsNullOrEmpty(_fileName))
            {
                saveFileDialog.FileName = _fileName;
                LoadTemplate(_fileName);
            }
            else
            {
                //新建
                saveFileDialog.FileName = NewFileName;
                deviceTemplate = new DeviceTemplate();
            }


        }

        private void FrmDevTemplate_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = !CheckChange();
        }

        #region tool栏事件
        private void btnNew_Click(object sender, EventArgs e)
        {
            if (CheckChange())
            {
                saveFileDialog.FileName = NewFileName;
                deviceTemplate = new DeviceTemplate();
                _fileName = String.Empty;
            }
        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            if (CheckChange())
            {
                openFileDialog.FileName = String.Empty;

                if(openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    saveFileDialog.FileName = openFileDialog.FileName;
                    LoadTemplate(openFileDialog.FileName);
                }
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            SaveChange(false);
        }

        private void btnSaveAs_Click(object sender, EventArgs e)
        {
            SaveChange(true);
        }


        /// <summary>
        /// 导入
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnImport_Click(object sender, EventArgs e)
        {
            var openFile = new OpenFileDialog();
            openFile.SetFilter(TempleteKeyString.OpenExcelFilterStr);
            var reuslt = openFile.ShowDialog();
            if (reuslt != DialogResult.OK)
                return;
            Modified = true;
            var filePath = openFile.FileName;

        }
        /// <summary>
        /// 导出
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnExport_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFile = new SaveFileDialog();
            saveFile.SetFilter(TempleteKeyString.OpenExcelFilterStr);
            var reuslt = saveFile.ShowDialog();
            if (reuslt != DialogResult.OK)
                return;
            var filePath = saveFile.FileName;
            var extensionName = Path.GetExtension(filePath);
        }
        #endregion

        #region 存储配置
        private bool CheckChange()
        {
            if (modified)
            {
                var result = MessageBox.Show("是否确认保存模板?", CommonPhrases.QuestionCaption, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                switch (result)
                {
                    case DialogResult.Yes:
                        return SaveChange(false);
                    case DialogResult.No:
                        return true;
                    default:
                        return false;
                }
            }
            else
                return true;
        }

        private bool SaveChange(bool saveAs)
        {
            string newFileName = string.Empty;
            if (saveAs || string.IsNullOrEmpty(_fileName))
            {
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                    newFileName = saveFileDialog.FileName;
            }
            else
                newFileName = _fileName;

            if (string.IsNullOrEmpty(newFileName))
                return false;

            string errMsg = string.Empty;
            if (saveAs)
            {
                JObject jobect = JObject.Parse( JsonConvert.SerializeObject(deviceTemplate));
                File.WriteAllText(newFileName,jobect.ToString());
                _fileName = newFileName;
                Modified = false;
                return true;
            }
            if (deviceTemplate.Save(newFileName, out errMsg))
            {
                _fileName = newFileName;
                Modified = false;
                return true;
            }
            else
            {
                ScadaUiUtils.ShowError(errMsg);
                return false;
            }
        }
        #endregion

        #region 载入模板
        private void LoadTemplate(string fileName)
        {
            deviceTemplate = new DeviceTemplate();
            _fileName = fileName;

            string errMsg = string.Empty;
            if (!deviceTemplate.Load(fileName, out errMsg))
                ScadaUiUtils.ShowError(errMsg);

            //将载入的连接参数赋值
            ViewModel.ClientID = deviceTemplate.ConnectionOptions.ClientId;
            ViewModel.UserName = deviceTemplate.ConnectionOptions.Credentials.UserName;
            ViewModel.Password = deviceTemplate.ConnectionOptions.Credentials.Password;
            ViewModel.HeartInterval = deviceTemplate.ConnectionOptions.KeepAliveSendInterval.TotalSeconds;
            ViewModel.MqttServerIp = deviceTemplate.ConnectionOptions.IpAddress;
            ViewModel.MqttServerPort = deviceTemplate.ConnectionOptions.Port;
        }
        #endregion

        #region 新增、删除、编辑物模型
        private void btnAddModel_Click(object sender, EventArgs e)
        {
            FrmDevModel frm = new FrmDevModel();
            frm.ShowDialog();
        }

        private void tsmEditModel_Click(object sender, EventArgs e)
        {
            FrmDevModel frm = new FrmDevModel();
            frm.ShowDialog();

        }

        private void tsmDeleteModel_Click(object sender, EventArgs e)
        {

        }

        #endregion


    }
}
