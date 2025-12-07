using PIF.EBP.Application.EntitiesCache.DTOs;
using PIF.EBP.Core.DependencyInjection;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PIF.EBP.Application.EntitiesCache
{
    public interface IEntitiesCacheManager:ICacheManagerBase<EntitiesCacheItem>, IScopedDependency
    {
        Task<List<PortalConfigDto>> GetCachedPortalConfigsAsync();
        Task<List<StepStatusTemplateDto>> GetCachedStepStatusTemplateAsync();
        Task<List<ProcessStatusTemplateDto>> GetCachedProcessStatusTemplateAsync();
        Task<List<OptionSetCache>> GetCachedOptioSetsAsync();
    }
}
