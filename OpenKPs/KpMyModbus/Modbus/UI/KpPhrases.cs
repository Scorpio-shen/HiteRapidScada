#pragma warning disable 1591 // Missing XML comment for publicly visible type or member

namespace Scada.Comm.Devices.Modbus.UI
{
    /// <summary>
    /// The phrases used by the library.
    /// <para>Фразы, используемые библиотекой.</para>
    /// </summary>
    public static class KpPhrases
    {
        // Scada.Comm.Devices.Modbus.UI.FrmDevTemplate
        public static string TemplFormTitle { get; private set; }
        public static string GrsNode { get; private set; }
        public static string CmdsNode { get; private set; }
        public static string DefGrName { get; private set; }
        public static string DefElemName { get; private set; }
        public static string DefCmdName { get; private set; }
        public static string AddressHint { get; private set; }
        public static string SaveTemplateConfirm { get; private set; }
        public static string ElemCntExceeded { get; private set; }
        public static string ElemRemoveWarning { get; private set; }
        public static string TemplateFileFilter { get; private set; }

        // Scada.Comm.Devices.Modbus.UI.FrmDevProps
        public static string TemplNotExists { get; private set; }

        public static void Init()
        {
            Localization.Dict dict = Localization.GetDictionary("Scada.Comm.Devices.Modbus.UI.FrmDevTemplate");
            TemplFormTitle = dict.GetPhrase("this");
            GrsNode = dict.GetPhrase("GrsNode");
            CmdsNode = dict.GetPhrase("CmdsNode");
            DefGrName = dict.GetPhrase("DefGrName");
            DefElemName = dict.GetPhrase("DefElemName");
            DefCmdName = dict.GetPhrase("DefCmdName");
            AddressHint = dict.GetPhrase("AddressHint");
            SaveTemplateConfirm = dict.GetPhrase("SaveTemplateConfirm");
            ElemCntExceeded = dict.GetPhrase("ElemCntExceeded");
            ElemRemoveWarning = dict.GetPhrase("ElemRemoveWarning");
            TemplateFileFilter = dict.GetPhrase("TemplateFileFilter");

            dict = Localization.GetDictionary("Scada.Comm.Devices.Modbus.UI.FrmDevProps");
            TemplNotExists = dict.GetPhrase("TemplNotExists");
        }
    }
}
