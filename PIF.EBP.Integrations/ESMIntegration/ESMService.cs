using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PIF.EBP.Application.Commercialization.DTOs.IESMServiceModels;
using PIF.EBP.Application.Commercialization.Interfaces;
using PIF.EBP.Core.ESM.DTOs;
using PIF.EBP.Core.Exceptions;
using PIF.EBP.Core.Shared;
using PIF.EBP.Core.Utilities;
using PIF.EBP.Core.Utilities.Interfaces;
using PIF.EBP.Integrations.ESMIntegration.Response;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace PIF.EBP.Integrations.ESMIntegration
{
    public class ESMService: IESMService
    {
        private readonly IHttpClientService _httpClientService;
        public ESMService(HttpClientServiceFactory httpClientServiceFactory)
        {
            _httpClientService = httpClientServiceFactory.Create(ApplicationConsts.ESMHttpClientName);

        }
        public async Task<List<ServiceItem>> GetServicesList(string itemId)
        {
            var url = $"global/enterprise_service_commercialization/get_service/{itemId}";
            var responseContent = await _httpClientService.GetAsync(url);
             return  await HandleApiResponseAsync<List<ServiceItem>>(responseContent);
        }
        public async Task<ServiceRequestResponse> GetRequestsListByCompanyId(string companyId, string sort, string filter, int limit, int offset, string order)
        {
            var url = $"global/enterprise_service_commercialization/get_request_list/{companyId}?limit={limit}&offset={offset}";
            if (!string.IsNullOrEmpty(filter))
            {
                url+=$"&filter={filter}";
            }
            if (!string.IsNullOrEmpty(sort))
            {
                url+=$"&sort={sort}";
                if (!string.IsNullOrEmpty(order))
                {
                    url+=$" {order}";
                }
            }

            var responseContent = await _httpClientService.GetAsync(url);
            return await HandleApiResponseAsync<ServiceRequestResponse>(responseContent);
        }
        public async Task<Dictionary<string, double>> GetRequestCountByCompanyId(string companyId)
        {
            var url = $"global/enterprise_service_commercialization/get_request_count/{companyId}";
            var responseContent = await _httpClientService.GetAsync(url);
            return await HandleApiResponseAsync<Dictionary<string, double>>(responseContent);
        }
        public async Task<JObject> GetRequestDetailsByRequestId(string requestId)
        {
            var url = $"global/enterprise_service_commercialization/get_request_data/{requestId}";
            var responseContent = await _httpClientService.GetAsync(url);
            return await HandleApiResponseAsync<JObject>(responseContent);
        }
        public async Task<FieldsMetadata> GetFieldsMetadataByServiceId(string serviceId)
        {
            var url = $"global/enterprise_service_commercialization/fields_metadata/{serviceId}";
            var responseContent = await _httpClientService.GetAsync(url);
            return await HandleApiResponseAsync<FieldsMetadata>(responseContent);
        }
        public async Task<List<SurveyQuestion>> GetSurveyQuestionsByRequestId(string requestId)
        {
            var url = $"global/enterprise_service_commercialization/get_survey_questions/{requestId}";
            var responseContent = await _httpClientService.GetAsync(url);
            return await HandleApiResponseAsync<List<SurveyQuestion>>(responseContent);
        }
        public async Task<SurveyStatus> GetSurveyStatusByRequestId(string requestId)
        {
            var url = $"global/enterprise_service_commercialization/get_survey_state/{requestId}";
            var responseContent = await _httpClientService.GetAsync(url);
            return await HandleApiResponseAsync<SurveyStatus>(responseContent);
        }
        public async Task<string> GetAttachmentByItemId(string itemId)
        {
            var url = $"now/attachment/{itemId}/file";
            var response = await _httpClientService.GetByteAsync(url);
            byte[] imageData = response.Item1;
            // Convert the byte array to base64
            string base64Image = Convert.ToBase64String(imageData);

            return base64Image;
        }
        public async Task<(byte[],string, JObject)> GetAttachmentByteByItemId(string itemId)
        {
            var url = $"now/attachment/{itemId}/file";
            var response = await _httpClientService.GetByteAsync(url);
            return response;
        }
        public async Task<JObject> GetAttachmentDetailsByItemId(string itemId)
        {
            var url = $"now/attachment/{itemId}";
            var response = await _httpClientService.GetAsync(url);
            return await HandleApiResponseAsync<JObject>(response);
        }
        public async Task<CreateRequestResponse> CreateRequest(string serviceId, Dictionary<string, string> formDictionary)
        {
            var url = $"global/enterprise_service_commercialization/create_request/{serviceId}";

            var jsonContent = JsonConvert.SerializeObject(formDictionary);

            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await _httpClientService.PostAsync(url, content);
            return await HandleApiResponseAsync<CreateRequestResponse>(response.Item2);
        }

        public async Task<CreateRequestResponse> UpdateRequest(string requestId, Dictionary<string, string> formDictionary)
        {
            var url = $"global/enterprise_service_commercialization/update_request/{requestId}";

            var jsonContent = JsonConvert.SerializeObject(formDictionary);

            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var responseContent = await _httpClientService.PutAsync(url, content);
            return await HandleApiResponseAsync<CreateRequestResponse>(responseContent);
        }

        public async Task<string> SubmitSurvey(string requestId, SurveyForm surveyForm)
        {
            var url = $"global/enterprise_service_commercialization/submit_survey/{requestId}";

            var jsonContent = JsonConvert.SerializeObject(surveyForm);

            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await _httpClientService.PostAsync(url, content);

            if (response.Item1)
            {
                return "Success";
            }
            return await HandleApiResponseAsync<string>(response.Item2);
        }
        public async Task<object> AttachFileToRequest(string requestId, EsmUploadedDocument document)
        {
            var url = $"now/attachment/file?table_name=sc_req_item&table_sys_id={requestId}&file_name={document.Name}";

            var request = new HttpRequestMessage(System.Net.Http.HttpMethod.Post, url);
            request.Content = new ByteArrayContent(document.Bytes);
            request.Content.Headers.ContentType = new MediaTypeHeaderValue(MapMimeType(document.Extension?.ToLowerInvariant()));

            var response = await _httpClientService.SendAsync(request);
            if (response.Item1)
            {
                return "Success";
            }
            return await HandleApiResponseAsync<object>(response.Item2);
        }
        public async Task<object> AttachFileToField(string requestId, EsmUploadedDocument document)
        {
            var contentType = new MediaTypeHeaderValue(MapMimeType(document.Extension?.ToLowerInvariant()));
            var url = $"global/enterprise_service_commercialization/upload_field_attachment/{requestId}?field={document.KeyName}&fileName={document.Name}&contentType={contentType}";

            var request = new HttpRequestMessage(System.Net.Http.HttpMethod.Post, url);
            request.Content = new ByteArrayContent(document.Bytes);

            var response = await _httpClientService.SendAsync(request);
            if (response.Item1)
            {
                return "Success";
            }
            return await HandleApiResponseAsync<object>(response.Item2);
        }
        public async Task<string> GetCategory(string sysParm,string sysParmFields)
        {
            var url = $"now/table/sc_category?sysparm_query={sysParm}";
            if (!string.IsNullOrEmpty(sysParmFields))
                url += $"&sysparm_fields={sysParmFields}";
            var responseContent = await _httpClientService.GetAsync(url);
            return responseContent;
        }

        private async Task<T> HandleApiResponseAsync<T>(string responseContent)
        {

            if (string.IsNullOrWhiteSpace(responseContent))
            {
                throw new UserFriendlyException("Empty response from the API.");
            }

            ApiResponse<T> apiResponse;
            try
            {
                apiResponse = JsonConvert.DeserializeObject<ApiResponse<T>>(responseContent);
            }
            catch (JsonException ex)
            {
                throw new UserFriendlyException("Invalid JSON response from the API.", ex);
            }

            if (!string.IsNullOrEmpty(apiResponse.Error))
            {
                throw new UserFriendlyException(apiResponse.Error);
            }

            return apiResponse.Result;
        }

        private string MapMimeType(string fileExtension)
        {
            string mimeType = string.Empty;

            switch (fileExtension?.ToLowerInvariant())
            {
                case ".pdf":
                    mimeType = "application/pdf";
                    break;
                case ".jpeg":
                case ".jpg":
                    mimeType = "image/jpeg";
                    break;
                case ".png":
                    mimeType = "image/png";
                    break;
                case ".txt":
                    mimeType = "text/plain";
                    break;
                case ".gif":
                    mimeType = "image/gif";
                    break;
                case ".bmp":
                    mimeType = "image/bmp";
                    break;
                case ".tiff":
                case ".tif":
                    mimeType = "image/tiff";
                    break;
                case ".doc":
                    mimeType = "application/msword";
                    break;
                case ".docx":
                    mimeType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
                    break;
                case ".xls":
                    mimeType = "application/vnd.ms-excel";
                    break;
                case ".xlsx":
                    mimeType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    break;
                case ".ppt":
                    mimeType = "application/vnd.ms-powerpoint";
                    break;
                case ".pptx":
                    mimeType = "application/vnd.openxmlformats-officedocument.presentationml.presentation";
                    break;
                case ".csv":
                    mimeType = "text/csv";
                    break;
                case ".xml":
                    mimeType = "application/xml";
                    break;
                case ".zip":
                    mimeType = "application/zip";
                    break;
                case ".rar":
                    mimeType = "application/x-rar-compressed";
                    break;
                case ".7z":
                    mimeType = "application/x-7z-compressed";
                    break;
                case ".mp3":
                    mimeType = "audio/mpeg";
                    break;
                case ".wav":
                    mimeType = "audio/wav";
                    break;
                case ".mp4":
                    mimeType = "video/mp4";
                    break;
                case ".avi":
                    mimeType = "video/x-msvideo";
                    break;
                case ".mov":
                    mimeType = "video/quicktime";
                    break;
                case ".html":
                case ".htm":
                    mimeType = "text/html";
                    break;
                case ".css":
                    mimeType = "text/css";
                    break;
                case ".js":
                    mimeType = "application/javascript";
                    break;
                case ".json":
                    mimeType = "application/json";
                    break;
                default:
                    throw new UserFriendlyException("InvalidFileExtension", System.Net.HttpStatusCode.BadRequest);
            }
            return mimeType;
        }
    }
}
