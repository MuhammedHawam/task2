using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using PIF.EBP.Application.AccessManagement.DTOs;
using PIF.EBP.Application.MetaData.DTOs;
using PIF.EBP.Application.Shared;
using PIF.EBP.Application.Shared.Helpers;
using PIF.EBP.Core.CRM;
using PIF.EBP.Core.CRM.Implementation;
using PIF.EBP.Core.Exceptions;
using PIF.EBP.Core.Session;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static PIF.EBP.Application.Shared.Enums;

namespace PIF.EBP.Application.AccessManagement.Implementation
{
    public class AccessManagementAppService : IAccessManagementAppService
    {
        private readonly ICrmService _crmService;
        private readonly ISessionService _sessionService;
        private readonly IAccessManagementCacheManager _accessManagementCacheManager;
        private readonly List<int> includedAssociationStatuses = new List<int>
        {
            (int)PortalAssociationStatus.Active,
            (int)PortalAssociationStatus.Registered,
            (int)PortalAssociationStatus.Invited,
        };

        public AccessManagementAppService(ICrmService crmService, ISessionService sessionService,
                                          IAccessManagementCacheManager accessManagementCacheManager)
        {
            _crmService = crmService;
            _sessionService = sessionService;
            _accessManagementCacheManager = accessManagementCacheManager;
        }
        

        public async Task<List<RolesPermissionDto>> GetRolesAndPermissionByContactId()
        {
            var contactId = _sessionService.GetContactId();
            if (string.IsNullOrEmpty(contactId))
                throw new UserFriendlyException("Unauthorized", System.Net.HttpStatusCode.Unauthorized);

            List<RolesPermissionDto> rolesPermissionList = new List<RolesPermissionDto>();
            var cachedItem = await _accessManagementCacheManager.GetAccessManagementCacheItem();

            if (cachedItem != null)
            {
                var contactRoles = GetContactRoles(contactId);

                foreach (var contactRole in contactRoles)
                {
                    RolesPermissionDto oRolesPermissionDto = new RolesPermissionDto();
                    oRolesPermissionDto.Email = contactRole.Email;
                    oRolesPermissionDto.CompanyId = contactRole.Company?.Id.ToString();
                    oRolesPermissionDto.RoleId = contactRole.PortalRole?.Id.ToString();
                    oRolesPermissionDto.AssociationId = contactRole.Id;

                    var portalRole = cachedItem.PortalRolesList.FirstOrDefault(x => x.Id == oRolesPermissionDto.RoleId);
                    if (portalRole != null)
                    {
                        oRolesPermissionDto.ShowExternal = portalRole.ShowExternal;
                        oRolesPermissionDto.ShowInternal = portalRole.ShowInternal;

                        var parentRoleId = portalRole.ParentportalRole?.Id.ToString(); // Unused variable, consider removing if not needed

                        var matchingPermissions = cachedItem.PortalPermissionsList
                    .Where(permission => permission.LinkedRoles.Contains(oRolesPermissionDto.RoleId))
                    .ToList();

                        var portalPermissionsDict = new Dictionary<string, PortalPermissionDto>();

                        foreach (var item in matchingPermissions)
                        {
                            var portalPage = cachedItem.PortalPagesList.FirstOrDefault(x => x.Id == item.PortalPage.Id);
                            if (portalPage != null)
                            {
                                string pageKey = portalPage.Id.ToString();
                                if (!portalPermissionsDict.ContainsKey(pageKey))
                                {
                                    portalPermissionsDict[pageKey] = new PortalPermissionDto
                                    {
                                        PortalPage = new PageDto
                                        {
                                            Link = portalPage.Link,
                                            Name = portalPage.Name,
                                            NameAr = portalPage.NameAr
                                        },
                                        Permissions = new List<PermissionDto>()
                                    };
                                }

                                portalPermissionsDict[pageKey].Permissions.Add(new PermissionDto
                                {
                                    Name = item.Name,
                                    Create = item.Create,
                                    Delete = item.Delete,
                                    Read = item.Read,
                                    Write = item.Write,
                                    ServiceId = item.ServiceId
                                });
                            }
                        }

                        oRolesPermissionDto.PortalPermissions = portalPermissionsDict.Values.ToList();

                        rolesPermissionList.Add(oRolesPermissionDto);
                    }
                }

            }

            return rolesPermissionList;
        }

