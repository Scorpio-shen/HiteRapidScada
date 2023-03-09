using KpCommon.Extend;
using KpCommon.Model;
using KpCommon.Model.EnumType;
using KpHiteMqtt.Mqtt.Helper;
using KpHiteMqtt.Mqtt.Model;
using KpHiteMqtt.Mqtt.ViewModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Scada;
using Scada.Comm;
using Scada.Data.Entities;
using Scada.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace KpHiteMqtt.Mqtt.View
{
    public partial class FrmDevTemplate : Form
    {
        const string NewFileName = "KpHiteMqtt_NewTemplate.xml";
        string _fileName;                                   //载入已定义模板时文件名称或者新建的模板文件名称
        AppDirs _appDirs;
        DeviceTemplate deviceTemplate;                      //模板文件
        int CurrentIndex = default;                         //当前Property的索引

        private static List<InCnl> allInCnls;                              //所有输入通道
        private static List<CtrlCnl> allCtrlCnls;                          //所有输出通道
        
        
        public static List<InCnl> AllInCnls
        {
            get=>allInCnls.Select(incnl=>incnl.InCnlCopy()).ToList();
        }

        public static List<CtrlCnl> AllCtrlCnls
        {
            get=>allCtrlCnls.Select(ctrlcnl=>ctrlcnl.CtrlCnlCopy()).ToList();
        }

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

        public List<Property> Properties
        {
            get=>deviceTemplate.Properties;
            set
            {
                deviceTemplate.Properties = value;
                //BindProperty(deviceTemplate.Properties);
            }
        }
        public FrmDevTemplate(AppDirs appDirs,string fileName)
        {
            InitializeComponent();
            _appDirs = appDirs;
            _fileName = fileName;

           
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
                deviceTemplate = new DeviceTemplate()
                {
                    ConnectionOptions = new HslCommunication.MQTT.MqttConnectionOptions() 
                    {
                        Credentials = new HslCommunication.MQTT.MqttCredential()
                    },
                    Properties = new List<Property>()
                };
            }
            //载入所有输入、输出通道
            LoadConfigInCtrlCnls();

            //绑定控件
            var connectionOption = deviceTemplate.ConnectionOptions;

            txtClientId.AddDataBindings(connectionOption, nameof(connectionOption.ClientId));
            txtServerIp.AddDataBindings(connectionOption, nameof(connectionOption.IpAddress));
            txtPort.AddDataBindings(connectionOption, nameof(connectionOption.Port));
            txtUserName.AddDataBindings(connectionOption.Credentials, nameof(connectionOption.Credentials.UserName));
            txtPassword.AddDataBindings(connectionOption.Credentials, nameof(connectionOption.Credentials.Password));
            //txtAliveInterval.AddDataBindings(connectionOption, nameof(connectionOption.KeepAliveSendInterval));
            var binding = new Binding(nameof(txtAliveInterval.Text), connectionOption, nameof(connectionOption.KeepAliveSendInterval), false, DataSourceUpdateMode.OnPropertyChanged);
            binding.Format += (binding_sender, binding_e) =>
            {
                var span = binding_e.Value as TimeSpan?;
                if(span != null)
                {
                    binding_e.Value = span.Value.TotalSeconds;
                }
                else
                    binding_e.Value = default(double);
            };
            txtAliveInterval.DataBindings.Add(binding);
            //绑定属性集合
            dgvProperty.AutoGenerateColumns = false;
            bdsProperty.DataSource = deviceTemplate.Properties;
            dgvProperty.DataSource = bdsProperty;

            //控件事件
            txtClientId.TextChanged += ControlProperty_Changed;
            txtServerIp.TextChanged += ControlProperty_Changed;
            txtPort.TextChanged += ControlProperty_Changed;
            txtUserName.TextChanged += ControlProperty_Changed;
            txtPassword.TextChanged += ControlProperty_Changed;
            txtAliveInterval.TextChanged += ControlProperty_Changed;

        }

        private void ControlProperty_Changed(object sender, EventArgs e)
        {
            Modified = true;
        }

        private void LoadConfigInCtrlCnls()
        {
            int nIndex = _appDirs.ConfigDir.IndexOf("Instances");
            string baseInCnlXml = _appDirs.ConfigDir.Substring(0, nIndex) + "BaseXML\\InCnl.xml";
            string baseCtrlCnlXml = _appDirs.ConfigDir.Substring(0, nIndex) + "BaseXML\\CtrlCnl.xml";
            CXMLOperator operatorXml = new CXMLOperator();
            allInCnls = new List<InCnl>();
            allInCnls = operatorXml.GetConfig<List<InCnl>>(baseInCnlXml);
            allCtrlCnls = new List<CtrlCnl>();
            allCtrlCnls = operatorXml.GetConfig<List<CtrlCnl>>(baseCtrlCnlXml);

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
            //属性Index重新排序
            int index = 1;
            deviceTemplate.Properties.ForEach(p =>
            {
                p.Index = index++;
            });
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
        }
        #endregion

        #region 新增、删除、编辑物模型
        private void btnAddModel_Click(object sender, EventArgs e)
        {
            
            var property = new Property()
            {
                Index = CurrentIndex++
            };
            FrmDevModel frm = new FrmDevModel(property);
            if(frm.ShowDialog() != DialogResult.OK)
            {
                return;
            }
            //判断物模型是否已存在
            if (Properties.Any(p => p.Identifier.Equals(property.Identifier)))
            {
                ScadaUiUtils.ShowError($"标识符:{property.Identifier}已存在!");
                return;
            }
            Properties.Add(property);
            RefreshDataGridView();
            Modified = true;
        }

        private void tsmEditModel_Click(object sender, EventArgs e)
        {
            Property property = null;
            property = bdsProperty.Current as Property;
            if(property == null)
            {
                var rows = dgvProperty.SelectedRows;
                if (rows.Count > 0)
                {
                    var indexStr = rows[0].Cells["Index"].Value?.ToString();
                    if (!string.IsNullOrEmpty(indexStr))
                    {
                        property = Properties.FirstOrDefault(p => p.Index == indexStr.ToInt());
                    }
                    
                }
            }
            if(property == null)
            {
                ScadaUiUtils.ShowError("当前未选中任何要编辑的项!");
                return; 
            }
            FrmDevModel frm = new FrmDevModel(property);
            frm.ShowDialog();
            Modified = true;

        }

        private void tsmDeleteModel_Click(object sender, EventArgs e)
        {

        }

        #endregion

        #region 显示属性
        private void BindProperty(List<Property> properties)
        {
            bdsProperty.DataSource = null;
            bdsProperty.DataSource = properties;
        }
        #endregion

        #region 刷新datagridview显示
        private void RefreshDataGridView()
        {
            bdsProperty.ResetBindings(false);
            dgvProperty.Invalidate();
        }
        #endregion

    }
}
