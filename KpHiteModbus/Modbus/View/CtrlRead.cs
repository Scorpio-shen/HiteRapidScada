using KpHiteModbus.Modbus.Extend;
using KpHiteModbus.Modbus.Model;
using KpHiteModbus.Modbus.Model.EnumType;
using Scada.Helper;
using Scada.KPModel.Extend;
using Scada.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace KpHiteModbus.Modbus.View
{

    public partial class CtrlRead : UserControl
    {
        public event ModbusConfigChangedEventHandler TagGroupChanged;
        private ModbusTagGroup tagGroup;
        private ComboBox comboBox;

        BindingSource bindSourceDataTypeOnlyBool;
        BindingSource bindSourceDataTypeExceptBool;
        public ModbusTagGroup ModbusTagGroup
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
            Dictionary<string, RegisterTypeEnum> keyValueMemoryEnums = new Dictionary<string, RegisterTypeEnum>();
            foreach (RegisterTypeEnum type in Enum.GetValues(typeof(RegisterTypeEnum)))
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

            numTagCount.Maximum = ushort.MaxValue;
            numTagCount.Minimum = 0;

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
            if (ModbusTagGroup == null)
                return;

            txtGroupName.AddDataBindings(ModbusTagGroup, nameof(ModbusTagGroup.Name));
            chkActive.AddDataBindings(ModbusTagGroup, nameof(ModbusTagGroup.Active));
            cbxRegisterType.AddDataBindings(ModbusTagGroup, nameof(ModbusTagGroup.RegisterType));
            numTagCount.AddDataBindings(ModbusTagGroup, nameof(ModbusTagGroup.TagCount));
            txtMaxAddressLength.AddDataBindings(ModbusTagGroup, nameof(ModbusTagGroup.MaxRequestByteLength));
            txtRquestLength.AddDataBindings(ModbusTagGroup, nameof(ModbusTagGroup.RequestLength));

            //txtGroupName.Text = ModbusTagGroup.Name;
            //chkActive.Checked = ModbusTagGroup.Active;
            //cbxRegisterType.SelectedValue = ModbusTagGroup.RegisterType;
            //numTagCount.Value = ModbusTagGroup.TagCount;
            //txtMaxAddressLength.Text = ModbusTagGroup.MaxRequestByteLength.ToString();
        }

        private void BindTagGroupTags(ModbusTagGroup group)
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
                ModbusTagGroup.RefreshTagAddress();
            }
            TagGroupChanged?.Invoke(sender,new ModbusConfigChangedEventArgs
            {
                ChangeType = ModifyType.Tags,
                TagGroup = ModbusTagGroup,
            });
        }

        public void SetFocus()
        {
            txtGroupName.Select();
        }
        /// <summary>
        /// 刷新绑定datagridview数据源
        /// </summary>
        public void RefreshDataGridView()
        {
            dgvTags.Invalidate();
        }

        #region 控件事件
        private void NumTagCount_ValueChanged(object sender, EventArgs e)
        {
            if (ModbusTagGroup == null)
                return;
            if(IsShowTagGroup)  //只是展示属性部分,不走后面逻辑
                return;
            var oldTagCount = ModbusTagGroup.TagCount;
            var newTagCount = (int)numTagCount.Value;

            if(oldTagCount < newTagCount)
            {
                for(int i = oldTagCount; i < newTagCount; i++)
                {
                    var tag = Model.Tag.CreateNewTag();
                    ModbusTagGroup.Tags.Add(tag);
                }
            }
            else if(oldTagCount > newTagCount)
            {
                for(int i = newTagCount; i < oldTagCount; i++)
                {
                    ModbusTagGroup.Tags.RemoveAt(i);
                }
            }
            
            bdsTags.ResetBindings(false);
            OnTagGroupChanged(sender, ModifyType.TagCount);
        }



        private void TxtGroupName_TextChanged(object sender, EventArgs e)
        {
            if (ModbusTagGroup == null)
                return;
            if (IsShowTagGroup)
                return;
            ModbusTagGroup.Name =txtGroupName.Text;
            //CtrlReadViewModel.GroupName = txtGroupName.Text; //Winform 虽然绑定了，但是得在焦点移开当前控件，Model里面的值才能变成textbox控件里的值,所以这里手动赋值
            TagGroupChanged?.Invoke(sender,new ModbusConfigChangedEventArgs
            {
                ChangeType = Model.EnumType.ModifyType.GroupName,
                TagGroup = ModbusTagGroup
            });
        }

        private void cbxRegisterType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ModbusTagGroup == null)
                return;
            
            var registerType = cbxRegisterType.SelectedValue as RegisterTypeEnum?;
            if (registerType == null)
                return;

            //根据不同的类型使能读写属性列
            if(registerType == RegisterTypeEnum.Coils)
            {
                comboBox.DataSource = bindSourceDataTypeOnlyBool;
                dgvTagCanWrite.ReadOnly = false;
            }
            else if(registerType == RegisterTypeEnum.DiscretesInputs)
            {
                comboBox.DataSource = bindSourceDataTypeOnlyBool;
                dgvTagCanWrite.ReadOnly = true;
            }
            else if(registerType == RegisterTypeEnum.HoldingRegisters)
            {
                comboBox.DataSource = bindSourceDataTypeExceptBool;
                dgvTagCanWrite.ReadOnly = false;
            }
            else
            {
                comboBox.DataSource = bindSourceDataTypeExceptBool;
                dgvTagCanWrite.ReadOnly = true;
            }

            RefreshDataGridView();
            if (IsShowTagGroup)
                return;
            OnTagGroupChanged(sender, ModifyType.RegisterType);
        }

        private void btnAddRange_Click(object sender, EventArgs e)
        {
            var frm = new FrmDevAddRange(ModbusTagGroup);
            var dialogResult = frm.ShowDialog();
            if (dialogResult == DialogResult.Cancel)
                return;

            //界面显示刷新
            numTagCount.Value = ModbusTagGroup.TagCount;
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
                var startAddress = ModbusTagGroup.StartAddress;

                double requestLength = default;
                if(inputValue >= startAddress)
                    requestLength = inputValue - startAddress + dataLength;
                else
                    requestLength = dataLength;

                if (requestLength > ModbusTagGroup.MaxRequestByteLength)
                {
                    //超出最大地址限制
                    ScadaUiUtils.ShowError(TempleteKeyString.RangeOutOfMaxRequestLengthErrorMsg);
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
            TagGroupChanged?.Invoke(sender,new ModbusConfigChangedEventArgs
            {
                ChangeType = changeType,
                TagGroup = ModbusTagGroup
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
            numTagCount.Value = ModbusTagGroup.TagCount;
            //刷新地址显示
            ModbusTagGroup.RefreshTagAddress();
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

                if (!ModbusTagGroup.CheckAndAddTags(listTagsDouble, true))
                {
                    ScadaUiUtils.ShowError(TempleteKeyString.RangeOutOfMaxRequestLengthErrorMsg);
                    return;
                }
                //界面显示刷新
                numTagCount.Value = ModbusTagGroup.TagCount;
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
                using (var ms = ExcelHelper.ToExcel( ModbusTagGroup.Tags, excelType))
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
