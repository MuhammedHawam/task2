using PIF.EBP.Application.DataCollection;
using PIF.EBP.Application.FileScanning;
using PIF.EBP.Application.PortalConfiguration;
using PIF.EBP.Application.Shared;
using PIF.EBP.Core.DependencyInjection;
using PIF.EBP.Core.Exceptions;
using PIF.EBP.Core.FileManagement.DTOs;
using PIF.EBP.Core.Session;
using PIF.EBP.Core.Utilities;
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
    [RoutePrefix("DataCollection")]
    [ApiResponseWrapper]
    public class DataCollectionController : BaseController
    {
        private readonly IDataCollectionService _dataCollectionService;
        private readonly IPortalConfigAppService _portalConfigAppService;
        private readonly IFileScanningService _FileScanService;
        private readonly ISessionService _sessionService;

        public DataCollectionController()
        {
            _dataCollectionService = WindsorContainerProvider.Container.Resolve<IDataCollectionService>();
            _portalConfigAppService = WindsorContainerProvider.Container.Resolve<IPortalConfigAppService>();
            _FileScanService = WindsorContainerProvider.Container.Resolve<IFileScanningService>();
            _sessionService = WindsorContainerProvider.Container.Resolve<ISessionService>();
        }

        [HttpGet]
        [Route("get-folders")]
        public async Task<IHttpActionResult> GetFolders()
        {
            var result = await _dataCollectionService.GetDataCollectionFolders(_sessionService.GetCompanyId(), _sessionService.GetContactId());
            return Ok(result);
        }

        [HttpGet]
        [OverrideActionFilters]
        [APIKEY]
        [Route("get-folders-by-id")]
        public async Task<IHttpActionResult> GetFoldersById(Guid? companyId, Guid? contactId)
        {
            var result = new FolderStructureRes();
            if (!companyId.HasValue && !contactId.HasValue)
            {
                throw new UserFriendlyException("NullArgument");
            }
            if (companyId.HasValue)
            {
                result = (await _dataCollectionService.GetDataCollectionFolders(companyId.ToString(), string.Empty, isForCrm: true)).FirstOrDefault();
                return Ok(result);
            }
            if (contactId.HasValue)
            {
                result = (await _dataCollectionService.GetDataCollectionFolders(string.Empty, contactId.ToString(), isForCrm: true)).FirstOrDefault();
                return Ok(result);
            }
            return Ok(result);
        }

        [HttpPost]
        [OverrideActionFilters]
        [APIKEY]
        [Route("upload-documents")]
        public async Task<IHttpActionResult> UploadDocuments()
        {
            if (!Request.Content.IsMimeMultipartContent())
            {
                return StatusCode(HttpStatusCode.UnsupportedMediaType);
            }

            List<UploadFilesResponse> uploadFiles = new List<UploadFilesResponse>();
            var provider = new MultipartMemoryStreamProvider();
            await Request.Content.ReadAsMultipartAsync(provider);

            var configurations = _portalConfigAppService.RetrievePortalConfiguration(new List<string> { PortalConfigurations.EnableFileScanning });

            var isNotify = ExtractContentByKeyName(provider.Contents, "IsPifNotify");
            var targetFolderURL = ExtractContentByKeyName(provider.Contents, "TargetFolderURL");
            var contactId = ExtractContentByKeyName(provider.Contents, "ContactId");
            var companyId = ExtractContentByKeyName(provider.Contents, "CompanyId");

            if (string.IsNullOrEmpty(targetFolderURL))
            {
                return BadRequest("TargetFolderURL is required");
            }
            if (string.IsNullOrEmpty(isNotify))
            {
                return BadRequest("IsPifNotify is required");
            }

            var documents = await ExtractFiles(provider.Contents);

            var metaData = new { TargetFolderURL = targetFolderURL, IsPifNotify = isNotify, CompanyId = companyId.ToString(), modified_by_contact_id = contactId.ToString(), created_by_contact_Id = contactId.ToString(), modified_by_contact_DateTime = DateTime.UtcNow };

            bool.TryParse(configurations.SingleOrDefault(a => a.Key == PortalConfigurations.EnableFileScanning).Value, out bool enableFileScanning);
            if (enableFileScanning)
            {
                foreach (UploadedDocDetails uploadedDocDetails in documents)
                {
                    var response = await _FileScanService.AnalyzeFile(Convert.FromBase64String(uploadedDocDetails.DocumentContent), uploadedDocDetails.DocumentName, metaData);

                    UploadFilesResponse uploadFilesResponse = new UploadFilesResponse();

                    if (Guid.TryParse(response, out Guid dataId))
                    {
                        uploadFilesResponse.Uploaded = false;
                        uploadFilesResponse.Status = response;
                        uploadFilesResponse.DocumentName = uploadedDocDetails.DocumentName;
                        uploadFilesResponse.DocumentPath = targetFolderURL;
                    }
                    else
                    {
                        uploadFilesResponse.Uploaded = true;
                        uploadFilesResponse.Status = "File has been sent to scan successfully";
                        uploadFilesResponse.DocumentName = uploadedDocDetails.DocumentName;
                        uploadFilesResponse.DocumentPath = targetFolderURL;
                    }

                    uploadFiles.Add(uploadFilesResponse);
                }
            }
            else
            {
                var metadataDic = new Dictionary<string, object>
                                    {
                                        { DocumentMetadata.GetDocumentMetadataByKey("CompanyId")?.FieldName, companyId},
                                        { DocumentMetadata.GetDocumentMetadataByKey("ModifiedBy")?.FieldName, contactId },
                                        { DocumentMetadata.GetDocumentMetadataByKey("ModifiedOn")?.FieldName, DateTime.Now },
                                        { DocumentMetadata.GetDocumentMetadataByKey("CreatedBy")?.FieldName ,contactId}
                                    };

                bool.TryParse(isNotify, out bool IsNotifyFlag);

                var uploadDocumentsDto = new UploadDocumentsDto
                {
                    TargetFolderURL = targetFolderURL,
                    FileMetadata = metadataDic,
                    Documents = documents,
                    IsPifNotify = IsNotifyFlag,
                    CompanyId = companyId,
                    ContactId = contactId
                };

                uploadFiles = await _dataCollectionService.UploadDocuments(uploadDocumentsDto);
            }

            return Ok(uploadFiles);
        }

        [HttpGet]
        [OverrideActionFilters]
        [APIKEY]
        [Route("download-documents")]
        public async Task<HttpResponseMessage> DownloadDocuments(string sourceFilePath)
        {
            if (string.IsNullOrWhiteSpace(sourceFilePath))
                throw new UserFriendlyException("DocumentPathShouldNotBeEmpty");

            var downloadResponse = await _dataCollectionService.DownloadDocument(sourceFilePath);

            // Create an HttpResponseMessage with the byte array
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(downloadResponse.Item1)
            };

            // Set the media type - adjust this as needed based on your actual file type
            response.Content.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment");
            response.Content.Headers.ContentDisposition.FileName = Path.GetFileName(sourceFilePath);
            response.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(downloadResponse.Item2);

            return response;
        }

        [HttpPost]
        [OverrideActionFilters]
        [APIKEY]
        [Route("get-documents")]
        public async Task<IHttpActionResult> GetDocuments(GetDocumentListDto searchDocumentListDto)
        {
            var result = await _dataCollectionService.GetDocuments(searchDocumentListDto);

            return Ok(result);
        }

        [HttpPost]
        [Route("search")]
        public async Task<IHttpActionResult> SearchDocuments(SearchDocumentListDto searchDocumentListDto)
        {
            var result = await _dataCollectionService.SearchDocuments(searchDocumentListDto);

            return Ok(result);
        }

        [HttpPost]
        [Route("upload")]
        public async Task<IHttpActionResult> Upload()
        {
            try
            {
                if (!Request.Content.IsMimeMultipartContent())
                {
                    return StatusCode(HttpStatusCode.UnsupportedMediaType);
                }

                List<UploadFilesResponse> uploadFiles = new List<UploadFilesResponse>();
                var companyId = _sessionService.GetCompanyId();
                var contactId = _sessionService.GetContactId();
                var provider = new MultipartMemoryStreamProvider();
                await Request.Content.ReadAsMultipartAsync(provider);

                var configurations = _portalConfigAppService.RetrievePortalConfiguration(new List<string> { PortalConfigurations.EnableFileScanning });

                string isNotify = ExtractContentByKeyName(provider.Contents, "IsPifNotify");
                string targetFolderURL = ExtractContentByKeyName(provider.Contents, "TargetFolderURL");

                if (string.IsNullOrEmpty(targetFolderURL))
                {
                    return BadRequest("TargetFolderURL is required");
                }
                if (string.IsNullOrEmpty(isNotify))
                {
                    return BadRequest("IsPifNotify is required");
                }

                var documents = await ExtractFiles(provider.Contents);

                bool.TryParse(configurations.SingleOrDefault(a => a.Key == PortalConfigurations.EnableFileScanning).Value, out bool enableFileScanning);
                if (enableFileScanning)
                {
                    foreach (UploadedDocDetails uploadedDocDetails in documents)
                    {
                        var metaData = new
                        {
                            TargetFolderURL = targetFolderURL,
                            IsPifNotify = isNotify,
                            CompanyId = companyId,
                            modified_by_contact_id = contactId,
                            created_by_contact_Id = contactId,
                            modified_by_contact_DateTime = DateTime.UtcNow,
                            DocumentExtension = uploadedDocDetails.DocumentExtension,
                            DocumentSize = uploadedDocDetails.DocumentSize
                        };

                        var response = await _FileScanService.AnalyzeFile(Convert.FromBase64String(uploadedDocDetails.DocumentContent), uploadedDocDetails.DocumentName, metaData);

                        UploadFilesResponse uploadFilesResponse = new UploadFilesResponse();

                        uploadFilesResponse.Uploaded = true;
                        uploadFilesResponse.Status = "File has been sent to scan successfully";
                        uploadFilesResponse.SentForScanning = true;
                        uploadFilesResponse.DocumentName = uploadedDocDetails.DocumentName;
                        uploadFilesResponse.DocumentPath = targetFolderURL;
                        

                        uploadFiles.Add(uploadFilesResponse);
                    }
                }
                else
                {
                    var metadataDic = new Dictionary<string, object>
                                    {
                                        { DocumentMetadata.GetDocumentMetadataByKey("CompanyId")?.FieldName, companyId},
                                        { DocumentMetadata.GetDocumentMetadataByKey("ModifiedBy")?.FieldName, contactId },
                                        { DocumentMetadata.GetDocumentMetadataByKey("ModifiedOn")?.FieldName, DateTime.Now },
                                        { DocumentMetadata.GetDocumentMetadataByKey("CreatedBy")?.FieldName ,contactId}
                                    };

                    bool.TryParse(isNotify, out bool IsNotifyFlag);

                    var uploadDocumentsDto = new UploadDocumentsDto
                    {
                        TargetFolderURL = targetFolderURL,
                        FileMetadata = metadataDic,
                        Documents = documents,
                        IsPifNotify = IsNotifyFlag,
                        CompanyId = companyId,
                        ContactId = contactId
                    };

                    uploadFiles = await _dataCollectionService.UploadDocuments(uploadDocumentsDto);

                    foreach (var item in uploadFiles)
                    {
                        item.SentForScanning = false;
                    }

                }

                return Ok(uploadFiles);
            }
            catch (Exception)
            {
                throw new UserFriendlyException("MsgUnexpectedError");
            }
        }

        [HttpPost]
        [Route("move")]
        public async Task<IHttpActionResult> MoveDocuments(List<MoveFilesDto> moveFilesDtos)
        {
            try
            {
                var result = await _dataCollectionService.MoveDocuments(moveFilesDtos);

                return Ok(result);
            }
            catch (Exception)
            {
                throw new UserFriendlyException("MsgUnexpectedError");
            }
        }

        [HttpPost]
        [Route("rename")]
        public async Task<IHttpActionResult> RenameDocuments(List<RenameFilesDto> renameFilesDtos)
        {
            try
            {
                var result = await _dataCollectionService.RenameDocuments(renameFilesDtos);

                return Ok(result);
            }
            catch (Exception)
            {
                throw new UserFriendlyException("MsgUnexpectedError");
            }
        }
        [HttpPost]
        [Route("delete")]
        public async Task<IHttpActionResult> DeleteDocuments(List<DeleteFileRequest> deleteFileRequest)
        {
            try
            {
                var result = await _dataCollectionService.DeleteDocuments(deleteFileRequest);

                return Ok(result);
            }
            catch (Exception)
            {
                throw new UserFriendlyException("MsgUnexpectedError");
            }
        }
        [HttpPost]
        [Route("copy")]
        public async Task<IHttpActionResult> CopyDocuments(List<CopyFileDto> copyFileRequest)
        {
            try
            {
                var result = await _dataCollectionService.CopyDocuments(copyFileRequest);

                return Ok(result);
            }
            catch (Exception)
            {
                throw new UserFriendlyException("MsgUnexpectedError");
            }
        }
        [HttpGet]
        [OverrideActionFilters]
        [EBPAuthorize]
        [Route("download")]
        public async Task<HttpResponseMessage> Download(string sourceFilePath)
        {
            if (string.IsNullOrWhiteSpace(sourceFilePath))
                throw new UserFriendlyException("DocumentPathShouldNotBeEmpty");

            var downloadResponse = await _dataCollectionService.DownloadDocument(sourceFilePath);

            // Create an HttpResponseMessage with the byte array
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(downloadResponse.Item1)
            };

            // Set the media type - adjust this as needed based on your actual file type
            response.Content.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment");
            response.Content.Headers.ContentDisposition.FileName =  Path.GetFileName(sourceFilePath);
            response.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(downloadResponse.Item2);

            return response;
        }

        private string ExtractContentByKeyName(IEnumerable<HttpContent> contents, string key)
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
                    var fileExtension = Path.GetExtension(fileName);

                    using (var fileStream = await content.ReadAsStreamAsync())
                    using (var memoryStream = new MemoryStream())
                    {
                        await fileStream.CopyToAsync(memoryStream);
                        var fileBytes = memoryStream.ToArray();
                        var base64Content = Convert.ToBase64String(fileBytes);
                        var fileSize = fileBytes.Length; 

                        documents.Add(new UploadedDocDetails
                        {
                            DocumentName = fileName,
                            DocumentContent = base64Content,
                            DocumentExtension = fileExtension,
                            DocumentSize = fileSize
                        });
                    }
                }
            }

            return documents;
        }
    }
}