        public async Task<List<EntityReferenceDto>> GetAssociatedRoles(string contactId = "", string companyId = null)
        {
            var cachedItem = await _accessManagementCacheManager.GetAccessManagementCacheItem();
            if (cachedItem == null)
            {
                return new List<EntityReferenceDto>();
            }

            // Filter the ContactRoleList based on the defined statuses and if the role is external
            var query = GetContactRoles(contactId);

            if (!string.IsNullOrEmpty(companyId))
            {
                query = query.Where(x => x.Company.Id == companyId).ToList();
            }

            // Select the PortalRole to EntityReferenceDto
            var contactRoles = query.Select(x => x.PortalRole).ToList();

            return contactRoles;
        }

        public async Task<List<EntityReferenceDto>> GetCompaniesByContactId(string contactId)
        {
            if (string.IsNullOrEmpty(contactId))
                throw new UserFriendlyException("Unauthorized", System.Net.HttpStatusCode.Unauthorized);

            var cachedItem = await _accessManagementCacheManager.GetAccessManagementCacheItem();
            if (cachedItem == null)
            {
                return new List<EntityReferenceDto>(); // Return an empty list if the cache is not available
            }


            // Use LINQ to filter and select in one operation
            var companies = GetContactRoles(contactId).Select(x => x.Company).ToList();
            return companies; // Always return a list, even if empty
        }

        public async Task<List<ContactRole>> GetContactRolesByContactId(string contactId)
        {
            if (string.IsNullOrEmpty(contactId))
                throw new UserFriendlyException("Unauthorized", System.Net.HttpStatusCode.Unauthorized);

            var cachedItem = await _accessManagementCacheManager.GetAccessManagementCacheItem();
            if (cachedItem == null)
            {
                return new List<ContactRole>(); // Return an empty list if the cache is not available
            }


            // Use LINQ to filter and select in one operation
            var contactRoles = GetContactRoles(contactId).ToList();
            return contactRoles;

        }

        public async Task<List<AuthPermission>> GetAuthorizedPermissions()
        {
            var contactId = _sessionService.GetContactId();
            if (string.IsNullOrEmpty(contactId))
                throw new UserFriendlyException("Unauthorized", System.Net.HttpStatusCode.Unauthorized);

            List<AuthPermission> authPermissionsList = new List<AuthPermission>();
            var cachedItem = await _accessManagementCacheManager.GetAccessManagementCacheItem();

            if (cachedItem != null)
            {
                var contactRole = GetContactRoles(contactId, _sessionService.GetCompanyId()).Where(x => x.PortalRole.Id == _sessionService.GetRoleId()).SingleOrDefault();

                var portalRole = cachedItem.PortalRolesList.First(x => x.Id == contactRole.PortalRole?.Id);
                if (portalRole != null)
                {

                    var matchingPermissions = cachedItem.PortalPermissionsList
                        .Where(permission => permission.LinkedRoles.Contains(contactRole.PortalRole?.Id))
                        .ToList();

                    foreach (var item in matchingPermissions)
                    {
                        var portalPage = cachedItem.PortalPagesList.First(x => x.Id == item.PortalPage.Id);
                        if (portalPage != null)
                        {
                            var oAuthPermission = (new AuthPermission
                            {
                                Create = item.Create,
                                Delete = item.Delete,
                                Read = item.Read,
                                Write = item.Write,
                                PageLink = portalPage.Link,
                                ServiceId = item.ServiceId,
                                Name = item.Name
                            });
                            authPermissionsList.Add(oAuthPermission);
                        }
                    }

                }

            }
            return authPermissionsList
            .GroupBy(p => p.Name)
            .Select(g => g.First())
            .ToList();
        }

