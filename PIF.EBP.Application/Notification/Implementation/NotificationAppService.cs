using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk;
using PIF.EBP.Application.MetaData.DTOs;
using PIF.EBP.Application.Notification.DTOs;
using PIF.EBP.Application.Shared;
using PIF.EBP.Application.Shared.AppRequest;
using PIF.EBP.Application.Shared.AppResponse;
using PIF.EBP.Core.CRM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PIF.EBP.Core.Session;
using PIF.EBP.Application.Shared.Helpers;
using static PIF.EBP.Application.Shared.Enums;
using Microsoft.Xrm.Sdk.Query;
using PIF.EBP.Application.AccessManagement;
using PIF.EBP.Application.EntitiesCache;
using PIF.EBP.Application.AccessManagement.DTOs;

namespace PIF.EBP.Application.Notification.Implementation
{
    public class NotificationAppService : INotificationAppService
    {
        private readonly ICrmService _crmService;
        private readonly ISessionService _sessionService;
        private readonly IAccessManagementAppService _accessManagementAppService;
        private readonly IEntitiesCacheAppService _entitiesCacheAppService;
        private readonly IUserPermissionAppService _userPermissionAppService;

        public NotificationAppService(ICrmService crmService,
            ISessionService sessionService,
            IAccessManagementAppService accessManagementAppService,
            IEntitiesCacheAppService entitiesCacheAppService,
            IUserPermissionAppService userPermissionAppService)
        {
            _crmService = crmService;
            _sessionService = sessionService;
            _accessManagementAppService = accessManagementAppService;
            _entitiesCacheAppService = entitiesCacheAppService;
            _userPermissionAppService = userPermissionAppService;

        }

        public string AddNotification(List<Entity> entities)
        {
            foreach (var entity in entities)
            {
                _crmService.Create(entity, EntityNames.Notifications);
            }
            return "Notification is added sucessfully!!";
        }

        public async Task<NotificationResponse> RetrieveNotifications(PagingRequest pagingRequest)
        {
            var response = new NotificationResponse();
            var associationsWithRoles = await _accessManagementAppService.GetActiveRolesAssociationForSignedInContactWithDepartments();

            ListPagingResponse<NotificationDto> oResponse = new ListPagingResponse<NotificationDto>();
            var notifications = new List<NotificationDto>();

            if (pagingRequest == null)
            {
                pagingRequest = new PagingRequest();
            }
            var associations = _accessManagementAppService.GetActiveRolesAssociationForSignedInContact();
            EntityCollection results = await GetNotificationsList(associations, pagingRequest);

            notifications.AddRange(results.Entities.Select(entity => FillNotifications(entity, associations)).ToList());

            oResponse.TotalCount = results.TotalRecordCount;
            oResponse.ListResponse = notifications.ToList();

            response.NotificationResponses = oResponse;
            response.Associations = associationsWithRoles;
            return response;
        }

        public async Task<int> RetrieveUnreadNotifications()
        {
            int unreadNotificationCount = 0;

            var associations = _accessManagementAppService.GetActiveRolesAssociationForSignedInContact();
            unreadNotificationCount = await GetUnReadNotifications(associations);

            return unreadNotificationCount;
        }

        public async Task<string> UpdateNotificationReadStatus(Guid Id)
        {
            List<Guid> Ids = new List<Guid>();
            var orgContext = new OrganizationServiceContext(_crmService.GetInstance());

            if (Id != Guid.Empty)
            {
                Ids.Add(Id);
            }
            else
            {
                IQueryable<Entity> requestQuery = orgContext.CreateQuery(EntityNames.Notifications).AsQueryable();
                requestQuery = requestQuery.Where(x => ((EntityReference)x["pwc_contactid"]).Id.Equals(_sessionService.GetContactId()) && ((DateTime)x["createdon"]) >= DateTime.Now.AddMonths(-3));
                Ids = requestQuery.Select(entity => entity.Id).ToList();
            }

            foreach (var id in Ids)
            {
                var Entity = new Entity(EntityNames.Notifications);

                Entity.Id = id;
                Entity["pwc_readstatustypecode"] = new OptionSetValue((int)NotificationReadStatus.Read);

                _crmService.Update(Entity, EntityNames.Notifications);
            }

            return "Notification(s) Read Status has been updated";
        }

