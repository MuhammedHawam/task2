using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using Newtonsoft.Json;
using PIF.EBP.Application.ExternalFormConfiguration;
using PIF.EBP.Application.ExternalFormConfiguration.DTOs;
using PIF.EBP.Application.Hexa.DTOs;
using PIF.EBP.Application.MetaData.DTOs;
using PIF.EBP.Application.Shared;
using PIF.EBP.Application.Shared.Helpers;
using PIF.EBP.Core.CRM;
using PIF.EBP.Core.Exceptions;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using static PIF.EBP.Application.Shared.Enums;
using Newtonsoft.Json.Linq;
using PIF.EBP.Core.Session;
using PIF.EBP.Application.PortalConfiguration;
using PIF.EBP.Application.AccessManagement;
using System.Threading.Tasks;
using System.Configuration;
using System.Net;
using PIF.EBP.Application.EntitiesCache;
using PIF.EBP.Application.EntitiesCache.DTOs;
using PIF.EBP.Application.MetaData;

namespace PIF.EBP.Application.Hexa.Implementation
{
    public class HexaAppService : IHexaAppService
    {
        private readonly ICrmService _crmService;
        private readonly IExternalFormConfigAppService _externalFormConfigAppService;
        private readonly ISessionService _sessionService;
        private readonly IPortalConfigAppService _portalConfigAppService;
        private readonly IAccessManagementAppService _accessManagementAppService;
        private readonly IEntitiesCacheAppService _entitiesCacheAppService;
        private readonly IMetadataCacheManager _metadataCacheManager;
        private readonly IUserPermissionAppService _userPermissionAppService;

        public HexaAppService(ICrmService crmService,
            IExternalFormConfigAppService externalFormConfigAppService,
            ISessionService sessionService,
            IPortalConfigAppService portalConfigAppService,
            IAccessManagementAppService accessManagementAppService,
            IEntitiesCacheAppService entitiesCacheAppService,
            IMetadataCacheManager metadataCacheManager,
            IUserPermissionAppService userPermissionAppService)
        {
            _crmService = crmService;
            _externalFormConfigAppService = externalFormConfigAppService;
            _sessionService = sessionService;
            _portalConfigAppService = portalConfigAppService;
            _accessManagementAppService = accessManagementAppService;
            _entitiesCacheAppService = entitiesCacheAppService;
            _metadataCacheManager = metadataCacheManager;
            _userPermissionAppService = userPermissionAppService;
        }

        public void UpdateHexaReqDocNoOfUploadedDoc(Guid ReqDocId, int ItemCount, bool IsUpload, string folderPath = null)
        {
            var orgContext = new OrganizationServiceContext(_crmService.GetInstance());
            IQueryable<Entity> requestQuery = orgContext.CreateQuery(EntityNames.RequestDocument).AsQueryable();
            int noOfUploadedDoc = 0;
            try
            {
                var noOfUploadedDocs = requestQuery.Where(x => ((Guid)x["hexa_requestdocumentid"])
                .Equals(ReqDocId)).FirstOrDefault();
                noOfUploadedDoc = requestQuery.Where(x => ((Guid)x["hexa_requestdocumentid"])
                .Equals(ReqDocId)).Select(x => (int)x["hexa_numberofdocumentsuploaded"])
                .FirstOrDefault();
            }
            catch (Exception ex)
            {
                if (!ex.InnerException.Message.Contains("key"))
                {
                    throw new UserFriendlyException("SomethingWentWrong");
                }
            }
            Entity newReqDoc = new Entity(EntityNames.RequestDocument);

            newReqDoc["hexa_requestdocumentid"] = ReqDocId;

            if (IsUpload)
            {
                noOfUploadedDoc += ItemCount;
                newReqDoc["hexa_sharepointfolderurl"] = folderPath;
                newReqDoc["statuscode"] = new OptionSetValue((int)RequestDocStatusType.Uploaded);
            }
            else
            {
                noOfUploadedDoc -= ItemCount;
                if (noOfUploadedDoc == 0)
                {
                    newReqDoc["statuscode"] = new OptionSetValue((int)RequestDocStatusType.PendingUpload);
                    newReqDoc["hexa_sharepointfolderurl"] = "";
                }
            }

            newReqDoc["hexa_numberofdocumentsuploaded"] = noOfUploadedDoc;

            _crmService.Update(newReqDoc, EntityNames.RequestDocument);
        }

