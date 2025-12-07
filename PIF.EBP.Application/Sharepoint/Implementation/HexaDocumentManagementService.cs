using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using PIF.EBP.Application.AccessManagement;
using PIF.EBP.Application.Hexa;
using PIF.EBP.Application.MetaData.DTOs;
using PIF.EBP.Application.PortalConfiguration;
using PIF.EBP.Application.Shared;
using PIF.EBP.Application.Shared.Helpers;
using PIF.EBP.Core.CRM;
using PIF.EBP.Core.Exceptions;
using PIF.EBP.Core.FileManagement;
using PIF.EBP.Core.FileManagement.Consts;
using PIF.EBP.Core.FileManagement.DTOs;
using PIF.EBP.Core.Session;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using static PIF.EBP.Application.Shared.Enums;

namespace PIF.EBP.Application.Sharepoint.Implementation
{
    public class HexaDocumentManagementService : IHexaDocumentManagementService
    {
        private readonly ICrmService _crmService;
        private readonly IFileManagement _fileService;
        private readonly IPortalConfigAppService _configService;
        private readonly IHexaAppService _hexaService;
        private readonly IAccessManagementAppService _accessManagementAppService;
        private readonly ISessionService _sessionService;

        public HexaDocumentManagementService(IFileManagement fileService, ICrmService crmService,
                                IPortalConfigAppService configService, IHexaAppService hexaService,
                                IAccessManagementAppService accessManagementAppService, ISessionService sessionService)
        {
            _fileService = fileService;
            _crmService = crmService;
            _configService = configService;
            _hexaService = hexaService;
            _accessManagementAppService = accessManagementAppService;
            _sessionService = sessionService;
        }
        public async Task<DocumentHeader> UploadAttachmentsAsync(UploadDocumentsDto uploadFile)
        {
            var attachmentHeaders = new DocumentHeader();
            var configs = _configService.RetrievePortalConfiguration(new List<string> { PortalConfigurations.SharePointUserIdForPortal });
            if (configs == null || configs.Count == 0)
            {
                throw new Exception("SharePointUserIdIsNotConfigured");
            }
            var loggedInUserId = new Guid(configs.FirstOrDefault().Value);
            string strUserDomainName = GetUserDomainName(loggedInUserId);

            CommonValidationsForSharePointOperations(
                SharePointDocumentLocation.RegardingObjectName, uploadFile.RegardingObjectId);

            var fullRelativeUrl = retrieveFullRelativeUrl(uploadFile.RegardingObjectId);

            foreach (var document in uploadFile.Documents)
            {
                CheckUploadDocumentValidations(document);
                var attachmentHeader = await UploadAttachmentAsync(fullRelativeUrl,
                    document.DocumentName, document.DocumentContent, strUserDomainName);
                attachmentHeaders.Documents.Add(attachmentHeader);
            }

            _fileService.GetFolderPathAndItemCount(fullRelativeUrl, out string folderPath);

            attachmentHeaders.ItemCount = uploadFile.Documents.Count;
            attachmentHeaders.FolderPath = folderPath;
            _hexaService.UpdateHexaReqDocNoOfUploadedDoc(uploadFile.RegardingObjectId,
                attachmentHeaders.ItemCount, true, folderPath);

            return attachmentHeaders;
        }
        public async Task<DocumentHeader> RetrieveAttachmentsDetailsAsync(UploadDocumentsDto uploadFile)
        {
            var attachmentHeaders = new DocumentHeader();

            var fullRelativeUrl = retrieveFullRelativeUrl(uploadFile.RegardingObjectId);

            foreach (var document in uploadFile.Documents)
            {
                DocumentDetails documentDetails = new DocumentDetails();
                documentDetails.DocumentName = document.DocumentName;
                documentDetails.DocumentPath = $"{fullRelativeUrl}/{document.DocumentName}";
                attachmentHeaders.Documents.Add(documentDetails);
            }
            _fileService.GetFolderPathAndItemCount(fullRelativeUrl, out string folderPath);

            attachmentHeaders.ItemCount = uploadFile.Documents.Count;
            attachmentHeaders.FolderPath = folderPath;

            return attachmentHeaders;
        }

