using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using Scada.Comm.Devices.rttask.Protocol;
using Scada.UI;
using System.IO;
using System.Xml;

namespace Scada.Comm.Devices.rttask.UI
{
    public partial class FrmDevTemplate_rttask : Form
    {
        private const string NewFileName = "KpRtTask_NewTemplate.xml";

        private AppDirs appDirs;                 // директории приложения
        private UiCustomization uiCustomization; // the customization object
        private string initialFileName;          // имя файла шаблона для открытия при запуске формы
        private string fileName;                 // имя файла шаблона устройства
        private bool saveOnly;                   // разрешена только команда сохранения при работе с файлами

        private DeviceTemplate template;
        private bool modified;

        public FrmDevTemplate_rttask()
        {
            InitializeComponent();

            appDirs = null;
            initialFileName = "";
            fileName = "";
            saveOnly = false;

            template = null;
            modified = false;
        }

        private bool Modified
        {
            get
            {
                return modified;
            }
            set
            {
                modified = value;
                SetFormTitle();
                //btnSave.Enabled = modified;
            }
        }

        private void SetFormTitle()
        {
            Text = KpPhrases.TemplFormTitle + " - " +
                (fileName == "" ? NewFileName : Path.GetFileName(fileName)) +
                (Modified ? "*" : "");
        }

        private void LoadTemplate(string fname)
        {
            template = uiCustomization.TemplateFactory.CreateDeviceTemplate();
            fileName = fname;
            SetFormTitle();
            string errMsg;

            DataTable dt = new DataTable();
            if (!template.Load(fname, out errMsg, ref dt))
                ScadaUiUtils.ShowError(errMsg);

            dataGridView1.Columns.Clear();
            dataGridView1.DataSource = dt;
            //FillGrid();
            //FillTree();
        }

        //不需要
        private void FillGrid()
        {

        }

        /// <summary>
        /// Отобразить форму модально
        /// </summary>
        public static void ShowDialog(AppDirs appDirs, UiCustomization uiCustomization)
        {
            string fileName = "";
            ShowDialog(appDirs, uiCustomization, false, ref fileName);
        }

        public static void ShowDialog(AppDirs appDirs, UiCustomization uiCustomization, bool saveOnly, ref string fileName)
        {
            FrmDevTemplate_rttask frmDevTemplate = new FrmDevTemplate_rttask()
            {
                appDirs = appDirs ?? throw new ArgumentNullException("appDirs"),
                uiCustomization = uiCustomization ?? throw new ArgumentNullException("uiCustomization"),
                initialFileName = fileName,
                saveOnly = saveOnly
            };

            frmDevTemplate.ShowDialog();
            fileName = frmDevTemplate.fileName;
        }

        private void Update_DataColumn_Type(DataTable dtSrc, DataTable dtUpdate)
        {
            try
            {
                foreach (DataColumn dcSrc in dtSrc.Columns)
                {
                    DataColumn dc = new DataColumn();
                    dc.ColumnName = dcSrc.ColumnName;

                    switch (dc.ColumnName)
                    {
                        case "变量名":
                            dc.DataType = typeof(string);
                            break;

                        case "数据类型":
                            dc.DataType = typeof(string);
                            break;

                        case "变量地址":
                            dc.DataType = typeof(string);
                            break;

                        case "变量描述":
                            dc.DataType = typeof(string);
                            break;

                        case "RTTASK数据类型":
                            dc.DataType = typeof(string);
                            break;

                        default:
                            break;
                    }

                    dtUpdate.Columns.Add(dc);
                }

                foreach (DataRow drSrc in dtSrc.Rows)
                {
                    if (dtSrc.Columns.Contains("mod"))
                    {
                        if ((string)drSrc["mod"] == "")
                        {
                            drSrc["mod"] = "0";
                        }
                    }

                    if (dtSrc.Columns.Contains("bank_enable"))
                    {
                        if ((string)drSrc["bank_enable"] == "")
                        {
                            drSrc["bank_enable"] = "1";
                        }
                    }

                    dtUpdate.Rows.Add(drSrc.ItemArray);
                }
            }
            catch (Exception rttask_csvimport_e)
            {
                MessageBox.Show(rttask_csvimport_e.Message, "csv import failed.", MessageBoxButtons.OK);
                LogHelpter.AddLog(rttask_csvimport_e.Message);
            }
        }

        private void openCsvToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog op = new OpenFileDialog
                {
                    Filter = "CSV Files(*.csv)|*.csv"
                };

