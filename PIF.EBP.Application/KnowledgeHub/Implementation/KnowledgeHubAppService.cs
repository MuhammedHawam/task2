using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using PIF.EBP.Application.AccessManagement;
using PIF.EBP.Application.Contacts.Dtos;
using PIF.EBP.Application.DocumentLocation;
using PIF.EBP.Application.EntitiesCache.DTOs;
using PIF.EBP.Application.FileScanning;
using PIF.EBP.Application.KnowledgeHub.DTOs;
using PIF.EBP.Application.MetaData.DTOs;
using PIF.EBP.Application.PortalConfiguration;
using PIF.EBP.Application.Shared;
using PIF.EBP.Application.Shared.Helpers;
using PIF.EBP.Core.CRM;
using PIF.EBP.Core.Exceptions;
using PIF.EBP.Core.FileManagement;
using PIF.EBP.Core.FileManagement.DTOs;
using PIF.EBP.Core.Session;
using PIF.EBP.Core.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static PIF.EBP.Application.Shared.Enums;

namespace PIF.EBP.Application.KnowledgeHub.Implementation
{
    public class KnowledgeHubAppService : IKnowledgeHubAppService
    {
        private readonly ICrmService _crmService;
        private readonly ISessionService _sessionService;
        private readonly IPortalConfigAppService _portalConfigAppService;
        private readonly IUserPermissionAppService _userPermissionAppService;
        private readonly IFileManagement _fileService;
        private readonly IDocumentLocationAppService _documentLocationAppService;
        private readonly IFileScanningService _fileScanService;
        private readonly IKnowledgeHubFileUploaderService _knowledgeHubFileUploaderService;
        private readonly List<PortalConfigDto> _configurations;

        public KnowledgeHubAppService(ICrmService crmService, ISessionService sessionService,
                                      IPortalConfigAppService portalConfigAppService,
                                      IUserPermissionAppService userPermissionAppService, IFileManagement fileService,
                                       IDocumentLocationAppService documentLocationAppService,
                                       IFileScanningService fileScanService,
                                       IKnowledgeHubFileUploaderService knowledgeHubFileUploaderService)
        {
            _crmService = crmService;
            _sessionService = sessionService;
            _portalConfigAppService = portalConfigAppService;
            _userPermissionAppService=userPermissionAppService;
            _fileService = fileService;
            _documentLocationAppService=documentLocationAppService;
            _fileScanService=fileScanService;
            _knowledgeHubFileUploaderService=knowledgeHubFileUploaderService;
            _configurations = _portalConfigAppService.RetrievePortalConfiguration(new List<string>
            { PortalConfigurations.KnowledgeItemTypeTemplate, PortalConfigurations.KnowledgeItemTypeArticle, PortalConfigurations.KnowledgeItemTypeAnnouncement,
                PortalConfigurations.KnowledgeItemTypeFAQ, PortalConfigurations.KnowledgeItemTypePlaybook, PortalConfigurations.KnowledgeItemTypeManual,PortalConfigurations.EnableFileScanning
            });
        }

        public async Task<string> AddContent(ContentDto contentDto, bool isPortalCall)
        {
            string companyId = contentDto.CompanyId == Guid.Empty ? _sessionService.GetCompanyId() : contentDto.CompanyId.ToString();
            string contactId = contentDto.ContactId == Guid.Empty ? _sessionService.GetContactId() : contentDto.ContactId.ToString();
            var Entity = new Entity(EntityNames.KnowledgeItem);

            Entity["pwc_name"] = contentDto.Title;
            Entity["pwc_titilearabic"] = contentDto.TitleAr;
            Entity["pwc_description"] = contentDto.Description;
            Entity["pwc_descriptionarabic"] = contentDto.DescriptionAr;
            Entity["pwc_companyid"] = new EntityReference(EntityNames.Account, new Guid(companyId));
            Entity["pwc_contactid"] = new EntityReference(EntityNames.Contact, new Guid(contactId));
            Entity["pwc_knowledgeitemtypeid"] =  new EntityReference(EntityNames.KnowledgeItemType, new Guid(_configurations.SingleOrDefault(a => a.Key == PortalConfigurations.KnowledgeItemTypeArticle).Value));

            var Id = _crmService.Create(Entity, EntityNames.KnowledgeItem);

            if (Id != Guid.Empty && contentDto.Documents != null && contentDto.Documents.Count > 0)
            {

                bool.TryParse(_configurations.SingleOrDefault(a => a.Key == PortalConfigurations.EnableFileScanning).Value, out bool enableFileScanning);
                if (enableFileScanning)
                {
                    
                    foreach (var doc in contentDto.Documents)
                    {
                        var metaData = new
                        {
                            TargetFolderURL = EntityNames.KnowledgeItem,
                            IsPifNotify = "false",
                            CompanyId = companyId,
                            modified_by_contact_id = contactId,
                            created_by_contact_Id = contactId,
                            modified_by_contact_DateTime = DateTime.UtcNow,
                            KnowledgeItemId = Id.ToString(),
                            IsPortalCall = isPortalCall,
                            DocumentExtension = doc.DocumentExtension,
                            DocumentSize = doc.DocumentSize
                        };

                        var response = await _fileScanService.AnalyzeFile(Convert.FromBase64String(doc.DocumentContent), doc.DocumentName, metaData);

                        if (Guid.TryParse(response, out Guid dataId))
                        {
                            return response;
                        }
                    }
                }
                else
                {
                    var metadataDic = new Dictionary<string, object>
                    {
                                        { DocumentMetadata.GetDocumentMetadataByKey("CompanyId")?.FieldName, companyId},
                                        { DocumentMetadata.GetDocumentMetadataByKey("ModifiedBy")?.FieldName, contactId },
                                        { DocumentMetadata.GetDocumentMetadataByKey("ModifiedOn")?.FieldName, DateTime.Now },
                                        { DocumentMetadata.GetDocumentMetadataByKey("CreatedBy")?.FieldName ,contactId},
                                        {"ItemId",Id }
                                    };

                    var uploadDocumentsDto = new UploadDocumentsDto
                    {
                        TargetFolderURL = EntityNames.KnowledgeItem,
                        FileMetadata = metadataDic,
                        KnowledgeItemId=Id,
                        IsPortalCall=isPortalCall,
                        Documents = contentDto.Documents,
                        CompanyId = companyId,
                        ContactId = contactId
                    };
                    await _knowledgeHubFileUploaderService.KnowledgeItemUpload(uploadDocumentsDto);
                }
            }

            return "Knowledge item has been uploaded";
        }

        

        public async Task<KnowledgeItemDto> GetKnowledgeItems(int pageNumber, int pageSize,
            int knowledgeItemType, string searchText = null, int? articleFilter = (int)ArticleCategory.All,
            int? announcementFilter = (int)AnnouncementCategory.All, Guid? contentClassification = null)
        {
            KnowledgeItemDto knowledgeItemDto = new KnowledgeItemDto();

            if (knowledgeItemType == (int)KnowledgeItemType.All || knowledgeItemType == (int)KnowledgeItemType.Article)
            {
                knowledgeItemDto.Articles = await GetArticles(pageNumber, pageSize, searchText, articleFilter, contentClassification);
            }
            if (knowledgeItemType == (int)KnowledgeItemType.All || knowledgeItemType == (int)KnowledgeItemType.Announcement)
            {
                knowledgeItemDto.Announcements = await GetAnnouncements(pageNumber, pageSize, searchText, announcementFilter, contentClassification);
            }
            if (knowledgeItemType == (int)KnowledgeItemType.All || knowledgeItemType == (int)KnowledgeItemType.FAQ)
            {
                knowledgeItemDto.FAQs = await GetFAQs(searchText, contentClassification);
            }
            if (knowledgeItemType == (int)KnowledgeItemType.All || knowledgeItemType == (int)KnowledgeItemType.Template)
            {
                knowledgeItemDto.Templates = await GetTemplates(pageNumber, pageSize, searchText, contentClassification);
            }
            if (knowledgeItemType == (int)KnowledgeItemType.All || knowledgeItemType == (int)KnowledgeItemType.Playbook)
            {
                knowledgeItemDto.Playbooks = await GetPlaybooks(pageNumber, pageSize, searchText, contentClassification);
            }
            if (knowledgeItemType == (int)KnowledgeItemType.All || knowledgeItemType == (int)KnowledgeItemType.UserManual)
            {
                knowledgeItemDto.UserManuals = await GetUserManuals(pageNumber, pageSize, searchText, contentClassification);
            }

            return knowledgeItemDto;
        }

