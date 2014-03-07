using log4net;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Session.WEB.Hubs {
    [HubName("HomeHub")]
    public class HomeHub : Hub {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public HomeHub() {
            IHubContext context = GlobalHost.ConnectionManager.GetHubContext<HomeHub>();
            //context.Clients.All.ConnectionEstablished();
        }

        private static int hitCounter = 0;

        public int ButtonPress() {
            return ++hitCounter;
        }
    }
}