                if (op.ShowDialog() == DialogResult.OK)
                {
                    if (op.FileName == "")
                    {
                        return;
                    }

                    DataTable dt = CSVFileHelper.OpenCSV(op.FileName);
                    DataTable dtUpdate = new DataTable();
                    Update_DataColumn_Type(dt, dtUpdate);
                    dataGridView1.DataSource = dtUpdate;
                }
            }
            catch (Exception open_csv_rttask_e)
            {
                MessageBox.Show(open_csv_rttask_e.Message, "open_csv失败！", MessageBoxButtons.OK);
                LogHelpter.AddLog(open_csv_rttask_e.Message);
            }
        }

        private void dataGridView1_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
        {
            Rectangle rectangle = new Rectangle(e.RowBounds.Location.X, e.RowBounds.Location.Y, dataGridView1.RowHeadersWidth - 4, e.RowBounds.Height);
            TextRenderer.DrawText(e.Graphics, (e.RowIndex + 1).ToString(),
                dataGridView1.RowHeadersDefaultCellStyle.Font,
                rectangle,
                dataGridView1.RowHeadersDefaultCellStyle.ForeColor,
                TextFormatFlags.VerticalCenter | TextFormatFlags.Right);
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveChanges(sender == saveToolStripMenuItem);
        }

        //不需要
        //更新所选组的元素的节点
        private void UpdateElemNodes(TreeNode grNode = null)
        {
        }

        //不需要
        //从指定的树节点开始更新组的KP元素的信号
        private void UpdateSignals(TreeNode startGrNode)
        {
        }

        //不需要
        private void UpdateCmdNode()
        {
            //if (selCmd != null)
            //    selNode.Text = GetCmdCaption(selCmd);
        }

        //不需要
        private void ShowElemGroupProps(ElemGroup elemGroup)
        {
            //ctrlElemGroup.Visible = true;
            //ctrlElemGroup.Settings = template.Sett;
            //ctrlElemGroup.ElemGroup = elemGroup;
            //ctrlElem.Visible = false;
            //ctrlCmd.Visible = false;
        }

        //不需要
        private void ShowElemProps(ElemInfo elemInfo)
        {
            //ctrlElemGroup.Visible = false;
            //ctrlElem.Visible = true;
            //ctrlElem.ElemInfo = elemInfo;
            //ctrlCmd.Visible = false;
        }

        //不需要
        private void ShowCmdProps(ModbusCmd modbusCmd)
        {
            //ctrlElemGroup.Visible = false;
            //ctrlElem.Visible = false;
            //ctrlCmd.Visible = true;
            //ctrlCmd.Settings = template.Sett;
            //ctrlCmd.ModbusCmd = modbusCmd;
        }

        //不需要
        private void DisableProps()
        {
            //ctrlElemGroup.ElemGroup = null;
            //ctrlElem.ElemInfo = null;
            //ctrlCmd.ModbusCmd = null;
        }

        private bool SaveChanges(bool saveAs)
        {
            // определение имени файла
            string newFileName = "";

            if (saveAs || fileName == "")
            {
                if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                    newFileName = saveFileDialog1.FileName;
            }
            else
            {
                newFileName = fileName;
            }

            if (newFileName == "")
            {
                return false;
            }
            else
            {
                // сохранение шаблона устройства
                string errMsg;
                if (template.Save(newFileName, out errMsg, (DataTable )dataGridView1.DataSource))
                {
                    fileName = newFileName;
                    Modified = false;
                    return true;
                }
                else
                {
                    ScadaUiUtils.ShowError(errMsg);
                    return false;
                }
            }
        }

        private bool CheckChanges()
        {
            if (modified)
            {
                DialogResult result = MessageBox.Show(KpPhrases.SaveTemplateConfirm,
                    CommonPhrases.QuestionCaption, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

                switch (result)
                {
                    case DialogResult.Yes:
                        return SaveChanges(false);
                    case DialogResult.No:
                        return true;
                    default:
                        return false;
                }
            }
            else
            {
                return true;
            }
        }

        private void FrmDevTemplate_rttask_Load(object sender, EventArgs e)
        {
            // перевод формы
            Translator.TranslateForm(this, "Scada.Comm.Devices.rttask.UI.FrmDevTemplate");
            openFileDialog1.SetFilter(KpPhrases.TemplateFileFilter);
            saveFileDialog1.SetFilter(KpPhrases.TemplateFileFilter);
            //TranslateTree();

            // настройка элементов управления
            openFileDialog1.InitialDirectory = appDirs.ConfigDir;
            saveFileDialog1.InitialDirectory = appDirs.ConfigDir;
            //btnEditSettingsExt.Visible = uiCustomization.ExtendedSettingsAvailable;
            //ctrlElem.Top = ctrlCmd.Top = ctrlElemGroup.Top;

            //if (saveOnly)
            //{
            //    btnNew.Visible = false;
            //    btnOpen.Visible = false;
            //}

            if (string.IsNullOrEmpty(initialFileName))
            {
                saveFileDialog1.FileName = NewFileName;
                template = uiCustomization.TemplateFactory.CreateDeviceTemplate();
                dataGridView1.Columns.Clear();
                //FillGrid();
                //FillTree();
            }
            else
            {
                saveFileDialog1.FileName = initialFileName;
                LoadTemplate(initialFileName);
            }
        }

        private void FrmDevTemplate_rttask_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = !CheckChanges();
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (CheckChanges())
            {
                saveFileDialog1.FileName = NewFileName;
                template = uiCustomization.TemplateFactory.CreateDeviceTemplate();
                fileName = "";
                SetFormTitle();

                dataGridView1.Columns.Clear();
                //FillGrid();
                //FillTree();
            }
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (CheckChanges())
            {
                openFileDialog1.FileName = "";

                if (openFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    saveFileDialog1.FileName = openFileDialog1.FileName;
                    LoadTemplate(openFileDialog1.FileName);
                }
            }
        }
    }
}
