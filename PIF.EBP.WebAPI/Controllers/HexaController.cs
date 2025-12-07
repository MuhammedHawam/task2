using PIF.EBP.Application.ExternalFormConfiguration;
using PIF.EBP.Application.ExternalFormConfiguration.DTOs;
using PIF.EBP.Application.Hexa;
using PIF.EBP.Application.Hexa.DTOs;
using PIF.EBP.Application.Sharepoint;
using PIF.EBP.Core.DependencyInjection;
using PIF.EBP.Core.Exceptions;
using PIF.EBP.Core.FileManagement.DTOs;
using PIF.EBP.WebAPI.Middleware.ActionFilter;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;

namespace PIF.EBP.WebAPI.Controllers
{
    [ApiResponseWrapper]
    [RoutePrefix("Hexa")]
    public class HexaController : BaseController
    {
        private readonly IHexaAppService _hexaAppService;
        private readonly IExternalFormConfigAppService _externalFormConfigAppService;
        private readonly IHexaDocumentManagementService _sharepointService;
        public HexaController()
        {
            _hexaAppService = WindsorContainerProvider.Container.Resolve<IHexaAppService>();
            _externalFormConfigAppService = WindsorContainerProvider.Container.Resolve<IExternalFormConfigAppService>();
            _sharepointService = WindsorContainerProvider.Container.Resolve<IHexaDocumentManagementService>();
        }

        [HttpPost]
        [Route("SaveForm")]
        public async Task<IHttpActionResult> SaveForm(FormDto FormDto)
        {
            if (FormDto.ProcessTemplateId != null && (FormDto.TransitionId != null || FormDto.RequestStepId != null))
            {
                throw new UserFriendlyException("RequiredParameters", System.Net.HttpStatusCode.BadRequest);
            }

            var result = await _hexaAppService.SaveForm(FormDto);

            return Ok(result);
        }

        [HttpGet]
        [Route("get-available-entities")]
        public IHttpActionResult GetEntitySchema(Guid? FormId)
        {
            ExternalFormConfigDto externalFormConfigDto = null;

            if (FormId != null && FormId != Guid.Empty)
            {
                externalFormConfigDto = _externalFormConfigAppService.RetrieveExternalFormConfiguration(FormId).FirstOrDefault();
            }

            var result = _hexaAppService.RetrieveEntitySchema(externalFormConfigDto);

            return Ok(result);
        }

        [HttpGet]
        [Route("get-Request-By-Id")]
        public async Task<IHttpActionResult> GetRequestById(Guid RequestId)
        {
            var result = await _hexaAppService.RetrieveHexaRequestById(RequestId);

            return Ok(result);
        }

        [HttpGet]
        [Route("get-Request-Step-By-Id")]
        public async Task<IHttpActionResult> GetRequestStepById(Guid RequestStepId)
        {
            var result = await _hexaAppService.RetrieveHexaRequestStepById(RequestStepId);

            return Ok(result);
        }

        [HttpGet]
        [Route("get-step-transitions")]
        public IHttpActionResult GetStepTransitions(Guid? ProcessStepId, Guid? ProcessTemplateId)
        {
            if ((ProcessStepId == null || ProcessStepId == Guid.Empty) && (ProcessTemplateId == null || ProcessTemplateId == Guid.Empty))
            {
                throw new UserFriendlyException("NullArgument");
            }
            var result = _hexaAppService.RetrieveStepTransitions(ProcessStepId, ProcessTemplateId);

            return Ok(result);
        }

        [HttpGet]
        [Route("reference-files-list")]
        public async Task<IHttpActionResult> GetReferenceFilesList(Guid RegardingObjectId, bool? byRequest = false)
        {
            var getDocuments = new GetDocumentsDto
            {
                RegardingObjectId = RegardingObjectId,
                ByRequest = byRequest
            };

            var result = await _sharepointService.RetrieveDocumentsList(getDocuments);
            return Ok(result);
        }

        [HttpGet]
        [Route("get-process-templates")]
        public async Task<IHttpActionResult> GetProcessTemplates()
        {
            var result = await _hexaAppService.RetrieveProcessTemplates();
            return Ok(result);
        }

        [HttpGet]
        [Route("get-transition-history-by-request-id")]
        public IHttpActionResult GetTransitionHistoryByRequestId(Guid RequestId)
        {
            var result = _hexaAppService.RetrieveTransitionHistoriesByRequestId(RequestId);
            return Ok(result);
        }

        [HttpGet]
        [Route("get-Comments-by-request-id")]
        public IHttpActionResult GetPortalCommentsByRequestId(Guid RequestId)
        {
            var result = _hexaAppService.GetPortalCommentByRequestId(RequestId);
            return Ok(result);
        }
    }
}