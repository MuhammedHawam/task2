using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PIF.EBP.Application.AccessManagement;
using PIF.EBP.Application.AccessManagement.DTOs;
using PIF.EBP.Application.Commercialization.DTOs;
using PIF.EBP.Application.Commercialization.DTOs.IESMServiceModels;
using PIF.EBP.Application.Commercialization.Interfaces;
using PIF.EBP.Application.MetaData.DTOs;
using PIF.EBP.Application.PortalConfiguration;
using PIF.EBP.Application.Shared;
using PIF.EBP.Core.CRM;
using PIF.EBP.Core.ESM.DTOs;
using PIF.EBP.Core.Exceptions;
using PIF.EBP.Core.Session;
using PIF.EBP.Core.Utilities;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using static PIF.EBP.Application.Shared.Enums;

namespace PIF.EBP.Application.Commercialization.Implementation
{
    public class CommercializationAppService : ICommercializationAppService
    {
        private readonly IESMService _eSMService;
        private readonly ISessionService _sessionService;
        private readonly ICrmService _crmService;
        private readonly IPortalConfigAppService _portalConfigAppService;
        private readonly IAccessManagementAppService _accessMangementService;
        private readonly ICommercializationCacheManager _commercializationCacheManager;
        private readonly List<EsmOptionsDto> esmOptionsDtos = new List<EsmOptionsDto>();
        private readonly ILoggingUtility _loggingUtility;
        private Dictionary<string, string> RequestListFieldDic = new Dictionary<string, string>
        {
            { "Number","number" },
            { "SysId", "sys_id"},
            { "ServiceName", "service_name"},
            { "RequestedBy", "requested_by"},
            { "InitiationDate", "initiation_date"},
            { "State", "state"},
            { "CompanyId", "company_id"},
            { "DueDate", "due_date"},
            { "Updated", "updated"},
            { "UpdatedBy", "updated_by"},
            { "SurveyStatus", "survey_status"},
        };
        public CommercializationAppService(IESMService eSMService, ISessionService sessionService,
                                           ICrmService crmService, IPortalConfigAppService portalConfigAppService,
                                           IAccessManagementAppService accessMangementService, ILoggingUtility loggingUtility, ICommercializationCacheManager commercializationCacheManager)
        {
            _eSMService = eSMService;
            _sessionService = sessionService;
            _crmService = crmService;
            _portalConfigAppService = portalConfigAppService;
            _accessMangementService = accessMangementService;
            _loggingUtility = loggingUtility;
            _commercializationCacheManager = commercializationCacheManager;
            var statusesMappingJson = portalConfigAppService.RetrievePortalConfiguration(new List<string> { PortalConfigurations.ESMStatusMapping }).FirstOrDefault()?.Value;
            if (!string.IsNullOrEmpty(statusesMappingJson))
            {
                esmOptionsDtos = JsonConvert.DeserializeObject<List<EsmOptionsDto>>(statusesMappingJson);
            }

        }
        public async Task<CustomizedItemDto> GetServicesList(string itemId)
        {
            await CheckPermissionAdminAndAdminIT();
            var cacheItem = await _commercializationCacheManager.GetCustomizedServiceCacheItemAsync();

            return cacheItem.CustomizedItem;

        }

