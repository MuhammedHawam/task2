using PIF.EBP.Core.DependencyInjection;
using PIF.EBP.Core.FileManagement.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PIF.EBP.Application.DataCollection
{
    public interface IDataCollectionService : ITransientDependency
    {
        Task<List<FolderStructureRes>> GetDataCollectionFolders(string companyId, string contactId, bool isForCrm = false);
        Task<GetDocumentListRspBase> GetDocuments(GetDocumentListDto searchDocumentListDto);
        Task<GetDocumentListRspBase> SearchDocuments(SearchDocumentListDto searchDocumentListDto);
        Task<List<UploadFilesResponse>> UploadDocuments(UploadDocumentsDto uploadDocumentsDto);
        Task<List<RenameFilesResponse>> RenameDocuments(List<RenameFilesDto> renameFilesDtos);
        Task<List<MoveFilesResponse>> MoveDocuments(List<MoveFilesDto> moveFilesDtos);
        Task<List<DeleteFileResponse>> DeleteDocuments(List<DeleteFileRequest> deleteFileRequest);
        Task<List<CopyFileResponse>> CopyDocuments(List<CopyFileDto> copyFileRequest);
        Task<(byte[], string)> DownloadDocument(string sourceFilePath);

    } 
}
