using PIF.EBP.Application.MetaData;
using PIF.EBP.Core.Caching;
using System.Threading.Tasks;
using System;
using StackExchange.Redis;
using Newtonsoft.Json;

namespace PIF.EBP.Application.Cache.Implementation
{
    public class CacheManager : CacheManagerBase<MetadataCacheItem>, ICacheManager
    {
        private readonly ConnectionMultiplexer _redisConnection;

        public CacheManager(ITypedCache cacheService) : base(cacheService)
        {
            _redisConnection = ConnectionMultiplexer.Connect("localhost"); // Replace with your Redis connection string
        }

        public async Task ClearAllCache()
        {
            await ClearAllCacheAsync();
        }
        public async Task ClearCache(string cacheName)
        {
            await ClearCacheAsync(cacheName);
        }

        public async Task<T> GetObjectFromCacheAsync<T>(string key)
        {
            var redisDb = _redisConnection.GetDatabase();
            var cachedValue = await redisDb.StringGetAsync(key);
            if (cachedValue.HasValue)
                return JsonConvert.DeserializeObject<T>(cachedValue);
            return default;
        }
        protected bool IsRedisConnected()
        {
            return _redisConnection.IsConnected;
        }

        public async Task SetObjectInCacheAsync<T>(string key, T value, TimeSpan? expiry = null)
        {
            var redisDb = _redisConnection.GetDatabase();
            await redisDb.StringSetAsync(key, JsonConvert.SerializeObject(value), expiry);
        }
    }
}