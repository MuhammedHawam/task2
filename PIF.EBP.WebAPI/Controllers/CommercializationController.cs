using PIF.EBP.Application.Commercialization;
using PIF.EBP.Application.Commercialization.DTOs;
using PIF.EBP.Application.Contacts;
using PIF.EBP.Application.PerfomanceDashboard;
using PIF.EBP.Core.DependencyInjection;
using PIF.EBP.Core.Exceptions;
using PIF.EBP.WebAPI.Middleware.ActionFilter;
using PIF.EBP.WebAPI.Middleware.Authorize;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Net;
using PIF.EBP.Core.ESM.DTOs;
using PIF.EBP.Application.Shared;
using PIF.EBP.Application.PortalConfiguration;
using PIF.EBP.Application.FileScanning;
using static PIF.EBP.Application.Shared.Enums;

namespace PIF.EBP.WebAPI.Controllers
{
    [RoutePrefix("Commercialization")]
    [ApiResponseWrapper]
    public class CommercializationController : BaseController
    {
        private readonly ICommercializationAppService _commercializationAppService;
        private IContactAppService _contactAppService;
        private IPerformanceDashboardAppService _performanceDashboardAppService;
        private readonly IPortalConfigAppService _portalConfigAppService;
        private readonly IFileScanningService _FileScanService;
        public CommercializationController()
        {
            _commercializationAppService = WindsorContainerProvider.Container.Resolve<ICommercializationAppService>();
            _portalConfigAppService = WindsorContainerProvider.Container.Resolve<IPortalConfigAppService>();
            _FileScanService = WindsorContainerProvider.Container.Resolve<IFileScanningService>();
        }

        [HttpGet]
        [Route("services-list")]
        public async Task<IHttpActionResult> GetServices(string itemId)
        {
            var response = await _commercializationAppService.GetServicesList(itemId);
            return Ok(response);
        }

        [HttpGet]
        [Route("request-details")]
        public async Task<IHttpActionResult> GetRequestDetails(string requestId)
        {
            var response = await _commercializationAppService.GetRequestDetailsByRequestId(requestId);
            return Ok(response);
        }

        [HttpGet]
        [Route("fields-metadata")]
        public async Task<IHttpActionResult> GetFieldsMetadata(string serviceId)
        {
            var response = await _commercializationAppService.GetFieldsMetadataByServiceId(serviceId);
            return Ok(response);
        }

        [HttpGet]
        [Route("survey-questions")]
        public async Task<IHttpActionResult> GetSurveyQuestions(string requestId)
        {
            var response = await _commercializationAppService.GetSurveyQuestionsByRequestId(requestId);
            return Ok(response);
        }

        [HttpGet]
        [Route("survey-status")]
        public async Task<IHttpActionResult> GetSurveyStatus(string requestId)
        {
            var response = await _commercializationAppService.GetSurveyStatusByRequestId(requestId);
            return Ok(response);
        }

        [HttpGet]
        [Route("attachment-by-itemid")]
        public async Task<IHttpActionResult> GetAttachment(string itemId)
        {
            var response = await _commercializationAppService.GetAttachmentByItemId(itemId);
            return Ok(response);
        }

        [HttpGet]
        [OverrideActionFilters]
        [EBPAuthorize]
        [Route("attachment-byte-by-itemid")]
        public async Task<HttpResponseMessage> GetAttachmentByte(string itemId)
        {
            var (fileData, contentType,metadata) = await _commercializationAppService.GetAttachmentByteByItemId(itemId);


            // Create an HttpResponseMessage with the byte array
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(fileData)
            };

            // Set the media type - adjust this as needed based on your actual file type
            response.Content.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment");
            if (metadata !=null)
            {
                response.Content.Headers.ContentDisposition.FileName = metadata["file_name"]?.ToString();
            }

            // Safely parse the content type and charset
            if (!string.IsNullOrEmpty(contentType))
            {
                var mediaType = contentType.Split(';')[0].Trim();
                var charset = contentType.Contains("charset=") ? contentType.Split('=')[1].Trim() : null;

                response.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(mediaType);

                if (!string.IsNullOrEmpty(charset))
                {
                    response.Content.Headers.ContentType.CharSet = charset;
                }
            }
            else
            {
                // Default content type in case it's null or empty
                response.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");
            }

