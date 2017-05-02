using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(ProtonAnalytics.Startup))]
namespace ProtonAnalytics
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
