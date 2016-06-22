using System.Web.Http;
using Microsoft.Owin.Security.OAuth;
using System.Net.Http.Headers;
using System.Web.Http.Cors;

namespace TheLearningMaze_API
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Habilitar CORS para uso a partir do Angular
            var cors = new EnableCorsAttribute("*", "*", "*");
            config.EnableCors(cors);

            // Web API configuration and services
            // Configure Web API to use only bearer token authentication.
            config.SuppressDefaultHostAuthentication();
            config.Filters.Add(new HostAuthenticationFilter(OAuthDefaults.AuthenticationType));

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            //Retorno em JSON
            config.Formatters.JsonFormatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("text/html"));
        }
    }
}
