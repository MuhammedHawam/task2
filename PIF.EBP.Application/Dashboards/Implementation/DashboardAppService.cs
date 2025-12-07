using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Query;
using PIF.EBP.Application.AccessManagement;
using PIF.EBP.Application.AccessManagement.DTOs;
using PIF.EBP.Application.Dashboards.DTOs;
using PIF.EBP.Application.MetaData.DTOs;
using PIF.EBP.Application.PortalConfiguration;
using PIF.EBP.Application.Shared;
using PIF.EBP.Application.Shared.AppRequest;
using PIF.EBP.Application.Shared.AppResponse;
using PIF.EBP.Application.Shared.Helpers;
using PIF.EBP.Core.CRM;
using PIF.EBP.Core.Exceptions;
using PIF.EBP.Core.Session;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static PIF.EBP.Application.Shared.Enums;

namespace PIF.EBP.Application.Dashboards.Implementation
{
    public class DashboardAppService : IDashboardAppService
    {
        private readonly ICrmService _crmService;
        private readonly ISessionService _sessionService;
        private readonly IPortalConfigAppService _portalConfigAppService;
        private readonly IAccessManagementAppService _accessManagementAppService;
        private readonly IUserPermissionAppService _userPermissionAppService;

        public DashboardAppService(ICrmService crmService,
            ISessionService sessionService,
            IPortalConfigAppService portalConfigAppService,
            IAccessManagementAppService accessManagementAppService,
            IUserPermissionAppService userPermissionAppService)
        {
            _crmService = crmService;
            _sessionService = sessionService;
            _portalConfigAppService = portalConfigAppService;
            _accessManagementAppService = accessManagementAppService;
            _userPermissionAppService = userPermissionAppService;
        }

        public async Task<WidgetDataDto> RetrieveWidgetData(int? scope = (int)ScopeWidgetData.All, DateTime? calendarDate = null, int? scheduleFilter = (int)ScopeSchedule.All, Guid? taskFilter = null)
        {
            var WidgetData = new WidgetDataDto();

            if (new Guid(_sessionService.GetContactId()) == Guid.Empty)
            {
                throw new UserFriendlyException("InvalidContactIdValue", System.Net.HttpStatusCode.BadRequest);
            }
            else if (new Guid(_sessionService.GetCompanyId()) == Guid.Empty)
            {
                throw new UserFriendlyException("InvalidCompanyIdValue", System.Net.HttpStatusCode.BadRequest);
            }

            WidgetData.ShowTutorial = GetShowTutorial();
            WidgetData.QuickActions = await GetUserProfileQuickAction();

            if (scope == (int)ScopeWidgetData.RequestSummary || scope == (int)ScopeWidgetData.All)
            {
                WidgetData.RequestSummary = await GetRequestSummary();
            }
            if (scope == (int)ScopeWidgetData.Tasks || scope == (int)ScopeWidgetData.All)
            {
                WidgetData.Tasks = await RetrieveUserTasks(taskFilter);
            }
            if (scope == (int)ScopeWidgetData.Schedule || scope == (int)ScopeWidgetData.All)
            {
                WidgetData.CalendarDetails = await RetrieveCalendarDetailsByDate(calendarDate, scheduleFilter);
            }

            return WidgetData;
        }

        public async Task<CalendarDto> RetrieveCalendarDetailsByMonth(CalendarRequestDto calendarRequestDto)
        {
            var calendarDetails = new CalendarDto();

            if (new Guid(_sessionService.GetContactId()) == Guid.Empty)
            {
                throw new UserFriendlyException("InvalidContactIdValue", System.Net.HttpStatusCode.BadRequest);
            }
            calendarRequestDto.PagingRequest = new PagingRequest
            {
                PageNo = 1,
                PageSize = 400,
                SortField = string.Empty,
                SortOrder = SortOrder.Ascending
            };

            var companyIds = _accessManagementAppService.GetActiveRolesAssociationForSignedInContact().Select(x => x.Company.Id).ToList();

            Task<ListPagingResponse<CalendarTasks>> taskRequests = null;
            Task<List<ProfileTasks>> taskRequestsTasks = null;
            Task<ListPagingResponse<CalendarDetailsDto>> taskMeetings = null;
            Task<ListPagingResponse<CalendarDetailsDto>> taskEvents = null;

            if (calendarRequestDto.OptionalId != null && calendarRequestDto.OptionalId != Guid.Empty)
            {
                switch (calendarRequestDto.TypeOfId)
                {
                    case (int)ScopeSearchCalender.Requests:
                        taskRequests = RetrieveCalendarRequests(calendarRequestDto.CalendarDate, calendarRequestDto.OptionalId, calendarRequestDto.SearchFilter, calendarRequestDto.PagingRequest, null, false, companyIds);
                        break;
                    case (int)ScopeSearchCalender.Tasks:
                        taskRequestsTasks = RetrieveCalendarTasks(calendarRequestDto.CalendarDate, calendarRequestDto.OptionalId, calendarRequestDto.SearchFilter, calendarRequestDto.PagingRequest, null, false, companyIds);
                        break;
                    case (int)ScopeSearchCalender.Meetings:
                        taskMeetings = RetrieveCalendarMeetings(calendarRequestDto.CalendarDate, calendarRequestDto.OptionalId, calendarRequestDto.SearchFilter, calendarRequestDto.PagingRequest);
                        break;
                    case (int)ScopeSearchCalender.Events:
                        taskEvents = RetrieveCalendarEvents(calendarRequestDto.CalendarDate, calendarRequestDto.OptionalId, calendarRequestDto.SearchFilter, calendarRequestDto.PagingRequest);
                        break;
                }
            }
            else
            {
                if (calendarRequestDto.ScheduleFilter == (int)ScopeSchedule.All || calendarRequestDto.ScheduleFilter == (int)ScopeSchedule.Events)
                {
                    taskEvents = RetrieveCalendarEvents(calendarRequestDto.CalendarDate, null, calendarRequestDto.SearchFilter, calendarRequestDto.PagingRequest);
                }
                if (calendarRequestDto.ScheduleFilter == (int)ScopeSchedule.All || calendarRequestDto.ScheduleFilter == (int)ScopeSchedule.Meetings)
                {
                    taskMeetings = RetrieveCalendarMeetings(calendarRequestDto.CalendarDate, null, calendarRequestDto.SearchFilter, calendarRequestDto.PagingRequest);
                }
                if (calendarRequestDto.ScheduleFilter == (int)ScopeSchedule.All || calendarRequestDto.ScheduleFilter == (int)ScopeSchedule.DueDates)
                {
                    taskRequestsTasks = RetrieveCalendarTasks(calendarRequestDto.CalendarDate, null, calendarRequestDto.SearchFilter, calendarRequestDto.PagingRequest, calendarRequestDto.CompanyId, false, companyIds);
                }
            }

            List<Task> tasks = new List<Task>();
            if (taskRequests != null) tasks.Add(taskRequests);
            if (taskRequestsTasks != null) tasks.Add(taskRequestsTasks);
            if (taskMeetings != null) tasks.Add(taskMeetings);
            if (taskEvents != null) tasks.Add(taskEvents);

            await Task.WhenAll(tasks);

            calendarDetails.Requests = taskRequests != null ? taskRequests.Result : new ListPagingResponse<CalendarTasks>();
            calendarDetails.Tasks = taskRequestsTasks != null ? taskRequestsTasks.Result : new List<ProfileTasks>();
            calendarDetails.Events = taskEvents != null ? taskEvents.Result : new ListPagingResponse<CalendarDetailsDto>();
            calendarDetails.Meetings = taskMeetings != null ? taskMeetings.Result : new ListPagingResponse<CalendarDetailsDto>();

            return calendarDetails;
        }

        public async Task<SiteSearchDto> RetrieveSiteSearchResult(string searchText, int? scope = (int)ScopeWidgetData.All)
        {
            var companyIds = _accessManagementAppService.GetActiveRolesAssociationForSignedInContact().Select(x => x.Company.Id).ToList();

            if (string.IsNullOrEmpty(searchText))
            {
                return null;
            }

            var calendarDetails = new CalendarDto();
            var siteSearchDto = new SiteSearchDto();

            Task<ListPagingResponse<CalendarTasks>> taskRequests = null;
            Task<List<ProfileTasks>> taskRequestsTasks = null;
            Task<ListPagingResponse<CalendarDetailsDto>> taskMeetings = null;
            Task<ListPagingResponse<CalendarDetailsDto>> taskEvents = null;

            if (scope == (int)ScopeWidgetData.RequestSummary || scope == (int)ScopeWidgetData.All)
            {
                taskRequests = RetrieveCalendarRequests(null, null, searchText, null, null, true, companyIds);
            }
            if (scope == (int)ScopeWidgetData.Tasks || scope == (int)ScopeWidgetData.All)
            {
                taskRequestsTasks = RetrieveCalendarTasks(null, null, searchText, null, null, true, companyIds);
            }
            if (scope == (int)ScopeWidgetData.Schedule || scope == (int)ScopeWidgetData.All)
            {
                taskMeetings = RetrieveCalendarMeetings(null, null, searchText, null, true);
                taskEvents = RetrieveCalendarEvents(null, null, searchText, null, true);
            }

            List<Task> tasks = new List<Task>();
            if (taskRequests != null) tasks.Add(taskRequests);
            if (taskRequestsTasks != null) tasks.Add(taskRequestsTasks);
            if (taskMeetings != null) tasks.Add(taskMeetings);
            if (taskEvents != null) tasks.Add(taskEvents);

            await Task.WhenAll(tasks);

            calendarDetails.Requests = taskRequests != null ? await taskRequests : new ListPagingResponse<CalendarTasks>();
            calendarDetails.Tasks = taskRequestsTasks != null ? await taskRequestsTasks : new List<ProfileTasks>();
            calendarDetails.Events = taskEvents != null ? await taskEvents : new ListPagingResponse<CalendarDetailsDto>();
            calendarDetails.Meetings = taskMeetings != null ? await taskMeetings : new ListPagingResponse<CalendarDetailsDto>();

            siteSearchDto.Requests = calendarDetails.Requests.ListResponse;
            siteSearchDto.Tasks = calendarDetails.Tasks.SelectMany(a => a.CalendarTasks).ToList();
            siteSearchDto.Events = calendarDetails.Events.ListResponse;
            siteSearchDto.Meetings = calendarDetails.Meetings.ListResponse;

            return siteSearchDto;
        }

        private async Task<ListPagingResponse<CalendarDetailsDto>> RetrieveCalendarEvents(DateTime? calendarDate = null, Guid? optionalId = null, string searchFilter = null, PagingRequest pagingRequest = null, bool siteSearchDate = false)
        {
            ListPagingResponse<CalendarDetailsDto> oResponse = new ListPagingResponse<CalendarDetailsDto>();

            var filteredAppointments = await GetCalendarEventsList(calendarDate, optionalId, siteSearchDate);

            var selectedFieldsQuery = filteredAppointments.Select(entity => entity.Contains("pwc_eventid") ? FillCalendarDetails(entity, "Event", searchFilter) : null).ToList();

            if (selectedFieldsQuery != null && selectedFieldsQuery.Count > 0)
            {
                selectedFieldsQuery.RemoveAll(x => x == null);
            }

            oResponse.TotalCount = selectedFieldsQuery.Count();

            if (pagingRequest != null)
            {
                var pageNo = pagingRequest.PageNo;
                int pageSize = pagingRequest.PageSize;

                oResponse.ListResponse = selectedFieldsQuery.Skip((pageNo - 1) * pageSize).Take(pageSize).ToList();
            }
            else
            {
                oResponse.ListResponse = selectedFieldsQuery;
            }

            return oResponse;
        }

