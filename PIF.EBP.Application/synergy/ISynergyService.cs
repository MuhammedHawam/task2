using PIF.EBP.Application.Settings.DTOs;
using PIF.EBP.Application.Synergy.DTOs;
using PIF.EBP.Core.DependencyInjection;
using PIF.EBP.Core.FileManagement.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIF.EBP.Application.Synergy
{
    public interface ISynergyService : ITransientDependency
    {
        List<CompaniesLogosDto> GetCompaniesLogos(List<Guid> companiesIds);
        Task<SynergyUserDataDto> RetrieveUserProfileDataAndSector();
        List<SectorLookupDto> GetSectors();
        Task<List<UploadFilesResponse>> SynergyRequestUpload(UploadDocumentsDto uploadDocumentsDto);
        Task<List<DeleteFileResponse>> DeleteSynergyFiles(List<DeleteFileRequest> deleteFileRequests);
        Task<byte[]> DownloadSynergyDocument(string sourceFilePath);
    }
}
