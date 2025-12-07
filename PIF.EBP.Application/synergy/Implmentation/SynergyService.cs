using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using PIF.EBP.Application.Contacts.Dtos;
using PIF.EBP.Application.PerformanceDashboard.DTOs;
using PIF.EBP.Application.PortalConfiguration;
using PIF.EBP.Application.Settings;
using PIF.EBP.Application.Settings.DTOs;
using PIF.EBP.Application.Shared;
using PIF.EBP.Application.Shared.Helpers;
using PIF.EBP.Application.Synergy.DTOs;
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
using System.Text;
using System.Threading.Tasks;

namespace PIF.EBP.Application.Synergy.Implmentation
{
    public class SynergyService : ISynergyService
    {
        private readonly IUserProfileAppService _userProfileAppService;
        private readonly ISessionService _sessionService;
        private readonly IPortalConfigAppService _portalConfigAppService;
        private readonly ICrmService _crmService;
        private readonly IFileManagement _fileService;


        public SynergyService(ICrmService crmService,ISessionService sessionService, 
            IPortalConfigAppService portalConfigAppService, IFileManagement fileService,
            IUserProfileAppService userProfileAppService)
        {
            _userProfileAppService = userProfileAppService;
            _sessionService = sessionService ?? throw new ArgumentNullException(nameof(sessionService));
            _portalConfigAppService = portalConfigAppService ?? throw new ArgumentNullException(nameof(portalConfigAppService));
            _fileService = fileService;

            _crmService = crmService ?? throw new ArgumentNullException(nameof(crmService));
        }


