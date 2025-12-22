using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Newtonsoft.Json;
using PIF.EBP.Application.AccessManagement;
using PIF.EBP.Application.ciamcommunication.DTOs;
using PIF.EBP.Application.CIAMCommunication.DTOs;
using PIF.EBP.Application.Contacts.Dtos;
using PIF.EBP.Application.MetaData.DTOs;
using PIF.EBP.Application.PortalAdministration.DTOs;
using PIF.EBP.Application.PortalConfiguration;
using PIF.EBP.Application.Shared;
using PIF.EBP.Application.Shared.AppResponse;
using PIF.EBP.Application.Shared.Helpers;
using PIF.EBP.Core.CIAMCommunication;
using PIF.EBP.Core.CIAMCommunication.DTOs;
using PIF.EBP.Core.CRM;
using PIF.EBP.Core.Exceptions;
using PIF.EBP.Core.Session;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using static PIF.EBP.Application.Shared.Enums;

namespace PIF.EBP.Application.CIAMCommunication.Implmentation
{
    public class CIAMCommunicationService : ICIAMUserService
    {
        private readonly ISCIMCommunicationService _scimUserService;
        private readonly ISessionService _sessionService;
        private readonly IPortalConfigAppService _portalConfigAppService;
        private readonly ICrmService _crmService;
        private readonly IAccessManagementAppService _accessManagementAppService;

        public CIAMCommunicationService(
            ICrmService crmService,
            ISCIMCommunicationService scimUserService,
            ISessionService sessionService,
            IPortalConfigAppService portalConfigAppService, IAccessManagementAppService accessManagementAppService)
        {
            _scimUserService = scimUserService ?? throw new ArgumentNullException(nameof(scimUserService));
            _sessionService = sessionService ?? throw new ArgumentNullException(nameof(sessionService));
            _portalConfigAppService = portalConfigAppService ?? throw new ArgumentNullException(nameof(portalConfigAppService));
            _crmService = crmService ?? throw new ArgumentNullException(nameof(crmService));
            _accessManagementAppService = accessManagementAppService ?? throw new ArgumentNullException(nameof(accessManagementAppService));
        }

        public async Task<CreateInvitedUserResponse> CreateInvitedUserAsync(CreateSCIMUserRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (string.IsNullOrWhiteSpace(request.FirstName) ||
                string.IsNullOrWhiteSpace(request.FamilyName) ||
                string.IsNullOrWhiteSpace(request.UserName) ||
                string.IsNullOrWhiteSpace(request.Email))
            {
                throw new ArgumentException("FirstName, FamilyName, UserName and Email are required.");
            }

            var scimRequest = new ScimUserRequest
            {
                UserName = request.UserName,
                DisplayName = request.DisplayName ?? $"{request.FirstName} {request.FamilyName}",
                Name = new ScimName
                {
                    GivenName = request.FirstName,
                    FamilyName = request.FamilyName
                },
                Password = request.IsInvited ? null : request.Password,
                Emails = new List<ScimEmail>
                {
                    new ScimEmail { Value = request.Email, Primary = true }
                },
                PhoneNumbers = new List<ScimPhoneNumber>
                {
                    new ScimPhoneNumber { Value = request.MobileNumber, Type = "mobile" }
                },
                Wso2 = new ScimWso2Extension
                {
                    AskPassword = request.IsInvited ? "false" : "true",
                    Country = "Saudi Arabia",
                    AccountLocked = false,
                    AccountState = "UNLOCKED"
                },
                Custom = new ScimCustomExtension
                {
                    CompanyId = request.CompanyId,
                    ContactID = request.ContactID,
                    RoleID = request.RolesID?
                              .Select(g => g.ToString())
                              .ToList() ?? new List<string>()
                }
            };

            try
            {
                var scimUserResponse = await _scimUserService.CreateUserAsync(scimRequest).ConfigureAwait(false);

                if (Guid.TryParse(scimUserResponse.Id, out _))
                {
                    return new CreateInvitedUserResponse { IsSuccess = true };
                }
                else
                {
                    return new CreateInvitedUserResponse { IsSuccess = false };
                }
            }
            catch (Exception ex)
            {
                return new CreateInvitedUserResponse { IsSuccess = false };
            }
        }

