using Microsoft.Xrm.Sdk;
using PIF.EBP.Application.MetaData.DTOs;
using PIF.EBP.Application.Shared;
using PIF.EBP.Core.CRM;
using System;
using System.Collections.Generic;
using System.Linq;
using PIF.EBP.Application.Requests.DTOs;
using PIF.EBP.Application.Shared.Helpers;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk.Query;
using PIF.EBP.Application.Shared.AppResponse;
using PIF.EBP.Core.Session;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using Microsoft.Xrm.Sdk.Client;
using static PIF.EBP.Application.Shared.Enums;
using PIF.EBP.Application.PortalConfiguration;
using PIF.EBP.Application.EntitiesCache;
using PIF.EBP.Application.ExternalFormConfiguration;
using PIF.EBP.Application.ExternalFormConfiguration.DTOs;
using PIF.EBP.Application.AccessManagement;
using PIF.EBP.Application.AccessManagement.DTOs;

namespace PIF.EBP.Application.Requests.Implementation
{
    public class RequestAppService : IRequestAppService
    {
        private readonly ICrmService _crmService;
        private readonly ISessionService _sessionService;
        private readonly IPortalConfigAppService _portalConfigAppService;
        private readonly IEntitiesCacheAppService _entitiesCacheAppService;
        private readonly IExternalFormConfigAppService _externalFormConfigAppService;
        private readonly IUserPermissionAppService _userPermissionAppService;
        private readonly IAccessManagementAppService _accessManagementAppService;

        public RequestAppService(ICrmService crmService,
            ISessionService sessionService,
            IPortalConfigAppService portalConfigAppService,
            IEntitiesCacheAppService entitiesCacheAppService,
            IExternalFormConfigAppService externalFormConfigAppService,
            IUserPermissionAppService userPermissionAppService,
            IAccessManagementAppService accessManagementAppService)
        {
            _crmService = crmService;
            _sessionService = sessionService;
            _portalConfigAppService = portalConfigAppService;
            _entitiesCacheAppService = entitiesCacheAppService;
            _externalFormConfigAppService = externalFormConfigAppService;
            _userPermissionAppService = userPermissionAppService;
            _accessManagementAppService=accessManagementAppService;

        }

