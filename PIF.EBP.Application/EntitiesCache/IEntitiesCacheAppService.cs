using Microsoft.Xrm.Sdk;
using PIF.EBP.Application.EntitiesCache.DTOs;
using PIF.EBP.Application.MetaData.DTOs;
using PIF.EBP.Core.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PIF.EBP.Application.EntitiesCache
{
    public interface IEntitiesCacheAppService : ITransientDependency
    {
        EntityReferenceDto RetrieveEntityCacheById(string entityName, Guid? entityId);
        Task<List<PortalConfigDto>> RetrievePortalconfigurations();
        EntityOptionSetDto RetrieveOptionSetCacheByKeyWithValue(string key, OptionSetValue value);
        List<EntityOptionSetDto> RetrieveOptionSetCacheByKey(string key);
    }
}
