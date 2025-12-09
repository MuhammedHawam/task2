using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Newtonsoft.Json;
using PIF.EBP.Application.ciamcommunication.DTOs;
using PIF.EBP.Application.CIAMCommunication.DTOs;
using PIF.EBP.Application.Contacts.Dtos;
using PIF.EBP.Application.PortalConfiguration;
using PIF.EBP.Application.Shared;
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

namespace PIF.EBP.Application.CIAMCommunication.Implmentation
{
    public class CIAMCommunicationService : ICIAMUserService
    {
        private readonly ISCIMCommunicationService _scimUserService;
        private readonly ISessionService _sessionService;
        private readonly IPortalConfigAppService _portalConfigAppService;
        private readonly ICrmService _crmService;

        public CIAMCommunicationService(
            ICrmService crmService,
            ISCIMCommunicationService scimUserService,
            ISessionService sessionService,
            IPortalConfigAppService portalConfigAppService)
        {
            _scimUserService = scimUserService ?? throw new ArgumentNullException(nameof(scimUserService));
            _sessionService = sessionService ?? throw new ArgumentNullException(nameof(sessionService));
            _portalConfigAppService = portalConfigAppService ?? throw new ArgumentNullException(nameof(portalConfigAppService));
            _crmService = crmService ?? throw new ArgumentNullException(nameof(crmService));
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

        public class CreateInvitedUserResponse
        {
            public bool IsSuccess { get; set; }
        }
    }
}
