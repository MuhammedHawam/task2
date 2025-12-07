using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIF.EBP.Application.Contacts.Dtos
{
    public class PortalPermissionDto
    {
        public PortalPermissionType PortalRole {  get; set; }
        public Guid PortalRoleId { get; set; }
        public string PortalRoleName { get; set; }
    }

    public enum PortalPermissionType {None,Admin,Contributor,Viewer }
}
