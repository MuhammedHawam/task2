using PIF.EBP.Core.DependencyInjection;
using PIF.EBP.Core.FileManagement.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PIF.EBP.Application.InfraBase
{
    public interface IInfraBaseFileUploaderService : ITransientDependency
    {
        Task<List<UploadFilesResponse>> InfraBaseRequestUpload(UploadDocumentsDto uploadDocumentsDto);
        Task<List<DeleteFileResponse>> DeleteInfraBaseFiles(List<DeleteFileRequest> deleteFileRequests);
        Task<byte[]> DownloadInfraBaseDocument(string sourceFilePath);
    }
}
