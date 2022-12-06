using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KpCommon.Attributes
{
    public class DataTypeByteCountAttribute : Attribute
    {
        public int ByteCount { get; set; }
        public DataTypeByteCountAttribute(int byteCount)
        {
            ByteCount = byteCount;
        }
    }
}
