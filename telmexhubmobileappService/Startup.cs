using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(telmexhubmobileappService.Startup))]

namespace telmexhubmobileappService
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureMobileApp(app);
        }
    }
}