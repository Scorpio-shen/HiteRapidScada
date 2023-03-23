using KpHiteBeckHoff.Model;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TwinCAT.Ads;
using TwinCAT.Ads.TcpRouter;

namespace KpHiteBeckHoff.Service
{
    public class RouterService
    {
        private readonly IConfiguration _configuration;
        private AmsTcpIpRouter router;
        public RouterStatus Status { get => router.RouterStatus; }
        public RouterService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task ExecuteAsync(CancellationToken cancel)
        {
            router = new AmsTcpIpRouter(_configuration);
            router.RouterStatusChanged += Router_RouterStatusChanged;
            Task routerTask = router.StartAsync(cancel);
            await routerTask;
        }

        private void Router_RouterStatusChanged(object sender, RouterStatusChangedEventArgs e)
        {
            Debug.WriteLine($"RouterService:Router_RouterStatusChanged,Status:{e.RouterStatus}");
        }
    }
}
