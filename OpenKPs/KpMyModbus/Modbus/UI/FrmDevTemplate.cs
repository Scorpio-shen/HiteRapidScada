using HslCommunication.Profinet.Panasonic.Helper;
using KpMyModbus.Modbus;
using KpMyModbus.Modbus.Protocol;
using KpMyModbus.Modbus.UI;
using Scada.UI;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using System.Xml;

namespace Scada.Comm.Devices.Modbus.UI
{
    public partial class FrmDevTemplate : Form
    {
        private const string NewFileName = "KpModbus_NewTemplate.xml";

        private string initialFileName;
        private string fileName;
        private bool modified;

        MyTagGroup myTagGroup;
        List<ModbusCmd> modbusCmds;
        DeviceTemplate deviceTemplate;
        public bool Modified
        {
            get => modified;
            set
            {
                modified = value;
                btnSave.Enabled = modified;
            }
        }  
        private FrmDevTemplate()
        {
            InitializeComponent();
        }

        public static void ShowDialog(AppDirs appDirs)
        {
            string fileName = "";
            ShowDialog(appDirs, false, ref fileName);
        }

        public static void ShowDialog(AppDirs appDirs,bool saveOnly, ref string fileName)
        {
            FrmDevTemplate frm = new FrmDevTemplate() { initialFileName = fileName};
            frm.ShowDialog();
            fileName = frm.fileName;
        }

        #region 窗体载入初始化
        private void FrmDevTemplate_Load(object sender, EventArgs e)
        {
            openFileDialog.SetFilter(KpPhrases.TemplateFileFilter);
            saveFileDialog.SetFilter(KpPhrases.TemplateFileFilter);


            //初始化控件
            InitComboboxControl();

            deviceTemplate = new DeviceTemplate();
            if (!string.IsNullOrEmpty(initialFileName))
            {
                LoadTemplate(initialFileName);
            }

            //模板载入完毕后判断模板内是否有数据,没有则内部初始化默认数据
            if (deviceTemplate.TagGroups.Count > 0)
                myTagGroup = deviceTemplate.TagGroups[0];
            else
            {
                myTagGroup = new MyTagGroup(ModbusRegisterType.Coils)
                {
                    Name = "MyTagGroup"
                };
                deviceTemplate.TagGroups.Add(myTagGroup);
            }
                

            modbusCmds = deviceTemplate.ModbusCmds;
           
            //绑定数据到datagridview
            BindDataGridViewDataSource();
        }

        private void InitComboboxControl()
        {
            List<ModbusRegisterTypeKeyValue> modbusTypes = new List<ModbusRegisterTypeKeyValue>();
            foreach (ModbusRegisterType type in Enum.GetValues(typeof(ModbusRegisterType)))
                modbusTypes.Add(new ModbusRegisterTypeKeyValue
                {
                    ModbusRegisterTypeName = type.ToString(),
                    //ModbusRegisterTypeValue = (int)type,
                    ModbusRegisterType = type
                });
            cbxRegisterType.DataSource = modbusTypes;
            cbxRegisterType.DisplayMember = "ModbusRegisterTypeName";
            cbxRegisterType.ValueMember = "ModbusRegisterType";

            var modbusTypesCopy = new List<ModbusRegisterTypeKeyValue>();
            foreach (ModbusRegisterType type in Enum.GetValues(typeof(ModbusRegisterType)))
                modbusTypesCopy.Add(new ModbusRegisterTypeKeyValue
                {
                    ModbusRegisterTypeName = type.ToString(),
                    //ModbusRegisterTypeValue = (int)type,
                    ModbusRegisterType = type
                });

            //ModbusCmd DatagridView绑定数据源
            CmdModbusRegisterType.DataSource = modbusTypesCopy;
            CmdModbusRegisterType.DisplayMember = "ModbusRegisterTypeName";
            CmdModbusRegisterType.ValueMember = "ModbusRegisterType";

            

            //DataType DatagridView绑定数据
            var tagDataTypes = new List<TagDataTypeKeyValue>();
            foreach (TagDataType type in Enum.GetValues(typeof(TagDataType)))
                tagDataTypes.Add(new TagDataTypeKeyValue
                {
                    TagDataType = type,
                    TagDataTypeName = type.ToString()
                });

            TagGroupDataType.DataSource = tagDataTypes;
            TagGroupDataType.DisplayMember = "TagDataTypeName";
            TagGroupDataType.ValueMember = "TagDataType";
        }