        private async Task<EntityCollection> GetNotificationsList(List<ContactRole> associations, PagingRequest pagingRequest)
        {
            var RoleType = await _userPermissionAppService.LoggedInUserRoleType();
            var query = new QueryExpression(EntityNames.Notifications)
            {
                ColumnSet = new ColumnSet("pwc_portalnotificationsid", "pwc_descriptionen", "pwc_name", "pwc_namear", "createdon", "pwc_typetypecode", "pwc_readstatustypecode", "pwc_appointmentidid", "pwc_contactid", "pwc_companyid", "pwc_duedate", "pwc_overdueflag", "pwc_organizedbyid", "pwc_eventdate", "pwc_requeststepid", "pwc_requestid", "pwc_descriptionar", "pwc_portalroleid")
            };

            query.PageInfo = CRMOperations.GetPagingInfo(pagingRequest);

            var rolelink = new LinkEntity
            {
                LinkFromEntityName = EntityNames.Notifications,
                LinkFromAttributeName = "pwc_portalroleid",
                LinkToEntityName = EntityNames.PortalRole,
                LinkToAttributeName = "hexa_portalroleid",
                JoinOperator = JoinOperator.LeftOuter,
                Columns = new ColumnSet("hexa_portalroleid", "hexa_name", "pwc_namear", "pwc_departmentid", "pwc_parentportalroleid", "pwc_showexternal", "pwc_showinternal", "pwc_roletypetypecode"),
                EntityAlias = "Role",
                LinkCriteria = new FilterExpression
                {
                    FilterOperator = LogicalOperator.And,
                    Conditions =
                                {
                                    new ConditionExpression("pwc_roletypetypecode", ConditionOperator.Equal, RoleType)
                                }
                }
            };
            query.AddOrder("createdon", OrderType.Descending);
            query.LinkEntities.Add(rolelink);
            await ApplyCommonCriteria(associations, query);

            return _crmService.GetInstance().RetrieveMultiple(query);
        }

        private async Task<int> GetUnReadNotifications(List<ContactRole> associations)
        {
            var RoleType = await _userPermissionAppService.LoggedInUserRoleType();
            var query = new QueryExpression(EntityNames.Notifications)
            {
                ColumnSet = new ColumnSet("pwc_portalnotificationsid", "createdon", "pwc_readstatustypecode", "pwc_contactid", "pwc_companyid", "pwc_portalroleid")
            };

            var rolelink = new LinkEntity
            {
                LinkFromEntityName = EntityNames.Notifications,
                LinkFromAttributeName = "pwc_portalroleid",
                LinkToEntityName = EntityNames.PortalRole,
                LinkToAttributeName = "hexa_portalroleid",
                JoinOperator = JoinOperator.LeftOuter,
                Columns = new ColumnSet("hexa_portalroleid", "pwc_roletypetypecode"),
                EntityAlias = "Role",
                LinkCriteria = new FilterExpression
                {
                    FilterOperator = LogicalOperator.And,
                    Conditions =
                                {
                                    new ConditionExpression("pwc_roletypetypecode", ConditionOperator.Equal, RoleType)
                                }
                }
            };

            query.LinkEntities.Add(rolelink);

            await ApplyCommonCriteria(associations, query);

            query.Criteria.AddCondition("pwc_readstatustypecode", ConditionOperator.Equal, (int)NotificationReadStatus.Unread);


            var fetchXml = _crmService.GetFetchXml(query);
            CRMOperations.MakeAggregate(fetchXml);
            CRMOperations.AddAggregateColumn(fetchXml, EntityNames.Notifications, "pwc_readstatustypecode", "count", "unread");
            var queryResult = _crmService.GetInstance().RetrieveMultiple(CRMOperations.GetFetchExpression(fetchXml)).Entities;
            var notificationsList = queryResult.ToList();

            var result = notificationsList.Sum(x => CRMOperations.GetAliasedField<int>(x, "unread"));

            return result;
        }

