using PIF.EBP.Application.AccessManagement.DTOs;
using PIF.EBP.Application.Shared;
using System.Collections.Generic;

namespace PIF.EBP.Application.AccessManagement
{
    public class AccessManagementCacheItem : CacheItemBase, ICacheItem
    {
        public List<PortalRole> PortalRolesList { get; set; } = new List<PortalRole>();
        public List<PortalPermission> PortalPermissionsList { get; set; } = new List<PortalPermission>();
        public List<PortalPage> PortalPagesList { get; set; } = new List<PortalPage>();
    }
}
