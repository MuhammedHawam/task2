using PIF.EBP.Application.DocumentLocation;
using PIF.EBP.Application.PortalConfiguration;
using PIF.EBP.Application.Shared;
using PIF.EBP.Core.Exceptions;
using PIF.EBP.Core.FileManagement;
using PIF.EBP.Core.FileManagement.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using PIF.EBP.Core.CRM;
using PIF.EBP.Application.Shared.Helpers;
using PIF.EBP.Core.Utilities;
using System.Configuration;

namespace PIF.EBP.Application.KnowledgeHub.Implementation
{
    public class KnowledgeHubFileUploaderService: IKnowledgeHubFileUploaderService
    {
        private readonly IDocumentLocationAppService _documentLocationAppService;
        private readonly IPortalConfigAppService _portalConfigAppService;
        private readonly IFileManagement _fileService;
        private readonly ICrmService _crmService;

        public KnowledgeHubFileUploaderService(ICrmService crmService, IFileManagement fileService,
            IPortalConfigAppService portalConfigAppService, IDocumentLocationAppService documentLocationAppService)
        {
            _crmService= crmService;
            _fileService= fileService;
            _portalConfigAppService= portalConfigAppService;
            _documentLocationAppService= documentLocationAppService;
        }
        public async Task KnowledgeItemUpload(UploadDocumentsDto uploadDocumentsDto)
        {
            var response = await UploadDocuments(uploadDocumentsDto);

            EntityReference entRefRegardingObject = new EntityReference(EntityNames.KnowledgeItem, uploadDocumentsDto.KnowledgeItemId);
            _documentLocationAppService.CreateSharePointDocumentLocation(EntityNames.KnowledgeItem, entRefRegardingObject);
            await AddKnowledgeHubFolders(uploadDocumentsDto.KnowledgeItemId);
            if (uploadDocumentsDto.IsPortalCall)
            {
                NotifyPRM($"{response?.FirstOrDefault()?.DocumentPath}", uploadDocumentsDto.CompanyId, uploadDocumentsDto.ContactId);
            }
        }
        public async Task<bool> AddKnowledgeHubFolders(Guid knowledgeItemId)
        {
            try
            {
                var configs = _portalConfigAppService.RetrievePortalConfiguration(new List<string> { PortalConfigurations.SharepointKnowledgeHubGroupsAccess });
                var sharepointGroups = configs.SingleOrDefault(a => a.Key == PortalConfigurations.SharepointKnowledgeHubGroupsAccess)?.Value;
                var targetFolder = string.Empty;
                var relativeUrl = _documentLocationAppService.GetDocLocationByRegardingId(knowledgeItemId);
                if (!string.IsNullOrEmpty(relativeUrl))
                {
                    var linkFolderUrl = $"{EntityNames.KnowledgeItem}/{relativeUrl}/Attachment";
                    var mediaFolderUrl = $"{EntityNames.KnowledgeItem}/{relativeUrl}/Images";
                    var coverFolderUrl = $"{EntityNames.KnowledgeItem}/{relativeUrl}/Cover";
                    _fileService.CheckFolderStructure(linkFolderUrl, EntityNames.KnowledgeItem);
                    _fileService.CheckFolderStructure(mediaFolderUrl, EntityNames.KnowledgeItem);
                    _fileService.CheckFolderStructure(coverFolderUrl, EntityNames.KnowledgeItem);
                    targetFolder = $"{EntityNames.KnowledgeItem}/{relativeUrl}";
                }
                else
                {
                    EntityReference entRefRegardingObject = new EntityReference(EntityNames.KnowledgeItem, knowledgeItemId);
                    _documentLocationAppService.CreateSharePointDocumentLocation(EntityNames.KnowledgeItem, entRefRegardingObject);
                    var rootfolderName = GetFormattedFolderName(knowledgeItemId.ToString(), string.Empty);
                    var rootFolderUrl = $"{EntityNames.KnowledgeItem}/{rootfolderName}";
                    _fileService.CheckFolderStructure(rootFolderUrl, EntityNames.KnowledgeItem);
                    var linkFolderUrl = $"{EntityNames.KnowledgeItem}/{rootfolderName}/Attachment";
                    var mediaFolderUrl = $"{EntityNames.KnowledgeItem}/{rootfolderName}/Images";
                    var coverFolderUrl = $"{EntityNames.KnowledgeItem}/{rootfolderName}/Cover";
                    _fileService.CheckFolderStructure(linkFolderUrl, EntityNames.KnowledgeItem);
                    _fileService.CheckFolderStructure(mediaFolderUrl, EntityNames.KnowledgeItem);
                    _fileService.CheckFolderStructure(coverFolderUrl, EntityNames.KnowledgeItem);
                    targetFolder = rootFolderUrl;
                }
                if (!string.IsNullOrEmpty(sharepointGroups))
                {
                    ShareFileWithGroups(targetFolder, sharepointGroups);
                }
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private Task<List<UploadFilesResponse>> UploadDocuments(UploadDocumentsDto uploadDocumentsDto)
        {
            try
            {
                List<UploadFilesResponse> uploadFilesResponses = new List<UploadFilesResponse>();
                var configs = _portalConfigAppService.RetrievePortalConfiguration(new List<string> { PortalConfigurations.SharePointUserIdForPortal, PortalConfigurations.SharepointKnowledgeHubGroupsAccess });

                if (configs == null || configs.Count == 0)
                {
                    throw new Exception("SharePointUserIdIsNotConfigured");
                }
                var knowledgeItemFolderName = ConfigurationManager.AppSettings["KnowledgeItemFolderName"];
                _fileService.EnsureAllKnowledgeHubCustomFieldExists(knowledgeItemFolderName);
                uploadDocumentsDto.TargetFolderURL = "/" + EntityNames.KnowledgeItem + "/" + GetFormattedFolderName(uploadDocumentsDto.KnowledgeItemId.ToString(), GetItemName(uploadDocumentsDto.KnowledgeItemId.ToString()));
                _fileService.CheckFolderStructure(GetFormattedFolderName(uploadDocumentsDto.KnowledgeItemId.ToString(), GetItemName(uploadDocumentsDto.KnowledgeItemId.ToString())), EntityNames.KnowledgeItem);

                //uploadDocumentsDto.FileMetadata.Remove("ItemId");

                var loggedInUserId = new Guid(configs.FirstOrDefault().Value);
                string strUserDomainName = GetUserDomainName(loggedInUserId);
                
                foreach (var document in uploadDocumentsDto.Documents)
                {
                    CheckUploadDocumentValidations(document, uploadDocumentsDto.Documents.Count);
                    var uploadFilesResponse = _fileService.UploadFiles(uploadDocumentsDto.TargetFolderURL, document.DocumentName, document.DocumentContent, strUserDomainName, uploadDocumentsDto.FileMetadata);
                    var sharepointGroups = configs.SingleOrDefault(a => a.Key == PortalConfigurations.SharepointKnowledgeHubGroupsAccess)?.Value;
                    if (!string.IsNullOrEmpty(sharepointGroups) && uploadFilesResponse.Uploaded) {
                        ShareFileWithGroups(uploadDocumentsDto.TargetFolderURL, sharepointGroups);
                    }
                    uploadFilesResponses.Add(uploadFilesResponse);
                }

                return Task.FromResult(uploadFilesResponses);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private void NotifyPRM(string sharePointUrl, string companyId, string contactId)
        {
            OrganizationRequest NotifyPRM = new OrganizationRequest("pwc_EBPActionKnowledgeItemNotifyPRM");

            NotifyPRM["Contact"] = new EntityReference(EntityNames.Contact, new Guid(contactId));
            NotifyPRM["Company"] = new EntityReference(EntityNames.Account, new Guid(companyId));
            NotifyPRM["SharePointURL"] = sharePointUrl;
            var orgService = _crmService.GetInstance();
            orgService.Execute(NotifyPRM);
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
        private string GetUserDomainName(Guid userId)
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
        private string GetFormattedFolderName(string recordId, string recordName)
        {
            string strFormattedFolderName;

            try
            {
                string guidString = recordId != string.Empty
                    ? recordId.ToString().Replace("-", string.Empty).ToUpper()
                    : string.Empty.ToString();

                strFormattedFolderName = recordName + "_" + guidString;

            }
            catch (Exception)
            {
                throw;
            }

            return strFormattedFolderName;
        }
        private string GetItemName(string ItemId)
        {
            var primaryId = "pwc_knowledgeitemid";
            string[] columns = new string[] { primaryId, "pwc_name" };
            var oEntity = _crmService.GetById(EntityNames.KnowledgeItem, columns, Guid.Parse(ItemId), primaryId);
            Guard.AssertArgumentNotNull(oEntity);
            return oEntity.GetValueByAttributeName<string>("pwc_name");
        }
        private void CheckUploadDocumentValidations(UploadedDocDetails uploadAttachment, int countOfDocuments)
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
    }
}
