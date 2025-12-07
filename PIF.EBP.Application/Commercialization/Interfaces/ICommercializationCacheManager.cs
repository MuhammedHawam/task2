using PIF.EBP.Core.DependencyInjection;
using System.Threading.Tasks;

namespace PIF.EBP.Application.Commercialization.Interfaces
{
    public interface ICommercializationCacheManager : ICacheManagerBase<CommercializationCacheItem>, IScopedDependency
    {
        Task<CommercializationCacheItem> GetCustomizedServiceCacheItemAsync();
    }
}