        public async Task<HexaRequestListDto_RES> RetrieveRequestList(HexaRequestListDto_REQ oHexaRequestListDto_REQ)
        {
            HexaRequestListDto_RES hexaRequestListDto_RES = new HexaRequestListDto_RES();
            ListPagingResponse<HexaRequestList> AllRequestList = new ListPagingResponse<HexaRequestList>();

            HexaRequestListDto_REQ oHexaAllRequestList = new HexaRequestListDto_REQ();
            oHexaAllRequestList.SearchTerm = string.Empty;
            oHexaAllRequestList.StatusFilter = Guid.Empty;
            oHexaAllRequestList.PagingRequest = oHexaRequestListDto_REQ.PagingRequest;

            if (string.IsNullOrEmpty(oHexaRequestListDto_REQ.PagingRequest.SortField))
            {
                oHexaAllRequestList.PagingRequest.SortField = "createdon";
                oHexaAllRequestList.PagingRequest.SortOrder = SortOrder.Descending;
            }

            var statusesFromConfig = _portalConfigAppService
                .RetrievePortalConfiguration(new List<string> { PortalConfigurations.OverDueStatus, PortalConfigurations.PendingExternalStatus, PortalConfigurations.PendingPIFReviewExternalStatus });

            var pendingExternalStatus = statusesFromConfig.FirstOrDefault(x => x.Key == PortalConfigurations.PendingExternalStatus).Value;
            var pendingPIFReviewExternalStatus = statusesFromConfig.FirstOrDefault(x => x.Key == PortalConfigurations.PendingPIFReviewExternalStatus).Value;
            var overDueStatus = statusesFromConfig.FirstOrDefault(x => x.Key == PortalConfigurations.OverDueStatus).Value;

            bool OverDueStatus = oHexaRequestListDto_REQ.StatusFilter == new Guid(overDueStatus);
            var RoleType = await _userPermissionAppService.LoggedInUserRoleType();

            AllRequestList = await GetRequestList(oHexaAllRequestList, RoleType);
            hexaRequestListDto_RES.RequestsList.ListResponse = AllRequestList.ListResponse;

            if (AllRequestList.TotalCount > 0)
            {
                if (!string.IsNullOrEmpty(oHexaRequestListDto_REQ.SearchTerm))
                {
                    hexaRequestListDto_RES.RequestsList.ListResponse = hexaRequestListDto_RES.RequestsList.ListResponse.Where(x => (x.RequestNumber != null && x.RequestNumber.ToLower().Contains(oHexaRequestListDto_REQ.SearchTerm.ToLower())) || (x.RequestTitle != null && x.RequestTitle.ToLower().Contains(oHexaRequestListDto_REQ.SearchTerm.ToLower())) || (x.OverView != null && x.OverView.ToLower().Contains(oHexaRequestListDto_REQ.SearchTerm.ToLower()))).ToList();
                }
                if (oHexaRequestListDto_REQ.StatusFilter != Guid.Empty)
                {
                    if (OverDueStatus)
                    {
                        hexaRequestListDto_RES.RequestsList.ListResponse = hexaRequestListDto_RES.RequestsList.ListResponse.Where(x => x.OverDue == true).ToList();
                    }
                    else
                    {
                        hexaRequestListDto_RES.RequestsList.ListResponse = hexaRequestListDto_RES.RequestsList.ListResponse.Where(x => new Guid(x.ExternalStatus?.Id) == oHexaRequestListDto_REQ.StatusFilter).ToList();
                    }
                }

                hexaRequestListDto_RES.RequestsList.TotalCount = hexaRequestListDto_RES.RequestsList.ListResponse.Count;

                if (oHexaRequestListDto_REQ.PagingRequest != null)
                {
                    var pageNo = oHexaRequestListDto_REQ.PagingRequest.PageNo;
                    int pageSize = oHexaRequestListDto_REQ.PagingRequest.PageSize;

                    hexaRequestListDto_RES.RequestsList.ListResponse = hexaRequestListDto_RES.RequestsList.ListResponse.Skip((pageNo - 1) * pageSize).Take(pageSize).ToList();
                }

                HexaRequestListDto_RES requestSummary = GetRequestSummary(AllRequestList.ListResponse);

                hexaRequestListDto_RES.TotalDueNext2DaysRequests = requestSummary.TotalDueNext2DaysRequests;
                hexaRequestListDto_RES.TotalPendingRequests = AllRequestList.ListResponse.Count(x => x.ExternalStatus != null && !string.IsNullOrEmpty(x.ExternalStatus.Id) && Guid.TryParse(x.ExternalStatus.Id, out Guid parsedId) && (parsedId == new Guid(pendingExternalStatus) || parsedId == new Guid(pendingPIFReviewExternalStatus))); //requestSummary.TotalPendingRequests;
                hexaRequestListDto_RES.TotalOverDueRequests = requestSummary.TotalOverDueRequests;
                hexaRequestListDto_RES.TotalNewRequestsToday = requestSummary.TotalNewRequestsToday;
            }

            return hexaRequestListDto_RES;
        }

