using PIF.EBP.Application.AccessManagement.DTOs;
using PIF.EBP.Application.MetaData.DTOs;
using PIF.EBP.Core.DependencyInjection;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PIF.EBP.Application.AccessManagement
{
    public interface IAccessManagementAppService : ITransientDependency
    {
        Task<List<RolesPermissionDto>> GetRolesAndPermissionByContactId();
        Task<List<EntityReferenceDto>> GetAssociatedRoles(string contactId = "", string companyId = "");
        Task<List<EntityReferenceDto>> GetCompaniesByContactId(string contactId);
        Task<List<ContactRole>> GetContactRolesByContactId(string contactId);
        Task<List<AuthPermission>> GetAuthorizedPermissions();
        Task<string> GetParentPortalRole(string roleId, string departmentId);
        List<ContactRole> GetActiveRolesAssociationForSignedInContact();
        Task<List<ContactRole>> GetActiveRolesAssociationForSignedInContactWithDepartments();
        Task<List<AuthPermission>> GetPermissionsByRoleIdAsync(string roleId);
        Task<string> GetParentRoleByRoleId(string roleId);
        Task<PortalRole> GetRoleAndDepartmentByRoleId(string roleId);
        List<ContactRole> GetContactRoles(string contactId, string companyId = null);
        Task<bool> GetIsAdminRoleByRoleId(string roleId);
        Task<bool> GetIsAdminOrAdminITRoleByRoleId(string roleId);
        Task<bool> GetIsBoardMemberRoleByRoleId(string roleId);
        Task<int> GetLoggedInUserRoleType(string roleId);
        Task<List<string>> GetViewerRoles();
        Task<List<string>> GetBoardMemberRoles();
        List<ContactRole> GetDeletedConactRoleAssociations(string contactId, string companyId = null);
        string GetContactParentRole(string contactId, string companyId = null);
    }
}
