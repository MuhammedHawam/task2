using System.Collections.Generic;

namespace PIF.EBP.Application.AccessManagement.DTOs
{
    public class RolesPermissionDto
    {
        public string Email { get; set; }
        public string CompanyId { get; set; }
        public string RoleId { get; set; }
        public bool ShowExternal { get; set; }
        public bool ShowInternal { get; set; }
        public string AssociationId { get; set; }
        public List<PortalPermissionDto> PortalPermissions { get; set; } = new List<PortalPermissionDto>();
    }
    public class PortalPermissionDto
    {
        public PageDto PortalPage { get; set; }
        public List<PermissionDto> Permissions { get; set; } = new List<PermissionDto>();
    }
    public class PermissionDto
    {
        public string Name { get; set; }
        public int? Read { get; set; }
        public int? Create { get; set; }
        public int? Write { get; set; }
        public int? Delete { get; set; }
        public string ServiceId { get; set; }
    }
    public class PageDto
    {
        public string Name { get; set; }
        public string NameAr { get; set; }
        public string Link { get; set; }
    }
    public class AuthPermission
    {
        public int Read { get; set; }
        public int Create { get; set; }
        public int Write { get; set; }
        public int Delete { get; set; }
        public string PageLink { get; set; }
        public string ServiceId { get; set; }
        public string Name { get; set; }
    }
}
