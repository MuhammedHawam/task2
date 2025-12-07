using PIF.EBP.Core.DependencyInjection;
using PIF.EBP.Core.FileManagement.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PIF.EBP.Core.FileManagement
{
    public interface IFileManagement : ITransientDependency
    {
        Task<DocumentInfo> RetrieveDocumentAsync(RetrieveDocumentDto oRetrieveDocumenDto);
        int SPDeleteFile(List<string> filesFullPath, string user, string crmSharepointUrl, string sharepointLibraryName);
        void UploadFileToSPLocation(string relativePath, string fileName,
            string base64File, string user);
        UploadFilesResponse UploadFiles(string relativePath, string fileName,
                                        string base64File, string user, Dictionary<string, object> Metadata = null);
        void GetFolderPathAndItemCount(string fullRelativeUrl, out string folderPath);
        void AddFolderToSpRootLibrary(string entityLibrary, string folderName);
        int SPListItemsCount(string libraryName, string spList);
        List<GetDocumentListRsp> SPListItems(string targetFileURL);
        List<GetDocumentListRsp> GetDocumentsByTargetUrl(string targetURL);
        List<GetDocumentListRsp> SPListItems(string libraryName, string list,
            bool isArchive, string user);
        GetDocumentRsp SPGetItem(string libraryName, string DocName, string list, string user);
        GetDocumentRsp SPGetItem(string folderPath, string docName, string user);
        int CopyDocumentFiles(CopyFiles document, string source, string skip, string user,
                              string sourceLogical, string destinationObjectName, out int itemCount, ref string relativeUrlFolderPath);
        GetDocumentListRspBase SearchDocuments(SearchDocumentListDto searchDocumentListDto);
        GetDocumentListRspBase GetDocuments(GetDocumentListDto searchDocumentListDto);
        List<RenameFilesResponse> RenameDocuments(List<RenameFilesDto> renameFilesDtos, Guid contactId);
        List<MoveFilesResponse> MoveDocuments(List<MoveFilesDto> moveFilesDtos, Guid contactId);
        void CheckFolderStructure(string folderName, string rootFolderPath);
        List<DeleteFileResponse> DeleteDocument(List<DeleteFileRequest> deleteFileRequest);
        List<CopyFileResponse> CopyDocuments(List<CopyFileDto> copyFileRequest, Guid contactId);
        byte[] DowloadDocument(string fileUrl);
        string ShareFileOrFolder(string fileUrl, string userEmail, int role);
        string ShareFileOrFolderWithGroup(string fileUrl, string groupName, int role);
        bool CheckIfPathExistsInSharePoint_Ext(string spPath);
        void AddSubFolderWithMetaData(string folderName, string compFolderPath, Dictionary<string, object> metaDataDic);
        void EnsureAllDataCollCustomFieldExists(string companyLibDisplayName, string contactLibDisplayName);
        void EnsureAllKnowledgeHubCustomFieldExists(string knowledgeHubLibrary);
        string GetFileTypeByFileUrl(string fileUrl);
        AttendeeEventDetailsDto RetrieveAttendeesDetailsByRefId(string refId, string listName);
        bool UpdateAttendeesDetails(string listName, string refId, string newRSVPValue, string guestName, string guestRole);
    }
}