        private void BindDataGridViewDataSource()
        {
            dgvTagGroup.AutoGenerateColumns = false;
            bdsTagGroups.DataSource = myTagGroup.Tags;
            dgvTagGroup.DataSource = bdsTagGroups;

            dgvCmd.AutoGenerateColumns = false;
            bdsCmds.DataSource = modbusCmds;
            dgvCmd.DataSource = bdsCmds;
        }

        /// <summary>
        /// 载入已定义的模板文件
        /// </summary>
        /// <param name="fileName"></param>
        private void LoadTemplate(string fileName)
        {
            this.fileName = fileName;
            if (!deviceTemplate.Load(fileName, out string errMsg))
            {
                ScadaUiUtils.ShowError(errMsg);
            }
            
            //读取点
            if(deviceTemplate.TagGroups.Count > 0)
            {
                cbxRegisterType.SelectedValue = deviceTemplate.TagGroups[0].ModbusRegisterType;
                numTagCount.Value = deviceTemplate.TagGroups[0].Tags.Count;
                txtStartAddress.Text = deviceTemplate.TagGroups[0].Address.ToString();
            }
            
            //指令点
            if(deviceTemplate.ModbusCmds.Count > 0)
            {
                cbCmdRegType.SelectedIndex = deviceTemplate.ModbusCmds[0].ModbusRegisterType == ModbusRegisterType.Coils ? 0 : 1;
                txtCmdAddr.Text = deviceTemplate.ModbusCmds[0].Address.ToString();
            }
           
        }
        #endregion

        private byte GetFunctionCode(ModbusRegisterType registerType,bool isRead = true)
        {
            byte code = 1;
            switch (registerType)
            {
                case ModbusRegisterType.Coils:
                    code = 1;
                    if (!isRead)
                        code = 5;
                    break;
                case ModbusRegisterType.InputRegisters:
                    code = 2;
                    break;
                case ModbusRegisterType.HoldingRegisters:
                    code = 3;
                    if (!isRead)
                        code = 6;
                    break;
                case ModbusRegisterType.DiscreteInputs:
                    code = 4;
                    break;
            }

            return code;
        }

        #region 读取点
        private void numTagCount_ValueChanged(object sender, EventArgs e)
        {
            if(myTagGroup != null)
            {
                int oldtagCount = myTagGroup.Tags.Count;
                int newtagCount = (int)numTagCount.Value;

                if(oldtagCount < newtagCount)
                {
                    var tagGroup = myTagGroup;
                    tagGroup.Tags.Clear();

                    var tagCount = (int)numTagCount.Value;
                    ushort address = default;
                    if (ushort.TryParse(txtStartAddress.Text, out ushort result))
                        address = result;
                    tagGroup.Address = address;

                    ModbusRegisterType registerType = (ModbusRegisterType)cbxRegisterType.SelectedValue;
                    var tagDataType = (registerType == ModbusRegisterType.Coils || registerType == ModbusRegisterType.DiscreteInputs) ? TagDataType.Bool : TagDataType.UShort;
                    for (int i = 0; i < tagCount; i++)
                    {
                        tagGroup.Tags.Add(new Tag("tagRead" + i, tagDataType));
                    }
                }
                else if(oldtagCount > newtagCount)
                {
                    var tagGroup = myTagGroup;
                    for (int i = newtagCount; i < oldtagCount; i++)
                        tagGroup.Tags.RemoveAt(i);
                }
            }

            bdsTagGroups.ResetBindings(false);
        }

