using System;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Microsoft.AspNet.SignalR;

namespace TheLearningMaze_API
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            GlobalHost.Configuration.ConnectionTimeout = TimeSpan.FromSeconds(300);
            GlobalHost.Configuration.DisconnectTimeout = TimeSpan.FromSeconds(120);
            GlobalHost.Configuration.KeepAlive = TimeSpan.FromSeconds(10);
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }
    }
}
