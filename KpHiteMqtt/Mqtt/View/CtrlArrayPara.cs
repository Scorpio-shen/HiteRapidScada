using KpCommon.Extend;
using KpCommon.Helper;
using KpCommon.Model;
using KpHiteMqtt.Mqtt.Model;
using KpHiteMqtt.Mqtt.Model.Enum;
using Newtonsoft.Json;
using Scada.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace KpHiteMqtt.Mqtt.View
{
    public partial class CtrlArrayPara : UserControl
    {
        private Property _property;
        private DataArraySpecs _arraySpecs;
        

        public CtrlArrayPara()
        {
            InitializeComponent();
            //combobox绑定数据源
            Dictionary<string, ArrayDataTypeEnum> keyValueDataTypeEnums = new Dictionary<string, ArrayDataTypeEnum>();
            foreach (ArrayDataTypeEnum type in Enum.GetValues(typeof(ArrayDataTypeEnum)))
                keyValueDataTypeEnums.Add(type.ToString(), type);
            BindingSource bdsDataType = new BindingSource();
            bdsDataType.DataSource = keyValueDataTypeEnums;
            cbxDataType.DataSource = bdsDataType;
            cbxDataType.DisplayMember = "Key";
            cbxDataType.ValueMember = "Value";
        }
        public void InitCtrlArrayPara(Property property)
        {
            _property = property;
            _arraySpecs = _property.DataArraySpecs;
            _arraySpecs.Identifier = property.Identifier;
            ctrlJsonPara.InitPara(_arraySpecs.DataSpecs, false);

            //控件绑定
            txtArrayLength.AddDataBindings(_arraySpecs, nameof(_arraySpecs.ArrayLength));
            cbxDataType.AddDataBindings(_arraySpecs, nameof(_arraySpecs.DataType));
            txtArrayChannel.AddDataBindings(_arraySpecs, nameof(_arraySpecs.ArrayChannelString));
            ctrlJsonPara.AddVisableDataBindings(_arraySpecs, nameof(_arraySpecs.IsStruct));
            cbxDataType.AddDataBindings(_arraySpecs, nameof(_arraySpecs.DataType));
            _property.PropertyChanged += _property_PropertyChanged;

            ctrlJsonPara.DataSpecsChanged += CtrlJsonPara_DataSpecsChanged;

            //btnImport.DataBindings.Clear();
            //btnExport.DataBindings.Clear();
            //var bindImport = new Binding(nameof(btnImport.Enabled), _arraySpecs, nameof(_arraySpecs.IsStruct), false, DataSourceUpdateMode.OnPropertyChanged);
            //bindImport.Format += (sender, e) =>
            //{
            //    var tempValue = (bool)e.Value;
            //    e.Value = !tempValue;
            //};
            //var bindExport = new Binding(nameof(btnExport.Enabled), _arraySpecs, nameof(_arraySpecs.IsStruct), false, DataSourceUpdateMode.OnPropertyChanged);
            //bindExport.Format += (sender, e) =>
            //{
            //    var tempValue = (bool)e.Value;
            //    e.Value = !tempValue;
            //};

            //var bindTextBox = new Binding(nameof(btnExport.Enabled), _arraySpecs, nameof(_arraySpecs.IsStruct), false, DataSourceUpdateMode.OnPropertyChanged);
            //bindTextBox.Format += (sender, e) =>
            //{
            //    var tempValue = (bool)e.Value;
            //    e.Value = !tempValue;
            //};
            //btnImport.DataBindings.Add(nameof(btnImport.Enabled), _arraySpecs, nameof(_arraySpecs.IsStruct), false, DataSourceUpdateMode.OnPropertyChanged);
            //btnExport.DataBindings.Add(nameof(btnExport.Enabled), _arraySpecs, nameof(_arraySpecs.IsStruct), false, DataSourceUpdateMode.OnPropertyChanged);
            //txtArrayChannel.DataBindings.Add(nameof(txtArrayChannel.Enabled),_arraySpecs,nameof(_arraySpecs.IsStruct),false,DataSourceUpdateMode.OnPropertyChanged);

            //btnImport.DataBindings.Add(bindImport);
            //btnExport.DataBindings.Add(bindExport);
            //txtArrayChannel.DataBindings.Add(bindTextBox);
        }

        private void CtrlJsonPara_DataSpecsChanged(List<DataSpecs> dataSpecs)
        {
            _arraySpecs.DataSpecs = dataSpecs;
            _arraySpecs.SetArrayDataSpecs();
        }

        private void _property_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals(nameof(_property.Identifier)))
            {
                _arraySpecs.Identifier = _property.Identifier;
            }
        }

        private void CtrlArrayPara_Load(object sender, EventArgs e)
        {
            
        }

        #region 通道匹配关系导入、导出
        private void btnImport_Click(object sender, EventArgs e)
        {
            var openFile = new OpenFileDialog();
            openFile.Filter = TempleteKeyString.OpenJsonFilterStr;
            var reuslt = openFile.ShowDialog();
            if (reuslt != DialogResult.OK)
                return;
            var filePath = openFile.FileName;
            //ImportExcel(filePath);
            using( var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                using(var sr = new StreamReader(fs))
                {
                    var jsonStr = sr.ReadToEnd();

                    ImportJsonStr(jsonStr);

                }
            }
        }

        private void ImportJsonStr(string jsonStr)
        {
            try
            {
                var maps = JsonConvert.DeserializeObject<List<ArrayChannelMap>>(jsonStr);
                //判断是否与数组长度匹配
                if(maps.Count != _arraySpecs.ArrayLength)
                {
                    ScadaUiUtils.ShowError($"导入数据长度与输入数组长度不匹配!");
                    return;
                }
                //_arraySpecs.InCnlNums.Clear();
                //_arraySpecs.CtrlCnlNums.Clear();
                //根据数据类型进行导入
                if(_arraySpecs.DataType == ArrayDataTypeEnum.Struct)
                {
                    foreach(var map in maps)
                    {
                        if (map.Index >= _arraySpecs.ArraySpecs.Count)
                            continue;
                        var aySpecs = _arraySpecs.ArraySpecs[map.Index];
                        foreach(var parameter in map.Parameters)
                        {

                            var dataspecs = aySpecs.DataSpecs.FirstOrDefault(d => d.Identifier.Equals(parameter.Key));
                            if(dataspecs != null)
                            {
                                dataspecs.InCnlNum = parameter.Value.InputChannel;
                                dataspecs.CtrlCnlNum = parameter.Value.OutputChannel;
                            }
                        }
                    }
                }
                else
                {
                    foreach(var map in maps)
                    {
                        if (map.Index >= _arraySpecs.ArraySpecs.Count)
                            continue;
                        var aySpecs = _arraySpecs.ArraySpecs[map.Index];
                        aySpecs.InCnlNum = map.InputChannel;
                        aySpecs.CtrlCnlNum = map.OutputChannel;
                    }
                }
                txtArrayChannel.Text = _arraySpecs.ArrayChannelString;
            }
            catch (Exception ex)
            {
                ScadaUiUtils.ShowError($"导入异常,{ex.Message}!");
            }
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFile = new SaveFileDialog();
            saveFile.SetFilter(TempleteKeyString.OpenJsonFilterStr);
            var reuslt = saveFile.ShowDialog();
            if (reuslt != DialogResult.OK)
                return;
            var filePath = saveFile.FileName;
            var extensionName = Path.GetExtension(filePath);

            //导出Json文本文件
            //var jsonStr = _arraySpecs.GetArrayChannelString(_property.Identifier);
            using(var fs = new FileStream(filePath, FileMode.Create, FileAccess.ReadWrite))
            {
                using(var write = new StreamWriter(fs))
                {
                    write.Write(_arraySpecs.ArrayChannelString);
                }
            }
        }

        //private void ExportExcel(string filePath,string excelType)
        //{
        //    try
        //    {
        //        var arrayChnnelModels = new List<ArrayChannelModel>();
        //        for (int i = 0; i < _arraySpecs.ArrayLength; i++)
        //        {
        //            arrayChnnelModels.Add(new ArrayChannelModel()
        //            {
        //                ArrayIndex = i,
        //                InCnlNum = i < _arraySpecs.InCnlNums.Count ? _arraySpecs.InCnlNums[i].ToString() : string.Empty,
        //                CtrlCnlNum = i < _arraySpecs.CtrlCnlNums.Count ? _arraySpecs.CtrlCnlNums[i].ToString() : string.Empty,
        //            });
        //        }

        //        using (var ms = ExcelHelper.ToExcel(arrayChnnelModels, excelType))
        //        {
        //            using (var fileStream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite))
        //            {
        //                using (var binaryWrite = new BinaryWriter(fileStream))
        //                {
        //                    binaryWrite.Write(ms.ToArray());
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        ScadaUiUtils.ShowError($"导出Excel异常,{ex.Message}");
        //    }
        //}
        #endregion

    }
}
