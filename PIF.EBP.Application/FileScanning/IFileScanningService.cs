using PIF.EBP.Core.DependencyInjection;
using System.Threading.Tasks;

namespace PIF.EBP.Application.FileScanning
{
    public interface IFileScanningService : ITransientDependency
    {
        Task<string> AnalyzeFile(byte[] fileContent, string fileName, object metaData = null);
        Task GetFileResult(string content, string dataId);
        void WriteToFile(string fileName, string content);
    }
}
