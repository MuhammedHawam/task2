using System;
using System.Configuration;

namespace PIF.EBP.Application.Cache
{
    public class CacheEnum
    {
        public string CacheName { get; }
        public Type CacheManagerType { get; }

        private CacheEnum(string cacheName, Type cacheManagerType)
        {
            var baseCacheName = ConfigurationManager.AppSettings["BaseCacheName"];
            if (string.IsNullOrEmpty(baseCacheName))
            {
                CacheName = cacheName;
            }
            else
            {
                CacheName = $"{baseCacheName}-{cacheName}";
            }

            CacheManagerType = cacheManagerType;
        }

        public static CacheEnum MetaData = new CacheEnum("MetaDataCache", null);
        public static CacheEnum Entity = new CacheEnum("EntityCache", null);
        public static CacheEnum AccessManagement = new CacheEnum("AccessManagementCache",null);
        public static CacheEnum Commercialization = new CacheEnum("CommercializationCache", null);

    }
}
