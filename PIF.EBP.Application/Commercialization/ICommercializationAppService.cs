using Newtonsoft.Json.Linq;
using PIF.EBP.Application.Commercialization.DTOs;
using PIF.EBP.Core.DependencyInjection;
using PIF.EBP.Core.ESM.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PIF.EBP.Application.Commercialization
{
    public interface ICommercializationAppService : ITransientDependency
    {
        Task<CustomizedItemDto> GetServicesList(string itemId);
        Task<EsmRequestListRsp> GetRequestsListByCompanyId(EsmRequestListReq esmRequestListReq);
        Task<EsmWidgetDataRsp> GetServicesWidgetData(int status, int size);
        Task<RequestDetailsDto> GetRequestDetailsByRequestId(string requestId);
        Task<FieldsMetadataDto> GetFieldsMetadataByServiceId(string serviceId);
        Task<CreateUpdateRequestResponseDto> CreateRequest(string serviceId, Dictionary<string, string> formDictionary);
        Task<CreateUpdateRequestResponseDto> UpdateRequest(string requestId, Dictionary<string, string> formDictionary);
        Task<List<SurveyQuestionDto>> GetSurveyQuestionsByRequestId(string requestId);
        Task<SurveyStatusDto> GetSurveyStatusByRequestId(string requestId);
        Task<string> GetAttachmentByItemId(string itemId);
        Task<(byte[], string, JObject)> GetAttachmentByteByItemId(string itemId);
        Task<string> SubmitSurvey(SubmitSurveyReq submitSurveyReq);
        Task CommercializationUpload(string requestId, List<EsmUploadedDocument> documents);
        Task<TriggerNotificationResponse> TriggerNotification(TriggerNotificationRequest triggerNotificationRequest);
        Task<ServiceFileResponseDto> GetServiceFiles(string requestId);
    }
}
