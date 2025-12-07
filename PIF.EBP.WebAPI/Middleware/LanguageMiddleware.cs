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

namespace PIF.EBP.WebAPI.Middleware
{
    public class LanguageMiddleware : ActionFilterAttribute
    {
        public override async void OnActionExecuting(HttpActionContext actionContext)
        {
            // Check if the key is provided in the headers
            var requestHeaders = actionContext.Request.Headers;
            if (!requestHeaders.Contains("Authorization"))
            {
                actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.Unauthorized, "MissingAuthenticationToken");
                return;
            }

            var token = requestHeaders.GetValues("Authorization").FirstOrDefault();

            var _serverUrl = ConfigurationManager.AppSettings["IdentityServerUrl"];
            var _httpClient = new HttpClient();

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await _httpClient.GetAsync($"{_serverUrl}/verify-token");

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.OK, responseContent);
            }
            else
            {
                actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.Unauthorized, "InvalidToken");
            }

            base.OnActionExecuting(actionContext);
        }
    }
}