        public async Task<AnnouncementDto> GetAnnouncements(int pageNumber, int pageSize, string searchText = null, int? filter = (int)AnnouncementCategory.All, Guid? contentClassification = null)
        {
            AnnouncementDto announcementDto = new AnnouncementDto();
            List<Announcement> announcements = new List<Announcement>();
            List<Guid> pinnedIds = new List<Guid>();

            var knowledgeItemTypeId = new Guid(_configurations.SingleOrDefault(a => a.Key == PortalConfigurations.KnowledgeItemTypeAnnouncement)?.Value);


            var pinnedItems = (List<Announcement>)await GetPinnedKnowledgeItems((int)PinnedKnowledgeItemType.Announcement, knowledgeItemTypeId);
            if (pinnedItems.Count != 0)
            {
                pinnedIds = pinnedItems.Select(x => x.Id).ToList();
                if (pageNumber == 1)
                {
                    pageSize -= pinnedItems.Count;
                }
            }
            announcements = await GetAnnouncementsList(searchText, 
                (filter == (int)AnnouncementCategory.All ? (int?)null : filter.Value), pageNumber, pageSize, 
                pinnedIds, contentClassification);

            if (pageNumber == 1)
            {
                pinnedItems.ForEach(x => x.IsPin = true);
                announcements.InsertRange(0, pinnedItems);
            }


            announcementDto.UnReadCount = announcements.Where(x => x.IsUnRead).Count();
            announcementDto.ItemCount = await GetKnowledgeItemsCount(knowledgeItemTypeId, KnowledgeItemType.Announcement);
            announcementDto.Announcements = announcements;

            return announcementDto;
        }

        public async Task<string> UpdateAnnouncementReadStatus(AnnouncementReadReq oAnnouncementReadReq)
        {
            List<Tuple<Guid, bool>> knowledgeItemIds = new List<Tuple<Guid, bool>>();
            var companyId = _sessionService.GetCompanyId();
            var contactId = _sessionService.GetContactId();
            if (oAnnouncementReadReq.Id != Guid.Empty && oAnnouncementReadReq.Id != null)
            {
                var orgContext = new OrganizationServiceContext(_crmService.GetInstance());
                IQueryable<Entity> query = orgContext.CreateQuery(EntityNames.ReadReceipt).AsQueryable();
                query = query.Where(x => ((Guid)x["pwc_contactid"]).Equals(contactId) && ((Guid)x["pwc_companyid"]).Equals(companyId) && ((Guid)x["pwc_knowledgeitemid"]).Equals(oAnnouncementReadReq.Id));
                var readReceipts = query.ToList();
                if (!readReceipts.Any())
                {
                    knowledgeItemIds.Add(new Tuple<Guid, bool>(oAnnouncementReadReq.Id.Value, false));
                }
            }
            else
            {
                var requestQuery = new QueryExpression(EntityNames.KnowledgeItem)
                {
                    ColumnSet = new ColumnSet("pwc_knowledgeitemid")
                };

                await ApplyCommonCriteria(requestQuery, KnowledgeItemType.Announcement);

                var entityCollection = _crmService.GetInstance().RetrieveMultiple(requestQuery);

                knowledgeItemIds = entityCollection.Entities.Select(entity =>
                {
                    bool isRead = entity.Contains(EntityNames.ReadReceipt + ".pwc_readreceiptid");
                    return new Tuple<Guid, bool>(entity.Id, isRead);
                }).ToList();
            }
            knowledgeItemIds=knowledgeItemIds.Where(item => !item.Item2).ToList();// Only take items where isRead is false
            if (knowledgeItemIds.Count > 0)
            {
                // Create an ExecuteMultipleRequest to batch create records
                var executeMultipleRequest = new ExecuteMultipleRequest
                {
                    Settings = new ExecuteMultipleSettings
                    {
                        ContinueOnError = false,
                        ReturnResponses = false
                    },
                    Requests = new OrganizationRequestCollection()
                };

                foreach (var (knowledgeItemId, isRead) in knowledgeItemIds)  
                {
                    var entity = new Entity(EntityNames.ReadReceipt)
                    {
                        ["pwc_name"] = "New Read Receipt - Announcement",
                        ["pwc_companyid"] = new EntityReference(EntityNames.Account, new Guid(companyId)),
                        ["pwc_contactid"] = new EntityReference(EntityNames.Contact, new Guid(contactId)),
                        ["pwc_knowledgeitemid"] = new EntityReference(EntityNames.KnowledgeItem, knowledgeItemId)
                    };

                    var createRequest = new CreateRequest { Target = entity };
                    executeMultipleRequest.Requests.Add(createRequest);
                }

                // Execute the batch request
                var executeMultipleResponse = (ExecuteMultipleResponse)_crmService.GetInstance().Execute(executeMultipleRequest);

                if (executeMultipleResponse.IsFaulted)
                {
                    throw new UserFriendlyException("MsgUnexpectedError");
                }
            }

            return "Announcement(s) Read Status has been updated";
        }

        public async Task<List<FAQDto>> GetFAQs(string searchText = null, Guid? contentClassification = null)
        {
            return await GetFAQsList(searchText, contentClassification);
        }

        public async Task<ArticleDto> GetArticles(int pageNumber, int pageSize, string searchText = null, int? filter = (int)ArticleCategory.All, Guid? contentClassification = null)
        {
            ArticleDto articleDto = new ArticleDto();
            List<Article> articles = new List<Article>();
            List<Guid> pinnedIds = new List<Guid>();
            var knowledgeItemTypeId = new Guid(_configurations.SingleOrDefault(a => a.Key == PortalConfigurations.KnowledgeItemTypeArticle)?.Value);

            var pinnedItems = (List<Article>)await GetPinnedKnowledgeItems((int)PinnedKnowledgeItemType.Article, knowledgeItemTypeId);
            if (pinnedItems.Count != 0)
            {
                pinnedIds = pinnedItems.Select(x => x.Id).ToList();
                if (pageNumber == 1)
                {
                    pageSize -= pinnedItems.Count;
                }
            }
            articles = await GetArticlesList(searchText, 
                (filter == (int)ArticleCategory.All ? (int?)null : filter.Value), pageNumber, pageSize,
                pinnedIds, contentClassification);

            if (pageNumber == 1)
            {
                pinnedItems.ForEach(x => x.IsPin = true);
                articles.InsertRange(0, pinnedItems);
            }

            articleDto.ItemCount = await GetKnowledgeItemsCount(knowledgeItemTypeId, KnowledgeItemType.Article);
            articleDto.Articles = articles;

            return articleDto;
        }
        public async Task<Article> RetrieveArticleById(Guid id)
        {
            var query = new QueryExpression(EntityNames.KnowledgeItem)
            {
                ColumnSet = new ColumnSet(
                    "pwc_knowledgeitemid",
                    "pwc_name",
                    "createdon",
                    "pwc_description",
                    "pwc_companyid",
                    "pwc_titilearabic",
                    "pwc_descriptionarabic",
                    "createdby",
                    "ownerid",
                    "pwc_keywords",
                    "pwc_articletype",
                    "pwc_shortdescription",
                    "pwc_shortdescriptionarabic",
                    "pwc_medialinks"
                )
            };

            query.Criteria.AddCondition("pwc_knowledgeitemid", ConditionOperator.Equal, id);

            var entityCollection = _crmService.GetInstance().RetrieveMultiple(query);
            var article = entityCollection.Entities.Select(FillArticleRecord).FirstOrDefault();

            var relativeUrl = _documentLocationAppService.GetDocLocationByRegardingId(article.Id);

            var linkFolderUrl = $"{EntityNames.KnowledgeItem}/{relativeUrl}/Attachment";
            article.ReferenceFiles = await RetrieveDocumentsList(linkFolderUrl);

            var mediaFolderUrl = $"{EntityNames.KnowledgeItem}/{relativeUrl}/Images";
            article.MediaFiles = await RetrieveDocumentsList(mediaFolderUrl);

            var coverFolderUrl = $"{EntityNames.KnowledgeItem}/{relativeUrl}/Cover";
            var cover = await RetrieveDocumentsList(coverFolderUrl);
            if (cover.Any())
            {
                article.CoverImage=cover.FirstOrDefault();
            }

            return article;
        }