        private HexaRequestListDto_RES GetRequestSummary(List<HexaRequestList> hexaRequestLists)
        {
            HexaRequestListDto_RES hexaRequestListDto_RES = new HexaRequestListDto_RES();

            var statusesFromConfig = _portalConfigAppService
                .RetrievePortalConfiguration(new List<string> { PortalConfigurations.PendingExternalStatus, PortalConfigurations.PendingPIFReviewExternalStatus });

            var pendingExternalStatus = statusesFromConfig.FirstOrDefault(x => x.Key == PortalConfigurations.PendingExternalStatus).Value;
            var pendingPIFReviewExternalStatus = statusesFromConfig.FirstOrDefault(x => x.Key == PortalConfigurations.PendingPIFReviewExternalStatus).Value;

            string entity1LogicalName = EntityNames.RequestStep;
            string entity3LogicalName = EntityNames.ProcessStepTemplate;

            List<Guid> requestIds = new List<Guid>();
            foreach (HexaRequestList request in hexaRequestLists)
            {
                requestIds.Add(request.RequestId);
            }

            QueryExpression query = new QueryExpression(entity1LogicalName)
            {
                ColumnSet = new ColumnSet("hexa_requeststepid", "createdon", "hexa_dueon", "hexa_stepstatus", "hexa_request", "statecode"),
                Criteria = new FilterExpression
                {
                    FilterOperator = LogicalOperator.And
                }
            };

            ConditionExpression condition = new ConditionExpression
            {
                AttributeName = "hexa_request",
                Operator = ConditionOperator.In
            };

            foreach (var id in requestIds)
            {
                condition.Values.Add(id);
            }

            query.Criteria.AddCondition(condition);

            LinkEntity linkEntity1 = new LinkEntity
            {
                LinkFromEntityName = EntityNames.RequestStep,
                LinkFromAttributeName = "hexa_request",
                LinkToEntityName = EntityNames.HexaRequests,
                LinkToAttributeName = "hexa_requestid",
                JoinOperator = JoinOperator.LeftOuter,
                EntityAlias = "hexaRequest",
                Columns = new ColumnSet("hexa_requestid", "hexa_externalstatus"),
            };

            LinkEntity linkEntity2 = new LinkEntity
            {
                LinkFromEntityName = entity1LogicalName,
                LinkFromAttributeName = "hexa_processstep",
                LinkToEntityName = entity3LogicalName,
                LinkToAttributeName = "hexa_processsteptemplateid",
                JoinOperator = JoinOperator.Inner,
                EntityAlias = "processStepTemplateAlias",
                Columns = new ColumnSet("hexa_isexternalusertypecode")
            };

            linkEntity2.LinkCriteria.AddCondition(new ConditionExpression
            {
                AttributeName = "hexa_isexternalusertypecode",
                Operator = ConditionOperator.Equal,
                Values = { 0 }
            });

            query.LinkEntities.Add(linkEntity1);
            query.LinkEntities.Add(linkEntity2);

            EntityCollection results = _crmService.GetInstance().RetrieveMultiple(query);

            var finalQuery = results.Entities
                .Select(entity => FillHexaRequestStepsList(entity));

            var result = finalQuery.ToList();

            if (result.Count > 0)
            {
                hexaRequestListDto_RES.TotalDueNext2DaysRequests = GetTotalDueNext2DaysRequests(result);
                hexaRequestListDto_RES.TotalPendingRequests = GetTotalPendingRequests(result.Where(x => (x.HexaRequestStatus.Id == new Guid(pendingExternalStatus) || x.HexaRequestStatus.Id == new Guid(pendingPIFReviewExternalStatus))).ToList());
                hexaRequestListDto_RES.TotalOverDueRequests = GetTotalOverDueRequests(result);
                hexaRequestListDto_RES.TotalNewRequestsToday = GetTotalNewRequestsToday(result);
            }

            return hexaRequestListDto_RES;
        }

        private int GetTotalNewRequestsToday(List<HexaRequestStep> hexaRequestSteps)
        {
            var filteredRecords = hexaRequestSteps
                .Where(request => request.CreatedOn.Date == DateTime.Now.Date && request.StateCode.Value == (int)StateCode.Active)
                .GroupBy(request => request.HexaRequest.Id)
                .Select(group => group.First())
                .ToList();

            return filteredRecords.Count;
        }

        private int GetTotalOverDueRequests(List<HexaRequestStep> hexaRequestSteps)
        {
            var filteredRecords = hexaRequestSteps
                .Where(request => request.HexaDueOn != null && request.HexaDueOn.Value.Date < DateTime.Now.Date && request.StateCode.Value == (int)StateCode.Active)
                .GroupBy(request => request.HexaRequest.Id)
                .Select(group => group.First())
                .ToList();

            return filteredRecords.Count;
        }

        private int GetTotalPendingRequests(List<HexaRequestStep> hexaRequestSteps)
        {
            var filteredRecords = hexaRequestSteps
                .Where(request => request.StateCode.Value == (int)StateCode.Active)
                .GroupBy(request => request.HexaRequest.Id)
                .Select(group => group.First())
                .ToList();

            return filteredRecords.Count;
        }

        private int GetTotalDueNext2DaysRequests(List<HexaRequestStep> hexaRequestSteps)
        {
            DateTime now = DateTime.Now;
            DateTime nextTwoDays = now.AddDays(2);

            var filteredRecords = hexaRequestSteps
                .Where(request => request.HexaDueOn != null && request.HexaDueOn.Value.Date >= now.Date && request.HexaDueOn.Value.Date <= nextTwoDays.Date && request.StateCode.Value == (int)StateCode.Active)
                .GroupBy(request => request.HexaRequest.Id)
                .Select(group => group.First())
                .ToList();

            return filteredRecords.Count;
        }

