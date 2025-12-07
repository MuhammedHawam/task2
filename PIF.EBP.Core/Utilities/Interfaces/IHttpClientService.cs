using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace PIF.EBP.Core.Utilities.Interfaces
{
    public interface IHttpClientService
    {
        Task<string> GetAsync(string url);
        Task<(byte[], string, JObject)> GetByteAsync(string url);
        Task<(bool, string)> PostAsync(string url, HttpContent content);
        Task<string> PutAsync(string url, HttpContent content);
        Task<string> DeleteAsync(string url);
        Task<(bool, string)> SendAsync(HttpRequestMessage request);
    }
}
