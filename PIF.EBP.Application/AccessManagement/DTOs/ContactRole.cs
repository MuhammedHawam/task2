using PIF.EBP.Application.MetaData.DTOs;
using PIF.EBP.Application.Shared;

namespace PIF.EBP.Application.AccessManagement.DTOs
{
    public class ContactRole : ICacheItem
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public int? AssociationStatus { get; set; }
        public EntityReferenceDto Department { get; set; }
        public EntityReferenceDto Company { get; set; }
        public EntityReferenceDto Contact { get; set; }
        public EntityReferenceDto PortalRole { get; set; }
        public PortalRole PortalRoleEntity { get; set; }
        
    }
}
