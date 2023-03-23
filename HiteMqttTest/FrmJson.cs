using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Extensions.Configuration;
using System.Diagnostics;

namespace HiteMqttTest
{
    public partial class FrmJson : Form
    {
        public FrmJson()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var jsonPath = AppDomain.CurrentDomain.BaseDirectory + "appsettings.json";
            AmsRouterSettings setting = null;
            using (StreamReader sr = new StreamReader(jsonPath))
            {
                using (JsonTextReader jsonReader = new JsonTextReader(sr))
                {
                    JObject obj = (JObject)JToken.ReadFrom(jsonReader);
                    var str = obj.ToString();
                    setting = JsonConvert.DeserializeObject<AmsRouterSettings>(str);
                }
            }
            setting.AmsRouter.NetId = new TwinCAT.Ads.AmsNetId("192.168.0.111.1.1");
            setting.AmsRouter.TcpPort = 48898;

            List<RouteConfig> routeConfigs = new List<RouteConfig>();
            routeConfigs.Add(new RouteConfig()
            {
                Name = "RemoteSystem",
                Address = "192.168.0.100",
                NetId = new TwinCAT.Ads.AmsNetId("169.254.1.1.1.1"),
                Type = "TCP_IP"
            });
            setting.AmsRouter.RemoteConnections = routeConfigs.ToArray();

            JObject save = JObject.FromObject(setting);
            using (StreamWriter sw = new StreamWriter(jsonPath))
            {
                using (JsonTextWriter jsonWrite = new JsonTextWriter(sw))
                {
                    save.WriteTo(jsonWrite);
                }
            }


            var builder = new ConfigurationBuilder().AddJsonFile("appsettings.json"); //默认读取：当前运行目录
            IConfigurationRoot configuration = builder.Build();
            setting = configuration.Get<AmsRouterSettings>();

            Debug.WriteLine(JsonConvert.SerializeObject(setting));
        }

        private void button2_Click(object sender, EventArgs e)
        {

        }
    }
}
