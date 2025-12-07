using PIF.EBP.Application.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using PIF.EBP.Application.AccessManagement;
using PIF.EBP.Core.DependencyInjection;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Configuration;
using PIF.EBP.Application.AccessManagement.DTOs;
using static PIF.EBP.Application.Shared.Enums;

namespace PIF.EBP.WebAPI.Middleware.Authorize
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class EBPAuthorize : ActionFilterAttribute
    {
        private IAccessManagementAppService _accessManagementAppService;
        private List<PageEntry> _pages = new List<PageEntry>();
        public string[] Pages
        {
            get => _pages.Select(pe => $"{pe.Page}:{pe.PermissionType}").ToArray();
            set => _pages = value.Select(p =>
            {
                var parts = p.Split(':');
                return new PageEntry { Page = parts[0], PermissionType = parts[1] };
            }).ToList();
        }

        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            TokenValidation(actionContext);
            CheckPortalPermission(actionContext);

            base.OnActionExecuting(actionContext);
        }
        private void TokenValidation(HttpActionContext actionContext)
        {
            var requestHeaders = actionContext.Request.Headers;
            if (!requestHeaders.Contains("Authorization"))
            {
                actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.Unauthorized, "MissingAuthenticationToken");
                return;
            }
            var token = requestHeaders.GetValues("Authorization").FirstOrDefault();

            var _serverUrl = ConfigurationManager.AppSettings["IdentityServerUrl"];
            var _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add(
                               ConfigurationManager.AppSettings["IdentityServerAPIKeyName"],
                                              ConfigurationManager.AppSettings["IdentityServerAPIKeyValue"]);

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = _httpClient.GetAsync($"{_serverUrl}/verify-token").Result;

            if (!response.IsSuccessStatusCode)
            {
                actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.Unauthorized, "InvalidToken");
            }
        }

        private void CheckPortalPermission(HttpActionContext actionContext)
        {
            if (Convert.ToBoolean(ConfigurationManager.AppSettings["AuthPermissions"]) && (_pages !=null && _pages.Any()))
            {
                _accessManagementAppService = WindsorContainerProvider.Container.Resolve<IAccessManagementAppService>();

                List<AuthPermission> cachedItem = Task.Run(async () => await _accessManagementAppService.GetAuthorizedPermissions()).GetAwaiter().GetResult();
                if (cachedItem == null)
                {
                    actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.Forbidden, "Unauthorized");
                }
                var pageEntries = new HashSet<string>(_pages.Select(p => p.Page));
                var filteredItems = cachedItem.Where(x => pageEntries.Contains(x.PageLink)).ToList();

                bool hasPermission = filteredItems.Any(x => _pages.Any(p =>
                    p.Page == x.PageLink && (
                        (p.PermissionType == PermissionType.Read && (x.Read == (int)AccessLevel.Deep || x.Read == (int)AccessLevel.Basic)) ||
                        (p.PermissionType == PermissionType.Create && (x.Create == (int)AccessLevel.Deep || x.Create == (int)AccessLevel.Basic)) ||
                        (p.PermissionType == PermissionType.Write && (x.Write == (int)AccessLevel.Deep || x.Write == (int)AccessLevel.Basic)) ||
                        (p.PermissionType == PermissionType.Delete && (x.Delete == (int)AccessLevel.Deep || x.Delete == (int)AccessLevel.Basic))
                    )));

                if (!hasPermission)
                {
                    actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.Forbidden, "Unauthorized");
                }

            }
        }
    }
    public class PageEntry
    {
        public string Page { get; set; }
        public string PermissionType { get; set; }
    }
}