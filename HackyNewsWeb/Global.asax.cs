using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace HackyNewsWeb
{
    public class Global : HttpApplication
    {
        protected void Application_Start()
        {
            // to account for SSL error
            ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;

            AreaRegistration.RegisterAllAreas();
            RouteConfig.RegisterRoutes(RouteTable.Routes);
        }
    }
}
