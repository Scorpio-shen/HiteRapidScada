using HslCommunication.Profinet.AllenBradley;
using KpHiteBeckHoff.Model.EnumType;
using KpHiteBeckHoff.ViewModel;
using KpCommon.Extend;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KpHiteBeckHoff.View
{
    public partial class FrmPLCImport : Form
    {
        private FrmPLCImportViewModel _viewModel;
        public FrmPLCImport(FrmPLCImportViewModel viewModel)
        {
            InitializeComponent();
            _viewModel= viewModel;
            //绑定控件
            txtIPAddress.AddDataBindings(_viewModel, nameof(_viewModel.IpAddress));
            txtPort.AddDataBindings(_viewModel, nameof(_viewModel.Port));
            cbxProtocolType.AddDataBindings(_viewModel,nameof(_viewModel.ProtocolType));
            
            var btnBinding = new Binding(nameof(btnConnect.Enabled), _viewModel, nameof(_viewModel.IsConnected), true, DataSourceUpdateMode.OnPropertyChanged);
            btnBinding.Format += (sender, e) =>
            {
                e.Value = !((bool)e.Value);
            };
            btnConnect.DataBindings.Add(btnBinding);
            btnDisConnect.DataBindings.Add(nameof(btnConnect.Enabled), _viewModel, nameof(_viewModel.IsConnected), true, DataSourceUpdateMode.OnPropertyChanged);
        }

        private void FrmPLCImport_Load(object sender, EventArgs e)
        {
            //Combobox绑定数据源
            var keyValueConnectionEnums = new Dictionary<string, ProtocolTypeEnum>();
            foreach (ProtocolTypeEnum type in Enum.GetValues(typeof(ProtocolTypeEnum)))
                keyValueConnectionEnums.Add(type.GetDescription(), type);

            BindingSource bindingSource = new BindingSource();
            bindingSource.DataSource = keyValueConnectionEnums;
            cbxProtocolType.DataSource = bindingSource;
            cbxProtocolType.DisplayMember = "Key";
            cbxProtocolType.ValueMember = "Value";
        }

        private void FrmPLCImport_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_viewModel.AllenBradleyNet != null)
            {
                _viewModel.AllenBradleyNet.ConnectClose();
                _viewModel.AllenBradleyNet.Dispose();
                _viewModel.AllenBradleyNet = null;
            }
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            if(!_viewModel.IsConnected)
            {
                MessageBox.Show("未连接到PLC!");
                return; 
            }

            //读取
            ctrlTreeView.InitPLCConnect(_viewModel.AllenBradleyNet);
            ctrlTreeView.TagRefresh();
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            if(_viewModel.AllenBradleyNet == null)
            {
                _viewModel.IsConnected = false;
                _viewModel.AllenBradleyNet = new HslCommunication.Profinet.AllenBradley.AllenBradleyNet(_viewModel.IpAddress, _viewModel.Port);
            }
                
            if(_viewModel.IsConnected)
                _viewModel.AllenBradleyNet.ConnectClose();

            //重新连接
            _viewModel.AllenBradleyNet.IpAddress = _viewModel.IpAddress;
            _viewModel.AllenBradleyNet.Port = _viewModel.Port;
            var connectResult = _viewModel.AllenBradleyNet.ConnectServer();
            _viewModel.IsConnected = connectResult.IsSuccess;
            if (!connectResult.IsSuccess)
            {
                MessageBox.Show(connectResult.Message);
            }
            ctrlTreeView.InitPLCConnect(_viewModel.AllenBradleyNet);
            ctrlTreeView.TagRefresh();
        }

        private void btnDisConnect_Click(object sender, EventArgs e)
        {
            if (_viewModel.AllenBradleyNet == null)
            {
                _viewModel.IsConnected = false;
                _viewModel.AllenBradleyNet = new HslCommunication.Profinet.AllenBradley.AllenBradleyNet(_viewModel.IpAddress, _viewModel.Port);
                return;
            }

            if (_viewModel.IsConnected)
            {
                _viewModel.AllenBradleyNet.ConnectClose();
                _viewModel.IsConnected = false;
            }
        }

        //private void btnConfirm_Click(object sender, EventArgs e)
        //{
        //    //将选中AbTagItem转内部TagItem
        //    var abTagItems = ctrlTreeView.GetAllTagItems();
        //    _viewModel.Tags = new List<Model.Tag>();
        //    var tags = _viewModel.Tags;
        //    foreach(var abTag in abTagItems)
        //    {
        //        DataTypeEnum dataType = DataTypeEnum.UShort;
        //        try
        //        {
        //            dataType = ConvertToDataType(abTag.TagItem.SymbolType);
        //        }
        //        catch(Exception ex)
        //        {
        //            Console.WriteLine($"Name:{abTag.Name},SymbolType:{abTag.TagItem.SymbolType}不支持转内部数据类型");
        //            continue;
        //        }
        //        //判断是否是数组
        //        var tag = new Model.Tag
        //        {
        //            Name = abTag.Name,
        //            DataType = dataType,
        //            CanWrite = 1
        //        };
        //        //判断是否是String类型
        //        if (dataType == DataTypeEnum.String)
        //        {
        //            tag.IsArray = false;
        //            tag.ParentTag = null;
        //            if (abTag.TagItem.Members?.Count() >= 2)
        //            {
        //                var dataMember = abTag.TagItem.Members.FirstOrDefault(m => m.SymbolType == 0xC2);//存储字符串的SByte数组
        //                if(dataMember != null && dataMember.ArrayLength.Length > 0)
        //                {
        //                    tag.Length= dataMember.ArrayLength[0];
        //                    tags.Add(tag);
        //                }
        //            }                   
        //            continue;
        //        }

        //        //判断是否是struct
        //        if (abTag.TagItem.IsStruct)
        //            continue;
        //        //判断是否是数组
        //        if (abTag.TagItem.ArrayDimension == 0)
        //        {
        //            tag.Length = 0;
        //            tag.IsArray = false;
        //            tag.Index= 0;
        //            tag.ParentTag = null;
        //            tags.Add(tag);
        //        }
        //        else if (abTag.TagItem.ArrayDimension == 1)
        //        {
                    
        //            //数组类型
        //            var arrayLength = abTag.TagItem.ArrayLength[0];//要生成数组的长度
        //            if(abTag.TagItem.SymbolType == AllenBradleyHelper.CIP_Type_D3)
        //            {
        //                //Bit string, 32 bits, DWORD 由32 Bool组成数组
        //                arrayLength = 32;
        //            }
        //            //生成ParentTag
        //            tag.Length = arrayLength;
        //            tag.IsArray = false;
        //            tag.Index= 0;
        //            tag.ParentTag = null;

        //            _viewModel.ParentTags.Add(tag);
        //            //子Tag集合
        //            for (int i = 0;i < arrayLength; i++)
        //            {
        //                var arrayTag = new Model.Tag
        //                {
        //                    Name = abTag.Name + $"[{i}]",
        //                    DataType = dataType,
        //                    CanWrite = 1,
        //                    Length = default,
        //                    IsArray = true,
        //                    Index = i,
        //                    ParentTag = tag
        //                };
        //                tags.Add(arrayTag);
        //            }
        //        }
        //        else
        //            continue;
        //    }
        //    DialogResult = DialogResult.OK;
        //}

        private void btnCancle_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

       

        private DataTypeEnum ConvertToDataType(ushort symbolType)
        {
            switch(symbolType)
            {
                case 0xC1:
                case 0xD3:
                    return DataTypeEnum.Bool;
                case 0xC2:
                    return DataTypeEnum.SByte;
                case 0xC3:
                    return DataTypeEnum.Short;
                case 0xC4:
                    return DataTypeEnum.Int;
                case 0xC5:
                    return DataTypeEnum.Long;
                case 0xC6:
                    return DataTypeEnum.Byte;
                case 0xC7:
                    return DataTypeEnum.UShort;
                case 0xC8:
                    return DataTypeEnum.UInt;
                case 0xC9:
                    return DataTypeEnum.ULong;
                case 0xCA:
                    return DataTypeEnum.Float;
                case 0xCB:
                    return DataTypeEnum.Double;
                case 0x0211:
                    return DataTypeEnum.String; //string由一个int表示有效长度,
                default:
                    throw new Exception("未兼容的数据类型!");

            }
        }
    }
}
