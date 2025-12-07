using PIF.EBP.Core.DependencyInjection;
using PIF.EBP.Core.FileManagement.DTOs;
using System.Threading.Tasks;

namespace PIF.EBP.Core.FileScanning
{
    public interface IExternalFileScanningService : ITransientDependency
    {
        Task<string> AnalyzeFile(byte[] fileContent, string fileName,object metaData = null);
        Task<UploadDocumentsDto> GetFileResult(string content,string dataId);        
        void WriteToFile(string fileName, string content);
    }
}