        private async Task<List<Entity>> GetCalendarEventsList(DateTime? calendarDate = null, Guid? optionalId = null, bool siteSearchDate = false)
        {
            var orgContext = new OrganizationServiceContext(_crmService.GetInstance());

            IQueryable<Entity> EventsQuery = orgContext.CreateQuery(EntityNames.Appointment)
                .Where(x => (int)x["pwc_typetypecode"] == (int)AppointmentType.Event);

            DateTime startDate = DateTime.UtcNow.AddYears(-1);
            DateTime endDate = DateTime.UtcNow.AddYears(1);

            if (!siteSearchDate)
            {
                if (calendarDate.HasValue)
                {
                    startDate = new DateTime(calendarDate.Value.Year, calendarDate.Value.Month, 1);
                    var lastDayOfTheMonth = startDate.AddMonths(1).AddDays(-1);
                    endDate = new DateTime(lastDayOfTheMonth.Year, lastDayOfTheMonth.Month, lastDayOfTheMonth.Day, 23, 59, 59);
                }
                else
                {
                    startDate = DateTime.UtcNow.AddMonths(-3);
                    endDate = DateTime.UtcNow.AddMonths(3);
                }
            }

            EventsQuery = EventsQuery.Where(x => (DateTime)x["scheduledstart"] >= startDate && (DateTime)x["scheduledend"] <= endDate);

            if (optionalId != null && optionalId != Guid.Empty)
            {
                EventsQuery = EventsQuery.Where(x => ((Guid)x["activityid"]).Equals(optionalId));
            }

            var appointmentsList = EventsQuery.ToList();

            var filteredAppointments = appointmentsList
                .Where(x => ((EntityCollection)x["requiredattendees"]).Entities.Any(attendee => ((EntityReference)attendee["partyid"])?.Id == new Guid(_sessionService.GetContactId()) && ((EntityReference)attendee["partyid"])?.LogicalName == EntityNames.Contact)
                         || ((EntityCollection)x["optionalattendees"]).Entities.Any(attendee => ((EntityReference)attendee["partyid"])?.Id == new Guid(_sessionService.GetContactId()) && ((EntityReference)attendee["partyid"])?.LogicalName == EntityNames.Contact))
                .ToList();

            return filteredAppointments;
        }

        private async Task<ListPagingResponse<CalendarDetailsDto>> RetrieveCalendarMeetings(DateTime? calendarDate = null, Guid? optionalId = null, string searchFilter = null, PagingRequest pagingRequest = null, bool siteSearchDate = false)
        {
            ListPagingResponse<CalendarDetailsDto> oResponse = new ListPagingResponse<CalendarDetailsDto>();

            var filteredAppointments = await GetCalendarMeetingsList(calendarDate, optionalId, siteSearchDate);

            var selectedFieldsQuery = filteredAppointments.Select(entity => FillCalendarDetails(entity, "Meeting", searchFilter)).ToList();

            if (selectedFieldsQuery != null && selectedFieldsQuery.Count > 0)
            {
                selectedFieldsQuery.RemoveAll(x => x == null);
            }

            oResponse.TotalCount = selectedFieldsQuery.Count();

            if (pagingRequest != null)
            {
                var pageNo = pagingRequest.PageNo;
                int pageSize = pagingRequest.PageSize;

                oResponse.ListResponse = selectedFieldsQuery.Skip((pageNo - 1) * pageSize).Take(pageSize).ToList();
            }
            else
            {
                oResponse.ListResponse = selectedFieldsQuery;
            }

            return oResponse;
        }

        private async Task<List<Entity>> GetCalendarMeetingsList(DateTime? calendarDate = null, Guid? optionalId = null, bool siteSearchDate = false)
        {
            DateTime startDate = DateTime.UtcNow.AddYears(-1);
            DateTime endDate = DateTime.UtcNow.AddYears(1);

            if (!siteSearchDate)
            {
                if (calendarDate.HasValue)
                {
                    startDate = new DateTime(calendarDate.Value.Year, calendarDate.Value.Month, 1);
                    var lastDayOfTheMonth = startDate.AddMonths(1).AddDays(-1);
                    endDate = new DateTime(lastDayOfTheMonth.Year, lastDayOfTheMonth.Month, lastDayOfTheMonth.Day, 23, 59, 59);
                }
                else
                {
                    startDate = DateTime.UtcNow.AddMonths(-3);
                    endDate = DateTime.UtcNow.AddMonths(3);
                }
            }

            QueryExpression query = new QueryExpression(EntityNames.Appointment)
            {
                ColumnSet = new ColumnSet("subject", "pwc_meetingtitlear", "description", "scheduledstart", "location", "requiredattendees", "organizer")
            };

            var organizerLink = new LinkEntity
            {
                LinkFromEntityName = EntityNames.Appointment,
                LinkFromAttributeName = "activityid",
                LinkToEntityName = EntityNames.ActivityParty,
                LinkToAttributeName = "activityid",
                JoinOperator = JoinOperator.Inner,
                Columns = new ColumnSet("partyid"),
                EntityAlias = "OrganizerActivityParty"
            };

            var systemUserLink = new LinkEntity
            {
                LinkFromEntityName = EntityNames.ActivityParty,
                LinkFromAttributeName = "partyid",
                LinkToEntityName = EntityNames.SystemUser,
                LinkToAttributeName = "systemuserid",
                JoinOperator = JoinOperator.Inner,
                Columns = new ColumnSet("systemuserid", "fullname", "pwc_fullnamearabic"),
                EntityAlias = "SystemUser"
            };

            organizerLink.LinkEntities.Add(systemUserLink);
            query.LinkEntities.Add(organizerLink);

            query.Criteria.AddCondition("pwc_typetypecode", ConditionOperator.Equal, (int)AppointmentType.Meeting);
            query.Criteria.AddCondition("scheduledstart", ConditionOperator.GreaterEqual, startDate);
            query.Criteria.AddCondition("scheduledstart", ConditionOperator.LessEqual, endDate);


            if (optionalId != null && optionalId != Guid.Empty)
            {
                query.Criteria.AddCondition("activityid", ConditionOperator.Equal, optionalId);
            }

            var entityCollection = _crmService.GetInstance().RetrieveMultiple(query).Entities.ToList();

            var filteredAppointments = entityCollection
                .Where(x => ((EntityCollection)x["requiredattendees"]).Entities.Any(attendee => ((EntityReference)attendee["partyid"])?.Id == new Guid(_sessionService.GetContactId()) && ((EntityReference)attendee["partyid"])?.LogicalName == EntityNames.Contact)
                         || ((EntityCollection)x["optionalattendees"]).Entities.Any(attendee => ((EntityReference)attendee["partyid"])?.Id == new Guid(_sessionService.GetContactId()) && ((EntityReference)attendee["partyid"])?.LogicalName == EntityNames.Contact))
                .GroupBy(item => item.Id).Select(group => group.First()).ToList();

            return filteredAppointments;
        }