        public List<CompaniesLogosDto> GetCompaniesLogos(List<Guid> companiesIds)
        {
            var orgService = _crmService.GetInstance();

            var query = new QueryExpression(EntityNames.Account)
            {
                ColumnSet = new ColumnSet("accountid", "entityimage"),
                Criteria = new FilterExpression()
            };

            query.Criteria.AddCondition("accountid", ConditionOperator.In, companiesIds);

            var companies = orgService.RetrieveMultiple(query);


            return companies.Entities.Select(a => new CompaniesLogosDto
            {
                CompanyId = a.Id,
                CompanyLogo = a.GetAttributeValue<string>("entityimage")
            }).ToList();

        }
        public async Task<SynergyUserDataDto> RetrieveUserProfileDataAndSector()
        {
            UserProfileData userData = _userProfileAppService.RetrieveUserProfileData();
            if (userData == null)
            {
                throw new UserFriendlyException("InvalidContactIdValue", System.Net.HttpStatusCode.BadRequest);

            }
            ServiceProvider serviceProvider = await GetServiceProvider();
            if (serviceProvider == null) {
                throw new UserFriendlyException("InvalidContactIdValue", System.Net.HttpStatusCode.BadRequest);

            }
            SynergyUserDataDto synergyUser = new SynergyUserDataDto()
            {
                FirstName = userData.FirstName,
                LastName = userData.LastName,

                Email = userData.Email,
                Mobile = userData.Mobile,
                Position = userData.Position,

                Sector = new SectorDto()
                {
                    Id = serviceProvider.Id,
                    Name = serviceProvider.Name
                }
                
            };
            return synergyUser;
        }
        private async Task<ServiceProvider> GetServiceProvider()
        {
            var query = new QueryExpression(EntityNames.Account)
            {
                ColumnSet = new ColumnSet(),
                LinkEntities =
        {
            new LinkEntity
            {
                LinkFromEntityName = EntityNames.Account,
                LinkFromAttributeName = "ntw_gicssectorid",
                LinkToEntityName = EntityNames.GICSSector,
                LinkToAttributeName = "ntw_gicssectorid",
                JoinOperator = JoinOperator.LeftOuter,
                EntityAlias = "GICSSectorAlias",
                Columns = new ColumnSet("ntw_gicssectorid", "pwc_referenceidarabic", "ntw_name", "pwc_flag")
            }
        }
            };
            query.Criteria.AddCondition("accountid", ConditionOperator.Equal, _sessionService.GetCompanyId());

            var entityCollection = _crmService.GetInstance().RetrieveMultiple(query);

            return entityCollection.Entities.Select(entity => new ServiceProvider
            {
                Id = entity.Contains("GICSSectorAlias.ntw_gicssectorid") ? new Guid(entity.GetValueByAttributeName<AliasedValue>("GICSSectorAlias.ntw_gicssectorid")?.Value.ToString()) : Guid.Empty,
                Name = entity.GetValueByAttributeName<AliasedValue>("GICSSectorAlias.ntw_name")?.Value.ToString() ?? string.Empty,
                NameAr = entity.GetValueByAttributeName<AliasedValue>("GICSSectorAlias.pwc_referenceidarabic")?.Value.ToString() ?? string.Empty,
                Flag = entity.GetValueByAttributeName<AliasedValue>("GICSSectorAlias.pwc_flag")?.Value.ToString() ?? string.Empty,
            }).FirstOrDefault();
        }
        public List<SectorLookupDto> GetSectors()
        {
            var orgService = _crmService.GetInstance();

            var query = new QueryExpression(EntityNames.GICSSector)
            {
                ColumnSet = new ColumnSet("ntw_gicssectorid", "ntw_name"),
                Criteria = new FilterExpression()
            };

            var sectors = orgService.RetrieveMultiple(query);


            return sectors.Entities.Select(a => new SectorLookupDto
            {
                SectorId = a.Id,
                SectorName = a.GetAttributeValue<string>("ntw_name")
            }).ToList();

        }
        public async Task<List<UploadFilesResponse>> SynergyRequestUpload(UploadDocumentsDto uploadDocumentsDto)
        {
            try
            {
                Guard.AssertArgumentNotNull(uploadDocumentsDto, nameof(uploadDocumentsDto));
                Guard.AssertArgumentNotNull(uploadDocumentsDto.Documents, nameof(uploadDocumentsDto.Documents));

                if (string.IsNullOrEmpty(uploadDocumentsDto.SynergyRequestId))
                {
                    throw new UserFriendlyException("SynergyRequestIdIsRequired");
                }

                if (string.IsNullOrEmpty(uploadDocumentsDto.CompanyId))
                {
                    throw new UserFriendlyException("CompanyIdIsRequired");
                }

                List<UploadFilesResponse> uploadFilesResponses = new List<UploadFilesResponse>();

                var configs = _portalConfigAppService.RetrievePortalConfiguration(new List<string>
                {
                    PortalConfigurations.SharePointUserIdForPortal
                });

                if (configs == null || configs.Count == 0)
                {
                    throw new Exception("SharePointUserIdIsNotConfigured");
                }

                var synergyFolderName = ConfigurationManager.AppSettings["SynergyFolderName"] ?? "Synergy";
                var companyFolderName = GetFormattedFolderName(uploadDocumentsDto.CompanyId);
                var requestFolderName = GetFormattedFolderName(uploadDocumentsDto.SynergyRequestId);

                uploadDocumentsDto.TargetFolderURL = $"/{synergyFolderName}/{companyFolderName}/{requestFolderName}";

                _fileService.CheckFolderStructure(companyFolderName, synergyFolderName);
                _fileService.CheckFolderStructure(requestFolderName, $"{synergyFolderName}/{companyFolderName}");

                string strUserDomainName = GetSharePointServiceAccount();

                var metadata = new Dictionary<string, object>();

                if (!string.IsNullOrEmpty(uploadDocumentsDto.SynergyRequestId))
                    metadata["SynergyRequestId"] = uploadDocumentsDto.SynergyRequestId;

                if (!string.IsNullOrEmpty(uploadDocumentsDto.CompanyId))
                    metadata["CompanyId"] = uploadDocumentsDto.CompanyId;

                if (!string.IsNullOrEmpty(uploadDocumentsDto.ContactId))
                    metadata["ContactId"] = uploadDocumentsDto.ContactId;

                if (!string.IsNullOrEmpty(uploadDocumentsDto.Description))
                    metadata["Description"] = uploadDocumentsDto.Description;

                foreach (var document in uploadDocumentsDto.Documents)
                {
                    CheckUploadDocumentValidations(document, uploadDocumentsDto.Documents.Count);

                    var uploadFilesResponse = _fileService.UploadFiles(
                        uploadDocumentsDto.TargetFolderURL,
                        document.DocumentName,
                        document.DocumentContent,
                        strUserDomainName,
                        metadata.Any() ? metadata : null);

                    uploadFilesResponses.Add(uploadFilesResponse);
                }

                return uploadFilesResponses;
            }
            catch (Exception ex)
            {
                throw new UserFriendlyException($"ErrorUploadingSynergyFiles: {ex.Message}");
            }
        }

