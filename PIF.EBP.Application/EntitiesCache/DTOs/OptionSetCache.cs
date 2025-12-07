using PIF.EBP.Application.MetaData.DTOs;
using PIF.EBP.Application.Shared;
using System.Collections.Generic;

namespace PIF.EBP.Application.EntitiesCache.DTOs
{
    public class OptionSetCache: ICacheItem
    {
        public string EntityName { get; set; }
        public string AttributeName { get; set; }
        public List<EntityOptionSetDto> OptionSets { get; set; }
    }
}
