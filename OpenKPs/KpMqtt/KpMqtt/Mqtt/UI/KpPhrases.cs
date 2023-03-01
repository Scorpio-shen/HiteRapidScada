using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scada.Comm.Devices.Mqtt.UI
{
    public static class KpPhrases
    {
        // Scada.Comm.Devices.Mqtt.UI.FrmDevTemplate
        public static string TemplFormTitle { get; private set; }
        public static string Publish { get; private set; }
        public static string Subscription { get; private set; }
        public static string DefGrName { get; private set; }
        public static string DefElemName { get; private set; }
        public static string DefCmdName { get; private set; }
        public static string AddressHint { get; private set; }
        public static string SaveTemplateConfirm { get; private set; }
        public static string ElemCntExceeded { get; private set; }
        public static string ElemRemoveWarning { get; private set; }
        public static string TemplateFileFilter { get; private set; }

        // Scada.Comm.Devices.Mqtt.UI.FrmDevProps
        public static string TemplNotExists { get; private set; }

        public static void Init()
        {
            Localization.Dict dict = Localization.GetDictionary("Scada.Comm.Devices.Mqtt.UI.FrmMqttDevTemplate");
            TemplFormTitle = dict.GetPhrase("this");
            Publish = dict.GetPhrase("Publish");
            Subscription = dict.GetPhrase("Subscription");
            DefGrName = dict.GetPhrase("DefGrName");
            DefElemName = dict.GetPhrase("DefElemName");
            DefCmdName = dict.GetPhrase("DefCmdName");
            AddressHint = dict.GetPhrase("AddressHint");
            SaveTemplateConfirm = dict.GetPhrase("SaveTemplateConfirm");
            ElemCntExceeded = dict.GetPhrase("ElemCntExceeded");
            ElemRemoveWarning = dict.GetPhrase("ElemRemoveWarning");
            TemplateFileFilter = dict.GetPhrase("TemplateFileFilter");

            dict = Localization.GetDictionary("Scada.Comm.Devices.Mqtt.UI.FrmDevProps");
            TemplNotExists = dict.GetPhrase("TemplNotExists");
        }
    }
}
