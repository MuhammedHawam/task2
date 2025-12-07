using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Net;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using System.Configuration;
using PIF.EBP.Application.AccessManagement;
using PIF.EBP.Core.DependencyInjection;
using PIF.EBP.Application.PortalConfiguration;
using System.Threading.Tasks;
using PIF.EBP.Application.Shared;

namespace PIF.EBP.WebAPI.Middleware.Authorize
{
    public class APIKEY : ActionFilterAttribute
    {
        private IPortalConfigAppService _portalConfigAppService;
        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            var requestHeaders = actionContext.Request.Headers;
            if (!requestHeaders.Contains("X-API-KEY"))
            {
                actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.Unauthorized, "MissingAuthenticationToken");
            }
            var ApiKey = requestHeaders.GetValues("X-API-KEY").FirstOrDefault();

            _portalConfigAppService = WindsorContainerProvider.Container.Resolve<IPortalConfigAppService>();
            var configs = _portalConfigAppService.RetrievePortalConfiguration(new List<string> { PortalConfigurations.CrmIntegrationApiKey, PortalConfigurations.EsmIntegrationApiKey }).ToList();
            if (configs ==null)
            {
                actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.NotFound, "CouldNotFindAPIKeyTokenInPortalConfiguration");
            }

            if (!configs.Any(a => a.Value!= ApiKey))
            {
                actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.Unauthorized, "InvalidAPIKeyToken");
            }

            base.OnActionExecuting(actionContext);
        }
    }
}