        private string GetSharePointServiceAccount()
        {
            var encSPUsername = ConfigurationManager.AppSettings["SPUsername_ext"];
            var SPDomainname = ConfigurationManager.AppSettings["SPDomainname_ext"];

            if (string.IsNullOrEmpty(encSPUsername) || string.IsNullOrEmpty(SPDomainname))
            {
                throw new UserFriendlyException("SharePointCredentialsNotConfigured");
            }

            try
            {
                var SPUsername = Core.Helpers.CryptoUtils.Decrypt(encSPUsername);
                return $"{SPDomainname}\\{SPUsername}";
            }
            catch
            {
                throw new UserFriendlyException("SharePointCredentialsDecryptionFailed");
            }
        }

        private void ShareFileWithGroups(string targetUrl, string sharepointGroups)
        {
            try
            {
                var groups = sharepointGroups.Split(',');
                foreach (var item in groups)
                {
                    _fileService.ShareFileOrFolderWithGroup(targetUrl, item.Trim(), 6 /*Editor*/);
                }
            }
            catch (Exception ex)
            {
                throw new UserFriendlyException($"ErrorSharingFileWithGroups: {ex.Message}");
            }
        }

        private string GetUserDomainName(Guid userId)
        {
            string domainName = "";
            if (userId != Guid.Empty)
            {
                var columns = new Microsoft.Xrm.Sdk.Query.ColumnSet("domainname");
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

        private string GetFormattedFolderName(string requestId)
        {
            try
            {
                string guidString = requestId.Replace("-", string.Empty).ToUpper();
                return guidString;
            }
            catch (Exception)
            {
                throw new UserFriendlyException("InvalidRequestIdFormat");
            }
        }

        private void CheckUploadDocumentValidations(UploadedDocDetails uploadAttachment, int countOfDocuments)
        {
            var configurations = _portalConfigAppService.RetrievePortalConfiguration(new List<string>
            {
                PortalConfigurations.MaxFileSize,
                PortalConfigurations.MaxNumberOfUploadedFiles,
                PortalConfigurations.AllowedFilesPageFilesTypes
            });

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

        public async Task<List<DeleteFileResponse>> DeleteSynergyFiles(List<DeleteFileRequest> deleteFileRequests)
        {
            try
            {
                Guard.AssertArgumentNotNull(deleteFileRequests, nameof(deleteFileRequests));

                if (!deleteFileRequests.Any())
                {
                    throw new UserFriendlyException("NoFilesToDelete");
                }

                var siteName = ConfigurationManager.AppSettings["SPSiteName_ext"];
                var sitePrefix = $"/sites/{siteName}";

                foreach (var request in deleteFileRequests)
                {
                    if (request.SourceFilePath.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                        request.SourceFilePath.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                    {
                        try
                        {
                            var uri = new Uri(request.SourceFilePath);
                            request.SourceFilePath = uri.AbsolutePath;
                        }
                        catch (Exception ex)
                        {
                            throw new UserFriendlyException($"InvalidFileUrl: {ex.Message}");
                        }
                    }

                    if (request.SourceFilePath.StartsWith(sitePrefix, StringComparison.OrdinalIgnoreCase))
                    {
                        request.SourceFilePath = request.SourceFilePath.Substring(sitePrefix.Length);
                    }
                }

                return await Task.FromResult(_fileService.DeleteDocument(deleteFileRequests));
            }
            catch (Exception ex)
            {
                throw new UserFriendlyException($"ErrorDeletingSynergyFiles: {ex.Message}");
            }
        }

        public async Task<byte[]> DownloadSynergyDocument(string sourceFilePath)
        {
            try
            {
                Guard.AssertArgumentNotNull(sourceFilePath, nameof(sourceFilePath));

                if (string.IsNullOrWhiteSpace(sourceFilePath))
                {
                    throw new UserFriendlyException("DocumentPathShouldNotBeEmpty");
                }

                if (sourceFilePath.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                    sourceFilePath.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        var uri = new Uri(sourceFilePath);
                        sourceFilePath = uri.AbsolutePath;
                    }
                    catch (Exception ex)
                    {
                        throw new UserFriendlyException($"InvalidFileUrl: {ex.Message}");
                    }
                }

                var siteName = ConfigurationManager.AppSettings["SPSiteName_ext"];
                var sitePrefix = $"/sites/{siteName}";

                if (sourceFilePath.StartsWith(sitePrefix, StringComparison.OrdinalIgnoreCase))
                {
                    sourceFilePath = sourceFilePath.Substring(sitePrefix.Length);
                }

                return await Task.FromResult(_fileService.DowloadDocument(sourceFilePath));
            }
            catch (Exception ex)
            {
                throw new UserFriendlyException($"ErrorDownloadingSynergyFile: {ex.Message}");
            }
        }

    }
}