        public async Task<TemplateDto> GetTemplates(int pageNumber, int pageSize, string searchText = null, Guid? contentClassification = null)
        {
            TemplateDto templateDto = new TemplateDto();
            List<Template> templates = new List<Template>();
            List<Guid> pinnedIds = new List<Guid>();
            var knowledgeItemTypeId = new Guid(_configurations.SingleOrDefault(a => a.Key == PortalConfigurations.KnowledgeItemTypeTemplate)?.Value);

            var pinnedItems = (List<Template>)await GetPinnedKnowledgeItems((int)PinnedKnowledgeItemType.Template, knowledgeItemTypeId);
            if (pinnedItems.Count != 0)
            {
                pinnedIds = pinnedItems.Select(x => x.Id).ToList();
                if (pageNumber == 1)
                {
                    pageSize -= pinnedItems.Count;
                }
            }
            templates = await GetTemplatesList(searchText, pageNumber, pageSize,
                pinnedIds, contentClassification);

            if (pageNumber == 1)
            {
                pinnedItems.ForEach(x => x.IsPin = true);
                templates.InsertRange(0, pinnedItems);  
            }


            templateDto.ItemCount = await GetKnowledgeItemsCount(knowledgeItemTypeId, KnowledgeItemType.Template);
            templateDto.Templates = templates;

            return templateDto;
        }
        public async Task<PlaybookDto> GetPlaybooks(int pageNumber, int pageSize, string searchText = null, Guid? contentClassification = null)
        {
            PlaybookDto playbookDtoDto = new PlaybookDto();
            List<Playbook> playbooks = new List<Playbook>();
            List<Guid> pinnedIds = new List<Guid>();
            var knowledgeItemTypeId = new Guid(_configurations.SingleOrDefault(a => a.Key == PortalConfigurations.KnowledgeItemTypePlaybook)?.Value);

            var pinnedItems = (List<Playbook>)await GetPinnedKnowledgeItems((int)PinnedKnowledgeItemType.Playbook, knowledgeItemTypeId);
            if (pinnedItems.Count != 0)
            {
                pinnedIds = pinnedItems.Select(x => x.Id).ToList();
                if (pageNumber == 1)
                {
                    pageSize -= pinnedItems.Count;
                }
            }
            playbooks = await GetPlaybooksList(searchText, pageNumber, pageSize,
                pinnedIds, contentClassification);

            if (pageNumber == 1)
            {
                pinnedItems.ForEach(x => x.IsPin = true);
                playbooks.InsertRange(0, pinnedItems);
            }

            playbookDtoDto.ItemCount = await GetKnowledgeItemsCount(knowledgeItemTypeId, KnowledgeItemType.Playbook);
            playbookDtoDto.Playbooks = playbooks;

            return playbookDtoDto;
        }
        public async Task<UserManualDto> GetUserManuals(int pageNumber, int pageSize, string searchText = null, Guid? contentClassification = null)
        {
            UserManualDto userManualDto = new UserManualDto();
            List<UserManual> manuals = new List<UserManual>();
            List<Guid> pinnedIds = new List<Guid>();
            var knowledgeItemTypeId = new Guid(_configurations.SingleOrDefault(a => a.Key == PortalConfigurations.KnowledgeItemTypeManual)?.Value);

            var pinnedItems = (List<UserManual>)await GetPinnedKnowledgeItems((int)PinnedKnowledgeItemType.Mannual, knowledgeItemTypeId);
            if (pinnedItems.Count != 0)
            {
                pinnedIds = pinnedItems.Select(x => x.Id).ToList();
                if (pageNumber == 1)
                {
                    pageSize -= pinnedItems.Count;
                }
            }
            manuals = await GetUserManualsList(searchText, pageNumber, pageSize,
                pinnedIds, contentClassification);

            if (pageNumber == 1)
            {
                pinnedItems.ForEach(x => x.IsPin = true);
                manuals.InsertRange(0, pinnedItems);
            }

            userManualDto.ItemCount = await GetKnowledgeItemsCount(knowledgeItemTypeId, KnowledgeItemType.UserManual);
            userManualDto.UserManuals = manuals;

            return userManualDto;
        }

        

        public async Task<byte[]> DownloadDocument(string sourceFilePath)
        {
            try
            {
                var result = _fileService.DowloadDocument(sourceFilePath);
                return result;
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("File Not Found"))
                {
                    throw new UserFriendlyException("FileNotFound");
                }
                else
                {
                    throw new UserFriendlyException("MsgUnexpectedError");
                }
            }
        }

