using PIF.EBP.Application.Cache;
using PIF.EBP.Application.MetaData.DTOs;
using PIF.EBP.Core.Caching;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PIF.EBP.Application.MetaData.Implementation
{
    public class MetadataCacheManager : CacheManagerBase<MetadataCacheItem>, IMetadataCacheManager
    {
        public MetadataCacheManager(ITypedCache cacheService, IMetadataCrmQueries queriesBase) :
            base(cacheService, queriesBase, CacheEnum.MetaData.CacheName)
        {

        }
        public async Task<List<EntityAttributeDto>> GetCachedEntityAttributesAsync(string entityName)
        {
            return await GetCachedItemAsync<EntityAttributeDto, MetadataCacheItem>(entityName, i => i.AttributeList);
        }

        public async Task<List<EntityRelationshipDto>> GetCachedEntityRelationshipsAsync(string entityName)
        {
            return await GetCachedItemAsync<EntityRelationshipDto, MetadataCacheItem>(entityName, i => i.RelationshipList);
        }
        public async Task<List<EntityFormDto>> GetCachedEntityFormsAsync(string entityName)
        {
            return await GetCachedItemAsync<EntityFormDto, MetadataCacheItem>(entityName, i => i.FormList);
        }
        public async Task<List<EntityViewDto>> GetCachedEntityViewsAsync(string entityName)
        {
            return await GetCachedItemAsync<EntityViewDto, MetadataCacheItem>(entityName, i => i.ViewList);
        }
    }
}
