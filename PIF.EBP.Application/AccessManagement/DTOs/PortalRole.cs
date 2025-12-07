using PIF.EBP.Application.MetaData.DTOs;
using PIF.EBP.Application.Shared;

namespace PIF.EBP.Application.AccessManagement.DTOs
{
    public class PortalRole : ICacheItem
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string NameAr { get; set; }
        public bool ShowInternal { get; set; }
        public bool ShowExternal { get; set; }
        public bool IsAdmin { get; set; }
        public EntityOptionSetDto RoleType  { get; set; }
        public bool IsViewer { get; set; }
        public bool IsAdminIT { get; set; }
        public EntityReferenceDto Department { get; set; }
        public EntityReferenceDto ParentportalRole { get; set; }
    }
}
