using PIF.EBP.Core.DependencyInjection;
using PIF.EBP.Core.FileManagement.DTOs;
using System.Threading.Tasks;

namespace PIF.EBP.Application.Sharepoint
{
    public interface IHexaDocumentManagementService : ITransientDependency
    {
        Task<DocumentHeader> UploadAttachmentsAsync(UploadDocumentsDto uploadFile);
        Task<DeleteDocumentsRes> DeleteAttachmentsAsync(DeleteDocumentsDto deleteDocumentRqt);
        Task<GetDocumentListRspBase> RetrieveDocumentsList(GetDocumentsDto getDocuments);
        Task<GetDocumentRsp> GetAttachmentAsync(GetDocumentDto getDocument);
        Task<CopyFilesResponse> CopyDocumentFiles(CopyFiles copyFiles);
        Task<DocumentHeader> RetrieveAttachmentsDetailsAsync(UploadDocumentsDto uploadFile);
    }
}
