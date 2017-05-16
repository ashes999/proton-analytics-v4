using ProtonAnalytics;
using ProtonAnalytics.App_Start.RuntimeConfiguration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace ProtonAnalytics
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register); // Web API
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            BundleConfig.RegisterBundles(BundleTable.Bundles);

            new FeatureConfig(System.Web.Hosting.HostingEnvironment.MapPath("~/App_Data/Configuration.json")); // JSON config
        }
    }
}
