using PIF.EBP.Application.FileScanning;
using PIF.EBP.Application.PortalConfiguration;
using PIF.EBP.Application.Settings;
using PIF.EBP.Application.Synergy;
using PIF.EBP.Application.Synergy.DTOs;
using PIF.EBP.Core.DependencyInjection;
using PIF.EBP.Core.FileManagement.DTOs;
using PIF.EBP.Core.Session;
using PIF.EBP.WebAPI.Middleware.ActionFilter;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace PIF.EBP.WebAPI.Controllers
{
    [ApiResponseWrapper]
    [RoutePrefix("Synergy")]
    public class SynergyController : BaseController
    {
        private ISynergyService _synergyService;
        private readonly IPortalConfigAppService _portalConfigAppService;
        private readonly IFileScanningService _fileScanningService;
        private readonly ISessionService _sessionService;
        private readonly IUserProfileAppService _userProfileAppService;
        public SynergyController()
        {
            _synergyService = WindsorContainerProvider.Container
                               .Resolve<ISynergyService>();
            _portalConfigAppService = WindsorContainerProvider.Container.Resolve<IPortalConfigAppService>();
            _fileScanningService = WindsorContainerProvider.Container.Resolve<IFileScanningService>();
            _sessionService = WindsorContainerProvider.Container.Resolve<ISessionService>();
            _userProfileAppService = WindsorContainerProvider.Container.Resolve<IUserProfileAppService>();

        }
        [HttpGet]
        [Route("get-user-profile-data-with-sector")]
        public async Task<IHttpActionResult> RetrieveUserProfileDataAndSector()
        {
            var result = await _synergyService.RetrieveUserProfileDataAndSector();

            return Ok(result);
        }
        [HttpGet]
        [Route("Sectors-list")]
        public async Task<IHttpActionResult> GetSectorsList()
        {

            var serviceResult =  _synergyService.GetSectors();

            return Ok(serviceResult);
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

            string synergyRequestId = ExtractContentByKeyName(provider.Contents, "SynergyRequestId");
            string companyId = ExtractContentByKeyName(provider.Contents, "CompanyId");
            string contactId = ExtractContentByKeyName(provider.Contents, "ContactId");
            string description = ExtractContentByKeyName(provider.Contents, "Description");

            if (string.IsNullOrEmpty(synergyRequestId))
            {
                synergyRequestId = Guid.NewGuid().ToString();
            }

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

            if (!string.IsNullOrEmpty(contactId) && !Guid.TryParse(contactId, out _))
            {
                return BadRequest("ContactId must be a valid GUID");
            }

            var documents = await ExtractFiles(provider.Contents);

            if (documents == null || !documents.Any())
            {
                return BadRequest("No files were uploaded");
            }

            var uploadDocumentsDto = new UploadDocumentsDto
            {
                SynergyRequestId = synergyRequestId,
                CompanyId = companyId,
                ContactId = contactId,
                Description = description,
                Documents = documents
            };

            bool enableFileScanningForSynergy = false;
            bool.TryParse(ConfigurationManager.AppSettings["EnableFileScanningForSynergy"], out enableFileScanningForSynergy);

            SynergyUploadResponseDto response = new SynergyUploadResponseDto
            {
                UploadedFiles = new List<SynergyFileInfo>()
            };

            if (enableFileScanningForSynergy)
            {
                foreach (UploadedDocDetails uploadedDocDetails in documents)
                {
                    var metaData = new
                    {
                        SynergyRequestId = synergyRequestId,
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

                    response.UploadedFiles.Add(new SynergyFileInfo
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
                var uploadResults = await _synergyService.SynergyRequestUpload(uploadDocumentsDto);

                var SPSiteCollectionUri = ConfigurationManager.AppSettings["SPSiteUri_ext"];

                foreach (var result in uploadResults)
                {
                    var fileSize = documents.FirstOrDefault(d => d.DocumentName == result.DocumentName)?.DocumentSize ?? 0;

                    string sharePointUrl = $"{SPSiteCollectionUri}{result.DocumentPath}/{result.DocumentName}";

                    response.UploadedFiles.Add(new SynergyFileInfo
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
        public async Task<IHttpActionResult> DeleteFiles([FromBody] List<DeleteFileRequest> deleteFileRequests)
        {
            if (deleteFileRequests == null || !deleteFileRequests.Any())
            {
                return BadRequest("No files specified for deletion");
            }

            var deleteResults = await _synergyService.DeleteSynergyFiles(deleteFileRequests);

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

            byte[] fileBytes = await _synergyService.DownloadSynergyDocument(sourceFilePath);

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

            byte[] fileBytes = await _synergyService.DownloadSynergyDocument(sourceFilePath);

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