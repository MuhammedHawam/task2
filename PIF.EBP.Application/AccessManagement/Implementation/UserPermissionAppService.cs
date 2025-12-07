using PIF.EBP.Application.AccessManagement.DTOs;
using PIF.EBP.Application.PortalConfiguration;
using PIF.EBP.Core.CRM;
using PIF.EBP.Core.Session;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PIF.EBP.Application.AccessManagement.Implementation
{
    public class UserPermissionAppService : IUserPermissionAppService
    {
        private readonly ISessionService _sessionService;
        private readonly IAccessManagementAppService _accessMangementService;
        private readonly IPortalConfigAppService _portalConfigAppService;

        public UserPermissionAppService(ICrmService crmService, ISessionService sessionService, IAccessManagementAppService accessMangementService, IPortalConfigAppService portalConfigAppService)
        {
            _sessionService = sessionService;
            _accessMangementService = accessMangementService;
            _portalConfigAppService = portalConfigAppService;
        }

        public async Task<bool> CheckLoggedInUserPermissions(Guid? contactId = null, Guid? roleId = null)
        {
            if (contactId.HasValue)
            {
                if (new Guid(_sessionService.GetContactId()) == contactId)
                {
                    return false;
                }
            }
            if (roleId.HasValue)
            {
                bool isAdminModifiedContact = IsAdminPermissions(roleId.Value);
                bool isAdminLoggedInContact = false;

                ContactRole contactRole = _accessMangementService.GetContactRoles(_sessionService.GetContactId(), _sessionService.GetCompanyId()).FirstOrDefault();
                if (contactRole != null && string.IsNullOrEmpty(contactRole.Id) && new Guid(contactRole.Id) != Guid.Empty)
                {
                    isAdminLoggedInContact =  IsAdminPermissions(new Guid(contactRole.Id));
                }

                if (!isAdminLoggedInContact)
                {
                    return false;
                }
                else
                {
                    if (isAdminModifiedContact)
                    {
                        return false;
                    }
                }
            }

            return true;
        }
        public bool CheckReadUserPermissions(Guid? contactId = null, Guid? roleId = null)
        {
            if (contactId.HasValue&&new Guid(_sessionService.GetContactId()) == contactId)
            {
                return false;
            }
            if (roleId.HasValue)
            {
                bool isAdminModifiedContact = IsAdminPermissions(roleId.Value);

                if (isAdminModifiedContact)
                {
                    return false;
                }

            }
            return true;
        }
        public async Task<bool> CheckRoleHierarchyByRoleId(string roleId)
        {
            var roleToBeChecked = await _accessMangementService.GetRoleAndDepartmentByRoleId(roleId);

            var currentRole = await _accessMangementService.GetRoleAndDepartmentByRoleId(_sessionService.GetRoleId());

            if (roleToBeChecked.ParentportalRole == null || currentRole.ParentportalRole == null)
            {
                if (roleToBeChecked.ParentportalRole == null && currentRole.ParentportalRole != null)
                {
                    return roleToBeChecked.Id == currentRole.ParentportalRole.Id;
                }
                if (roleToBeChecked.ParentportalRole != null && currentRole.ParentportalRole == null)
                {
                    return roleToBeChecked.ParentportalRole.Id == currentRole.Id;
                }
            }

            return roleToBeChecked.Id == currentRole.Id;
        }

        public async Task<bool> CheckDeepAccessForTheRole(string roleId)
        {
            var roleToBeChecked = await _accessMangementService.GetRoleAndDepartmentByRoleId(roleId);

            var currentRole = await _accessMangementService.GetRoleAndDepartmentByRoleId(_sessionService.GetRoleId());

            if (roleToBeChecked.ParentportalRole == null || currentRole.ParentportalRole == null)
            {
                return true;
            }

            return currentRole.Department.Id == roleToBeChecked.Department.Id;
        }

        private bool IsAdminPermissions(Guid roleId)
        {
            var result = Task.Run(async () => await _accessMangementService.GetIsAdminRoleByRoleId(roleId.ToString())).GetAwaiter().GetResult();
            return result;
        }
        
        public bool IsLoggedInUserIsAdmin()
        {
            var roleId = _sessionService.GetRoleId();
            var result = Task.Run(async () => await _accessMangementService.GetIsAdminRoleByRoleId(roleId)).GetAwaiter().GetResult();
            return result;
        }
        public async Task<bool> IsLoggedInUserIsBoardMember()
        {
            var roleId = _sessionService.GetRoleId();
            var result = await _accessMangementService.GetIsBoardMemberRoleByRoleId(roleId);
            return result;
        }
        public async Task<int> LoggedInUserRoleType()
        {
            var roleId = _sessionService.GetRoleId();
            var result = await _accessMangementService.GetLoggedInUserRoleType(roleId);
            return result;
        }
        public async Task<List<string>> GetViewerRoles()
        {
            var result = await _accessMangementService.GetViewerRoles();
            return result;
        }
        public async Task<List<string>> GetBoardMemberRoles()
        {
            var result = await _accessMangementService.GetBoardMemberRoles();
            return result;
        }
    }
}
