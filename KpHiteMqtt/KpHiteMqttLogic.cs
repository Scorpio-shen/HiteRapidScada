using KpHiteMqtt.Mqtt.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scada.Comm.Devices
{
    public class KpHiteMqttLogic : KPLogic
    {
        DeviceTemplate deviceTemplate;
        public KpHiteMqttLogic() : base()
        {
            
        }

        public KpHiteMqttLogic(int number) : base(number) 
        {
        }

        public override void OnAddedToCommLine()
        {
            string fileName = ReqParams.CmdLine.Trim();
            InitDeviceTemplate(fileName);
            InputChannels = new Dictionary<int, InputChannelModel>();
            InputChannels = deviceTemplate.InCnls.ToDictionary(incnl => incnl.CnlNum, incnl =>
            {
                var model = new InputChannelModel
                {
                    InCnl = incnl,
                    Value = null,

                };
                model.ValueChanged += Model_ValueChanged;
                return model;
            });
        }

        private void InitDeviceTemplate(string fileName)
        {
            deviceTemplate = new DeviceTemplate();
            string filePath = Path.IsPathRooted(fileName) ?
                        fileName : Path.Combine(AppDirs.ConfigDir, fileName);
            if (!deviceTemplate.Load(filePath, out string errMsg))
            {
                WriteToLog($"KpHiteMqttLogic:InitDeviceTemplate,初始化DeviceTemplate异常,{errMsg}");
                return;
            }


        }

        public override void OnCommLineStart()
        {
            base.OnCommLineStart();
        }

        public override void Session()
        {
            base.Session();
        }


    }
}
