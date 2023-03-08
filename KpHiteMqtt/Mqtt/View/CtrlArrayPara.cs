using KpCommon.Extend;
using KpCommon.Helper;
using KpCommon.Model;
using KpHiteMqtt.Mqtt.Model;
using KpHiteMqtt.Mqtt.Model.Enum;
using Scada;
using Scada.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KpHiteMqtt.Mqtt.View
{
    public partial class CtrlArrayPara : UserControl
    {
        private DataArraySpecs _arraySpecs;
        public CtrlArrayPara(DataArraySpecs arraySpecs)
        {
            InitializeComponent();
            _arraySpecs = arraySpecs;
        }

        private void CtrlArrayPara_Load(object sender, EventArgs e)
        {
            //combobox绑定数据源
            Dictionary<string, ArrayDataTypeEnum> keyValueDataTypeEnums = new Dictionary<string, ArrayDataTypeEnum>();
            foreach (ArrayDataTypeEnum type in Enum.GetValues(typeof(ArrayDataTypeEnum)))
                keyValueDataTypeEnums.Add(type.ToString(), type);
            BindingSource bdsDataType = new BindingSource();
            bdsDataType.DataSource = keyValueDataTypeEnums;
            cbxDataType.DataSource = bdsDataType;
            cbxDataType.DisplayMember = "Key";
            cbxDataType.ValueMember = "Value";
            //控件绑定
            txtArrayLength.AddDataBindings(_arraySpecs, nameof(_arraySpecs.ArrayLength));
            cbxDataType.AddDataBindings(_arraySpecs,nameof(_arraySpecs.DataType));
            txtArrayChannel.AddDataBindings(_arraySpecs, nameof(_arraySpecs.ArrayChannelString));
            ctrlJsonPara.AddVisableDataBindings(_arraySpecs, nameof(_arraySpecs.IsStruct));

            ctrlJsonPara.DataSpecs = _arraySpecs.DataSpecs;
        }

        #region 通道匹配关系导入、导出
        private void btnImport_Click(object sender, EventArgs e)
        {
            var openFile = new OpenFileDialog();
            openFile.Filter = TempleteKeyString.OpenExcelFilterStr;
            var reuslt = openFile.ShowDialog();
            if (reuslt != DialogResult.OK)
                return;
            var filePath = openFile.FileName;
            ImportExcel(filePath);
        }

        private void ImportExcel(string filePath)
        {
            try
            {
                var arrayChannels = ExcelHelper.GetListExtend<ArrayChannelModel>(filePath, 0);
                //判断是否与数组长度匹配
                if(arrayChannels.Count != _arraySpecs.ArrayLength)
                {
                    ScadaUiUtils.ShowError($"导入数据长度与输入数组长度不匹配!");
                    return;
                }
                //赋值输入，输出通道
                foreach( var arrayChannel in arrayChannels )
                {
                    _arraySpecs.InCnlNums.Add(arrayChannel.InCnlNum);
                    _arraySpecs.CtrlCnlNums.Add(arrayChannel.CtrlCnlNum);
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
            saveFile.SetFilter(TempleteKeyString.OpenExcelFilterStr);
            var reuslt = saveFile.ShowDialog();
            if (reuslt != DialogResult.OK)
                return;
            var filePath = saveFile.FileName;
            var extensionName = Path.GetExtension(filePath);

            ExportExcel(filePath, extensionName);


        }

        private void ExportExcel(string filePath,string excelType)
        {
            try
            {
                var arrayChnnelModels = new List<ArrayChannelModel>();
                for (int i = 0; i < _arraySpecs.ArrayLength; i++)
                {
                    arrayChnnelModels.Add(new ArrayChannelModel()
                    {
                        ArrayIndex = i,
                        InCnlNum = _arraySpecs.InCnlNums[i],
                        CtrlCnlNum = _arraySpecs.CtrlCnlNums[i],
                    });
                }

                using (var ms = ExcelHelper.ToExcel(arrayChnnelModels, excelType))
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
            catch (Exception ex)
            {
                ScadaUiUtils.ShowError($"导出Excel异常,{ex.Message}");
            }
        }
        #endregion

    }
}
