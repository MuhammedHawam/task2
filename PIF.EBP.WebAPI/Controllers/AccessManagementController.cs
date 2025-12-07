using PIF.EBP.Application.AccessManagement;
using PIF.EBP.Core.DependencyInjection;
using PIF.EBP.WebAPI.Middleware.ActionFilter;
using System.Threading.Tasks;
using System.Web.Http;

namespace PIF.EBP.WebAPI.Controllers
{
    [ApiResponseWrapper]
    [RoutePrefix("AccessManagement")]
    public class AccessManagementController : BaseController
    {
        private readonly IAccessManagementAppService _accessManagementAppService;
        public AccessManagementController()
        {
            _accessManagementAppService = WindsorContainerProvider.Container.Resolve<IAccessManagementAppService>();
        }

        [HttpGet]
        [Route("roles-and-permissions")]
        public async Task<IHttpActionResult> RolesandPermissions()
        {
            var result = await _accessManagementAppService.GetRolesAndPermissionByContactId();

            return Ok(result);
        }
    }
}
