using PIF.EBP.Core.DependencyInjection;
using System.Threading.Tasks;

namespace PIF.EBP.Application.Cache
{
    public interface ICacheAppService : ITransientDependency
    {
        Task ClearEntityCache(string entityName);
        Task ClearMetaDataCache(string entityName);
        Task ClearAccessManagementCache();
        Task ClearAllCache();
        Task ClearAllCacheForNode();
    }
}
