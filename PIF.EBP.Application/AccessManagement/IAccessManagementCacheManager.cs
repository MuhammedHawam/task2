using PIF.EBP.Core.DependencyInjection;
using System.Threading.Tasks;

namespace PIF.EBP.Application.AccessManagement
{
    public interface IAccessManagementCacheManager : ICacheManagerBase<AccessManagementCacheItem>, IScopedDependency
    {
        Task<AccessManagementCacheItem> GetAccessManagementCacheItem();
    }
}