        private async Task<List<Template>> GetTemplatesList(string searchText, int pageNumber,
            int pageSize, List<Guid> pinnedIds, Guid? contentClassification = null)
        {
            var query = new QueryExpression(EntityNames.KnowledgeItem)
            {
                ColumnSet = new ColumnSet(
                    "pwc_knowledgeitemid",
                    "pwc_name",
                    "createdon",
                    "pwc_shortdescription",
                    "pwc_companyid",
                    "pwc_titilearabic",
                    "pwc_shortdescriptionarabic",
                    "createdby",
                    "pwc_description",
                    "pwc_descriptionarabic",
                    "pwc_contentclassificationid"
                ),
                PageInfo = new PagingInfo
                {
                    PageNumber = pageNumber,
                    Count = pageSize
                }
            };

            if (pinnedIds.Count != 0)
            {
                foreach (var pinnedId in pinnedIds)
                {
                    query.Criteria.AddCondition("pwc_knowledgeitemid", ConditionOperator.NotEqual, pinnedId);
                }
            }
            await ApplyCommonCriteria(query, KnowledgeItemType.Template);

            if (contentClassification != null && contentClassification.Value != Guid.Empty)
            {
                query.Criteria.AddCondition("pwc_contentclassificationid", ConditionOperator.Equal, contentClassification.Value);
            }

            var entityCollection = _crmService.GetInstance().RetrieveMultiple(query);
            var templateTasks = entityCollection.Entities.Select(entity => FillTemplateRecord(entity));
            var _templates = await Task.WhenAll(templateTasks);
            var templates = _templates.ToList();
            if (!string.IsNullOrEmpty(searchText))
            {
                templates = templates.Where(x => (!string.IsNullOrEmpty(x.Title) && x.Title.ToLower().Contains(searchText.ToLower())) || (!string.IsNullOrEmpty(x.Description) && x.Description.ToLower().Contains(searchText.ToLower()))).ToList();
            }

            return templates;
        }
        private async Task<List<Playbook>> GetPlaybooksList(string searchText, int pageNumber,
            int pageSize, List<Guid> pinnedIds, Guid? contentClassification = null)
        {
            var query = new QueryExpression(EntityNames.KnowledgeItem)
            {
                ColumnSet = new ColumnSet(
                    "pwc_knowledgeitemid",
                    "pwc_name",
                    "createdon",
                    "pwc_shortdescription",
                    "pwc_companyid",
                    "pwc_titilearabic",
                    "pwc_shortdescriptionarabic",
                    "createdby",
                    "pwc_description",
                    "pwc_descriptionarabic",
                    "pwc_contentclassificationid"
                ),
                PageInfo = new PagingInfo
                {
                    PageNumber = pageNumber,
                    Count = pageSize
                }
            };

            if (pinnedIds.Count != 0)
            {
                foreach (var pinnedId in pinnedIds)
                {
                    query.Criteria.AddCondition("pwc_knowledgeitemid", ConditionOperator.NotEqual, pinnedId);
                }
            }
            await ApplyCommonCriteria(query, KnowledgeItemType.Playbook);

            if (contentClassification != null && contentClassification.Value != Guid.Empty)
            {
                query.Criteria.AddCondition("pwc_contentclassificationid", ConditionOperator.Equal, contentClassification.Value);
            }

            var entityCollection = _crmService.GetInstance().RetrieveMultiple(query);
            var playbooksTasks = entityCollection.Entities.Select(FillPlaybookRecord);
            var _result = await Task.WhenAll(playbooksTasks);
            var playbooks = _result.ToList();

            if (!string.IsNullOrEmpty(searchText))
            {
                playbooks = playbooks.Where(x => (!string.IsNullOrEmpty(x.Title) && x.Title.ToLower().Contains(searchText.ToLower())) || (!string.IsNullOrEmpty(x.Description) && x.Description.ToLower().Contains(searchText.ToLower()))).ToList();
            }

            return playbooks;
        }
        private async Task<List<UserManual>> GetUserManualsList(string searchText, int pageNumber,
            int pageSize, List<Guid> pinnedIds, Guid? contentClassification = null)
        {
            var query = new QueryExpression(EntityNames.KnowledgeItem)
            {
                ColumnSet = new ColumnSet(
                    "pwc_knowledgeitemid",
                    "pwc_name",
                    "createdon",
                    "pwc_companyid",
                    "pwc_titilearabic",
                    "createdby",
                    "pwc_shortdescription",
                    "pwc_shortdescriptionarabic",
                    "pwc_description",
                    "pwc_descriptionarabic",
                    "pwc_keywords",
                    "pwc_contentclassificationid"
                ),
                PageInfo = new PagingInfo
                {
                    PageNumber = pageNumber,
                    Count = pageSize
                }
            };

            if (pinnedIds.Count != 0)
            {
                foreach (var pinnedId in pinnedIds)
                {
                    query.Criteria.AddCondition("pwc_knowledgeitemid", ConditionOperator.NotEqual, pinnedId);
                }
            }
            await ApplyCommonCriteria(query, KnowledgeItemType.UserManual);

            if (contentClassification != null && contentClassification.Value != Guid.Empty)
            {
                query.Criteria.AddCondition("pwc_contentclassificationid", ConditionOperator.Equal, contentClassification.Value);
            }

            var entityCollection = _crmService.GetInstance().RetrieveMultiple(query);
            var manualsTasks = entityCollection.Entities.Select(FillUserManualRecord);
            var _result = await Task.WhenAll(manualsTasks);
            var manuals = _result.ToList();
            if (!string.IsNullOrEmpty(searchText))
            {
                manuals = manuals.Where(x => (!string.IsNullOrEmpty(x.Title) && x.Title.ToLower().Contains(searchText.ToLower())) ||
                                             (!string.IsNullOrEmpty(x.Description) && x.Description.ToLower().Contains(searchText.ToLower())) ||
                                             (!string.IsNullOrEmpty(x.Keywords) && x.Keywords.ToLower().Contains(searchText.ToLower()))
                                             ).ToList();
            }

            return manuals;
        }

        private async Task<List<Article>> GetArticlesList(string searchText, int? filter, int pageNumber,
            int pageSize, List<Guid> pinnedIds, Guid? contentClassification = null)
        {
            var query = new QueryExpression(EntityNames.KnowledgeItem)
            {
                ColumnSet = new ColumnSet(
                    "pwc_knowledgeitemid",
                    "pwc_name",
                    "createdon",
                    "pwc_description",
                    "pwc_companyid",
                    "pwc_titilearabic",
                    "pwc_descriptionarabic",
                    "createdby",
                    "ownerid",
                    "pwc_keywords",
                    "pwc_articletype",
                    "pwc_shortdescription",
                    "pwc_shortdescriptionarabic",
                    "pwc_medialinks",
                    "pwc_contentclassificationid"
                ),
                PageInfo = new PagingInfo
                {
                    PageNumber = pageNumber,
                    Count = pageSize
                }
            };

            await ApplyCommonCriteria(query, KnowledgeItemType.Article);

            if (filter != null)
            {
                query.Criteria.AddCondition("pwc_articletype", ConditionOperator.Equal, (int)filter);
            }

            if(contentClassification != null && contentClassification.Value != Guid.Empty)
            {
                query.Criteria.AddCondition("pwc_contentclassificationid", ConditionOperator.Equal, contentClassification.Value);
            }

            if (pinnedIds.Count != 0)
            {
                foreach (var pinnedId in pinnedIds)
                {
                    query.Criteria.AddCondition("pwc_knowledgeitemid", ConditionOperator.NotEqual, pinnedId);
                }
            }

            var entityCollection = _crmService.GetInstance().RetrieveMultiple(query);
            var articles = entityCollection.Entities.Select(FillArticleRecord).ToList();

            if (!string.IsNullOrEmpty(searchText))
            {
                articles = articles.Where(x => (!string.IsNullOrEmpty(x.Title) && x.Title.ToLower().Contains(searchText.ToLower())) || (!string.IsNullOrEmpty(x.Description) && x.Description.ToLower().Contains(searchText.ToLower()))).ToList();
            }

            await UpdateArticlesReferenceFiles(articles);

            return articles;
        }

        private async Task UpdateArticlesReferenceFiles(List<Article> articles)
        {
            foreach (var article in articles)
            {
                var relativeUrl = _documentLocationAppService.GetDocLocationByRegardingId(article.Id);
                var linkFolderUrl = $"{EntityNames.KnowledgeItem}/{relativeUrl}/Attachment";
                article.ReferenceFiles = await RetrieveDocumentsList(linkFolderUrl);

                var mediaFolderUrl = $"{EntityNames.KnowledgeItem}/{relativeUrl}/Images";
                article.MediaFiles = await RetrieveDocumentsList(mediaFolderUrl);

                var coverFolderUrl = $"{EntityNames.KnowledgeItem}/{relativeUrl}/Cover";
                var cover = await RetrieveDocumentsList(coverFolderUrl);
                if (cover.Any())
                {
                    article.CoverImage=cover.FirstOrDefault();
                }
            }
        }

        private async Task<List<Announcement>> GetAnnouncementsList(string searchText, int? filter, int pageNumber,
            int pageSize, List<Guid> pinnedIds, Guid? contentClassification = null)
        {
            var query = new QueryExpression(EntityNames.KnowledgeItem)
            {
                ColumnSet = new ColumnSet(
                    "pwc_knowledgeitemid",
                    "pwc_name",
                    "createdon",
                    "pwc_label",
                    "pwc_companyid",
                    "pwc_titilearabic",
                    "pwc_shortdescription",
                    "pwc_shortdescriptionarabic",
                    "pwc_description",
                    "pwc_descriptionarabic",
                    "pwc_contentclassificationid"
                ),
                PageInfo = new PagingInfo
                {
                    PageNumber = pageNumber,
                    Count = pageSize
                }
            };

            await ApplyCommonCriteria(query, KnowledgeItemType.Announcement);

            if (filter != null)
            {
                query.Criteria.AddCondition("pwc_label", ConditionOperator.Equal, (int)filter);
            }

            if (contentClassification != null && contentClassification.Value != Guid.Empty)
            {
                query.Criteria.AddCondition("pwc_contentclassificationid", ConditionOperator.Equal, contentClassification.Value);
            }

            if (pinnedIds.Count != 0)
            {
                foreach (var pinnedId in pinnedIds)
                {
                    query.Criteria.AddCondition("pwc_knowledgeitemid", ConditionOperator.NotEqual, pinnedId);
                }
            }

            var entityCollection = _crmService.GetInstance().RetrieveMultiple(query);
            var announcements = entityCollection.Entities.Select(FillAnnouncementRecord).ToList();

            if (!string.IsNullOrEmpty(searchText))
            {
                announcements = announcements.Where(x => (!string.IsNullOrEmpty(x.Title) && x.Title.ToLower().Contains(searchText.ToLower())) || (!string.IsNullOrEmpty(x.Description) && x.Description.ToLower().Contains(searchText.ToLower()))).ToList();
            }

            return announcements;
        }

