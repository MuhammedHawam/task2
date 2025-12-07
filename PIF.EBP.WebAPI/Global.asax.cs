using PIF.EBP.WebAPI.App_Start;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Routing;

namespace PIF.EBP.WebAPI
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            bool.TryParse(ConfigurationManager.AppSettings["EnableSwagger"], out bool enableSwagger);
            if (enableSwagger)
            {
                SwaggerConfig.Register();
            }
            GlobalConfiguration.Configure(WebApiConfig.Register);
        }
    }
}
