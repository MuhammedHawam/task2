using Newtonsoft.Json;
using PIF.EBP.Application.Cache;
using PIF.EBP.Application.Shared;
using PIF.EBP.Core.Caching;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PIF.EBP.Application
{
    public class CacheManagerBase<TCacheItem> : CacheManagerBase<TCacheItem, string>
        where TCacheItem : CacheItemBase<string>
    {
        public CacheManagerBase(ITypedCache typedCache, ICrmQueriesBase queriesBase, string cacheName)
            : base(typedCache, queriesBase, cacheName)
        {

        }
        public CacheManagerBase(ITypedCache typedCache)
            : base(typedCache)
        {

        }
    }

    public class CacheManagerBase<TCacheItem, TPrimaryKey> : ICacheManagerBase<TCacheItem, TPrimaryKey>
        where TCacheItem : CacheItemBase<TPrimaryKey>
    {
        protected readonly ConnectionMultiplexer _redisConnection;
        protected readonly ICrmQueriesBase _queriesBase;
        protected readonly string _cacheName;

        public CacheManagerBase(ITypedCache typedCache, ICrmQueriesBase queriesBase, string cacheName)
        {
            _redisConnection = typedCache.Connection;
            _queriesBase = queriesBase;
            _cacheName = cacheName;
        }

        public CacheManagerBase(ITypedCache typedCache)
        {
            _redisConnection = typedCache.Connection;
        }

        public async Task ClearCacheAsync()
        {
            var redisDb = _redisConnection.GetDatabase();
            await redisDb.KeyDeleteAsync(_cacheName);
        }

        public async Task ClearCacheAsync(string cacheName)
        {
            var redisDb = _redisConnection.GetDatabase();
            await redisDb.KeyDeleteAsync(cacheName);
        }

        public async Task ClearAllCacheAsync()
        {
            var server = _redisConnection.GetServer(_redisConnection.GetEndPoints()[0]);
            var db = _redisConnection.GetDatabase();

            // Get all keys in the database
            var keys = server.Keys();

            // Delete each key
            foreach (var key in keys)
            {
                await db.KeyDeleteAsync(key);
            }
        }

        protected async Task SetItemsInCacheAsync(HashEntry[] hashEntries, string entityName = "")
        {
            var redisDb = _redisConnection.GetDatabase();
            string key = string.IsNullOrEmpty(entityName) ? _cacheName : $"{_cacheName}-{entityName}";
            await redisDb.HashSetAsync(key, hashEntries);
        }

        protected bool IsRedisConnected()
        {
            return _redisConnection.IsConnected;
        }

        protected async Task<HashEntry[]> GetAllInCacheAsync(string entityName = "")
        {
            if (IsRedisConnected())
            {
                var redisDb = _redisConnection.GetDatabase();
                string cacheKey = string.IsNullOrEmpty(entityName) ? _cacheName : $"{_cacheName}-{entityName}";
                return await redisDb.HashGetAllAsync(cacheKey);
            }
            return Array.Empty<HashEntry>();  // Return an empty array to ensure consistency and avoid null references
        }

        public async Task<List<T>> GetCachedItemAsync<T, TCacheItem>(string entityName = "", Func<TCacheItem, List<T>> selector = null)
                where T : ICacheItem
                where TCacheItem : CacheItemBase<string>
        {
            var lstOfItems = await GetAllInCacheAsync(entityName);

            if (lstOfItems.Any())
            {
                var oCacheItem = lstOfItems.Select(a => JsonConvert.DeserializeObject<TCacheItem>(a.Value))
                                 .FirstOrDefault(item => item != null);
                if (oCacheItem != null)
                {
                    if (selector == null)
                        return new List<T> { (T)(object)oCacheItem };
                    else
                        return selector(oCacheItem);
                }
            }

            if (IsRedisConnected())
            {
                var items = await _queriesBase.GetItemFromDataSource<TCacheItem, string>(entityName);
                var hashEntries = items.Select(i => new HashEntry(i.Id.ToString(), JsonConvert.SerializeObject(i))).ToArray();
                await SetItemsInCacheAsync(hashEntries, entityName);
                return selector != null ? selector(items.First()) : new List<T> { (T)(object)items.First() };
            }

            return await _queriesBase.GetFromDataSource<T>(entityName);
        }
    }
}