        public async Task<EsmRequestListRsp> GetRequestsListByCompanyId(EsmRequestListReq esmRequestListReq)
        {
            try
            {
                await CheckPermissionAdminAndAdminIT();
                EsmRequestListRsp response = new EsmRequestListRsp();

                var companyId = _sessionService.GetCompanyId();

                Dictionary<string, double> requestCountRes = await _eSMService.GetRequestCountByCompanyId(companyId);
                if (requestCountRes != null)
                {
                    response.RequestCount = MapRequestCounts(requestCountRes);
                }


                var offset = (esmRequestListReq.PagingRequest.PageNo - 1) * esmRequestListReq.PagingRequest.PageSize;

                var order = "";
                if (esmRequestListReq.PagingRequest.SortOrder == SortOrder.Descending)
                {
                    order = "desc";
                }

                if (string.IsNullOrEmpty(esmRequestListReq.PagingRequest.SortField))
                {
                    esmRequestListReq.PagingRequest.SortField = "initiation_date";
                    order = "desc";
                }
                else
                {
                    var fieldValue = RequestListFieldDic.FirstOrDefault(x => x.Key.ToLower() == esmRequestListReq.PagingRequest.SortField.ToLower()).Value;

                    if (string.IsNullOrEmpty(fieldValue))
                    {
                        throw new UserFriendlyException("WrongSortFieldName");
                    }
                    esmRequestListReq.PagingRequest.SortField = fieldValue;
                }

                var searchParam = string.Empty;

                if (!string.IsNullOrEmpty(esmRequestListReq.Search))
                {
                    searchParam = $"service_name={esmRequestListReq.Search}*^ORnumber={esmRequestListReq.Search}*";
                }

                if (!string.IsNullOrEmpty(esmRequestListReq.StatusFilter))
                {
                    var statusFilter = Convert.ToInt32(esmRequestListReq.StatusFilter);
                    var statusOnEsm = GetESMServiceStatus(statusFilter);
                    if (string.IsNullOrEmpty(searchParam))
                    {
                        searchParam = $"state={statusOnEsm}";
                    }
                    else
                    {
                        searchParam += $"^state={statusOnEsm}";
                    }
                }

                var serviceRequestResponse = await _eSMService.GetRequestsListByCompanyId(companyId, esmRequestListReq.PagingRequest.SortField, searchParam, esmRequestListReq.PagingRequest.PageSize, offset, order);
                if (serviceRequestResponse.RequestedItems.Any())
                {
                    CustomizedItemDto customizedItemDto = await GetCustomizedServiceItem();

                    Guid[] contactIds = serviceRequestResponse.RequestedItems.Where(x => !string.IsNullOrEmpty(x.RequestedBy) && Guid.TryParse(x.RequestedBy, out _))
                                                      .Select(x => Guid.Parse(x.RequestedBy))
                                                      .Distinct()
                                                      .ToArray();
                    var contacts = RetrieveContactsById(contactIds);
                    response.Requests = serviceRequestResponse.RequestedItems.Select(request => MapRequestToDto(request, contacts, customizedItemDto)).ToList();
                }
                response.TotalCount = serviceRequestResponse.Count;

                response.Statuses = esmOptionsDtos;

                return response;
            }
            catch (UserFriendlyException ex)
            {
                if (ex.Message.ToLower().Contains("this company name does not exist"))
                {
                    return new EsmRequestListRsp();
                }
                throw ex;
            }
        }

        public async Task<EsmWidgetDataRsp> GetServicesWidgetData(int status, int size)
        {
            try
            {
                EsmWidgetDataRsp response = new EsmWidgetDataRsp();

                var companyId = _sessionService.GetCompanyId();
                Dictionary<string, double> requestCountRes = await _eSMService.GetRequestCountByCompanyId(companyId);
                if (requestCountRes != null)
                {
                    response.WidgetDataCount = MapWidgetDataCounts(requestCountRes);
                }

                var offset = 0;
                var sortField = "initiation_date";
                var order = "desc";

                var serviceRequestResponse = await _eSMService.GetRequestsListByCompanyId(companyId, sortField, string.Empty, size, offset, order);
                if (serviceRequestResponse.RequestedItems.Any())
                {
                    response.Requests = serviceRequestResponse.RequestedItems.Select(request => new RequestWidgetData
                    {
                        RequestId = request.SysId,
                        ServiceName = request.ServiceName,
                        RequestNumber = request.Number,
                        Status = GetServiceStatus(request.State)
                    }).ToList();
                }
                response.Statuses = esmOptionsDtos;

                return response;
            }
            catch (UserFriendlyException ex)
            {

                if (ex.Message.ToLower().Contains("this company name does not exist"))
                {
                    return new EsmWidgetDataRsp();
                }
                throw ex;
            }
        }

        public async Task<RequestDetailsDto> GetRequestDetailsByRequestId(string requestId)
        {
            JObject requestDetail = await _eSMService.GetRequestDetailsByRequestId(requestId);

            if (requestDetail == null || !requestDetail.HasValues)
            {
                throw new UserFriendlyException("Request detail not found.");
            }
            var sysIdValue = requestDetail["service_name"]?.ToString();//we need serviceId
            if (!string.IsNullOrEmpty(sysIdValue))
            {
                var serviceItem = await GetServiceItemByName(sysIdValue);

                FieldsMetadataDto fieldsMetadataDto = await GetCustomizedFieldsMetadata(serviceItem.ServiceId);
                RequestDetailsDto requestDetailsDto = await MapRequestDetailToDto(requestDetail, serviceItem, fieldsMetadataDto);

                return requestDetailsDto;
            }
            return null;
        }