        public async Task<HexaRequestDto> RetrieveHexaRequestById(Guid RequestId)
        {
            var hexaRequest = new HexaRequestDto();
            var orgContext = new OrganizationServiceContext(_crmService.GetInstance());
            var RoleType = await _userPermissionAppService.LoggedInUserRoleType();

            QueryExpression query = new QueryExpression(EntityNames.HexaRequests)
            {
                ColumnSet = new ColumnSet(
                "hexa_requestid", "hexa_name", "pwc_title", "pwc_titlear", "hexa_summary",
                "pwc_summaryar", "createdon", "hexa_portalcontact", "ownerid", "hexa_processtemplate", "hexa_currentstep", "hexa_customer", "hexa_externalstatus"),
                TopCount = 1
            };
            var processTemplateLink = new LinkEntity
            {
                LinkFromEntityName = EntityNames.HexaRequests,
                LinkFromAttributeName = "hexa_processtemplate",
                LinkToEntityName = EntityNames.ProcessTemplate,
                LinkToAttributeName = "hexa_processtemplateid",
                JoinOperator = JoinOperator.Inner,
                EntityAlias = "ProcessTemplate",
                Columns = new ColumnSet("hexa_nameen", "hexa_namear", "hexa_portalroleid")
            };

            var roleLink = new LinkEntity
            {
                LinkFromEntityName = EntityNames.ProcessTemplate,
                LinkFromAttributeName = "hexa_portalroleid",
                LinkToEntityName = EntityNames.PortalRole,
                LinkToAttributeName = "hexa_portalroleid",
                JoinOperator = JoinOperator.Inner,
                Columns = new ColumnSet("hexa_portalroleid"),
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

            processTemplateLink.LinkEntities.Add(roleLink);
            query.LinkEntities.Add(processTemplateLink);
            query.Criteria.AddCondition("hexa_requestid", ConditionOperator.Equal, RequestId);
            query.Criteria.AddCondition("Role", "hexa_portalroleid", ConditionOperator.NotNull);
            var entityCollection = _crmService.GetInstance().RetrieveMultiple(query);

            if (entityCollection != null && entityCollection.Entities.Count > 0)
            {
                hexaRequest = entityCollection.Entities.Select(entity => FillHexaRequest(entity)).FirstOrDefault();
            }

            if (hexaRequest.Company.Id != _sessionService.GetCompanyId())
            {
                throw new UserFriendlyException("CannotAccessThisRequest", System.Net.HttpStatusCode.Forbidden);
            }

            if (hexaRequest.Id != Guid.Empty)
            {

                if (hexaRequest.ProcessTemplate != null && hexaRequest.ProcessTemplate.Id != string.Empty && new Guid(hexaRequest.ProcessTemplate.Id) != Guid.Empty)
                {
                    IQueryable<Entity> requestQuery = orgContext.CreateQuery(EntityNames.HexaRequests).AsQueryable();
                    requestQuery = requestQuery.Where(x => ((Guid)x["hexa_requestid"]).Equals(RequestId));
                    hexaRequest.Stages = await GetStepTemplates(new Guid(hexaRequest.ProcessTemplate.Id), RequestId);
                    hexaRequest.ExtensionObject = GetExtensionObjectByProcessTemplateId(requestQuery, new Guid(hexaRequest.ProcessTemplate.Id));
                    hexaRequest.ExternalFormConfiguration = GetExternalFormConfigByProcessTemplateId(new Guid(hexaRequest.ProcessTemplate.Id));

                    if (hexaRequest.ExternalFormConfiguration.Any())
                    {
                        var parsedExternalFormConfig = JsonConvert.DeserializeObject<List<ExternalFormConfigurationParsed>>(hexaRequest.ExternalFormConfiguration.FirstOrDefault()?.SelectedEntities);
                        var dynamicData = GetDynamicDataForRequest(RequestId, parsedExternalFormConfig);
                        hexaRequest.DynamicData = new JObject();
                        foreach (var x in dynamicData)
                        {
                            JObject innerTransformedObject = new JObject();
                            if (x.Value.Count > 0)
                            {
                                foreach (var y in x.Value)
                                {
                                    innerTransformedObject[y.Key] = JToken.FromObject(y.Value);
                                }
                            }
                            hexaRequest.DynamicData[x.Key] = JToken.FromObject(innerTransformedObject);
                        }
                    }
                }

                hexaRequest.RequestDocuments = GetRequestDocumentsByRequestId(RequestId);
                var activeStage = hexaRequest.Stages?.LastOrDefault(x => x.Steps.Any(z => z.IsExternalUser == (int)ExternalUser.Yes && z.StateCode.Value == "0")) ?? null;
                if (activeStage != null)
                {
                    hexaRequest.Stage = new EntityReferenceDto { Id = activeStage.Stage.Id.ToString(), Name = activeStage.Stage.Name, NameAr = activeStage.Stage.NameAr };
                }
                else
                {
                    var statusesFromConfig = _portalConfigAppService.RetrievePortalConfiguration(
                    new List<string>
                    {
                        PortalConfigurations.CompletedStepStatus,PortalConfigurations.SubmittedStepStatus
                    });
                    var completedStatus = new Guid(statusesFromConfig.First(x => x.Key == PortalConfigurations.CompletedStepStatus).Value);
                    var submittedStatus = new Guid(statusesFromConfig.First(x => x.Key == PortalConfigurations.SubmittedStepStatus).Value);
                    var stage = hexaRequest.Stages.LastOrDefault(x => x.Steps.Any(z => z.IsExternalUser == (int)ExternalUser.Yes &&
                                (z.Status.Id == completedStatus.ToString() || z.Status.Id == submittedStatus.ToString())));
                    if (stage != null)
                    {
                        hexaRequest.Stage = new EntityReferenceDto { Id = stage.Stage.Id.ToString(), Name = stage.Stage.Name, NameAr = stage.Stage.NameAr };
                    }

                }

            }

            return hexaRequest;
        }

        public async Task<DTOs.HexaRequestStepDto> RetrieveHexaRequestStepById(Guid RequestStepId)
        {
            string extensionEntityName = string.Empty;
            var hexaRequestStep = new DTOs.HexaRequestStepDto();

            var orgContext = new OrganizationServiceContext(_crmService.GetInstance());

            IQueryable<Entity> Query = orgContext.CreateQuery(EntityNames.RequestStep).AsQueryable();
            Query = Query.Where(x => ((Guid)x["hexa_requeststepid"]).Equals(RequestStepId));
            QueryExpression query = BuildRequestStepByIdQuery(Guid.Empty, RequestStepId);
            EntityCollection result = _crmService.GetInstance().RetrieveMultiple(query);
            hexaRequestStep = result.Entities.Select(entity => FillHexaRequestStep(entity)).FirstOrDefault();

            var isAuthorizedToRead = await CheckPermissionToReadHexaRequestStep(hexaRequestStep) && hexaRequestStep.Customer?.Id == _sessionService.GetCompanyId();
            var sameRoleType = (await _userPermissionAppService.LoggedInUserRoleType()) == hexaRequestStep.RoleTypeCode;

            if (!isAuthorizedToRead || !hexaRequestStep.ProcessStep.IsExternal || !sameRoleType)
            {
                throw new UserFriendlyException("UserUnauthorized", HttpStatusCode.Forbidden);
            }

            hexaRequestStep.CanReassign = await CheckPermissionToReassignHexaRequestStep(hexaRequestStep);

            hexaRequestStep.CanUpdate = await CheckPermissionToUpdateHexaRequestStepOrRequest(hexaRequestStep, PageRoute.TaskDetails);

            if (hexaRequestStep.ProcessStep != null && hexaRequestStep.ProcessStep.Id != Guid.Empty)
            {
                hexaRequestStep.ExtensionObject = GetExtensionObjectByProcessStepTemplateId(Query, hexaRequestStep.ProcessStep.Id, hexaRequestStep.MasterRequest.Id);
                hexaRequestStep.ExternalFormConfiguration = GetExternalFormConfigByProcessStepTemplateId(hexaRequestStep.ProcessStep.Id);

                var parsedExternalFormConfig = hexaRequestStep.ExternalFormConfiguration.FirstOrDefault()?.SelectedEntities != null ?
                                    JsonConvert.DeserializeObject<List<ExternalFormConfigurationParsed>>(hexaRequestStep.ExternalFormConfiguration.FirstOrDefault()?.SelectedEntities) :
                                    GetDefaultEntitiesForEmptyFormConfiguration();

                var dynamicData = GetDynamicDataForRequestStep(RequestStepId, parsedExternalFormConfig);

                dynamicData.Add(new KeyValuePair<string, List<KeyValuePair<string, object>>>("extensionObject", GetDataFromExtensionObject(hexaRequestStep.ExtensionObject.FirstOrDefault(), parsedExternalFormConfig)));

                hexaRequestStep.DynamicData = new JObject();
                foreach (var x in dynamicData)
                {
                    JObject innerTransformedObject = new JObject();
                    if (x.Value.Count > 0)
                    {
                        foreach (var y in x.Value)
                        {
                            innerTransformedObject[y.Key] = JToken.FromObject(y.Value);
                        }
                    }
                    hexaRequestStep.DynamicData[x.Key] = JToken.FromObject(innerTransformedObject);
                }

                hexaRequestStep.StepTransition = RetrieveStepTransitions(hexaRequestStep.ProcessStep.Id, null);
            }

            hexaRequestStep.StepRequestDocuments = GetRequestDocumentsByStepRequestId(RequestStepId, hexaRequestStep.PortalRole, hexaRequestStep.Department);
            if (hexaRequestStep.InitiatingStep != null && hexaRequestStep.InitiatingStep.Id != Guid.Empty)
            {
                hexaRequestStep.LatestPortalComment = GetLatestPortalCommentByRequestStepId(hexaRequestStep.InitiatingStep.Id);
            }
            return hexaRequestStep;
        }

        private List<ExternalFormConfigurationParsed> GetDefaultEntitiesForEmptyFormConfiguration()
        {
            return new List<ExternalFormConfigurationParsed>
            {
                new ExternalFormConfigurationParsed
                {
                    LogicalName = EntityNames.Request,
                },
                new ExternalFormConfigurationParsed
                {
                    LogicalName = EntityNames.RequestStep
                }
            };
        }

        private QueryExpression BuildRequestStepByIdQuery(Guid requestId, Guid requestStepId)
        {
            QueryExpression query = new QueryExpression(EntityNames.RequestStep);
            query.ColumnSet = new ColumnSet("hexa_requeststepnumber", "hexa_name", "hexa_stepnumber", "hexa_dueon", "createdon", "hexa_request", "hexa_stageid", "hexa_stepstatus", "hexa_processstep", "ownerid", "statecode", "hexa_customer", "hexa_portalcontact", "hexa_externalstepstatus", "hexa_initiatingstep");

            var requestLink = new LinkEntity
            {
                LinkFromEntityName = EntityNames.RequestStep,
                LinkFromAttributeName = "hexa_request",
                LinkToEntityName = EntityNames.Request,
                LinkToAttributeName = "hexa_requestid",
                JoinOperator = JoinOperator.Inner,
                Columns = new ColumnSet("hexa_name", "hexa_summary", "pwc_summaryar", "pwc_title", "pwc_titlear"),
                EntityAlias = "Request",
                LinkEntities =
                            {
                                new LinkEntity
                                {
                                    LinkFromEntityName = EntityNames.Request,
                                    LinkFromAttributeName = "hexa_requestid",
                                    LinkToEntityName = EntityNames.FileSharingRequestExtension,
                                    LinkToAttributeName = "pwc_requestid",
                                    EntityAlias = "FileSharingExtension",
                                    Columns = new ColumnSet("pwc_requestid","pwc_requestpurposevalue"),
                                    JoinOperator = JoinOperator.LeftOuter
                                }
                            }
            };
            var companyLink = new LinkEntity
            {
                LinkFromEntityName = EntityNames.RequestStep,
                LinkFromAttributeName = "hexa_customer",
                LinkToEntityName = EntityNames.Account,
                LinkToAttributeName = "accountid",
                JoinOperator = JoinOperator.LeftOuter,
                Columns = new ColumnSet("accountid", "entityimage", "name", "ntw_companynamearabic"),
                EntityAlias = "Company",
            };
            var portalContactLink = new LinkEntity
            {
                LinkFromEntityName = EntityNames.RequestStep,
                LinkFromAttributeName = "hexa_portalcontact",
                LinkToEntityName = EntityNames.Contact,
                LinkToAttributeName = "contactid",
                JoinOperator = JoinOperator.LeftOuter,
                Columns = new ColumnSet("contactid", "entityimage", "firstname", "ntw_firstnamearabic", "lastname", "ntw_lastnamearabic"),
                EntityAlias = "PortalContact",
            };

            var processStepTemplateLink = new LinkEntity
            {
                LinkFromEntityName = EntityNames.RequestStep,
                LinkFromAttributeName = "hexa_processstep",
                LinkToEntityName = EntityNames.ProcessStepTemplate,
                LinkToAttributeName = "hexa_processsteptemplateid",
                JoinOperator = JoinOperator.Inner,
                Columns = new ColumnSet("hexa_processsteptemplateid", "hexa_portalroleid", "hexa_isexternalusertypecode"),
                EntityAlias = "ProcessTemplate"
            };

            var roleLink = new LinkEntity
            {
                LinkFromEntityName = EntityNames.ProcessStepTemplate,
                LinkFromAttributeName = "hexa_portalroleid",
                LinkToEntityName = EntityNames.PortalRole,
                LinkToAttributeName = "hexa_portalroleid",
                EntityAlias = "PortalRole",
                Columns = new ColumnSet("hexa_portalroleid", "pwc_departmentid", "pwc_roletypetypecode"),
                JoinOperator = JoinOperator.LeftOuter
            };

            processStepTemplateLink.LinkEntities.Add(roleLink);

            // Add link entities to the query
            query.LinkEntities.Add(requestLink);
            query.LinkEntities.Add(companyLink);
            query.LinkEntities.Add(portalContactLink);
            query.LinkEntities.Add(processStepTemplateLink);
            if (requestStepId != Guid.Empty)
            {
                query.Criteria.AddCondition("hexa_requeststepid", ConditionOperator.Equal, requestStepId);
            }
            if (requestId != Guid.Empty)
            {
                query.Criteria.AddCondition("hexa_request", ConditionOperator.Equal, requestId);
            }

            return query;
        }

        private async Task<bool> CheckPermissionToReadHexaRequestStep(DTOs.HexaRequestStepDto hexaRequestStep)
        {
            var userPermissions = await _accessManagementAppService.GetAuthorizedPermissions();
            var requestDetailsPermission = userPermissions.Where(x => x.PageLink.Contains(PageRoute.TaskDetails) 
                                            && string.IsNullOrEmpty(x.ServiceId)).FirstOrDefault();

            if (requestDetailsPermission == null || requestDetailsPermission.Read == (int)AccessLevel.None)
            {
                return false;
            }

            if(requestDetailsPermission.Read == (int)AccessLevel.User)
            {
                return hexaRequestStep.PortalContact != null && hexaRequestStep.PortalContact.Id == _sessionService.GetContactId();
            }

            if (hexaRequestStep.PortalRole == null)
            {
                return false;
            }

            if (requestDetailsPermission.Read == (int)AccessLevel.Basic)
            {
                return await _userPermissionAppService.CheckRoleHierarchyByRoleId(hexaRequestStep.PortalRole.Id.ToString());
            }

            //deep access
            return await _userPermissionAppService.CheckDeepAccessForTheRole(hexaRequestStep.PortalRole.Id.ToString());
        }

        private async Task<bool> CheckPermissionToUpdateHexaRequestStepOrRequest(DTOs.HexaRequestStepDto hexaRequestStep, string pageRoute)
        {
            if (hexaRequestStep.StateCode.Value == "1")
            {
                return false;
            }

            var writePermission = (await _accessManagementAppService.GetAuthorizedPermissions())
                                .Where(x => !string.IsNullOrEmpty(x.PageLink) && x.PageLink.Contains(pageRoute)).FirstOrDefault();

            if (writePermission == null || writePermission.Write == (int)AccessLevel.None)
            {
                return false;
            }

            if (writePermission.Write == (int)AccessLevel.User)
            {
                return hexaRequestStep.PortalContact != null && hexaRequestStep.PortalContact.Id == _sessionService.GetContactId();
            }

            if (hexaRequestStep.PortalContact != null)
            {
                if (hexaRequestStep.PortalContact.Id == _sessionService.GetContactId())
                {
                    return true;
                }
            }

            if (hexaRequestStep.PortalRole == null)
            {
                return false;
            }

            var parentRoleForLoggedInUser = await _accessManagementAppService.GetParentRoleByRoleId(_sessionService.GetRoleId());

            if (writePermission.Write == (int)AccessLevel.Basic)
            {
                return await _userPermissionAppService.CheckRoleHierarchyByRoleId(hexaRequestStep.PortalRole.Id.ToString());
            }
            //deep access
            return await _userPermissionAppService.CheckDeepAccessForTheRole(hexaRequestStep.PortalRole.Id.ToString());
        }

        private async Task<bool> CheckPermissionToCreateHexaRequest(DTOs.HexaRequestStepDto hexaRequestStep, string pageRoute)
        {
            var userPermissions = await _accessManagementAppService.GetAuthorizedPermissions();
            var requestPermission = userPermissions.Where(x => x.PageLink.Contains(pageRoute)).FirstOrDefault();

            if (requestPermission == null || requestPermission.Create == (int)AccessLevel.None)
            {
                return false;
            }


            if (hexaRequestStep.PortalRole == null)
            {
                return false;
            }

            if (requestPermission.Create == (int)AccessLevel.Basic)
            {
                return await _userPermissionAppService.CheckRoleHierarchyByRoleId(hexaRequestStep.PortalRole.Id.ToString());
            }

            //deep access
            return await _userPermissionAppService.CheckDeepAccessForTheRole(hexaRequestStep.PortalRole.Id.ToString());
        }

        private async Task<bool> CheckPermissionToReassignHexaRequestStep(DTOs.HexaRequestStepDto hexaRequestStep)
        {
            if (hexaRequestStep.StateCode.Value == "1")
            {
                return false;
            }

            // we need to check if he has the write and the service id "reassign"
            var userPermissions = await _accessManagementAppService.GetAuthorizedPermissions();
            var reassignPermission = userPermissions.Where(x => x.PageLink.Contains(PageRoute.TaskDetails) && !string.IsNullOrEmpty(x.ServiceId) && x.ServiceId.ToLower().Contains("reassign".ToLower()))?.FirstOrDefault();


            if (reassignPermission == null || reassignPermission.Write == (int)AccessLevel.None)
            {
                return false;
            }
            if (hexaRequestStep.PortalRole == null)
            {
                return false;
            }

            if (reassignPermission.Write == (int)AccessLevel.User)
            {
                return hexaRequestStep.PortalContact != null && hexaRequestStep.PortalContact.Id == _sessionService.GetContactId();
            }

            if (reassignPermission.Write == (int)AccessLevel.Basic)
            {
                return await _userPermissionAppService.CheckRoleHierarchyByRoleId(hexaRequestStep.PortalRole.Id.ToString());                
            }

            //deep access
            return await _userPermissionAppService.CheckDeepAccessForTheRole(hexaRequestStep.PortalRole.Id.ToString());
        }

        private List<KeyValuePair<string, object>> GetDataFromExtensionObject(dynamic extensionObject, List<ExternalFormConfigurationParsed> parsedExternalFormConfig)
        {
            IDictionary<string, object> dictionary = (IDictionary<string, object>)extensionObject;
            var result = dictionary
                .Select(at => new KeyValuePair<string, object>(at.Key, GetCustomAliasedValue(at.Value)))
                .ToList();

            return result.ToList();
        }

        private RequestStepAuthorizationNeedsDto GetRequestStepAuthorizationNeedsDtoById(Guid requestStepId)
        {
            QueryExpression query = new QueryExpression(EntityNames.RequestStep);
            query.ColumnSet = new ColumnSet("hexa_requeststepnumber", "hexa_name", "hexa_stepnumber", "hexa_dueon", "createdon", "hexa_request", "hexa_stageid", "hexa_stepstatus", "hexa_processstep", "statecode", "ownerid", "hexa_customer", "hexa_portalcontact");

            var portalContactLink = new LinkEntity
            {
                LinkFromEntityName = EntityNames.RequestStep,
                LinkFromAttributeName = "hexa_portalcontact",
                LinkToEntityName = EntityNames.Contact,
                LinkToAttributeName = "contactid",
                JoinOperator = JoinOperator.LeftOuter,
                Columns = new ColumnSet("contactid", "entityimage", "firstname", "ntw_firstnamearabic", "lastname", "ntw_lastnamearabic"),
                EntityAlias = "PortalContact",
            };

            var processStepTemplateLink = new LinkEntity
            {
                LinkFromEntityName = EntityNames.RequestStep,
                LinkFromAttributeName = "hexa_processstep",
                LinkToEntityName = EntityNames.ProcessStepTemplate,
                LinkToAttributeName = "hexa_processsteptemplateid",
                JoinOperator = JoinOperator.Inner,
                Columns = new ColumnSet("hexa_processsteptemplateid", "hexa_portalroleid"),
                EntityAlias = "ProcessTemplate",
            };
            // Add link entities to the query
            query.LinkEntities.Add(portalContactLink);
            query.LinkEntities.Add(processStepTemplateLink);
            query.Criteria.AddCondition("hexa_requeststepid", ConditionOperator.Equal, requestStepId);
            EntityCollection result = _crmService.GetInstance().RetrieveMultiple(query);
            var hexaEntity = result.Entities.Select(entity => new RequestStepAuthorizationNeedsDto
            {
                PortalContact = entity.GetValueByAttributeName<EntityReferenceDto>("hexa_portalcontact"),
                portalRoleOnProcessStepTemplate = (EntityReference)CRMOperations.GetValueByAttributeName<AliasedValue>(entity, "ProcessTemplate.hexa_portalroleid")?.Value,
                StateCode = entity.GetValueByAttributeName<EntityOptionSetDto>("statecode")
            }).FirstOrDefault();

            return hexaEntity;
        }

        private RequestStepAuthorizationNeedsDto GetRequestAuthorizationNeedsDtoByProcessTemplateId(Guid processTemplateId)
        {
            QueryExpression query = new QueryExpression(EntityNames.ProcessTemplate);
            query.ColumnSet = new ColumnSet("hexa_processtemplateid", "hexa_portalroleid");

            query.Criteria.AddCondition("hexa_processtemplateid", ConditionOperator.Equal, processTemplateId);
            EntityCollection result = _crmService.GetInstance().RetrieveMultiple(query);
            var hexaEntity = result.Entities.Select(entity => new RequestStepAuthorizationNeedsDto
            {
                portalRoleOnProcessStepTemplate = entity.GetValueByAttributeName<EntityReference>("hexa_portalroleid"),
            }).FirstOrDefault();

            return hexaEntity;
        }

        private List<KeyValuePair<string, List<KeyValuePair<string, object>>>> GetDynamicDataForRequest(Guid requestId, List<ExternalFormConfigurationParsed> parsedExternalFormConfig)
        {
            var criteria = new FilterExpression();
            criteria.AddCondition("hexa_requestid", ConditionOperator.Equal, requestId);

            var requestCols = parsedExternalFormConfig.Where(a => a.LogicalName == EntityNames.Request)?.FirstOrDefault()?.Attributes ?? new HashSet<string>();
            var contactsCols = parsedExternalFormConfig.Where(a => a.LogicalName == EntityNames.Contact)?.FirstOrDefault()?.Attributes;
            var companyCols = parsedExternalFormConfig.Where(a => a.LogicalName == EntityNames.Account)?.FirstOrDefault()?.Attributes;
            var requestStepCols = parsedExternalFormConfig.Where(a => a.LogicalName == EntityNames.RequestStep)?.FirstOrDefault()?.Attributes;

            QueryExpression requestQuery = new QueryExpression(EntityNames.Request)
            {
                ColumnSet = new ColumnSet(requestCols.ToArray()),
                Criteria = criteria
            };

            if (requestStepCols != null && requestStepCols.Count > 0)
            {
                var requestStepLink = new LinkEntity
                {
                    LinkFromEntityName = EntityNames.Request,
                    LinkFromAttributeName = "hexa_requestid",
                    LinkToEntityName = EntityNames.RequestStep,
                    LinkToAttributeName = "hexa_request",
                    JoinOperator = JoinOperator.LeftOuter,
                    Columns = new ColumnSet(requestStepCols.ToArray()),
                    EntityAlias = EntityNames.RequestStep,
                };
                requestQuery.LinkEntities.Add(requestStepLink);
            }

            if (contactsCols != null && contactsCols.Count > 0)
            {
                var contactsLink = new LinkEntity
                {
                    LinkFromEntityName = EntityNames.Request,
                    LinkFromAttributeName = "hexa_portalcontact",
                    LinkToEntityName = EntityNames.Contact,
                    LinkToAttributeName = "contactid",
                    JoinOperator = JoinOperator.Inner,
                    Columns = new ColumnSet(contactsCols.ToArray()),
                    EntityAlias = EntityNames.Contact,
                };
                requestQuery.LinkEntities.Add(contactsLink);
            }
            if (companyCols != null && companyCols.Count > 0)
            {
                var companiesLink = new LinkEntity
                {
                    LinkFromEntityName = EntityNames.Request,
                    LinkFromAttributeName = "hexa_customer",
                    LinkToEntityName = EntityNames.Account,
                    LinkToAttributeName = "accountid",
                    JoinOperator = JoinOperator.Inner,
                    Columns = new ColumnSet(companyCols.ToArray()),
                    EntityAlias = EntityNames.Account,
                };
                requestQuery.LinkEntities.Add(companiesLink);
            }

            var result = _crmService.GetInstance().RetrieveMultiple(requestQuery)?.Entities?.FirstOrDefault();

            List<string> staticEntityNames = new List<string> { EntityNames.Account, EntityNames.Contact, EntityNames.RequestStep };
            var dicResult = staticEntityNames
                                .Select(e => new KeyValuePair<string, List<KeyValuePair<string, object>>>(
                                        e, result.Attributes
                                        .Where(at => at.Key.Contains(e + "."))
                                        .Select(at => new KeyValuePair<string, object>(at.Key.Replace(e + ".", ""), GetCustomAliasedValue(at.Value)))
                                        .ToList()))
                                .ToList();

            // Handle direct entity attributes
            var requestStepDic = result.Attributes
                .Where(at => !at.Key.Contains("."))
                .Select(at => new KeyValuePair<string, object>(at.Key, GetCustomAliasedValue(at.Value)))
                .ToList();

            dicResult.Add(new KeyValuePair<string, List<KeyValuePair<string, object>>>(EntityNames.Request, requestStepDic));

            return dicResult;
        }

        private List<KeyValuePair<string, List<KeyValuePair<string, object>>>> GetDynamicDataForRequestStep(Guid requestStepId, List<ExternalFormConfigurationParsed> parsedExternalFormConfig)
        {
            var criteria = new FilterExpression();
            criteria.AddCondition("hexa_requeststepid", ConditionOperator.Equal, requestStepId);

            var requestCols = parsedExternalFormConfig.Where(a => a.LogicalName == EntityNames.Request)?.FirstOrDefault()?.Attributes;
            var contactsCols = parsedExternalFormConfig.Where(a => a.LogicalName == EntityNames.Contact)?.FirstOrDefault()?.Attributes;
            var companyCols = parsedExternalFormConfig.Where(a => a.LogicalName == EntityNames.Account)?.FirstOrDefault()?.Attributes;
            var requestStepCols = parsedExternalFormConfig.Where(a => a.LogicalName == EntityNames.RequestStep)?.FirstOrDefault()?.Attributes;

            AddIntialFieldsForRequestAndRequestStep(ref requestCols, ref requestStepCols);

            QueryExpression requestStepQuery = new QueryExpression(EntityNames.RequestStep)
            {
                ColumnSet = new ColumnSet(requestStepCols.ToArray()),
                Criteria = criteria
            };

            var requestLink = new LinkEntity
            {
                LinkFromEntityName = EntityNames.RequestStep,
                LinkFromAttributeName = "hexa_request",
                LinkToEntityName = EntityNames.Request,
                LinkToAttributeName = "hexa_requestid",
                JoinOperator = JoinOperator.LeftOuter,
                Columns = new ColumnSet(requestCols.ToArray()),
                EntityAlias = EntityNames.Request,
            };

            if (contactsCols != null && contactsCols.Count > 0)
            {
                var contactsLink = new LinkEntity
                {
                    LinkFromEntityName = EntityNames.Request,
                    LinkFromAttributeName = "hexa_portalcontact",
                    LinkToEntityName = EntityNames.Contact,
                    LinkToAttributeName = "contactid",
                    JoinOperator = JoinOperator.Inner,
                    Columns = new ColumnSet(contactsCols.ToArray()),
                    EntityAlias = EntityNames.Contact,
                };
                requestLink.LinkEntities.Add(contactsLink);
            }
            if (companyCols != null && companyCols.Count > 0)
            {
                var companiesLink = new LinkEntity
                {
                    LinkFromEntityName = EntityNames.Request,
                    LinkFromAttributeName = "hexa_customer",
                    LinkToEntityName = EntityNames.Account,
                    LinkToAttributeName = "accountid",
                    JoinOperator = JoinOperator.Inner,
                    Columns = new ColumnSet(companyCols.ToArray()),
                    EntityAlias = EntityNames.Account,
                };
                requestLink.LinkEntities.Add(companiesLink);
            }

            requestStepQuery.LinkEntities.Add(requestLink);

            var result = _crmService.GetInstance().RetrieveMultiple(requestStepQuery)?.Entities?.FirstOrDefault();

            List<string> staticEntityNames = new List<string> { EntityNames.Account, EntityNames.Contact, EntityNames.Request };

            var dicResult = staticEntityNames
                                .Select(e => new KeyValuePair<string, List<KeyValuePair<string, object>>>(
                                        e, result.Attributes
                                        .Where(at => at.Key.Contains(e + "."))
                                        .Select(at => new KeyValuePair<string, object>(at.Key.Replace(e + ".", ""), GetCustomAliasedValue(at.Value)))
                                        .ToList()))
                                .ToList();

            // Handle direct entity attributes
            var requestStepDic = result.Attributes
                .Where(at => !at.Key.Contains("."))
                .Select(at => new KeyValuePair<string, object>(at.Key, GetCustomAliasedValue(at.Value)))
                .ToList();

            dicResult.Add(new KeyValuePair<string, List<KeyValuePair<string, object>>>(EntityNames.RequestStep, requestStepDic));


            return dicResult;
        }
        private object GetAliasedValue(object value)
        {
            if (value is AliasedValue aliasedValue)
            {
                return GetAliasedValue(aliasedValue.Value);
            }
            else if (value is OptionSetValue optionSetValue)
            {
                return optionSetValue.Value;
            }
            else if (value is EntityReference entityReference)
            {
                var oEntityReferenceDto = new EntityReferenceDto
                {
                    Id = entityReference.Id.ToString(),
                    Name = entityReference.Name,
                };
                return oEntityReferenceDto;
            }
            return value;
        }
        private object GetCustomAliasedValue(object value)
        {
            if (value is AliasedValue aliasedValue)
            {
                return GetCustomAliasedValue(aliasedValue.Value);
            }
            else if (value is OptionSetValue optionSetValue)
            {
                return optionSetValue.Value;
            }
            else if (value is EntityReference entityReference)
            {
                //this is the change which is different on GetAliasedValue method
                return entityReference.Id.ToString();
            }
            else if (value is Microsoft.Xrm.Sdk.Money money)
            {
                //this is the change which is different on GetAliasedValue method
                return money.Value.ToString();
            }
            return value;
        }

        private void AddIntialFieldsForRequestAndRequestStep(ref HashSet<string> requestCols, ref HashSet<string> requestStepCols)
        {
            if (requestCols != null && requestCols.Count > 0)
            {
                requestCols.Add("hexa_requestid");
                requestCols.Add("hexa_portalcontact");
                requestCols.Add("hexa_customer");
                requestCols.Add("hexa_summary");
                requestCols.Add("pwc_summaryar");
                requestCols.Add("pwc_title");
                requestCols.Add("pwc_titlear");
            }
            else
            {
                requestCols = new HashSet<string>
                {
                    "hexa_requestid",
                    "hexa_portalcontact",
                    "hexa_customer",
                    "hexa_summary",
                    "pwc_summaryar",
                    "pwc_title",
                    "pwc_titlear"
                };
            }

            if (requestStepCols != null && requestStepCols.Count > 0)
            {
                requestStepCols.Add("hexa_name");
                requestStepCols.Add("hexa_stepnumber");
                requestStepCols.Add("hexa_dueon");
                requestStepCols.Add("createdon");
                requestStepCols.Add("hexa_request");
                requestStepCols.Add("hexa_stageid");
                requestStepCols.Add("hexa_stepstatus");
                requestStepCols.Add("hexa_processstep");
                requestStepCols.Add("ownerid");
            }
            else
            {
                requestStepCols = new HashSet<string>
                {
                    "hexa_name",
                    "hexa_stepnumber",
                    "hexa_dueon",
                    "createdon",
                    "hexa_request",
                    "hexa_stageid",
                    "hexa_stepstatus",
                    "hexa_processstep",
                    "ownerid"
                };
            }
        }

        public List<HexaStepTransitionDto> RetrieveStepTransitions(Guid? ProcessStepId, Guid? ProcessTemplateId)
        {
            List<HexaStepTransitionDto> hexaStepTransitions = new List<HexaStepTransitionDto>();
            var orgContext = new OrganizationServiceContext(_crmService.GetInstance());
            IQueryable<Entity> Query = orgContext.CreateQuery("hexa_steptransitiontemplate").AsQueryable();
            if (ProcessStepId.HasValue)
            {
                Query = Query.Where(x => ((Guid)x["hexa_processstep"]).Equals(ProcessStepId.Value));
            }
            if (ProcessTemplateId.HasValue)
            {
                Query = Query.Where(x => ((Guid)x["hexa_processtemplate"]).Equals(ProcessTemplateId.Value));
            }
            hexaStepTransitions = Query.Select(entity => FillStepTransitions(entity)).ToList();
            return hexaStepTransitions.OrderBy(x => x.Name).ToList();
        }

        public async Task<List<SaveFormResponse>> SaveForm(FormDto formDto)
        {
            ValidateFormDto(formDto);

            if (!await IsGrantedPermissionsForSaveForm(formDto))
            {
                throw new UserFriendlyException("UserUnauthorized", System.Net.HttpStatusCode.Forbidden);
            }

            var statusesFromConfig = _portalConfigAppService.RetrievePortalConfiguration(
                new List<string>
                {
            PortalConfigurations.CompletedProcessStatus,
            PortalConfigurations.PendingProcessStatus,
            PortalConfigurations.SubmittedProcessStatus
                });
            var relationshipFieldDic = new Dictionary<string, EntityFields>();

            var isNewRequest = IsNewRequest(formDto);
            Guid createdRequestId = InitializeRequest(formDto);

            if (createdRequestId != Guid.Empty)
            {
                UpdateFormEntitiesWithRequestId(formDto, createdRequestId);
            }

            using (var orgContext = new OrganizationServiceContext(_crmService.GetInstance()))
            {
                var transactionRequest = await CreateTransactionRequest(formDto, statusesFromConfig, orgContext, createdRequestId, relationshipFieldDic, isNewRequest);

                var response = ExecuteTransaction(transactionRequest);

                if (relationshipFieldDic.Any())
                {
                    await CreateManyToManyAssociation(orgContext, formDto, response, relationshipFieldDic);
                }

                var createdEntities = ProcessTransactionResponse(response, formDto);

                // Ensure that the request step is submitted if TransitionId and RequestStepId have values
                if (formDto.TransitionId.HasValue && formDto.RequestStepId.HasValue)
                {
                    SubmitHexaRequestStep(formDto);
                }

                return createdEntities;
            }
        }


        private void ValidateFormDto(FormDto formDto)
        {
            if (!formDto.RequestStepId.HasValue && !formDto.ProcessTemplateId.HasValue)
            {
                throw new UserFriendlyException("NullArgument", System.Net.HttpStatusCode.BadRequest);
            }
        }

        private Guid InitializeRequest(FormDto formDto)
        {
            Guid requestId = formDto.FormEntities
                .Where(e => e.EntityName == EntityNames.HexaRequests)
                .Select(a => a.EntityId)
                .FirstOrDefault();

            if (formDto.ProcessTemplateId.HasValue && requestId == Guid.Empty)
            {
                return CreateRequest(formDto);
            }
            return Guid.Empty;
        }

        private bool IsNewRequest(FormDto formDto)
        {
            Guid requestId = formDto.FormEntities
                .Where(e => e.EntityName == EntityNames.HexaRequests)
                .Select(a => a.EntityId)
                .FirstOrDefault();

            return formDto.ProcessTemplateId.HasValue && requestId == Guid.Empty;

        }

        private void UpdateFormEntitiesWithRequestId(FormDto formDto, Guid createdRequestId)
        {
            formDto.FormEntities
                .Where(a => a.EntityName == EntityNames.HexaRequests)
                .FirstOrDefault()
                .EntityId = createdRequestId;
        }

        private async Task<ExecuteTransactionRequest> CreateTransactionRequest(FormDto formDto, List<PortalConfigDto> statusesFromConfig, OrganizationServiceContext orgContext, Guid createdRequestId, Dictionary<string, EntityFields> relationshipFieldDic, bool isNewRequest)
        {
            var transactionRequest = new ExecuteTransactionRequest
            {
                Requests = new OrganizationRequestCollection(),
                ReturnResponses = true
            };

            foreach (var formEntity in formDto.FormEntities)
            {
                if (!string.IsNullOrEmpty(formEntity.RelationshipName))
                {
                    var entityField = await RemoveFieldBasedOnRelationship(formEntity);
                    if (entityField != null)
                    {
                        relationshipFieldDic.Add(formEntity.EntityName, entityField);
                    }
                }
                if (!ValidateEntityFields(formEntity.EntityName, formEntity.Fields, formEntity.EntityId))
                {
                    throw new UserFriendlyException("UnexpectedMsgError");
                }

                var entity = PrepareEntity(formEntity, statusesFromConfig, formDto, orgContext, createdRequestId, isNewRequest);

                if (formEntity.EntityId != Guid.Empty)
                {
                    entity.Id = formEntity.EntityId;
                    transactionRequest.Requests.Add(new UpdateRequest { Target = entity });
                }
                else
                {
                    transactionRequest.Requests.Add(new CreateRequest { Target = entity });
                }

            }

            return transactionRequest;
        }


        private async Task<EntityFields> RemoveFieldBasedOnRelationship(FormEntities formEntity)
        {
            EntityRelationshipDto relationship = await GetEntityRelationshipByName(formEntity.EntityName, formEntity.RelationshipName);
            if (relationship != null && relationship.Type == (int)Microsoft.Crm.Sdk.Messages.RelationshipType.ManyToManyRelationship)
            {
                var secondAttribute = formEntity.EntityName == relationship.Entity1LogicalName ? relationship.Entity2IntersectAttribute : relationship.Entity1IntersectAttribute;

                var entityField = formEntity.Fields.FirstOrDefault(x => x.AttributeName == secondAttribute);
                if (entityField != null)
                {
                    formEntity.Fields.Remove(entityField);
                }
                return entityField;
            }
            return null;
        }

        private async Task<EntityRelationshipDto> GetEntityRelationshipByName(string entityName, string relationshipName)
        {
            var cachedItem = await _metadataCacheManager.GetCachedEntityRelationshipsAsync(entityName);
            var relationship = cachedItem.FirstOrDefault(x => x.Name == relationshipName);
            return relationship;
        }

        private async Task CreateManyToManyAssociation(OrganizationServiceContext orgContext, FormDto formDto, ExecuteTransactionResponse response, Dictionary<string, EntityFields> relationshipFieldDic)
        {
            foreach (var relationshipField in relationshipFieldDic)
            {
                int index = 0;
                var entityName = relationshipField.Key;
                var relationshipName = formDto.FormEntities.Where(x => x.EntityName == entityName).Select(x => x.RelationshipName).FirstOrDefault();
                EntityRelationshipDto relationship = await GetEntityRelationshipByName(entityName, relationshipName);
                if (relationship != null && relationship.Type == (int)Microsoft.Crm.Sdk.Messages.RelationshipType.ManyToManyRelationship)
                {
                    var firstEntity = entityName == relationship.Entity1LogicalName ? relationship.Entity1LogicalName : relationship.Entity2LogicalName;
                    var secondEntity = entityName == relationship.Entity1LogicalName ? relationship.Entity2LogicalName : relationship.Entity1LogicalName;

                    var responseItem = response.Responses[index];
                    var formEntity = formDto.FormEntities[index];
                    var entityId = responseItem.ResponseName == "Create"
                        ? ((CreateResponse)responseItem).id
                        : formEntity.EntityId;

                    Guid firstId = entityId;
                    Guid secondId = new Guid(relationshipField.Value.AttributeValue);
                    AssociateRequest associateRequest = new AssociateRequest
                    {
                        Target = new EntityReference(firstEntity, firstId),
                        Relationship = new Relationship(relationship.Name),
                        RelatedEntities = new EntityReferenceCollection
                        {
                            new EntityReference(secondEntity, secondId)
                        }
                    };
                    orgContext.Execute(associateRequest);
                }
            }

        }

        private Entity PrepareEntity(FormEntities formEntity, List<PortalConfigDto> statusesFromConfig, FormDto formDto, OrganizationServiceContext orgContext, Guid createdRequestId, bool isNewRequest)
        {
            var entity = new Entity(formEntity.EntityName);

            foreach (var entityField in formEntity.Fields)
            {
                entity[entityField.AttributeName] = GetAttributeValue(entityField, formEntity.EntityName, entityField.RefEntityName);
            }

            if (formEntity.EntityName == EntityNames.HexaRequests)
            {
                SetEntityStatuses(entity, statusesFromConfig, formDto, orgContext, createdRequestId, isNewRequest);
            }

            return entity;
        }


        private void SetEntityStatuses(Entity entity, List<PortalConfigDto> statusesFromConfig, FormDto formDto, OrganizationServiceContext orgContext, Guid createdRequestId, bool isNewRequest)
        {
            var pendingExternalStatus = new Guid(statusesFromConfig.First(x => x.Key == PortalConfigurations.PendingProcessStatus).Value);
            var submittedStatus = new Guid(statusesFromConfig.First(x => x.Key == PortalConfigurations.SubmittedProcessStatus).Value);

            if (createdRequestId != Guid.Empty && formDto.IsSubmit && !isNewRequest)
            {
                entity["hexa_externalstatus"] = new EntityReference(EntityNames.ProcessStatusTemplate, pendingExternalStatus);
                entity["hexa_internalstatus"] = new EntityReference(EntityNames.ProcessStatusTemplate, submittedStatus);
            }
            if (isNewRequest && formDto.ProcessTemplateId.HasValue)
            {
                if (ShouldSubmitOnCreate(orgContext, formDto.ProcessTemplateId.Value))
                {
                    entity["hexa_externalstatus"] = new EntityReference(EntityNames.ProcessStatusTemplate, pendingExternalStatus);
                    entity["hexa_internalstatus"] = new EntityReference(EntityNames.ProcessStatusTemplate, submittedStatus);
                }
            }
        }

        private bool ShouldSubmitOnCreate(OrganizationServiceContext orgContext, Guid processTemplateId)
        {
            var Query = orgContext.CreateQuery(EntityNames.ProcessTemplate).AsQueryable();
            Query = Query.Where(x => ((Guid)x["hexa_processtemplateid"]).Equals(processTemplateId) && ((bool)x["hexa_issubmitoncreate"]).Equals(true));
            bool isSubmitOnCreate = Query.Select(x => x.Id).AsEnumerable().Any();
            return isSubmitOnCreate;
        }

        private ExecuteTransactionResponse ExecuteTransaction(ExecuteTransactionRequest transactionRequest)
        {
            return (ExecuteTransactionResponse)_crmService.GetInstance().Execute(transactionRequest);
        }

        private List<SaveFormResponse> ProcessTransactionResponse(ExecuteTransactionResponse response, FormDto formDto)
        {
            var createdEntities = new List<SaveFormResponse>();

            for (int index = 0; index < response.Responses.Count; index++)
            {
                var responseItem = response.Responses[index];
                var formEntity = formDto.FormEntities[index];
                var entityId = responseItem.ResponseName == "Create"
                    ? ((CreateResponse)responseItem).id
                    : formEntity.EntityId;
                try
                {
                    var internalStatus = formEntity.EntityName == EntityNames.HexaRequests ? GetRequestInternalStatus(entityId) : null;

                    createdEntities.Add(new SaveFormResponse
                    {
                        EntityName = formEntity.EntityName,
                        EntityId = entityId,
                        InternalStatus = internalStatus
                    });
                }
                catch (Exception)
                {
                    //Key Already Added
                }

            }

            return createdEntities;
        }



        private async Task<bool> IsGrantedPermissionsForSaveForm(FormDto formDto)
        {
            RequestStepAuthorizationNeedsDto requestStepAuthorizationNeeds = new RequestStepAuthorizationNeedsDto();
            var updateRequestId = formDto.FormEntities.Where(e => e.EntityName == EntityNames.HexaRequests).Select(a => a.EntityId).FirstOrDefault();

            if (formDto.RequestStepId.HasValue)
            {
                requestStepAuthorizationNeeds = GetRequestStepAuthorizationNeedsDtoById(formDto.RequestStepId.Value);
            }

            if (formDto.ProcessTemplateId.HasValue)
            {
                requestStepAuthorizationNeeds = GetRequestAuthorizationNeedsDtoByProcessTemplateId(formDto.ProcessTemplateId.Value);
            }

            if (updateRequestId != Guid.Empty)
            {
                requestStepAuthorizationNeeds = GetRequestAuthorizationNeedsDtoByRequestId(updateRequestId);
            }

            var hexaRequsetDto = new DTOs.HexaRequestStepDto
            {
                PortalContact = requestStepAuthorizationNeeds.PortalContact,
                PortalRole = requestStepAuthorizationNeeds.portalRoleOnProcessStepTemplate,
                StateCode = requestStepAuthorizationNeeds.StateCode
            };
            if (hexaRequsetDto.StateCode == null)
            {
                hexaRequsetDto.StateCode = new EntityOptionSetDto { Value = "0" };
            }

            if (formDto.IsReassign.Value)
            {
                return await CheckPermissionToReassignHexaRequestStep(hexaRequsetDto);
            }
            else
            {
                if (formDto.ProcessTemplateId.HasValue)
                {
                    if (updateRequestId == Guid.Empty)
                    {
                        return await CheckPermissionToCreateHexaRequest(hexaRequsetDto, PageRoute.RequestNew);
                    }
                    else
                    {

                        return await CheckPermissionToUpdateHexaRequestStepOrRequest(hexaRequsetDto, PageRoute.RequestDetails);
                    }
                }

                return await CheckPermissionToUpdateHexaRequestStepOrRequest(hexaRequsetDto, PageRoute.TaskDetails);
            }
        }

        private RequestStepAuthorizationNeedsDto GetRequestAuthorizationNeedsDtoByRequestId(Guid updateRequestId)
        {
            QueryExpression query = new QueryExpression(EntityNames.HexaRequests);
            query.ColumnSet = new ColumnSet("hexa_requestid", "hexa_portalcontact", "statecode");

            var portalContactLink = new LinkEntity
            {
                LinkFromEntityName = EntityNames.HexaRequests,
                LinkFromAttributeName = "hexa_portalcontact",
                LinkToEntityName = EntityNames.Contact,
                LinkToAttributeName = "contactid",
                JoinOperator = JoinOperator.LeftOuter,
                Columns = new ColumnSet("contactid", "firstname", "ntw_firstnamearabic", "lastname", "ntw_lastnamearabic"),
                EntityAlias = "PortalContact",
            };

            var processStepTemplateLink = new LinkEntity
            {
                LinkFromEntityName = EntityNames.HexaRequests,
                LinkFromAttributeName = "hexa_processtemplate",
                LinkToEntityName = EntityNames.ProcessTemplate,
                LinkToAttributeName = "hexa_processtemplateid",
                JoinOperator = JoinOperator.Inner,
                Columns = new ColumnSet("hexa_processtemplateid", "hexa_portalroleid"),
                EntityAlias = "ProcessTemplate",
            };
            // Add link entities to the query
            query.LinkEntities.Add(portalContactLink);
            query.LinkEntities.Add(processStepTemplateLink);
            query.Criteria.AddCondition("hexa_requestid", ConditionOperator.Equal, updateRequestId);
            EntityCollection result = _crmService.GetInstance().RetrieveMultiple(query);
            var hexaEntity = result.Entities.Select(entity => new RequestStepAuthorizationNeedsDto
            {
                PortalContact = entity.GetValueByAttributeName<EntityReferenceDto>("hexa_portalcontact"),
                portalRoleOnProcessStepTemplate = (EntityReference)CRMOperations.GetValueByAttributeName<AliasedValue>(entity, "ProcessTemplate.hexa_portalroleid")?.Value,
                StateCode = entity.GetValueByAttributeName<EntityOptionSetDto>("statecode")
            }).FirstOrDefault();

            return hexaEntity;
        }

        private Guid CreateRequest(FormDto formDto)
        {
            var Entity = new Entity(EntityNames.Request);

            Entity["hexa_processtemplate"] = new EntityReference(EntityNames.ProcessTemplate, formDto.ProcessTemplateId.Value);
            Entity["hexa_customer"] = new EntityReference(EntityNames.Account, new Guid(_sessionService.GetCompanyId()));
            Entity["hexa_portalcontact"] = new EntityReference(EntityNames.Contact, new Guid(_sessionService.GetContactId()));

            foreach (var entityField in formDto.FormEntities[0].Fields)
            {
                Entity[entityField.AttributeName] = GetAttributeValue(entityField, formDto.FormEntities[0].EntityName, entityField.RefEntityName);
            }

            return _crmService.Create(Entity, EntityNames.Request);
        }

        public void SubmitHexaRequestStep(FormDto formDto)
        {
            var criteria = new FilterExpression();
            criteria.AddCondition("hexa_steptransitiontemplateid", ConditionOperator.Equal, formDto.TransitionId.Value);
            QueryExpression query = new QueryExpression(EntityNames.HexaStepTransitionTemplate)
            {
                ColumnSet = new ColumnSet("hexa_transition"),
                LinkEntities =
                {
                    new LinkEntity
                    {
                        LinkFromEntityName = EntityNames.HexaStepTransitionTemplate,
                        LinkFromAttributeName = "hexa_transition",
                        LinkToEntityName = EntityNames.HexaTransitionTemplate,
                        LinkToAttributeName = "hexa_transitiontemplateid",
                        JoinOperator = JoinOperator.Inner,
                        Columns = new ColumnSet("hexa_statusfrom","hexa_statusto"),
                        EntityAlias = "TransitionTemplate"
                    }
                },
                Criteria = criteria
            };
            var transitionEntity = _crmService.GetInstance().RetrieveMultiple(query).Entities?.FirstOrDefault();
            if (transitionEntity != null)
            {
                try
                {
                    var statusTo = CRMOperations.GetValueByAttributeName<AliasedValue>(transitionEntity, "TransitionTemplate.hexa_statusto");
                    var updatedRequestStepEntity = new Entity("hexa_requeststep");
                    updatedRequestStepEntity["hexa_requeststepid"] = formDto.RequestStepId.Value;
                    updatedRequestStepEntity["hexa_stepstatus"] = new EntityReference("hexa_stepstatustemplate", ((EntityReference)statusTo.Value).Id);
                    _crmService.Update(updatedRequestStepEntity, "hexa_requeststep");
                }
                catch (Exception ex)
                {
                    bool.TryParse(ConfigurationManager.AppSettings["PassError"], out bool passError);
                    var errorMsg = ConfigurationManager.AppSettings["SaveFormErrorMsg"];
                    if (passError)
                    {
                        if (!ex.Message.ToLower().Contains(errorMsg.ToLower()))
                        {
                            throw new UserFriendlyException("MsgUnexpectedError");
                        }
                    }
                    else
                    {
                        throw new UserFriendlyException("MsgUnexpectedError");
                    }
                }

            }
        }

        public List<EntityDetailsDto> RetrieveEntitySchema(ExternalFormConfigDto FormConfig)
        {
            List<EntityDetailsDto> entityDetailsDtos = new List<EntityDetailsDto>();
            var extensionEntities = new List<string>();
            var entities = new List<string>() { EntityNames.Account, EntityNames.Contact, EntityNames.Request, EntityNames.RequestStep };

            var orgContext = new OrganizationServiceContext(_crmService.GetInstance());

            if (FormConfig == null)
            {
                IQueryable<Entity> entityQuery = orgContext.CreateQuery(EntityNames.ExtensionObject).AsQueryable();
                extensionEntities = entityQuery.Select(entity => FillEntitiesList(entity)).ToList();
            }
            else
            {
                extensionEntities.Add(RetrieveExtensionObjectByFormConfig(FormConfig));
                if (!string.IsNullOrEmpty(FormConfig.GridTargetEntity))
                {
                    entities.Add(FormConfig.GridTargetEntity);
                }
            }

            entities.AddRange(extensionEntities);
            entities.RemoveAll(item => string.IsNullOrEmpty(item));

            // if type is process template and formType is create
            if (FormConfig.Type.Value == "0" && FormConfig.FormType.Value == "0")
            {
                entities = new List<string> { EntityNames.Request };
            }

            // if type is process template and form type is update
            if (FormConfig.Type.Value == "0" && FormConfig.FormType.Value == "1")
            {
                entities.Remove(EntityNames.RequestStep);
            }

            entityDetailsDtos = FillEntityDetails(entities);

            return entityDetailsDtos;
        }

        public async Task<List<HexaProcessTemplateDto>> RetrieveProcessTemplates()
        {
            var RoleType = await _userPermissionAppService.LoggedInUserRoleType();

            QueryExpression query = new QueryExpression(EntityNames.ProcessTemplate)
            {
                ColumnSet = new ColumnSet("hexa_processtemplateid", "hexa_name", "hexa_nameen", "hexa_namear", "hexa_description", "hexa_descriptionar", "hexa_sla", "createdon", "hexa_portalroleid"),
            };
            var roleLink = new LinkEntity
            {
                LinkFromEntityName = EntityNames.ProcessTemplate,
                LinkFromAttributeName = "hexa_portalroleid",
                LinkToEntityName = EntityNames.PortalRole,
                LinkToAttributeName = "hexa_portalroleid",
                JoinOperator = JoinOperator.Inner,
                Columns = new ColumnSet("hexa_portalroleid"),
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

            query.LinkEntities.Add(roleLink);
            query.Criteria.AddCondition("hexa_isexternalusertypecode", ConditionOperator.Equal, 0);
            query.Criteria.AddCondition("hexa_isshowonprocedurescreen", ConditionOperator.Equal, true);
            query.Criteria.AddCondition("Role", "hexa_portalroleid", ConditionOperator.NotNull);
            var entityCollection = _crmService.GetInstance().RetrieveMultiple(query);
            List<HexaProcessTemplateDto> hexaProcessTemplates = entityCollection.Entities.Select(entity => FillProcessTemplates(entity)).ToList();
            return hexaProcessTemplates.OrderBy(x => x.Name).ToList();

        }

        public List<TransitionHistoryDto> RetrieveTransitionHistoriesByRequestId(Guid requestId)
        {
            QueryExpression query = new QueryExpression(EntityNames.RequestStep);
            query.ColumnSet = new ColumnSet("hexa_requeststepid", "hexa_request", "hexa_name", "hexa_processstep", "ownerid", "hexa_portalcontact");

            var processStepTemplateLink = new LinkEntity(EntityNames.RequestStep, EntityNames.ProcessStepTemplate, "hexa_processstep", "hexa_processsteptemplateid", JoinOperator.Inner);
            processStepTemplateLink.Columns = new ColumnSet("hexa_isexternalusertypecode", "hexa_name");
            processStepTemplateLink.EntityAlias = "ProcessStepTemplate";

            var transitionHistoryLink = new LinkEntity
            {
                LinkFromEntityName = EntityNames.RequestStep,
                LinkFromAttributeName = "hexa_requeststepid",
                LinkToEntityName = EntityNames.TransitionHistory,
                LinkToAttributeName = "hexa_requeststepid",
                JoinOperator = JoinOperator.Inner,
                Columns = new ColumnSet("hexa_transitionhistoryid", "hexa_comments", "createdon", "hexa_actionname", "hexa_actiontype"),
                EntityAlias = "TransitionHistory"
            };
            transitionHistoryLink.LinkCriteria.AddCondition(new ConditionExpression
            {
                AttributeName = "hexa_isrecommendation",
                Operator = ConditionOperator.Equal,
                Values = { true }
            });

            var portalContactLink = new LinkEntity
            {
                LinkFromEntityName = EntityNames.RequestStep,
                LinkFromAttributeName = "hexa_portalcontact",
                LinkToEntityName = EntityNames.Contact,
                LinkToAttributeName = "contactid",
                JoinOperator = JoinOperator.LeftOuter,
                Columns = new ColumnSet("contactid", "firstname", "ntw_firstnamearabic", "lastname", "ntw_lastnamearabic"),
                EntityAlias = "PortalContact",
            };

            query.Criteria.AddCondition("hexa_request", ConditionOperator.Equal, requestId);
            processStepTemplateLink.LinkCriteria.AddCondition(new ConditionExpression
            {
                AttributeName = "hexa_isexternalusertypecode",
                Operator = ConditionOperator.Equal,
                Values = { (int)ExternalUser.Yes }
            });
            query.LinkEntities.Add(processStepTemplateLink);
            query.LinkEntities.Add(transitionHistoryLink);
            query.LinkEntities.Add(portalContactLink);

            transitionHistoryLink.Orders.Add(new OrderExpression("createdon", OrderType.Descending));

            var entityCollection = _crmService.GetInstance().RetrieveMultiple(query);

            if (entityCollection.Entities.Any())
            {
                return entityCollection.Entities.Select(entityValue => FillTransitionHistory(entityValue)).ToList();
            }

            return new List<TransitionHistoryDto>();
        }

        public List<PortalCommentDto> GetPortalCommentByRequestId(Guid requestId)
        {
            List<PortalCommentDto> portalComments = new List<PortalCommentDto>();
            QueryExpression query = new QueryExpression(EntityNames.PortalComment)
            {
                ColumnSet = new ColumnSet("hexa_portalcommentid", "hexa_comments", "hexa_request", "createdon", "hexa_contact", "ownerid"),
                Criteria = new FilterExpression
                {
                    Conditions =
                            {
                                new ConditionExpression("hexa_request", ConditionOperator.Equal, requestId)
                            }
                },
                Orders =
                            {
                                new OrderExpression("createdon", OrderType.Descending)
                            },
            };
            var contactLink = new LinkEntity
            {
                LinkFromEntityName = EntityNames.PortalComment,
                LinkFromAttributeName = "hexa_contact",
                LinkToEntityName = EntityNames.Contact,
                LinkToAttributeName = "contactid",
                JoinOperator = JoinOperator.LeftOuter,
                Columns = new ColumnSet("contactid", "entityimage", "firstname", "ntw_firstnamearabic", "lastname", "ntw_lastnamearabic"),
                EntityAlias = "Contact",
            };
            query.LinkEntities.Add(contactLink);
            var entityCollection = _crmService.GetInstance().RetrieveMultiple(query);
            if (entityCollection.Entities.Any())
            {
                return entityCollection.Entities.Select(entity =>
                {
                    var contact = entity.GetValueByAttributeName<EntityReferenceDto>("hexa_contact");

                    var portalCommentDto = new PortalCommentDto
                    {
                        Id = entity.Id,
                        Comment = entity.GetValueByAttributeName<string>("hexa_comments"),
                        CreationDate = entity.GetValueByAttributeName<DateTime>("createdon"),
                        Owner = entity.GetValueByAttributeName<EntityReferenceDto>("ownerid")
                    };

                    if (contact == null)
                    {
                        portalCommentDto.OwnerImage = GetEntityImageById(entity.GetValueByAttributeName<EntityReference>("ownerid")?.Id, entity.GetValueByAttributeName<EntityReference>("ownerid")?.LogicalName);
                        portalCommentDto.IsExternal = false;
                    }
                    else
                    {
                        var firstnameAr = CRMOperations.GetValueByAttributeName<AliasedValue>(entity, "Contact.ntw_firstnamearabic")?.Value?.ToString() ?? string.Empty;
                        var lastnameAr = CRMOperations.GetValueByAttributeName<AliasedValue>(entity, "Contact.ntw_lastnamearabic")?.Value?.ToString() ?? string.Empty;

                        portalCommentDto.Contact = contact;
                        portalCommentDto.Contact.NameAr = $"{firstnameAr} {lastnameAr}".Trim();
                        portalCommentDto.ContactImage = CRMOperations.GetValueByAttributeName<AliasedValue>(entity, "Contact.entityimage")?.Value as byte[];
                        portalCommentDto.IsExternal = true;
                    }

                    return portalCommentDto;
                }).ToList();
            }

            return portalComments;
        }

        private List<ExternalFormConfigDto> GetExternalFormConfigByProcessStepTemplateId(Guid ProcessStepTemplateId)
        {
            List<ExternalFormConfigDto> externalFormConfigDtos = new List<ExternalFormConfigDto>();

            List<Guid> ExternalFormConfigIds = GetExternalFormConfigIdsByTemplateId(ProcessStepTemplateId, ExternalFormType.ProcessStepTemplate);

            if (ExternalFormConfigIds != null && ExternalFormConfigIds.Count > 0)
            {
                List<ExternalFormConfigDto> externalFormConfig = new List<ExternalFormConfigDto>();
                foreach (Guid Id in ExternalFormConfigIds)
                {
                    externalFormConfig.Add(_externalFormConfigAppService.RetrieveExternalFormConfiguration(Id).FirstOrDefault());
                }

                externalFormConfig.RemoveAll(x => x.Id == Guid.Empty);

                externalFormConfigDtos = externalFormConfig;
            }

            return externalFormConfigDtos;
        }

        private List<ExternalFormConfigDto> GetExternalFormConfigByProcessTemplateId(Guid ProcessTemplateId)
        {
            List<ExternalFormConfigDto> externalFormConfigDtos = new List<ExternalFormConfigDto>();

            List<Guid> ExternalFormConfigIds = GetExternalFormConfigIdsByTemplateId(ProcessTemplateId, ExternalFormType.ProcessTemplate);

            if (ExternalFormConfigIds != null && ExternalFormConfigIds.Count > 0)
            {
                List<ExternalFormConfigDto> externalFormConfig = new List<ExternalFormConfigDto>();
                foreach (Guid Id in ExternalFormConfigIds)
                {
                    externalFormConfig.Add(_externalFormConfigAppService.RetrieveExternalFormConfiguration(Id).FirstOrDefault());
                }

                externalFormConfig.RemoveAll(x => x.Id == Guid.Empty);

                externalFormConfigDtos = externalFormConfig;
            }

            return externalFormConfigDtos;
        }

        private List<dynamic> GetRequestDocumentsByStepRequestId(Guid stepRequestId, EntityReference portalRole, EntityReference department)
        {
            List<dynamic> values = new List<dynamic>();

            var query = new QueryExpression(EntityNames.RequestDocument)
            {
                ColumnSet = new ColumnSet(true),
                Criteria = new FilterExpression()
            };
            var ProcessDocumentLink = new LinkEntity
            {
                LinkFromEntityName = EntityNames.RequestDocument,
                LinkFromAttributeName = "hexa_processdocument",
                LinkToEntityName = EntityNames.ProcessDocumentTemplate,
                LinkToAttributeName = "hexa_processdocumenttemplateid",
                JoinOperator = JoinOperator.LeftOuter,
                Columns = new ColumnSet("hexa_nameen", "hexa_namear"),
                EntityAlias = "ProcessDocument",
            };

            query.LinkEntities.Add(ProcessDocumentLink);
            query.Criteria.AddCondition("hexa_evaluatedatstep", ConditionOperator.Equal, stepRequestId);
            var entityCollection = _crmService.GetInstance().RetrieveMultiple(query);

            if (entityCollection.Entities.Any())
            {
                var refDocConfiguration = _portalConfigAppService
                .RetrievePortalConfiguration(new List<string> { PortalConfigurations.ReferenceDocumentsStructure })
                .FirstOrDefault();

                var referenceDocumentsDic = refDocConfiguration != null ? JsonConvert.DeserializeObject<Dictionary<string, string>>(refDocConfiguration?.Value) : null;
                if (referenceDocumentsDic == null)
                    throw new UserFriendlyException("ReferenceDocumentsStructureIsNotConfigured");

                var departmentName = department ==null ? string.Empty : department.Name;
                var externalRefDocName = referenceDocumentsDic[departmentName];
                referenceDocumentsDic.Remove(departmentName);
                values = ConvertEntityCollectionToListOfDynamics(entityCollection);
                values.RemoveAll(x => referenceDocumentsDic.ContainsValue((string)x.hexa_name));
                var index = values.FindIndex(x => (string)x.hexa_name == externalRefDocName);
                if (index >= 0)
                {
                    values[index].hexa_name = ((string)values[index].hexa_name).Split('-')[0].Trim();
                }
            }

            return values;
        }

        private List<dynamic> GetRequestDocumentsByRequestId(Guid RequestId)
        {
            List<dynamic> values = new List<dynamic>();

            var query = new QueryExpression(EntityNames.RequestDocument)
            {
                ColumnSet = new ColumnSet(true),
                Criteria = new FilterExpression()
            };

            query.Criteria.AddCondition("hexa_request", ConditionOperator.Equal, RequestId);
            var entityCollection = _crmService.GetInstance().RetrieveMultiple(query);

            if (entityCollection.Entities.Any())
            {
                values = ConvertEntityCollectionToListOfDynamics(entityCollection);
            }

            return values;
        }

        private List<dynamic> GetExtensionObjectByProcessTemplateId(IQueryable<Entity> RequestQuery, Guid ProcessTemplateId)
        {
            List<dynamic> Values = new List<dynamic>();

            string entityName = string.Empty;
            string fieldName = string.Empty;

            GetExtensionObjectSchemaByProcessTemplateId(ProcessTemplateId, out entityName, out fieldName);
            if (!string.IsNullOrEmpty(entityName) && !string.IsNullOrEmpty(fieldName))
            {
                Guid entityId = Guid.Empty;
                EntityReference entityReference = RequestQuery.Select(index => index.GetValueByAttributeName<EntityReference>(fieldName, ""))?.FirstOrDefault() ?? null;
                if (entityReference != null && entityReference.Id != Guid.Empty)
                {
                    entityId = entityReference.Id;
                }

                if (entityId != Guid.Empty)
                {
                    ColumnSet columns = new ColumnSet(true);
                    Entity entity = _crmService.GetInstance().Retrieve(entityName, entityId, columns);

                    Values = ConvertEntityToListOfDynamics(entity);
                }
            }

            return Values;
        }

        private List<dynamic> GetExtensionObjectByProcessStepTemplateId(IQueryable<Entity> RequestQuery, Guid ProcessStepTemplateId, Guid MasterRequest)
        {
            List<dynamic> Values = new List<dynamic>();

            GetExtensionObjectSchemaByProcessStepTemplateId(ProcessStepTemplateId, out var entityName, out var fieldName);
            if (!string.IsNullOrEmpty(entityName) && !string.IsNullOrEmpty(fieldName))
            {
                Guid entityId = Guid.Empty;
                EntityReference entityReference = RequestQuery.Select(index => index.GetValueByAttributeName<EntityReference>(fieldName, ""))?.FirstOrDefault() ?? null;
                if (entityReference != null && entityReference.Id != Guid.Empty)
                {
                    entityId = entityReference.Id;
                }

                if (entityId != Guid.Empty)
                {
                    ColumnSet columns = new ColumnSet(true);
                    Entity entity = _crmService.GetInstance().Retrieve(entityName, entityId, columns);

                    Values = ConvertEntityToListOfDynamics(entity);
                }
            }

            if (Values == null || Values.Count <= 0)
            {

                Values = GetExtensionObjectByRequestId(MasterRequest);
            }

            return Values;
        }

        private List<dynamic> GetExtensionObjectByRequestId(Guid RequestId)
        {
            var hexaRequest = new HexaRequestDto();
            var orgContext = new OrganizationServiceContext(_crmService.GetInstance());

            List<dynamic> Values = new List<dynamic>();

            IQueryable<Entity> requestQuery = orgContext.CreateQuery("hexa_request").AsQueryable();
            requestQuery = requestQuery.Where(x => ((Guid)x["hexa_requestid"]).Equals(RequestId));
            hexaRequest = requestQuery.Select(entity => FillHexaRequest(entity)).FirstOrDefault();

            if (hexaRequest != null && hexaRequest.Id != Guid.Empty)
            {
                if (hexaRequest.ProcessTemplate != null && hexaRequest.ProcessTemplate.Id != string.Empty && new Guid(hexaRequest.ProcessTemplate.Id) != Guid.Empty)
                {
                    Values = GetExtensionObjectByProcessTemplateId(requestQuery, new Guid(hexaRequest.ProcessTemplate.Id));
                }
            }

            return Values;
        }

        private void GetExtensionObjectSchemaByProcessTemplateId(Guid ProcessTemplateId, out string EntityName, out string FieldName)
        {
            var orgContext = new OrganizationServiceContext(_crmService.GetInstance());

            var Query = orgContext.CreateQuery("hexa_extensionobject")
                               .Join(
                                   orgContext.CreateQuery("hexa_processtemplate"),
                                   eo => eo["hexa_extensionobjectid"],
                                   pt => pt["hexa_extensionobject"],
                                   (eo, pt) => new { ExtensionObject = eo, ProcessTemplate = pt })
                               .Where(x => (Guid)x.ProcessTemplate["hexa_processtemplateid"] == ProcessTemplateId)
                               .Select(x => new
                               {
                                   hexa_fieldschema = x.ExtensionObject.Contains("hexa_fieldschema") ? x.ExtensionObject["hexa_fieldschema"].ToString() : null,
                                   hexa_extensionentity = x.ExtensionObject.Contains("hexa_extensionentity") ? x.ExtensionObject["hexa_extensionentity"].ToString() : null
                               }).FirstOrDefault();

            EntityName = string.Empty;
            FieldName = string.Empty;

            if (Query != null && Query.hexa_extensionentity != null && Query.hexa_fieldschema != null)
            {
                EntityName = Query.hexa_extensionentity;
                FieldName = Query.hexa_fieldschema;
            }
        }

        private void GetExtensionObjectSchemaByProcessStepTemplateId(Guid ProcessStepTemplateId, out string EntityName, out string FieldName)
        {
            var orgContext = new OrganizationServiceContext(_crmService.GetInstance());

            var Query = orgContext.CreateQuery("hexa_extensionobject")
                               .Join(
                                   orgContext.CreateQuery("hexa_processsteptemplate"),
                                   eo => eo["hexa_extensionobjectid"],
                                   pt => pt["hexa_extensionobject"],
                                   (eo, pt) => new { ExtensionObject = eo, ProcessTemplate = pt })
                               .Where(x => (Guid)x.ProcessTemplate["hexa_processsteptemplateid"] == ProcessStepTemplateId)
                               .Select(x => new
                               {
                                   hexa_fieldschema = x.ExtensionObject.Contains("hexa_fieldschema") ? x.ExtensionObject["hexa_fieldschema"].ToString() : null,
                                   hexa_extensionentity = x.ExtensionObject.Contains("hexa_extensionentity") ? x.ExtensionObject["hexa_extensionentity"].ToString() : null
                               }).FirstOrDefault();

            EntityName = string.Empty;
            FieldName = string.Empty;

            if (Query != null && Query.hexa_extensionentity != null && Query.hexa_fieldschema != null)
            {
                EntityName = Query.hexa_extensionentity;
                FieldName = Query.hexa_fieldschema;
            }
        }

        private async Task<List<DTOs.HexaRequestStepDto>> GetRequestSteps(Guid requestId)
        {
            var hexaRequestSteps = new List<DTOs.HexaRequestStepDto>();
            QueryExpression query = BuildRequestStepByIdQuery(requestId, Guid.Empty);
            EntityCollection result = _crmService.GetInstance().RetrieveMultiple(query);
            var selectedFieldsQuery = result.Entities.Select(entity => FillHexaRequestStep(entity)).ToList();
            hexaRequestSteps.AddRange(selectedFieldsQuery);

            return hexaRequestSteps;

        }

        private List<HexaProcessStepTemplate> GetProcessStepTemplates(Guid? processTemplateId, Guid? processStepTemplateId = null)
        {
            var hexaHexaProcessStepTemplates = new List<HexaProcessStepTemplate>();

            var orgContext = new OrganizationServiceContext(_crmService.GetInstance());
            IQueryable<Entity> Query = orgContext.CreateQuery("hexa_processsteptemplate").AsQueryable();

            if (processTemplateId.HasValue && processTemplateId != Guid.Empty)
            {
                Query = Query.Where(x => ((Guid)x["hexa_processtemplate"]).Equals(processTemplateId));
            }

            if (processStepTemplateId.HasValue && processStepTemplateId != Guid.Empty)
            {
                Query = Query.Where(x => ((Guid)x["hexa_processsteptemplateid"]).Equals(processStepTemplateId));
            }

            var selectedFieldsQuery = Query.Select(entity => FillProcessStepTemplate(entity)).ToList();

            hexaHexaProcessStepTemplates.AddRange(selectedFieldsQuery);

            return hexaHexaProcessStepTemplates;
        }

        private async Task<List<StageDetailsDto>> GetStepTemplates(Guid processTemplateId, Guid requestId)
        {
            var stageDetailsList = new List<StageDetailsDto>();
            var requestSteps = await GetRequestSteps(requestId);
            var roleType = await _userPermissionAppService.LoggedInUserRoleType();

            var processStepTemplateQuery = new QueryExpression(EntityNames.ProcessStepTemplate)
            {
                ColumnSet = new ColumnSet("hexa_processsteptemplateid", "hexa_processtemplate", "hexa_stageid", "hexa_name", "hexa_stepnumber", "hexa_stepinstructions", "hexa_stepinstructionsar", "hexa_summary", "hexa_summaryar", "hexa_groupid", "hexa_isexternalusertypecode", "statuscode", "hexa_defaultstepstatus", "modifiedon", "hexa_portalroleid")
            };

            var stagelink = processStepTemplateQuery.AddLink(EntityNames.StepTemplate, "hexa_stageid", "hexa_steptemplateid");
            stagelink.Columns = new ColumnSet("hexa_steptemplateid", "hexa_nameen", "hexa_namear", "hexa_stagenumber", "hexa_summary", "pwc_processtemplate");
            stagelink.EntityAlias = "StepTemplate";
            processStepTemplateQuery.Criteria.AddCondition("hexa_processtemplate", ConditionOperator.Equal, processTemplateId);
            var queryResult = _crmService.GetInstance().RetrieveMultiple(processStepTemplateQuery).Entities;
            var companyId = _sessionService.GetCompanyId();
            // Group by the Stage Id
            var groupedByStage = queryResult
                .Where(x => x.Contains("hexa_stageid") && x["hexa_stageid"] != null)
                .GroupBy(x => ((EntityReference)x["hexa_stageid"]).Id);

            int stageCounter = 0;
            foreach (var group in groupedByStage)
            {
                stageCounter++;
                var stageId = group.Key;
                var existingStage = stageDetailsList.FirstOrDefault(s => s.Stage.Id == stageId);

                if (existingStage == null)
                {
                    var firstStep = group.First();
                    var stagenumber = CRMOperations.GetValueByAttributeName<AliasedValue>(firstStep, "StepTemplate.hexa_stagenumber")?.Value.ToString() ?? string.Empty;
                    var stageDetails = new StageDetailsDto
                    {

                        Stage = new HexaStageDto
                        {
                            Id = stageId,
                            Name = CRMOperations.GetValueByAttributeName<AliasedValue>(firstStep, "StepTemplate.hexa_nameen")?.Value.ToString() ?? string.Empty,
                            NameAr = CRMOperations.GetValueByAttributeName<AliasedValue>(firstStep, "StepTemplate.hexa_namear")?.Value.ToString() ?? string.Empty,
                            StageNumber = string.IsNullOrEmpty(stagenumber) ? 0 : Convert.ToInt32(stagenumber),
                            Description = CRMOperations.GetValueByAttributeName<AliasedValue>(firstStep, "StepTemplate.hexa_summary")?.Value.ToString() ?? string.Empty,
                            Disabled = true,
                            RequestStepOwnership = (int)RequestStepOwnership.Internal,
                            CompanyId = companyId,
                            ProcessTemplate = firstStep.GetValueByAttributeName<EntityReferenceDto>("hexa_processtemplate")
                        },
                        Steps = new List<RequestStepInStagesDto>()
                    };
                    var tasks = group
                              .Where(x => requestSteps.Any())
                              .Select(async x => await FillStepTemplate(x, requestSteps))
                              .ToList();
                    var results = await Task.WhenAll(tasks);
                    stageDetails.Steps.AddRange(results.OrderBy(x => x.StepNumber));

                    stageDetailsList.Add(stageDetails);

                    //if any request under a stage is external step, then isPif flag is false, if not isPif flag should be true
                    if (stageDetails.Steps.Any(step => step.IsExternalUser == (int)ExternalUser.Yes))
                    {
                        if ((stageDetails.Steps.Any(step => step.IsExternalUser == (int)ExternalUser.Yes)) && (stageDetails.Steps.Any(step => step.IsExternalUser == (int)ExternalUser.No)))
                        {
                            stageDetails.Stage.RequestStepOwnership = (int)RequestStepOwnership.Both;
                        }
                        else
                        {
                            stageDetails.Stage.RequestStepOwnership = (int)RequestStepOwnership.External;
                        }

                        stageDetails.Stage.Disabled = false;
                    }

                    if (stageDetails.Steps.All(x => x.Id == null))
                    {
                        stageDetails.Stage.Disabled = true;
                        stageDetails.Stage.Status = new EntityOptionSetDto { Value = "3", Name = "NOT STARTED", NameAr = "لم تبدأ" };
                    }
                    else if (stageDetails.Steps.Any(x => x.StateCode.Value == "0" /*Active*/))
                    {
                        stageDetails.Stage.Status = new EntityOptionSetDto { Value = "1", Name = "PENDING", NameAr = "قيد الانتظار" };
                        var dueCol = "pwc_stage" + stageCounter + "duedate";
                        var orgContext = new OrganizationServiceContext(_crmService.GetInstance());
                        IQueryable<Entity> requestQuery = orgContext.CreateQuery("hexa_request").AsQueryable();
                        requestQuery = requestQuery.Where(x => ((Guid)x["hexa_requestid"]).Equals(requestId));
                        var dueDate = requestQuery.Select(entity => entity.GetAttributeValue<DateTime?>(dueCol)).FirstOrDefault();
                        stageDetails.Stage.DueDate = dueDate;

                    }
                    else if (stageDetails.Steps.Where(z => z.Id != null).All(x => x.StateCode.Value == "1" /*InActive*/))
                    {
                        stageDetails.Stage.Status = new EntityOptionSetDto { Value = "2", Name = "COMPLETED", NameAr = "مكتمل" };
                        stageDetails.Stage.CompletedDate = stageDetails.Steps.OrderBy(z => z.ModifiedDate).First().ModifiedDate;
                    }
                    stageDetails.Steps = stageDetails.Steps.Where(step => step.IsExternalUser == 0  /*YES*/ && (!step.RoleTypeCode.HasValue || (step.RoleTypeCode.HasValue && step.RoleTypeCode.Value == roleType))).ToList();
                }
            }
            return stageDetailsList.OrderBy(a => a.Stage.StageNumber).ToList();
        }

        private async Task<RequestStepInStagesDto> FillStepTemplate(Entity entity, List<DTOs.HexaRequestStepDto> requestSteps)
        {
            var processStepTemplateId = CRMOperations.GetValueByAttributeName<Guid>(entity, "hexa_processsteptemplateid");
            var portalRole = CRMOperations.GetValueByAttributeName<EntityReference>(entity, "hexa_portalroleid");
            var latestRequestStep = requestSteps.Where(x => x.ProcessStep.Id == processStepTemplateId).OrderBy(z => z.CreationDate).LastOrDefault();

            var defaultStepStatusConfiguration = _portalConfigAppService
                .RetrievePortalConfiguration(new List<string> { PortalConfigurations.DefaultStepStatus })
                .FirstOrDefault();

            var defaultStepStatus = defaultStepStatusConfiguration != null ? JsonConvert.DeserializeObject<EntityReferenceDto>(defaultStepStatusConfiguration?.Value) : null;

            if (latestRequestStep != null)
            {
                latestRequestStep.PortalRole = portalRole;
                var isAuthorized = await CheckPermissionToReadHexaRequestStep(latestRequestStep);
                return new RequestStepInStagesDto
                {
                    Id = latestRequestStep.Id,
                    Name = entity.GetValueByAttributeName<string>("hexa_name"),
                    NameAr = entity.GetValueByAttributeName<string>("hexa_name"),
                    StepNumber = entity.GetValueByAttributeName<decimal>("hexa_stepnumber"),
                    Instruction = entity.GetValueByAttributeName<string>("hexa_stepinstructions"),
                    InstructionAr = entity.GetValueByAttributeName<string>("hexa_stepinstructionsar"),
                    Summary = entity.GetValueByAttributeName<string>("hexa_summary"),
                    SummaryAr = entity.GetValueByAttributeName<string>("hexa_summaryar"),
                    Group = entity.GetValueByAttributeName<EntityReferenceDto>("hexa_groupid"),
                    StateCode = latestRequestStep.StateCode,
                    Status = latestRequestStep.ExternalStepStatus,
                    Customer = latestRequestStep.Customer,
                    AssignedTo = latestRequestStep.PortalContact,
                    RequestStepNumber = latestRequestStep.RequestStepNumber,
                    ModifiedDate = entity.GetValueByAttributeName<DateTime>("modifiedon"),
                    IsExternalUser = entity.GetValueByAttributeName<OptionSetValue>("hexa_isexternalusertypecode")?.Value ?? 1,
                    CanOpen = isAuthorized,
                    RoleTypeCode = latestRequestStep.RoleTypeCode
                };
            }
            else
            {
                return new RequestStepInStagesDto
                {
                    Id = null,
                    Name = entity.GetValueByAttributeName<string>("hexa_name"),
                    NameAr = entity.GetValueByAttributeName<string>("hexa_name"),
                    StepNumber = entity.GetValueByAttributeName<decimal>("hexa_stepnumber"),
                    Instruction = entity.GetValueByAttributeName<string>("hexa_stepinstructions"),
                    InstructionAr = entity.GetValueByAttributeName<string>("hexa_stepinstructionsar"),
                    Summary = entity.GetValueByAttributeName<string>("hexa_summary"),
                    SummaryAr = entity.GetValueByAttributeName<string>("hexa_summaryar"),
                    Group = entity.GetValueByAttributeName<EntityReferenceDto>("hexa_groupid"),
                    StateCode = new EntityOptionSetDto { Value = "4" },
                    Status = defaultStepStatus ?? entity.GetValueByAttributeName<EntityReferenceDto>("hexa_defaultstepstatus"),
                    Customer = null,
                    AssignedTo = null,
                    RequestStepNumber = null,
                    ModifiedDate = entity.GetValueByAttributeName<DateTime>("modifiedon"),
                    IsExternalUser = entity.GetValueByAttributeName<OptionSetValue>("hexa_isexternalusertypecode")?.Value ?? 1,
                };
            }

        }

        private HexaRequestDto FillHexaRequest(Entity entity)
        {
            DateTime? dueOnDate = GetRequestDueOnDate(entity.Id);
            var processTemplate = CRMOperations.GetValueByAttributeName<EntityReferenceDto>(entity, "hexa_processtemplate");
            if (processTemplate != null)
            {
                processTemplate.Name = CRMOperations.GetValueByAttributeName<AliasedValue>(entity, "ProcessTemplate.hexa_nameen")?.Value as string;
                processTemplate.NameAr = CRMOperations.GetValueByAttributeName<AliasedValue>(entity, "ProcessTemplate.hexa_namear")?.Value as string;
            }
            return new HexaRequestDto
            {
                Id = entity.Id,
                RequestId = entity.GetValueByAttributeName<string>("hexa_name"),
                RequestName = entity.GetValueByAttributeName<string>("hexa_name"),
                Title = entity.GetValueByAttributeName<string>("pwc_title"),
                TitleAr = entity.GetValueByAttributeName<string>("pwc_titlear"),
                Summary = entity.GetValueByAttributeName<string>("hexa_summary"),
                SummaryAr = entity.GetValueByAttributeName<string>("pwc_summaryar"),
                DueOn = dueOnDate != null ? (DateTime)dueOnDate : (DateTime?)null,
                CreationDate = entity.GetValueByAttributeName<DateTime>("createdon"),
                Status = _entitiesCacheAppService.RetrieveEntityCacheById(EntityNames.ProcessStatusTemplate, entity.GetValueByAttributeName<EntityReference>("hexa_externalstatus").Id),
                Owner = string.IsNullOrEmpty(entity.GetValueByAttributeName<EntityReferenceDto>("hexa_portalcontact")?.Id) ? entity.GetValueByAttributeName<EntityReferenceDto>("ownerid") : entity.GetValueByAttributeName<EntityReferenceDto>("hexa_portalcontact"),
                OwnerImage = string.IsNullOrEmpty(entity.GetValueByAttributeName<EntityReferenceDto>("hexa_portalcontact")?.Id) ? GetEntityImageById(entity.GetValueByAttributeName<EntityReference>("ownerid")?.Id, entity.GetValueByAttributeName<EntityReference>("ownerid")?.LogicalName) : GetEntityImageById(entity.GetValueByAttributeName<EntityReference>("hexa_portalcontact")?.Id, entity.GetValueByAttributeName<EntityReference>("hexa_portalcontact")?.LogicalName),
                ProcessTemplate = processTemplate,
                CurrentStep = entity.GetValueByAttributeName<EntityReferenceDto>("hexa_currentstep"),
                Company = entity.GetValueByAttributeName<EntityReferenceDto>("hexa_customer")
            };
        }

        private byte[] GetEntityImageById(Guid? Id, string EntityLogicalName)
        {
            byte[] ownerImage = null;

            if (Id.HasValue && Id != Guid.Empty)
            {
                var orgContext = new OrganizationServiceContext(_crmService.GetInstance());

                IQueryable<Entity> Query = orgContext.CreateQuery(EntityLogicalName).AsQueryable();
                if (EntityLogicalName == "systemuser")
                {
                    Query = Query.Where(x => ((Guid)x["systemuserid"]).Equals(Id));
                    ownerImage = (Query.Select(entity => entity.Attributes.Keys.Contains("entityimage") ? (byte[])entity["entityimage"] : null).FirstOrDefault());
                }
                if (EntityLogicalName == "team")
                {
                    Query = Query.Where(x => ((Guid)x["teamid"]).Equals(Id));
                    ownerImage = (Query.Select(entity => entity != null && entity.Attributes.Keys.Contains("entityimage") ? (byte[])entity["entityimage"] : null).FirstOrDefault());

                }
                if (EntityLogicalName == "contact")
                {
                    Query = Query.Where(x => ((Guid)x["contactid"]).Equals(Id));
                    ownerImage = (Query.Select(entity => entity != null && entity.Attributes.Keys.Contains("entityimage") ? (byte[])entity["entityimage"] : null).FirstOrDefault());
                }
            }

            return ownerImage;
        }

        private DTOs.HexaRequestStepDto FillHexaRequestStep(Entity entity)
        {
            var userImage = CRMOperations.GetValueByAttributeName<AliasedValue>(entity, "Company.entityimage")?.Value;
            var customer = entity.GetValueByAttributeName<EntityReferenceDto>("hexa_customer");
            var portalContact = entity.GetValueByAttributeName<EntityReferenceDto>("hexa_portalcontact");
            var portalRoleOnProcessStepTemplate = (EntityReference)CRMOperations.GetValueByAttributeName<AliasedValue>(entity, "ProcessTemplate.hexa_portalroleid")?.Value;
            var portalRoleType = CRMOperations.GetValueByAttributeName<AliasedValue>(entity, "PortalRole.pwc_roletypetypecode")?.Value as OptionSetValue;
            if (customer != null)
            {
                customer.NameAr = CRMOperations.GetValueByAttributeName<AliasedValue>(entity, "Company.ntw_companynamearabic")?.Value?.ToString() ?? string.Empty;
            }
            if (portalContact != null)
            {
                var firstnameAr = CRMOperations.GetValueByAttributeName<AliasedValue>(entity, "PortalContact.ntw_firstnamearabic")?.Value?.ToString() ?? string.Empty;
                var lastnameAr = CRMOperations.GetValueByAttributeName<AliasedValue>(entity, "PortalContact.ntw_lastnamearabic")?.Value?.ToString() ?? string.Empty;
                portalContact.NameAr = $"{firstnameAr} {lastnameAr}".Trim();
            }
            var department = (EntityReference)CRMOperations.GetValueByAttributeName<AliasedValue>(entity, "PortalRole.pwc_departmentid")?.Value;

            return new DTOs.HexaRequestStepDto
            {
                Id = entity.Id,
                RequestStepNumber = entity.GetValueByAttributeName<string>("hexa_requeststepnumber"),
                Name = entity.GetValueByAttributeName<string>("hexa_name"),
                StepNumber = entity.GetValueByAttributeName<decimal>("hexa_stepnumber"),
                DueOn = entity.Contains("hexa_dueon") ? entity.GetValueByAttributeName<DateTime>("hexa_dueon") : (DateTime?)null,
                CreationDate = entity.GetValueByAttributeName<DateTime>("createdon"),
                MasterRequest = FillMasterRequest(entity),
                Stage = entity.GetValueByAttributeName<EntityReferenceDto>("hexa_stageid"),
                InitiatingStep = entity.GetValueByAttributeName<EntityReference>("hexa_initiatingstep"),
                Status = _entitiesCacheAppService.RetrieveEntityCacheById(EntityNames.HexaStepStatusTemplate, entity.GetValueByAttributeName<EntityReference>("hexa_stepstatus")?.Id),
                ExternalStepStatus = _entitiesCacheAppService.RetrieveEntityCacheById(EntityNames.HexaStepStatusTemplate, entity.GetValueByAttributeName<EntityReference>("hexa_externalstepstatus")?.Id),
                StateCode = entity.GetValueByAttributeName<EntityOptionSetDto>("statecode"),
                ProcessStep = GetProcessStepTemplates(Guid.Empty, entity.GetValueByAttributeName<EntityReference>("hexa_processstep")?.Id).FirstOrDefault(),
                Customer = customer,
                PortalContact = portalContact,
                PortalRole = portalRoleOnProcessStepTemplate,
                Department=department,
                Entityimage = userImage == null ? new byte[0] : (byte[])userImage,
                RoleTypeCode = portalRoleType == null ? (int?)null : Convert.ToInt32(portalRoleType.Value)
            };
        }

        private HexaProcessStepTemplate FillProcessStepTemplate(Entity entity)
        {
            var isExternal = entity.GetValueByAttributeName<OptionSetValue>("hexa_isexternalusertypecode")?.Value ?? 1;
            return new HexaProcessStepTemplate
            {
                Id = entity.Id,
                Group = entity.GetValueByAttributeName<EntityReferenceDto>("hexa_groupid"),
                ProcessTemplate = entity.GetValueByAttributeName<EntityReferenceDto>("hexa_processtemplate"),
                Stage = entity.GetValueByAttributeName<EntityReferenceDto>("hexa_stageid"),
                StepNumber = entity.GetValueByAttributeName<decimal>("hexa_stepnumber"),
                Instruction = entity.GetValueByAttributeName<string>("hexa_stepinstructions"),
                InstructionAr = entity.GetValueByAttributeName<string>("hexa_stepinstructionsar"),
                Summary = entity.GetValueByAttributeName<string>("hexa_summary"),
                SummaryAr = entity.GetValueByAttributeName<string>("hexa_summaryar"),
                Name = entity.GetValueByAttributeName<string>("hexa_name"),
                NameAR = entity.GetValueByAttributeName<string>("hexa_name"),
                IsExternal = isExternal == 0 ? true : false,
            };
        }

        private MasterRequestDto FillMasterRequest(Entity entity)
        {
            return new MasterRequestDto
            {
                Id = entity.GetValueByAttributeName<EntityReference>("hexa_request").Id,
                Name = entity.GetValueByAttributeName<string>("hexa_name"),
                Title = entity.GetValueByAttributeName<AliasedValue>("Request.pwc_title")?.Value.ToString() ?? string.Empty,
                TitleAr = entity.GetValueByAttributeName<AliasedValue>("Request.pwc_titlear")?.Value.ToString() ?? string.Empty,
                Summary = entity.GetValueByAttributeName<AliasedValue>("Request.hexa_summary")?.Value.ToString() ?? string.Empty,
                SummaryAr = entity.GetValueByAttributeName<AliasedValue>("Request.pwc_summaryar")?.Value.ToString() ?? string.Empty,
                Purpose = (string)CRMOperations.GetValueByAttributeName<AliasedValue>(entity, "FileSharingExtension.pwc_requestpurposevalue")?.Value,
                PurposeAr = (string)CRMOperations.GetValueByAttributeName<AliasedValue>(entity, "FileSharingExtension.pwc_requestpurposevalue")?.Value,
            };
        }

        private object GetAttributeValue(EntityFields field, string entityName, string refEntityName)
        {
            string Type = string.Empty;

            Type = GetAttributeType(entityName, field.AttributeName);

            if (string.IsNullOrEmpty(Type))
            {
                throw new UserFriendlyException("InvalidAttributeType", System.Net.HttpStatusCode.BadRequest);

            }
            if (string.IsNullOrEmpty(field.AttributeValue))
            {
                return null;
            }

            try
            {
                switch (Type)
                {
                    case "string":
                        return field.AttributeValue;
                    case "integer":
                        return int.Parse(field.AttributeValue);
                    case "decimal":
                        return decimal.Parse(field.AttributeValue);
                    case "boolean":
                        return bool.Parse(field.AttributeValue);
                    case "datetime":
                        return DateTime.Parse(field.AttributeValue);
                    case "optionset":
                        return new OptionSetValue(int.Parse(field.AttributeValue));
                    case "entityreference":
                        return new EntityReference(refEntityName, Guid.Parse(field.AttributeValue));
                    case "currency":
                        return new Money(decimal.Parse(field.AttributeValue));
                    case "guid":
                        return Guid.Parse(field.AttributeValue);
                    case "byte[]":
                        return Convert.FromBase64String(field.AttributeValue);
                    default:
                        throw new UserFriendlyException("InvalidAttributeType", System.Net.HttpStatusCode.BadRequest);
                }

            }
            catch (Exception)
            {
                throw new UserFriendlyException("InvalidAttributeType", System.Net.HttpStatusCode.BadRequest);
            }
        }

        private bool ValidateEntityFields(string entityName, List<EntityFields> fields, Guid entityId)
        {
            try
            {
                RetrieveEntityRequest retrieveEntity = new RetrieveEntityRequest
                {
                    EntityFilters = EntityFilters.Entity,
                    LogicalName = entityName
                };

                _crmService.GetInstance().Execute(retrieveEntity);
            }
            catch (Exception)
            {
                throw new UserFriendlyException("InvalidEntityName", System.Net.HttpStatusCode.BadRequest);
            }

            try
            {
                if (entityId != Guid.Empty)
                {
                    ColumnSet columns = new ColumnSet(false);
                    _crmService.GetInstance().Retrieve(entityName, entityId, columns);
                }
            }
            catch (Exception)
            {
                throw new UserFriendlyException("InvalidEntityId", System.Net.HttpStatusCode.BadRequest);
            }

            RetrieveEntityRequest retrieveEntityRequest = new RetrieveEntityRequest
            {
                EntityFilters = EntityFilters.Attributes,
                LogicalName = entityName
            };

            RetrieveEntityResponse retrieveEntityResponse = (RetrieveEntityResponse)_crmService.GetInstance().Execute(retrieveEntityRequest);
            var entityMetadata = retrieveEntityResponse.EntityMetadata;

            foreach (var field in fields)
            {
                if (!entityMetadata.Attributes.Any(a => a.LogicalName.Equals(field.AttributeName, StringComparison.OrdinalIgnoreCase)))
                {
                    throw new UserFriendlyException("InvalidAttributeName", System.Net.HttpStatusCode.BadRequest);

                }
            }

            return true;
        }

        private string FillEntitiesList(Entity entity)
        {
            return entity.Contains("hexa_extensionentity") ? entity.GetAttributeValue<string>("hexa_extensionentity") : string.Empty;
        }

        private List<EntityDetailsDto> FillEntityDetails(List<string> entities)
        {
            List<EntityDetailsDto> entityDetailsDtos = new List<EntityDetailsDto>();

            foreach (var entity in entities)
            {
                RetrieveEntityRequest retrieveEntityRequest = new RetrieveEntityRequest
                {
                    EntityFilters = EntityFilters.Entity,
                    LogicalName = entity
                };

                RetrieveEntityResponse retrieveEntityResponse = (RetrieveEntityResponse)_crmService.GetInstance().Execute(retrieveEntityRequest);

                EntityMetadata entityMetadata = retrieveEntityResponse.EntityMetadata;

                EntityDetailsDto entityDetailsDto = new EntityDetailsDto();
                entityDetailsDto.LogicalName = entityMetadata.LogicalName;
                entityDetailsDto.DisplayName = entityMetadata.DisplayName.UserLocalizedLabel?.Label ?? string.Empty;
                entityDetailsDto.Description = entityMetadata.Description.UserLocalizedLabel?.Label ?? string.Empty;

                entityDetailsDtos.Add(entityDetailsDto);
            }

            return entityDetailsDtos;
        }

        private List<dynamic> ConvertEntityToListOfDynamics(Entity entity)
        {
            List<dynamic> resultList = new List<dynamic>();

            dynamic expandoObj = new ExpandoObject();
            var expandoDict = (IDictionary<string, object>)expandoObj;

            expandoDict["EntityId"] = entity.Id;
            expandoDict["EntityLogicalName"] = entity.LogicalName;
            expandoDict["IdFieldName"] = GetPrimaryKeyName(entity.LogicalName);

            foreach (var attribute in entity.Attributes)
            {
                expandoDict[attribute.Key] = attribute.Value;
            }

            resultList.Add(expandoObj);

            return resultList;
        }

        private string GetPrimaryKeyName(string entityName)
        {
            var orgContext = new OrganizationServiceContext(_crmService.GetInstance());
            RetrieveEntityRequest retrieveAttributeRequest = new RetrieveEntityRequest()
            {
                LogicalName = entityName
            };

            RetrieveEntityResponse entityResponse =
                (RetrieveEntityResponse)orgContext.Execute(retrieveAttributeRequest);

            return entityResponse.EntityMetadata.PrimaryIdAttribute;
        }

        private string RetrieveExtensionObjectByFormConfig(ExternalFormConfigDto FormConfig)
        {
            string entityName = string.Empty;
            string fieldName = string.Empty;

            if (FormConfig.Type != null && !string.IsNullOrEmpty(FormConfig.Type.Value))
            {
                int Type = -1;
                int.TryParse(FormConfig.Type.Value, out Type);
                if (Type != -1)
                {
                    switch (Type)
                    {
                        case (int)ExternalFormType.ProcessTemplate:

                            GetExtensionObjectSchemaByProcessTemplateId(new Guid(FormConfig.ProcessTemplate.Id), out entityName, out fieldName);

                            break;

                        case (int)ExternalFormType.ProcessStepTemplate:

                            GetExtensionObjectSchemaByProcessStepTemplateId(new Guid(FormConfig.ProcessStepTemplate.Id), out entityName, out fieldName);

                            if (string.IsNullOrEmpty(entityName))
                            {
                                Guid ProcessTemplateId = GetProcessTemplateId(new Guid(FormConfig.ProcessStepTemplate.Id));
                                if (ProcessTemplateId != Guid.Empty)
                                {
                                    GetExtensionObjectSchemaByProcessTemplateId(ProcessTemplateId, out entityName, out fieldName);
                                }
                            }

                            break;
                    }
                }
                else
                {
                    throw new UserFriendlyException("FormTypeInvalid", System.Net.HttpStatusCode.BadRequest);
                }
            }
            else
            {
                throw new UserFriendlyException("FormTypeInvalid", System.Net.HttpStatusCode.BadRequest);
            }

            return entityName;
        }

        private Guid GetProcessTemplateId(Guid ProcessStepTemplateId)
        {
            Guid processTemplateId = Guid.Empty;
            var orgContext = new OrganizationServiceContext(_crmService.GetInstance());

            IQueryable<Entity> Query = orgContext.CreateQuery(EntityNames.ProcessStepTemplate).AsQueryable();
            Query = Query.Where(x => ((Guid)x["hexa_processsteptemplateid"]).Equals(ProcessStepTemplateId));
            processTemplateId = Query.Select(entity => ((EntityReference)entity["hexa_processtemplate"]) != null ? ((EntityReference)entity["hexa_processtemplate"]).Id : Guid.Empty).FirstOrDefault();

            return processTemplateId;
        }

        private List<dynamic> ConvertEntityCollectionToListOfDynamics(EntityCollection entities)
        {
            var resultList = new List<dynamic>();

            foreach (var entity in entities.Entities)
            {
                dynamic expandoObj = new ExpandoObject();
                var expandoDict = (IDictionary<string, object>)expandoObj;

                foreach (var attribute in entity.Attributes)
                {
                    var key = attribute.Key;
                    var value = key.Contains(".") ? GetAliasedValue(attribute.Value) : attribute.Value;

                    expandoDict[key] = value;
                }

                resultList.Add(expandoObj);
            }

            return resultList;
        }

        private string GetAttributeType(string EntityName, string AttributeName)
        {
            string type = string.Empty;

            RetrieveEntityRequest retrieveEntityRequest = new RetrieveEntityRequest()
            {
                EntityFilters = EntityFilters.Attributes,
                LogicalName = EntityName
            };

            RetrieveEntityResponse retrieveEntityResponse = (RetrieveEntityResponse)_crmService.GetInstance().Execute(retrieveEntityRequest);
            EntityMetadata entityMetadata = retrieveEntityResponse.EntityMetadata;

            AttributeMetadata attributeMetadata = Array.Find(entityMetadata.Attributes, a => a.LogicalName == AttributeName);

            if (attributeMetadata != null)
            {

                type = attributeMetadata.AttributeType.HasValue ? MapAttributeTypeCode(attributeMetadata.AttributeType.Value) : string.Empty;
            }
            else
            {
                throw new UserFriendlyException("InvalidAttributeName", System.Net.HttpStatusCode.BadRequest);
            }

            return type;
        }

        private string MapAttributeTypeCode(AttributeTypeCode attributeTypeCode)
        {
            switch (attributeTypeCode)
            {
                case AttributeTypeCode.Boolean:
                    return "boolean";
                case AttributeTypeCode.Customer:
                case AttributeTypeCode.Lookup:
                case AttributeTypeCode.Owner:
                case AttributeTypeCode.PartyList:
                    return "entityreference";
                case AttributeTypeCode.DateTime:
                    return "datetime";
                case AttributeTypeCode.Decimal:
                    return "decimal";
                case AttributeTypeCode.Double:
                    return "decimal";
                case AttributeTypeCode.Integer:
                    return "integer";
                case AttributeTypeCode.Memo:
                case AttributeTypeCode.String:
                    return "string";
                case AttributeTypeCode.Money:
                    return "currency";
                case AttributeTypeCode.Picklist:
                case AttributeTypeCode.State:
                case AttributeTypeCode.Status:
                    return "optionset";
                case AttributeTypeCode.Uniqueidentifier:
                    return "guid";
                case AttributeTypeCode.BigInt:
                    return "integer";
                case AttributeTypeCode.ManagedProperty:
                    return "string";
                case AttributeTypeCode.CalendarRules:
                case AttributeTypeCode.Virtual:
                case AttributeTypeCode.EntityName:
                default:
                    throw new UserFriendlyException("InvalidAttributeType", System.Net.HttpStatusCode.BadRequest);
            }
        }

        private HexaStepTransitionDto FillStepTransitions(Entity entity)
        {
            return new HexaStepTransitionDto
            {
                Id = entity.Id,
                Name = CRMOperations.GetValueByAttributeName<string>(entity, "hexa_name"),
                ActionLabel = CRMOperations.GetValueByAttributeName<string>(entity, "hexa_actionlabel"),
                DisplayOrder = CRMOperations.GetValueByAttributeName<int>(entity, "hexa_displayorder"),
                Transition = CRMOperations.GetValueByAttributeName<EntityReferenceDto>(entity, "hexa_transition"),
                ProcessTemplate = CRMOperations.GetValueByAttributeName<EntityReferenceDto>(entity, "hexa_processtemplate"),
                ProcessStep = CRMOperations.GetValueByAttributeName<EntityReferenceDto>(entity, "hexa_processstep"),
                RequestStatusExternal = CRMOperations.GetValueByAttributeName<EntityReferenceDto>(entity, "hexa_requeststatusexternal"),
                RequestStatusInternal = CRMOperations.GetValueByAttributeName<EntityReferenceDto>(entity, "hexa_requeststatusinternal"),
                IsCommentMandatory = CRMOperations.GetValueByAttributeName<bool>(entity, "hexa_iscommentmandatory"),
                CreationDate = CRMOperations.GetValueByAttributeName<DateTime>(entity, "createdon"),
            };
        }

        private HexaProcessTemplateDto FillProcessTemplates(Entity entity)
        {
            return new HexaProcessTemplateDto
            {
                Id = entity.Id,
                ExternalForms = GetExternalFormConfigByProcessTemplateId(entity.Id),
                Name = CRMOperations.GetValueByAttributeName<string>(entity, "hexa_name"),
                NameEn = CRMOperations.GetValueByAttributeName<string>(entity, "hexa_nameen"),
                NameAr = CRMOperations.GetValueByAttributeName<string>(entity, "hexa_namear"),
                Description = CRMOperations.GetValueByAttributeName<string>(entity, "hexa_description"),
                DescriptionAr = CRMOperations.GetValueByAttributeName<string>(entity, "hexa_descriptionar"),
                EstimatedSla = CRMOperations.GetValueByAttributeName<double?>(entity, "hexa_sla"),
                CreationDate = CRMOperations.GetValueByAttributeName<DateTime>(entity, "createdon"),
            };
        }

        private List<Guid> GetExternalFormConfigIdsByTemplateId(Guid TemplateId, ExternalFormType externalFormType)
        {
            List<Guid> Ids = new List<Guid>();
            var externalFormConfigs = new List<ExternalFormConfigDto>();
            var orgContext = new OrganizationServiceContext(_crmService.GetInstance());

            IQueryable<Entity> requestQuery = orgContext.CreateQuery(EntityNames.ExternalFormConfiguration).AsQueryable();

            if (externalFormType == ExternalFormType.ProcessTemplate)
            {
                requestQuery = requestQuery.Where(x => ((EntityReference)x["hexa_processtemplateid"]).Id.Equals(TemplateId));
            }
            else
            {
                requestQuery = requestQuery.Where(x => ((EntityReference)x["hexa_processsteptemplateid"]).Id.Equals(TemplateId));
            }

            Ids = requestQuery.Select(entity => entity.Id).ToList();

            return Ids;
        }

        private DateTime? GetRequestDueOnDate(Guid RequestId)
        {
            QueryExpression query = new QueryExpression("hexa_requeststep")
            {
                ColumnSet = new ColumnSet("hexa_requeststepid", "hexa_dueon"),
                Criteria = new FilterExpression
                {
                    Conditions =
                {
                    new ConditionExpression("hexa_request", ConditionOperator.Equal, RequestId)
                }
                },
                LinkEntities =
            {
                new LinkEntity
                {
                    LinkFromEntityName = EntityNames.RequestStep,
                    LinkFromAttributeName = "hexa_processstep",
                    LinkToEntityName = EntityNames.ProcessStepTemplate,
                    LinkToAttributeName = "hexa_processsteptemplateid",
                    JoinOperator = JoinOperator.Inner,
                    EntityAlias = "ab",
                    LinkCriteria =
                    {
                        Conditions =
                        {
                            new ConditionExpression("hexa_isexternalusertypecode", ConditionOperator.Equal, 0)
                        }
                    }
                }
            }
            };

            EntityCollection results = _crmService.GetInstance().RetrieveMultiple(query);

            List<DateTime> hexaDueOnList = new List<DateTime>();
            foreach (var entity in results.Entities)
            {
                if (entity.Contains("hexa_dueon"))
                {
                    hexaDueOnList.Add(entity.GetAttributeValue<DateTime>("hexa_dueon"));
                }
            }

            hexaDueOnList.RemoveAll(x => x == null || x.Year <= 1900);

            if (hexaDueOnList.Count > 0)
            {
                DateTime now = DateTime.Now;
                return hexaDueOnList.OrderBy(date => Math.Abs((date - now).TotalSeconds)).FirstOrDefault();
            }

            return null;
        }

        private TransitionHistoryDto FillTransitionHistory(Entity entity)
        {
            var portalContact = entity.GetValueByAttributeName<EntityReferenceDto>("hexa_portalcontact");
            if (portalContact != null)
            {
                var firstnameAr = CRMOperations.GetValueByAttributeName<AliasedValue>(entity, "PortalContact.ntw_firstnamearabic")?.Value?.ToString() ?? string.Empty;
                var lastnameAr = CRMOperations.GetValueByAttributeName<AliasedValue>(entity, "PortalContact.ntw_lastnamearabic")?.Value?.ToString() ?? string.Empty;
                portalContact.NameAr = $"{firstnameAr} {lastnameAr}".Trim();
            }
            return new DTOs.TransitionHistoryDto
            {
                Id = entity.Id,
                Comment = CRMOperations.GetValueByAttributeName<AliasedValue>(entity, "TransitionHistory.hexa_comments")?.Value?.ToString() ?? string.Empty,
                CreationDate = (DateTime)CRMOperations.GetValueByAttributeName<AliasedValue>(entity, "TransitionHistory.createdon").Value,
                PortalContact = portalContact,
                Owner = entity.GetValueByAttributeName<EntityReferenceDto>("ownerid"),
                TaskName = entity.GetValueByAttributeName<string>("hexa_name"),
                ActionName = entity.GetValueByAttributeName<AliasedValue>("TransitionHistory.hexa_actionname")?.Value?.ToString() ?? string.Empty,
                ActionType = _entitiesCacheAppService.RetrieveOptionSetCacheByKeyWithValue(OptionSetKey.TransitionHistoryActionType, (OptionSetValue)entity.GetValueByAttributeName<AliasedValue>("TransitionHistory.hexa_actiontype")?.Value)
            };
        }

        private EntityReferenceDto GetRequestInternalStatus(Guid requestId)
        {
            var entity = _crmService.GetById(EntityNames.HexaRequests,
                                                new string[] { "hexa_internalstatus" },
                                                requestId,
                                                "hexa_requestid");
            var internalStatus = _entitiesCacheAppService.RetrieveEntityCacheById(EntityNames.ProcessStatusTemplate, entity.GetValueByAttributeName<EntityReference>("hexa_internalstatus").Id);
            return internalStatus;
        }
        private string GetLatestPortalCommentByRequestStepId(Guid requestStepId)
        {
            QueryExpression query = new QueryExpression(EntityNames.PortalComment)
            {
                ColumnSet = new ColumnSet("hexa_comments", "hexa_requeststep"),
                Criteria = new FilterExpression
                {
                    Conditions =
                            {
                                new ConditionExpression("hexa_requeststep", ConditionOperator.Equal, requestStepId)
                            }
                },
                Orders =
                            {
                                new OrderExpression("createdon", OrderType.Descending)
                            },
                TopCount = 1
            };

            var entityCollection = _crmService.GetInstance().RetrieveMultiple(query);
            var entity = entityCollection.Entities.FirstOrDefault();
            return entity?.GetAttributeValue<string>("hexa_comments") ?? string.Empty;
        }
    }
}