        private async Task<List<FAQDto>> GetFAQsList(string searchText, Guid? contentClassification = null)
        {
            var query = new QueryExpression(EntityNames.KnowledgeItem)
            {
                ColumnSet = new ColumnSet(
                    "pwc_knowledgeitemid",
                    "pwc_name",
                    "createdon",
                    "pwc_description",
                    "pwc_query",
                    "pwc_queryarabic",
                    "pwc_responsefaq",
                    "pwc_responsearabicfaq",
                    "pwc_companyid",
                    "pwc_titilearabic",
                    "pwc_descriptionarabic",
                    "pwc_category",
                    "pwc_shortdescription",
                    "pwc_shortdescriptionarabic",
                    "pwc_contentclassificationid"
                )
            };

            await ApplyCommonCriteria(query, KnowledgeItemType.FAQ);

            if (contentClassification != null && contentClassification.Value != Guid.Empty)
            {
                query.Criteria.AddCondition("pwc_contentclassificationid", ConditionOperator.Equal, contentClassification.Value);
            }

            var entityCollection = _crmService.GetInstance().RetrieveMultiple(query);
            var fAQs = entityCollection.Entities.Select(FillFAQRecord).ToList();

            if (!string.IsNullOrEmpty(searchText))
            {
                fAQs = fAQs.Where(x => (!string.IsNullOrEmpty(x.Query) && x.Query.ToLower().Contains(searchText.ToLower())) || (!string.IsNullOrEmpty(x.Response) && x.Response.ToLower().Contains(searchText.ToLower()))).ToList();
            }

            return fAQs;
        }

        private async Task<List<KnowledgeHubDocumentRsp>> RetrieveDocumentsList(string targetURL)
        {
            List<KnowledgeHubDocumentRsp> response = new List<KnowledgeHubDocumentRsp>();

            if (!string.IsNullOrEmpty(targetURL))
            {
                response= _fileService.GetDocumentsByTargetUrl(targetURL).Select(x=>new KnowledgeHubDocumentRsp
                {
                    DocumentCreatedOnInUTC=x.DocumentCreatedOnInUTC,
                    DocumentId=x.DocumentId,
                    DocumentName=x.DocumentName,
                    DocumentType=x.DocumentType,
                    DocumentSizeInBytes=x.DocumentSizeInBytes,
                    DocumentPath=targetURL+"/"+x.DocumentName
                }).ToList();
            }
            return response;
        }

        private async Task<int> GetKnowledgeItemsCount(Guid knowledgeItemTypeId, KnowledgeItemType type)
        {
            var query = new QueryExpression(EntityNames.KnowledgeItem)
            {
                ColumnSet = new ColumnSet(false), 
                Criteria = new FilterExpression
                {
                    Conditions =
                    {
                        new ConditionExpression("pwc_knowledgeitemtypeid", ConditionOperator.Equal, knowledgeItemTypeId),
                        new ConditionExpression("statecode", ConditionOperator.Equal, 0)
                    }
                }
            };

            await ApplyCommonCriteria(query, type);
            var entityCollection = _crmService.GetInstance().RetrieveMultiple(query);

            return entityCollection.Entities.Count;
        }

        

        private FAQDto FillFAQRecord(Entity entity)
        {
            return new FAQDto
            {
                Id = entity.Id,
                Title =  entity.GetValueByAttributeName<string>("pwc_name"),
                TitleAr = entity.GetValueByAttributeName<string>("pwc_titilearabic"),
                Description = entity.GetValueByAttributeName<string>("pwc_shortdescription"),
                DescriptionAr =  entity.GetValueByAttributeName<string>("pwc_shortdescriptionarabic"),
                Created =  entity.GetValueByAttributeName<DateTime>("createdon"),
                Query =  entity.GetValueByAttributeName<string>("pwc_query"),
                QueryAr = entity.GetValueByAttributeName<string>("pwc_queryarabic"),
                Response =  entity.GetValueByAttributeName<string>("pwc_responsefaq"),
                ResponseAr = entity.GetValueByAttributeName<string>("pwc_responsearabicfaq"),
                Category = entity.GetValueByAttributeName<EntityOptionSetDto>("pwc_category"),
                ContentClassification = entity.GetValueByAttributeName<EntityReferenceDto>("pwc_contentclassificationid", "ContentClassification.pwc_namear")
            };
        }

        private Announcement FillAnnouncementRecord(Entity entity)
        {
            return new Announcement
            {
                Id = entity.Id,
                Title = entity.Contains("pwc_name") ? entity.GetValueByAttributeName<string>("pwc_name") : string.Empty,
                TitleAr = entity.Contains("pwc_titilearabic") ? entity.GetValueByAttributeName<string>("pwc_titilearabic") : string.Empty,
                ShortDescription = entity.Contains("pwc_shortdescription") ? entity.GetValueByAttributeName<string>("pwc_shortdescription") : string.Empty,
                ShortDescriptionAr = entity.Contains("pwc_shortdescriptionarabic") ? entity.GetValueByAttributeName<string>("pwc_shortdescriptionarabic") : string.Empty,
                DescriptionAr = entity.Contains("pwc_descriptionarabic") ? entity.GetValueByAttributeName<string>("pwc_descriptionarabic") : string.Empty,
                Description = entity.Contains("pwc_description") ? entity.GetValueByAttributeName<string>("pwc_description") : string.Empty,
                Created = entity.Contains("createdon") ? entity.GetValueByAttributeName<DateTime>("createdon") : DateTime.Now,
                Category = entity.Contains("pwc_label") ? entity.GetValueByAttributeName<EntityOptionSetDto>("pwc_label") : null,
                IsUnRead= !entity.Contains(EntityNames.ReadReceipt + ".pwc_readreceiptid"),
                ContentClassification = entity.GetValueByAttributeName<EntityReferenceDto>("pwc_contentclassificationid", "ContentClassification.pwc_namear")
            };
        }
        private Article FillArticleRecord(Entity entity)
        {
            var mediaLink = entity.GetValueByAttributeName<string>("pwc_medialinks");
            return new Article
            {
                Id = entity.Id,
                Title =  entity.GetValueByAttributeName<string>("pwc_name"),
                TitleAr =  entity.GetValueByAttributeName<string>("pwc_titilearabic"),
                Description =  entity.GetValueByAttributeName<string>("pwc_description"),
                DescriptionAr = entity.GetValueByAttributeName<string>("pwc_descriptionarabic"),
                ShortDescription = entity.GetValueByAttributeName<string>("pwc_shortdescription"),
                ShortDescriptionAr = entity.GetValueByAttributeName<string>("pwc_shortdescriptionarabic"),
                CreatedDate = entity.GetValueByAttributeName<DateTime>("createdon"),
                CreatedBy = entity.GetValueByAttributeName<EntityReferenceDto>("createdby"),
                Keywords =  entity.GetValueByAttributeName<string>("pwc_keywords"),
                Author = entity.GetValueByAttributeName<EntityReferenceDto>("ownerid"),
                ArticleType = entity.GetValueByAttributeName<EntityOptionSetDto>("pwc_articletype"),
                ContentClassification = entity.GetValueByAttributeName<EntityReferenceDto>("pwc_contentclassificationid", "ContentClassification.pwc_namear"),
                MediaLinks=string.IsNullOrEmpty(mediaLink)?new string[] { }: mediaLink.Split(','),
                ReferenceFiles = null,
                CoverImage=null,
                MediaFiles = null,
            };
        }


