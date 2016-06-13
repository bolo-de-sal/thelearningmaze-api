using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Owin;
using Owin;
using TheLearningMaze_API.Models;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin.Cors;

[assembly: OwinStartup(typeof(TheLearningMaze_API.Startup))]

namespace TheLearningMaze_API
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // Configure the db context and user manager to use a single instance per request
            app.CreatePerOwinContext(ApplicationDbContext.Create);

            // Inicia aplicação signalR
            app.Map("/signalr", map =>
            {
                map.UseCors(CorsOptions.AllowAll);
                var hubConfiguration = new HubConfiguration { EnableJSONP = true };
                map.RunSignalR(hubConfiguration);
            });
        }
    }
}
