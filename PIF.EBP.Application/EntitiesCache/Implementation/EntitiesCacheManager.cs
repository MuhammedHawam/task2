using PIF.EBP.Application.Cache;
using PIF.EBP.Application.EntitiesCache.DTOs;
using PIF.EBP.Application.Shared;
using PIF.EBP.Core.Caching;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PIF.EBP.Application.EntitiesCache.Implementation
{
    public class EntitiesCacheManager : CacheManagerBase<EntitiesCacheItem>, IEntitiesCacheManager
    {
        public EntitiesCacheManager(ITypedCache cacheService, IEntitiesCrmQueries queriesBase)
            : base(cacheService, queriesBase, CacheEnum.Entity.CacheName)
        {
        }
        public async Task<List<PortalConfigDto>> GetCachedPortalConfigsAsync()
        {
            return await GetCachedItemAsync<PortalConfigDto, EntitiesCacheItem>(EntityNames.Portalconfiguration, i => i.PortalConfigsList);
        }
        public async Task<List<StepStatusTemplateDto>> GetCachedStepStatusTemplateAsync()
        {
            return await GetCachedItemAsync<StepStatusTemplateDto, EntitiesCacheItem>(EntityNames.HexaStepStatusTemplate, i => i.StepStatusTemplateList);
        }
        public async Task<List<ProcessStatusTemplateDto>> GetCachedProcessStatusTemplateAsync()
        {
            return await GetCachedItemAsync<ProcessStatusTemplateDto, EntitiesCacheItem>(EntityNames.ProcessStatusTemplate, i => i.ProcessStatusTemplateList);
        }
        public async Task<List<OptionSetCache>> GetCachedOptioSetsAsync()
        {
            return await GetCachedItemAsync<OptionSetCache, EntitiesCacheItem>(EntityNames.OptionSet, i => i.OptionSetList);
        }
    }
}
