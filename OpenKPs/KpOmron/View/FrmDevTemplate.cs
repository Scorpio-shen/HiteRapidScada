using KpCommon.Model;
using KpCommon.Model.EnumType;
using KpOmron.Model;
using KpOmron.Model.EnumType;
using Scada;
using Scada.Comm;
using Scada.UI;
using System;
using System.IO;
using System.Windows.Forms;

namespace KpOmron.View
{
    public partial class FrmDevTemplate : Form
    {
        const string NewFileName = "KpHiteOmron_NewTemplate.xml";
        string _fileName;                                   //载入已定义模板时文件名称或者新建的模板文件名称
        AppDirs _appDirs;
        DeviceTemplate deviceTemplate;                      //模板文件
        TagGroup tagGroup;                            //选中测点组的数据
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
            tagGroupRootNode = treeView.Nodes["tagGroupNode"];
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
                ctrlConfig.ConnectionOptions = deviceTemplate.CreateOptions();
                FillTree();
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

                FillTree();
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

        private void btnAddTagGroup_Click(object sender, EventArgs e)
        {
            var tagGroup = deviceTemplate.CreateTagGroup();
            int index = currentNode != null && currentNode.Tag is TagGroup ? currentNode.Index + 1 : deviceTemplate.TagGroups.Count;
            deviceTemplate.TagGroups.Insert(index,tagGroup);
            RefreshTagGroupIndex();

            TreeNode treeNode = NewTagGroupNode(tagGroup);
            tagGroupRootNode.Nodes.Insert(index, treeNode);
            treeView.SelectedNode = treeNode;
            ctrlRead.SetFocus();

            Modified = true;
        }


        private void btnMoveUp_Click(object sender, EventArgs e)
        {
            TreeNode prevNode = currentNode.PrevNode;
            int prevIndex = prevNode.Index;

            if(tagGroup != null)
            {
                var prevTagGroup = prevNode.Tag as TagGroup;
                deviceTemplate.TagGroups.RemoveAt(prevIndex);
                deviceTemplate.TagGroups.Insert(prevIndex + 1,prevTagGroup);

                tagGroupRootNode.Nodes.RemoveAt(prevIndex);
                tagGroupRootNode.Nodes.Insert(prevIndex + 1, prevNode);

                RefreshTagGroupIndex();
            }

            btnMoveUp.Enabled = currentNode.PrevNode != null;
            btnMoveDown.Enabled = currentNode.NextNode != null;
            Modified = true;
        }

        private void btnMoveDown_Click(object sender, EventArgs e)
        {
            TreeNode nextNode = currentNode.NextNode;
            int prevIndex = nextNode.Index;

            //在测点区域
            if (nextNode != null)
            {
                treeView.SelectedNode = nextNode;
            }

            btnMoveUp.Enabled = currentNode.PrevNode != null;
            btnMoveDown.Enabled = currentNode.NextNode != null;
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if(tagGroup != null)
            {
                deviceTemplate.TagGroups.Remove(tagGroup);
                RefreshTagGroupIndex();
                tagGroupRootNode.Nodes.Remove(currentNode);
            }
            Modified = true;
        }

        private void treeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            currentNode = e.Node;
            var tag = currentNode.Tag;
            tagGroup = tag as TagGroup;

            if(currentNode == tagGroupRootNode)
                btnDelete.Enabled = false;
            else
                btnMoveUp.Enabled = true;

            if (tagGroup != null)
                ShowTagGroupProps(tagGroup);
            else
                HideGroupProps();


            bool nodeIsSelect = tagGroup != null;
            btnMoveUp.Enabled = nodeIsSelect && currentNode.PrevNode != null;
            btnMoveDown.Enabled = nodeIsSelect && currentNode.NextNode != null;
            btnDelete.Enabled = nodeIsSelect;
            btnImport.Enabled = nodeIsSelect;
            btnExport.Enabled = nodeIsSelect;
            ctrlConfig.Enabled = !nodeIsSelect;
        }

        /// <summary>
        /// 导入
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnImport_Click(object sender, EventArgs e)
        {
            var openFile = new OpenFileDialog();
            openFile.SetFilter(KpCommon.Model.TempleteKeyString.OpenExcelFilterStr);
            var reuslt = openFile.ShowDialog();
            if (reuslt != DialogResult.OK)
                return;
            Modified = true;
            var filePath = openFile.FileName;
            if (tagGroup != null)
            {
                ctrlRead.ImportExcel(filePath);
            }

        }
        /// <summary>
        /// 导出
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnExport_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFile = new SaveFileDialog();
            saveFile.SetFilter(KpCommon.Model.TempleteKeyString.OpenExcelFilterStr);
            var reuslt = saveFile.ShowDialog();
            if (reuslt != DialogResult.OK)
                return;
            var filePath = saveFile.FileName;
            var extensionName = Path.GetExtension(filePath);
            if (tagGroup != null)
            {
                ctrlRead.ExportExcel(filePath, extensionName);
            }
        }
        #endregion

        #region TagGroup、Cmd事件
        private void ctrlRead_TagGroupChanged(object sender, ConfigChangedEventArgs<Tag> e)
        {
            Modified = true;
            if(tagGroup != null)
            {
                if(currentNode != null)
                {
                    switch (e.ModifyType)
                    {
                        case ModifyType.TagCount:
                            RefreshTagGroupIndex();
                            break;
                        case ModifyType.GroupName:
                            currentNode.Text = GetTagGroupDesc(tagGroup);
                            break;
                        case ModifyType.IsActive:
                            currentNode.ImageKey = currentNode.SelectedImageKey = tagGroup.Active ? "group.png" : "group_inactive.png";
                            break;
                        case ModifyType.MemoryType:
                            currentNode.Text = GetTagGroupDesc(tagGroup);
                            break;
                        case ModifyType.Tags:
                            break;
                        default:
                            return;
                    }
                    
                    
                }
            }
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
            ctrlConfig.ConnectionOptions = deviceTemplate.ConnectionOptions;
            FillTree();
        }

        private void ShowTagGroupProps(TagGroup tagGroup)
        {
            ctrlRead.Visible = true;
            ctrlRead.TagGroup = tagGroup;
        }


        private void HideGroupProps()
        {
            ctrlRead.Visible = false;
        }
        #endregion

        #region 左侧TreeView显示
        private void FillTree()
        {
            tagGroup = null;

            treeView.BeginUpdate();
            tagGroupRootNode.Nodes.Clear();

            foreach (TagGroup tagGroup in deviceTemplate.TagGroups)
                tagGroupRootNode.Nodes.Add(NewTagGroupNode(tagGroup));

            tagGroupRootNode.Expand();
            treeView.EndUpdate();
        }

        private TreeNode NewTagGroupNode(TagGroup tagGroup)
        {
            //tagGroup.Name = "点组";
            TreeNode node = new TreeNode(GetTagGroupDesc(tagGroup));
            node.ImageKey = node.SelectedImageKey = tagGroup.Active ? "group.png" : "group_inactive.png";
            node.Tag = tagGroup;

            return node;
        }

        private string GetTagGroupDesc(TagGroup tagGroup)
        {
            var groupName = string.IsNullOrEmpty(tagGroup.Name) ? KpCommon.Model.TempleteKeyString.DefaultTagGroupName : tagGroup.Name;
            var registerType = tagGroup.MemoryType;
            return $"{groupName} ({registerType})";
        }

        #endregion

        #region TagGroupIndex刷新
        private void RefreshTagGroupIndex()
        {
            deviceTemplate.RefreshTagGroupIndex();
            //ctrlRead.RefreshDataGridView();
        }
        #endregion
    }
}