        private async Task<List<ProfileTasks>> RetrieveCalendarTasks(DateTime? calendarDate = null, Guid? optionalId = null, string searchFilter = null, PagingRequest pagingRequest = null, Guid? companyId = null, bool siteSearchDate = false, List<string> companiesIds = null)
        {
            var associations = await _accessManagementAppService.GetActiveRolesAssociationForSignedInContactWithDepartments();

            var tasksByProfile = associations.Select(a => new ProfileTasks { Association = a}).ToList();

            var contactId = _sessionService.GetContactId();
            foreach (var association in associations)
            {
                var permissions = await _accessManagementAppService.GetPermissionsByRoleIdAsync(association.PortalRole.Id);
                var accessLevel = GetPermissionAccessLevelOnRequestStep(permissions);

                if (accessLevel != AccessLevel.None)
                {
                    var result = await GetCalendarTasksList(calendarDate, Guid.Parse(association.Company.Id), siteSearchDate, null, pagingRequest, optionalId, searchFilter,association.PortalRole.Id);

                    var calendarTasksForAssociation = FillCalendarTasksList(result);

                    foreach (var task in calendarTasksForAssociation)
                    {
                        if(task.PortalContactId == contactId || CheckPermissionToReadHexaRequestStep(task.PortalRole, association.PortalRoleEntity, accessLevel, task.PortalContactId))
                        {
                            tasksByProfile.FirstOrDefault(a=>a.Association==association).CalendarTasks.Add(task);
                        }
                    }
                }
            }

            return tasksByProfile;
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

        private bool CheckPermissionToReadHexaRequestStep(PortalRole roleOnStep, PortalRole currentRole, AccessLevel accessLevel, string portalContactId)
        {
            if (roleOnStep == null)
            {
                return false;
            }

            if (accessLevel == AccessLevel.User)
            {
                return portalContactId != null && portalContactId == _sessionService.GetContactId();
            }

            if (accessLevel == AccessLevel.Basic)
            {
                return CheckRoleHierarchyByRoleId(roleOnStep, currentRole);
            }

            //deep access
            return CheckDeepAccessForTheRole(roleOnStep, currentRole);
        }

        public bool CheckRoleHierarchyByRoleId(PortalRole roleOnStep, PortalRole currentRole)
        {
            if (roleOnStep.ParentportalRole == null || currentRole.ParentportalRole == null)
            {
                if (roleOnStep.ParentportalRole == null && currentRole.ParentportalRole != null)
                {
                    return roleOnStep.Id == currentRole.ParentportalRole.Id;
                }
                if (roleOnStep.ParentportalRole != null && currentRole.ParentportalRole == null)
                {
                    return roleOnStep.ParentportalRole.Id == currentRole.Id;
                }
            }

            return roleOnStep.Id == currentRole.Id;
        }

        public bool CheckDeepAccessForTheRole(PortalRole roleOnStep, PortalRole currentRole)
        {
            if (roleOnStep.ParentportalRole == null || currentRole.ParentportalRole == null)
            {
                return true;
            }

            return currentRole.Department.Id == roleOnStep.Department.Id;
        }

        private async Task<EntityCollection> GetCalendarTasksList(DateTime? calendarDate = null, Guid? companyId = null, bool siteSearchDate = false, List<string> companiesIds = null, PagingRequest pagingRequest = null, Guid? optionalId = null, string searchFilter = null, string roleId = null)
        {
            DateTime startDate = DateTime.UtcNow.AddYears(-1);
            DateTime endDate = DateTime.UtcNow.AddYears(1);
            var opagingInfo = CRMOperations.GetPagingInfo(pagingRequest);

            if (!siteSearchDate)
            {
                startDate = new DateTime(calendarDate.Value.Year, calendarDate.Value.Month, 1);
                var lastDayOfTheMonth = startDate.AddMonths(1).AddDays(-1);
                endDate = new DateTime(lastDayOfTheMonth.Year, lastDayOfTheMonth.Month, lastDayOfTheMonth.Day, 23, 59, 59);
            }

            QueryExpression query = new QueryExpression(EntityNames.RequestStep)
            {
                ColumnSet = new ColumnSet("hexa_requeststepid", "hexa_stepstatus", "hexa_dueon", "hexa_request", "ownerid", "hexa_customer", "createdon", "hexa_portalcontact"),
                PageInfo = opagingInfo
            };

            LinkEntity processStepTemplateLink = new LinkEntity(EntityNames.RequestStep, EntityNames.ProcessStepTemplate, "hexa_processstep", "hexa_processsteptemplateid", JoinOperator.Inner)
            {
                Columns = new ColumnSet("hexa_name", "hexa_stepinstructions", "hexa_stepinstructionsar", "hexa_summaryar", "hexa_summary", "hexa_portalroleid"),
                EntityAlias = "processsteptemplateAlias"
            };
            processStepTemplateLink.LinkCriteria.AddCondition(new ConditionExpression
            {
                AttributeName = "hexa_isexternalusertypecode",
                Operator = ConditionOperator.Equal,
                Values = { 0 }
            });

            LinkEntity portalRoleLink = new LinkEntity(EntityNames.ProcessStepTemplate, EntityNames.PortalRole, "hexa_portalroleid", "hexa_portalroleid", JoinOperator.LeftOuter)
            {
                Columns = new ColumnSet("hexa_portalroleid", "hexa_name", "pwc_namear", "pwc_departmentid", "pwc_parentportalroleid", "pwc_showexternal", "pwc_showinternal", "pwc_roletypetypecode"),
                EntityAlias = "Role"
            };
            if (!string.IsNullOrEmpty(roleId))
            {
                portalRoleLink.LinkCriteria.AddCondition("hexa_portalroleid", ConditionOperator.Equal, Guid.Parse(roleId));
            }


            processStepTemplateLink.LinkEntities.Add(portalRoleLink);

            LinkEntity stepStatusTemplateLink = new LinkEntity(EntityNames.RequestStep, EntityNames.HexaStepStatusTemplate, "hexa_externalstepstatus", "hexa_stepstatustemplateid", JoinOperator.Inner);
            stepStatusTemplateLink.Columns = new ColumnSet("hexa_stepstatustemplateid", "hexa_nameen", "hexa_namear", "hexa_name");
            stepStatusTemplateLink.EntityAlias = "stepstatustemplateAlias";

            var queryFilter = new FilterExpression(LogicalOperator.And);

            queryFilter.AddCondition("hexa_dueon", ConditionOperator.GreaterEqual, startDate);
            queryFilter.AddCondition("hexa_dueon", ConditionOperator.LessEqual, endDate);



            var contactFilter = new FilterExpression(LogicalOperator.Or);
            contactFilter.AddCondition("hexa_portalcontact", ConditionOperator.Equal, _sessionService.GetContactId());
            contactFilter.AddCondition("Role", "hexa_portalroleid", ConditionOperator.NotNull);
            queryFilter.AddFilter(contactFilter);

            if (companiesIds != null)
            {
                var companyFilter = new FilterExpression(LogicalOperator.And);

                companyFilter.AddCondition("hexa_customer", ConditionOperator.In, companiesIds.ToArray());
                queryFilter.AddFilter(companyFilter);
            }

            if (companyId != null || companyId != Guid.Empty)
            {
                var companyFilter = new FilterExpression(LogicalOperator.And);

                companyFilter.AddCondition("hexa_customer", ConditionOperator.Equal, companyId);
                queryFilter.AddFilter(companyFilter);
            }

            if (optionalId != null && optionalId != Guid.Empty)
            {
                queryFilter.AddCondition("hexa_requeststepid", ConditionOperator.Equal, optionalId);
            }

            if (!string.IsNullOrEmpty(searchFilter))
            {
                var includeEFilter = new FilterExpression(LogicalOperator.Or);
                includeEFilter.AddCondition(new ConditionExpression("processsteptemplateAlias", "hexa_stepinstructions", ConditionOperator.Like, $"%{searchFilter}%"));
                includeEFilter.AddCondition(new ConditionExpression("processsteptemplateAlias", "hexa_summary", ConditionOperator.Like, $"%{searchFilter}%"));
                query.Criteria.AddFilter(includeEFilter);
            }

            query.AddOrder("hexa_stepstatus", OrderType.Ascending);

            query.LinkEntities.Add(processStepTemplateLink);
            query.LinkEntities.Add(stepStatusTemplateLink);
            query.Criteria.AddFilter(queryFilter);

            EntityCollection results = _crmService.GetInstance().RetrieveMultiple(query);

            return results;
        }

        private List<CalendarTasks> FillCalendarTasksList(EntityCollection tasks)
        {
            var calendarTasks = new List<CalendarTasks>();

            var finalQuery = tasks.Entities
                .Select(e => new
                {
                    HexaRequestStepId = e.GetAttributeValue<Guid>("hexa_requeststepid"),
                    HexaPortalContact = e.GetAttributeValue<EntityReference>("hexa_portalcontact"),
                    HexaStepStatus = e.GetAttributeValue<EntityReference>("hexa_stepstatus"),
                    HexaDueOn = e.GetAttributeValue<DateTime>("hexa_dueon"),
                    HexaRequest = e.GetAttributeValue<EntityReference>("hexa_request"),
                    HexaInitiator = e.GetAttributeValue<EntityReference>("ownerid"),
                    HexaCustomer = e.GetValueByAttributeName<EntityReferenceDto>("hexa_customer"),
                    HexaProcessStepTemplateName = e.Contains("processsteptemplateAlias.hexa_summary") ? e.GetAttributeValue<AliasedValue>("processsteptemplateAlias.hexa_summary").Value : null,
                    HexaProcessStepTemplateNameAr = e.Contains("processsteptemplateAlias.hexa_summaryar") ? e.GetAttributeValue<AliasedValue>("processsteptemplateAlias.hexa_summaryar").Value : null,
                    HexaProcessStepTemplateDescription = e.Contains("processsteptemplateAlias.hexa_stepinstructions") ? e.GetAttributeValue<AliasedValue>("processsteptemplateAlias.hexa_stepinstructions").Value : null,
                    HexaProcessStepTemplateDescriptionAr = e.Contains("processsteptemplateAlias.hexa_stepinstructionsar") ? e.GetAttributeValue<AliasedValue>("processsteptemplateAlias.hexa_stepinstructionsar").Value : null,
                    HexaStepStatusTemplateId = e.Contains("stepstatustemplateAlias.hexa_stepstatustemplateid") ? e.GetAttributeValue<AliasedValue>("stepstatustemplateAlias.hexa_stepstatustemplateid").Value : null,
                    HexaStepStatusTemplateName = e.Contains("stepstatustemplateAlias.hexa_nameen") ? e.GetAttributeValue<AliasedValue>("stepstatustemplateAlias.hexa_nameen").Value : null,
                    HexaStepStatusTemplateNameAr = e.Contains("stepstatustemplateAlias.hexa_namear") ? e.GetAttributeValue<AliasedValue>("stepstatustemplateAlias.hexa_namear").Value : null,
                    PortalRole = GetPortalRoleByAliasedValue(e)
                });


            var result = finalQuery.ToList();

            foreach (var entity in result)
            {

                var calendarTask = new CalendarTasks();

                var request = new EntityReferenceDto()
                {
                    Name = entity.HexaRequest != null ? ((EntityReference)entity.HexaRequest).Name : string.Empty,
                    Id = entity.HexaRequest != null ? ((EntityReference)entity.HexaRequest).Id.ToString() : string.Empty,
                };

                calendarTask.RequestStepId = entity.HexaRequestStepId != null ? entity.HexaRequestStepId : (Guid?)null;
                calendarTask.DueDate = entity.HexaDueOn != null ? (DateTime)entity.HexaDueOn : (DateTime?)null;
                calendarTask.OverDue = calendarTask.DueDate.HasValue && calendarTask.DueDate < DateTime.UtcNow;
                calendarTask.Company = entity.HexaCustomer != null ? (EntityReferenceDto)entity.HexaCustomer : null;
                calendarTask.Initiator = entity.HexaInitiator != null ? entity.HexaInitiator.Name : string.Empty;
                calendarTask.InitiatorId = entity.HexaInitiator != null ? entity.HexaInitiator.Id : Guid.Empty;
                calendarTask.Name = entity.HexaProcessStepTemplateName != null ? entity.HexaProcessStepTemplateName.ToString() : string.Empty;
                calendarTask.NameAr = entity.HexaProcessStepTemplateNameAr != null ? entity.HexaProcessStepTemplateNameAr.ToString() : string.Empty;
                calendarTask.Description = entity.HexaProcessStepTemplateDescription != null ? entity.HexaProcessStepTemplateDescription.ToString() : string.Empty;
                calendarTask.DescriptionAr = entity.HexaProcessStepTemplateDescriptionAr != null ? entity.HexaProcessStepTemplateDescriptionAr.ToString() : string.Empty;
                calendarTask.Request = request;
                calendarTask.Type = "Task";
                calendarTask.TypeAr = "مهمة";
                calendarTask.PortalRole = entity.PortalRole;
                calendarTask.PortalContactId = entity.HexaPortalContact != null ? entity.HexaPortalContact.Id.ToString() : string.Empty;

                if (entity.HexaStepStatusTemplateId != null)
                {
                    EntityOptionSetDto entityOptionSetDto = new EntityOptionSetDto();


                    entityOptionSetDto.Value = entity.HexaStepStatusTemplateId.ToString();

                    if (entity.HexaStepStatusTemplateName != null)
                    {
                        entityOptionSetDto.Name = entity.HexaStepStatusTemplateName.ToString();
                    }

                    if (entity.HexaStepStatusTemplateNameAr != null)
                    {
                        entityOptionSetDto.NameAr = entity.HexaStepStatusTemplateNameAr.ToString();
                    }

                    calendarTask.Status = entityOptionSetDto;
                }

                calendarTasks.Add(calendarTask);

            }

            return calendarTasks;
        }

        private PortalRole GetPortalRoleByAliasedValue(Entity entity)
        {
            var portalRole = new PortalRole();

            var role = CRMOperations.GetValueByAttributeName<AliasedValue>(entity, "Role.hexa_portalroleid")?.Value??null;
            if (role != null)
            {
                portalRole.Id = role.ToString();
                var department = ((EntityReference)CRMOperations.GetValueByAttributeName<AliasedValue>(entity, "Role.pwc_departmentid")?.Value);
                var parentRole = ((EntityReference)CRMOperations.GetValueByAttributeName<AliasedValue>(entity, "Role.pwc_parentportalroleid")?.Value);
                portalRole.Department = department == null ? null : new EntityReferenceDto { Id = department.Id.ToString(), Name = department.Name };
                portalRole.ParentportalRole = parentRole == null ? null : new EntityReferenceDto { Id = parentRole.Id.ToString(), Name = parentRole.Name };
                portalRole.Name = CRMOperations.GetValueByAttributeName<AliasedValue>(entity, "Role.hexa_name")?.Value.ToString() ?? string.Empty;
                portalRole.NameAr = CRMOperations.GetValueByAttributeName<AliasedValue>(entity, "Role.pwc_namear")?.Value.ToString() ?? string.Empty;
            }
            return portalRole;
        }

        private async Task<ListPagingResponse<CalendarTasks>> RetrieveCalendarRequests(DateTime? calendarDate = null, Guid? optionalId = null, string searchFilter = null, PagingRequest pagingRequest = null, Guid? companyId = null, bool siteSearchDate = false, List<string> companiesIds = null)
        {
            ListPagingResponse<CalendarTasks> oResponse = new ListPagingResponse<CalendarTasks>();

            EntityCollection results = await GetCalendarRequestsList(calendarDate, companyId, siteSearchDate, companiesIds, pagingRequest, optionalId, searchFilter);

            var calendarTasks = await FillCalendarRequestsList(results);

            oResponse.TotalCount = results.TotalRecordCount;
            oResponse.ListResponse = calendarTasks;


            return oResponse;
        }

        private async Task<EntityCollection> GetCalendarRequestsList(DateTime? calendarDate = null, Guid? companyId = null, bool siteSearchDate = false, List<string> companiesIds = null, PagingRequest pagingRequest = null, Guid? optionalId = null, string searchFilter = null)
        {
            DateTime startDate = DateTime.UtcNow.AddYears(-1);
            DateTime endDate = DateTime.UtcNow.AddYears(1);
            var opagingInfo = CRMOperations.GetPagingInfo(pagingRequest);
            if (!siteSearchDate)
            {
                startDate = new DateTime(calendarDate.Value.Year, calendarDate.Value.Month, 1);
                var lastDayOfTheMonth = startDate.AddMonths(1).AddDays(-1);
                endDate = new DateTime(lastDayOfTheMonth.Year, lastDayOfTheMonth.Month, lastDayOfTheMonth.Day, 23, 59, 59);
            }

            QueryExpression query = new QueryExpression(EntityNames.HexaRequests)
            {
                ColumnSet = new ColumnSet("hexa_requestid", "hexa_name", "ownerid", "hexa_dueon", "hexa_internalstatus", "createdon", "hexa_customer"),
                PageInfo = opagingInfo
            };

            LinkEntity processTemplateLink = new LinkEntity(EntityNames.HexaRequests, EntityNames.ProcessTemplate, "hexa_processtemplate", "hexa_processtemplateid", JoinOperator.Inner)
            {
                Columns = new ColumnSet("hexa_name", "hexa_description", "hexa_portalroleid"),
                EntityAlias = "processtemplateAlias"
            };
            processTemplateLink.LinkCriteria.AddCondition(new ConditionExpression
            {
                AttributeName = "hexa_isexternalusertypecode",
                Operator = ConditionOperator.Equal,
                Values = { 0 }
            });

            LinkEntity processStatusTemplateLink = new LinkEntity(EntityNames.HexaRequests, EntityNames.ProcessStatusTemplate, "hexa_externalstatus", "hexa_processstatustemplateid", JoinOperator.Inner);
            processStatusTemplateLink.Columns = new ColumnSet("hexa_processstatustemplateid", "hexa_nameen", "hexa_namear", "hexa_name");
            processStatusTemplateLink.EntityAlias = "processstatustemplateAlias";


            LinkEntity portalRoleLink = new LinkEntity(EntityNames.ProcessTemplate, EntityNames.PortalRole, "hexa_portalroleid", "hexa_portalroleid", JoinOperator.LeftOuter)
            {
                Columns = new ColumnSet("hexa_portalroleid", "hexa_name", "pwc_namear", "pwc_departmentid", "pwc_parentportalroleid", "pwc_showexternal", "pwc_showinternal", "pwc_roletypetypecode"),
                EntityAlias = "Role"
            };

            portalRoleLink.LinkCriteria.AddCondition(new ConditionExpression
            {
                AttributeName = "pwc_roletypetypecode",
                Operator = ConditionOperator.Equal,
                Values = { await _userPermissionAppService.LoggedInUserRoleType() }
            });

            processTemplateLink.LinkEntities.Add(portalRoleLink);



            query.Criteria.AddCondition("createdon", ConditionOperator.GreaterEqual, startDate);
            query.Criteria.AddCondition("createdon", ConditionOperator.LessEqual, endDate);


            var contactFilter = new FilterExpression(LogicalOperator.Or);
            contactFilter.AddCondition("hexa_portalcontact", ConditionOperator.Equal, _sessionService.GetContactId());
            contactFilter.AddCondition("Role", "hexa_portalroleid", ConditionOperator.NotNull);
            query.Criteria.AddFilter(contactFilter);

            if (companiesIds != null)
            {
                var companyFilter = new FilterExpression(LogicalOperator.And);

                companyFilter.AddCondition("hexa_customer", ConditionOperator.In, companiesIds.ToArray());

                query.Criteria.AddFilter(companyFilter);
            }

            if (optionalId != null && optionalId != Guid.Empty)
            {
                query.Criteria.AddCondition("hexa_requestid", ConditionOperator.Equal, optionalId);
            }

            if (!string.IsNullOrEmpty(searchFilter))
            {
                var includeEFilter = new FilterExpression(LogicalOperator.Or);
                includeEFilter.AddCondition(new ConditionExpression("processtemplateAlias", "hexa_name", ConditionOperator.Like, $"%{searchFilter}%"));
                includeEFilter.AddCondition(new ConditionExpression("processtemplateAlias", "hexa_description", ConditionOperator.Like, $"%{searchFilter}%"));
                query.Criteria.AddFilter(includeEFilter);
            }

            query.Criteria.FilterOperator = LogicalOperator.And;

            query.LinkEntities.Add(processTemplateLink);
            query.LinkEntities.Add(processStatusTemplateLink);

            EntityCollection results = _crmService.GetInstance().RetrieveMultiple(query);

            return results;
        }

        private async Task<List<CalendarTasks>> FillCalendarRequestsList(EntityCollection requests)
        {
            var calendarTasks = new List<CalendarTasks>();

            var finalQuery = requests.Entities
                .Select(e => new
                {
                    HexaRequestId = e.GetAttributeValue<Guid>("hexa_requestid"),
                    HexaName = e.GetAttributeValue<string>("hexa_name"),
                    HexaInitiator = e.GetAttributeValue<EntityReference>("ownerid"),
                    HexaCustomer = e.GetValueByAttributeName<EntityReferenceDto>("hexa_customer"),
                    HexaDueOn = e.GetAttributeValue<DateTime>("hexa_dueon"),
                    HexaInternalStatus = e.GetAttributeValue<EntityReference>("hexa_internalstatus"),
                    HexaProcessTemplateName = e.Contains("processtemplateAlias.hexa_name") ? e.GetAttributeValue<AliasedValue>("processtemplateAlias.hexa_name").Value : null,
                    HexaProcessTemplateDescription = e.Contains("processtemplateAlias.hexa_description") ? e.GetAttributeValue<AliasedValue>("processtemplateAlias.hexa_description").Value : null,
                    HexaStatusTemplateId = e.Contains("processstatustemplateAlias.hexa_processstatustemplateid") ? e.GetAttributeValue<AliasedValue>("processstatustemplateAlias.hexa_processstatustemplateid").Value : null,
                    HexaStatusTemplateName = e.Contains("processstatustemplateAlias.hexa_nameen") ? e.GetAttributeValue<AliasedValue>("processstatustemplateAlias.hexa_nameen").Value : null,
                    HexaStatusTemplateNameAr = e.Contains("processstatustemplateAlias.hexa_namear") ? e.GetAttributeValue<AliasedValue>("processstatustemplateAlias.hexa_namear").Value : null,
                });

            var result = finalQuery.ToList();


            foreach (var entity in result)
            {
                var calendarTask = new CalendarTasks();

                var request = new EntityReferenceDto()
                {
                    Name = entity.HexaName != null ? entity.HexaName.ToString() : string.Empty,
                    Id = entity.HexaRequestId != null ? entity.HexaRequestId.ToString() : string.Empty,
                };

                calendarTask.DueDate = GetRequestDueOnDate(entity.HexaRequestId);
                calendarTask.OverDue = calendarTask.DueDate.HasValue && calendarTask.DueDate < DateTime.UtcNow;
                calendarTask.Initiator = entity.HexaInitiator != null ? entity.HexaInitiator.Name : string.Empty;
                calendarTask.Company = entity.HexaCustomer != null ? (EntityReferenceDto)entity.HexaCustomer : null;
                calendarTask.InitiatorId = entity.HexaInitiator != null ? entity.HexaInitiator.Id : Guid.Empty;
                calendarTask.Name = entity.HexaProcessTemplateName != null ? entity.HexaProcessTemplateName.ToString() : string.Empty;
                calendarTask.Description = entity.HexaProcessTemplateDescription != null ? entity.HexaProcessTemplateDescription.ToString() : string.Empty;
                calendarTask.Request = request;
                calendarTask.Type = "Request";
                calendarTask.TypeAr = "طلب";

                if (entity.HexaStatusTemplateId != null)
                {
                    EntityOptionSetDto entityOptionSetDto = new EntityOptionSetDto();

                    entityOptionSetDto.Value = entity.HexaStatusTemplateId.ToString();

                    if (entity.HexaStatusTemplateName != null)
                    {
                        entityOptionSetDto.Name = entity.HexaStatusTemplateName.ToString();
                    }

                    if (entity.HexaStatusTemplateNameAr != null)
                    {
                        entityOptionSetDto.NameAr = entity.HexaStatusTemplateNameAr.ToString();
                    }

                    calendarTask.Status = entityOptionSetDto;
                }

                calendarTasks.Add(calendarTask);
            }

            return calendarTasks;
        }

        private DateTime? GetRequestDueOnDate(Guid requestId)
        {
            var orgContext = new OrganizationServiceContext(_crmService.GetInstance());

            var values = orgContext.CreateQuery(EntityNames.RequestStep)
                                .Where(x => ((EntityReference)x["hexa_request"]).Id == requestId && ((int)x["statecode"]) == (int)StateCode.Active)
                                .Select(x => x.Contains("hexa_dueon") ? (DateTime?)x["hexa_dueon"] : null)
                                .ToList();

            values.RemoveAll(x => x == null);

            if (values.Count > 0)
            {
                List<DateTime> dates = values.Cast<DateTime>().ToList();

                DateTime now = DateTime.UtcNow;
                return dates.OrderBy(date => Math.Abs((date - now).TotalSeconds)).FirstOrDefault();
            }

            return null;
        }

        private async Task<List<CalendarDetailsDto>> RetrieveCalendarDetailsByDate(DateTime? calendarDate, int? scheduleFilter = (int)ScopeSchedule.All)
        {
            var calendarDetails = new List<CalendarDetailsDto>();

            ListPagingResponse<CalendarDetailsDto> taskMeetings = null;
            ListPagingResponse<CalendarDetailsDto> taskEvents = null;

            if (new Guid(_sessionService.GetContactId()) == Guid.Empty)
            {
                throw new UserFriendlyException("InvalidContactIdValue", System.Net.HttpStatusCode.BadRequest);
            }

            if (scheduleFilter == (int)ScopeSchedule.All || scheduleFilter == (int)ScopeSchedule.Events)
            {
                taskEvents = await RetrieveCalendarEvents(calendarDate);
                calendarDetails.AddRange(taskEvents.ListResponse);
            }
            if (scheduleFilter == (int)ScopeSchedule.All || scheduleFilter == (int)ScopeSchedule.Meetings)
            {
                taskMeetings = await RetrieveCalendarMeetings(calendarDate);
                calendarDetails.AddRange(taskMeetings.ListResponse);
            }

            return calendarDetails;
        }

        private async Task<List<UserTask>> RetrieveUserTasks(Guid? taskFilter = null)
        {
            var usertasks = new List<UserTask>();

            var RoleType = await _userPermissionAppService.LoggedInUserRoleType();
            List<Entity> filteredEntity = GetUserTasks(RoleType, taskFilter);
            usertasks = await FillUserTasks(filteredEntity);

            return usertasks;
        }

        private List<Entity> GetUserTasks(int RoleType, Guid? TaskFilter = null)
        {
            QueryExpression query = new QueryExpression(EntityNames.RequestStep);
            query.ColumnSet = new ColumnSet("hexa_requeststepid", "hexa_name", "createdon", "hexa_request", "hexa_processstep", "hexa_portalcontact");

            query.Criteria.AddCondition(new ConditionExpression
            {
                AttributeName = "hexa_customer",
                Operator = ConditionOperator.Equal,
                Values = { new Guid(_sessionService.GetCompanyId()) }
            });

            LinkEntity processStepTemplateLink = new LinkEntity
            {
                LinkFromEntityName = EntityNames.RequestStep,
                LinkFromAttributeName = "hexa_processstep",
                LinkToEntityName = EntityNames.ProcessStepTemplate,
                LinkToAttributeName = "hexa_processsteptemplateid",
                JoinOperator = JoinOperator.Inner,
                EntityAlias = "processsteptemplateAlias",
                Columns = new ColumnSet("hexa_stepinstructions", "hexa_stepinstructionsar", "hexa_name", "hexa_summaryar", "hexa_summary", "hexa_portalroleid")
            };

            processStepTemplateLink.LinkCriteria.AddCondition(new ConditionExpression
            {
                AttributeName = "hexa_isexternalusertypecode",
                Operator = ConditionOperator.Equal,
                Values = { (int)ExternalUser.Yes }
            });

            LinkEntity portalRoleLink = new LinkEntity(EntityNames.ProcessStepTemplate, EntityNames.PortalRole, "hexa_portalroleid", "hexa_portalroleid", JoinOperator.LeftOuter)
            {
                Columns = new ColumnSet("hexa_portalroleid", "pwc_roletypetypecode"),
                EntityAlias = "Role"
            };
            portalRoleLink.LinkCriteria.AddCondition(new ConditionExpression
            {
                AttributeName = "pwc_roletypetypecode",
                Operator = ConditionOperator.Equal,
                Values = { RoleType }
            });

            processStepTemplateLink.LinkEntities.Add(portalRoleLink);

            LinkEntity stepStatusTemplateLink = new LinkEntity(EntityNames.RequestStep, EntityNames.HexaStepStatusTemplate, "hexa_externalstepstatus", "hexa_stepstatustemplateid", JoinOperator.Inner);
            stepStatusTemplateLink.Columns = new ColumnSet("hexa_stepstatustemplateid", "hexa_nameen", "hexa_namear", "hexa_name");
            stepStatusTemplateLink.EntityAlias = "stepstatustemplateAlias";

            if (TaskFilter.HasValue && TaskFilter.Value != Guid.Empty)
            {
                stepStatusTemplateLink.LinkCriteria.AddCondition(new ConditionExpression
                {
                    AttributeName = "hexa_stepstatustemplateid",
                    Operator = ConditionOperator.Equal,
                    Values = { TaskFilter }
                });
            }

            query.LinkEntities.Add(processStepTemplateLink);
            query.LinkEntities.Add(stepStatusTemplateLink);

            var portalFilter = new FilterExpression(LogicalOperator.Or);
            portalFilter.AddCondition("Role", "hexa_portalroleid", ConditionOperator.NotNull);
            query.Criteria.AddFilter(portalFilter);


            EntityCollection results = _crmService.GetInstance().RetrieveMultiple(query);
            List<Entity> filteredEntity;

            try
            {
                filteredEntity = results.Entities.ToList().GroupBy(a => ((EntityReference)a["hexa_processstep"]).Id)
                    .Select(g => g.OrderByDescending(t => ((DateTime)t["createdon"])).First()).ToList();
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return filteredEntity;
        }

        private async Task<List<UserTask>> FillUserTasks(List<Entity> filteredEntity)
        {
            var usertasks = new List<UserTask>();
            var permissions = await _accessManagementAppService.GetAuthorizedPermissions();
            var accessLevel = GetPermissionAccessLevelOnRequestStep(permissions);
            var currentRole = await _accessManagementAppService.GetRoleAndDepartmentByRoleId(_sessionService.GetRoleId());

            foreach (var entity in filteredEntity)
            {
                var portalRoleEntity = GetPortalRoleByAliasedValue(entity);
                var portalContactId = CRMOperations.GetValueByAttributeName<EntityReference>(entity, "hexa_portalcontact")?.Id.ToString() ?? string.Empty;

                var isAuthorizedToRead = CheckPermissionToReadHexaRequestStep(portalRoleEntity, currentRole, accessLevel, portalContactId);
                if (isAuthorizedToRead)
                {
                    var userTask = new UserTask();

                    userTask.Id = entity.GetAttributeValue<Guid>("hexa_requeststepid");
                    userTask.ParentRequestId = entity.GetAttributeValue<EntityReference>("hexa_request").Id;

                    if (entity.Attributes.Any(a => a.Key.Contains("processsteptemplateAlias")))
                    {
                        if (entity.Attributes.Contains("processsteptemplateAlias.hexa_stepinstructions"))
                        {
                            AliasedValue aliasedValue = (AliasedValue)entity.Attributes["processsteptemplateAlias.hexa_stepinstructions"];
                            userTask.Description = (string)aliasedValue.Value;
                        }
                        if (entity.Attributes.Contains("processsteptemplateAlias.hexa_stepinstructionsar"))
                        {
                            AliasedValue aliasedValue = (AliasedValue)entity.Attributes["processsteptemplateAlias.hexa_stepinstructionsar"];
                            userTask.DescriptionAr = (string)aliasedValue.Value;
                        }
                        if (entity.Attributes.Contains("processsteptemplateAlias.hexa_summaryar"))
                        {
                            AliasedValue aliasedValue = (AliasedValue)entity.Attributes["processsteptemplateAlias.hexa_summaryar"];
                            userTask.NameAr = (string)aliasedValue.Value;
                        }
                        if (entity.Attributes.Contains("processsteptemplateAlias.hexa_summary"))
                        {
                            AliasedValue aliasedValue = (AliasedValue)entity.Attributes["processsteptemplateAlias.hexa_summary"];
                            userTask.Name = (string)aliasedValue.Value;
                        }
                    }

                    if (entity.Attributes.Any(a => a.Key.Contains("stepstatustemplateAlias")))
                    {
                        if (entity.Attributes.Contains("stepstatustemplateAlias.hexa_stepstatustemplateid"))
                        {
                            EntityOptionSetDto entityOptionSetDto = new EntityOptionSetDto();

                            AliasedValue aliasedValue = (AliasedValue)entity.Attributes["stepstatustemplateAlias.hexa_stepstatustemplateid"];
                            entityOptionSetDto.Value = ((Guid)aliasedValue.Value).ToString();

                            if (entity.Attributes.Contains("stepstatustemplateAlias.hexa_nameen"))
                            {
                                aliasedValue = (AliasedValue)entity.Attributes["stepstatustemplateAlias.hexa_nameen"];
                                entityOptionSetDto.Name = (string)aliasedValue.Value;
                            }

                            if (entity.Attributes.Contains("stepstatustemplateAlias.hexa_namear"))
                            {
                                aliasedValue = (AliasedValue)entity.Attributes["stepstatustemplateAlias.hexa_namear"];
                                entityOptionSetDto.NameAr = (string)aliasedValue.Value;
                            }

                            userTask.Status = entityOptionSetDto;
                        }
                    }

                    usertasks.Add(userTask);
                }
            }

            return usertasks;
        }

        private CalendarDetailsDto FillCalendarDetails(Entity entity, string type, string searchFilter = null)
        {
            CalendarDetailsDto calendarDetailsDto = new CalendarDetailsDto();

            switch (type)
            {
                case "Event":
                    calendarDetailsDto = GetCalendarEventsDetail(entity, type, searchFilter);
                    break;
                case "Meeting":
                    calendarDetailsDto = GetCalendarMeetingsDetail(entity, type, searchFilter);
                    break;
            }

            return calendarDetailsDto;
        }

        private CalendarDetailsDto GetCalendarMeetingsDetail(Entity entity, string type, string searchFilter = null)
        {
            CalendarDetailsDto calendarDetailsDto = new CalendarDetailsDto();

            calendarDetailsDto.Id = entity.Id;
            calendarDetailsDto.EventType = type;
            calendarDetailsDto.EventTypeAr = "اجتماع";
            calendarDetailsDto.Title = entity.GetValueByAttributeName<string>("subject");
            calendarDetailsDto.TitleAr = entity.GetValueByAttributeName<string>("pwc_meetingtitlear");
            calendarDetailsDto.Description = entity.GetValueByAttributeName<string>("description");
            calendarDetailsDto.Date = entity.GetValueByAttributeName<DateTime>("scheduledstart").ToString("dd MMM yyyy");
            calendarDetailsDto.Time = entity.GetValueByAttributeName<DateTime>("scheduledstart").ToString("h:mm tt");

            if (!string.IsNullOrEmpty(calendarDetailsDto.Date) && (DateTime.Parse(calendarDetailsDto.Date)).Year == DateTime.MinValue.Year)
            {
                calendarDetailsDto.Date = string.Empty;
                calendarDetailsDto.Time = string.Empty;
            }


            var systemId = entity.GetValueByAttributeName<AliasedValue>("SystemUser.systemuserid")?.Value.ToString() ?? string.Empty;
            calendarDetailsDto.OrganizerId = string.IsNullOrEmpty(systemId) ? Guid.Empty : new Guid(systemId);

            if (calendarDetailsDto.OrganizerId != Guid.Empty)
            {
                calendarDetailsDto.OrganizerName = entity.GetValueByAttributeName<AliasedValue>("SystemUser.fullname")?.Value.ToString() ?? string.Empty;
                calendarDetailsDto.OrganizerNameAr = entity.GetValueByAttributeName<AliasedValue>("SystemUser.pwc_fullnamearabic")?.Value.ToString() ?? string.Empty;

            }

            calendarDetailsDto.Location = entity.GetValueByAttributeName<string>("location");
            calendarDetailsDto.Attendees = FillAttendees(entity.GetValueByAttributeName<EntityCollection>("requiredattendees"), entity.GetValueByAttributeName<EntityCollection>("optionalattendees"));

            if (!string.IsNullOrEmpty(searchFilter))
            {
                if (!((!string.IsNullOrEmpty(calendarDetailsDto.Title) && calendarDetailsDto.Title.ToLower().Contains(searchFilter.ToLower())) ||
                    (!string.IsNullOrEmpty(calendarDetailsDto.Description) && calendarDetailsDto.Description.ToLower().Contains(searchFilter.ToLower()))))
                {
                    calendarDetailsDto = null;
                }
            }

            return calendarDetailsDto;
        }

        private CalendarDetailsDto GetCalendarEventsDetail(Entity entity, string type, string searchFilter = null)
        {
            CalendarDetailsDto calendarDetailsDto = new CalendarDetailsDto();

            Entity eventEntity = GetEventDetails(entity.GetValueByAttributeName<EntityReferenceDto>("pwc_eventid").Id);

            calendarDetailsDto.Id = entity.Id;
            calendarDetailsDto.EventType = type;
            calendarDetailsDto.EventTypeAr = "حدث";
            calendarDetailsDto.Title = eventEntity.GetValueByAttributeName<string>("pwc_title");
            calendarDetailsDto.TitleAr = eventEntity.GetValueByAttributeName<string>("pwc_name");
            calendarDetailsDto.Description = eventEntity.GetValueByAttributeName<string>("pwc_description");
            calendarDetailsDto.Date = eventEntity.GetValueByAttributeName<DateTime>("pwc_eventdatetime").ToString("dd MMM yyyy");
            calendarDetailsDto.Time = eventEntity.GetValueByAttributeName<DateTime>("pwc_eventdatetime").ToString("h:mm tt");

            if (!string.IsNullOrEmpty(calendarDetailsDto.Date) && (DateTime.Parse(calendarDetailsDto.Date)).Year == DateTime.MinValue.Year)
            {
                calendarDetailsDto.Date = string.Empty;
                calendarDetailsDto.Time = string.Empty;
            }

            calendarDetailsDto.OrganizerName = eventEntity.GetValueByAttributeName<string>("pwc_organizername");
            calendarDetailsDto.OrganizerNameAr = eventEntity.GetValueByAttributeName<string>("pwc_organizationnamearabic");
            calendarDetailsDto.OrganizerId = Guid.Empty;
            calendarDetailsDto.Location = eventEntity.GetValueByAttributeName<string>("pwc_location");
            calendarDetailsDto.Attendees = FillAttendees(entity.GetValueByAttributeName<EntityCollection>("requiredattendees"), entity.GetValueByAttributeName<EntityCollection>("optionalattendees"));

            if (!string.IsNullOrEmpty(searchFilter))
            {
                if (!((!string.IsNullOrEmpty(calendarDetailsDto.Title) && calendarDetailsDto.Title.ToLower().Contains(searchFilter.ToLower())) ||
                    (!string.IsNullOrEmpty(calendarDetailsDto.Description) && calendarDetailsDto.Description.ToLower().Contains(searchFilter.ToLower()))))
                {
                    calendarDetailsDto = null;
                }
            }

            return calendarDetailsDto;
        }

        private List<Attendees> FillAttendees(EntityCollection requiredAttendee, EntityCollection optionalAttendee)
        {
            var attendees = new List<Attendees>();
            List<Guid> requiredAttendeeIds = new List<Guid>();
            List<Guid> optionalAttendeeIds = new List<Guid>();

            foreach (var entity in requiredAttendee.Entities)
            {
                EntityReference entityReference = entity.GetValueByAttributeName<EntityReference>("partyid");

                if (entityReference != null && entityReference.Id != Guid.Empty)
                {
                    requiredAttendeeIds.Add(entityReference.Id);
                }
            }

            foreach (var entity in optionalAttendee.Entities)
            {
                EntityReference entityReference = entity.GetValueByAttributeName<EntityReference>("partyid");

                if (entityReference != null && entityReference.Id != Guid.Empty)
                {
                    optionalAttendeeIds.Add(entityReference.Id);
                }
            }

            List<Guid> allAttendeeIds = new List<Guid>();
            allAttendeeIds.AddRange(requiredAttendeeIds);
            allAttendeeIds.AddRange(optionalAttendeeIds);

            QueryExpression query = new QueryExpression(EntityNames.Contact)
            {
                ColumnSet = new ColumnSet("fullname", "contactid", "ntw_lastnamearabic", "ntw_firstnamearabic"),
                Criteria = new FilterExpression
                {
                    FilterOperator = LogicalOperator.And,
                    Conditions =
                        {
                            new ConditionExpression("contactid", ConditionOperator.In, allAttendeeIds)
                        }
                }
            };

            EntityCollection result = _crmService.GetInstance().RetrieveMultiple(query);

            foreach (Entity contact in result.Entities)
            {
                var attendee = new Attendees();

                attendee.Id = contact.Contains("contactid") ? new Guid(contact["contactid"].ToString()) : Guid.Empty;
                attendee.Name = contact.Contains("fullname") ? contact["fullname"].ToString() : string.Empty;
                attendee.NameAr = ((contact.Contains("ntw_firstnamearabic") ? contact["ntw_firstnamearabic"].ToString() : string.Empty) + " " + (contact.Contains("ntw_lastnamearabic") ? contact["ntw_lastnamearabic"].ToString() : string.Empty)).Trim();

                if (requiredAttendeeIds.Any(x => x == contact.Id))
                {
                    attendee.IsRequired = true;
                }
                else
                {
                    attendee.IsRequired = false;
                }

                attendees.Add(attendee);
            }

            return attendees;
        }

        private Entity GetEventDetails(string eventId)
        {
            var orgContext = new OrganizationServiceContext(_crmService.GetInstance());

            IQueryable<Entity> EventsQuery = orgContext.CreateQuery("pwc_event").AsQueryable();
            Entity eventEntity = EventsQuery.Where(x => ((Guid)x["pwc_eventid"]).Equals(new Guid(eventId))).FirstOrDefault();

            return eventEntity;
        }

        private async Task<List<UserProfileQuickAction>> FillQuickActionList(List<Entity> entityList, string status)
        {
            List<UserProfileQuickAction> userProfileQuickAction = new List<UserProfileQuickAction>();

            var permissions = await _accessManagementAppService.GetAuthorizedPermissions();
            var accessLevel = GetPermissionAccessLevelOnRequestStep(permissions);
            var currentRole = await _accessManagementAppService.GetRoleAndDepartmentByRoleId(_sessionService.GetRoleId());

            foreach (var taskEntity in entityList)
            {
                Guid parentRequestId = Guid.Empty;
                Entity requestEntity = null;

                EntityReferenceDto entityReference = taskEntity.GetValueByAttributeName<EntityReferenceDto>("hexa_request");
                if (entityReference != null && !string.IsNullOrEmpty(entityReference.Id))
                {
                    parentRequestId = new Guid(entityReference.Id);
                }

                if (parentRequestId != Guid.Empty)
                {
                    requestEntity = GetRequestDetails(parentRequestId);
                }
                
                var portalRoleEntity = GetPortalRoleByAliasedValue(taskEntity);
                var portalContactId = CRMOperations.GetValueByAttributeName<EntityReference>(taskEntity, "hexa_portalcontact")?.Id.ToString() ?? string.Empty;

                var isAuthorizedToRead = CheckPermissionToReadHexaRequestStep(portalRoleEntity, currentRole, accessLevel, portalContactId);

                if (isAuthorizedToRead)
                {
                    var quickAction = new UserProfileQuickAction
                    {
                        Id = taskEntity.Id,
                        ParentRequestId = parentRequestId,
                        Name = taskEntity.Contains("processStepTemplateAlias.hexa_summary") ? taskEntity.GetAttributeValue<AliasedValue>("processStepTemplateAlias.hexa_summary").Value.ToString() : null,
                        NameAr = taskEntity.Contains("processStepTemplateAlias.hexa_summaryar") ? taskEntity.GetAttributeValue<AliasedValue>("processStepTemplateAlias.hexa_summaryar").Value.ToString() : null,
                        Status = status,
                        RequestStepNumber = taskEntity.GetValueByAttributeName<string>("hexa_requeststepnumber"),
                        ParentRequestNumber = requestEntity != null && requestEntity.Contains("hexa_name") ? requestEntity.GetValueByAttributeName<string>("hexa_name") : string.Empty,
                        ParentRequestName = requestEntity != null && requestEntity.Contains("processtemplateAlias.hexa_nameen") ? requestEntity.GetAttributeValue<AliasedValue>("processtemplateAlias.hexa_nameen").Value.ToString() : string.Empty,
                        ParentRequestNameAr = requestEntity != null && requestEntity.Contains("processtemplateAlias.hexa_namear") ? requestEntity.GetAttributeValue<AliasedValue>("processtemplateAlias.hexa_namear").Value.ToString() : string.Empty,
                    };

                    userProfileQuickAction.Add(quickAction);
                }
            }

            return userProfileQuickAction;
        }

        private Entity GetRequestDetails(Guid requestId)
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
                Values = { requestId }
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
                return results.Entities[0];
            }

            return null;
        }