        public async Task<ScimOperationResponse> UpdateUserAsync(string userId, ContactUpdateDto payload)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException(nameof(userId));
            if (payload == null)
                throw new ArgumentNullException(nameof(payload));

            try
            {
                return await _scimUserService.UpdateUserAsync(userId, ToScimPatchRequest(payload)).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return new ScimOperationResponse
                {
                    IsSuccess = false,
                    ErrorMessage = $"Exception in UpdateUserAsync: {ex.Message}",
                    RawResponse = JsonConvert.SerializeObject(ex)
                };
            }
        }

        public async Task<ScimOperationResponse> SetAccountLockedAsync(string userId, string userName, bool lockAccount)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException(nameof(userId));
            if (string.IsNullOrWhiteSpace(userName))
                throw new ArgumentException(nameof(userName));

            try
            {
                return await _scimUserService.SetAccountLockedAsync(userId, userName, lockAccount).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return new ScimOperationResponse
                {
                    IsSuccess = false,
                    ErrorMessage = $"Exception in SetAccountLockedAsync: {ex.Message}",
                    RawResponse = JsonConvert.SerializeObject(ex)
                };
            }
        }

        public async Task<ScimOperationResponse> SetAccountDisabledAsync(string userId, string userName, bool disableAccount)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException(nameof(userId));
            if (string.IsNullOrWhiteSpace(userName))
                throw new ArgumentException(nameof(userName));

            try
            {
                return await _scimUserService.SetAccountDisabledAsync(userId, userName, disableAccount).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return new ScimOperationResponse
                {
                    IsSuccess = false,
                    ErrorMessage = $"Exception in SetAccountDisabledAsync: {ex.Message}",
                    RawResponse = JsonConvert.SerializeObject(ex)
                };
            }
        }

        public async Task<CreateContactResultDto> CreateContact(SCIMContactCreateDto contactDto)
        {
            var orgService = _crmService.GetInstance();
            OrganizationRequest ContactCreateAndInvite = new OrganizationRequest("pwc_EBPValidateContactCreation");

            var firstCompanyRole = contactDto.CompanyRoles?.FirstOrDefault();
            if (firstCompanyRole == null)
                throw new UserFriendlyException("CompanyRolesRequired", HttpStatusCode.BadRequest);

            ContactCreateAndInvite["FirstName"] = contactDto.FirstName;
            ContactCreateAndInvite["LastName"] = contactDto.LastName;
            ContactCreateAndInvite["FirstNameAr"] = contactDto.FirstNameAr;
            ContactCreateAndInvite["LastNameAr"] = contactDto.LastNameAr;
            ContactCreateAndInvite["PhoneNumber"] = contactDto.MobilePhone;
            ContactCreateAndInvite["EmailAddress"] = contactDto.Email;
            ContactCreateAndInvite["CompanyId"] = firstCompanyRole.CompanyId;
            ContactCreateAndInvite["Nationality"] = contactDto.Nationality;
            ContactCreateAndInvite["CountryId"] = contactDto.Country;
            ContactCreateAndInvite["PortalRoleId"] = firstCompanyRole.RoleID;
            ContactCreateAndInvite["DepartmentId"] = contactDto.Department;
            ContactCreateAndInvite["PCAdmin"] = _sessionService.GetContactId();
            ContactCreateAndInvite["Position"] = contactDto.Position;

            string value = "";
            try
            {
                var orgResponse = orgService.Execute(ContactCreateAndInvite);
                value = orgResponse.Results.Values.FirstOrDefault()?.ToString() ?? "";

                var query = new QueryExpression(EntityNames.Contact)
                {
                    ColumnSet = new ColumnSet("contactid"),
                    Criteria = new FilterExpression()
                };
                query.Criteria.AddCondition("emailaddress1", ConditionOperator.In, contactDto.Email);
                query.Criteria.AddCondition("mobilephone", ConditionOperator.Equal, contactDto.MobilePhone);

                var contact = orgService.RetrieveMultiple(query);
                
                if (!value.StartsWith("ERR"))
                {
                    var contactEntity = contact.Entities.FirstOrDefault();
                    if (contactEntity == null)
                        throw new UserFriendlyException("ContactNotFound", HttpStatusCode.NotFound);

                    var scimResult = await CreateInvitedUserAsync(new CreateSCIMUserRequest
                    {
                        CompanyId = firstCompanyRole.CompanyId,
                        ContactID = contactEntity.Id.ToString(),
                        DisplayName = contactDto.FirstName,
                        Email = contactDto.Email,
                        FamilyName = contactDto.LastName,
                        FirstName = contactDto.FirstName,
                        MobileNumber = contactDto.MobilePhone,
                        RolesID = firstCompanyRole.RoleID,
                        UserName = contactDto.Email,
                        IsInvited = true,
                    }).ConfigureAwait(false);

                    if (scimResult.IsSuccess)
                    {
                        return new CreateContactResultDto
                        {
                            Message = value,
                            RequireAdminApproval = value.ToLower().Contains("lead"),
                        };
                    }
                    else
                    {
                        orgService.Delete(EntityNames.Contact, contactEntity.Id);
                        throw new UserFriendlyException("Issue While Registering user SCIAM Server");
                    }
                }
                else
                {
                    throw new UserFriendlyException(value);
                }
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("Required"))
                    throw new UserFriendlyException("RequiredParameters", HttpStatusCode.BadRequest);
                else if (value.StartsWith("ERR"))
                {
                    var result = _portalConfigAppService.RetrievePortalConfigurationByValue(value);
                    throw new UserFriendlyException(result.Key);
                }
                else
                    throw new UserFriendlyException("MsgUnexpectedError");
            }
        }

        public async Task<ScimUserListDto> ListAllUsersAsync()
        {
            var scimResult = await _scimUserService.GetAllUsersAsync().ConfigureAwait(false);

            if (!scimResult.IsSuccess)
                throw new Exception($"SCIM List failed: {scimResult.ErrorMessage}");

            var users = scimResult.Payload.Resources
                .Select(MapToUserDto)
                .ToList();

            return new ScimUserListDto
            {
                TotalResults = scimResult.Payload.TotalResults,
                StartIndex = scimResult.Payload.StartIndex,
                ItemsPerPage = scimResult.Payload.ItemsPerPage,
                Users = users
            };
        }

        public async Task<ScimUserDto> GetUserByUserNameAsync(string userName)
        {
            var scimResult = await _scimUserService.GetUserByUserNameAsync(userName).ConfigureAwait(false);

            if (!scimResult.IsSuccess)
                throw new Exception($"SCIM filter by userName failed: {scimResult.ErrorMessage}");

            var resource = scimResult.Payload.Resources.FirstOrDefault();
            return resource != null ? MapToUserDto(resource) : null;
        }

        public async Task<IList<ScimUserDto>> GetUsersByEmailAsync(string email)
        {
            var scimResult = await _scimUserService.GetUserByEmailAsync(email).ConfigureAwait(false);

            if (!scimResult.IsSuccess)
                throw new Exception($"SCIM filter by email failed: {scimResult.ErrorMessage}");

            return scimResult.Payload.Resources
                .Select(MapToUserDto)
                .ToList();
        }

        public async Task<ScimOperationResponse> ResendInvitationAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException(nameof(userId));

            try
            {
                return await _scimUserService.ResendInvitationAsync(userId).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return new ScimOperationResponse
                {
                    IsSuccess = false,
                    ErrorMessage = $"Exception in ResendInvitationAsync: {ex.Message}",
                    RawResponse = JsonConvert.SerializeObject(ex)
                };
            }
        }

        private static ScimPatchRequest ToScimPatchRequest(ContactUpdateDto dto)
        {
            var patch = new ScimPatchRequest();

            void AddReplace(string path, object value)
            {
                patch.Operations.Add(new ScimPatchOperation
                {
                    Op = "replace",
                    Value = new Dictionary<string, object> { { path, value } }
                });
            }


            var nameObj = new ExpandoObject() as IDictionary<string, object>;
            if (dto.FirstName != null) nameObj["givenName"] = dto.FirstName;
            if (dto.LastName != null) nameObj["familyName"] = dto.LastName;
            if (nameObj.Any())
            {
                AddReplace("name", nameObj);
            }

            if (dto.Email != null)
            {
                var emailObj = new
                {
                    value = dto.Email,
                    primary = true
                };
                AddReplace("emails", new[] { emailObj });
            }

            if (dto.MobilePhone != null)
            {
                var phoneObj = new
                {
                    value = dto.MobilePhone,
                    type = "mobile"
                };
                AddReplace("phoneNumbers", new[] { phoneObj });
            }

            var enterpriseObj = new ExpandoObject() as IDictionary<string, object>;
            if (enterpriseObj.Any())
            {
                AddReplace("urn:ietf:params:scim:schemas:extension:enterprise:2.0:User", enterpriseObj);
            }

            var wso2Obj = new ExpandoObject() as IDictionary<string, object>;
            if (dto.Country != null) wso2Obj["country"] = dto.Country;
            if (wso2Obj.Any())
            {
                AddReplace("urn:scim:wso2:schema", wso2Obj);
            }

            var customObj = new ExpandoObject() as IDictionary<string, object>;
            if (dto?.Company != null) customObj["companyId"] = dto.Company;

            if (dto.PortalRole == string.Empty)
                customObj["RoleID"] = dto.PortalRole;

            if (customObj.Any())
            {
                AddReplace("urn:scim:schemas:extension:custom:User", customObj);
            }

            return patch;
        }

        private static ScimUserDto MapToUserDto(ScimUserDetailResponse src)
        {
            var dto = new ScimUserDto
            {
                Id = src.Id,
                UserName = src.UserName,
                DisplayName = src.DisplayName,
                Email = src.Emails?.FirstOrDefault(),
                MobileNumber = src.PhoneNumbers?.FirstOrDefault(p => p.Type == "mobile")?.Value,
                GivenName = src.Name?.GivenName,
                FamilyName = src.Name?.FamilyName,
                AccountLocked = src.Enterprise?.AccountLocked,
                AccountDisabled = src.Enterprise?.AccountDisabled,
                Organization = src.Enterprise?.Organization,
                AskPassword = src.Wso2?.AskPassword != null 
                    ? (bool?)src.Wso2.AskPassword.Equals("true", StringComparison.OrdinalIgnoreCase) 
                    : null,
                Country = src.Wso2?.Country,
                AccountState = src.Wso2?.AccountState,
                CompanyRoles = new List<CompanyRolesDto>
                {
                    new CompanyRolesDto
                    {
                        CompanyId = src.Custom?.CompanyId,
                        RoleID = src.Custom?.RoleID?.Select(x => Guid.Parse(x)).ToList()
                    }
                },
                ContactID = src.Custom?.ContactID,
                Participant = src.Custom?.Participant,
            };
            return dto;
        }


        public async Task<List<ContactListResponse>> GetAllContatcs()
        {

            ListPagingResponse<ContactListResponse> oResponse = new ListPagingResponse<ContactListResponse>();
            List<ContactListResponse> contacts = new List<ContactListResponse>();
            List<ContactListResponse> pinnedContacts = new List<ContactListResponse>();

            var query = new QueryExpression(EntityNames.Contact)
            {
                ColumnSet = new ColumnSet("contactid", "firstname", "lastname", "ntw_firstnamearabic", "ntw_lastnamearabic", "emailaddress1", "mobilephone", "pwc_countryid", "pwc_position", "pwc_city", "entityimage"),
                LinkEntities =
                {
                    new LinkEntity
                    {
                        LinkFromEntityName = EntityNames.Contact,
                        LinkFromAttributeName = "contactid",
                        LinkToEntityName = EntityNames.PortalPinned,
                        LinkToAttributeName = "pwc_contactid",
                        Columns= new ColumnSet("pwc_contactid", "pwc_companyidid"),
                        EntityAlias = "Pin",
                    }


                }
            };
            var Countrylink = new LinkEntity
            {
                LinkFromEntityName = EntityNames.Contact,
                LinkFromAttributeName = "pwc_countryid",
                LinkToEntityName = EntityNames.Country,
                LinkToAttributeName = "ntw_countryid",
                JoinOperator = JoinOperator.LeftOuter,
                Columns = new ColumnSet("ntw_name", "ntw_arabicname"),
                EntityAlias = "Country"
            };
            var Positionlink = new LinkEntity
            {
                LinkFromEntityName = EntityNames.Contact,
                LinkFromAttributeName = "pwc_position",
                LinkToEntityName = EntityNames.Position,
                LinkToAttributeName = "ntw_positionid",
                JoinOperator = JoinOperator.LeftOuter,
                Columns = new ColumnSet("ntw_name", "ntw_namear"),
                EntityAlias = "Position"
            };
            var citylink = new LinkEntity
            {
                LinkFromEntityName = EntityNames.Contact,
                LinkFromAttributeName = "pwc_city",
                LinkToEntityName = EntityNames.City,
                LinkToAttributeName = "ntw_citiesid",
                JoinOperator = JoinOperator.LeftOuter,
                Columns = new ColumnSet("ntw_name", "pwc_namear"),
                EntityAlias = "City"
            };
            var RoleAssociation = new LinkEntity
            {
                LinkFromEntityName = EntityNames.Contact,
                LinkFromAttributeName = "contactid",
                LinkToEntityName = EntityNames.ContactAssociation,
                LinkToAttributeName = "hexa_contactid",
                JoinOperator = JoinOperator.LeftOuter,
                Columns = new ColumnSet("hexa_contactroleassociationid", "hexa_portalroleid"),
                EntityAlias = "RoleAssociation",
                LinkEntities =
                            {
                                new LinkEntity
                                {
                                    LinkFromEntityName = EntityNames.ContactAssociation,
                                    LinkFromAttributeName = "hexa_portalroleid",
                                    LinkToEntityName = EntityNames.PortalRole,
                                    LinkToAttributeName = "hexa_portalroleid",
                                    JoinOperator = JoinOperator.LeftOuter,
                                    Columns = new ColumnSet("pwc_roletypetypecode"),
                                    EntityAlias = "RoleTypeCode",
                                }
                            }
            };

            query.LinkEntities.Add(Countrylink);
            query.LinkEntities.Add(Positionlink);
            query.LinkEntities.Add(citylink);
            query.LinkEntities.Add(RoleAssociation);

            // Execute the query
            var retrievedContacts = _crmService.GetInstance().RetrieveMultiple(query);
            var result = retrievedContacts.Entities.Select(entity =>
            {
                OptionSetValue roleTypeValue = entity.Contains("RoleTypeCode.pwc_roletypetypecode") ?
                    (OptionSetValue)((AliasedValue)entity["RoleTypeCode.pwc_roletypetypecode"]).Value
                    : null;

                var roleTypeCode = roleTypeValue?.Value;

                return new ContactListResponse
                {
                    Id = entity.Id,
                    FirstName = CRMOperations.GetValueByAttributeName<string>(entity, "firstname"),
                    LastName = CRMOperations.GetValueByAttributeName<string>(entity, "lastname"),
                    FirstNameAr = CRMOperations.GetValueByAttributeName<string>(entity, "ntw_firstnamearabic"),
                    LastNameAr = CRMOperations.GetValueByAttributeName<string>(entity, "ntw_lastnamearabic"),
                    Email = roleTypeCode != (int)RoleType.BoardMember ? CRMOperations.GetValueByAttributeName<string>(entity, "emailaddress1") : null,
                    MobilePhone = roleTypeCode != (int)RoleType.BoardMember ? CRMOperations.GetValueByAttributeName<string>(entity, "mobilephone") : null,
                    Country = CRMOperations.GetValueByAttributeName<EntityReferenceDto>(entity, "pwc_countryid", "Country.ntw_arabicname"),
                    City = CRMOperations.GetValueByAttributeName<EntityReferenceDto>(entity, "pwc_city", "City.pwc_namear"),
                    Position = CRMOperations.GetValueByAttributeName<EntityReferenceDto>(entity, "pwc_position", "Position.ntw_namear"),
                    CompanyId = CRMOperations.GetValueByAttrNameAlised(entity, "Pin.pwc_companyidid"),
                    Entityimage = CRMOperations.GetValueByAttributeName<byte[]>(entity, "entityimage"),
                    IsPinned = true,
                };
            }).ToList();


            return result;
        }

        public async Task<List<CompanyDto>> RetrievecompaniesByContactId(string ContactId)
        {
            List<CompanyDto> response = new List<CompanyDto>();
            var contactRoles = await _accessManagementAppService.GetContactRolesByContactId(ContactId);
            if (contactRoles == null || !contactRoles.Any())
            {
                return response;
            }
            var companyIds = contactRoles.Select(x => x.Company.Id).Distinct().ToArray();
            var query = new QueryExpression(EntityNames.Account)
            {
                ColumnSet = new ColumnSet("accountid", "name", "ntw_companynamearabic", "entityimage"),
                Criteria = { Conditions = { new ConditionExpression("accountid", ConditionOperator.In, companyIds),
                new ConditionExpression("statecode", ConditionOperator.Equal, 0),
                        new ConditionExpression("ntw_isitannounced", ConditionOperator.Equal, true) }
                }
            };

            var entityCollection = _crmService.GetInstance().RetrieveMultiple(query);
            var companyDtoList = entityCollection.Entities.Select(FillEntityRoles).ToList();

            foreach (var contactRole in contactRoles)
            {
                var oCompanyDto = companyDtoList.FirstOrDefault(x => x.Id == contactRole.Company.Id);

                if (oCompanyDto != null)
                {
                    var newCompanyDto = new CompanyDto
                    {
                        Id = oCompanyDto.Id,
                        PortalRoleAssociationId = contactRole.Id,
                        RoleName = contactRole.PortalRole.Name,
                        RoleNameAr = contactRole.PortalRole.NameAr,
                        Name = oCompanyDto.Name,
                        NameAr = oCompanyDto.NameAr,
                        EntityImage = oCompanyDto.EntityImage
                    };

                    // Add the new instance to the response list
                    response.Add(newCompanyDto);
                }
            }
            return response;
        }


        private CompanyDto FillEntityRoles(Entity entity)
        {
            return new CompanyDto
            {
                Id = entity.Id.ToString(),
                Name = CRMUtility.GetAttributeValue(entity, "name", string.Empty),
                NameAr = CRMUtility.GetAttributeValue(entity, "ntw_companynamearabic", string.Empty),
                EntityImage = CRMUtility.GetAttributeValue<byte[]>(entity, "entityimage")

            };

        }

        public class CreateInvitedUserResponse
        {
            public bool IsSuccess { get; set; }
        }
    }
}