        public List<ContactRole> GetActiveRolesAssociationForSignedInContact()
        {
            var result = GetContactRoles(_sessionService.GetContactId());
            return result.Where(x => x.AssociationStatus == (int)PortalAssociationStatus.Active).ToList();
        }

        public async Task<List<ContactRole>> GetActiveRolesAssociationForSignedInContactWithDepartments()
        {
            var result = GetContactRoles(_sessionService.GetContactId());
            var cachedItem = await _accessManagementCacheManager.GetAccessManagementCacheItem();

            foreach (var item in result)
            {
                item.PortalRoleEntity = cachedItem.PortalRolesList.FirstOrDefault(a => a.Id == item.PortalRole.Id);
            }
            return result.Where(x => x.AssociationStatus == (int)PortalAssociationStatus.Active || x.AssociationStatus == (int)PortalAssociationStatus.Registered || x.AssociationStatus == (int)PortalAssociationStatus.Invited).ToList();
        }

        public async Task<List<AuthPermission>> GetPermissionsByRoleIdAsync(string roleId)
        {
            var cachedItem = await _accessManagementCacheManager.GetAccessManagementCacheItem();
            List<AuthPermission> authPermissionsList = new List<AuthPermission>();

            if (cachedItem != null)
            {
                var portalRole = cachedItem.PortalRolesList.First(x => x.Id == roleId);
                if (portalRole != null)
                {

                    var matchingPermissions = cachedItem.PortalPermissionsList
                        .Where(permission => permission.LinkedRoles.Contains(roleId))
                        .ToList();

                    foreach (var item in matchingPermissions)
                    {
                        var portalPage = cachedItem.PortalPagesList.First(x => x.Id == item.PortalPage.Id);
                        if (portalPage != null)
                        {
                            var oAuthPermission = (new AuthPermission
                            {
                                Create = item.Create,
                                Delete = item.Delete,
                                Read = item.Read,
                                Write = item.Write,
                                PageLink = portalPage.Link,
                                ServiceId = item.ServiceId,
                                Name = item.Name
                            });
                            authPermissionsList.Add(oAuthPermission);
                        }
                    }

                }

            }
            return authPermissionsList
            .GroupBy(p => p.Name)
            .Select(g => g.First())
            .ToList();
        }

        public List<ContactRole> GetDeletedConactRoleAssociations(string contactId, string companyId = null)
        {
            var query = new QueryExpression(EntityNames.ContactAssociation)
            {
                ColumnSet = new ColumnSet("hexa_contactroleassociationid", "hexa_name", "hexa_email", "hexa_companyid", "hexa_contactid", "hexa_portalroleid", "hexa_associationstatustypecode"),
                Criteria = new FilterExpression()
            };
            List<int> statuses = new List<int> { (int)PortalAssociationStatus.Deleted, (int)PortalAssociationStatus.Inactive };
            // Convert the result of Select to an array
            query.Criteria.AddCondition("hexa_associationstatustypecode", ConditionOperator.In, statuses.Cast<object>().ToArray());

            query.Criteria.AddCondition("hexa_contactid", ConditionOperator.Equal, new Guid(contactId));
            if (companyId != null)
            {
                query.Criteria.AddCondition("hexa_companyid", ConditionOperator.Equal, new Guid(companyId));
            }

            var entityCollection = _crmService.GetInstance().RetrieveMultiple(query);

            if (entityCollection.Entities.Any())
            {
                return entityCollection.Entities.Select(entityValue => FillContactRoles(entityValue)).ToList();
            }

            return new List<ContactRole>();
        }

