using PIF.EBP.Application.MetaData.DTOs;
using PIF.EBP.Core.DependencyInjection;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PIF.EBP.Application.MetaData
{
    public interface IMetadataCacheManager : ICacheManagerBase<MetadataCacheItem>,IScopedDependency
    {
        Task<List<EntityAttributeDto>> GetCachedEntityAttributesAsync(string entityName);
        Task<List<EntityRelationshipDto>> GetCachedEntityRelationshipsAsync(string entityName);
        Task<List<EntityFormDto>> GetCachedEntityFormsAsync(string entityName);
        Task<List<EntityViewDto>> GetCachedEntityViewsAsync(string entityName);
    }
}