        private async Task<Template> FillTemplateRecord(Entity entity)
        {
            KnowledgeHubDocumentDto oKnowledgeHubDocumentDto = null;
            var title = entity.GetValueByAttributeName<string>("pwc_name");
            var relativeUrl = _documentLocationAppService.GetDocLocationByRegardingId(entity.Id);
            var libFolderUrl= $"{EntityNames.KnowledgeItem}/{relativeUrl}";
            var docs = await RetrieveDocumentsList(libFolderUrl);
            if (docs.Any())
            {
                oKnowledgeHubDocumentDto= docs.Select(x=>new KnowledgeHubDocumentDto { DocumentName=x.DocumentName,DocumentPath=x.DocumentPath,DocumentType=x.DocumentType}).FirstOrDefault();
            }
            return new Template
            {
                Id = entity.Id,
                Title =  title,
                TitleAr =  entity.GetValueByAttributeName<string>("pwc_titilearabic"),
                ShortDescription =  entity.GetValueByAttributeName<string>("pwc_shortdescription"),
                ShortDescriptionAr =  entity.GetValueByAttributeName<string>("pwc_shortdescriptionarabic"),
                Description = entity.GetValueByAttributeName<string>("pwc_description"),
                DescriptionAr = entity.GetValueByAttributeName<string>("pwc_descriptionarabic"),
                CreatedDate =  entity.GetValueByAttributeName<DateTime>("createdon"),
                CreatedBy =  entity.GetValueByAttributeName<EntityReferenceDto>("createdby"),
                ContentClassification = entity.GetValueByAttributeName<EntityReferenceDto>("pwc_contentclassificationid", "ContentClassification.pwc_namear"),
                TargetFolderURL = $"/{libFolderUrl}",
                DocumentDetails=oKnowledgeHubDocumentDto
            };
        }

        private async Task<Playbook> FillPlaybookRecord(Entity entity)
        {
            KnowledgeHubDocumentDto oKnowledgeHubDocumentDto = null;
            var title = entity.GetValueByAttributeName<string>("pwc_name");
            var relativeUrl = _documentLocationAppService.GetDocLocationByRegardingId(entity.Id);
            var libFolderUrl = $"{EntityNames.KnowledgeItem}/{relativeUrl}";
            var docs = await RetrieveDocumentsList(libFolderUrl);
            if (docs.Any())
            {
                oKnowledgeHubDocumentDto= docs.Select(x => new KnowledgeHubDocumentDto { DocumentName=x.DocumentName, DocumentPath=x.DocumentPath, DocumentType=x.DocumentType }).FirstOrDefault();
            }

            return new Playbook
            {
                Id = entity.Id,
                Title = title,
                TitleAr =  entity.GetValueByAttributeName<string>("pwc_titilearabic"),
                ShortDescription =  entity.GetValueByAttributeName<string>("pwc_shortdescription"),
                ShortDescriptionAr = entity.GetValueByAttributeName<string>("pwc_shortdescriptionarabic"),
                Description = entity.GetValueByAttributeName<string>("pwc_description"),
                DescriptionAr = entity.GetValueByAttributeName<string>("pwc_descriptionarabic"),
                CreatedDate =  entity.GetValueByAttributeName<DateTime>("createdon"),
                CreatedBy =  entity.GetValueByAttributeName<EntityReferenceDto>("createdby"),
                ContentClassification = entity.GetValueByAttributeName<EntityReferenceDto>("pwc_contentclassificationid", "ContentClassification.pwc_namear"),
                TargetFolderURL = $"/{libFolderUrl}",
                DocumentDetails=oKnowledgeHubDocumentDto
            };
        }
        private async Task<UserManual> FillUserManualRecord(Entity entity)
        {
            KnowledgeHubDocumentDto oKnowledgeHubDocumentDto = null;
            var title = entity.GetValueByAttributeName<string>("pwc_name");
            var relativeUrl = _documentLocationAppService.GetDocLocationByRegardingId(entity.Id);
            var libFolderUrl = $"{EntityNames.KnowledgeItem}/{relativeUrl}";
            string targetFolderURL = $"/{libFolderUrl}";

            var docs = await RetrieveDocumentsList(libFolderUrl);
            if (docs.Any())
            {
                oKnowledgeHubDocumentDto= docs.Select(x => new KnowledgeHubDocumentDto { DocumentName=x.DocumentName, DocumentPath=x.DocumentPath, DocumentType=x.DocumentType }).FirstOrDefault();
            }
            return new UserManual
            {
                Id = entity.Id,
                Title = title,
                TitleAr = entity.GetValueByAttributeName<string>("pwc_titilearabic"),
                ShortDescription = entity.GetValueByAttributeName<string>("pwc_shortdescription"),
                ShortDescriptionAr = entity.GetValueByAttributeName<string>("pwc_shortdescriptionarabic"),
                Description = entity.GetValueByAttributeName<string>("pwc_description"),
                DescriptionAr = entity.GetValueByAttributeName<string>("pwc_descriptionarabic"),
                CreatedDate = entity.GetValueByAttributeName<DateTime>("createdon"),
                CreatedBy = entity.GetValueByAttributeName<EntityReferenceDto>("createdby"),
                TargetFolderURL = targetFolderURL,
                DocumentDetails = oKnowledgeHubDocumentDto,
                Keywords =  entity.GetValueByAttributeName<string>("pwc_keywords"),
                ContentClassification = entity.GetValueByAttributeName<EntityReferenceDto>("pwc_contentclassificationid", "ContentClassification.pwc_namear")
            };
        }


        public async Task<bool> PinKnowledgeItem(Guid pinKnowledgeId, bool isPin)
        {
            var orgContext = new OrganizationServiceContext(_crmService.GetInstance());
            var contactId = _sessionService.GetContactId();
            var companyId = _sessionService.GetCompanyId();
            var configs = _portalConfigAppService.RetrievePortalConfiguration(new List<string> { PortalConfigurations.MaxPInContact });
            var pinNumber = Convert.ToInt32(configs.Single(a => a.Key == PortalConfigurations.MaxPInContact).Value);
            var knowledgeItemTypeId = GetKnowledgeItemTypeId(pinKnowledgeId);
            var result = GetPortalPins(contactId, companyId, knowledgeItemTypeId);

            if (result.Count == pinNumber && isPin)
            {
                throw new UserFriendlyException("TheUserShouldBeAbleToPinUpToItems", pinNumber.ToString());
            }

            var oPortalPin = result.FirstOrDefault(x => x.KnowledgeItemId != null && x.KnowledgeItemId.Id == pinKnowledgeId.ToString());
            
            if (isPin && oPortalPin == null)
            {
                Entity PortalPinEntity = new Entity(EntityNames.PortalPinned);

                PortalPinEntity["pwc_name"] = contactId;
                PortalPinEntity["pwc_useridid"] = new EntityReference(EntityNames.Contact, new Guid(contactId));
                PortalPinEntity["pwc_companyidid"] = new EntityReference(EntityNames.Account, new Guid(companyId));
                PortalPinEntity["pwc_contactid"] = new EntityReference(EntityNames.Contact, new Guid(contactId));
                PortalPinEntity["pwc_knowledgeitem"] = new EntityReference(EntityNames.KnowledgeItem, pinKnowledgeId);
                orgContext.AddObject(PortalPinEntity);

                orgContext.SaveChanges();
            }
            else if (!isPin && oPortalPin != null)
            {
                _crmService.Delete(oPortalPin.Id.ToString(), EntityNames.PortalPinned);
            }
            else if (!isPin && oPortalPin == null)
            {
                throw new UserFriendlyException("TheItemIsNotPinned");
            }
            else
            {
                throw new UserFriendlyException("TheItemAlreadyPinned");
            }
            return true;
        }

        private string GetKnowledgeItemTypeId(Guid knowledgeItemId)
        {
            var query = new QueryExpression(EntityNames.KnowledgeItem)
            {
                ColumnSet = new ColumnSet(false),
                LinkEntities =
                {
                    new LinkEntity
                    {
                        LinkFromEntityName = EntityNames.KnowledgeItem,
                        LinkFromAttributeName = "pwc_knowledgeitemtypeid",
                        LinkToEntityName = EntityNames.KnowledgeItemType,
                        LinkToAttributeName = "pwc_masterentityknowledgeitemtypeid",
                        Columns= new ColumnSet("pwc_masterentityknowledgeitemtypeid"),
                        EntityAlias = "Pin"
                    }
                }
            };

            query.Criteria.AddCondition("pwc_knowledgeitemid", ConditionOperator.Equal, knowledgeItemId);

            var entityCollection = _crmService.GetInstance().RetrieveMultiple(query);
            var knowledgeItemTypeId = entityCollection.Entities.Select(item => item.GetAttributeValue<AliasedValue>("Pin.pwc_masterentityknowledgeitemtypeid")).FirstOrDefault();


            return knowledgeItemTypeId.Value.ToString();
        }

