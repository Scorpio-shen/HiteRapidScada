using KpHiteModbus.Modbus.Model;
using Scada.Comm;
using Scada.UI;
using System;
using System.IO;
using System.Windows.Forms;
using static Scada.Comm.Devices.KPView;

namespace KpHiteModbus.Modbus.View
{
    public partial class FrmDevProps : Form
    {
        private AppDirs _appDirs;
        private KPProperties _kpProperties;
        private int _kpNum;
        public FrmDevProps()
        {
            InitializeComponent();
        }

        public FrmDevProps(int kpNum, AppDirs appDirs, KPProperties kpProperties)
        {
            _appDirs = appDirs;
            _kpProperties = kpProperties;
            _kpNum = kpNum;
            InitializeComponent();
        }
        private void FrmDevProps_Load(object sender, EventArgs e)
        {
            Translator.TranslateForm(this, "KpSiemens.Siemens.View.FrmDevProps", toolTip);
            openFileDialog.SetFilter(TempleteKeyString.DialogFilterStr);

            txtDevTemplate.Text = _kpProperties.CmdLine;
            _kpProperties.Modified = false;
        }

        private void btnBrowseDevTemplate_Click(object sender, EventArgs e)
        {
            var openFileDialog = new OpenFileDialog();
            if(openFileDialog.ShowDialog() == DialogResult.OK)
                txtDevTemplate.Text = MakeRelative(openFileDialog.FileName);
            txtDevTemplate.Select();
        }

        private void btnCreateDevTemplate_Click(object sender, EventArgs e)
        {
            CreateOrEditTemplate(string.Empty);
            txtDevTemplate.Select();
        }

        private void btnEditDevTemplate_Click(object sender, EventArgs e)
        {
            CreateOrEditTemplate(MakeAbsolute(txtDevTemplate.Text));
            txtDevTemplate.Select();
        }

        private string MakeRelative(string fileName)
        {
            return fileName.StartsWith(_appDirs.ConfigDir,StringComparison.OrdinalIgnoreCase) ? fileName.Substring(_appDirs.ConfigDir.Length) : fileName;
        }

        private string MakeAbsolute(string fileName)
        {
            return Path.IsPathRooted(fileName) ?
                fileName : Path.Combine(_appDirs.ConfigDir, fileName);
        }

        private void CreateOrEditTemplate(string fileName)
        {
            var frm = new FrmDevTemplate(_appDirs, fileName);
            frm.ShowDialog();
            if(!string.IsNullOrEmpty(frm.TemplateFileName))
                txtDevTemplate.Text = MakeRelative(frm.TemplateFileName);

        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (!File.Exists(MakeAbsolute(txtDevTemplate.Text)))
            {
                ScadaUiUtils.ShowError("模板文件不存在!");
                return;
            }

            if (_kpProperties.Modified)
            {
                _kpProperties.CmdLine = txtDevTemplate.Text;
            }
            DialogResult = DialogResult.OK;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void txtDevTemplate_TextChanged(object sender, EventArgs e)
        {
            _kpProperties.Modified = true;
            btnEditDevTemplate.Enabled = txtDevTemplate.Text.Trim() != "";
        }
    }
}