            return response;
        }


        [HttpGet]
        [Route("service-files")]
        public async Task<IHttpActionResult> GetServiceFiles(string requestId)
        {
            var response = await _commercializationAppService.GetServiceFiles(requestId);
            return Ok(response);
        }

        [HttpGet]
        [Route("services-widget-data")]
        public async Task<IHttpActionResult> GetServicesWidgetData(int? status = (int)ServicesFilter.All, int? size = 5)
        {
            var response = await _commercializationAppService.GetServicesWidgetData(status.Value, size.Value);
            return Ok(response);
        }

        [HttpPost]
        [Route("requests-list")]
        public async Task<IHttpActionResult> GetRequests(EsmRequestListReq esmRequestListReq)
        {
            var response = await _commercializationAppService.GetRequestsListByCompanyId(esmRequestListReq);
            return Ok(response);
        }

        [HttpPost]
        [Route("create-request/{serviceId}")]
        public async Task<IHttpActionResult> CreateRequest(string serviceId)
        {
            if (!Request.Content.IsMimeMultipartContent())
            {
                return StatusCode(HttpStatusCode.UnsupportedMediaType);
            }
            var formdata = HttpContext.Current.Request.Form;
            Dictionary<string, string> formDictionary = formdata.AllKeys.ToDictionary(key => key, key => formdata[key]);

            var provider = new MultipartMemoryStreamProvider();
            await Request.Content.ReadAsMultipartAsync(provider);
            var documents = await ExtractFiles(provider.Contents);
            CheckAllowedFileTypes(documents);

            var response = await _commercializationAppService.CreateRequest(serviceId, formDictionary);
            if (response !=null && !string.IsNullOrEmpty(response.RequestId) && documents.Any())
            {
                var configurations = _portalConfigAppService.RetrievePortalConfiguration(new List<string> { PortalConfigurations.EnableFileScanning });
                bool.TryParse(configurations.SingleOrDefault(a => a.Key == PortalConfigurations.EnableFileScanning).Value, out bool enableFileScanning);
                if (enableFileScanning)
                {
                    foreach (EsmUploadedDocument uploadedDoc in documents)
                    {
                        var metaData = new
                        {
                            EsmRequestId = response.RequestId,
                            DocumentExtension = uploadedDoc.Extension,
                            DocumentSize = uploadedDoc.Size,
                            FormDataKeyName = uploadedDoc.KeyName
                        };
                        await _FileScanService.AnalyzeFile(uploadedDoc.Bytes, uploadedDoc.Name, metaData);
                    }
                    response.SentForScanning = true;
                }
                else
                {
                    await _commercializationAppService.CommercializationUpload(response.RequestId, documents);
                }

            }
            return Ok(response);
        }

        [HttpPost]
        [Route("submit-survey")]
        public async Task<IHttpActionResult> SubmitSurvey(SubmitSurveyReq submitSurveyReq)
        {
            var response = await _commercializationAppService.SubmitSurvey(submitSurveyReq);
            return Ok(response);
        }

        [HttpPut]
        [Route("update-request/{requestId}")]
        public async Task<IHttpActionResult> UpdateRequest(string requestId)
        {
            if (!Request.Content.IsMimeMultipartContent())
            {
                return StatusCode(HttpStatusCode.UnsupportedMediaType);
            }
            var formdata = HttpContext.Current.Request.Form;
            Dictionary<string, string> formDictionary = formdata.AllKeys.ToDictionary(key => key, key => formdata[key]);

            var provider = new MultipartMemoryStreamProvider();
            await Request.Content.ReadAsMultipartAsync(provider);
            var documents = await ExtractFiles(provider.Contents);
            CheckAllowedFileTypes(documents);

            var response = await _commercializationAppService.UpdateRequest(requestId, formDictionary);
            return Ok(response);
        }

