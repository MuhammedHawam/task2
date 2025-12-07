using PIF.EBP.Application.KnowledgeHub;
using PIF.EBP.Application.KnowledgeHub.DTOs;
using PIF.EBP.Core.DependencyInjection;
using PIF.EBP.Core.FileManagement.DTOs;
using PIF.EBP.WebAPI.Middleware.ActionFilter;
using PIF.EBP.WebAPI.Middleware.Authorize;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using static PIF.EBP.Application.Shared.Enums;
using PIF.EBP.Core.Exceptions;

namespace PIF.EBP.WebAPI.Controllers
{
    [ApiResponseWrapper]
    [RoutePrefix("KnowledgeHub")]
    public class KnowledgeHubController : BaseController
    {
        private readonly IKnowledgeHubAppService _knowledgeHubAppService;
        private readonly IKnowledgeHubFileUploaderService _knowledgeHubFileUploaderService;
        public KnowledgeHubController()
        {
            _knowledgeHubAppService = WindsorContainerProvider.Container.Resolve<IKnowledgeHubAppService>();
            _knowledgeHubFileUploaderService = WindsorContainerProvider.Container.Resolve<IKnowledgeHubFileUploaderService>();
        }

        [HttpPost]
        [OverrideActionFilters]
        [APIKEY]
        [Route("add-knowledgehub-content")]
        public async Task<IHttpActionResult> AddKnowledgeHubContent()
        {
            ContentDto contentDto = new ContentDto();

            var provider = new MultipartMemoryStreamProvider();
            await Request.Content.ReadAsMultipartAsync(provider);

            contentDto.Title = string.Empty;
            contentDto.TitleAr = string.Empty;
            contentDto.Description = ExtractContentByKeyName(provider.Contents, "Description");
            contentDto.DescriptionAr = string.Empty;
            contentDto.CompanyId = new Guid(ExtractContentByKeyName(provider.Contents, "CompanyId"));
            contentDto.ContactId = new Guid(ExtractContentByKeyName(provider.Contents, "ContactId"));

            var documents = await ExtractFiles(provider.Contents);
            contentDto.Documents = documents;

            var result = await _knowledgeHubAppService.AddContent(contentDto, false);

            return Ok(result);
        }

        [HttpPost]
        [Route("add-content")]
        public async Task<IHttpActionResult> AddContent()
        {
            ContentDto contentDto = new ContentDto();

            var provider = new MultipartMemoryStreamProvider();
            await Request.Content.ReadAsMultipartAsync(provider);

            contentDto.Title =string.Empty;
            contentDto.TitleAr =string.Empty;
            contentDto.Description = ExtractContentByKeyName(provider.Contents, "Description");
            contentDto.DescriptionAr = string.Empty;

            var documents = await ExtractFiles(provider.Contents);
            contentDto.Documents = documents;

            var result = await _knowledgeHubAppService.AddContent(contentDto, true);

            return Ok(result);
        }

        [HttpPost]
        [Route("update-announcements")]
        public async Task<IHttpActionResult> UpdateAnnouncementReadStatus(AnnouncementReadReq oAnnouncementReadReq)
        {
            var result = await _knowledgeHubAppService.UpdateAnnouncementReadStatus(oAnnouncementReadReq);

            return Ok(result);
        }

        [HttpPost]
        [Route("pin-knowledge-item")]
        public async Task<IHttpActionResult> PinKnowledgeItem(PinKnowledgeItemReq request)
        {
            var result = await _knowledgeHubAppService.PinKnowledgeItem(request.Id, request.IsPin);

            return Ok(result);
        }

        [HttpGet]
        [Route("get-knowledge-items")]
        public async Task<IHttpActionResult> GetKnowledgeItems(int pageNumber, int pageSize, int knowledgeItemType, string searchText = null, int? articleFilter = (int)ArticleCategory.All, int? announcementFilter = (int)AnnouncementCategory.All, Guid? contentClassification = null)
        {
            var result = await _knowledgeHubAppService.GetKnowledgeItems(pageNumber, pageSize, knowledgeItemType, searchText, articleFilter, announcementFilter, contentClassification);

            return Ok(result);
        }
        [HttpGet]
        [Route("get-article-by-id")]
        public async Task<IHttpActionResult> GetArticleById(Guid id)
        {
            var result = await _knowledgeHubAppService.RetrieveArticleById(id);

            return Ok(result);
        }
        [HttpGet]
        [Route("download-base64")]
        public async Task<IHttpActionResult> DownloadBase64(string sourceFilePath)
        {
            if (string.IsNullOrWhiteSpace(sourceFilePath))
                throw new UserFriendlyException("DocumentPathShouldNotBeEmpty");

            byte[] fileBytes = await _knowledgeHubAppService.DownloadDocument(sourceFilePath);

            return Ok(fileBytes);
        }
        [HttpPost]
        [OverrideActionFilters]
        [APIKEY]
        [Route("add-knowledgehub-folders")]
        public async Task<IHttpActionResult> AddKnowledgeHubFolders(Guid knowledgeItemId)
        {
            var result = await _knowledgeHubFileUploaderService.AddKnowledgeHubFolders(knowledgeItemId);

            return Ok(result);
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