        public async Task<FieldsMetadataDto> GetFieldsMetadataByServiceId(string serviceId)
        {
            FieldsMetadataDto fieldsMetadataDto = await GetCustomizedFieldsMetadata(serviceId);

            var serviceItem = await GetServiceItemById(serviceId);
            if (serviceItem != null)
            {
                fieldsMetadataDto.RequestTitle = serviceItem.Name;
                fieldsMetadataDto.RequestTitleAr = serviceItem.Name;
                fieldsMetadataDto.ServiceDescription = serviceItem.Description;
                fieldsMetadataDto.ServiceDescriptionAr = serviceItem.Description;
            }
            return fieldsMetadataDto;
        }

        public async Task<List<SurveyQuestionDto>> GetSurveyQuestionsByRequestId(string requestId)
        {
            var surveyQuestions = await _eSMService.GetSurveyQuestionsByRequestId(requestId);

            return surveyQuestions.Select(x => MapSurveyQuestionToDto(x)).ToList();
        }

        public async Task<SurveyStatusDto> GetSurveyStatusByRequestId(string requestId)
        {
            var surveyStatus = await _eSMService.GetSurveyStatusByRequestId(requestId);

            return new SurveyStatusDto { State = surveyStatus.State };
        }


        public async Task<string> GetAttachmentByItemId(string itemId)
        {
            var attachmentByte = await _eSMService.GetAttachmentByItemId(itemId);

            return attachmentByte;
        }
        public async Task<(byte[], string, JObject)> GetAttachmentByteByItemId(string itemId)
        {
            var response = await _eSMService.GetAttachmentByteByItemId(itemId);

            return response;
        }

        public async Task<CreateUpdateRequestResponseDto> CreateRequest(string serviceId, Dictionary<string, string> formDictionary)
        {
            var companyId = _sessionService.GetCompanyId();
            var contactId = _sessionService.GetContactId();
            ContactRole contactRole = _accessMangementService.GetContactRoles(contactId, companyId).FirstOrDefault();
            if (contactRole != null)
            {
                formDictionary.Add("user", contactRole.Contact.Name);
                formDictionary.Add("company", contactRole.Company.Name);
                formDictionary.Add("email", contactRole.Email);
            }
            formDictionary.Add("user_id", contactId);
            formDictionary.Add("company_id", companyId);

            var createResponse = await _eSMService.CreateRequest(serviceId, formDictionary);

            if (createResponse == null)
            {
                throw new UserFriendlyException("Create request failed.");
            }
            var responseDto = new CreateUpdateRequestResponseDto
            {
                Number = createResponse.Number,
                RequestId = createResponse.SysId
            };
            return responseDto;

        }

        public async Task<CreateUpdateRequestResponseDto> UpdateRequest(string requestId, Dictionary<string, string> formDictionary)
        {
            var companyId = _sessionService.GetCompanyId();
            var contactId = _sessionService.GetContactId();
            ContactRole contactRole = _accessMangementService.GetContactRoles(contactId, companyId).FirstOrDefault();
            if (contactRole != null)
            {
                formDictionary.Add("user", contactRole.Contact.Name);
                formDictionary.Add("company", contactRole.Company.Name);
                formDictionary.Add("email", contactRole.Email);
            }
            formDictionary.Add("user_id", contactId);
            formDictionary.Add("company_id", companyId);

            var requestDetail = await _eSMService.UpdateRequest(requestId, formDictionary);
            if (requestDetail == null)
            {
                throw new UserFriendlyException("Request detail not found.");
            }
            return new CreateUpdateRequestResponseDto
            {
                Number = requestDetail.Number,
                RequestId = requestDetail.SysId
            };

        }

        public async Task CommercializationUpload(string requestId, List<EsmUploadedDocument> documents)
        {
            var fileRequestAttachements = documents.Where(x => x.KeyName.ToLower() == "attachment").ToList();
            var fileFieldAttachements = documents.Where(x => x.KeyName.ToLower() != "attachment").ToList();

            foreach (var doc in fileRequestAttachements)
            {
                await _eSMService.AttachFileToRequest(requestId, doc);
            }
            foreach (var doc in fileFieldAttachements)
            {
                await _eSMService.AttachFileToField(requestId, doc);
            }
        }

