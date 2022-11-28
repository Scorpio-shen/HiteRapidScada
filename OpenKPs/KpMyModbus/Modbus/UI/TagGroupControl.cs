using KpMyModbus.Modbus.Protocol;
using KpMyModbus.Modbus.ViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KpMyModbus.Modbus.UI
{
    public partial class TagGroupControl : UserControl
    {
        private TagGroupControlViewModel ViewModel;

        private MyTagGroup tagGroup;
        public MyTagGroup TagGroup
        {
            get => tagGroup;
            set
            {
                tagGroup = value;
                ShowTagGroupProps(tagGroup);
            }
        }
        public TagGroupControl()
        {
            InitializeComponent();
            ViewModel = new TagGroupControlViewModel();
            txtGrName.DataBindings.Add(nameof(txtGrName.Text), ViewModel, nameof(ViewModel.TagGroupName));
            txtGrFuncCode.DataBindings.Add(nameof(txtGrFuncCode.Text),ViewModel,nameof(ViewModel.FunctionCode));
            numGrAddress.DataBindings.Add(nameof(numGrAddress.Value),ViewModel,nameof(ViewModel.TagStartAddess));
            numGrTagCnt.DataBindings.Add(nameof(numGrTagCnt.Value), ViewModel, nameof(ViewModel.TagCount));
        }


        private void ShowTagGroupProps(MyTagGroup tagGroup)
        {
            if(tagGroup == null)
            {
                ViewModel.TagGroupName = String.Empty;
                ViewModel.TagStartAddess = 1;
                ViewModel.TagCount = 1;
                cbGrRegisterType.SelectedIndex = 0;
                gbTagGroup.Enabled = false; 
            }
            else
            {
                ViewModel.TagGroupName = tagGroup.Name;
                ViewModel.TagStartAddess = tagGroup.Address;
                ViewModel.TagCount = tagGroup.Tags.Count;
                cbGrRegisterType.SelectedIndex = (int)tagGroup.ModbusRegisterType;
                gbTagGroup.Enabled = true;
            }
        }
    }
}
