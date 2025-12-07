using PIF.EBP.Application.EntitiesCache.DTOs;
using PIF.EBP.Application.Shared;
using System.Collections.Generic;

namespace PIF.EBP.Application.EntitiesCache
{
    public class EntitiesCacheItem : CacheItemBase, ICacheItem
    {
        public List<PortalConfigDto> PortalConfigsList { get; set; } = new List<PortalConfigDto>();
        public List<StepStatusTemplateDto> StepStatusTemplateList { get; set; } = new List<StepStatusTemplateDto>();
        public List<ProcessStatusTemplateDto> ProcessStatusTemplateList { get; set; } = new List<ProcessStatusTemplateDto>();
        public List<OptionSetCache> OptionSetList { get; set; } = new List<OptionSetCache>();

    }
}