        private async Task ApplyCommonCriteria(List<ContactRole> associations, QueryExpression query)
        {
            var associationFilter = new FilterExpression(LogicalOperator.Or);
            foreach (var association in associations)
            {
                var permissions = await _accessManagementAppService.GetAuthorizedPermissions();
                var taskAccessLevel = GetPermissionAccessLevelOnRequestStep(permissions);
                var requestAccessLevel = GetPermissionAccessLevelOnRequest(permissions);

                FilterExpression permissionBasedFilter;
                if (taskAccessLevel == AccessLevel.User && requestAccessLevel == AccessLevel.User)
                {
                    permissionBasedFilter = new FilterExpression(LogicalOperator.And);
                    permissionBasedFilter.AddCondition("pwc_contactid", ConditionOperator.Equal, _sessionService.GetContactId());

                }
                else if (taskAccessLevel == AccessLevel.User)
                {
                    permissionBasedFilter = new FilterExpression(LogicalOperator.And);
                    permissionBasedFilter.AddCondition("pwc_contactid", ConditionOperator.Equal, _sessionService.GetContactId());
                    permissionBasedFilter.AddCondition("pwc_typetypecode", ConditionOperator.NotIn,
                        new int[] { (int)PortalNotificationType.RequestOverdue, (int)PortalNotificationType.ReturnedRequest });
                }
                else if (requestAccessLevel == AccessLevel.User)
                {
                    permissionBasedFilter = new FilterExpression(LogicalOperator.And);
                    permissionBasedFilter.AddCondition("pwc_contactid", ConditionOperator.Equal, _sessionService.GetContactId());
                    permissionBasedFilter.AddCondition("pwc_typetypecode", ConditionOperator.NotIn,
                        new int[] { (int)PortalNotificationType.NewTaskReceived, (int)PortalNotificationType.TaskAssigned, (int)PortalNotificationType.ReturnedTask });
                }
                else
                {
                    permissionBasedFilter = new FilterExpression(LogicalOperator.Or);
                    permissionBasedFilter.AddCondition("pwc_contactid", ConditionOperator.Equal, _sessionService.GetContactId());
                    permissionBasedFilter.AddCondition("Role", "hexa_portalroleid", ConditionOperator.NotNull);
                }

                var mainFilter = new FilterExpression(LogicalOperator.And);
                mainFilter.AddFilter(permissionBasedFilter);

                mainFilter.AddCondition("createdon", ConditionOperator.GreaterEqual, DateTime.Now.AddMonths(-3));

                var companyIds = associations.Select(z => z.Company.Id).ToArray();
                mainFilter.AddCondition("pwc_companyid", ConditionOperator.In, companyIds);
                associationFilter.AddFilter(mainFilter);
            }
            

            query.Criteria.AddFilter(associationFilter);
        }

        private NotificationDto FillNotifications(Entity entity, List<AccessManagement.DTOs.ContactRole> associations)
        {

            var notification = new NotificationDto
            {
                Id = entity.Id,
                Description = entity.GetValueByAttributeName<string>("pwc_descriptionen"),
                DescriptionAr = entity.GetValueByAttributeName<string>("pwc_descriptionar"),
                Name = entity.GetValueByAttributeName<string>("pwc_name"),
                NameAr = entity.GetValueByAttributeName<string>("pwc_namear"),
                CreatedOn = entity.GetValueByAttributeName<DateTime>("createdon"),
                Type = _entitiesCacheAppService.RetrieveOptionSetCacheByKeyWithValue(OptionSetKey.NotificationType, CRMOperations.GetValueByAttributeName<OptionSetValue>(entity, "pwc_typetypecode")),
                ReadStatus = entity.GetValueByAttributeName<EntityOptionSetDto>("pwc_readstatustypecode"),
                Appointment = GetAppointmentDetails(entity.GetValueByAttributeName<EntityReferenceDto>("pwc_appointmentidid") != null ? entity.GetValueByAttributeName<EntityReferenceDto>("pwc_appointmentidid").Id : string.Empty),
                Contact = entity.GetValueByAttributeName<EntityReferenceDto>("pwc_contactid"),
                Company = entity.GetValueByAttributeName<EntityReferenceDto>("pwc_companyid"),
                DueDate = entity.GetValueByAttributeName<DateTime?>("pwc_duedate") != null ? entity.GetValueByAttributeName<DateTime>("pwc_duedate").ToString("dd MMM yyyy") : null,
                OverdueFlag = entity.GetValueByAttributeName<bool>("pwc_overdueflag"),
                Organizedby = entity.GetValueByAttributeName<EntityReferenceDto>("pwc_organizedbyid"),
                EventDate = entity.GetValueByAttributeName<DateTime>("pwc_eventdate"),
                RequestStep = entity.GetValueByAttributeName<EntityReferenceDto>("pwc_requeststepid")?.Id != null ? GetRequestStepDetails(new Guid(entity.GetValueByAttributeName<EntityReferenceDto>("pwc_requeststepid")?.Id)) : null,
                Request = entity.GetValueByAttributeName<EntityReferenceDto>("pwc_requestid")?.Id != null ? GetRequestDetails(new Guid(entity.GetValueByAttributeName<EntityReferenceDto>("pwc_requestid")?.Id)) : null,

            };

            if (notification.Company != null)
            {
                var association = associations.FirstOrDefault(a => a.Company.Id == notification.Company.Id);
                notification.RoleAssociationId = association?.Id;
                notification.PortalRole = association?.PortalRole;
            }

            return notification;
        }

