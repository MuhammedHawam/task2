using PIF.EBP.Application.Cache;
using PIF.EBP.Core.Caching;
using System.Linq;
using System.Threading.Tasks;

namespace PIF.EBP.Application.AccessManagement.Implementation
{
    public class AccessManagementCacheManager : CacheManagerBase<AccessManagementCacheItem>, IAccessManagementCacheManager
    {
        public AccessManagementCacheManager(ITypedCache cacheService, IAccessManagementCrmQueries queriesBase) :
            base(cacheService, queriesBase, CacheEnum.AccessManagement.CacheName)
        {

        }
        public async Task<AccessManagementCacheItem> GetAccessManagementCacheItem()
        {
            var cachedItems = await GetCachedItemAsync<AccessManagementCacheItem, AccessManagementCacheItem>(string.Empty, null);

            return cachedItems.FirstOrDefault();
        }
    }
}