        private List<PortalPinDto> GetPortalPins(string contactId, string companyId, string knowledgeItemTypeId)
        {
            var query = new QueryExpression(EntityNames.PortalPinned)
            {
                ColumnSet = new ColumnSet("pwc_useridid", "pwc_companyidid", "pwc_contactid", "pwc_knowledgeitem"), // Select required fields from PortalPin entity
                Criteria =
                {
                    Filters =
                        {
                            new FilterExpression
                            {
                                Conditions =
                                {
                                    new ConditionExpression("pwc_useridid", ConditionOperator.Equal, new Guid(contactId)),
                                    new ConditionExpression("pwc_companyidid", ConditionOperator.Equal, new Guid(companyId))
                                }
                            }
                        }
                },
                LinkEntities =
                {
                    new LinkEntity
                    {
                        LinkFromEntityName = EntityNames.PortalPinned,
                        LinkFromAttributeName = "pwc_knowledgeitem",
                        LinkToEntityName = EntityNames.KnowledgeItem,
                        LinkToAttributeName = "pwc_knowledgeitemid",
                        Columns = new ColumnSet("pwc_knowledgeitemtypeid"),
                        LinkEntities =
                        {
                            new LinkEntity
                            {
                                LinkFromEntityName = EntityNames.KnowledgeItem,
                                LinkFromAttributeName = "pwc_knowledgeitemtypeid",
                                LinkToEntityName = EntityNames.KnowledgeItemType,
                                LinkToAttributeName = "pwc_masterentityknowledgeitemtypeid",
                                Columns = new ColumnSet("pwc_masterentityknowledgeitemtypeid"),
                                EntityAlias = "KnowledgeItemTypeAlias", 
                                LinkCriteria =
                                {
                                    Conditions =
                                    {
                                        new ConditionExpression("pwc_masterentityknowledgeitemtypeid", ConditionOperator.Equal, new Guid(knowledgeItemTypeId))
                                    }
                                }
                            }
                        }
                    }
                }
            };

            var entityCollection = _crmService.GetInstance().RetrieveMultiple(query);

            var portalPinDtos = entityCollection.Entities.Select(entity => new PortalPinDto
            {
                Id = entity.Id,
                UserId = CRMOperations.GetValueByAttributeName<EntityReferenceDto>(entity, "pwc_useridid", ""),
                CompanyId = CRMOperations.GetValueByAttributeName<EntityReferenceDto>(entity, "pwc_companyidid", ""),
                ContactId = CRMOperations.GetValueByAttributeName<EntityReferenceDto>(entity, "pwc_contactid", ""),
                KnowledgeItemId = CRMOperations.GetValueByAttributeName<EntityReferenceDto>(entity, "pwc_knowledgeitem", "")
            }).ToList();

            return portalPinDtos;
        }

        private async Task<object> GetPinnedKnowledgeItems(int type, Guid KnowledgeItemtypeId)
        {
            var companyId = _sessionService.GetCompanyId();
            var contactId = _sessionService.GetContactId();

            var query = new QueryExpression(EntityNames.KnowledgeItem)
            {
                ColumnSet = GetColumnSetForType(type),
                LinkEntities =
                {
                    new LinkEntity
                    {
                        LinkFromEntityName = EntityNames.KnowledgeItem,
                        LinkFromAttributeName = "pwc_knowledgeitemid",
                        LinkToEntityName = EntityNames.PortalPinned,
                        LinkToAttributeName = "pwc_knowledgeitem",
                        Columns= new ColumnSet("pwc_knowledgeitem", "pwc_companyidid"),
                        EntityAlias = "Pin",
                        LinkCriteria =
                        {
                            Conditions =
                            {
                                new ConditionExpression("pwc_companyidid", ConditionOperator.Equal, companyId),
                                new ConditionExpression("pwc_useridid", ConditionOperator.Equal, contactId),
                            }
                        }
                    }
                }
            };

            query.Criteria.AddCondition("pwc_knowledgeitemtypeid", ConditionOperator.Equal, KnowledgeItemtypeId);

            switch (type)
            {
                case (int)PinnedKnowledgeItemType.Announcement:
                    await ApplyCommonCriteria(query, KnowledgeItemType.Announcement);
                    break;

                case (int)PinnedKnowledgeItemType.Article:
                    await ApplyCommonCriteria(query, KnowledgeItemType.Article);
                    break;

                case (int)PinnedKnowledgeItemType.Template:
                    await ApplyCommonCriteria(query, KnowledgeItemType.Template);
                    break;

                case (int)PinnedKnowledgeItemType.Playbook:
                    await ApplyCommonCriteria(query, KnowledgeItemType.Playbook);
                    break;

                case (int)PinnedKnowledgeItemType.Mannual:
                    await ApplyCommonCriteria(query, KnowledgeItemType.UserManual);
                    break;

                default:
                    break;
            }
            
            var entityCollection = _crmService.GetInstance().RetrieveMultiple(query);

            switch (type)
            {
                case (int)PinnedKnowledgeItemType.Announcement:
                    return entityCollection.Entities.Select(item => FillAnnouncementRecord(item)).ToList();

                case (int)PinnedKnowledgeItemType.Article:
                    return entityCollection.Entities.Select(item => FillArticleRecord(item)).ToList();

                case (int)PinnedKnowledgeItemType.Template:
                    var templateTasks = entityCollection.Entities.Select(item => FillTemplateRecord(item));
                    var arrTemplates = await Task.WhenAll(templateTasks);
                    return arrTemplates.ToList();

                case (int)PinnedKnowledgeItemType.Playbook:
                    var playbookTasks = entityCollection.Entities.Select(item => FillPlaybookRecord(item));
                    var arrPlaybooks = await Task.WhenAll(playbookTasks);
                    return arrPlaybooks.ToList();

                case (int)PinnedKnowledgeItemType.Mannual:
                    var manualTasks = entityCollection.Entities.Select(item => FillUserManualRecord(item));
                    var arrManuals = await Task.WhenAll(manualTasks);
                    return arrManuals.ToList();

                default:
                    throw new UserFriendlyException("MsgUnexpectedError");
            }
        }

        private ColumnSet GetColumnSetForType(int type)
        {
            switch (type)
            {
                case (int)PinnedKnowledgeItemType.Announcement:
                    return new ColumnSet(
                        "pwc_knowledgeitemid",
                        "pwc_name",
                        "createdon",
                        "pwc_shortdescription",
                        "pwc_label",
                        "pwc_companyid",
                        "pwc_titilearabic",
                        "pwc_shortdescriptionarabic",
                        "pwc_descriptionarabic",
                        "pwc_description",
                        "pwc_contentclassificationid"
                    );

                case (int)PinnedKnowledgeItemType.Article:
                    return new ColumnSet(
                        "pwc_knowledgeitemid",
                        "pwc_name",
                        "createdon",
                        "pwc_shortdescription",
                        "pwc_companyid",
                        "pwc_titilearabic",
                        "pwc_shortdescriptionarabic",
                        "createdby",
                        "ownerid",
                        "pwc_keywords",
                        "pwc_articletype",
                        "pwc_description",
                        "pwc_descriptionarabic",
                        "pwc_contentclassificationid"
                    );

                case (int)PinnedKnowledgeItemType.Template:
                case (int)PinnedKnowledgeItemType.Playbook:
                case (int)PinnedKnowledgeItemType.Mannual:
                    return new ColumnSet(
                        "pwc_knowledgeitemid",
                        "pwc_name",
                        "createdon",
                        "pwc_shortdescription",
                        "pwc_companyid",
                        "pwc_titilearabic",
                        "pwc_shortdescriptionarabic",
                        "createdby",
                        "pwc_description",
                        "pwc_descriptionarabic",
                        "pwc_keywords",
                        "pwc_contentclassificationid"
                    );

                default:
                    throw new UserFriendlyException("MsgUnexpectedError");
            }
        }