        public async Task<string> SubmitSurvey(SubmitSurveyReq submitSurveyReq)
        {
            var companyId = _sessionService.GetCompanyId();
            var contactId = _sessionService.GetContactId();
            SurveyForm surveyForm = new SurveyForm();
            ContactRole contactRole = _accessMangementService.GetContactRoles(contactId, companyId).FirstOrDefault();
            if (contactRole != null)
            {
                surveyForm.User = contactRole.Contact.Name;
                surveyForm.Company = contactRole.Company.Name;
                surveyForm.Email = contactRole.Email;
            }
            surveyForm.UserId = contactId;
            surveyForm.CompanyId = companyId;

            surveyForm.Result.AddRange(submitSurveyReq.SurveyResults.Select(x => new SurveyResultForm { Answer = x.Answer, Question = x.Question }));

            var createResponse = await _eSMService.SubmitSurvey(submitSurveyReq.RequestId, surveyForm);
            return createResponse;
        }

        public async Task<TriggerNotificationResponse> TriggerNotification(TriggerNotificationRequest triggerNotificationRequest)
        {
            var orgService = _crmService.GetInstance();
            OrganizationRequest commercializationEmail = new OrganizationRequest("pwc_EBPSendCommercializationEmail");

            switch (triggerNotificationRequest.Status)
            {
                case "3":
                    commercializationEmail["Status"] = "Completed";
                    break;
                case "-5":
                    commercializationEmail["Status"] = "Returned";
                    break;
                case "4":
                case "7":
                    commercializationEmail["Status"] = "Rejected";
                    break;
                case "100":
                    commercializationEmail["Status"] = "Rating";
                    break;
                default:
                    throw new UserFriendlyException("Status Code is incorrect");
            }


            commercializationEmail["ContactId"] = triggerNotificationRequest.ContactId;
            commercializationEmail["RequestId"] = triggerNotificationRequest.RequestNumber;

            try
            {
                var orgResponse = orgService.Execute(commercializationEmail);
                string value = orgResponse.Results.Values?.FirstOrDefault()?.ToString();

                return new TriggerNotificationResponse
                {
                    Status = "success",
                    Message = "Notification created successfully"
                };
            }
            catch (Exception ex)
            {
                if (ex.Message.ToLower().Contains("contact"))
                {
                    throw new UserFriendlyException("Contact does not exists");
                }
                throw new UserFriendlyException("MsgUnexpectedError");
            }

        }
        public async Task<ServiceFileResponseDto> GetServiceFiles(string requestId)
        {
            ServiceFileResponseDto serviceFileResponseDto = new ServiceFileResponseDto();
            JObject requestDetail = await _eSMService.GetRequestDetailsByRequestId(requestId);

            var surveyStatus = requestDetail["survey_status"]?.ToString();
            serviceFileResponseDto.RequestName = requestDetail["service_name"]?.ToString();
            serviceFileResponseDto.RequestNumber = requestDetail["number"]?.ToString();
            serviceFileResponseDto.ReturnedReason = requestDetail["returned_reason"]?.ToString();
            serviceFileResponseDto.CompletionComment = requestDetail["completion_comment"]?.ToString();
            serviceFileResponseDto.RejectionComment = requestDetail["rejection_comment"]?.ToString();
            serviceFileResponseDto.State = GetServiceStatus(requestDetail["state"]?.ToString());

            var attachements = requestDetail["attachments"]?.ToObject<List<ServiceRequestAttachment>>();
            if (attachements != null && attachements.Any())
            {
                serviceFileResponseDto.Attachments = attachements.Select(x => new RequestAttachmentDto
                {
                    ContentType = x.ContentType,
                    FileName = x.FileName,
                    IssueDate = x.IssueDate,
                    Size = x.Size,
                    SysId = x.SysId,
                }).ToList();

            }
            if (!string.IsNullOrEmpty(surveyStatus))
            {
                if (surveyStatus.ToLower().Contains("ready".ToLower()) || surveyStatus.ToLower().Contains("جاهز".ToLower()))
                {
                    serviceFileResponseDto.SurveyQuestions = await GetSurveyQuestionsByRequestId(requestId);
                    if (serviceFileResponseDto.SurveyQuestions.Any())
                    {
                        serviceFileResponseDto.SurveyQuestions = serviceFileResponseDto.SurveyQuestions.OrderBy(x => x.Order).ToList();
                    }
                }
            }
            return serviceFileResponseDto;

        }

