using PIF.EBP.Application.Shared;

namespace PIF.EBP.Application.MetaData.DTOs
{
    public class EntityRelationshipDto : ICacheItem
    {
        public string MetadataId { get; set; }
        public string Name { get; set; }
        public int Type { get; set; }
        public string ReferencingEntity { get; set; }
        public string ReferencedEntity { get; set; }
        public string ReferencingAttribute { get; set; }
        public string ReferencedAttribute { get; set; }
        public string Entity1IntersectAttribute { get; set; }
        public string Entity2IntersectAttribute { get; set; }
        public string Entity1LogicalName { get; set; }
        public string Entity2LogicalName { get; set; }
        public string IntersectEntityName { get; set; }
        public string RefAttribute { get; set; }
        public string RefEntity { get; set; }
    }

}
