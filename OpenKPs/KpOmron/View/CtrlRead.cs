using KpCommon.Extend;
using KpCommon.Helper;
using KpCommon.Model;
using KpCommon.Model.EnumType;
using KpOmron.Extend;
using KpOmron.Model;
using KpOmron.Model.EnumType;
using Scada.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace KpOmron.View
{

    public partial class CtrlRead : UserControl
    {
        public event ConfigChangedEventHandler<Tag> TagGroupChanged;
        private TagGroup tagGroup;
        private ComboBox comboBox;

        BindingSource bindSourceDataTypeOnlyBool;
        BindingSource bindSourceDataTypeExceptBool;
        public TagGroup TagGroup
        {
            get => tagGroup;
            set
            {
                tagGroup = value;
                IsShowTagGroup = true;
                ShowTagGroup();
                BindTagGroupTags(tagGroup);
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
            cbxRegisterType.DataSource = bindingSource;
            cbxRegisterType.DisplayMember = "Key";
            cbxRegisterType.ValueMember = "Value";

            //绑定 datagridView 数据源
            dgvTags.DataSource = bdsTags;
        }
        

        private void InitControl()
        {

            //numTagCount.Maximum = ushort.MaxValue;
            //numTagCount.Minimum = 0;

            dgvTags.AutoGenerateColumns = false;

            comboBox = new ComboBox();
            Dictionary<string, DataTypeEnum> keyValueDataTypeEnumsOnlyBool = new Dictionary<string, DataTypeEnum>();
            keyValueDataTypeEnumsOnlyBool.Add(DataTypeEnum.Bool.ToString(), DataTypeEnum.Bool);
            bindSourceDataTypeOnlyBool = new BindingSource();
            bindSourceDataTypeOnlyBool.DataSource = keyValueDataTypeEnumsOnlyBool;

            Dictionary<string, DataTypeEnum> keyValueDataTypeEnumsExceptBool = new Dictionary<string, DataTypeEnum>();
            foreach (DataTypeEnum type in Enum.GetValues(typeof(DataTypeEnum)))
                if(type != DataTypeEnum.Bool)
                    keyValueDataTypeEnumsExceptBool.Add(type.ToString(), type);
            bindSourceDataTypeExceptBool = new BindingSource();
            bindSourceDataTypeExceptBool.DataSource = keyValueDataTypeEnumsExceptBool;

            comboBox.DataSource = bindSourceDataTypeExceptBool;
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
            if (TagGroup == null)
                return;

            txtGroupName.AddDataBindings(TagGroup, nameof(TagGroup.Name));
            chkActive.AddDataBindings(TagGroup, nameof(TagGroup.Active));
            cbxRegisterType.AddDataBindings(TagGroup, nameof(TagGroup.MemoryType));
            lblTagCount.AddDataBindings(TagGroup, nameof(TagGroup.TagCount));
            txtMaxAddressLength.AddDataBindings(TagGroup, nameof(TagGroup.MaxRequestByteLength));

            chkAllCanWrite.Checked = TagGroup.AllCanWrite;

            //txtGroupName.Text = ModbusTagGroup.Name;
            //chkActive.Checked = ModbusTagGroup.Active;
            //cbxRegisterType.SelectedValue = ModbusTagGroup.RegisterType;
            //numTagCount.Value = ModbusTagGroup.TagCount;
            //txtMaxAddressLength.Text = ModbusTagGroup.MaxRequestByteLength.ToString();
        }

        private void BindTagGroupTags(TagGroup group)
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
            if (e.PropertyName != nameof(TagGroup.TagCount))
                return;
            TagGroupChanged?.Invoke(sender, new ConfigChangedEventArgs<Tag>
            {
                ModifyType = ModifyType.Tags,
                TagGroup = TagGroup,
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
                TagGroup.RefreshTagIndex();
            }
            TagGroupChanged?.Invoke(sender,new  ConfigChangedEventArgs<Tag>
            {
                ModifyType = ModifyType.Tags,
                TagGroup = TagGroup,
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
        //    if (ModbusTagGroup == null)
        //        return;
        //    if(IsShowTagGroup)  //只是展示属性部分,不走后面逻辑
        //        return;
        //    var oldTagCount = ModbusTagGroup.TagCount;
        //    var newTagCount = (int)numTagCount.Value;

        //    if(oldTagCount < newTagCount)
        //    {
        //        List<Tag> tempTags = new List<Tag>();
        //        for(int i = oldTagCount; i < newTagCount; i++)
        //        {
        //            tempTags.Add(Model.Tag.CreateNewTag());
        //        }

        //        ModbusTagGroup.CheckAndAddTags(tempTags,out string errorMsg,)
        //    }
        //    else if(oldTagCount > newTagCount)
        //    {
        //        for(int i = newTagCount; i < oldTagCount; i++)
        //        {
        //            ModbusTagGroup.Tags.RemoveAt(i);
        //        }
        //    }

        //    bdsTags.ResetBindings(false);
        //    ModbusTagGroup.RefreshTagIndex();
        //    OnTagGroupChanged(sender, ModifyType.TagCount);
        //}

        private void chkAllCanWrite_CheckedChanged(object sender, EventArgs e)
        {
            if (TagGroup == null)
                return;
            if (IsShowTagGroup)
                return;

            bool canwrite = chkAllCanWrite.Checked;
            TagGroup.SetTagCanWrite(canwrite);
            RefreshDataGridView(false);
        }

        private void TxtGroupName_TextChanged(object sender, EventArgs e)
        {
            if (TagGroup == null)
                return;
            if (IsShowTagGroup)
                return;
            TagGroup.Name =txtGroupName.Text;
            //CtrlReadViewModel.GroupName = txtGroupName.Text; //Winform 虽然绑定了，但是得在焦点移开当前控件，Model里面的值才能变成textbox控件里的值,所以这里手动赋值
            TagGroupChanged?.Invoke(sender,new ConfigChangedEventArgs<Tag>
            {
                ModifyType = ModifyType.GroupName,
                TagGroup = TagGroup
            });
        }

        private void chkActive_CheckedChanged(object sender, EventArgs e)
        {
            if(TagGroup == null) return;
            if(IsShowTagGroup)
                return;
            TagGroupChanged?.Invoke(sender, new ConfigChangedEventArgs<Tag>
            {
                ModifyType = ModifyType.IsActive,
                TagGroup = TagGroup
            });
        }

        private void cbxRegisterType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (TagGroup == null)
                return;
            
            var registerType = cbxRegisterType.SelectedValue as MemoryTypeEnum?;
            if (registerType == null)
                return;

            RefreshDataGridView();
            if (IsShowTagGroup)
                return;
            OnTagGroupChanged(sender, ModifyType.MemoryType);
        }

        private void btnAddRange_Click(object sender, EventArgs e)
        {
            var frm = new FrmDevAddRange(TagGroup);
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
                var startAddress = TagGroup.StartAddress;

                double requestLength = default;
                if(inputValue >= startAddress)
                    requestLength = inputValue - startAddress + dataLength;
                else
                    requestLength = dataLength;

                if (requestLength > TagGroup.MaxRequestByteLength)
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
            //ModbusTagGroup.RefreshTagAddress();
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
        public void OnTagGroupChanged(object sender,ModifyType changeType)
        {
            TagGroupChanged?.Invoke(sender,new ConfigChangedEventArgs<Tag>
            {
                ModifyType = changeType,
                TagGroup = TagGroup
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
            //numTagCount.Value = ModbusTagGroup.TagCount;
            //刷新地址显示
            TagGroup.RefreshTagIndex();
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

                if (!TagGroup.CheckAndAddTags(listTagsDouble, out string errorMsg,true))
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
                var exportTags = TagGroup.Tags;
                if (exportTags.Count == 0)
                {
                    exportTags = new List<Tag>
                    {
                        //当前导出为空模板时添加两条空数据,用于指示用户使用
                        Model.Tag.CreateNewTag(tagID: 1, tagname: "xx1", dataType: DataTypeEnum.UInt, memoryType: MemoryTypeEnum.D, address: "1", canwrite: true),
                        Model.Tag.CreateNewTag(tagID: 2, tagname: "xx2", dataType: DataTypeEnum.UInt, memoryType: MemoryTypeEnum.D, address: "2", canwrite: true)
                    };
                }
                using (var ms = ExcelHelper.ToExcel(exportTags, excelType))
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
