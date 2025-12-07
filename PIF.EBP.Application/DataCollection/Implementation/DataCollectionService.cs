using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Newtonsoft.Json;
using PIF.EBP.Application.AccessManagement;
using PIF.EBP.Application.DocumentLocation;
using PIF.EBP.Application.MetaData.DTOs;
using PIF.EBP.Application.Notification;
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
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using static PIF.EBP.Application.Shared.Enums;

namespace PIF.EBP.Application.DataCollection.Implementation
{
    public class DataCollectionService : IDataCollectionService
    {
        private readonly ICrmService _crmService;
        private readonly IFileManagement _fileService;
        private readonly IPortalConfigAppService _portalConfigAppService;
        private readonly IAccessManagementAppService _accessManagementAppService;
        private readonly ISessionService _sessionService;
        private readonly IUserPermissionAppService _userPermissionAppService;
        private readonly INotificationAppService _notificationAppService;
        private readonly IDocumentLocationAppService _documentLocationAppService;

        public DataCollectionService(IFileManagement fileService,
                               ICrmService crmService,
                               IPortalConfigAppService portalConfigAppService,
                               IAccessManagementAppService accessManagementAppService,
                               ISessionService sessionService,
                               IUserPermissionAppService userPermissionAppService,
                               INotificationAppService notificationAppService,
                               IDocumentLocationAppService documentLocationAppService)
        {
            _fileService = fileService;
            _crmService = crmService;
            _portalConfigAppService = portalConfigAppService;
            _accessManagementAppService = accessManagementAppService;
            _sessionService = sessionService;
            _userPermissionAppService = userPermissionAppService;
            _notificationAppService = notificationAppService;
            _documentLocationAppService = documentLocationAppService;

        }

        public async Task<List<FolderStructureRes>> GetDataCollectionFolders(string companyId, string contactId, bool isForCrm = false)
        {
            var configurations = _portalConfigAppService.RetrievePortalConfiguration(new List<string> { PortalConfigurations.DataCollectionCompanyFolder, PortalConfigurations.DataCollectionBoardMemberFolder });
            var oDataCollectionFolderReq = new DataCollectionFolderReq();
            oDataCollectionFolderReq.CompanyLibDisplayName = ConfigurationManager.AppSettings["CompanyFolderName"];
            oDataCollectionFolderReq.CompanyId = companyId;
            oDataCollectionFolderReq.ContactId = contactId;
            oDataCollectionFolderReq.CompanyFolderStructure = configurations.SingleOrDefault(a => a.Key == PortalConfigurations.DataCollectionCompanyFolder)?.Value ?? string.Empty;
            oDataCollectionFolderReq.BoardMemberFolderStructure = configurations.SingleOrDefault(a => a.Key == PortalConfigurations.DataCollectionBoardMemberFolder)?.Value ?? string.Empty;
            oDataCollectionFolderReq.IsBoardMember = isForCrm ? isForCrm : await _userPermissionAppService.IsLoggedInUserIsBoardMember();
            var result = GetDataCollectionFoldersStructure(oDataCollectionFolderReq);
            if (!isForCrm)
            {
                await CheckFolderPermission(result);
            }
            return result;
        }
        public async Task<GetDocumentListRspBase> GetDocuments(GetDocumentListDto searchDocumentListDto)
        {

            if (string.IsNullOrEmpty(searchDocumentListDto.targetFolderURL))
            {
                string companyTargetFolderURL = $"/{EntityNames.Account}/{_documentLocationAppService.GetDocLocationByRegardingId(new Guid(searchDocumentListDto.CompanyId))}";
                string boardMemberTargetFolderURL = $"/{EntityNames.Contact}/{_documentLocationAppService.GetDocLocationByRegardingId(new Guid(searchDocumentListDto.ContactId))}";

                searchDocumentListDto.targetFolderURL = companyTargetFolderURL + "," + boardMemberTargetFolderURL;
            }
            else
            {
                /*var segments = searchDocumentListDto.targetFolderURL.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

                if (segments[0] == EntityNames.Account)
                {
                    searchDocumentListDto.targetFolderURL = $"/{segments[0]}/{_documentLocationAppService.GetDocLocationByRegardingId(new Guid(searchDocumentListDto.CompanyId))}";
                    if (segments.Length > 1)
                    {
                        searchDocumentListDto.targetFolderURL += "/" + segments[1];
                    }
                }
                else if (segments[0] == EntityNames.Contact)
                {
                    searchDocumentListDto.targetFolderURL = $"/{segments[0]}/{_documentLocationAppService.GetDocLocationByRegardingId(new Guid(searchDocumentListDto.ContactId))}";
                    if (segments.Length > 1)
                    {
                        searchDocumentListDto.targetFolderURL += "/" + segments[1];
                    }
                }*/
                searchDocumentListDto.targetFolderURL = $"/pwc_rule/{_documentLocationAppService.GetDocLocationByRegardingId(new Guid(searchDocumentListDto.CompanyId))}";

            }

            searchDocumentListDto.targetFolderURL = searchDocumentListDto.targetFolderURL.Replace(" ", "%20");

            GetDocumentListRspBase response = _fileService.GetDocuments(searchDocumentListDto);

            foreach (var file in response.GetDocumentListRsp)
            {
                Guid.TryParse(file.DocumentCreatedBy, out Guid createdByUser);
                Guid.TryParse(file.DocumentModifiedBy, out Guid modifiedByUser);

                if (createdByUser != Guid.Empty || modifiedByUser != Guid.Empty)
                {
                    FileMetadata fileMetadata = GetUserInfoByFileMetadata(!file.IsExternalModifiedByUser, createdByUser, modifiedByUser);

                    file.DocumentCreatedBy = fileMetadata.CreatedBy;
                    file.DocumentCreatedByAr = fileMetadata.CreatedByAr;
                    file.DocumentModifiedBy = fileMetadata.ModifiedBy;
                    file.DocumentModifiedByAr = fileMetadata.ModifiedByAr;
                }
            }

            if (response != null && response.ItemCount > 0)
            {
                response.GetDocumentListRsp = await CheckFilePermission(response.GetDocumentListRsp);

                return new GetDocumentListRspBase()
                {
                    GetDocumentListRsp = response.GetDocumentListRsp,
                    ItemCount = response.ItemCount,
                    Description = "Documents Retrived Successfully!"
                };
            }
            else
            {
                return new GetDocumentListRspBase()
                {
                    GetDocumentListRsp = response.GetDocumentListRsp,
                    ItemCount = response.ItemCount,
                    Description = "No Document were found on the specified path!"
                };
            }
        }

