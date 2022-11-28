using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KpSiemens.Siemens.Model
{
    public static class TagIDHelper
    {
        private static int TagGroupTagID = 1;
        private static int TagCmdGroupID = 1;
        private static int CmdTagID=1;
        public static int NewTagGroupTagID()
        {
            return TagGroupTagID++;
        }

        public static int NewTagCmdGroupID()
        {
            return TagCmdGroupID++;
        }

        public static int NewCmdTagID()
        {
            return CmdTagID++;
        }

        public static void SetTagGroupTagID(int id)
        {
            TagGroupTagID = id;
        }

        public static void SetTagCmdGroupID(int id)
        {
            TagCmdGroupID = id; 
        }

        public static void SetCmdTagID()
        {

        }
    }
}
