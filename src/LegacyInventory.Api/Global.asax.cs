using System;
using System.Web;
using System.Web.Http;
using log4net;

namespace LegacyInventory.Api
{
    public class WebApiApplication : HttpApplication
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(WebApiApplication));

        protected void Application_Start()
        {
            log4net.Config.XmlConfigurator.Configure();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            Log.Info("LegacyInventory.Api started");
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            var error = Server.GetLastError();
            Log.Error("Unhandled exception", error);
        }
    }
}
