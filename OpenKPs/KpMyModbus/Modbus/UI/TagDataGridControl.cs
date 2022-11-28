using KpMyModbus.Modbus.Protocol;
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
    public partial class TagDataGridControl : UserControl
    {
        public List<Tag> Tags;
        public TagDataGridControl()
        {
            InitializeComponent();
            Tags = new List<Tag>();
            Load += TagDataGridControl_Load;
        }

        private void TagDataGridControl_Load(object sender, EventArgs e)
        {
            bdsTags.DataSource = Tags;
            dgvTagItems.DataSource = bdsTags;
        }
    }
}