        private async Task CheckPermissionAdminAndAdminIT()
        {
            var isAuthorized = await _accessMangementService.GetIsAdminOrAdminITRoleByRoleId(_sessionService.GetRoleId());
            if (!isAuthorized)
            {
                throw new UserFriendlyException("UserUnauthorized", System.Net.HttpStatusCode.Unauthorized);
            }
        }

        private async Task<FieldsMetadataDto> GetCustomizedFieldsMetadata(string serviceId)
        {
            List<FieldDto> fields = await GetAllFieldsList(serviceId);

            if (!fields.Any(f => f.Type.ToLower() == "label"))
            {
                fields.Add(new FieldDto
                {
                    Type = "label",
                    Label = "Request Details",
                    Order = int.MinValue
                });
            }

            var formTitles = fields.Where(a => a.Type.ToLower() == "label").Select(s => s.Label).Reverse().ToList();
            var formPages = fields.Where(a => a.Type.ToLower() != "label")
                .GroupBy(item => fields
                    .Where(b => b.Type.ToLower() == "label" && b.Order <= item.Order)
                    .OrderByDescending(a => a.Order)
                    .FirstOrDefault()?.Label)
                .Select(g => new FormPageDto
                {
                    Fields = g.OrderBy(a => a.Order).ToList()
                })
                .ToList();
            for (int x = 0; x < formPages.Count; x++)
            {
                formPages[x].PageTitle = formTitles[x];
            }


            var fieldsMetadataDto = new FieldsMetadataDto();
            formPages.Reverse();
            fieldsMetadataDto.FormPages = formPages;
            return fieldsMetadataDto;
        }

        private async Task<List<FieldDto>> GetAllFieldsList(string serviceId)
        {
            var fieldsMetadata = await _eSMService.GetFieldsMetadataByServiceId(serviceId);

            if (fieldsMetadata == null)
            {
                throw new UserFriendlyException("Fields metadata not found.");
            }

            var fields = fieldsMetadata?.DynamicFields?.Select(a => new FieldDto
            {
                Name = a.Name,
                Label = a.Label,
                LabelAr = a.Label,
                Type = a.Type,
                Description = a.Description,
                DescriptionAr = a.Description,
                Rules = a.Rules,
                Required = a.Mandatory,
                Order = Convert.ToInt32(a.Order),
                Options = a.Choices?.Select(c => new OptionDto { Label = c.Label, Value = c.Name })?.ToList(),
                Condition = a.Condition,
            })?.OrderBy(a => a.Order).ToList();

            var staticFields = fieldsMetadata?.StaticFields?.Select(a => new FieldDto
            {
                Name = a.Name,
                Label = a.Label,
                LabelAr = a.Label,
                Type = a.Type,
                Description = a.Description,
                DescriptionAr = a.Description,
                Rules = a.Rules,
                Required = a.Mandatory,
                Order = Convert.ToInt32(a.Order),
                Options = a.Choices?.Select(c => new OptionDto { Label = c.Label, Value = c.Name })?.ToList()
            }).OrderBy(a => a.Order).ToList();

            fields.AddRange(staticFields);
            return fields;
        }

        private async Task<CustomizedItemDto> GetCustomizedServiceItem()
        {
            var customizedItemDto = await GetServicesList("");
            return customizedItemDto;
        }