        public async Task<DocumentDetails> UploadAttachmentAsync(string fullRelativeUrl,
            string documentName, string documentFile,
            string userId)
        {
            DocumentDetails uploadedDocument = new DocumentDetails();

            try
            {
                _fileService.UploadFileToSPLocation(fullRelativeUrl, documentName, documentFile, userId);

                uploadedDocument.DocumentName = documentName;
                uploadedDocument.DocumentPath = $"{fullRelativeUrl}/{documentName}";

                return uploadedDocument;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private string retrieveFullRelativeUrl(Guid regardingObjectId)
        {
            EntityReference entrefRegardingObject = new EntityReference(
                    SharePointDocumentLocation.RegardingObjectName,
                    regardingObjectId);

            string recordName = GetEntityPrimaryNameValueByRecordId(entrefRegardingObject);
            string recordDefaultFolderName =
                GetDefaultSpLocationOfEntityRecord(regardingObjectId, recordName, true);
            string fullRelativeUrl = $"{SharePointDocumentLocation.RegardingObjectName}/{recordDefaultFolderName}";
            return fullRelativeUrl;
        }

        public async Task<GetDocumentListRspBase> RetrieveDocumentsList(GetDocumentsDto getDocuments)
        {
            List<GetDocumentListRsp> response = new List<GetDocumentListRsp>();

            string targetFileURL = !getDocuments.ByRequest.GetValueOrDefault() ? GetTargetFileURL(getDocuments.RegardingObjectId) : string.Empty;

            if (!string.IsNullOrEmpty(targetFileURL))
            {
                response = _fileService.SPListItems(targetFileURL);
            }
            else
            {
                var configs = _configService.RetrievePortalConfiguration(new List<string> { PortalConfigurations.SharePointUserIdForPortal });
                if (configs == null || configs.Count == 0)
                {
                    throw new ConfigurationException("SharePoint User ID is not configured.");
                }

                var loggedInUserId = Guid.Parse(configs.First().Value);
                string userDomainName = GetUserDomainName(loggedInUserId);

                var requestDocuments = GetRequestDocumentsList(getDocuments);

                foreach (var requestDocument in requestDocuments)
                {
                    var documentList = _fileService.SPListItems(SharePointDocumentLocation.RegardingObjectName, requestDocument.ListName, false, userDomainName);

                    documentList.ForEach(doc =>
                    {
                        doc.EvaluatedAtStep = requestDocument.EvaluatedAtStep;
                        doc.RequestDocumentId = requestDocument.RequestDocumentId;
                        doc.EvaluatedStepName = requestDocument.EvaluatedStepName;
                    });

                    response.AddRange(documentList);
                }
            }

            return new GetDocumentListRspBase
            {
                GetDocumentListRsp = response,
                ItemCount = response.Count,
                Description = response.Any() ? "Documents retrieved successfully!" : "No documents were found at the specified path."
            };
        }

        public List<RequestReferenceDocument> GetRequestDocuments(Guid requestId)
        {
            QueryExpression query = new QueryExpression(EntityNames.RequestDocument)
            {
                ColumnSet = new ColumnSet("hexa_requestdocumentid", "hexa_request", "hexa_evaluatedatstep"),
                Criteria = new FilterExpression
                {
                    FilterOperator = LogicalOperator.And,
                    Conditions =
                    {
                        new ConditionExpression("hexa_request", ConditionOperator.Equal, requestId)
                    }
                }
            };
            var requeststepLink = new LinkEntity
            {
                LinkFromEntityName = EntityNames.RequestDocument,
                LinkFromAttributeName = "hexa_evaluatedatstep",
                LinkToEntityName = EntityNames.RequestStep,
                LinkToAttributeName = "hexa_requeststepid",
                JoinOperator = JoinOperator.LeftOuter,
                Columns = new ColumnSet("hexa_name"),
                EntityAlias = "RequestStep",
            };
            query.LinkEntities.Add(requeststepLink);
            EntityCollection result = _crmService.GetInstance().RetrieveMultiple(query);

            var documents = result.Entities.Select(e => new RequestReferenceDocument
            {
                RequestDocumentId=e.Id,
                EvaluatedAtStep= e.GetAttributeValue<EntityReference>("hexa_evaluatedatstep")?.Id,
                EvaluatedStepName=e.GetAttributeValue<AliasedValue>("RequestStep.hexa_name")?.Value.ToString()??string.Empty,
            }).ToList();

            return documents;
        }

        private string GetTargetFileURL(Guid regardingObjectId)
        {
            string targetFileURL = string.Empty;
            var orgContext = new OrganizationServiceContext(_crmService.GetInstance());

            var query = from document in orgContext.CreateQuery("hexa_requestdocument")
                        where document.GetAttributeValue<Guid>("hexa_requestdocumentid") == regardingObjectId
                        select new
                        {
                            SharepointFolderUrl = document.GetAttributeValue<string>("hexa_sharepointfolderurl"),
                            ShareFolderUrl = document.GetAttributeValue<string>("hexa_sharefolderurl")
                        };

            var result = query.FirstOrDefault();

            if (result != null)
            {
                return string.IsNullOrEmpty(result.SharepointFolderUrl) ? result.ShareFolderUrl : result.SharepointFolderUrl;
            }

            return targetFileURL;
        }

        public async Task<DeleteDocumentsRes> DeleteAttachmentsAsync(DeleteDocumentsDto deleteDocumentRqt)
        {
            var configs = _configService.RetrievePortalConfiguration(new List<string> { PortalConfigurations.SharePointUserIdForPortal });
            if (configs == null || configs.Count == 0)
            {
                throw new Exception("SharePointUserIdIsNotConfigured");
            }
            var loggedInUserId = new Guid(configs.FirstOrDefault().Value);
            string strUserDomainName = GetUserDomainName(loggedInUserId);
            var oRegardingObject = GetRegardingObjectDetails(deleteDocumentRqt.RegardingObjectId);
            bool authorizationCheck = await CheckPermissionToDeleteFile(oRegardingObject.Item2);
            if (!authorizationCheck)
            {
                throw new UserFriendlyException("UserUnauthorized", System.Net.HttpStatusCode.Forbidden);
            }

            string sharepointLibraryName = GetListName(deleteDocumentRqt.RegardingObjectId);
            int deleteResult = _fileService.SPDeleteFile(deleteDocumentRqt.DocumentsName, strUserDomainName, oRegardingObject.Item1, sharepointLibraryName);


            var itemCount = _fileService.SPListItemsCount(SharePointDocumentLocation.RegardingObjectName, sharepointLibraryName);

            _hexaService.UpdateHexaReqDocNoOfUploadedDoc(deleteDocumentRqt.RegardingObjectId,
                deleteResult, false);

            if (deleteResult.Equals(deleteDocumentRqt.DocumentsName.Count))
            {
                return new DeleteDocumentsRes()
                {
                    DeletedCount = deleteResult,
                    ItemCount = itemCount,
                    Description = "All files were deleted successfully."
                };
            }
            else if (deleteResult.Equals(0))
            {
                return new DeleteDocumentsRes()
                {
                    DeletedCount = deleteResult,
                    ItemCount = itemCount,
                    Description = "All Attachment Path are Invalid "
                };
            }
            else
            {
                return new DeleteDocumentsRes()
                {
                    DeletedCount = deleteResult,
                    ItemCount = itemCount,
                    Description = "Some of the  Attachment Path is Invalid "
                };
            }
        }

        public async Task<CopyFilesResponse> CopyDocumentFiles(CopyFiles documentInfo)
        {
            try
            {
                CopyFilesResponse CopyResults = new CopyFilesResponse();
                int successCounter = 0;
                string errorMessage = string.Empty;

                var configs = _configService.RetrievePortalConfiguration(new List<string> { PortalConfigurations.SharePointUserIdForPortal });
                if (configs == null || configs.Count == 0)
                {
                    throw new Exception("SharePointUserIdIsNotConfigured");
                }

                var loggedInUserId = new Guid(configs.FirstOrDefault().Value);
                string strUserDomainName = GetUserDomainName(loggedInUserId);

                try
                {
                    string sourceObjectName = GetListName(documentInfo.SourceObjectListId, false);
                    string destinationObjectName = GetListName(documentInfo.DestinationObjectId, false);
                    string destinationFolderPath = string.Empty;
                    int copyDocsCount = _fileService.CopyDocumentFiles(documentInfo, sourceObjectName,
                        "Archive", strUserDomainName, SharePointDocumentLocation.RegardingObjectName,
                        destinationObjectName, out int itemCount, ref destinationFolderPath);

                    CopyResults = new CopyFilesResponse
                    {
                        SourceObjectListId = documentInfo.SourceObjectListId,
                        CopiedItemCount = copyDocsCount,
                        ItemCount = itemCount,
                        FolderPath = destinationFolderPath
                    };
                    successCounter++;
                }
                catch (Exception ex)
                {
                    errorMessage = ex.Message;
                }

                if (successCounter > 0)
                {
                    UpdateDestinationSPFolderUrl(CopyResults.FolderPath, documentInfo.DestinationObjectId);
                    return CopyResults;
                }
                else
                    throw new UserFriendlyException(errorMessage);
            }
            catch (Exception ex)
            {
                throw new UserFriendlyException(ex.Message);
            }
        }

        public void UpdateDestinationSPFolderUrl(string path, Guid destinationObjId)
        {
            var requestDoc = new Entity
            {
                Id = destinationObjId,
                LogicalName = SharePointDocumentLocation.RegardingObjectName
            };

            requestDoc["hexa_sharepointfolderurl"] = path;
            _crmService.Update(requestDoc, SharePointDocumentLocation.RegardingObjectName);
        }

        public async Task<GetDocumentRsp> GetAttachmentAsync(GetDocumentDto getDocument)
        {
            try
            {
                var configs = _configService.RetrievePortalConfiguration(new List<string> { PortalConfigurations.SharePointUserIdForPortal });
                if (configs == null || configs.Count == 0)
                {
                    throw new Exception("SharePointUserIdIsNotConfigured");
                }
                var loggedInUserId = new Guid(configs.FirstOrDefault().Value);
                string strUserDomainName = GetUserDomainName(loggedInUserId);

                if (string.IsNullOrWhiteSpace(getDocument.DocumentName))
                    throw new UserFriendlyException("DocumentPathShouldNotBeEmpty");

                if (getDocument.RegardingObjectId == null || getDocument.RegardingObjectId == Guid.Empty)
                    throw new UserFriendlyException("RegardingObjectShouldNotBeEmpty");

                string folderPath = GetTargetFileURL(getDocument.RegardingObjectId);
                GetDocumentRsp attachmentData = GetDocumentContentData(folderPath, getDocument.DocumentName,
                    strUserDomainName);

                return attachmentData;
            }
            catch (Exception ex)
            {
                throw new UserFriendlyException(ex.Message);
            }
        }

        public GetDocumentRsp GetDocumentContentData(string folderPath, string docName, string user)
        {
            var doc = _fileService.SPGetItem(folderPath, docName, user);
            return new GetDocumentRsp()
            {
                DocumentContent = doc.DocumentContent
            };
        }

        public string GetDefaultSpLocationOfEntityRecord(Guid regardingObjectId, string recordName, bool IsFolderNeedToBeCreated = false)
        {
            string defaultSpLocRecordFolder = string.Empty;

            try
            {
                //check if default location avilable for entity record
                EntityReference rootLocationId = GetRootSpLocationByEntityType(SharePointDocumentLocation.RegardingObjectName);
                EntityReference entrefRegardingObject =
                    new EntityReference(SharePointDocumentLocation.RegardingObjectName, regardingObjectId);

                ColumnSet columns = new ColumnSet
                (
                    SharePointDocumentLocation.ParentSiteOrLocation,
                    SharePointDocumentLocation.RelativeUrl,
                    SharePointDocumentLocation.RegardingObjectId
                );

                FilterExpression filterExp = new FilterExpression();
                filterExp.AddCondition(SharePointDocumentLocation.RegardingObjectId, ConditionOperator.Equal,
                    regardingObjectId);
                filterExp.AddCondition(SharePointDocumentLocation.ParentSiteOrLocation, ConditionOperator.Equal,
                    rootLocationId.Id);

                EntityCollection crmSpLocList =
                    RetrieveAllRecordsByEntityNameColumnSet(SharePointDocumentLocation.EntityLogicalName,
                        columns, filterExp);

                if (crmSpLocList != null && crmSpLocList.Entities.Count > 0)
                {
                    crmSpLocList.Entities
                    .Select(entity => new
                    {
                        RelativeUrl = entity.Contains(SharePointDocumentLocation.RelativeUrl)
                        ? entity[SharePointDocumentLocation.RelativeUrl].ToString() : null
                    })
                    .Where(spLocationEnt => !string.IsNullOrEmpty(spLocationEnt.RelativeUrl))
                    .ToList()
                    .ForEach(spLocationEnt =>
                    {
                        defaultSpLocRecordFolder = spLocationEnt.RelativeUrl;
                    });
                }

                if (string.IsNullOrWhiteSpace(defaultSpLocRecordFolder))
                {
                    defaultSpLocRecordFolder = CreateDefaultSpLocationForCrmRecord(rootLocationId, entrefRegardingObject, recordName);
                }
                else
                {
                    if (IsFolderNeedToBeCreated)
                        _fileService.AddFolderToSpRootLibrary(SharePointDocumentLocation.RegardingObjectName, defaultSpLocRecordFolder);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return defaultSpLocRecordFolder;
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

        public bool CheckIfRecordExistWithId(string entityLogicalName, Guid? crmRecordId)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(entityLogicalName) && crmRecordId.HasValue && crmRecordId != Guid.Empty)
                {
                    _crmService.GetInstance().Retrieve(entityLogicalName, (Guid)crmRecordId, new ColumnSet(false));
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        public string GetEntityPrimaryNameValueByRecordId(EntityReference entrefRecord)
        {
            string strEntityPrimaryAttributeName = string.Empty;

            try
            {
                RetrieveEntityRequest retrieveAttributeRequest = new RetrieveEntityRequest()
                {
                    LogicalName = entrefRecord.LogicalName
                };

                RetrieveEntityResponse entityResponse =
                    (RetrieveEntityResponse)_crmService.GetInstance().Execute(retrieveAttributeRequest);

                string primaryIdAttribute = entityResponse.EntityMetadata.PrimaryIdAttribute;
                string primaryNameAttribute = entityResponse.EntityMetadata.PrimaryNameAttribute;

                FilterExpression filterExp = new FilterExpression();
                filterExp.AddCondition(primaryIdAttribute,
                    ConditionOperator.Equal, entrefRecord.Id);

                EntityCollection ecolResults = RetrieveAllRecordsByEntityNameColumnSet(
                    entrefRecord.LogicalName, new ColumnSet(primaryNameAttribute), filterExp);

                if (ecolResults != null && ecolResults.Entities.Count > 0)
                {
                    Entity crmRecord = ecolResults.Entities.FirstOrDefault();

                    if (crmRecord != null && crmRecord.Contains(primaryNameAttribute))
                        strEntityPrimaryAttributeName = crmRecord.GetAttributeValue<string>(primaryNameAttribute);
                }
            }
            catch (Exception)
            {
                throw;
            }

            return strEntityPrimaryAttributeName;
        }

        public EntityCollection RetrieveAllRecordsByEntityNameColumnSet(string entityName,
            ColumnSet entityColumnSet = null, FilterExpression filterExpression = null)
        {
            try
            {
                QueryExpression query = new QueryExpression(entityName);

                if (entityColumnSet != null)
                {
                    query.ColumnSet = new ColumnSet();
                    query.ColumnSet = entityColumnSet;
                }

                query.Distinct = true;
                query.PageInfo = new PagingInfo();
                query.PageInfo.Count = 5000;
                query.PageInfo.PageNumber = 1;
                query.PageInfo.ReturnTotalRecordCount = true;
                query.PageInfo.PagingCookie = string.Empty;

                if (filterExpression != null)
                {
                    query.Criteria = filterExpression;
                }

                EntityCollection lstEntities = new EntityCollection();
                bool moreRecords = false;

                do
                {
                    EntityCollection entityCollection = _crmService.GetInstance().RetrieveMultiple(query);

                    if (entityCollection != null && entityCollection.Entities.Count > 0)
                    {
                        moreRecords = entityCollection.MoreRecords;
                        lstEntities.Entities.AddRange(entityCollection.Entities);

                        if (moreRecords)
                        {
                            query.PageInfo.PageNumber++;
                            query.PageInfo.PagingCookie = entityCollection.PagingCookie;
                        }
                    }
                } while (moreRecords);

                return lstEntities;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private EntityReference GetRootSpLocationByEntityType(string entityLogicalName)
        {
            EntityReference entrefSpLocation = null;

            try
            {
                ColumnSet columns = new ColumnSet
                (
                    SharePointDocumentLocation.Id,
                    SharePointDocumentLocation.RelativeUrl
                );
                FilterExpression filterExp = new FilterExpression();
                filterExp.AddCondition(SharePointDocumentLocation.RelativeUrl, ConditionOperator.Equal,
                    entityLogicalName);

                EntityCollection crmSpLocList =
                    RetrieveAllRecordsByEntityNameColumnSet(
                        SharePointDocumentLocation.EntityLogicalName, columns, filterExp);

                if (crmSpLocList != null && crmSpLocList.Entities.Count > 0)
                {
                    var entity = crmSpLocList.Entities.FirstOrDefault();
                    if (entity != null)
                        entrefSpLocation = entity.ToEntityReference();
                }
                else
                {
                    throw new UserFriendlyException("RootFolderNotFound");
                }
            }
            catch (Exception)
            {
                throw;
            }

            return entrefSpLocation;
        }

        private string CreateDefaultSpLocationForCrmRecord(EntityReference rootLocationRef, EntityReference recordId, string recordName)
        {
            string folderName = string.Empty;
            try
            {
                folderName = GetFormattedFolderName(recordId.Id, recordName);

                //Create the same folder in SharePoint
                _fileService.AddFolderToSpRootLibrary(recordId.LogicalName, folderName);

                Entity entSPDocLocation = new Entity(SharePointDocumentLocation.EntityLogicalName);
                entSPDocLocation.Attributes[SharePointDocumentLocation.Name] = folderName;
                entSPDocLocation.Attributes[SharePointDocumentLocation.ParentSiteOrLocation] = rootLocationRef;
                entSPDocLocation.Attributes[SharePointDocumentLocation.RelativeUrl] = folderName;
                entSPDocLocation.Attributes[SharePointDocumentLocation.RegardingObjectId] = recordId;
                _crmService.GetInstance().Create(entSPDocLocation);
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return folderName;
        }

        private static string GetFormattedFolderName(Guid recordId, string recordName)
        {
            string strFormattedFolderName;

            try
            {
                string guidString = recordId != Guid.Empty
                    ? recordId.ToString().Replace("-", string.Empty).ToUpper()
                    : Guid.Empty.ToString();

                strFormattedFolderName = recordName + "_" + guidString;

            }
            catch (Exception)
            {
                throw;
            }

            return strFormattedFolderName;
        }

        public void CommonValidationsForSharePointOperations(string strTargetLogicalName, Guid guidTargetId)
        {
            EntityReference entrefRegardingObject = new EntityReference(strTargetLogicalName, guidTargetId);

            if (entrefRegardingObject == null || entrefRegardingObject.Id == Guid.Empty)
                throw new UserFriendlyException("RegardingObjectShouldNotBeEmpty");

            if (!CheckIfRecordExistWithId(entrefRegardingObject.LogicalName, entrefRegardingObject.Id))
                throw new InvalidPluginExecutionException("Regarding Object is not valid!");
        }

        public void CheckUploadDocumentValidations(UploadedDocDetails uploadAttachment)
        {
            if (string.IsNullOrWhiteSpace(uploadAttachment.DocumentName) || string.IsNullOrWhiteSpace(uploadAttachment.DocumentContent))
                throw new UserFriendlyException("AttachmentInformationShouldNotBeEmpty");

            if (!IsFileSizeInAllowedLimit(uploadAttachment.DocumentName.Length))
                throw new UserFriendlyException($"UploadedFileIsExceededTheMaximumAllowedSizeOf " +
                    $"{500}MB");
        }

        private bool IsFileSizeInAllowedLimit(int LengthInCharacters)
        {
            int kiloByteSize = (3 * (LengthInCharacters / 4)) / 1024;

            int allowedKB = 500 * 1024;

            if (kiloByteSize <= allowedKB)
                return true;
            else
                return false;
        }

        public string GetListName(Guid SPLibraryId, bool IsFolderNeedToBeCreated = false)
        {
            string strSPListName;

            try
            {
                CommonValidationsForSharePointOperations(SharePointDocumentLocation.RegardingObjectName, SPLibraryId);

                EntityReference entrefRegardingObject = new EntityReference(SharePointDocumentLocation.RegardingObjectName, SPLibraryId);

                string recordName = GetEntityPrimaryNameValueByRecordId(entrefRegardingObject);

                strSPListName = GetDefaultSpLocationOfEntityRecord(SPLibraryId, recordName, IsFolderNeedToBeCreated);
            }
            catch (Exception)
            {
                throw;
            }

            return strSPListName;
        }

        public (string, Guid) GetRegardingObjectDetails(Guid regardingObjectId)
        {
            string strSPListName = string.Empty;
            Guid requestStepId = Guid.Empty;
            QueryExpression query = new QueryExpression(SharePointDocumentLocation.RegardingObjectName);
            query.ColumnSet = new ColumnSet("hexa_requestdocumentid", "hexa_sharefolderurl", "hexa_evaluatedatstep");

            query.Criteria.AddCondition("hexa_requestdocumentid", ConditionOperator.Equal, regardingObjectId);
            EntityCollection result = _crmService.GetInstance().RetrieveMultiple(query);
            if (result.Entities.Any())
            {
                var entity = result.Entities.FirstOrDefault();
                strSPListName = entity.GetValueByAttributeName<string>("hexa_sharefolderurl");
                requestStepId = entity.GetValueByAttributeName<EntityReference>("hexa_evaluatedatstep")?.Id ?? Guid.Empty;
            }
            return (strSPListName, requestStepId);
        }

        private async Task<bool> CheckPermissionToDeleteFile(Guid requestStepId)
        {
            QueryExpression query = new QueryExpression(EntityNames.RequestStep);
            query.ColumnSet = new ColumnSet("hexa_processstep", "statecode", "hexa_customer", "hexa_portalcontact");
            var processStepTemplateLink = new LinkEntity
            {
                LinkFromEntityName = EntityNames.RequestStep,
                LinkFromAttributeName = "hexa_processstep",
                LinkToEntityName = EntityNames.ProcessStepTemplate,
                LinkToAttributeName = "hexa_processsteptemplateid",
                JoinOperator = JoinOperator.Inner,
                Columns = new ColumnSet("hexa_processsteptemplateid", "hexa_portalroleid"),
                EntityAlias = "ProcessTemplate",
            };
            // Add link entities to the query
            query.LinkEntities.Add(processStepTemplateLink);
            query.Criteria.AddCondition("hexa_requeststepid", ConditionOperator.Equal, requestStepId);
            EntityCollection result = _crmService.GetInstance().RetrieveMultiple(query);
            if (result.Entities.Any())
            {
                var hexaEntity = result.Entities.Select(entity => new
                {
                    PortalContact = entity.GetValueByAttributeName<EntityReferenceDto>("hexa_portalcontact"),
                    PortalRole = (EntityReference)CRMOperations.GetValueByAttributeName<AliasedValue>(entity, "ProcessTemplate.hexa_portalroleid")?.Value,
                    StateCode = entity.GetValueByAttributeName<EntityOptionSetDto>("statecode")
                }).FirstOrDefault();
                if (hexaEntity.StateCode.Value == "1")
                {
                    return false;
                }

                var userPermissions = await _accessManagementAppService.GetAuthorizedPermissions();
                var permission = userPermissions.Where(x => x.PageLink.Contains(PageRoute.TaskDetails)).FirstOrDefault();

                if (permission == null || permission.Delete == (int)AccessLevel.None)
                {
                    return false;
                }
                if (hexaEntity.PortalContact != null)
                {
                    if (hexaEntity.PortalContact.Id != _sessionService.GetContactId())
                    {
                        return false;
                    }
                }

                var parentRoleForLoggedInUser = await _accessManagementAppService.GetParentRoleByRoleId(_sessionService.GetRoleId());

                if (permission.Delete == (int)AccessLevel.Basic)
                {
                    if (hexaEntity.PortalRole != null)
                    {
                        if (!(hexaEntity.PortalRole.Id.ToString() == parentRoleForLoggedInUser || hexaEntity.PortalRole.Id.ToString() == _sessionService.GetRoleId()))
                        {
                            return false;
                        }
                    }

                }
            }
            return true;
        }

        private List<RequestReferenceDocument> GetRequestDocumentsList(GetDocumentsDto getDocuments)
        {
            var requestDocuments = new List<RequestReferenceDocument>();

            if (getDocuments.ByRequest.GetValueOrDefault())
            {
                var documents = GetRequestDocuments(getDocuments.RegardingObjectId);
                requestDocuments = documents.Select(doc => new RequestReferenceDocument
                {
                    ListName = GetListName(doc.RequestDocumentId.Value),
                    EvaluatedAtStep = doc.EvaluatedAtStep,
                    RequestDocumentId = doc.RequestDocumentId,
                    EvaluatedStepName=doc.EvaluatedStepName
                }).ToList();
            }
            else
            {
                requestDocuments.Add(new RequestReferenceDocument
                {
                    ListName = GetListName(getDocuments.RegardingObjectId),
                    EvaluatedAtStep = null,
                    RequestDocumentId = null
                });
            }

            return requestDocuments;
        }
    }
}
