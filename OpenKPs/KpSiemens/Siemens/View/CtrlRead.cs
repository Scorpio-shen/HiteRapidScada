﻿using KpCommon.Extend;
using KpCommon.Helper;
using KpCommon.Model;
using KpSiemens.Siemens.Extend;
using KpSiemens.Siemens.Model;
using Scada.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace KpSiemens.Siemens.View
{

    public partial class CtrlRead : UserControl
    {
        public event TagGroupChangedEventHandler TagGroupChanged;
        private SiemensTagGroup siemensTagGroup;
        private ComboBox comboBox;
        public SiemensTagGroup SiemensTagGroup
        {
            get => siemensTagGroup;
            set
            {
                siemensTagGroup = value;
                IsShowTagGroup = true;
                ShowTagGroup();
                BindTagGroupTags(siemensTagGroup);
                IsShowTagGroup = false;
            }
        }
        /// <summary>
        /// 是否是展示TagGroup内容,只是展示内容不触发控件事件
        /// </summary>
        public bool IsShowTagGroup { get; set; } = false;
        public CtrlRead()
        {
            InitializeComponent();

            //初始化下拉框数据源
            InitDataSourceBind();
            //初始化控件
            InitControl();
            
        }

        #region 初始化部分
        private void InitDataSourceBind()
        {
            //绑定存储器类
            Dictionary<string, MemoryTypeEnum> keyValueMemoryEnums = new Dictionary<string, MemoryTypeEnum>();
            foreach (MemoryTypeEnum type in Enum.GetValues(typeof(MemoryTypeEnum)))
                keyValueMemoryEnums.Add(type.ToString(), type);

            BindingSource bindingSource = new BindingSource();
            bindingSource.DataSource = keyValueMemoryEnums;
            cbxMemoryType.DataSource = bindingSource;
            cbxMemoryType.DisplayMember = "Key";
            cbxMemoryType.ValueMember = "Value";

            //绑定 datagridView 数据源
            dgvTags.DataSource = bdsTags;
        }
        

        private void InitControl()
        {
            numDbNum.Maximum = ushort.MaxValue;
            numDbNum.Minimum = 1;

            //numTagCount.Maximum = ushort.MaxValue;
            //numTagCount.Minimum = 0;

            dgvTags.AutoGenerateColumns = false;

            comboBox = new ComboBox();
            Dictionary<string, DataTypeEnum> keyValueDataTypeEnums = new Dictionary<string, DataTypeEnum>();
            foreach (DataTypeEnum type in Enum.GetValues(typeof(DataTypeEnum)))
                keyValueDataTypeEnums.Add(type.ToString(), type);

            BindingSource bindingSourceDataType = new BindingSource();
            bindingSourceDataType.DataSource = keyValueDataTypeEnums;
            comboBox.DataSource = bindingSourceDataType;
            comboBox.DisplayMember = "Key";
            comboBox.ValueMember = "Value";

            comboBox.Leave += ComboBox_Leave;
            comboBox.SelectedIndexChanged += ComboBox_SelectedIndexChanged;
            comboBox.Visible = false;
            comboBox.DropDownStyle = ComboBoxStyle.DropDownList;

            dgvTags.Controls.Add(comboBox);
        }

        
        #endregion

        private void ShowTagGroup()
        {
            if (SiemensTagGroup == null)
                return;
            txtGroupName.AddDataBindings(SiemensTagGroup, nameof(SiemensTagGroup.Name));
            chkActive.AddDataBindings(SiemensTagGroup,nameof(SiemensTagGroup.Active));
            cbxMemoryType.AddDataBindings(SiemensTagGroup, nameof(SiemensTagGroup.MemoryType));
            lblTagCount.AddDataBindings(SiemensTagGroup, nameof(SiemensTagGroup.TagCount));
            txtMaxAddressLength.AddDataBindings(SiemensTagGroup, nameof(SiemensTagGroup.MaxRequestByteLength));
            //chkAllCanWrite.AddDataBindings(SiemensTagGroup, nameof(SiemensTagGroup.AllCanWrite));
            lblDbNum.Visible = numDbNum.Visible = SiemensTagGroup.MemoryType == MemoryTypeEnum.DB;

            chkAllCanWrite.Checked = SiemensTagGroup.AllCanWrite;
            //txtGroupName.Text = SiemensTagGroup.Name;
            //chkActive.Checked = SiemensTagGroup.Active;
            //cbxMemoryType.SelectedValue = SiemensTagGroup.MemoryType;
            //lblDbNum.Visible = numDbNum.Visible = SiemensTagGroup.MemoryType == MemoryTypeEnum.DB;
            //numDbNum.Value = SiemensTagGroup.DBNum < 1 ? 1 : SiemensTagGroup.DBNum;
            //numTagCount.Value = SiemensTagGroup.TagCount;
            //txtMaxAddressLength.Text = SiemensTagGroup.MaxRequestByteLength.ToString();
        }

        private void BindTagGroupTags(SiemensTagGroup group)
        {
            if(group == null)
                return ;
            bdsTags.DataSource = null;
            bdsTags.DataSource = group.Tags;

            //变化通知
            foreach(var tag in group.Tags)
            {
                tag.PropertyChanged -= Tag_PropertyChanged;
                tag.PropertyChanged += Tag_PropertyChanged;
            }

            //tags数目变化通知
            group.PropertyChanged -= TagGroup_PropertyChanged;
            group.PropertyChanged += TagGroup_PropertyChanged;
        }

        private void TagGroup_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (IsShowTagGroup)
                return;

            if (e.PropertyName != nameof(SiemensTagGroup.TagCount))
                return;

            TagGroupChanged?.Invoke(sender, new TagGroupChangedEventArgs
            {
                ModifyType = ModifyType.Tags,
                ViewModel = SiemensTagGroup
            });
        }

        /// <summary>
        /// 修改datagridview中tag内容通知父窗体修改按钮使能
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Tag_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var tag = sender as Tag;
            if (tag == null)
                return;
            if (e.PropertyName.Equals(nameof(tag.Address)))
            {
                //地址变化需要对当前数组进行重新排序
                siemensTagGroup.RefreshTagIndex();
            }
            TagGroupChanged?.Invoke(sender, new TagGroupChangedEventArgs
            {
                ModifyType = ModifyType.Tags,
                ViewModel = siemensTagGroup
            });
        }

        public void SetFocus()
        {
            txtGroupName.Select();
        }
        /// <summary>
        /// 刷新绑定datagridview数据源
        /// </summary>
        public void RefreshDataGridView(bool needResetBindTags = true)
        {
            try
            {
                var index = dgvTags.FirstDisplayedScrollingRowIndex;
                if (needResetBindTags)
                    bdsTags.ResetBindings(false);
                dgvTags.Invalidate();
                if (index >= 0)
                    dgvTags.FirstDisplayedScrollingRowIndex = index;
            }
            catch { }
        }

        #region 控件事件
        //private void NumTagCount_ValueChanged(object sender, EventArgs e)
        //{
        //    if (SiemensTagGroup == null)
        //        return;
        //    if(IsShowTagGroup)  //只是展示属性部分,不走后面逻辑
        //        return;
        //    var oldTagCount = SiemensTagGroup.TagCount;
        //    var newTagCount = (int)numTagCount.Value;

        //    if(oldTagCount < newTagCount)
        //    {
        //        for(int i = oldTagCount; i < newTagCount; i++)
        //        {
        //            var tag = Model.Tag.CreateNewTag();
        //            SiemensTagGroup.Tags.Add(tag);
        //        }
        //    }
        //    else if(oldTagCount > newTagCount)
        //    {
        //        for(int i = newTagCount; i < oldTagCount; i++)
        //        {
        //            SiemensTagGroup.Tags.RemoveAt(i);
        //        }
        //    }

        //    bdsTags.ResetBindings(false);
        //    SiemensTagGroup.RefreshTagIndex();
        //    OnTagGroupChanged(sender, ModifyType.TagCount);
        //}

        private void chkActive_CheckedChanged(object sender, EventArgs e)
        {
            if (SiemensTagGroup == null)
                return;
            if (IsShowTagGroup)
                return;

            OnTagGroupChanged(sender, ModifyType.IsActive);
        }

        private void chkAllCanWrite_CheckedChanged(object sender, EventArgs e)
        {
            if (SiemensTagGroup == null)
                return;
            if (IsShowTagGroup)
                return;

            bool canwrite = chkAllCanWrite.Checked;
            SiemensTagGroup.SetTagCanWrite(canwrite);
            RefreshDataGridView(false);
        }


        private void TxtGroupName_TextChanged(object sender, EventArgs e)
        {
            if (SiemensTagGroup == null)
                return;
            if (IsShowTagGroup)
                return;
            SiemensTagGroup.Name =txtGroupName.Text;
            //CtrlReadViewModel.GroupName = txtGroupName.Text; //Winform 虽然绑定了，但是得在焦点移开当前控件，Model里面的值才能变成textbox控件里的值,所以这里手动赋值
            OnTagGroupChanged(sender, ModifyType.GroupName);
        }
        private void cbxMemoryType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (SiemensTagGroup == null)
                return;
            var memoryType = cbxMemoryType.SelectedValue as MemoryTypeEnum?;
            if (memoryType == null)
                return;
            numDbNum.Visible = lblDbNum.Visible = memoryType == MemoryTypeEnum.DB;
            if(SiemensTagGroup.MemoryType != memoryType)
                SiemensTagGroup.MemoryType = (MemoryTypeEnum)memoryType;

            if(memoryType == MemoryTypeEnum.I)
            {
                SiemensTagGroup.SetTagCanWrite(false);
                chkAllCanWrite.Checked = false;
                chkAllCanWrite.Enabled = false;
                dgvTagCanWrite.ReadOnly = true;

                RefreshDataGridView(false);
            }
            else
            {
                chkAllCanWrite.Enabled = true;
                dgvTagCanWrite.ReadOnly = false;
            }

            if (IsShowTagGroup)
                return;
            OnTagGroupChanged(sender, ModifyType.MemoryType);
        }

        private void numDbNum_ValueChanged(object sender, EventArgs e)
        {
            if (SiemensTagGroup == null)
                return;
            if (IsShowTagGroup)
                return;
            //SiemensTagGroup.DBNum = (int)numDbNum.Value;
            OnTagGroupChanged(sender, ModifyType.DBNum);

        }

        private void btnAddRange_Click(object sender, EventArgs e)
        {
            var frm = new FrmDevAddRange(siemensTagGroup);
            var dialogResult = frm.ShowDialog();
            if (dialogResult == DialogResult.Cancel)
                return;

            //界面显示刷新
            RefreshDataGridView();
        }

        private void dgvTags_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            var indexofAddress = dgvTagAddress.Index;
            if(e.ColumnIndex == indexofAddress)
            {
                //修改地址列
                if(!double.TryParse(e.FormattedValue?.ToString(),out double inputValue))
                    e.Cancel = true;

                //获取到匹配的Tag
                var currentTag = bdsTags.Current as Tag;
                if (currentTag == null)
                    return;
                //获取数据占据字节长度
                var dataLength = currentTag.DataType.GetByteCount() + currentTag.Length;
                //验证是否超出
                var startAddress = siemensTagGroup.StartAddress;

                double requestLength = default;
                if(inputValue >= startAddress)
                    requestLength = inputValue - startAddress + dataLength;
                else
                    requestLength = dataLength;

                if (requestLength > SiemensTagGroup.MaxRequestByteLength)
                {
                    //超出最大地址限制
                    ScadaUiUtils.ShowError(KpCommon.Model.TempleteKeyString.RangeOutOfMaxRequestLengthErrorMsg);
                    e.Cancel = true;
                }
            }
        }

        #region DataGridView下拉框部分
        private void ComboBox_Leave(object sender, EventArgs e)
        {
            comboBox.Visible = false;
        }

        private void ComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            var currentCell = dgvTags.CurrentCell;
            if(currentCell != null)
            {
                currentCell.Value = ((ComboBox)sender).SelectedValue;
            }

            OnTagGroupChanged(sender, ModifyType.Tags);
            //siemensTagGroup.RefreshTagAddress();
            //bdsTags.ResetBindings(false);
        }
        private void dgvTags_CurrentCellChanged(object sender, EventArgs e)
        {

            var currentCell = dgvTags.CurrentCell;
            if (currentCell == null)
            {
                comboBox.Visible = false;
                return;
            }
            if (!currentCell.OwningColumn.HeaderText.Equals(dgvTagDataType.HeaderText))
            {
                comboBox.Visible = false;
                return;
            }

            try
            {
                Rectangle rectangle = dgvTags.GetCellDisplayRectangle(currentCell.ColumnIndex, currentCell.RowIndex, false);

                string value = currentCell.Value.ToString();
                comboBox.Text = value;
                comboBox.Left = rectangle.Left;
                comboBox.Top = rectangle.Top;
                comboBox.Width = rectangle.Width;
                comboBox.Height = rectangle.Height;
                comboBox.Visible = true;
            }
            catch
            {
                return;
            }
        }
        #endregion
        public void OnTagGroupChanged(object sender,ModifyType modifyType)
        {
            TagGroupChanged?.Invoke(sender, new TagGroupChangedEventArgs
            {
                ViewModel = SiemensTagGroup,
                ModifyType = modifyType
            });
        }


        private void deleteTStripItem_Click(object sender, EventArgs e)
        {
            var index = dgvTags.CurrentCell?.RowIndex ?? -1;
            if(index == -1)
                return;
            if (index >= dgvTags.RowCount)
                return;
            dgvTags.Rows.RemoveAt(index);
            //numTagCount.Value = SiemensTagGroup.TagCount;
            //lblTagCount.Text = SiemensTagGroup.TagCount.ToString();
            //刷新地址显示
            siemensTagGroup.RefreshTagIndex();
        }
        #endregion

        #region 导入、导出Excel
        /// <summary>
        /// 导入Excel文件
        /// </summary>
        /// <param name="filePath"></param>
        public void ImportExcel(string filePath)
        {
            if(!File.Exists(filePath))
            {
                ScadaUiUtils.ShowError("文件不存在!");
                return;
            }
            try
            {
                var listTags = ExcelHelper.GetListExtend<Tag>(filePath, 0);
                //刷新部分控件显示
                
                //对listTags排序，按地址从小到大
                var listTagsDouble = listTags.Where(t => double.TryParse(t.Address, out double val)).ToList();
                //数据添加到对象

                if (!siemensTagGroup.CheckAndAddTags(listTagsDouble,out string errorMsg, true))
                {
                    ScadaUiUtils.ShowError(errorMsg);
                    return;
                }
                //界面显示刷新
                RefreshDataGridView();
            }
            catch(Exception ex)
            {
                ScadaUiUtils.ShowError($"导入异常,{ex.Message}");
            }
        }
        /// <summary>
        /// 导出Excel
        /// </summary>
        /// <param name="filePath"></param>
        public void ExportExcel(string filePath,string excelType)
        {
            var directory = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);
            try
            {
                var exportTags = SiemensTagGroup.Tags;
                if (exportTags.Count == 0)
                {
                    exportTags = new List<Tag>
                    {
                        //当前导出为空模板时添加两条空数据,用于指示用户使用
                        Model.Tag.CreateNewTag(tagID: 1, tagname: "xx1", dataType: DataTypeEnum.Int, memoryType: MemoryTypeEnum.DB, address: "0", canwrite: 1),
                        Model.Tag.CreateNewTag(tagID: 2, tagname: "xx2", dataType: DataTypeEnum.Int, memoryType: MemoryTypeEnum.DB, address: "0", canwrite: 2)
                    };
                }
                    
                using (var ms = ExcelHelper.ToExcel(SiemensTagGroup.Tags,excelType))
                {
                    using (var fileStream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                    {
                        using (var binaryWrite = new BinaryWriter(fileStream))
                        {
                            binaryWrite.Write(ms.ToArray());
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                ScadaUiUtils.ShowError($"导出异常,{ex.Message}");
            }
        }

        #endregion

       
    }
}