        private DateTime? GetRequestDueOnDate(Guid RequestId, EntityCollection entityCollection)
        {
            var values = entityCollection.Entities.Where(x => x.Id == RequestId).Select(x => x.Contains("as.hexa_dueon") ? x.GetAttributeValue<AliasedValue>("as.hexa_dueon").Value : null).ToList();

            values.RemoveAll(x => x == null);

            if (values.Count > 0)
            {
                List<DateTime> dates = values.Cast<DateTime>().ToList();

                DateTime now = DateTime.Now;
                return dates.OrderBy(date => Math.Abs((date - now).TotalSeconds)).FirstOrDefault();
            }

            return null;
        }

        private async Task<ListPagingResponse<HexaRequestList>> GetRequestList(HexaRequestListDto_REQ oHexaRequestListDto_REQ,int RoleType)
        {
            var externalStatusesToNotShowDueDate = _portalConfigAppService.RetrievePortalConfiguration(new List<string> { PortalConfigurations.CompletedProcessStatus, PortalConfigurations.PendingPIFReviewExternalStatus });
            ListPagingResponse<HexaRequestList> HexaRequestList = new ListPagingResponse<HexaRequestList>();
            var fieldsDicMapping = GetFieldsMapping();
            var permissions = await _accessManagementAppService.GetAuthorizedPermissions();
            var query = BuildRequestListQueryExpression(RoleType, permissions);

            if (oHexaRequestListDto_REQ.PagingRequest != null && !string.IsNullOrEmpty(oHexaRequestListDto_REQ.PagingRequest.SortField) && oHexaRequestListDto_REQ.PagingRequest.SortField == "dueOnDate")
            {
                oHexaRequestListDto_REQ.PagingRequest.SortField = "hexa_dueon";
                CRMOperations.AddOrderToLinkEntity(query.LinkEntities.FirstOrDefault(le => le.EntityAlias == "as"), oHexaRequestListDto_REQ.PagingRequest);
            }
            else
            {
                CRMOperations.GetQueryExpression(query, oHexaRequestListDto_REQ.PagingRequest, fieldsDicMapping);
            }

            var entityCollection = _crmService.GetInstance().RetrieveMultiple(query);

            List<HexaRequestList> data = MapRequestListToDto(entityCollection);
            foreach(var item in data)
            {
                if (externalStatusesToNotShowDueDate.Any(a => a.Value == item.ExternalStatus.Id) || !item.DueOnDate.HasValue || item.DueOnDate.GetValueOrDefault() == DateTime.MinValue)
                {
                    item.OverDue = false;
                }
            }
            HexaRequestList.ListResponse = new List<HexaRequestList>();
            HexaRequestList.ListResponse.AddRange(data);
            HexaRequestList.TotalCount = data.Count;

            return HexaRequestList;
        }