        private async Task<RequestDetailsDto> MapRequestDetailToDto(JObject requestDetail, ServiceItemDto serviceItem, FieldsMetadataDto fieldsMetadataDto)
        {
            RequestDetailsDto requestDetailsDto = new RequestDetailsDto();
            var initiationDate = requestDetail["initiation_date"]?.ToString();
            var dueDate = requestDetail["due_date"]?.ToString();
            requestDetailsDto.RequestTitle = serviceItem.Name;
            requestDetailsDto.RequestTitleAr = serviceItem.Name;
            requestDetailsDto.ServiceDescription = serviceItem.Description;
            requestDetailsDto.ServiceDescriptionAr = serviceItem.Description;

            requestDetailsDto.RequestNumber = requestDetail["sys_id"]?.ToString();
            requestDetailsDto.ReferenceNumber = requestDetail["number"]?.ToString();
            requestDetailsDto.RequestedBy = GetRequestedByContact(requestDetail["requested_by"]?.ToString());
            requestDetailsDto.State = GetServiceStatus(requestDetail["state"]?.ToString());
            requestDetailsDto.Company = RetrieveCompanyById(requestDetail["company_id"]?.ToString());
            requestDetailsDto.SurveyStatus = requestDetail["survey_status"]?.ToString();
            requestDetailsDto.ReturnedReason = requestDetail["returned_reason"]?.ToString();
            requestDetailsDto.CompletionComment = requestDetail["completion_comment"]?.ToString();
            requestDetailsDto.RejectionComment = requestDetail["rejection_comment"]?.ToString();


            var formPages = new List<FormPageDto>();

            foreach (var x in fieldsMetadataDto.FormPages)
            {
                var fields = new List<FieldDto>();

                foreach (var z in x.Fields)
                {
                    var fieldDto = new FieldDto
                    {
                        Name = z.Name,
                        Description = z.Description,
                        Condition = z.Condition,
                        DescriptionAr = z.DescriptionAr,
                        Label = z.Label,
                        LabelAr = z.LabelAr,
                        Options = z.Options,
                        Order = z.Order,
                        Required = z.Required,
                        Rules = z.Rules,
                        Type = z.Type,
                        Value = await GetFieldValue(z, requestDetail[z.Name])
                    };

                    fields.Add(fieldDto);
                }

                formPages.Add(new FormPageDto
                {
                    PageTitle = x.PageTitle,
                    PageTitleAr = x.PageTitleAr,
                    Fields = fields
                });
            }

            requestDetailsDto.FormPages = formPages;

            if (string.IsNullOrEmpty(initiationDate))
            {
                requestDetailsDto.InitiationDate = null;
            }
            else
            {
                if (DateTime.TryParse(initiationDate, out DateTime parsedDate))
                {
                    requestDetailsDto.InitiationDate = parsedDate;
                }
                else
                {
                    requestDetailsDto.InitiationDate = null;
                }
            }

            if (string.IsNullOrEmpty(dueDate))
            {
                requestDetailsDto.DueDate = null;
            }
            else
            {
                if (DateTime.TryParse(dueDate, out DateTime parsedDate))
                {
                    requestDetailsDto.DueDate = parsedDate;
                }
                else
                {
                    requestDetailsDto.DueDate = null;
                }
            }
            return requestDetailsDto;
        }

        private RequestCountDto MapRequestCounts(Dictionary<string, double> requestCountRes)
        {
            // Map request counts to response object
            return new RequestCountDto
            {
                UnderReview = requestCountRes.GetValueOrDefault(((int)ServiceState.Open).ToString()),
                Rejected = requestCountRes.GetValueOrDefault(((int)ServiceState.ClosedIncomplete).ToString()) + requestCountRes.GetValueOrDefault(((int)ServiceState.ClosedSkipped).ToString()),
                Returned = requestCountRes.GetValueOrDefault(((int)ServiceState.Pending).ToString()),
                TotalPending = requestCountRes.GetValueOrDefault(((int)ServiceState.Open).ToString()) + requestCountRes.GetValueOrDefault(((int)ServiceState.WorkInProgress).ToString()) + requestCountRes.GetValueOrDefault(((int)ServiceState.Pending).ToString()),
                Completed = requestCountRes.GetValueOrDefault(((int)ServiceState.ClosedComplete).ToString())
            };
        }
        private WidgetDataCountDto MapWidgetDataCounts(Dictionary<string, double> requestCountRes)
        {
            return new WidgetDataCountDto
            {
                UnderReview = requestCountRes.GetValueOrDefault(((int)ServiceState.Open).ToString()),
                Returned = requestCountRes.GetValueOrDefault(((int)ServiceState.Pending).ToString()),
                Completed = requestCountRes.GetValueOrDefault(((int)ServiceState.ClosedComplete).ToString()),
                TotalPending = requestCountRes.GetValueOrDefault(((int)ServiceState.Open).ToString()) + requestCountRes.GetValueOrDefault(((int)ServiceState.WorkInProgress).ToString()) + requestCountRes.GetValueOrDefault(((int)ServiceState.Pending).ToString()),
                Rejected = requestCountRes.GetValueOrDefault(((int)ServiceState.ClosedIncomplete).ToString()) + requestCountRes.GetValueOrDefault(((int)ServiceState.ClosedSkipped).ToString()),
            };
        }