        private async Task<NewRequests> GetNewRequestsCounts()
        {
            var firstMonthEnd = DateTime.UtcNow;
            var firstMonthStart = firstMonthEnd.AddMonths(-1).AddDays(1);

            var secondMonthEnd = firstMonthStart.AddDays(-1);
            var secondMonthStart = secondMonthEnd.AddMonths(-1).AddDays(1);

            var thirdMonthEnd = secondMonthStart.AddDays(-1);
            var thirdMonthStart = thirdMonthEnd.AddMonths(-1).AddDays(1);

            var RoleType = await _userPermissionAppService.LoggedInUserRoleType();

            var permissions = await _accessManagementAppService.GetAuthorizedPermissions();
            var accessLevel = GetPermissionAccessLevelOnRequestStep(permissions);

            var firstMonthTask = Task.Run(() => CountTasksByCreationDateVariant(firstMonthStart, firstMonthEnd, RoleType, accessLevel == AccessLevel.User));

            var secondMonthTask = Task.Run(() => CountTasksByCreationDateVariant(secondMonthStart, secondMonthEnd, RoleType, accessLevel == AccessLevel.User));

            var thirdMonthTask = Task.Run(() => CountTasksByCreationDateVariant(thirdMonthStart, thirdMonthEnd, RoleType, accessLevel == AccessLevel.User));

            var result = await Task.WhenAll(firstMonthTask, secondMonthTask, thirdMonthTask);

            return new NewRequests
            {
                FirstMonth = result[0],
                SecondMonth = result[1],
                ThirdMonth = result[2]
            };
        }