        [HttpPost]
        [OverrideActionFilters]
        [APIKEY]
        [Route("trigger-notification")]
        public async Task<IHttpActionResult> TriggerNotification(TriggerNotificationRequest triggerNotificationRequest)
        {
            if (string.IsNullOrEmpty(triggerNotificationRequest.ContactId) ||
                string.IsNullOrEmpty(triggerNotificationRequest.CompanyId) ||
                string.IsNullOrEmpty(triggerNotificationRequest.RequestId) ||
                string.IsNullOrEmpty(triggerNotificationRequest.Status) ||
                string.IsNullOrEmpty(triggerNotificationRequest.RequestNumber))
            {
                throw new UserFriendlyException("InvalidRequest", System.Net.HttpStatusCode.BadRequest);
            }
            else
            {
                var response = await _commercializationAppService.TriggerNotification(triggerNotificationRequest);
                return Ok(response);
            }
        }

        [HttpPost]
        [OverrideActionFilters]
        [APIKEY]
        [Route("profile-data")]
        public async Task<IHttpActionResult> GetProfileData(ProfileDataRequest profileDataRequest)
        {
            if (string.IsNullOrEmpty(profileDataRequest.ObjectId))
            {
                throw new UserFriendlyException("InvalidRequest", System.Net.HttpStatusCode.BadRequest);
            }
            if(profileDataRequest.ObjectType == "1")
            {
                _contactAppService = WindsorContainerProvider.Container.Resolve<IContactAppService>();
                var contact = await _contactAppService.GetContactById(profileDataRequest.ObjectId);
                var response = new
                {
                    name = contact.FirstName + " " + contact.LastName,
                    email = contact.Email,
                    phone = contact.MobilePhone,
                    contactId = profileDataRequest.ObjectId
                };

                return Ok(response);
            }

            if(profileDataRequest.ObjectType == "2")
            {
                _performanceDashboardAppService = WindsorContainerProvider.Container.Resolve<IPerformanceDashboardAppService>();
                var company = await _performanceDashboardAppService.GetCompanyNameById(profileDataRequest.ObjectId);
                var response = new
                {
                    companyName = company,
                    companyId = profileDataRequest.ObjectId
                };

                return Ok(response);
            }
            else
            {
                return BadRequest();
            }
        }

        private async Task<List<EsmUploadedDocument>> ExtractFiles(IEnumerable<HttpContent> contents)
        {
            var documents = new List<EsmUploadedDocument>();

            foreach (var content in contents)
            {
                var contentDisposition = content.Headers.ContentDisposition;
                if (contentDisposition != null && contentDisposition.FileName != null)
                {
                    var fileName = contentDisposition.FileName.Trim('"');
                    var fileExtension = Path.GetExtension(fileName);
                    var keyName = contentDisposition.Name.Trim('"');

                    using (var fileStream = await content.ReadAsStreamAsync())
                    using (var memoryStream = new MemoryStream())
                    {
                        await fileStream.CopyToAsync(memoryStream);
                        var fileBytes = memoryStream.ToArray();
                        var fileSize = fileBytes.Length;

                        documents.Add(new EsmUploadedDocument
                        {
                            Name = fileName,
                            KeyName =keyName,
                            Bytes = fileBytes,
                            Extension = fileExtension,
                            Size = fileSize
                        });
                    }
                }
            }

            return documents;
        }
        private void CheckAllowedFileTypes(List<EsmUploadedDocument> documents)
        {
            var allowedFileType = _portalConfigAppService.RetrievePortalConfiguration(new List<string> { PortalConfigurations.AllowedDigitalServiceRequestFileTypes }).FirstOrDefault()?.Value;

            if (!string.IsNullOrEmpty(allowedFileType))
            {
                var allowedExtensions = allowedFileType
                                        .Split(',')
                                        .Where(ext => !string.IsNullOrWhiteSpace(ext))
                                        .Select(ext => ext.Trim().ToLower())
                                        .ToList();

                foreach (var document in documents)
                {
                    if (string.IsNullOrEmpty(document.Extension))
                    {
                        throw new UserFriendlyException($"Document '{document.Name}' does not have a valid extension.");
                    }

                    var extension = document.Extension.Trim().ToLower();

                    if (!allowedExtensions.Contains(extension))
                    {
                        throw new UserFriendlyException($"File type '{extension}' for document '{document.Name}' is not allowed.");
                    }
                }

            }

        }

    }

}
