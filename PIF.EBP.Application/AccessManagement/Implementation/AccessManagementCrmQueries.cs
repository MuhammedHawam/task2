using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk;
using PIF.EBP.Application.MetaData.DTOs;
using PIF.EBP.Application.Shared;
using PIF.EBP.Core.CRM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PIF.EBP.Application.AccessManagement.DTOs;
using PIF.EBP.Application.PortalConfiguration;
using PIF.EBP.Application.Shared.Helpers;
using PIF.EBP.Application.EntitiesCache.DTOs;

namespace PIF.EBP.Application.AccessManagement.Implementation
{
    public class AccessManagementCrmQueries : IAccessManagementCrmQueries
    {
        private readonly ICrmService _crmService;
        private readonly IPortalConfigAppService _portalConfigAppService;
        public AccessManagementCrmQueries(ICrmService crmService, IPortalConfigAppService portalConfigAppService)
        {
            _crmService = crmService;
            _portalConfigAppService = portalConfigAppService;
        }

        public async Task<List<T>> GetFromDataSource<T>(string entityName) where T : ICacheItem
        {
            List<PortalRole> portalRolesList = GetPortalRoles();
            List<PortalPermission> portalPermissionsList = GetPortalPermissions();
            List<PortalPage> portalPagesList = GetPortalPages();


            var cacheItem = new AccessManagementCacheItem
            {
                Id = "",
                PortalRolesList = portalRolesList,
                PortalPermissionsList = portalPermissionsList,
                PortalPagesList = portalPagesList
            };

            return new List<T> { (T)(ICacheItem)cacheItem };

        }

        public async Task<IEnumerable<TCacheItem>> GetItemFromDataSource<TCacheItem, TPrimaryKey>(string entityName) where TCacheItem : CacheItemBase<TPrimaryKey>
        {
            List<PortalRole> portalRolesList = GetPortalRoles();
            List<PortalPermission> portalPermissionsList = GetPortalPermissions();
            List<PortalPage> portalPagesList = GetPortalPages();
            var cacheItem = new AccessManagementCacheItem
            {
                Id = "",
                PortalRolesList = portalRolesList,
                PortalPermissionsList = portalPermissionsList,
                PortalPagesList = portalPagesList,

            };
            return (IEnumerable<TCacheItem>)new List<AccessManagementCacheItem>() { cacheItem };
        }


        private List<PortalRole> GetPortalRoles()
        {
            var query = new QueryExpression(EntityNames.PortalRole)
            {
                ColumnSet = new ColumnSet("hexa_portalroleid", "hexa_name", "pwc_namear", "pwc_departmentid", "pwc_parentportalroleid", "pwc_showexternal", "pwc_showinternal", "pwc_roletypetypecode"),
                Criteria = new FilterExpression()
            };

            query.Criteria.AddCondition("statecode", ConditionOperator.Equal, 0 /*Active*/);

            var entityCollection = _crmService.GetInstance().RetrieveMultiple(query);

            if (entityCollection.Entities.Any())
            {
                var configurations = _portalConfigAppService.RetrievePortalConfiguration(new List<string> { PortalConfigurations.PCAdminRoleID, PortalConfigurations.PCViewerRoleID });

                return entityCollection.Entities.Select(entityValue => FillEntityRoles(entityValue, configurations)).ToList();
            }

            return new List<PortalRole>();
        }
        private List<PortalPage> GetPortalPages()
        {
            var query = new QueryExpression("hexa_portalpage")
            {
                ColumnSet = new ColumnSet("hexa_portalpageid", "hexa_name", "hexa_namear", "hexa_link"),
                Criteria = new FilterExpression()
            };

            query.Criteria.AddCondition("statecode", ConditionOperator.Equal, 0 /*Active*/);

            var entityCollection = _crmService.GetInstance().RetrieveMultiple(query);

            if (entityCollection.Entities.Any())
            {
                return entityCollection.Entities.Select(entityValue => FillEntityPortalPages(entityValue)).ToList();
            }

            return new List<PortalPage>();
        }


