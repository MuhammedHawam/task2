using PIF.EBP.Application.MetaData.DTOs;
using PIF.EBP.Application.Shared;

namespace PIF.EBP.Application.EntitiesCache.DTOs
{
    public class PortalConfigDto : ICacheItem
    {
        public string Key { get; set; }
        public string Value { get; set; }
        public string ValueAr { get; set; }
        public EntityOptionSetDto Type { get; set; }
    }
}