        private EntityReferenceDto GetRequestDetails(Guid RequestId)
        {
            QueryExpression query = new QueryExpression(EntityNames.HexaRequests)
            {
                ColumnSet = new ColumnSet("hexa_requestid", "hexa_name"),
                Criteria = new FilterExpression
                {
                    FilterOperator = LogicalOperator.And
                }
            };

            query.Criteria.AddCondition(new ConditionExpression
            {
                AttributeName = "hexa_requestid",
                Operator = ConditionOperator.Equal,
                Values = { RequestId }
            });

            LinkEntity linkEntity = new LinkEntity
            {
                LinkFromEntityName = EntityNames.HexaRequests,
                LinkFromAttributeName = "hexa_processtemplate",
                LinkToEntityName = EntityNames.ProcessTemplate,
                LinkToAttributeName = "hexa_processtemplateid",
                JoinOperator = JoinOperator.LeftOuter,
                EntityAlias = "processtemplateAlias",
                Columns = new ColumnSet("hexa_nameen", "hexa_namear")
            };

            query.LinkEntities.Add(linkEntity);

            EntityCollection results = _crmService.GetInstance().RetrieveMultiple(query);

            if (results != null && results.Entities.Count > 0)
            {
                EntityReferenceDto entityReferenceDto = new EntityReferenceDto();

                entityReferenceDto.Id = results.Entities[0].GetAttributeValue<Guid>("hexa_requestid").ToString();
                entityReferenceDto.Name = results.Entities[0] != null && results.Entities[0].Contains("processtemplateAlias.hexa_nameen") ? results.Entities[0].GetAttributeValue<AliasedValue>("processtemplateAlias.hexa_nameen").Value.ToString() : string.Empty;
                entityReferenceDto.NameAr = results.Entities[0] != null && results.Entities[0].Contains("processtemplateAlias.hexa_namear") ? results.Entities[0].GetAttributeValue<AliasedValue>("processtemplateAlias.hexa_namear").Value.ToString() : string.Empty;

                return entityReferenceDto;
            }
            return new EntityReferenceDto();
        }