        private List<PortalPermission> GetPortalPermissions()
        {
            var query = new QueryExpression("hexa_portalpermission")
            {
                Distinct = true,
                ColumnSet = new ColumnSet("hexa_portalpermissionid", "hexa_name", "hexa_portalpageid", "pwc_serviceid", "hexa_read", "hexa_write", "hexa_create", "hexa_delete"),
            };
            query.AddOrder("hexa_name", OrderType.Ascending);

            var linkEntity = new LinkEntity
            {
                LinkFromEntityName = "hexa_portalpermission",
                LinkFromAttributeName = "hexa_portalpermissionid",
                LinkToEntityName = "hexa_hexa_portalpermission_hexa_portalrole",
                LinkToAttributeName = "hexa_portalpermissionid",
                JoinOperator = JoinOperator.Inner,
                LinkEntities =
                {
                new LinkEntity
                    {
                        LinkFromEntityName = "hexa_hexa_portalpermission_hexa_portalrole",
                        LinkFromAttributeName = "hexa_portalroleid",
                        LinkToEntityName = "hexa_portalrole",
                        LinkToAttributeName = "hexa_portalroleid",
                        EntityAlias = "ab",
                        Columns = new ColumnSet("hexa_portalroleid"),
                        JoinOperator = JoinOperator.Inner
                    }
                }
            };

            query.Criteria.AddCondition("statecode", ConditionOperator.Equal, 0 /*Active*/);


            query.LinkEntities.Add(linkEntity);

            var entityCollection = _crmService.GetInstance().RetrieveMultiple(query);

            return MapEntitiesToDto(entityCollection);
        }

        private List<PortalPermission> MapEntitiesToDto(EntityCollection entityCollection)
        {
            var permissions = new Dictionary<Guid, PortalPermission>();
            foreach (var entity in entityCollection.Entities)
            {
                Guid permissionId = entity.Id;
                if (!permissions.TryGetValue(permissionId, out var permission))
                {
                    permission = new PortalPermission
                    {
                        Id = permissionId.ToString(),
                        Name = CRMUtility.GetAttributeValue(entity, "hexa_name", string.Empty),
                        PortalPage = CRMUtility.GetEntityReferenceDto(entity, "hexa_portalpageid"),
                        Read = CRMUtility.GetOptionSetValue(entity, "hexa_read"),
                        Write = CRMUtility.GetOptionSetValue(entity, "hexa_write"),
                        Create = CRMUtility.GetOptionSetValue(entity, "hexa_create"),
                        Delete = CRMUtility.GetOptionSetValue(entity, "hexa_delete"),
                        ServiceId = CRMUtility.GetAttributeValue<string>(entity, "pwc_serviceid"),
                        LinkedRoles = new List<string>()
                    };
                    permissions.Add(permissionId, permission);
                }
                var role = CRMUtility.GetValueByAttrNameAliased(entity, "ab.hexa_portalroleid");
                if (!string.IsNullOrEmpty(role))
                {
                    permission.LinkedRoles.Add(role);
                }
            }
            return permissions.Values.ToList();
        }
        private PortalRole FillEntityRoles(Entity entity, List<PortalConfigDto> configurations)
        {
            Guid.TryParse(configurations.SingleOrDefault(a => a.Key == PortalConfigurations.PCAdminRoleID).Value, out Guid pcAdminRoleId);
            Guid.TryParse(configurations.SingleOrDefault(a => a.Key == PortalConfigurations.PCViewerRoleID).Value, out Guid pcViewerRoleId);
            var roleId = entity.Id.ToString();
            var parent = CRMUtility.GetEntityReferenceDto(entity, "pwc_parentportalroleid");
            var department = CRMUtility.GetEntityReferenceDto(entity, "pwc_departmentid");
            bool isAdmin = false, isViewer = false, isAdminIT = false;
            if (pcAdminRoleId.ToString() == roleId || (parent != null && pcAdminRoleId.ToString() == parent.Id))
            {
                isAdmin = true;
            }
            if (pcViewerRoleId.ToString() == roleId || (parent != null && pcViewerRoleId.ToString() == parent.Id))
            {
                isViewer = true;
            }
            if (pcAdminRoleId.ToString() == roleId || (department != null && department.Name.ToLower().Contains("information technology")))
            {
                isAdminIT = true;

            }
            return new PortalRole
            {
                Id = roleId,
                Name = CRMUtility.GetAttributeValue(entity, "hexa_name", string.Empty),
                NameAr = CRMUtility.GetAttributeValue(entity, "pwc_namear", string.Empty),
                ParentportalRole = parent,
                Department = department,
                ShowExternal = CRMUtility.GetAttributeValue(entity, "pwc_showexternal", false),
                ShowInternal = CRMUtility.GetAttributeValue(entity, "pwc_showinternal", false),
                RoleType = entity.GetValueByAttributeName<EntityOptionSetDto>("pwc_roletypetypecode"),
                IsAdmin = isAdmin,
                IsViewer = isViewer,
                IsAdminIT = isAdminIT,
            };

        }
        private PortalPage FillEntityPortalPages(Entity entity)
        {
            return new PortalPage
            {
                Id = entity.Id.ToString(),
                Name = CRMUtility.GetAttributeValue(entity, "hexa_name", string.Empty).ToString(),
                NameAr = CRMUtility.GetAttributeValue(entity, "hexa_namear", string.Empty),
                Link = CRMUtility.GetAttributeValue(entity, "hexa_link", string.Empty),

            };

        }


    }
}
