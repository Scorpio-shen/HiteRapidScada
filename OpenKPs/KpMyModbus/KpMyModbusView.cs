using KpMyModbus.Modbus;
using KpMyModbus.Modbus.Protocol;
using Scada;
using Scada.Comm.Devices;
using Scada.Comm.Devices.Modbus.UI;
using Scada.Data.Configuration;
using Scada.UI;
using System.Collections.Generic;
using System.IO;

namespace Scada.Comm.Devices
{

    public class KpMyModbusView : KPView
    {
        /// <summary>
        /// 驱动版本信息描述
        /// </summary>
        internal const string KpVersion = "1.0.0.0";

        /// <summary>
        /// The UI customization object.
        /// </summary>
        //private static readonly UiCustomization UiCustomization = new UiCustomization();


        /// <summary>
        /// Initializes a new instance of the class. Designed for general configuring.
        /// </summary>
        public KpMyModbusView()
            : this(0)
        {
        }

        /// <summary>
        /// Initializes a new instance of the class. Designed for configuring a particular device.
        /// </summary>
        public KpMyModbusView(int number)
            : base(number)
        {
            CanShowProps = true;
        }


        /// <summary>
        /// Gets the driver description.
        /// </summary>
        public override string KPDescr
        {
            get
            {
                return Localization.UseRussian ?
                    "Взаимодействие с контроллерами по протоколу Modbus.\n\n" +
                    "Пользовательский параметр линии связи:\n" +
                    "TransMode - режим передачи данных (RTU, ASCII, TCP).\n\n" +
                    "Параметр командной строки:\n" +
                    "имя файла шаблона.\n\n" +
                    "Команды ТУ:\n" +
                    "определяются шаблоном (стандартные или бинарные)." :

                    "Interacting with controllers via Modbus protocol.\n\n" +   //
                    "Custom communication line parameter:\n" +
                    "TransMode - data transmission mode (RTU, ASCII, TCP).\n\n" +
                    "Command line parameter:\n" +
                    "template file name.\n\n" +
                    "Commands:\n" +
                    "defined by template (standard or binary).";
            }
        }

        /// <summary>
        /// Gets the driver version.
        /// </summary>
        public override string Version
        {
            get
            {
                return KpVersion;
            }
        }

        /// <summary>
        /// Gets the default channel prototypes.
        /// </summary>
        public override KPCnlPrototypes DefaultCnls
        {
            get
            {
                // загрузка шаблона устройства
                string fileName = KPProps == null ? "" : KPProps.CmdLine.Trim();

                if (fileName == "")
                    return null;

                string filePath = Path.IsPathRooted(fileName) ? fileName : Path.Combine(AppDirs.ConfigDir, fileName);
                DeviceTemplate deviceTemplate = new DeviceTemplate();

                if (!deviceTemplate.Load(filePath, out string errMsg))
                    throw new ScadaException(errMsg);

                // создание прототипов каналов КП
                return CreateCnlPrototypes(deviceTemplate);
            }
        }


        /// <summary>
        /// Creates channel prototypes based on the device template.
        /// </summary>
        protected virtual KPCnlPrototypes CreateCnlPrototypes(DeviceTemplate deviceTemplate)
        {
            KPCnlPrototypes prototypes = new KPCnlPrototypes();
            List<InCnlPrototype> inCnls = prototypes.InCnls;
            List<CtrlCnlPrototype> ctrlCnls = prototypes.CtrlCnls;

            // создание прототипов входных каналов
            int signal = 1;
            foreach (MyTagGroup elemGroup in deviceTemplate.TagGroups)
            {
                bool isTS =
                    elemGroup.ModbusRegisterType == ModbusRegisterType.DiscreteInputs ||
                    elemGroup.ModbusRegisterType == ModbusRegisterType.Coils;

                foreach (Tag elem in elemGroup.Tags)
                {
                    InCnlPrototype inCnl = isTS ?
                        new InCnlPrototype(elem.Name, BaseValues.CnlTypes.TS) :
                        new InCnlPrototype(elem.Name, BaseValues.CnlTypes.TI);
                    inCnl.Signal = signal++;

                    if (isTS)
                    {
                        inCnl.ShowNumber = false;
                        inCnl.UnitName = BaseValues.UnitNames.OffOn;
                        inCnl.EvEnabled = true;
                        inCnl.EvOnChange = true;
                    }

                    inCnls.Add(inCnl);
                }
            }

            // создание прототипов каналов управления
            foreach (ModbusCmd modbusCmd in deviceTemplate.ModbusCmds)
            {
                CtrlCnlPrototype ctrlCnl = modbusCmd.ModbusRegisterType == ModbusRegisterType.Coils && modbusCmd.Multiple ?
                    new CtrlCnlPrototype(modbusCmd.Name, BaseValues.CmdTypes.Binary) :
                    new CtrlCnlPrototype(modbusCmd.Name, BaseValues.CmdTypes.Standard);
                ctrlCnl.CmdNum = modbusCmd.CmdNum;

                if (modbusCmd.ModbusRegisterType == ModbusRegisterType.Coils && !modbusCmd.Multiple)
                    ctrlCnl.CmdValName = BaseValues.CmdValNames.OffOn;

                ctrlCnls.Add(ctrlCnl);
            }

            return prototypes;
        }

        /// <summary>
        /// Localizes the driver UI.
        /// </summary>
        protected virtual void Localize()
        {
            if (!Localization.LoadDictionaries(AppDirs.LangDir, "KpModbus", out string errMsg))
                ScadaUiUtils.ShowError(errMsg);

            KpPhrases.Init();
        }

        /// <summary>
        /// Gets a UI customization object.
        /// </summary>
        //protected virtual UiCustomization GetUiCustomization()
        //{
        //    return UiCustomization;
        //}


        /// <summary>
        /// Shows the driver properties.
        /// </summary>
        public override void ShowProps()
        {
            Localize();

            if (Number > 0)
            {
                // show properties of the particular device
                FrmDevProps.ShowDialog(Number, KPProps, AppDirs);
            }
            else
            {
                // show the device template editor
                FrmDevTemplate.ShowDialog(AppDirs);
            }
        }
    }
}
