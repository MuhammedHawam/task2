using PIF.EBP.Application.ExternalFormConfiguration;
using PIF.EBP.Application.ExternalFormConfiguration.DTOs;
using PIF.EBP.Application.PortalConfiguration;
using PIF.EBP.Application.PortalConfiguration.DTOs;
using PIF.EBP.Core.DependencyInjection;
using PIF.EBP.WebAPI.Middleware.ActionFilter;
using PIF.EBP.WebAPI.Middleware.Authorize;
using System;
using System.Threading.Tasks;
using System.Web.Http;

namespace PIF.EBP.WebAPI.Controllers
{
    [ApiResponseWrapper]
    [RoutePrefix("PortalConfiguration")]
    public class PortalConfigurationController : ApiController
    {
        private readonly IPortalConfigAppService _portalConfigAppService;
        private readonly IExternalFormConfigAppService _externalFormConfigAppService;

        public PortalConfigurationController()
        {
            _portalConfigAppService = WindsorContainerProvider.Container.Resolve<IPortalConfigAppService>();
            _externalFormConfigAppService = WindsorContainerProvider.Container.Resolve<IExternalFormConfigAppService>();
        }

        [HttpPost]
        [Route("get-portal-configuration")]
        public async Task<IHttpActionResult> GetPortalConfiguration([FromBody] PortalConfigRequestDto portalConfigRequestDto)
        {
            var result = _portalConfigAppService.RetrievePortalConfiguration(portalConfigRequestDto?.Keys);

            return Ok(result);
        }

        [HttpPost]
        [EBPAuthorize]
        [Route("create-or-update-portal-configuration")]
        public async Task<IHttpActionResult> CreateOrUpdatePortalConfiguration([FromBody] CreateOrUpdatePortalConfigDto createOrUpdatePortalConfigDto)
        {
            var result = await _portalConfigAppService.CreateOrUpdatePortalConfiguration(createOrUpdatePortalConfigDto);

            return Ok(result);
        }

        [HttpGet]
        [EBPAuthorize]
        [Route("get-external-form-configuration")]
        public IHttpActionResult GetExternalFormConfiguration(Guid? Id = null, int? Type = null)
        {
            var result = _externalFormConfigAppService.RetrieveExternalFormConfiguration(Id,Type);

            return Ok(result);
        }

        [HttpPost]
        [EBPAuthorize]
        [Route("create-external-form-configuration")]
        public IHttpActionResult CreateExternalFormConfiguration(ExternalFormConfigDto ExternalFormConfig)
        {
            var result = _externalFormConfigAppService.CreateExternalFormConfiguration(ExternalFormConfig);

            return Ok(result);
        }

        [HttpPut]
        [EBPAuthorize]
        [Route("update-external-form-configuration")]
        public IHttpActionResult UpdateExternalFormConfiguration(ExternalFormConfigDto ExternalFormConfig)
        {
            _externalFormConfigAppService.UpdateExternalFormConfiguration(ExternalFormConfig);

            return Ok();
        }

        [HttpDelete]
        [EBPAuthorize]
        [Route("delete-external-form-configuration")]
        public IHttpActionResult DeleteExternalFormConfiguration(string Id)
        {
            _externalFormConfigAppService.DeleteExternalFormConfiguration(Id);

            return Ok();
        }
    }
}
