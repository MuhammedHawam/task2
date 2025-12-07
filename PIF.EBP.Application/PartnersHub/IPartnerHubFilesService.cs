using PIF.EBP.Core.DependencyInjection;
using PIF.EBP.Core.FileManagement.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIF.EBP.Application.Innovation
{
    public interface IPartnerHubFilesService : ITransientDependency
    {
        Task<List<UploadFilesResponse>> RequestUpload(UploadDocumentsDto uploadDocumentsDto);
        Task<List<DeleteFileResponse>> DeleteFiles(List<DeleteFileRequest> deleteFileRequests);
        Task<byte[]> DownloadDocument(string sourceFilePath);
    }
}