        public async Task<GetDocumentListRspBase> SearchDocuments(SearchDocumentListDto searchDocumentListDto)
        {
            var contactId = _sessionService.GetContactId();
            var companyId = _sessionService.GetCompanyId();

            if (string.IsNullOrEmpty(searchDocumentListDto.targetFolderURL))
            {
                string companyDocLocation = _documentLocationAppService.GetDocLocationByRegardingId(new Guid(_sessionService.GetCompanyId()));

                if (string.IsNullOrEmpty(companyDocLocation))
                {
                    return new GetDocumentListRspBase()
                    {
                        GetDocumentListRsp = new List<GetDocumentListRsp>(),
                        ItemCount = 0,
                        Description = "No Document were found on the specified path!"
                    };
                }

                var siteName = ConfigurationManager.AppSettings["SPSiteName_ext"];
                var SPRelativeUriPrefix = "/sites/" + siteName;
                string companyPath = $"{SPRelativeUriPrefix}/{EntityNames.Account}/{companyDocLocation}";

                var isExists = _fileService.CheckIfPathExistsInSharePoint_Ext(companyPath);
                if (!isExists)
                {
                    return new GetDocumentListRspBase()
                    {
                        GetDocumentListRsp = new List<GetDocumentListRsp>(),
                        ItemCount = 0,
                        Description = "No Document were found on the specified path!"
                    };
                }

                string companyTargetFolderURL = $"/{EntityNames.Account}/{_documentLocationAppService.GetDocLocationByRegardingId(new Guid(_sessionService.GetCompanyId()))}";
                string boardMemberTargetFolderURL = $"/{EntityNames.Contact}/{_documentLocationAppService.GetDocLocationByRegardingId(new Guid(_sessionService.GetContactId()))}";

                searchDocumentListDto.targetFolderURL = companyTargetFolderURL + "," + boardMemberTargetFolderURL;
            }
            else
            {
                var segments = searchDocumentListDto.targetFolderURL.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

                if (segments[0] == EntityNames.Account)
                {
                    searchDocumentListDto.targetFolderURL = $"/{segments[0]}/{_documentLocationAppService.GetDocLocationByRegardingId(new Guid(_sessionService.GetCompanyId()))}";
                    if (segments.Length > 1)
                    {
                        searchDocumentListDto.targetFolderURL += "/" + segments[1];
                    }
                }
                else if (segments[0] == EntityNames.Contact)
                {
                    searchDocumentListDto.targetFolderURL = $"/{segments[0]}/{_documentLocationAppService.GetDocLocationByRegardingId(new Guid(_sessionService.GetContactId()))}";
                    if (segments.Length > 1)
                    {
                        searchDocumentListDto.targetFolderURL += "/" + segments[1];
                    }
                }
            }

            searchDocumentListDto.targetFolderURL = searchDocumentListDto.targetFolderURL.Replace(" ", "%20");

            GetDocumentListRspBase response = _fileService.SearchDocuments(searchDocumentListDto);

            foreach (var file in response.GetDocumentListRsp)
            {
                Guid.TryParse(file.DocumentCreatedBy, out Guid createdByUser);
                Guid.TryParse(file.DocumentModifiedBy, out Guid modifiedByUser);

                if (createdByUser != Guid.Empty || modifiedByUser != Guid.Empty)
                {
                    FileMetadata fileMetadata = GetUserInfoByFileMetadata(!file.IsExternalModifiedByUser, createdByUser, modifiedByUser);

                    file.DocumentCreatedBy = fileMetadata.CreatedBy;
                    file.DocumentCreatedByAr = fileMetadata.CreatedByAr;
                    file.DocumentModifiedBy = fileMetadata.ModifiedBy;
                    file.DocumentModifiedByAr = fileMetadata.ModifiedByAr;
                }
            }

            if (response != null && response.ItemCount > 0)
            {
                response.GetDocumentListRsp = await CheckFilePermission(response.GetDocumentListRsp);

                return new GetDocumentListRspBase()
                {
                    GetDocumentListRsp = response.GetDocumentListRsp,
                    ItemCount = response.ItemCount,
                    Description = "Documents Retrived Successfully!"
                };
            }
            else
            {
                return new GetDocumentListRspBase()
                {
                    GetDocumentListRsp = response.GetDocumentListRsp,
                    ItemCount = response.ItemCount,
                    Description = "No Document were found on the specified path!"
                };
            }
        }

