using PIF.EBP.Application.PortalConfiguration;
using PIF.EBP.Application.Settings;
using PIF.EBP.Application.Shared;
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

namespace PIF.EBP.Application.Innovation.Implmentation
{
    public class PartnerHubFilesService : IPartnerHubFilesService
    {
        private readonly IUserProfileAppService _userProfileAppService;
        private readonly ISessionService _sessionService;
        private readonly IPortalConfigAppService _portalConfigAppService;
        private readonly ICrmService _crmService;
        private readonly IFileManagement _fileService;


        public PartnerHubFilesService(ICrmService crmService, 
                                 ISessionService sessionService,
                                 IPortalConfigAppService portalConfigAppService, 
                                 IFileManagement fileService,
                                 IUserProfileAppService userProfileAppService)
        {
            _userProfileAppService = userProfileAppService;
            _sessionService = sessionService ?? throw new ArgumentNullException(nameof(sessionService));
            _portalConfigAppService = portalConfigAppService ?? throw new ArgumentNullException(nameof(portalConfigAppService));
            _fileService = fileService;
            _crmService = crmService ?? throw new ArgumentNullException(nameof(crmService));
        }
        public async Task<List<UploadFilesResponse>> RequestUpload(UploadDocumentsDto uploadDocumentsDto)
        {
            try
            {
                Guard.AssertArgumentNotNull(uploadDocumentsDto, nameof(uploadDocumentsDto));
                Guard.AssertArgumentNotNull(uploadDocumentsDto.Documents, nameof(uploadDocumentsDto.Documents));

                if (string.IsNullOrEmpty(uploadDocumentsDto.RequestId))
                {
                    throw new UserFriendlyException("RequestIdIsRequired");
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

                var FolderName = ConfigurationManager.AppSettings[uploadDocumentsDto.ModuleName+"FolderName"] ?? uploadDocumentsDto.ModuleName;
                var companyFolderName = GetFormattedFolderName(uploadDocumentsDto.CompanyId);
                var requestFolderName = GetFormattedFolderName(uploadDocumentsDto.RequestId);

                uploadDocumentsDto.TargetFolderURL = $"/{FolderName}/{companyFolderName}/{requestFolderName}";

                _fileService.CheckFolderStructure(companyFolderName, FolderName);
                _fileService.CheckFolderStructure(requestFolderName, $"{FolderName}/{companyFolderName}");

                string strUserDomainName = GetSharePointServiceAccount();

                var metadata = new Dictionary<string, object>();

                if (!string.IsNullOrEmpty(uploadDocumentsDto.RequestId))
                    metadata["RequestId"] = uploadDocumentsDto.RequestId;

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
                throw new UserFriendlyException($"ErrorUploadingInnovationFiles: {ex.Message}");
            }
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

        public async Task<List<DeleteFileResponse>> DeleteFiles(List<DeleteFileRequest> deleteFileRequests)
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
                throw new UserFriendlyException($"ErrorDeletingFiles: {ex.Message}");
            }
        }

        public async Task<byte[]> DownloadDocument(string sourceFilePath)
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
                throw new UserFriendlyException($"ErrorDownloadingFile: {ex.Message}");
            }
        }
    }
}
