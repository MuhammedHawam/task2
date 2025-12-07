using PIF.EBP.Application.Commercialization.DTOs;
using PIF.EBP.Application.Shared;

namespace PIF.EBP.Application.Commercialization
{
    public class CommercializationCacheItem : CacheItemBase, ICacheItem
    {
        public CustomizedItemDto CustomizedItem { get; set; }

    }
}
