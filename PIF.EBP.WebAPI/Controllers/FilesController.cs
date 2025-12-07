using PIF.EBP.Application.FileScanning;
using PIF.EBP.Application.PortalConfiguration;
using PIF.EBP.Application.Shared;
using PIF.EBP.Application.Sharepoint;
using PIF.EBP.Core.DependencyInjection;
using PIF.EBP.Core.Exceptions;
using PIF.EBP.Core.FileManagement.DTOs;
using PIF.EBP.Core.Session;
using PIF.EBP.WebAPI.Middleware.ActionFilter;
using PIF.EBP.WebAPI.Middleware.Authorize;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace PIF.EBP.WebAPI.Controllers
{
    [RoutePrefix("Files")]
    [ApiResponseWrapper]
    public class FilesController : BaseController
    {
        private readonly IHexaDocumentManagementService _sharepointService;
        private readonly IPortalConfigAppService _portalConfigAppService;
        private readonly IFileScanningService _FileScanService;
        private readonly ISessionService _sessionService;

        public FilesController()
        {
            _sharepointService = WindsorContainerProvider.Container.Resolve<IHexaDocumentManagementService>();
            _portalConfigAppService = WindsorContainerProvider.Container.Resolve<IPortalConfigAppService>();
            _FileScanService = WindsorContainerProvider.Container.Resolve<IFileScanningService>();
            _sessionService = WindsorContainerProvider.Container.Resolve<ISessionService>();
        }

        [HttpPost]
        [Route("upload")]
        public async Task<IHttpActionResult> UploadDocuments()
        {
            if (!Request.Content.IsMimeMultipartContent())
            {
                return StatusCode(HttpStatusCode.UnsupportedMediaType);
            }

            DocumentHeader attachmentHeader = new DocumentHeader();
            var provider = new MultipartMemoryStreamProvider();
            await Request.Content.ReadAsMultipartAsync(provider);
            var configurations = _portalConfigAppService.RetrievePortalConfiguration(new List<string> { PortalConfigurations.EnableFileScanning });

            Guid regardingObjectId = ExtractRegardingObjectId(provider.Contents);
            if (regardingObjectId == Guid.Empty)
            {
                return BadRequest("RegardingObjectId is required");
            }

            var documents = await ExtractFiles(provider.Contents);

            var uploadDocumentsDto = new UploadDocumentsDto
            {
                RegardingObjectId = regardingObjectId,
                Documents = documents
            };

            bool.TryParse(configurations.SingleOrDefault(a => a.Key == PortalConfigurations.EnableFileScanning).Value, out bool enableFileScanning);
            if (enableFileScanning)
            {
                var metaData = new { RegardingObjectId = regardingObjectId.ToString() };

                foreach (UploadedDocDetails uploadedDocDetails in documents)
                {
                    var response = await _FileScanService.AnalyzeFile(Convert.FromBase64String(uploadedDocDetails.DocumentContent), uploadedDocDetails.DocumentName, metaData);
                }
                attachmentHeader = await _sharepointService.RetrieveAttachmentsDetailsAsync(uploadDocumentsDto);
                attachmentHeader.SendForScanning = true;
            }
            else
            {
                attachmentHeader = await _sharepointService.UploadAttachmentsAsync(uploadDocumentsDto);
            }

            return Ok(attachmentHeader);
        }

        [HttpGet]
        [Route("download")]
        public async Task<IHttpActionResult> GetDocument(Guid RegardingObjectId, string DocumentName)
        {
            var getDocuments = new GetDocumentDto
            {
                RegardingObjectId = RegardingObjectId,
                DocumentName = DocumentName
            };

            var result = await _sharepointService.GetAttachmentAsync(getDocuments);
            return Ok(result);
        }

        [HttpDelete]
        [Route("delete")]
        public async Task<IHttpActionResult> DeleteDocuments([FromBody] DeleteDocumentsDto deleteDocumentRqt)
        {
            if (!deleteDocumentRqt.DocumentsName.Any())
                throw new UserFriendlyException("DocumentPathShouldNotBeEmpty");

            var deletedResult = await _sharepointService.DeleteAttachmentsAsync(deleteDocumentRqt);

            return Ok(deletedResult);
        }

        [HttpPost]
        [OverrideActionFilters]
        [APIKEY]
        [Route("copy")]
        public async Task<IHttpActionResult> CopyFiles(CopyFiles files)
        {
            if (files.SourceObjectListId == null || files.DestinationObjectId == null)
                throw new UserFriendlyException("RequiredParameters");

            var deletedResult = await _sharepointService.CopyDocumentFiles(files);
            return Ok(deletedResult);
        }

        [HttpPost]
        [Route("bulk-upload")]
        public async Task<IHttpActionResult> BulkUploadDocuments()
        {
            if (!Request.Content.IsMimeMultipartContent())
            {
                return StatusCode(HttpStatusCode.UnsupportedMediaType);
            }

            BulkUploadDocumentResponse oBulkUploadDocumentResponse = new BulkUploadDocumentResponse();
            var provider = new MultipartMemoryStreamProvider();
            await Request.Content.ReadAsMultipartAsync(provider);

            // Extract form data
            string folderName = ExtractContentByKeyName(provider.Contents, "FolderName");
            string isNotify = ExtractContentByKeyName(provider.Contents, "IsPifNotify");
            var configurations = _portalConfigAppService.RetrievePortalConfiguration(new List<string> { PortalConfigurations.EnableFileScanning });
            // Extract files
            var documents = await ExtractFiles(provider.Contents);
            bool.TryParse(configurations.SingleOrDefault(a => a.Key == PortalConfigurations.EnableFileScanning).Value, out bool enableFileScanning);
            if (enableFileScanning)
            {
                var metaData = new { CompanyId = _sessionService.GetCompanyId(), FolderName= folderName, IsPifNotify= isNotify };

                foreach (UploadedDocDetails uploadedDocDetails in documents)
                {
                    var response = await _FileScanService.AnalyzeFile(Convert.FromBase64String(uploadedDocDetails.DocumentContent), uploadedDocDetails.DocumentName, metaData);
                }
                oBulkUploadDocumentResponse.FileUnderScanning=true;
            }
            else
            {
                var oBulkUploadDocumentDto = new BulkUploadDocumentDto
                {
                    CompanyId = Guid.Empty,
                    FolderName = folderName,
                    IsPifNotify=Convert.ToBoolean(isNotify),
                    Documents = documents
                };

                oBulkUploadDocumentResponse.UploadedDocuments=documents.Select(z=>new DocumentDetails { DocumentName=z.DocumentName,DocumentPath=""}).ToList();
            }

            return Ok(oBulkUploadDocumentResponse);
        }

        private Guid ExtractRegardingObjectId(IEnumerable<HttpContent> contents)
        {
            var content = contents.FirstOrDefault(c => c.Headers.ContentDisposition?.Name.Trim('"') == "RegardingObjectId");
            if (content != null)
            {
                var stringGuid = content.ReadAsStringAsync().Result;
                if (Guid.TryParse(stringGuid, out var parsedGuid))
                {
                    return parsedGuid;
                }
            }
            return Guid.Empty;
        }
        private string ExtractContentByKeyName(IEnumerable<HttpContent> contents,string key)
        {
            var content = contents.FirstOrDefault(c => c.Headers.ContentDisposition?.Name.Trim('"') == key);
            if (content != null)
            {
                var result = content.ReadAsStringAsync().Result;
                return result;
            }
            return string.Empty;
        }

        private async Task<List<UploadedDocDetails>> ExtractFiles(IEnumerable<HttpContent> contents)
        {
            var documents = new List<UploadedDocDetails>();

            foreach (var content in contents)
            {
                var contentDisposition = content.Headers.ContentDisposition;
                if (contentDisposition != null && contentDisposition.FileName != null)
                {
                    var fileName = contentDisposition.FileName.Trim('"');
                    using (var fileStream = await content.ReadAsStreamAsync())
                    using (var memoryStream = new MemoryStream())
                    {
                        await fileStream.CopyToAsync(memoryStream);
                        var fileBytes = memoryStream.ToArray();
                        var base64Content = Convert.ToBase64String(fileBytes);

                        documents.Add(new UploadedDocDetails
                        {
                            DocumentName = fileName,
                            DocumentContent = base64Content
                        });
                    }
                }
            }

            return documents;
        }
    }
}