        private int CountTasksByCreationDateVariant(DateTime startOfMonth, DateTime endOfMonth, int? RoleType = null, bool userLevelAccess = false)
        {
            QueryExpression query = new QueryExpression(EntityNames.RequestStep)
            {
                ColumnSet = new ColumnSet("hexa_request", "createdon", "hexa_portalcontact")
            };

            if (userLevelAccess)
            {
                query.Criteria.AddCondition("hexa_portalcontact", ConditionOperator.Equal, _sessionService.GetContactId());
            }

            LinkEntity statusTemplateLink = new LinkEntity(EntityNames.RequestStep, EntityNames.HexaStepStatusTemplate,
                "hexa_stepstatus", "hexa_stepstatustemplateid", JoinOperator.Inner)
            {
                Columns = new ColumnSet("hexa_type"),
                EntityAlias = "statusTemplateAlias"
            };
            statusTemplateLink.LinkCriteria.AddCondition("hexa_type", ConditionOperator.Equal, (int)HexaInternalStatusType.start);

            LinkEntity requestLink = new LinkEntity(EntityNames.RequestStep, EntityNames.Request,
                "hexa_request", "hexa_requestid", JoinOperator.Inner)
            {
                Columns = new ColumnSet("hexa_customer"),
                EntityAlias = "requestAlias"
            };
            requestLink.LinkCriteria.AddCondition("hexa_customer", ConditionOperator.Equal, new Guid(_sessionService.GetCompanyId()));

            LinkEntity processStepLink = new LinkEntity(EntityNames.RequestStep, EntityNames.ProcessStepTemplate,
                "hexa_processstep", "hexa_processsteptemplateid", JoinOperator.Inner)
            {
                Columns = new ColumnSet("hexa_isexternalusertypecode", "hexa_portalroleid"),
                EntityAlias = "processStepAlias"
            };
            processStepLink.LinkCriteria.AddCondition("hexa_isexternalusertypecode", ConditionOperator.Equal, (int)ExternalUser.Yes);

            LinkEntity portalRoleLink = new LinkEntity(EntityNames.ProcessStepTemplate, EntityNames.PortalRole, "hexa_portalroleid", "hexa_portalroleid", JoinOperator.LeftOuter)
            {
                Columns = new ColumnSet("hexa_portalroleid", "hexa_name", "pwc_namear", "pwc_departmentid", "pwc_parentportalroleid", "pwc_showexternal", "pwc_showinternal", "pwc_roletypetypecode"),
                EntityAlias = "Role"
            };
            portalRoleLink.LinkCriteria.AddCondition(new ConditionExpression
            {
                AttributeName = "pwc_roletypetypecode",
                Operator = ConditionOperator.Equal,
                Values = { RoleType.HasValue ? RoleType.Value : 0 }
            });

            var linkFilter = new FilterExpression(LogicalOperator.And);
            linkFilter.AddCondition("requestAlias", "hexa_customer", ConditionOperator.NotNull);
            linkFilter.AddCondition("Role", "hexa_portalroleid", ConditionOperator.NotNull);
            query.Criteria.AddFilter(linkFilter);

            query.Criteria.AddCondition("createdon", ConditionOperator.GreaterEqual, startOfMonth);
            query.Criteria.AddCondition("createdon", ConditionOperator.LessEqual, endOfMonth);

            processStepLink.LinkEntities.Add(portalRoleLink);
            query.LinkEntities.Add(statusTemplateLink);
            query.LinkEntities.Add(requestLink);
            query.LinkEntities.Add(processStepLink);

            EntityCollection results = _crmService.GetInstance().RetrieveMultiple(query);

            var uniqueRequestIds = results.Entities
                .Select(e => e.GetAttributeValue<EntityReference>("hexa_request").Id)
                .Distinct()
                .Count();

            return uniqueRequestIds;
        }

