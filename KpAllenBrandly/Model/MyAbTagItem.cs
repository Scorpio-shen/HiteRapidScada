using HslCommunication.Profinet.AllenBradley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KpAllenBrandly.Model
{
    public class MyAbTagItem
    {
        public MyAbTagItem() { }
        public MyAbTagItem(AbTagItem tagItem,string name) 
        {
            TagItem = tagItem;
            Name = name;
        }
        public AbTagItem TagItem { get; set; }
        public string Name { get; set; }
    }
}
