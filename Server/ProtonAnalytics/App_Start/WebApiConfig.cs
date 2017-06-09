using ProtonAnalytics.App_Start.RuntimeConfiguration;
using ProtonAnalytics.Repositories;
using System.Configuration;
using System.Web.Http;

namespace ProtonAnalytics
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config.MapHttpAttributeRoutes();

            System.Diagnostics.Debugger.Break();
            // We don't have an initialized FeatureTogglesRepository instance. Make one.
            var repo = new FeatureTogglesRepository(ConfigurationManager.ConnectionStrings["DefaultConnection"]);

            if (repo.IsToggleEnabled("EnableCors"))
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
