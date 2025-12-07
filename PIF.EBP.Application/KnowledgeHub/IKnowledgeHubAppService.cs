using PIF.EBP.Application.KnowledgeHub.DTOs;
using PIF.EBP.Core.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static PIF.EBP.Application.Shared.Enums;

namespace PIF.EBP.Application.KnowledgeHub
{
    public interface IKnowledgeHubAppService : ITransientDependency
    {
        Task<string> AddContent(ContentDto contentDto, bool isPortalCall);
        Task<KnowledgeItemDto> GetKnowledgeItems(int pageNumber, int pageSize,
            int knowledgeItemType, string searchText = null, int? articleFilter = (int)ArticleCategory.All,
            int? announcementFilter = (int)AnnouncementCategory.All, Guid? contentClassification = null);
        Task<AnnouncementDto> GetAnnouncements(int pageNumber, int pageSize, string searchText = null, int? filter = (int)AnnouncementCategory.All, Guid? contentClassification = null);
        Task<string> UpdateAnnouncementReadStatus(AnnouncementReadReq oAnnouncementReadReq);
        Task<List<FAQDto>> GetFAQs(string searchText = null, Guid? contentClassification = null);
        Task<ArticleDto> GetArticles(int pageNumber, int pageSize, string searchText = null, int? filter = (int)ArticleCategory.All, Guid? contentClassification = null);
        Task<TemplateDto> GetTemplates(int pageNumber, int pageSize, string searchText = null, Guid? contentClassification = null);
        Task<bool> PinKnowledgeItem(Guid pinKnowledgeId, bool isPin);
        Task<Article> RetrieveArticleById(Guid id);
        Task<byte[]> DownloadDocument(string sourceFilePath);
    }
}
