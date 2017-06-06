using ProtonAnalytics.App_Start.RuntimeConfiguration;
using System.Web.Http;

namespace ProtonAnalytics
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config.MapHttpAttributeRoutes();

            if (FeatureConfig.LastInstance.Get<bool>("EnableCors") == true)
            {
                config.EnableCors();
            }

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
        }
    }
}
