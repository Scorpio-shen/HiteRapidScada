using System.Windows.Forms;

namespace KpCommon.Extend
{
    public static class ControlExtend
    {
        /// <summary>
        /// 控件数据绑定扩展方法(简写绑定，原来绑定方法写的太啰嗦)
        /// </summary>
        /// <param name="control">需要绑定的控件</param>
        /// <param name="dataSource">绑定数据源</param>
        /// <param name="dataMember">绑定数据源的目标属性</param>
        /// <param name="propertyName">自身控件要绑定的属性名,如果是默认的就不用填，默认TextBox(Text)，Combobox(SelectValue),CheckBox(Check),</param>
        /// <param name="needClear"></param>
        public static void AddDataBindings(this Control control,object dataSource,string dataMember,string propertyName = "",bool needClear = true)
        {
            if(needClear)
                control.DataBindings.Clear();
            if(string.IsNullOrEmpty(propertyName))
            {
                if (control is ComboBox)
                    propertyName = "SelectedValue";
                else if (control is TextBox)
                    propertyName = "Text";
                else if (control is CheckBox)
                    propertyName = "Checked";
                else if (control is NumericUpDown)
                    propertyName = "Value";
                else if (control is Label)
                    propertyName = "Text";
            }

            control.DataBindings.Add(propertyName,dataSource,dataMember,false,DataSourceUpdateMode.OnPropertyChanged);
        }
        /// <summary>
        /// RadioButton绑定
        /// </summary>
        /// <param name="radio"></param>
        /// <param name="dataSource">数据源</param>
        /// <param name="dataMember">要绑定的属性</param>
        /// <param name="needNegation">是否需要取反(绑定的属性bool值与控件Check)</param>
        /// <param name="propertyName">绑定控件的属性名</param>
        /// <param name="needClear"></param>
        public static void AddDataBindings(this RadioButton radio, object dataSource, string dataMember,bool needNegation = false, string propertyName = "", bool needClear = true)
        {
            if (needClear)
                radio.DataBindings.Clear();
            if (string.IsNullOrEmpty(propertyName))
            {
                propertyName = nameof(radio.Checked);
            }

            var binding = new Binding(propertyName, dataSource, dataMember, false, DataSourceUpdateMode.OnPropertyChanged);
            binding.Format += (sender, e) =>
            {
                //需要取反
                if (needNegation)
                {
                    e.Value = !(bool)e.Value;
                }
            };
            radio.DataBindings.Add(binding);
        }

        public static void AddVisableDataBindings(this Control control,object dataSource,string dataMember, bool needNegation = false, string propertyName = "", bool needClear = true)
        {
            if(needClear)
                control.DataBindings.Clear();
            if (string.IsNullOrEmpty(propertyName))
            {
                propertyName = nameof(control.Visible);
            }

            var binding = new Binding(propertyName, dataSource, dataMember, false, DataSourceUpdateMode.OnPropertyChanged);
            binding.Format += (sender, e) =>
            {
                //需要取反
                if (needNegation)
                {
                    e.Value = !(bool)e.Value;
                }
            };

            control.DataBindings.Add(binding);
        }
    }
}
