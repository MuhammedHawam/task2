using Newtonsoft.Json.Linq;
using PIF.EBP.Application.Commercialization.DTOs.IESMServiceModels;
using PIF.EBP.Core.DependencyInjection;
using PIF.EBP.Core.ESM.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PIF.EBP.Application.Commercialization.Interfaces
{
    public interface IESMService : ITransientDependency
    {
        Task<List<ServiceItem>> GetServicesList(string itemId);
        Task<ServiceRequestResponse> GetRequestsListByCompanyId(string companyId, string sort, string filter, int limit, int offset, string order);
        Task<Dictionary<string, double>> GetRequestCountByCompanyId(string companyId);
        Task<JObject> GetRequestDetailsByRequestId(string requestId);
        Task<FieldsMetadata> GetFieldsMetadataByServiceId(string serviceId);
        Task<CreateRequestResponse> CreateRequest(string serviceId, Dictionary<string, string> formDictionary);
        Task<CreateRequestResponse> UpdateRequest(string requestId, Dictionary<string, string> formDictionary);
        Task<List<SurveyQuestion>> GetSurveyQuestionsByRequestId(string requestId);
        Task<SurveyStatus> GetSurveyStatusByRequestId(string requestId);
        Task<string> GetAttachmentByItemId(string itemId);
        Task<(byte[], string, JObject)> GetAttachmentByteByItemId(string itemId);
        Task<JObject> GetAttachmentDetailsByItemId(string itemId);
        Task<string> SubmitSurvey(string requestId, SurveyForm surveyForm);
        Task<object> AttachFileToRequest(string requestId, EsmUploadedDocument document);
        Task<object> AttachFileToField(string requestId, EsmUploadedDocument document);
    }
}
