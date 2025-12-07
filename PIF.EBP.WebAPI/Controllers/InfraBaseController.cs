using PIF.EBP.Application.FileScanning;
using PIF.EBP.Application.InfraBase;
using PIF.EBP.Application.InfraBase.DTOs;
using PIF.EBP.Application.PortalConfiguration;
using PIF.EBP.Application.Shared;
using PIF.EBP.Core.DependencyInjection;
using PIF.EBP.Core.Exceptions;
using PIF.EBP.Core.FileManagement.DTOs;
using PIF.EBP.Core.Session;
using PIF.EBP.WebAPI.Middleware.ActionFilter;
using PIF.EBP.WebAPI.Middleware.Authorize;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace PIF.EBP.WebAPI.Controllers
{
    [RoutePrefix("InfraBase")]
    [ApiResponseWrapper]
    public class InfraBaseController : BaseController
    {
        private readonly IInfraBaseFileUploaderService _infraBaseFileUploaderService;
        private readonly IPortalConfigAppService _portalConfigAppService;
        private readonly IFileScanningService _fileScanningService;
        private readonly ISessionService _sessionService;

        public InfraBaseController()
        {
            _infraBaseFileUploaderService = WindsorContainerProvider.Container.Resolve<IInfraBaseFileUploaderService>();
            _portalConfigAppService = WindsorContainerProvider.Container.Resolve<IPortalConfigAppService>();
            _fileScanningService = WindsorContainerProvider.Container.Resolve<IFileScanningService>();
            _sessionService = WindsorContainerProvider.Container.Resolve<ISessionService>();
        }

        [HttpPost]
        [Route("upload-request-files")]
        public async Task<IHttpActionResult> UploadRequestFiles()
        {
            if (!Request.Content.IsMimeMultipartContent())
            {
                return StatusCode(HttpStatusCode.UnsupportedMediaType);
            }

            var provider = new MultipartMemoryStreamProvider();
            await Request.Content.ReadAsMultipartAsync(provider);

            // Extract form data
            string referenceId = ExtractContentByKeyName(provider.Contents, "ReferenceId");
            string companyId = ExtractContentByKeyName(provider.Contents, "CompanyId");
            string contactId = ExtractContentByKeyName(provider.Contents, "ContactId");
            string description = ExtractContentByKeyName(provider.Contents, "Description");

            // Validate ReferenceId
            if (string.IsNullOrEmpty(referenceId))
            {
                referenceId = Guid.NewGuid().ToString();
            }

            // Validate CompanyId
            if (string.IsNullOrEmpty(companyId))
            {
                companyId = _sessionService.GetCompanyId();
            }

            if (string.IsNullOrEmpty(companyId))
            {
                return BadRequest("CompanyId is required");
            }

            if (!Guid.TryParse(companyId, out _))
            {
                return BadRequest("CompanyId must be a valid GUID");
            }

            // Validate ContactId if provided
            if (!string.IsNullOrEmpty(contactId) && !Guid.TryParse(contactId, out _))
            {
                return BadRequest("ContactId must be a valid GUID");
            }

            // Extract files
            var documents = await ExtractFiles(provider.Contents);

            if (documents == null || !documents.Any())
            {
                return BadRequest("No files were uploaded");
            }

            // Prepare file metadata
            var uploadDocumentsDto = new UploadDocumentsDto
            {
                ReferenceId = referenceId,
                CompanyId = companyId,
                ContactId = contactId,
                Description = description,
                Documents = documents
            };

            // For InfraBase: Bypass file scanning by default to enable immediate upload
            // File scanning can be optionally enabled via Web.config setting
            bool enableFileScanningForInfraBase = false;
            bool.TryParse(ConfigurationManager.AppSettings["EnableFileScanningForInfraBase"], out enableFileScanningForInfraBase);

            InfraBaseUploadResponseDto response = new InfraBaseUploadResponseDto
            {
                UploadedFiles = new List<InfraBaseFileInfo>()
            };

            if (enableFileScanningForInfraBase)
            {
                // File scanning path - async upload via callback
                foreach (UploadedDocDetails uploadedDocDetails in documents)
                {
                    var metaData = new
                    {
                        ReferenceId = referenceId,
                        CompanyId = companyId,
                        ContactId = contactId,
                        Description = description,
                        DocumentExtension = uploadedDocDetails.DocumentExtension,
                        DocumentSize = uploadedDocDetails.DocumentSize
                    };

                    await _fileScanningService.AnalyzeFile(
                        Convert.FromBase64String(uploadedDocDetails.DocumentContent),
                        uploadedDocDetails.DocumentName,
                        metaData);

                    response.UploadedFiles.Add(new InfraBaseFileInfo
                    {
                        FileName = uploadedDocDetails.DocumentName,
                        FileSize = uploadedDocDetails.DocumentSize,
                        Uploaded = false,
                        Status = "File sent for scanning",
                        UploadedOn = DateTime.UtcNow
                    });
                }

                response.Success = true;
                response.Message = "Files sent for scanning successfully";
            }
            else
            {
                // Direct upload to SharePoint (default for InfraBase)
                var uploadResults = await _infraBaseFileUploaderService.InfraBaseRequestUpload(uploadDocumentsDto);

                var SPSiteCollectionUri = ConfigurationManager.AppSettings["SPSiteUri_ext"];

                foreach (var result in uploadResults)
                {
                    // Get file size from original documents
                    var fileSize = documents.FirstOrDefault(d => d.DocumentName == result.DocumentName)?.DocumentSize ?? 0;

                    // Construct proper SharePoint URL with filename
                    string sharePointUrl = $"{SPSiteCollectionUri}{result.DocumentPath}/{result.DocumentName}";

                    response.UploadedFiles.Add(new InfraBaseFileInfo
                    {
                        FileName = result.DocumentName,
                        FilePath = result.DocumentPath,
                        SharePointUrl = sharePointUrl,
                        FileSize = fileSize,
                        Uploaded = result.Uploaded,
                        Status = result.Status,
                        UploadedOn = DateTime.UtcNow
                    });
                }

                response.Success = uploadResults.All(r => r.Uploaded);
                response.Message = response.Success ? "All files uploaded successfully" : "Some files failed to upload";
            }

            return Ok(response);
        }

        [HttpPost]
        [Route("delete-files")]
        public async Task<IHttpActionResult> DeleteFiles(List<DeleteFileRequest> deleteFileRequests)
        {
            if (deleteFileRequests == null || !deleteFileRequests.Any())
            {
                return BadRequest("No files specified for deletion");
            }

            var deleteResults = await _infraBaseFileUploaderService.DeleteInfraBaseFiles(deleteFileRequests);

            var response = new
            {
                Success = deleteResults.All(r => r.Deleted),
                Message = deleteResults.All(r => r.Deleted) ? "All files deleted successfully" : "Some files failed to delete",
                DeletedFiles = deleteResults.Select(r => new
                {
                    r.DocumentPath,
                    r.Deleted,
                    r.Status
                })
            };

            return Ok(response);
        }

        [HttpGet]
        [Route("download")]
        public async Task<IHttpActionResult> DownloadDocument(string sourceFilePath)
        {
            if (string.IsNullOrWhiteSpace(sourceFilePath))
            {
                return BadRequest("Document path is required");
            }

            byte[] fileBytes = await _infraBaseFileUploaderService.DownloadInfraBaseDocument(sourceFilePath);

            var documentInfo = new DocumentInfo
            {
                DocumentName = System.IO.Path.GetFileName(sourceFilePath),
                DocumentContent = Convert.ToBase64String(fileBytes),
                DocumentPath = sourceFilePath
            };

            return Ok(documentInfo);
        }

        [HttpGet]
        [Route("download-base64")]
        public async Task<IHttpActionResult> DownloadBase64(string sourceFilePath)
        {
            if (string.IsNullOrWhiteSpace(sourceFilePath))
            {
                return BadRequest("Document path is required");
            }

            byte[] fileBytes = await _infraBaseFileUploaderService.DownloadInfraBaseDocument(sourceFilePath);

            return Ok(fileBytes);
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
