using PIF.EBP.Application.MetaData.DTOs;
using PIF.EBP.Application.Shared;
using System;

namespace PIF.EBP.Application.EntitiesCache.DTOs
{
    public class ProcessStatusTemplateDto : ICacheItem
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string NameAr { get; set; }
        public EntityOptionSetDto Type { get; set; }

    }
}
