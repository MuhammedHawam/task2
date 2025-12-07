using PIF.EBP.Application.EntitiesCache;
using PIF.EBP.Application.Shared;
using PIF.EBP.Core.Exceptions;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace PIF.EBP.Application.Cache.Implementation
{
    public class CacheAppService : ICacheAppService
    {
        private readonly IEntitiesCacheAppService _entitiesCacheAppService;
        private readonly ICacheManager _cacheManager;
        public CacheAppService(
            IEntitiesCacheAppService entitiesCacheAppService,
            ICacheManager cacheManager)
        {
            _entitiesCacheAppService = entitiesCacheAppService;
            _cacheManager=cacheManager;

        }
        public async Task ClearEntityCache(string entityName)
        {
            await _cacheManager.ClearCache(CacheEnum.Entity.CacheName+"-"+entityName);

        }
        public async Task ClearMetaDataCache(string entityName)
        {
            await _cacheManager.ClearCache(CacheEnum.MetaData.CacheName+"-"+entityName);

        }
        public async Task ClearAccessManagementCache()
        {
            await _cacheManager.ClearCache(CacheEnum.AccessManagement.CacheName);

        }
        public async Task ClearAllCache()
        {
            await _cacheManager.ClearAllCache();
            var targetNode = ConfigurationManager.AppSettings["TargetNodeURL"];
            if (!string.IsNullOrEmpty(targetNode))
            {
                using (var httpClient = new HttpClient())
                {
                    string key = await GetAPIKeyFromPortalConfiguration();

                    httpClient.DefaultRequestHeaders.Add(ConfigurationManager.AppSettings["IdentityServerAPIKeyName"], key);
                    string requesturi = targetNode + "/" + "Cache/clear-all-cache-for-node";
                    var response = await httpClient.PostAsync(requesturi, null);
                    if (!response.IsSuccessStatusCode)
                    {
                        throw new UserFriendlyException("MsgUnexpectedError");
                    }
                }

            }
        }

        private async Task<string> GetAPIKeyFromPortalConfiguration()
        {
            var portalConfigs = await _entitiesCacheAppService.RetrievePortalconfigurations();
            var portalConfig = portalConfigs?.FirstOrDefault(x => x.Key == PortalConfigurations.CrmIntegrationApiKey);
            var key = portalConfig?.Value ?? string.Empty;
            return key;
        }

        public async Task ClearAllCacheForNode()
        {
            await _cacheManager.ClearAllCache();
        }
    }
}