        private async Task<SubmittedPendingRequests> GetSubmittedPendingRequests()
        {
            var statusesFromConfig = (_portalConfigAppService
                .RetrievePortalConfiguration(new List<string> { PortalConfigurations.UnderReviewStatus, PortalConfigurations.ApprovedStatus, PortalConfigurations.ReturnedStatus }));

            var underReviewStatus = new Guid(statusesFromConfig.FirstOrDefault(x => x.Key == PortalConfigurations.UnderReviewStatus).Value);
            var approvedStatus = new Guid(statusesFromConfig.FirstOrDefault(x => x.Key == PortalConfigurations.ApprovedStatus).Value);
            var returnedStatus = new Guid(statusesFromConfig.FirstOrDefault(x => x.Key == PortalConfigurations.ReturnedStatus).Value);

            var RoleType = await _userPermissionAppService.LoggedInUserRoleType();
            var permissions = await _accessManagementAppService.GetAuthorizedPermissions();
            var accessLevel = GetPermissionAccessLevelOnRequestStep(permissions);

            var underReviewTask = Task.Run(() => CountTasksByStatusVariant(underReviewStatus, RoleType, accessLevel == AccessLevel.User));

            var approvedTask = Task.Run(() => CountTasksByStatusVariant(approvedStatus, RoleType, accessLevel == AccessLevel.User));

            var returnedTask = Task.Run(() => CountTasksByStatusVariant(returnedStatus, RoleType, accessLevel == AccessLevel.User));

            var resultList = await Task.WhenAll(underReviewTask, approvedTask, returnedTask);

            return new SubmittedPendingRequests
            {
                UnderReview = resultList[0],
                Approved = resultList[1],
                Returned = resultList[2]
            };
        }

