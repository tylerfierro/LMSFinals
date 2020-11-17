using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(LMSFinals.UI.MVC.Startup))]
namespace LMSFinals.UI.MVC
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