        private async Task ApplyCommonCriteria(QueryExpression query, KnowledgeItemType type)
        {
            var IsBoardMember = await _userPermissionAppService.IsLoggedInUserIsBoardMember();
            if (IsBoardMember)
            {
                LinkEntity linkToContact = new LinkEntity
                {
                    LinkFromEntityName = EntityNames.KnowledgeItem,
                    LinkFromAttributeName = "pwc_knowledgeitemid",
                    LinkToEntityName = "pwc_pwc_knowledgeitem_contact",
                    LinkToAttributeName = "pwc_knowledgeitemid",
                    JoinOperator = JoinOperator.LeftOuter,
                    LinkCriteria = new FilterExpression
                    {
                        FilterOperator = LogicalOperator.And,
                        Conditions =
                                {
                                    new ConditionExpression("contactid", ConditionOperator.Equal, new Guid(_sessionService.GetContactId()))
                                }
                    }
                };

                query.LinkEntities.Add(linkToContact);

            }

            LinkEntity linkToAccount = new LinkEntity
            {
                LinkFromEntityName = EntityNames.KnowledgeItem,
                LinkFromAttributeName = "pwc_knowledgeitemid",
                LinkToEntityName = "pwc_pwc_knowledgeitem_account",
                LinkToAttributeName = "pwc_knowledgeitemid",
                JoinOperator = JoinOperator.LeftOuter,
                EntityAlias = "AccountRelationJoin",
                LinkCriteria = new FilterExpression
                {
                    FilterOperator = LogicalOperator.And,
                    Conditions =
                                    {
                                        new ConditionExpression("accountid", ConditionOperator.Equal, new Guid(_sessionService.GetCompanyId()))
                                    }
                }
            };
            LinkEntity linkToContentClassification = new LinkEntity
            {
                LinkFromEntityName = EntityNames.KnowledgeItem,
                LinkFromAttributeName = "pwc_contentclassificationid",
                LinkToEntityName = EntityNames.KnowledgeContentClassification,
                LinkToAttributeName = "pwc_knowledgecontentclassificationid",
                JoinOperator = JoinOperator.LeftOuter,
                Columns = new ColumnSet ("pwc_knowledgecontentclassificationid", "pwc_name", "pwc_namear"),
                EntityAlias = "ContentClassification",
            };


            FilterExpression mainFilter = new FilterExpression(LogicalOperator.And);
            mainFilter.Conditions.Add(new ConditionExpression("statecode", ConditionOperator.Equal, 0));//Active
            FilterExpression dateFilter = new FilterExpression(LogicalOperator.Or);
            dateFilter.Conditions.Add(new ConditionExpression("pwc_expirydate", ConditionOperator.OnOrAfter, DateTime.Now));
            dateFilter.Conditions.Add(new ConditionExpression("pwc_expirydate", ConditionOperator.Null));

            mainFilter.AddFilter(dateFilter);
            mainFilter.Conditions.Add(new ConditionExpression("pwc_status", ConditionOperator.Equal, 517430000));//Published
            FilterExpression manualFilter = new FilterExpression(LogicalOperator.And);
            manualFilter.Conditions.Add(new ConditionExpression("pwc_visibilityset", ConditionOperator.NotNull));//Manual & Criteria&public
            FilterExpression joinRelationFilter = new FilterExpression(LogicalOperator.Or);
            joinRelationFilter.Conditions.Add(new ConditionExpression("pwc_pwc_knowledgeitem_account", "pwc_knowledgeitemid", ConditionOperator.NotNull));
            if (IsBoardMember)
            {
                joinRelationFilter.Conditions.Add(new ConditionExpression("pwc_pwc_knowledgeitem_contact", "pwc_knowledgeitemid", ConditionOperator.NotNull));
            }

            manualFilter.AddFilter(joinRelationFilter);

            FilterExpression publicFilter = new FilterExpression(LogicalOperator.And);
            publicFilter.Conditions.Add(new ConditionExpression("pwc_visibilityset", ConditionOperator.Equal, 517430002));//public

            FilterExpression faqFilter = new FilterExpression(LogicalOperator.And);
            faqFilter.Conditions.Add(new ConditionExpression("pwc_knowledgeitemtypeid", ConditionOperator.Equal, new Guid(_configurations.SingleOrDefault(a => a.Key == PortalConfigurations.KnowledgeItemTypeFAQ)?.Value)));

            FilterExpression manualorPublicFilter = new FilterExpression(LogicalOperator.Or);
            manualorPublicFilter.AddFilter(manualFilter);
            manualorPublicFilter.AddFilter(publicFilter);
            manualorPublicFilter.AddFilter(faqFilter);

            query.LinkEntities.Add(linkToAccount);
            query.LinkEntities.Add(linkToContentClassification);

            switch (type)
            {
                case KnowledgeItemType.Article:
                    mainFilter.Conditions.Add(new ConditionExpression("pwc_knowledgeitemtypeid", ConditionOperator.Equal, new Guid(_configurations.SingleOrDefault(a => a.Key == PortalConfigurations.KnowledgeItemTypeArticle)?.Value)));
                    break;
                case KnowledgeItemType.Announcement:

                    var readReceiptLink = new LinkEntity
                    {
                        LinkFromEntityName = EntityNames.KnowledgeItem,
                        LinkFromAttributeName = "pwc_knowledgeitemid",
                        LinkToEntityName = EntityNames.ReadReceipt,
                        LinkToAttributeName = "pwc_knowledgeitemid",
                        JoinOperator = JoinOperator.LeftOuter,
                        Columns = new ColumnSet("pwc_readreceiptid"),
                        EntityAlias = EntityNames.ReadReceipt,
                    };
                    var includeFilter = new FilterExpression();
                    includeFilter.AddCondition(new ConditionExpression("pwc_companyid", ConditionOperator.Equal, new Guid(_sessionService.GetCompanyId())));
                    includeFilter.AddCondition(new ConditionExpression("pwc_contactid", ConditionOperator.Equal, new Guid(_sessionService.GetContactId())));
                    readReceiptLink.LinkCriteria = includeFilter;
                    query.LinkEntities.Add(readReceiptLink);

                    mainFilter.Conditions.Add(new ConditionExpression("pwc_knowledgeitemtypeid", ConditionOperator.Equal, new Guid(_configurations.SingleOrDefault(a => a.Key == PortalConfigurations.KnowledgeItemTypeAnnouncement)?.Value)));

                    break;
                case KnowledgeItemType.FAQ:
                    mainFilter.Conditions.Add(new ConditionExpression("pwc_knowledgeitemtypeid", ConditionOperator.Equal, new Guid(_configurations.SingleOrDefault(a => a.Key == PortalConfigurations.KnowledgeItemTypeFAQ)?.Value)));
                    break;
                case KnowledgeItemType.Template:
                    mainFilter.Conditions.Add(new ConditionExpression("pwc_knowledgeitemtypeid", ConditionOperator.Equal, new Guid(_configurations.SingleOrDefault(a => a.Key == PortalConfigurations.KnowledgeItemTypeTemplate)?.Value)));
                    break;
                case KnowledgeItemType.Playbook:
                    mainFilter.Conditions.Add(new ConditionExpression("pwc_knowledgeitemtypeid", ConditionOperator.Equal, new Guid(_configurations.SingleOrDefault(a => a.Key == PortalConfigurations.KnowledgeItemTypePlaybook)?.Value)));
                    break;
                case KnowledgeItemType.UserManual:
                    mainFilter.Conditions.Add(new ConditionExpression("pwc_knowledgeitemtypeid", ConditionOperator.Equal, new Guid(_configurations.SingleOrDefault(a => a.Key == PortalConfigurations.KnowledgeItemTypeManual)?.Value)));
                    break;
                default:
                    break;
            }
            mainFilter.AddFilter(manualorPublicFilter);
            query.Criteria= mainFilter;
        }

        
        
    }
}
