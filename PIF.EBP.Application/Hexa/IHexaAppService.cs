using PIF.EBP.Application.ExternalFormConfiguration.DTOs;
using PIF.EBP.Application.Hexa.DTOs;
using PIF.EBP.Core.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PIF.EBP.Application.Hexa
{
    public interface IHexaAppService : ITransientDependency
    {
        Task<List<SaveFormResponse>> SaveForm(FormDto formDto);
        List<EntityDetailsDto> RetrieveEntitySchema(ExternalFormConfigDto FormConfig);
        Task<HexaRequestDto> RetrieveHexaRequestById(Guid RequestId);
        Task<HexaRequestStepDto> RetrieveHexaRequestStepById(Guid RequestStepId);
        List<HexaStepTransitionDto> RetrieveStepTransitions(Guid? ProcessStepId, Guid? ProcessTemplateId);
        void UpdateHexaReqDocNoOfUploadedDoc(Guid ReqDocId, int ItemCount, bool IsUpload, string folderPath = null);
        Task<List<HexaProcessTemplateDto>> RetrieveProcessTemplates();
        List<TransitionHistoryDto> RetrieveTransitionHistoriesByRequestId(Guid requestId);
        List<PortalCommentDto> GetPortalCommentByRequestId(Guid requestId);
    }
}