        public List<ContactRole> GetContactRoles(string contactId, string companyId = null)
        {
            var query = new QueryExpression(EntityNames.ContactAssociation)
            {
                ColumnSet = new ColumnSet("hexa_contactroleassociationid", "hexa_name", "hexa_email", "hexa_companyid", "hexa_contactid", "hexa_portalroleid", "hexa_associationstatustypecode"),
                Criteria = new FilterExpression()
            };
            var rolelink = new LinkEntity
            {
                LinkFromEntityName = EntityNames.ContactAssociation,
                LinkFromAttributeName = "hexa_portalroleid",
                LinkToEntityName = EntityNames.PortalRole,
                LinkToAttributeName = "hexa_portalroleid",
                JoinOperator = JoinOperator.Inner,
                Columns = new ColumnSet("hexa_portalroleid", "hexa_name", "pwc_namear"),
                EntityAlias = "Role"
            };
            query.LinkEntities.Add(rolelink);
            // Convert the result of Select to an array
            query.Criteria.AddCondition("hexa_associationstatustypecode", ConditionOperator.In, includedAssociationStatuses.Cast<object>().ToArray());

            query.Criteria.AddCondition("hexa_contactid", ConditionOperator.Equal, new Guid(contactId));
            if (companyId != null)
            {
                query.Criteria.AddCondition("hexa_companyid", ConditionOperator.Equal, new Guid(companyId));
            }

            var entityCollection = _crmService.GetInstance().RetrieveMultiple(query);

            if (entityCollection.Entities.Any())
            {
                return entityCollection.Entities.Select(entityValue => FillContactRoles(entityValue)).ToList();
            }

            return new List<ContactRole>();
        }

   public string GetContactParentRole(string contactId, string companyId = null)
    {
        var query = new QueryExpression(EntityNames.ContactAssociation)
        {
            ColumnSet = new ColumnSet("hexa_portalroleid"),
            Criteria = new FilterExpression()
        };
        var rolelink = new LinkEntity
        {
            LinkFromEntityName = EntityNames.ContactAssociation,
            LinkFromAttributeName = "hexa_portalroleid",
            LinkToEntityName = EntityNames.PortalRole,
            LinkToAttributeName = "hexa_portalroleid",
            JoinOperator = JoinOperator.Inner,
            Columns = new ColumnSet("hexa_portalroleid", "pwc_parentportalroleid"),
            EntityAlias = "Role"
        };
        query.LinkEntities.Add(rolelink);

        query.Criteria.AddCondition("hexa_associationstatustypecode", ConditionOperator.In, includedAssociationStatuses.Cast<object>().ToArray());
        query.Criteria.AddCondition("hexa_contactid", ConditionOperator.Equal, new Guid(contactId));
        if (companyId != null)
        {
            query.Criteria.AddCondition("hexa_companyid", ConditionOperator.Equal, new Guid(companyId));
        }

        var entityCollection = _crmService.GetInstance().RetrieveMultiple(query);

        if (entityCollection.Entities.Any())
        {
            var contactAssociationEntity = entityCollection.Entities.First();

            const string aliasedAttributeName = "Role.pwc_parentportalroleid";

            if (contactAssociationEntity.Attributes.Keys.Contains(aliasedAttributeName))
            {
                var aliasedValue = contactAssociationEntity.Attributes[aliasedAttributeName] as AliasedValue;

                if (aliasedValue?.Value is EntityReference parentRoleReference)
                {
                    return parentRoleReference.Id.ToString();
                }
                else if (aliasedValue?.Value is Guid parentRoleId)
                {
                    return parentRoleId.ToString();
                }
            } else
                {
                    var aliasedValue = contactAssociationEntity.Attributes["Role.hexa_portalroleid"] as AliasedValue;

                    if (aliasedValue?.Value is EntityReference parentRoleReference)
                    {
                        return parentRoleReference.Id.ToString();
                    }
                    else if (aliasedValue?.Value is Guid parentRoleId)
                    {
                        return parentRoleId.ToString();
                    }
                }
        }

        return string.Empty;
    }

