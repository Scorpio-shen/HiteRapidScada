using KpCommon.Model;
using KpCommon.Model.EnumType;
using KpHiteOpcUaServer.OPCUaServer.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Scada;
using Scada.Comm;
using Scada.UI;
using System;
using System.IO;
using System.Windows.Forms;

namespace KpHiteOpcUaServer.OPCUaServer.View
{
    public partial class FrmDevTemplate : Form
    {
        const string NewFileName = "KpHiteOpcUaServer_NewTemplate.xml";
        string _fileName;                                   //载入已定义模板时文件名称或者新建的模板文件名称
        AppDirs _appDirs;
        DeviceTemplate deviceTemplate;                      //模板文件
        TreeNode tagGroupRootNode;                          //Tag节点
        TreeNode currentNode;                               //当前选中节点
        

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
            modified = false;

            txtServerIP.TextChanged += TextBox_TextChanged;
            txtInputChannelPath.TextChanged += TextBox_TextChanged;
            txtOutputChannelPath.TextChanged += TextBox_TextChanged;
        }

        private void TextBox_TextChanged(object sender, EventArgs e)
        {
            Modified = true;
        }

        private void FrmDevTemplate_Load(object sender, EventArgs e)
        {
            openFileDialog.SetFilter(KpCommon.Model.TempleteKeyString.DialogFilterStr);
            saveFileDialog.SetFilter(KpCommon.Model.TempleteKeyString.DialogFilterStr);

            openFileDialog.InitialDirectory = _appDirs.ConfigDir;
            saveFileDialog.InitialDirectory = _appDirs.ConfigDir;

            btnNew.Visible = false;
            btnOpen.Visible = false;
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
                //FillTree();
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

                //FillTree();
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

            if (string.IsNullOrEmpty(txtInputChannelPath.Text))
            {
                ScadaUiUtils.ShowError("输入通道路径不能为空!");
                return false;
            }

            if(string.IsNullOrEmpty(txtOutputChannelPath.Text))
            {
                ScadaUiUtils.ShowError("输出通道路径不能为空!");
                return false;
            }

            string errMsg = string.Empty;
            if (saveAs)
            {
                JObject jobect = JObject.Parse( JsonConvert.SerializeObject(deviceTemplate));
                File.WriteAllText(newFileName,jobect.ToString());
                _fileName = newFileName;
                Modified = false;
                return true;
            }
            var inputPath = txtInputChannelPath.Text;
            var outputPath = txtOutputChannelPath.Text;

            deviceTemplate.OPCServerIP = txtServerIP.Text;
            deviceTemplate.InputChannelPath = inputPath;
            deviceTemplate.OutputChannelPath = outputPath;
            if (deviceTemplate.Save(newFileName, inputPath,outputPath,out errMsg))
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

            //显示参数
            txtServerIP.Text = deviceTemplate.OPCServerIP;
            txtInputChannelPath.Text = deviceTemplate.InputChannelPath;
            txtOutputChannelPath.Text = deviceTemplate.OutputChannelPath;

        }
        #endregion

        private void btnInputChoose_Click(object sender, EventArgs e)
        {
            var openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() != DialogResult.OK)
                return;
            var path = openFileDialog.FileName;
            if (!File.Exists(path))
            {
                ScadaUiUtils.ShowError($"选中文件路径不存在!");
                return;
            }
            
            txtInputChannelPath.Text = path;
        }

        private void btnOutputChoose_Click(object sender, EventArgs e)
        {
            var openFileDialog = new OpenFileDialog();  
            if(openFileDialog.ShowDialog() != DialogResult.OK)
                return;
            var path = openFileDialog.FileName;
            if (!File.Exists(path))
            {
                ScadaUiUtils.ShowError($"选中文件路径不存在!");
                return;
            }

            txtOutputChannelPath.Text = path;
        }
    }
}
