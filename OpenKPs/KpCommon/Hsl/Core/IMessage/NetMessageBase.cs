using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KpCommon.Hsl.Core.IMessage
{
    public class NetMessageBase
    {
        public byte[] HeadBytes { get; set; }

        public byte[] ContentBytes { get; set; }

        public byte[] SendBytes { get; set; }

        public virtual int PependedUselesByteLength(byte[] headByte)
        {
            return 0;
        }

        public virtual int GetHeadBytesIdentity()
        {
            return 0;
        }

        public virtual bool CheckHeadBytesLegal(byte[] token)
        {
            if (HeadBytes == null)
            {
                return false;
            }

            return true;
        }
    }
}
