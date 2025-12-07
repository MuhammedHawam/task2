using PIF.EBP.Application.MetaData.DTOs;
using PIF.EBP.Application.Shared;
using System.Collections.Generic;

namespace PIF.EBP.Application.MetaData
{
    public class MetadataCacheItem : CacheItemBase, ICacheItem
    {
        public List<EntityAttributeDto> AttributeList { get; set; } = new List<EntityAttributeDto>();

        public List<EntityRelationshipDto> RelationshipList { get; set; } = new List<EntityRelationshipDto>();
        public List<EntityFormDto> FormList { get; set; } = new List<EntityFormDto>();
        public List<EntityViewDto> ViewList { get; set; } = new List<EntityViewDto>();
    }
}