        private List<EntityReferenceDto> RetrieveContactsById(Guid[] contactIds)
        {
            QueryExpression query = new QueryExpression(EntityNames.Contact)
            {
                ColumnSet = new ColumnSet("contactid", "firstname", "lastname", "ntw_firstnamearabic", "ntw_lastnamearabic"),
                Criteria = new FilterExpression
                {
                    FilterOperator = LogicalOperator.And,
                    Conditions =
            {
                new ConditionExpression("contactid", ConditionOperator.In, contactIds)
            }
                }
            };
            EntityCollection entityCollection = _crmService.GetInstance().RetrieveMultiple(query);

            var result = entityCollection.Entities.Select(x => new EntityReferenceDto
            {
                Id = x.Id.ToString(),
                Name = $"{CRMUtility.GetAttributeValue(x, "firstname", string.Empty)} {CRMUtility.GetAttributeValue(x, "lastname", string.Empty)}".Trim(),
                NameAr = $"{CRMUtility.GetAttributeValue(x, "ntw_firstnamearabic", string.Empty)} {CRMUtility.GetAttributeValue(x, "ntw_lastnamearabic", string.Empty)}".Trim(),
            }).ToList();
            return result;
        }

        private EntityReferenceDto RetrieveCompanyById(string company)
        {
            Guid companyId;
            if (Guid.TryParse(company, out companyId))
            {
                string[] columns = new string[] { "accountid", "name", "ntw_companynamearabic" };
                var entity = _crmService.GetById(EntityNames.Account, columns, companyId, "accountid");
                Guard.AssertArgumentNotNull(entity);
                return new EntityReferenceDto
                {
                    Id = entity.Id.ToString(),
                    Name = CRMUtility.GetAttributeValue(entity, "name", string.Empty),
                    NameAr = CRMUtility.GetAttributeValue(entity, "ntw_companynamearabic", string.Empty),

                };
            }
            else
            {
                return null;
            }

        }

        private EsmRequestListDtoResponse MapRequestToDto(ServiceRequest request, IEnumerable<EntityReferenceDto> contacts, CustomizedItemDto customizedItemDto)
        {
            // Map individual request to DTO
            return new EsmRequestListDtoResponse
            {
                RequestId = request.SysId,
                RequestNumber = request.Number,
                ServiceName = request.ServiceName,
                ServiceType = GetServiceTypeByServiceName(request.ServiceName, customizedItemDto),
                InitiationDate = request.InitiationDate,
                Initiator = GetRequestedByContact(request.RequestedBy, contacts),
                ServiceSubtype = GetServiceSubTypeByServiceName(request.ServiceName, customizedItemDto),
                Status = GetServiceStatus(request.State)
            };
        }

        private SurveyQuestionDto MapSurveyQuestionToDto(SurveyQuestion surveyQuestion)
        {
            // Map individual request to DTO
            return new SurveyQuestionDto
            {
                Question = surveyQuestion.Question,
                Datatype = surveyQuestion.Datatype,
                Mandatory = surveyQuestion.Mandatory,
                Order = surveyQuestion.Order,
                Min = surveyQuestion.Min,
                Max = surveyQuestion.Max
            };
        }

        private EntityReferenceDto GetRequestedByContact(string requestedBy, IEnumerable<EntityReferenceDto> contacts)
        {
            if (string.IsNullOrEmpty(requestedBy))
            {
                return null;
            }

            return contacts?.FirstOrDefault(contact => contact.Id == requestedBy);
        }

        private EntityReferenceDto GetRequestedByContact(string requestedBy)
        {
            if (string.IsNullOrEmpty(requestedBy))
            {
                return null;
            }
            Guid contactId;
            if (Guid.TryParse(requestedBy, out contactId))
            {
                var contacts = RetrieveContactsById(new Guid[] { contactId });
                return contacts?.FirstOrDefault(contact => contact.Id == requestedBy);
            }
            else
            {
                return null;
            }

        }

        private async Task<ServiceItemDto> GetServiceItemById(string serviceId)
        {
            var customizedItemDto = await GetCustomizedServiceItem();
            if (customizedItemDto != null && customizedItemDto.Services.Any())
            {
                var serviceItem = customizedItemDto.Services.FirstOrDefault(x => x.ServiceId == serviceId);
                return serviceItem;
            }
            return null;
        }