        public Task<List<UploadFilesResponse>> UploadDocuments(UploadDocumentsDto uploadDocumentsDto)
        {
            try
            {
                List<UploadFilesResponse> uploadFilesResponses = new List<UploadFilesResponse>();
                var configs = _portalConfigAppService.RetrievePortalConfiguration(new List<string> { PortalConfigurations.SharePointUserIdForPortal, PortalConfigurations.SharepointDataCollectionGroupsAccess });

                if (configs == null || configs.Count == 0)
                {
                    throw new Exception("SharePointUserIdIsNotConfigured");
                }

                var segments = uploadDocumentsDto.TargetFolderURL.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

                if (segments[0] == EntityNames.Account)
                {
                    uploadDocumentsDto.TargetFolderURL = $"/{segments[0]}/{_documentLocationAppService.GetDocLocationByRegardingId(new Guid(uploadDocumentsDto.CompanyId))}/{segments[1].Replace(" ", "%20")}";
                }
                else if (segments[0] == EntityNames.Contact)
                {
                    uploadDocumentsDto.TargetFolderURL = $"/{segments[0]}/{_documentLocationAppService.GetDocLocationByRegardingId(new Guid(uploadDocumentsDto.ContactId))}/{segments[1].Replace(" ", "%20")}";
                    uploadDocumentsDto.FileMetadata.Remove("CompanyId");
                }
                else if (segments[0] == EntityNames.KnowledgeItem)
                {
                    uploadDocumentsDto.TargetFolderURL = $"/{EntityNames.KnowledgeItem}/{_documentLocationAppService.GetDocLocationByRegardingId(new Guid(uploadDocumentsDto.FileMetadata["ItemId"].ToString()))}";

                    _fileService.CheckFolderStructure(_documentLocationAppService.GetDocLocationByRegardingId(new Guid(uploadDocumentsDto.FileMetadata["ItemId"].ToString())), EntityNames.KnowledgeItem);

                    uploadDocumentsDto.FileMetadata.Remove("ItemId");
                }

                var loggedInUserId = new Guid(configs.FirstOrDefault().Value);
                string strUserDomainName = GetUserDomainName(loggedInUserId);

                foreach (var document in uploadDocumentsDto.Documents)
                {
                    CheckUploadDocumentValidations(document, uploadDocumentsDto.Documents.Count);
                    var uploadFilesResponse = _fileService.UploadFiles(uploadDocumentsDto.TargetFolderURL, document.DocumentName, document.DocumentContent, strUserDomainName, uploadDocumentsDto.FileMetadata);
                    var sharepointGroups = configs.SingleOrDefault(a => a.Key == PortalConfigurations.SharepointDataCollectionGroupsAccess)?.Value;
                    if (!string.IsNullOrEmpty(sharepointGroups) && uploadFilesResponse.Uploaded)
                    {
                        ShareFileWithGroups(uploadDocumentsDto.TargetFolderURL, sharepointGroups);
                    }
                    uploadFilesResponses.Add(uploadFilesResponse);
                }

                if (uploadDocumentsDto.Documents.Any() && uploadDocumentsDto.FileMetadata.Any() && uploadDocumentsDto.FileMetadata.ContainsKey(("IsPifNotify")))
                {
                    if (uploadDocumentsDto.IsPifNotify)
                    {
                        var SPSiteCollectionUri = ConfigurationManager.AppSettings["SPSiteUri_ext"];
                        var sharePointUrl = uploadDocumentsDto.Documents.Count == 1 ? $"{uploadDocumentsDto.TargetFolderURL}/{uploadDocumentsDto.Documents.First().DocumentName}" : uploadDocumentsDto.TargetFolderURL;
                        NotifyPRM($"{SPSiteCollectionUri}{sharePointUrl}", uploadDocumentsDto.CompanyId, uploadDocumentsDto.ContactId);
                    }

                }
                return Task.FromResult(uploadFilesResponses);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<List<RenameFilesResponse>> RenameDocuments(List<RenameFilesDto> renameFilesDtos)
        {
            try
            {
                List<RenameFilesResponse> response = _fileService.RenameDocuments(renameFilesDtos, new Guid(_sessionService.GetContactId()));

                return response;
            }
            catch (Exception)
            {
                throw new UserFriendlyException("MsgUnexpectedError");
            }
        }

        public async Task<List<MoveFilesResponse>> MoveDocuments(List<MoveFilesDto> moveFilesDtos)
        {
            try
            {
                List<MoveFilesResponse> response = _fileService.MoveDocuments(moveFilesDtos, new Guid(_sessionService.GetContactId()));

                return response;
            }
            catch (Exception)
            {
                throw new UserFriendlyException("MsgUnexpectedError");
            }
        }

        public async Task<List<DeleteFileResponse>> DeleteDocuments(List<DeleteFileRequest> deleteFileRequest)
        {
            try
            {
                List<DeleteFileResponse> response = _fileService.DeleteDocument(deleteFileRequest);

                if (response.Any(x => x.Status.Equals("Success")))
                {
                    await NotifyUserOfDeletionDocs();
                }

                return response;
            }
            catch (Exception)
            {
                throw new UserFriendlyException("MsgUnexpectedError");
            }
        }

        public async Task NotifyUserOfDeletionDocs()
        {
            var entities = new List<Entity>();
            var companyId = _sessionService.GetCompanyId();
            var roleColumns = new string[] { "hexa_portalroleid" };
            var portalRoleEntity = _crmService.GetAll(EntityNames.PortalRole, roleColumns, "PC Admin", "hexa_name");
            var pcAdminRoleId = portalRoleEntity.Entities.Select(x => x.Id).FirstOrDefault();

            var query = new QueryExpression(EntityNames.ContactAssociation)
            {
                ColumnSet = new ColumnSet(
                "hexa_contactid",
                "hexa_portalroleid"
                )
            };

            query.Criteria.AddCondition("hexa_companyid", ConditionOperator.Equal, Guid.Parse(companyId));
            query.Criteria.AddCondition("hexa_portalroleid", ConditionOperator.Equal, pcAdminRoleId);

            var entityCollection = _crmService.GetInstance().RetrieveMultiple(query);
            var desiredContactIds = entityCollection.Entities.Select(x =>
            {
                var contactRef = CRMOperations.GetValueByAttributeName<EntityReferenceDto>(x, "hexa_contactid");
                return contactRef != null ? new Guid(contactRef.Id) : Guid.Empty;
            })
            .Where(guid => guid != Guid.Empty)
            .ToList();

            foreach (var contactId in desiredContactIds)
            {
                entities.Add(new Entity(EntityNames.Notifications)
                {
                    ["pwc_companyid"] = new EntityReference(EntityNames.Account, new Guid(companyId)),
                    ["pwc_contactid"] = new EntityReference(EntityNames.Contact, contactId),
                    ["pwc_typetypecode"] = new OptionSetValue((int)PortalNotificationType.FileDeletion),
                    ["pwc_descriptionen"] = "There are some documents have been deleted",
                    ["pwc_descriptionar"] = "تم ازالة بعض الملفات",
                    ["pwc_readstatustypecode"] = new OptionSetValue((int)NotificationReadStatus.Unread),
                    ["pwc_name"] = "Document deletion",
                    ["pwc_namear"] = "ازالة الملفات",
                });
            }
            _notificationAppService.AddNotification(entities);
        }
        public async Task<List<CopyFileResponse>> CopyDocuments(List<CopyFileDto> copyFileRequest)
        {
            try
            {
                List<CopyFileResponse> response = _fileService.CopyDocuments(copyFileRequest, new Guid(_sessionService.GetContactId()));

                return response;
            }
            catch (Exception)
            {
                throw new UserFriendlyException("MsgUnexpectedError");
            }
        }
        public async Task<(byte[], string)> DownloadDocument(string sourceFilePath)
        {
            try
            {
                var result = _fileService.DowloadDocument(sourceFilePath);
                var fileType = _fileService.GetFileTypeByFileUrl(sourceFilePath);
                if (result.Any())
                {
                    var companyId = _sessionService.GetCompanyId();
                    var contactId = _sessionService.GetContactId();
                    var SPSiteCollectionUri = ConfigurationManager.AppSettings["SPSiteUri_ext"];
                    var oEntity = new Entity(EntityNames.DocumentHistory);

                    oEntity["pwc_name"] = "Download History from API";
                    oEntity["pwc_companyid"] = new EntityReference(EntityNames.Account, new Guid(companyId));
                    oEntity["pwc_contactid"] = new EntityReference(EntityNames.Contact, new Guid(contactId));
                    oEntity["pwc_sharepointpath"] = $"{SPSiteCollectionUri}{sourceFilePath}";
                    _crmService.Create(oEntity, EntityNames.DocumentHistory);
                }
                return (result, fileType);
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("File Not Found"))
                {
                    throw new Exception("File Not Found");
                }
                else
                {
                    throw new Exception(ex.Message);
                }
            }
        }

        public string GetUserDomainName(Guid userId)
        {
            string domainName = "";
            if (userId != Guid.Empty)
            {
                var columns = new ColumnSet("domainname");
                var userEntity = _crmService.GetInstance().Retrieve("systemuser", userId, columns);
                if (userEntity != null && userEntity.Id != Guid.Empty)
                {
                    domainName = userEntity.GetAttributeValue<string>("domainname");
                }
                else
                    throw new UserFriendlyException("UserDoesNotExist");
            }
            return domainName;
        }

        public void CheckUploadDocumentValidations(UploadedDocDetails uploadAttachment, int countOfDocuments)
        {
            var configurations = _portalConfigAppService.RetrievePortalConfiguration(new List<string> { PortalConfigurations.MaxFileSize, PortalConfigurations.MaxNumberOfUploadedFiles, PortalConfigurations.AllowedFilesPageFilesTypes });

            if (string.IsNullOrWhiteSpace(uploadAttachment.DocumentName) || string.IsNullOrWhiteSpace(uploadAttachment.DocumentContent))
                throw new UserFriendlyException("AttachmentInformationShouldNotBeEmpty");

            if (!IsFileTypeInAllowedList(uploadAttachment.DocumentExtension, configurations.SingleOrDefault(a => a.Key == PortalConfigurations.AllowedFilesPageFilesTypes)?.Value ?? string.Empty))
                throw new UserFriendlyException("UploadedFileTypeIsNotAllowed");

            if (!IsFileSizeInAllowedLimit(uploadAttachment.DocumentSize, configurations.SingleOrDefault(a => a.Key == PortalConfigurations.MaxFileSize)?.Value ?? string.Empty))
                throw new UserFriendlyException($"UploadedFileIsExceededTheMaximumAllowedSizeOf " +
                    $"{configurations.SingleOrDefault(a => a.Key == PortalConfigurations.MaxFileSize)?.Value ?? string.Empty}MB");

            if (!IsNumberOfUploadedFilesAllowed(countOfDocuments, configurations.SingleOrDefault(a => a.Key == PortalConfigurations.MaxNumberOfUploadedFiles)?.Value ?? string.Empty))
                throw new UserFriendlyException("ExceededMaxNumberOfUploadedFiles");
        }

        private bool IsFileSizeInAllowedLimit(long documentSize, string configuration)
        {
            if (!string.IsNullOrEmpty(configuration))
            {
                if ((documentSize / (1024.0 * 1024.0)) <= long.Parse(configuration))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            return true;
        }

        private bool IsFileTypeInAllowedList(string documentExtension, string configuration)
        {
            if (!string.IsNullOrEmpty(configuration))
            {
                if (configuration.ToLower().Contains(documentExtension.ToLower()))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            return true;
        }

        private bool IsNumberOfUploadedFilesAllowed(int countOfDocuments, string configuration)
        {
            if (!string.IsNullOrEmpty(configuration))
            {
                if (countOfDocuments <= long.Parse(configuration))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            return true;
        }

        private async Task CheckFolderPermission(List<FolderStructureRes> oFolderStructure)
        {
            var userPermissions = await _accessManagementAppService.GetAuthorizedPermissions();
            var filesPermissions = userPermissions.Where(x => x.PageLink == PageRoute.Files || x.PageLink.Contains(PageRoute.Files)).ToList();
            foreach (var folder in oFolderStructure)
            {
                foreach (var subfolder in folder.SubFolders)
                {
                    var subFolderPermissions = subfolder.PermissionName.Split(',').Select(s => s.Trim()).Where(s => !string.IsNullOrEmpty(s)).ToArray();
                    var filesPermission = filesPermissions.FirstOrDefault(z => subFolderPermissions.Contains(z.Name));
                    if (filesPermission != null)
                    {
                        if (filesPermission.Create == (int)AccessLevel.Basic || filesPermission.Create == (int)AccessLevel.Deep)
                        {
                            subfolder.Operations.CanUpload = true;
                            subfolder.Operations.CanCopy = true;
                        }
                        if (filesPermission.Write == (int)AccessLevel.Basic || filesPermission.Write == (int)AccessLevel.Deep)
                        {
                            subfolder.Operations.CanRename = true;
                            subfolder.Operations.CanMove = true;
                        }
                        if (filesPermission.Delete == (int)AccessLevel.Basic || filesPermission.Delete == (int)AccessLevel.Deep)
                        {
                            subfolder.Operations.CanDelete = true;
                        }
                        if (filesPermission.Read == (int)AccessLevel.Basic || filesPermission.Read == (int)AccessLevel.Deep)
                        {
                            subfolder.Operations.CanDownload = true;
                        }
                    }

                }
            }
        }

        private async Task<List<GetDocumentListRsp>> CheckFilePermission(List<GetDocumentListRsp> getDocumentListRsps)
        {

            var configurations = _portalConfigAppService.RetrievePortalConfiguration(new List<string> { PortalConfigurations.DataCollectionCompanyFolder, PortalConfigurations.DataCollectionBoardMemberFolder });

            var companyFolderStructure = configurations.SingleOrDefault(a => a.Key == PortalConfigurations.DataCollectionCompanyFolder)?.Value ?? string.Empty;
            var companyFolderConfigs = JsonConvert.DeserializeObject<FolderAndSubFolderConfig>(companyFolderStructure);

            var boardMemberFolderStructure = configurations.SingleOrDefault(a => a.Key == PortalConfigurations.DataCollectionBoardMemberFolder)?.Value ?? string.Empty;
            var boardMemberFolderConfigs = JsonConvert.DeserializeObject<FolderAndSubFolderConfig>(boardMemberFolderStructure);

            var userPermissions = await _accessManagementAppService.GetAuthorizedPermissions();
            var filesPermissions = userPermissions.Where(x => x.PageLink.Contains(PageRoute.Files));

            foreach (var file in getDocumentListRsps)
            {
                string filePermissionName = string.Empty;

                if (file.DocumentPath.Contains(EntityNames.Account))
                {
                    filePermissionName = companyFolderConfigs.SubFolders
                        .Where(x => x.Id == int.Parse(file.SubFolderId))
                        .Select(x => x.PermissionName)
                        .FirstOrDefault();
                }
                else
                {
                    filePermissionName = boardMemberFolderConfigs.SubFolders
                        .Where(x => x.Id == int.Parse(file.SubFolderId))
                        .Select(x => x.PermissionName)
                        .FirstOrDefault();
                }

                var filesPermission = filesPermissions.FirstOrDefault(z => z.Name == filePermissionName);
                if (filesPermission != null)
                {
                    if (filesPermission.Create == (int)AccessLevel.Basic || filesPermission.Create == (int)AccessLevel.Deep)
                    {
                        file.Operations.CanUpload = true;
                        file.Operations.CanCopy = true;
                    }
                    if (filesPermission.Write == (int)AccessLevel.Basic || filesPermission.Write == (int)AccessLevel.Deep)
                    {
                        file.Operations.CanRename = true;
                        file.Operations.CanMove = true;
                    }
                    if (filesPermission.Delete == (int)AccessLevel.Basic || filesPermission.Delete == (int)AccessLevel.Deep)
                    {
                        file.Operations.CanDelete = true;
                    }
                    if (filesPermission.Read == (int)AccessLevel.Basic || filesPermission.Read == (int)AccessLevel.Deep)
                    {
                        file.Operations.CanDownload = true;
                    }
                }
            }


            return getDocumentListRsps;
        }

        private void NotifyPRM(string sharePointUrl, string companyId, string contactId)
        {
            OrganizationRequest NotifyPRM = new OrganizationRequest("pwc_EBPActionNotifyPRM");

            NotifyPRM["Contact"] = new EntityReference(EntityNames.Contact, new Guid(contactId));
            NotifyPRM["Company"] = new EntityReference(EntityNames.Account, new Guid(companyId));
            NotifyPRM["SharePointURL"] = sharePointUrl;
            var orgService = _crmService.GetInstance();
            orgService.Execute(NotifyPRM);
        }

        private List<FolderStructureRes> GetDataCollectionFoldersStructure(DataCollectionFolderReq oDataCollectionFolderReq)
        {
            List<FolderStructureRes> response = new List<FolderStructureRes>();
            try
            {
                var siteName = ConfigurationManager.AppSettings["SPSiteName_ext"];
                var SPRelativeUriPrefix = "sites/" + siteName;

                if (string.IsNullOrEmpty(oDataCollectionFolderReq.CompanyFolderStructure))
                {
                    throw new UserFriendlyException("There is no portal configuration record for the company folder.");
                }
                if (string.IsNullOrEmpty(oDataCollectionFolderReq.BoardMemberFolderStructure))
                {
                    throw new UserFriendlyException("There is no portal configuration record for the board member folder.");
                }

                if (!string.IsNullOrEmpty(oDataCollectionFolderReq.CompanyId))
                {
                    CreateMainFolders(oDataCollectionFolderReq, SPRelativeUriPrefix);
                    var companyFolderConfigs = JsonConvert.DeserializeObject<FolderAndSubFolderConfig>(oDataCollectionFolderReq.CompanyFolderStructure);
                    FolderStructureRes oCompanyFolder = new FolderStructureRes();
                    oCompanyFolder.Id = companyFolderConfigs.Id;
                    oCompanyFolder.Path = EntityNames.Account;
                    oCompanyFolder.Name = companyFolderConfigs.Name;
                    oCompanyFolder.NameAr = companyFolderConfigs.NameAr;
                    oCompanyFolder.DisplayName = companyFolderConfigs.DisplayName;
                    oCompanyFolder.DisplayNameAr = companyFolderConfigs.DisplayNameAr;

                    CreateCompanyFolderStructure(oDataCollectionFolderReq, SPRelativeUriPrefix, oCompanyFolder, companyFolderConfigs.SubFolders);
                    oCompanyFolder.SubFolders = companyFolderConfigs.SubFolders.Select(z => new FolderStructure { Id = z.Id, Name = z.Name, NameAr = z.NameAr, Order = z.Order, PermissionName = z.PermissionName, DisplayName = z.DisplayName, DisplayNameAr = z.DisplayNameAr }).ToList();
                    response.Add(oCompanyFolder);
                }
                if (!string.IsNullOrEmpty(oDataCollectionFolderReq.ContactId) && oDataCollectionFolderReq.IsBoardMember)
                {
                    var boardMemberFolderConfigs = JsonConvert.DeserializeObject<FolderAndSubFolderConfig>(oDataCollectionFolderReq.BoardMemberFolderStructure);
                    FolderStructureRes oBoardMemberFolder = new FolderStructureRes();
                    oBoardMemberFolder.Id = boardMemberFolderConfigs.Id;
                    oBoardMemberFolder.Path = EntityNames.Contact;
                    oBoardMemberFolder.Name = boardMemberFolderConfigs.Name;
                    oBoardMemberFolder.NameAr = boardMemberFolderConfigs.NameAr;
                    oBoardMemberFolder.DisplayName = boardMemberFolderConfigs.DisplayName;
                    oBoardMemberFolder.DisplayNameAr = boardMemberFolderConfigs.DisplayNameAr;


                    CreateBoardMemberFolderStructure(oDataCollectionFolderReq, SPRelativeUriPrefix, oBoardMemberFolder, boardMemberFolderConfigs.SubFolders);
                    oBoardMemberFolder.SubFolders = boardMemberFolderConfigs.SubFolders.Select(z => new FolderStructure { Id = z.Id, Name = z.Name, NameAr = z.NameAr, Order = z.Order, PermissionName = z.PermissionName, DisplayName = z.DisplayName, DisplayNameAr = z.DisplayNameAr }).ToList();
                    response.Add(oBoardMemberFolder);
                }

                return response;

            }
            catch (Exception)
            {

                return response;
            }
        }
        private void CreateMainFolders(DataCollectionFolderReq oDataCollectionFolderReq, string sPRelativeUriPrefix)
        {

            _fileService.EnsureAllDataCollCustomFieldExists(oDataCollectionFolderReq.CompanyLibDisplayName, EntityNames.Contact);
        }

        private void CreateCompanyFolderStructure(DataCollectionFolderReq oDataCollectionFolderReq, string SPRelativeUriPrefix, FolderStructureRes oCompanyFolder, List<SubFolderConfig> companyFolderConfigs)
        {
            var configs = _portalConfigAppService.RetrievePortalConfiguration(new List<string> { PortalConfigurations.SharepointDataCollectionGroupsAccess });
            string compFolderPath = $"/{SPRelativeUriPrefix}/{EntityNames.Account}";
            var companyRelativeUrl = _documentLocationAppService.GetDocLocationByRegardingId(new Guid(oDataCollectionFolderReq.CompanyId));
            string subfolderPath = $"{compFolderPath}/{companyRelativeUrl}";
            if (string.IsNullOrEmpty(companyRelativeUrl))
            {
                EntityReference entRefRegardingObject = new EntityReference(EntityNames.Account, new Guid(oDataCollectionFolderReq.CompanyId));
                _documentLocationAppService.CreateSharePointDocumentLocation(EntityNames.Account, entRefRegardingObject);
                companyRelativeUrl = _documentLocationAppService.GetDocLocationByRegardingId(new Guid(oDataCollectionFolderReq.CompanyId));
            }

            var isExists = _fileService.CheckIfPathExistsInSharePoint_Ext(subfolderPath);
            if (!isExists)
            {
                var companyDic = new Dictionary<string, object>
                                    {
                                        { DocumentMetadata.GetDocMetadataFieldName("CompanyId"), oDataCollectionFolderReq.CompanyId},
                                        { DocumentMetadata.GetDocMetadataFieldName("FolderId"), oCompanyFolder.Id}
                                    };
                _fileService.AddSubFolderWithMetaData(companyRelativeUrl, compFolderPath, companyDic);

            }

            foreach (var companyFolderConfig in companyFolderConfigs)
            {
                var companyfolDic = new Dictionary<string, object>
                                    {
                                        { "Title", companyFolderConfig.Name},
                                        { DocumentMetadata.GetDocMetadataFieldName("CompanyId"), oDataCollectionFolderReq.CompanyId},
                                        { DocumentMetadata.GetDocMetadataFieldName("FolderId"), oCompanyFolder.Id},
                                        { DocumentMetadata.GetDocMetadataFieldName("SubFolderId"), companyFolderConfig.Id},
                                };
                string folder = $"/{SPRelativeUriPrefix}/{EntityNames.Account}/{companyRelativeUrl}";
                string subfolder = $"{compFolderPath}/{companyRelativeUrl}/{companyFolderConfig.Name}";
                var isSubExists = _fileService.CheckIfPathExistsInSharePoint_Ext(subfolder);
                if (!isSubExists)
                {
                    _fileService.AddSubFolderWithMetaData(companyFolderConfig.Name, folder, companyfolDic);
                }
                
                var sharepointGroups = configs.SingleOrDefault(a => a.Key == PortalConfigurations.SharepointDataCollectionGroupsAccess)?.Value;
                if (!string.IsNullOrEmpty(sharepointGroups))
                {
                    string targetfolder = $"/{EntityNames.Account}/{companyRelativeUrl}/{companyFolderConfig.Name}";
                    ShareFileWithGroups(targetfolder, sharepointGroups);
                }
            }
        }
        private void CreateBoardMemberFolderStructure(DataCollectionFolderReq oDataCollectionFolderReq, string SPRelativeUriPrefix, FolderStructureRes oBoardMemberFolder, List<SubFolderConfig> folderConfigs)
        {
            var configs = _portalConfigAppService.RetrievePortalConfiguration(new List<string> { PortalConfigurations.SharepointDataCollectionGroupsAccess });
            var contactRelativeUrl = _documentLocationAppService.GetDocLocationByRegardingId(new Guid(oDataCollectionFolderReq.ContactId));
            string bdFolderPath = $"/{SPRelativeUriPrefix}/{EntityNames.Contact}";
            string bdSubfolderPath = $"{bdFolderPath}/{contactRelativeUrl}";

            if (string.IsNullOrEmpty(contactRelativeUrl))
            {
                EntityReference entRefRegardingObject = new EntityReference(EntityNames.Contact, new Guid(oDataCollectionFolderReq.ContactId));
                _documentLocationAppService.CreateSharePointDocumentLocation(EntityNames.Contact, entRefRegardingObject);
                contactRelativeUrl = _documentLocationAppService.GetDocLocationByRegardingId(new Guid(oDataCollectionFolderReq.ContactId));
            }

            var isExists = _fileService.CheckIfPathExistsInSharePoint_Ext(bdSubfolderPath);
            if (!isExists)
            {
                var companyDic = new Dictionary<string, object>
                                    {
                                        { DocumentMetadata.GetDocMetadataFieldName("ContactId"), oDataCollectionFolderReq.ContactId},
                                        { DocumentMetadata.GetDocMetadataFieldName("FolderId"), oBoardMemberFolder.Id}
                                    };
                _fileService.AddSubFolderWithMetaData(contactRelativeUrl, bdFolderPath, companyDic);
            }
            foreach (var folderConfig in folderConfigs)
            {
                var companyfolDic = new Dictionary<string, object>
                                    {
                                        { "Title", folderConfig.Name},
                                        { DocumentMetadata.GetDocMetadataFieldName("ContactId"), oDataCollectionFolderReq.ContactId},
                                        { DocumentMetadata.GetDocMetadataFieldName("FolderId"), oBoardMemberFolder.Id},
                                        { DocumentMetadata.GetDocMetadataFieldName("SubFolderId"), folderConfig.Id},
                                };

                string folder = $"/{SPRelativeUriPrefix}/{EntityNames.Contact}/{contactRelativeUrl}";
                string subfolder = $"{bdFolderPath}/{contactRelativeUrl}/{folderConfig.Name}";

                var isSubExists = _fileService.CheckIfPathExistsInSharePoint_Ext(subfolder);
                if (!isSubExists)
                {
                    _fileService.AddSubFolderWithMetaData(folderConfig.Name, folder, companyfolDic);
                }

                var sharepointGroups = configs.SingleOrDefault(a => a.Key == PortalConfigurations.SharepointDataCollectionGroupsAccess)?.Value;
                if (!string.IsNullOrEmpty(sharepointGroups))
                {
                    string targetfolder = $"/{EntityNames.Contact}/{contactRelativeUrl}/{folderConfig.Name}";
                    ShareFileWithGroups(targetfolder, sharepointGroups);
                }
            }
        }

        private FileMetadata GetUserInfoByFileMetadata(bool systemUser, Guid createdByUser, Guid modifiedByUser)
        {
            FileMetadata fileMetadata = new FileMetadata();

            if (systemUser)
            {

            }
            else
            {
                var query = new QueryExpression("contact")
                {
                    ColumnSet = new ColumnSet("contactid", "lastname", "ntw_lastnamearabic", "firstname", "ntw_firstnamearabic"),
                    Criteria = new FilterExpression
                    {
                        Conditions =
                        {
                            new ConditionExpression("contactid", ConditionOperator.In,
                                createdByUser,
                                modifiedByUser)
                        }
                    }
                };

                EntityCollection result = _crmService.GetInstance().RetrieveMultiple(query);

                foreach (var contact in result.Entities)
                {
                    if (contact != null)
                    {
                        var contactId = contact.GetAttributeValue<Guid>("contactid");
                        var lastName = contact.Contains("lastname") ? contact.GetAttributeValue<string>("lastname") : string.Empty;
                        var lastNameArabic = contact.Contains("ntw_lastnamearabic") ? contact.GetAttributeValue<string>("ntw_lastnamearabic") : string.Empty;
                        var firstName = contact.Contains("firstname") ? contact.GetAttributeValue<string>("firstname") : string.Empty;
                        var firstNameArabic = contact.Contains("ntw_firstnamearabic") ? contact.GetAttributeValue<string>("ntw_firstnamearabic") : string.Empty;

                        if (contactId == createdByUser)
                        {
                            fileMetadata.CreatedBy = firstName + " " + lastName;
                            fileMetadata.CreatedByAr = firstNameArabic + " " + lastNameArabic;
                        }
                        if (contactId == modifiedByUser)
                        {
                            fileMetadata.ModifiedBy = firstName + " " + lastName;
                            fileMetadata.ModifiedByAr = firstNameArabic + " " + lastNameArabic;
                        }
                    }
                }
            }

            return fileMetadata;
        }

        private void ShareFileWithGroups(string targetUrl, string sharepointGroups)
        {
            try
            {
                var groups = sharepointGroups.Split(',');
                foreach (var item in groups)
                {
                    _fileService.ShareFileOrFolderWithGroup(targetUrl, item, 6 /*Editor*/);
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