        private EntityReferenceDto GetRequestStepDetails(Guid RequestStepId)
        {
            QueryExpression query = new QueryExpression(EntityNames.RequestStep)
            {
                ColumnSet = new ColumnSet("hexa_requeststepid", "hexa_name"),
                Criteria = new FilterExpression
                {
                    FilterOperator = LogicalOperator.And
                }
            };

            query.Criteria.AddCondition(new ConditionExpression
            {
                AttributeName = "hexa_requeststepid",
                Operator = ConditionOperator.Equal,
                Values = { RequestStepId }
            });

            LinkEntity linkEntity = new LinkEntity
            {
                LinkFromEntityName = EntityNames.RequestStep,
                LinkFromAttributeName = "hexa_processstep",
                LinkToEntityName = EntityNames.ProcessStepTemplate,
                LinkToAttributeName = "hexa_processsteptemplateid",
                JoinOperator = JoinOperator.LeftOuter,
                EntityAlias = "processsteptemplateAlias",
                Columns = new ColumnSet("hexa_summaryar", "hexa_summary")
            };

            query.LinkEntities.Add(linkEntity);

            EntityCollection results = _crmService.GetInstance().RetrieveMultiple(query);

            if (results != null && results.Entities.Count > 0)
            {
                EntityReferenceDto entityReferenceDto = new EntityReferenceDto();

                entityReferenceDto.Id = results.Entities[0].GetAttributeValue<Guid>("hexa_requeststepid").ToString();
                entityReferenceDto.Name = results.Entities[0] != null && results.Entities[0].Contains("processsteptemplateAlias.hexa_summary") ? results.Entities[0].GetAttributeValue<AliasedValue>("processsteptemplateAlias.hexa_summary").Value.ToString() : string.Empty;
                entityReferenceDto.NameAr = results.Entities[0] != null && results.Entities[0].Contains("processsteptemplateAlias.hexa_summaryar") ? results.Entities[0].GetAttributeValue<AliasedValue>("processsteptemplateAlias.hexa_summaryar").Value.ToString() : string.Empty;

                return entityReferenceDto;
            }

            return new EntityReferenceDto();
        }

        private AppointmentDto GetAppointmentDetails(string AppointmentId)
        {
            AppointmentDto appointment = new AppointmentDto();

            if (!string.IsNullOrEmpty(AppointmentId) && new Guid(AppointmentId) != Guid.Empty)
            {
                var orgContext = new OrganizationServiceContext(_crmService.GetInstance());

                IQueryable<Entity> requestQuery = orgContext.CreateQuery(EntityNames.Appointment).AsQueryable();
                requestQuery = requestQuery.Where(x => ((EntityReference)x["activityid"]).Id.Equals(new Guid(AppointmentId)));
                appointment = requestQuery.Select(entity => FillAppointment(entity)).FirstOrDefault();
            }

            return appointment;
        }

        private AppointmentDto FillAppointment(Entity entity)
        {
            var appointment = new AppointmentDto
            {
                Id = entity.Id,
                Date = entity.GetValueByAttributeName<DateTime?>("scheduledstart"),
                Title = entity.GetValueByAttributeName<string>("subject"),
                TitleAr = entity.GetValueByAttributeName<string>("pwc_meetingtitlear"),
            };

            EntityOptionSetDto type = entity.GetValueByAttributeName<EntityOptionSetDto>("pwc_typetypecode");


            if (type != null && !string.IsNullOrEmpty(type.Value))
            {
                if (int.Parse(type.Value) == (int)AppointmentType.Event)
                {
                    appointment.Type = (int)ScopeSearchCalender.Events;
                }
                else
                {
                    appointment.Type = (int)ScopeSearchCalender.Meetings;
                }
            }

            return appointment;
        }
        private AccessLevel GetPermissionAccessLevelOnRequestStep(List<AuthPermission> permissions)
        {
            var requestDetailsPermission = permissions.Where(x => x.PageLink.Contains(PageRoute.TaskDetails)
                                            && string.IsNullOrEmpty(x.ServiceId)).FirstOrDefault();

            if (requestDetailsPermission == null)
            {
                return AccessLevel.None;
            }
            else
            {
                return (AccessLevel)requestDetailsPermission.Read;
            }
        }
        private AccessLevel GetPermissionAccessLevelOnRequest(List<AuthPermission> permissions)
        {
            var requestDetailsPermission = permissions.Where(x => x.PageLink.Contains(PageRoute.RequestDetails)
                                            && string.IsNullOrEmpty(x.ServiceId)).FirstOrDefault();

            if (requestDetailsPermission == null)
            {
                return AccessLevel.None;
            }
            else
            {
                return (AccessLevel)requestDetailsPermission.Read;
            }
        }
    }

    public class NotificationResponse
    {
        public ListPagingResponse<NotificationDto> NotificationResponses { get; set; }
        public List<ContactRole> Associations { get; set; }
    }
}
