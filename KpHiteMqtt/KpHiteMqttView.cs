using Scada;
using Scada.Comm.Devices;
using Scada.Data.Configuration;
using Scada.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KpHiteMqtt
{

    public class KpHiteMqttView : KPView
    {
        internal const string KpVersion = "5.0.4.1";


        public KpHiteMqttView() :
            this(0)
        {
        }

        public KpHiteMqttView(int number)
            : base(number)
        {
            CanShowProps = number > 0;
        }


        public override string KPDescr
        {
            get
            {
                return Localization.UseRussian ?
                    "使用 MQTT 协议订阅和发布数据." :
                    "Subscribes and publishes data using the MQTT protocol.";
            }
        }

        public override string Version
        {
            get
            {
                return KpVersion;
            }
        }

        public override KPReqParams DefaultReqParams
        {
            get
            {
                return new KPReqParams(10000, 500);
            }
        }

        public override KPCnlPrototypes DefaultCnls
        {
            get
            {
                // load configuration
                DeviceConfig deviceConfig = new DeviceConfig();
                string configFileName = DeviceConfig.GetFileName(AppDirs.ConfigDir, Number, KPProps.CmdLine);

                if (!File.Exists(configFileName))
                    return null;
                else if (!deviceConfig.Load(configFileName, out string errMsg))
                    throw new ScadaException(errMsg);

                // create channel prototypes
                KPCnlPrototypes prototypes = new KPCnlPrototypes();
                int signal = 1;

                void AddCnl(string name)
                {
                    prototypes.InCnls.Add(new InCnlPrototype(name, BaseValues.CnlTypes.TI)
                    {
                        Signal = signal++
                    });
                }

                deviceConfig.SubTopics.ForEach(t => AddCnl(t.TopicName));
                deviceConfig.SubJSs.ForEach(t =>
                {
                    for (int i = 0; i < t.CnlCnt; i++)
                    {
                        AddCnl(t.TopicName + " [" + i + "]");
                    }
                });

                return prototypes;
            }
        }
        /// <summary>
        /// Localizes the driver UI.
        /// </summary>
        protected virtual void Localize()
        {
            if (!Localization.LoadDictionaries(AppDirs.LangDir, "KpMqtt", out string errMsg))
                ScadaUiUtils.ShowError(errMsg);
            KpPhrases.Init();
        }

        public override void ShowProps()
        {
            // create configuration file if it doesn't exist
            // string configFileName = DeviceConfig.GetFileName(AppDirs.ConfigDir, Number, KPProps.CmdLine);
            /* string configFileName = DeviceConfig.GetFileName(AppDirs.ConfigDir, Number, "");


             if (!File.Exists(configFileName))
              {
                  string resourceName = "Scada.Comm.Devices.Config.KpMqtt_001.xml";
                  string fileContents;

                  using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
                  {
                      using (StreamReader reader = new StreamReader(stream))
                      {
                          fileContents = reader.ReadToEnd();
                      }
                  }

                  File.WriteAllText(configFileName, fileContents, Encoding.UTF8);
              }

              // open configuration directory
             // Process.Start(AppDirs.ConfigDir);*/

            Localize();

            // show properties of the particular device
            // 直接配置 对话框
            //FrmMqttDevProps.ShowDialog(Number, KPProps, AppDirs);
            // ShowDialog
            FrmMqttDevTemplate.ShowDialog(Number, AppDirs);
        }
    }
}
