using PIF.EBP.Application.Cache;
using PIF.EBP.Core.Caching;
using System.Threading.Tasks;
using PIF.EBP.Application.Commercialization.Interfaces;
using System.Linq;

namespace PIF.EBP.Application.Commercialization.Implementation
{
    public class CommercializationCacheManager : CacheManagerBase<CommercializationCacheItem>, ICommercializationCacheManager
    {
        public CommercializationCacheManager(ITypedCache cacheService, ICommercializationQueries queriesBase)
            : base(cacheService, queriesBase, CacheEnum.Commercialization.CacheName)
        {
        }
        public async Task<CommercializationCacheItem> GetCustomizedServiceCacheItemAsync()
        {
            var cachedItems = await GetCachedItemAsync<CommercializationCacheItem, CommercializationCacheItem>("CustomizedServices", null);
            return cachedItems.FirstOrDefault();
        }
    }
}