        private QueryExpression BuildRequestListQueryExpression(int RoleType, List<AuthPermission> permissions)
        {
            QueryExpression query = new QueryExpression(EntityNames.HexaRequests)
            {
                ColumnSet = new ColumnSet(
                    "hexa_requestid", "hexa_name", "createdon", "pwc_title", "hexa_internalstatus", "hexa_dueon", "pwc_dealoverview", "pwc_summaryar", "createdon", "statecode", "hexa_externalstatus", "hexa_summary", "hexa_processtemplate", "pwc_titlear"),
                Distinct = true,
                LinkEntities =
                {
                    new LinkEntity
                    {
                        LinkFromEntityName = EntityNames.HexaRequests,
                        LinkFromAttributeName = "hexa_processtemplate",
                        LinkToEntityName = EntityNames.ProcessTemplate,
                        LinkToAttributeName = "hexa_processtemplateid",
                        JoinOperator = JoinOperator.Inner,
                        EntityAlias = "ProcessTemplate",
                        Columns = new ColumnSet("hexa_nameen", "hexa_namear", "hexa_portalroleid"),
                        LinkEntities =
                        {
                            new LinkEntity
                            {
                                LinkFromEntityName = EntityNames.ProcessTemplate,
                                LinkFromAttributeName = "hexa_processtemplateid",
                                LinkToEntityName = EntityNames.ProcessStepTemplate,
                                LinkToAttributeName = "hexa_processtemplate",
                                JoinOperator = JoinOperator.Inner,
                                EntityAlias = "af",
                                LinkCriteria = new FilterExpression
                                {
                                    FilterOperator = LogicalOperator.And,
                                    Conditions =
                                    {
                                        new ConditionExpression("hexa_isexternalusertypecode", ConditionOperator.Equal, 0)
                                    }
                                }
                            },
                            new LinkEntity
                            {
                                LinkFromEntityName = EntityNames.ProcessTemplate,
                                LinkFromAttributeName = "hexa_portalroleid",
                                LinkToEntityName = EntityNames.PortalRole,
                                LinkToAttributeName = "hexa_portalroleid",
                                JoinOperator = JoinOperator.Inner,
                                Columns = new ColumnSet("hexa_portalroleid", "hexa_name", "pwc_namear", "pwc_departmentid", "pwc_parentportalroleid","pwc_showexternal","pwc_showinternal", "pwc_roletypetypecode"),
                                EntityAlias = "Role",
                                LinkCriteria = new FilterExpression
                                {
                                    FilterOperator = LogicalOperator.And,
                                    Conditions =
                                    {
                                        new ConditionExpression("pwc_roletypetypecode", ConditionOperator.Equal, RoleType)
                                    }
                                }
                            }
                        }
                    },
                    new LinkEntity
                    {
                        LinkFromEntityName = EntityNames.HexaRequests,
                        LinkFromAttributeName = "hexa_requestid",
                        LinkToEntityName = EntityNames.RequestStep,
                        LinkToAttributeName = "hexa_request",
                        JoinOperator = JoinOperator.LeftOuter,
                        EntityAlias = "as",
                        Columns = new ColumnSet("hexa_requeststepid", "hexa_dueon"),
                        LinkEntities =
                        {
                            new LinkEntity
                            {
                                LinkFromEntityName = EntityNames.HexaRequests,
                                LinkFromAttributeName = "hexa_processstep",
                                LinkToEntityName = EntityNames.ProcessStepTemplate,
                                LinkToAttributeName = "hexa_processsteptemplateid",
                                JoinOperator = JoinOperator.Inner,
                                EntityAlias = "processsteptemplate",
                                LinkCriteria = new FilterExpression
                                {
                                    FilterOperator = LogicalOperator.And,
                                    Conditions =
                                    {
                                        new ConditionExpression("hexa_isexternalusertypecode", ConditionOperator.Equal, 0)
                                    }
                                }
                            }
                        }
                    }
                }
            };

            var mainFilter = new FilterExpression(LogicalOperator.And);

            mainFilter.AddCondition("hexa_customer", ConditionOperator.Equal, new Guid(_sessionService.GetCompanyId()));
            mainFilter.AddCondition("ProcessTemplate","hexa_portalroleid", ConditionOperator.NotNull);

            var requestAccessLevel = GetPermissionAccessLevelOnRequest(permissions);
            if (requestAccessLevel == AccessLevel.User)
            {
                mainFilter.AddCondition("hexa_portalcontact", ConditionOperator.Equal, _sessionService.GetContactId());

            }


            query.Criteria.AddFilter(mainFilter);

            return query;
        }

        private Dictionary<string, string> GetFieldsMapping()
        {
            var mappedColmnsDics = new Dictionary<string, string>
            {
                { "hexa_name", "RequestNumber" },
                { "pwc_title", "RequestTitle" },
                { "pwc_dealoverview", "OverView" },
                { "hexa_externalstatus", "ExternalStatus" },
                { "createdon", "createdon"}
            };

            return mappedColmnsDics;
        }

        private List<HexaRequestList> MapRequestListToDto(EntityCollection entityCollection)
        {
            var data = entityCollection.Entities.Select(entity => FillHexaRequestList(entity, entityCollection)).ToList();
            data = data.GroupBy(entity => entity.Id)
                   .Select(group => group.First())
                   .ToList();

            var externalFormConfigurations = _externalFormConfigAppService.GetExternalFormConfigurations().Where(x => x.ProcessTemplate != null);
            return data.Select(x => new HexaRequestList
            {
                RequestId = x.Id,
                DueOnDate = x.DueOnDate == DateTime.MinValue ? (DateTime?)null : x.DueOnDate,
                RequestTitle = x.Title,
                RequestTitleAr = x.TitleAr,
                RequestNumber = x.RequestNumber,
                OverView = x.DealOverview,
                OverViewAr = x.DealOverviewAr,
                ExternalStatus = x.ExternalStatus,
                ProcessTemplate = x.ProcessTemplate,
                OverDue = x.DueOnDate != DateTime.MinValue && x.DueOnDate < DateTime.Now,
                CreationDate = x.CreationDate,
                StateCode = x.StateCode != null && !string.IsNullOrEmpty(x.StateCode.Value) ? int.Parse(x.StateCode.Value) : 1,
                ExternalFormConfiguration = externalFormConfigurations.Where(configs => x.ProcessTemplate != null && configs.ProcessTemplate.Id == x.ProcessTemplate.Id)
                                                                      .Select(z => new ExternalFormConfigSimplifiedDto { Id = z.Id, Name = z.Name, FormType = z.FormType })
                                                                      .ToList()
            }).ToList();
        }