        private int CountTasksByStatusVariant(Guid status, int? RoleType = null, bool userLevelAccess = false)
        {
            QueryExpression query = new QueryExpression(EntityNames.RequestStep)
            {
                ColumnSet = new ColumnSet("hexa_request", "hexa_portalcontact")
            };

            if (userLevelAccess)
            {
                query.Criteria.AddCondition("hexa_portalcontact", ConditionOperator.Equal, _sessionService.GetContactId());
            }

            LinkEntity requestLink = new LinkEntity(EntityNames.RequestStep, EntityNames.Request,
                "hexa_request", "hexa_requestid", JoinOperator.Inner)
            {
                Columns = new ColumnSet("hexa_externalstatus", "hexa_customer"),
                EntityAlias = "requestAlias"
            };
            requestLink.LinkCriteria.AddCondition("hexa_externalstatus", ConditionOperator.Equal, status);
            requestLink.LinkCriteria.AddCondition("hexa_customer", ConditionOperator.Equal, new Guid(_sessionService.GetCompanyId()));

            LinkEntity processStepLink = new LinkEntity(EntityNames.RequestStep, EntityNames.ProcessStepTemplate,
                "hexa_processstep", "hexa_processsteptemplateid", JoinOperator.Inner)
            {
                Columns = new ColumnSet("hexa_isexternalusertypecode", "hexa_portalroleid"),
                EntityAlias = "processStepAlias"
            };
            processStepLink.LinkCriteria.AddCondition("hexa_isexternalusertypecode", ConditionOperator.Equal, (int)ExternalUser.Yes);

            LinkEntity portalRoleLink = new LinkEntity(EntityNames.ProcessStepTemplate, EntityNames.PortalRole, "hexa_portalroleid", "hexa_portalroleid", JoinOperator.LeftOuter)
            {
                Columns = new ColumnSet("hexa_portalroleid", "hexa_name", "pwc_namear", "pwc_departmentid", "pwc_parentportalroleid", "pwc_showexternal", "pwc_showinternal", "pwc_roletypetypecode"),
                EntityAlias = "Role"
            };
            portalRoleLink.LinkCriteria.AddCondition(new ConditionExpression
            {
                AttributeName = "pwc_roletypetypecode",
                Operator = ConditionOperator.Equal,
                Values = { RoleType.HasValue ? RoleType.Value : 0 }
            });

            var linkFilter = new FilterExpression(LogicalOperator.And);
            linkFilter.AddCondition("requestAlias", "hexa_customer", ConditionOperator.NotNull);
            linkFilter.AddCondition("Role", "hexa_portalroleid", ConditionOperator.NotNull);
            query.Criteria.AddFilter(linkFilter);

            processStepLink.LinkEntities.Add(portalRoleLink);
            query.LinkEntities.Add(requestLink);
            query.LinkEntities.Add(processStepLink);

            EntityCollection results = _crmService.GetInstance().RetrieveMultiple(query);

            var uniqueRequestIds = results.Entities
                .Select(e => e.GetAttributeValue<EntityReference>("hexa_request").Id)
                .Distinct()
                .Count();

            return uniqueRequestIds;
        }


        private async Task<OpenRequests> GetOpenRequests()
        {
            var now = DateTime.UtcNow;
            var startOfWeek = now.AddDays(-(int)now.DayOfWeek + (int)DayOfWeek.Sunday);
            var endOfWeek = startOfWeek.AddDays(6);

            var startOfMonth = endOfWeek.AddDays(1).Date;
            var endOfMonth = new DateTime(now.Year, now.Month, DateTime.DaysInMonth(now.Year, now.Month));

            var RoleType = await _userPermissionAppService.LoggedInUserRoleType();

            var permissions = await _accessManagementAppService.GetAuthorizedPermissions();
            var accessLevel = GetPermissionAccessLevelOnRequestStep(permissions);

            var dueMonthTask = Task.Run(() => CountTasksByDueDateVariant(startOfMonth, endOfMonth, RoleType, accessLevel == AccessLevel.User));

            var dueWeekTask = Task.Run(() => CountTasksByDueDateVariant(startOfWeek, endOfWeek, RoleType, accessLevel == AccessLevel.User));

            var overdueTask = Task.Run(() => CountOverDueTasks(now, RoleType, accessLevel == AccessLevel.User));

            var result = await Task.WhenAll(dueMonthTask, dueWeekTask, overdueTask);

            return new OpenRequests
            {
                DueMonth = result[0],
                DueWeek = result[1],
                OverDue = result[2]
            };
        }

        private int CountTasksByDueDateVariant(DateTime startDate, DateTime endDate, int? RoleType = null, bool userLevelAccess = false)
        {
            QueryExpression query = new QueryExpression(EntityNames.RequestStep)
            {
                ColumnSet = new ColumnSet("hexa_request", "hexa_dueon", "hexa_portalcontact")
            };

            if (userLevelAccess)
            {
                query.Criteria.AddCondition("hexa_portalcontact", ConditionOperator.Equal, _sessionService.GetContactId());
            }

            LinkEntity statusTemplateLink = new LinkEntity(EntityNames.RequestStep, EntityNames.HexaStepStatusTemplate,
                "hexa_stepstatus", "hexa_stepstatustemplateid", JoinOperator.Inner)
            {
                Columns = new ColumnSet("hexa_type"),
                EntityAlias = "statusTemplateAlias"
            };
            statusTemplateLink.LinkCriteria.AddCondition("hexa_type", ConditionOperator.Equal, (int)HexaInternalStatusType.start);

            LinkEntity requestLink = new LinkEntity(EntityNames.RequestStep, EntityNames.Request,
                "hexa_request", "hexa_requestid", JoinOperator.Inner)
            {
                Columns = new ColumnSet("hexa_customer"),
                EntityAlias = "requestAlias"
            };
            requestLink.LinkCriteria.AddCondition("hexa_customer", ConditionOperator.Equal, new Guid(_sessionService.GetCompanyId()));

            LinkEntity processStepLink = new LinkEntity(EntityNames.RequestStep, EntityNames.ProcessStepTemplate,
                "hexa_processstep", "hexa_processsteptemplateid", JoinOperator.Inner)
            {
                Columns = new ColumnSet("hexa_isexternalusertypecode", "hexa_portalroleid"),
                EntityAlias = "processStepAlias"
            };
            processStepLink.LinkCriteria.AddCondition("hexa_isexternalusertypecode", ConditionOperator.Equal, (int)ExternalUser.Yes);

            LinkEntity portalRoleLink = new LinkEntity(EntityNames.ProcessStepTemplate, EntityNames.PortalRole, "hexa_portalroleid", "hexa_portalroleid", JoinOperator.LeftOuter)
            {
                Columns = new ColumnSet("hexa_portalroleid", "hexa_name", "pwc_namear", "pwc_departmentid", "pwc_parentportalroleid", "pwc_showexternal", "pwc_showinternal", "pwc_roletypetypecode"),
                EntityAlias = "Role"
            };
            portalRoleLink.LinkCriteria.AddCondition(new ConditionExpression
            {
                AttributeName = "pwc_roletypetypecode",
                Operator = ConditionOperator.Equal,
                Values = { RoleType.HasValue ? RoleType.Value : 0 }
            });

            var linkFilter = new FilterExpression(LogicalOperator.And);
            linkFilter.AddCondition("requestAlias", "hexa_customer", ConditionOperator.NotNull);
            linkFilter.AddCondition("Role", "hexa_portalroleid", ConditionOperator.NotNull);
            query.Criteria.AddFilter(linkFilter);

            query.Criteria.AddCondition("hexa_dueon", ConditionOperator.GreaterEqual, startDate);
            query.Criteria.AddCondition("hexa_dueon", ConditionOperator.LessEqual, endDate);

            processStepLink.LinkEntities.Add(portalRoleLink);
            query.LinkEntities.Add(statusTemplateLink);
            query.LinkEntities.Add(requestLink);
            query.LinkEntities.Add(processStepLink);

            EntityCollection results = _crmService.GetInstance().RetrieveMultiple(query);

            var uniqueRequestIds = results.Entities
                .Select(e => e.GetAttributeValue<EntityReference>("hexa_request").Id)
                .Distinct()
                .Count();

            return uniqueRequestIds;
        }