    public async Task<string> GetParentPortalRole(string roleId, string departmentId)
        {
            var cachedItem = await _accessManagementCacheManager.GetAccessManagementCacheItem();
            var portalRole = cachedItem.PortalRolesList.Where(x =>
            x.ParentportalRole != null && x.ParentportalRole.Id == roleId
            && x.Department.Id == departmentId).Select(c => c.Id).FirstOrDefault();

            return portalRole;
        }

        public async Task<string> GetParentRoleByRoleId(string roleId)
        {
            var cachedItem = await _accessManagementCacheManager.GetAccessManagementCacheItem();

            return cachedItem.PortalRolesList.Where(x =>
            x.ParentportalRole != null && x.Id == roleId).Select(c => c.ParentportalRole.Id)?.FirstOrDefault();

        }
        public async Task<PortalRole> GetRoleAndDepartmentByRoleId(string roleId)
        {
            var cachedItem = await _accessManagementCacheManager.GetAccessManagementCacheItem();

            var oPortalRole = cachedItem.PortalRolesList.Where(x =>x.Id == roleId).FirstOrDefault();
            return oPortalRole;
        }

        public async Task<bool> GetIsAdminRoleByRoleId(string roleId)
        {
            var cachedItem = await _accessManagementCacheManager.GetAccessManagementCacheItem();
            var isAdmin = cachedItem.PortalRolesList.First(x => x.Id == roleId).IsAdmin;

            return isAdmin;
        }
        public async Task<bool> GetIsAdminOrAdminITRoleByRoleId(string roleId)
        {
            var cachedItem = await _accessManagementCacheManager.GetAccessManagementCacheItem();
            var isAdminIT = cachedItem.PortalRolesList.First(x => x.Id == roleId).IsAdminIT;
            
            return isAdminIT;
        }

        public async Task<bool> GetIsBoardMemberRoleByRoleId(string roleId)
        {
            var cachedItem = await _accessManagementCacheManager.GetAccessManagementCacheItem();
            var isBoardMember = cachedItem.PortalRolesList.Exists(x => x.Id == roleId && x.RoleType.Value == ((int)Enums.RoleType.BoardMember).ToString());

            return isBoardMember;
        }

        public async Task<int> GetLoggedInUserRoleType(string roleId)
        {
            var cachedItem = await _accessManagementCacheManager.GetAccessManagementCacheItem();
            var RoleType = cachedItem.PortalRolesList.First(x => x.Id == roleId).RoleType;

            int.TryParse(RoleType.Value, out int value);

            return value;
        }

        public async Task<List<string>> GetViewerRoles()
        {
            var cachedItem = await _accessManagementCacheManager.GetAccessManagementCacheItem();
            var viewerIds = cachedItem.PortalRolesList.Where(x => x.IsViewer).Select(z => z.Id).ToList();

            return viewerIds;
        }
        public async Task<List<string>> GetBoardMemberRoles()
        {
            var cachedItem = await _accessManagementCacheManager.GetAccessManagementCacheItem();
            var boardIds = cachedItem.PortalRolesList.Where(x => x.RoleType.Value==((int)Enums.RoleType.BoardMember).ToString()).Select(z => z.Id).ToList();

            return boardIds;
        }

        private ContactRole FillContactRoles(Entity entity)
        {
            return new ContactRole
            {
                Id = entity.Id.ToString(),
                Name = CRMUtility.GetAttributeValue(entity, "hexa_name", string.Empty),
                Email = CRMUtility.GetAttributeValue(entity, "hexa_email", string.Empty),
                Company = CRMUtility.GetEntityReferenceDto(entity, "hexa_companyid"),
                Contact = CRMUtility.GetEntityReferenceDto(entity, "hexa_contactid"),
                PortalRole = CRMOperations.GetValueByAttributeName<EntityReferenceDto>(entity, "hexa_portalroleid", "Role.pwc_namear"),
                AssociationStatus = CRMUtility.GetOptionSetValue(entity, "hexa_associationstatustypecode")
            };
        }
    }
}
