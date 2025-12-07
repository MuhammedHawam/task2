using PIF.EBP.Core.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PIF.EBP.Application.AccessManagement
{
    public interface IUserPermissionAppService : ITransientDependency
    {
        Task<bool> CheckLoggedInUserPermissions(Guid? contactId = null, Guid? roleId = null);
        bool CheckReadUserPermissions(Guid? contactId = null, Guid? roleId = null);
        bool IsLoggedInUserIsAdmin();
        Task<bool> IsLoggedInUserIsBoardMember();
        Task<int> LoggedInUserRoleType();
        Task<List<string>> GetViewerRoles();
        Task<List<string>> GetBoardMemberRoles();
        Task<bool> CheckRoleHierarchyByRoleId(string roleId);
        Task<bool> CheckDeepAccessForTheRole(string roleId);
    }
}