        private HexaRequest FillHexaRequestList(Entity entity, EntityCollection entityCollection)
        {
            DateTime? dueOnDate = GetRequestDueOnDate(entity.Id, entityCollection);
            var processTemplate = CRMOperations.GetValueByAttributeName<EntityReferenceDto>(entity, "hexa_processtemplate");
            if (processTemplate!=null)
            {
                processTemplate.Name=CRMOperations.GetValueByAttributeName<AliasedValue>(entity, "ProcessTemplate.hexa_nameen")?.Value as string;
                processTemplate.NameAr=CRMOperations.GetValueByAttributeName<AliasedValue>(entity, "ProcessTemplate.hexa_namear")?.Value as string;
            }

            return new HexaRequest
            {
                Id = entity.Id,
                RequestNumber = CRMOperations.GetValueByAttributeName<string>(entity, "hexa_name"),
                RequestName = CRMOperations.GetValueByAttributeName<string>(entity, "pwc_requestname"),
                Title = CRMOperations.GetValueByAttributeName<string>(entity, "pwc_title"),
                TitleAr = CRMOperations.GetValueByAttributeName<string>(entity, "pwc_titlear"),
                DealOverview = CRMOperations.GetValueByAttributeName<string>(entity, "hexa_summary"),
                DealOverviewAr = CRMOperations.GetValueByAttributeName<string>(entity, "pwc_summaryar"),
                CreationDate = CRMOperations.GetValueByAttributeName<DateTime>(entity, "createdon"),
                StateCode = CRMOperations.GetValueByAttributeName<EntityOptionSetDto>(entity, "statecode"),
                DueOnDate = dueOnDate != null ? (DateTime)dueOnDate : DateTime.MinValue,
                Stage = CRMOperations.GetValueByAttributeName<EntityReferenceDto>(entity, "pwc_stageid"),
                InternalStatus = _entitiesCacheAppService.RetrieveEntityCacheById(EntityNames.ProcessStatusTemplate, entity.GetValueByAttributeName<EntityReference>("hexa_internalstatus").Id),
                ExternalStatus = _entitiesCacheAppService.RetrieveEntityCacheById(EntityNames.ProcessStatusTemplate, entity.GetValueByAttributeName<EntityReference>("hexa_externalstatus").Id),
                Owner = CRMOperations.GetValueByAttributeName<EntityReferenceDto>(entity, "ownerid"),
                ProcessTemplate = processTemplate,
                CurrentStep = CRMOperations.GetValueByAttributeName<EntityReferenceDto>(entity, "hexa_currentstep"),
            };
        }

        private HexaRequestStep FillHexaRequestStepsList(Entity entity)
        {
            HexaRequestStep hexaRequestStep = new HexaRequestStep();

            hexaRequestStep.HexaRequestStepId = CRMOperations.GetValueByAttributeName<Guid>(entity, "hexa_requeststepid");
            hexaRequestStep.CreatedOn = CRMOperations.GetValueByAttributeName<DateTime>(entity, "createdon");
            hexaRequestStep.HexaDueOn = CRMOperations.GetValueByAttributeName<DateTime?>(entity, "hexa_dueon");
            hexaRequestStep.HexaStepStatus = CRMOperations.GetValueByAttributeName<EntityReference>(entity, "hexa_stepstatus");
            hexaRequestStep.HexaRequest = CRMOperations.GetValueByAttributeName<EntityReference>(entity, "hexa_request");
            hexaRequestStep.StateCode = CRMOperations.GetValueByAttributeName<OptionSetValue>(entity, "statecode");

            if (entity.Contains("hexaRequest.hexa_externalstatus"))
            {
                AliasedValue aliasedValue = null;

                aliasedValue = (AliasedValue)entity.Attributes["hexaRequest.hexa_externalstatus"];
                EntityReference entityReference = (EntityReference)aliasedValue.Value;
                hexaRequestStep.HexaRequestStatus = entityReference;
            }

            return hexaRequestStep;
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
}