        private void cbxRegisterType_SelectedIndexChanged(object sender, EventArgs e)
        {
            ModbusRegisterType? registerType = cbxRegisterType.SelectedValue as ModbusRegisterType?;
            
            if (registerType == null)
                return;
            var tagGroup = myTagGroup;
            if(tagGroup != null)
            {
                for (int i = 0; i < tagGroup.Tags.Count; i++)
                {
                    var tag = tagGroup.Tags[i];
                    var dataType = TagDataType.Bool;
                    switch (registerType)
                    {
                        case ModbusRegisterType.Coils:
                        case ModbusRegisterType.DiscreteInputs:
                            dataType = TagDataType.Bool;
                            break;
                        case ModbusRegisterType.InputRegisters:
                        case ModbusRegisterType.HoldingRegisters:
                            dataType = TagDataType.UShort;
                            break;
                    }
                    tag.DataType = dataType;
                }
                tagGroup.ModbusRegisterType = (ModbusRegisterType)registerType;
            }
           
        }

        private void txtStartAddress_TextChanged(object sender, EventArgs e)
        {
            var tagGroup = myTagGroup;
            ushort address;
            ushort.TryParse(txtStartAddress.Text, out address);
        }


        #endregion

        #region 指令点
        private void txtCmdAddr_TextChanged(object sender, EventArgs e)
        {
            //var cmd = ModbusCmds[0];
            //ushort address;
            //ushort.TryParse(txtCmdAddr.Text, out address);
        }

        private void cbCmdRegType_SelectedIndexChanged(object sender, EventArgs e)
        {
            //for (int i = 0; i < ModbusCmds.Count; i++)
            //{
            //    var cmd = ModbusCmds[i];
            //    cmd.ModbusRegisterType = cbCmdRegType.SelectedIndex == 0 ? ModbusRegisterType.Coils : ModbusRegisterType.HoldingRegisters;
            //}
        }

        private void cbCmdElemType_SelectedIndexChanged(object sender, EventArgs e)
        {
            //for(int i = 0;i < ModbusCmds.Count; i++)
            //{
            //    var cmd = ModbusCmds[i];
            //    TagDataType tagDataType = (TagDataType)cbCmdElemType.SelectedIndex;
            //    cmd.TagDataType = tagDataType;
            //}
        }

        private void btnAddModbusCmd_Click(object sender, EventArgs e)
        {
            ushort address;
            ushort.TryParse(txtCmdAddr.Text, out address);
            ModbusRegisterType registerType = cbCmdRegType.SelectedIndex == 0 ? ModbusRegisterType.Coils : ModbusRegisterType.HoldingRegisters;

            TagDataType tagDataType = (TagDataType)cbCmdElemType.SelectedIndex;
            
            modbusCmds.Add(new ModbusCmd(registerType, true)
            {
                Name = "tagWrite" + (modbusCmds.Count + 1),
                Address = address,
                TagDataType =tagDataType,
                CmdNum = modbusCmds.Count + 1
            });

            bdsCmds.ResetBindings(false);
        }

        #endregion


        #region 保存配置
        private void btnSave_Click(object sender, EventArgs e)
        {
            string newFileName = string.Empty;
            SaveChanged(false);
            
        }

        private bool SaveChanged(bool saveAs)
        {
            string newFileName = string.Empty;
            if(saveAs || string.IsNullOrEmpty(fileName))
            {
                if(saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    newFileName = saveFileDialog.FileName;
                }
            }
            else
                newFileName = fileName;

            if (newFileName == string.Empty)
                return false;
            else
            {
                string errMsg = string.Empty;
                if (deviceTemplate == null)
                    deviceTemplate = new DeviceTemplate();

                //deviceTemplate.TagGroups = MyTagGroups;
                //deviceTemplate.ModbusCmds = modbusCmds;
                if (deviceTemplate.Save(newFileName, out errMsg))
                {
                    fileName = newFileName;
                    return true;
                }
                else
                {
                    ScadaUiUtils.ShowError(errMsg);
                    return false;
                }
            }
        }
        #endregion

        private void btnEditSettings_Click(object sender, EventArgs e)
        {
            FrmModbusDeviceOptionConfig frm = new FrmModbusDeviceOptionConfig(deviceTemplate);
            frm.ShowDialog();
        }

        private void btnEditSettingsExt_Click(object sender, EventArgs e)
        {

        }
    }
}