        private async Task<ServiceItemDto> GetServiceItemByName(string serviceName)
        {
            var customizedItemDto = await GetCustomizedServiceItem();
            if (customizedItemDto != null && customizedItemDto.Services.Any())
            {
                var serviceItem = customizedItemDto.Services.FirstOrDefault(x => x.Name == serviceName);
                return serviceItem;
            }
            return null;
        }

        private async Task<object> GetFieldValue(FieldDto field, JToken fieldValue)
        {
            if (fieldValue != null &&
                fieldValue.Type == JTokenType.String &&
                !string.IsNullOrWhiteSpace(fieldValue.ToString()) &&
                field.Type.ToLower().Trim() == "attachment")
            {
                var attachmentDetails = await _eSMService.GetAttachmentDetailsByItemId(fieldValue.ToString());
                var attachment = attachmentDetails?.ToObject<ServiceRequestAttachment>();

                if (attachment != null)
                {
                    // Properly initialize a List of ExpandoObjects
                    var attachObjList = new List<ExpandoObject>();

                    // Create an ExpandoObject
                    dynamic attachObj = new ExpandoObject();
                    attachObj.content_type = attachment.ContentType;
                    attachObj.file_name = attachment.FileName;
                    attachObj.sys_id = attachment.SysId;

                    attachObjList.Add(attachObj); // Add ExpandoObject to the list

                    return attachObjList;
                }
                return null;
            }
            else
            {
                if (field.Type.ToLower().Trim() == "attachment")
                {
                    return null;
                }
                // Return the fieldValue converted to a generic object
                return fieldValue?.ToObject<object>();
            }
        }

        private string GetServiceSubTypeByServiceName(string serviceName, CustomizedItemDto customizedItemDto)
        {
            // Get subcategory by service name
            var parentServiceId = customizedItemDto.Services.FirstOrDefault(x => x.Name == serviceName)?.ParentSysId;
            return parentServiceId == null
                ? string.Empty
                : customizedItemDto.SubCategories.FirstOrDefault(x => x.SysId == parentServiceId)?.Name ?? string.Empty;
        }

        private string GetServiceTypeByServiceName(string serviceName, CustomizedItemDto customizedItemDto)
        {
            // Get category by service name
            var parentServiceId = customizedItemDto.Services.FirstOrDefault(x => x.Name == serviceName)?.ParentSysId;
            if (parentServiceId == null)
            {
                return string.Empty;
            }

            var parentSubId = customizedItemDto.SubCategories.FirstOrDefault(x => x.SysId == parentServiceId)?.ParentSysId;
            return parentSubId == null
                ? string.Empty
                : customizedItemDto.Categories.FirstOrDefault(x => x.SysId == parentSubId)?.Name ?? string.Empty;
        }

        private EsmOptionsDto GetServiceStatus(string state)
        {
            if (string.IsNullOrEmpty(state)) return null;
            int mappingState = 0;
            switch (int.Parse(state))
            {
                case (int)ServiceState.Pending:
                    mappingState = (int)ServiceStateMapping.Returned;
                    break;
                case (int)ServiceState.Open:
                    mappingState = (int)ServiceStateMapping.PendingPIFReview;
                    break;
                case (int)ServiceState.WorkInProgress:
                    mappingState = (int)ServiceStateMapping.WorkInProgress;
                    break;
                case (int)ServiceState.ClosedComplete:
                    mappingState = (int)ServiceStateMapping.Completed;
                    break;
                case (int)ServiceState.ClosedIncomplete:
                    mappingState = (int)ServiceStateMapping.Rejected;
                    break;
                case (int)ServiceState.ClosedSkipped:
                    mappingState = (int)ServiceStateMapping.Rejected;
                    break;
            }

            return esmOptionsDtos.FirstOrDefault(x => x.Value == mappingState.ToString()) ?? null;
        }

        private int GetESMServiceStatus(int state)
        {
            switch (state)
            {
                case (int)ServiceStateMapping.Returned:
                    return (int)ServiceState.Pending;
                case (int)ServiceStateMapping.PendingPIFReview:
                    return (int)ServiceState.Open;
                case (int)ServiceStateMapping.WorkInProgress:
                    return (int)ServiceState.WorkInProgress;
                case (int)ServiceStateMapping.Completed:
                    return (int)ServiceState.ClosedComplete;
                default:
                    return (int)ServiceState.ClosedIncomplete;
            }
        }
    }
}
