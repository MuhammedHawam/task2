using PIF.EBP.Application.MetaData;
using PIF.EBP.Core.DependencyInjection;
using System.Threading.Tasks;
using System;

namespace PIF.EBP.Application.Cache
{
    public interface ICacheManager : ICacheManagerBase<MetadataCacheItem>, IScopedDependency
    {
        Task ClearAllCache();
        Task ClearCache(string cacheName);
        Task<T> GetObjectFromCacheAsync<T>(string key);
        Task SetObjectInCacheAsync<T>(string key, T value, TimeSpan? expiry = null);
    }
}