        private int CountOverDueTasks(DateTime currentDate, int? RoleType = null, bool userLevelAccess = false)
        {
            QueryExpression query = new QueryExpression(EntityNames.RequestStep)
            {
                ColumnSet = new ColumnSet("hexa_request", "hexa_dueon", "hexa_portalcontact")
            };

            if (userLevelAccess)
            {
                query.Criteria.AddCondition("hexa_portalcontact", ConditionOperator.Equal, _sessionService.GetContactId());
            }

            LinkEntity statusTemplateLink = new LinkEntity(EntityNames.RequestStep, EntityNames.HexaStepStatusTemplate,
                "hexa_stepstatus", "hexa_stepstatustemplateid", JoinOperator.Inner)
            {
                Columns = new ColumnSet("hexa_type"),
                EntityAlias = "statusTemplateAlias"
            };
            statusTemplateLink.LinkCriteria.AddCondition("hexa_type", ConditionOperator.Equal, (int)HexaInternalStatusType.start);

            LinkEntity requestLink = new LinkEntity(EntityNames.RequestStep, EntityNames.Request,
                "hexa_request", "hexa_requestid", JoinOperator.Inner)
            {
                Columns = new ColumnSet("hexa_customer"),
                EntityAlias = "requestAlias"
            };
            requestLink.LinkCriteria.AddCondition("hexa_customer", ConditionOperator.Equal, new Guid(_sessionService.GetCompanyId()));

            LinkEntity processStepLink = new LinkEntity(EntityNames.RequestStep, EntityNames.ProcessStepTemplate,
                "hexa_processstep", "hexa_processsteptemplateid", JoinOperator.Inner)
            {
                Columns = new ColumnSet("hexa_isexternalusertypecode", "hexa_portalroleid"),
                EntityAlias = "processStepAlias"
            };
            processStepLink.LinkCriteria.AddCondition("hexa_isexternalusertypecode", ConditionOperator.Equal, (int)ExternalUser.Yes);

            LinkEntity portalRoleLink = new LinkEntity(EntityNames.ProcessStepTemplate, EntityNames.PortalRole, "hexa_portalroleid", "hexa_portalroleid", JoinOperator.LeftOuter)
            {
                Columns = new ColumnSet("hexa_portalroleid", "hexa_name", "pwc_namear", "pwc_departmentid", "pwc_parentportalroleid", "pwc_showexternal", "pwc_showinternal", "pwc_roletypetypecode"),
                EntityAlias = "Role"
            };
            portalRoleLink.LinkCriteria.AddCondition(new ConditionExpression
            {
                AttributeName = "pwc_roletypetypecode",
                Operator = ConditionOperator.Equal,
                Values = { RoleType.HasValue ? RoleType.Value : 0 }
            });

            var linkFilter = new FilterExpression(LogicalOperator.And);
            linkFilter.AddCondition("requestAlias", "hexa_customer", ConditionOperator.NotNull);
            linkFilter.AddCondition("Role", "hexa_portalroleid", ConditionOperator.NotNull);
            query.Criteria.AddFilter(linkFilter);

            query.Criteria.AddCondition("hexa_dueon", ConditionOperator.LessThan, currentDate);

            processStepLink.LinkEntities.Add(portalRoleLink);
            query.LinkEntities.Add(statusTemplateLink);
            query.LinkEntities.Add(requestLink);
            query.LinkEntities.Add(processStepLink);

            EntityCollection results = _crmService.GetInstance().RetrieveMultiple(query);

            var uniqueRequestIds = results.Entities
                .Select(e => e.GetAttributeValue<EntityReference>("hexa_request").Id)
                .Distinct()
                .Count();

            return uniqueRequestIds;
        }

        private async Task<List<UserProfileQuickAction>> GetUserProfileQuickAction()
        {
            List<UserProfileQuickAction> userProfileQuickActions = new List<UserProfileQuickAction>();

            EntityCollection results = await GetQuickActionList();

            var currentDate = DateTime.UtcNow;

            var recentlyOverdueRequests = results.Entities
                .Where(x => x.Contains("hexa_dueon") && x.GetAttributeValue<DateTime>("hexa_dueon") < currentDate)
                .OrderByDescending(x => x.GetAttributeValue<DateTime>("hexa_dueon"))
                .GroupBy(x => x.GetAttributeValue<EntityReference>("hexa_request").Id)
                .Select(group => group.First())
                .Take(4)
                .ToList();

            userProfileQuickActions.AddRange(await FillQuickActionList(recentlyOverdueRequests, "OVERDUE"));

            var overdueRequestIds = new List<Guid>();
            if (recentlyOverdueRequests != null && recentlyOverdueRequests.Count > 0)
            {
                overdueRequestIds = recentlyOverdueRequests.Select(x => x.GetAttributeValue<EntityReference>("hexa_request").Id).ToList();
            }

            var recentlyUpdatedRequests = results.Entities
                 .Where(x => x.Contains("modifiedon") && !overdueRequestIds.Contains(x.GetAttributeValue<EntityReference>("hexa_request").Id))
                 .OrderByDescending(x => x.GetAttributeValue<DateTime>("modifiedon"))
                 .GroupBy(x => x.GetAttributeValue<EntityReference>("hexa_request").Id)
                 .Select(group => group.First())
                 .Take(4 - (recentlyOverdueRequests != null ? recentlyOverdueRequests.Count : 0))
                 .ToList();

            userProfileQuickActions.AddRange(await FillQuickActionList(recentlyUpdatedRequests, "UPDATED"));

            return userProfileQuickActions;
        }

        private async Task<EntityCollection> GetQuickActionList()
        {
           var RoleType = await _userPermissionAppService.LoggedInUserRoleType();
            var statusesFromConfig = _portalConfigAppService
                .RetrievePortalConfiguration(new List<string> { PortalConfigurations.PendingStepStatus });

            var pendingStepStatus = new Guid(statusesFromConfig.FirstOrDefault(x => x.Key == PortalConfigurations.PendingStepStatus).Value);

            List<UserProfileQuickAction> userProfileQuickActions = new List<UserProfileQuickAction>();

            QueryExpression query = new QueryExpression(EntityNames.RequestStep)
            {
                ColumnSet = new ColumnSet("hexa_requeststepid", "hexa_name", "createdon", "hexa_dueon", "hexa_request", "hexa_requeststepnumber", "hexa_portalcontact", "modifiedon"),
                Criteria = new FilterExpression
                {
                    FilterOperator = LogicalOperator.And
                }
            };
            query.Criteria.AddCondition(new ConditionExpression
            {
                AttributeName = "hexa_customer",
                Operator = ConditionOperator.Equal,
                Values = { new Guid(_sessionService.GetCompanyId()) }
            });
            query.Criteria.AddCondition(new ConditionExpression
            {
                AttributeName = "hexa_externalstepstatus",
                Operator = ConditionOperator.Equal,
                Values = { pendingStepStatus }
            });

            LinkEntity linkEntity2 = new LinkEntity
            {
                LinkFromEntityName = EntityNames.RequestStep,
                LinkFromAttributeName = "hexa_processstep",
                LinkToEntityName = EntityNames.ProcessStepTemplate,
                LinkToAttributeName = "hexa_processsteptemplateid",
                JoinOperator = JoinOperator.Inner,
                EntityAlias = "processStepTemplateAlias",
                Columns = new ColumnSet("hexa_isexternalusertypecode", "hexa_summary", "hexa_summaryar", "hexa_portalroleid")
            };
            linkEntity2.LinkCriteria.AddCondition(new ConditionExpression
            {
                AttributeName = "hexa_isexternalusertypecode",
                Operator = ConditionOperator.Equal,
                Values = { (int)ExternalUser.Yes }
            });


            LinkEntity portalRoleLink = new LinkEntity(EntityNames.ProcessStepTemplate, EntityNames.PortalRole, "hexa_portalroleid", "hexa_portalroleid", JoinOperator.LeftOuter)
            {
                Columns = new ColumnSet("hexa_portalroleid", "pwc_roletypetypecode"),
                EntityAlias = "Role"
            };
            portalRoleLink.LinkCriteria.AddCondition(new ConditionExpression
            {
                AttributeName = "pwc_roletypetypecode",
                Operator = ConditionOperator.Equal,
                Values = { RoleType }
            });

            linkEntity2.LinkEntities.Add(portalRoleLink);

            query.LinkEntities.Add(linkEntity2);

            var linkFilter = new FilterExpression(LogicalOperator.And);
            linkFilter.AddCondition("Role", "hexa_portalroleid", ConditionOperator.NotNull);
            query.Criteria.AddFilter(linkFilter);

            EntityCollection results = _crmService.GetInstance().RetrieveMultiple(query);

            return results;
        }

        private async Task<RequestSummary> GetRequestSummary()
        {
            RequestSummary requestSummary = new RequestSummary();

            Task<NewRequests> taskNewRequests = GetNewRequestsCounts();
            Task<OpenRequests> taskOpenRequests = GetOpenRequests();
            Task<SubmittedPendingRequests> taskSubmittedPendingRequests = GetSubmittedPendingRequests();

            await Task.WhenAll(taskNewRequests, taskOpenRequests, taskSubmittedPendingRequests);

            requestSummary.NewRequestsDetails = await taskNewRequests;
            requestSummary.TotalNewRequests = requestSummary.NewRequestsDetails.FirstMonth
                                             + requestSummary.NewRequestsDetails.SecondMonth
                                             + requestSummary.NewRequestsDetails.ThirdMonth;

            requestSummary.SubmittedPendingRequestsDetails = await taskSubmittedPendingRequests;
            requestSummary.TotalSubmittedPendingRequests = requestSummary.SubmittedPendingRequestsDetails.UnderReview
                                                          + requestSummary.SubmittedPendingRequestsDetails.Returned
                                                          + requestSummary.SubmittedPendingRequestsDetails.Approved;

            requestSummary.OpenRequestsDetails = await taskOpenRequests;
            requestSummary.TotalOpenRequests = requestSummary.OpenRequestsDetails.DueMonth
                                              + requestSummary.OpenRequestsDetails.DueWeek
                                              + requestSummary.OpenRequestsDetails.OverDue;

            return requestSummary;
        }

        private bool GetShowTutorial()
        {
            var context = new OrganizationServiceContext(_crmService.GetInstance());

            IQueryable<Entity> ContactQuery = context.CreateQuery(EntityNames.Contact).AsQueryable();
            ContactQuery = ContactQuery.Where(x => ((Guid)x["contactid"]).Equals(new Guid(_sessionService.GetContactId())));

            return ContactQuery.Select(entity => entity.GetValueByAttributeName<bool>("pwc_showtutorial", ""))?.FirstOrDefault() ?? false;
        }

        //private async Task<bool> CheckPermissionToReadHexaRequestStep(EntityReference portalContact)
        //{
        //    var userPermissions = await _accessManagementAppService.GetAuthorizedPermissions();
        //    var requestDetailsPermission = userPermissions.FirstOrDefault(x => x.PageLink.Contains(PageRoute.TaskDetails));

        //    if (requestDetailsPermission == null || requestDetailsPermission.Read == (int)AccessLevel.None)
        //    {
        //        return false;
        //    }

        //    if (requestDetailsPermission.Read == (int)AccessLevel.User)
        //    {
        //        return portalContact != null && portalContact.Id.ToString() == _sessionService.GetContactId();
        //    }
        //    return true;
        //}
    }

    public class ProfileTasks
    {
        public ContactRole Association { get; set; }
        public List<CalendarTasks> CalendarTasks { get; set; } = new List<CalendarTasks> { };
    }
}
