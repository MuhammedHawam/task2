using PIF.EBP.Application.PortalAdministration;
using PIF.EBP.Core.DependencyInjection;
using PIF.EBP.Core.Exceptions;
using PIF.EBP.Core.Utilities;
using PIF.EBP.WebAPI.Middleware.ActionFilter;
using System;
using System.Threading.Tasks;
using System.Web.Http;

namespace PIF.EBP.WebAPI.Controllers
{
    [ApiResponseWrapper]
    [RoutePrefix("Administration")]
    public class PortalAdministrationController : BaseController
    {
        private readonly IPortalAdministrationAppService _portalAdministrationAppService;
        public PortalAdministrationController()
        {
            _portalAdministrationAppService = WindsorContainerProvider.Container.Resolve<IPortalAdministrationAppService>();
        }

        [HttpGet]
        [Route("get-companies")]
        public async Task<IHttpActionResult> GetCompanies()
        {
            //we need to take the contactId from the token
            var result = await _portalAdministrationAppService.RetrievecompaniesByContactId();

            return Ok(result);
        }
        [HttpGet]
        [Route("get-company-by-id")]
        public IHttpActionResult GetCompanyById(Guid companyId)
        {
            if (companyId==Guid.Empty)
            {
                throw new UserFriendlyException("NullArgument");
            }
            var result = _portalAdministrationAppService.RetrieveCompanyById(companyId);

            return Ok(result);
        }
        [HttpGet]
        [Route("switch-profile")]
        public async Task<IHttpActionResult> SwitchProfile(string portalRoleAssociationId)
        {
            Guard.AssertArgumentNotNull(portalRoleAssociationId);
            var result = await _portalAdministrationAppService.SwitchProfile(portalRoleAssociationId);

            return Ok(result);
        }
    }
}
