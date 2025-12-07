using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using PIF.EBP.Application.Contacts.Dtos;
using PIF.EBP.Application.Shared;
using PIF.EBP.Core.CRM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static PIF.EBP.Application.Shared.Enums;
using PIF.EBP.Application.MetaData.DTOs;
using PIF.EBP.Application.Shared.AppResponse;
using PIF.EBP.Application.Shared.Helpers;
using Microsoft.Xrm.Sdk.Client;
using PIF.EBP.Core.Session;
using PIF.EBP.Core.Exceptions;
using PIF.EBP.Application.AccessManagement;
using System.Net;
using Microsoft.Xrm.Sdk.Messages;
using PIF.EBP.Core.Utilities;
using PIF.EBP.Application.PortalConfiguration;
using PIF.EBP.Application.EntitiesCache.DTOs;
using Newtonsoft.Json;
using PIF.EBP.Application.Settings.DTOs;
using System.Net.Http;
using System.Text;
using System.Configuration;

namespace PIF.EBP.Application.Contacts.Implementation
{
    public class RuleBookAppService : IContactAppService
    {
        private readonly ICrmService _crmService;
        private readonly ISessionService _sessionService;
        private readonly IPortalConfigAppService _portalConfigAppService;
        private readonly IAccessManagementAppService _roleService;
        private readonly IUserPermissionAppService _userPermissionAppService;
        private readonly IAccessManagementCacheManager _accessManagementCacheManager;
        private readonly string _serverUrl;
        private readonly HttpClient _httpClient;

        public RuleBookAppService(ICrmService crmService, ISessionService sessionService,
                                 IAccessManagementAppService roleService, IPortalConfigAppService portalConfigAppService,
                                 IUserPermissionAppService userPermissionAppService, IAccessManagementCacheManager accessManagementCacheManager)
        {
            _crmService = crmService;
            _sessionService = sessionService;
            _roleService = roleService;
            _portalConfigAppService = portalConfigAppService;
            _userPermissionAppService = userPermissionAppService;
            _accessManagementCacheManager = accessManagementCacheManager;
            _serverUrl = ConfigurationManager.AppSettings["IdentityServerUrl"];
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri(_serverUrl);
            _httpClient.DefaultRequestHeaders.Add(ConfigurationManager.AppSettings["IdentityServerAPIKeyName"], ConfigurationManager.AppSettings["IdentityServerAPIKeyValue"]);
        }

        public CreateContactResultDto CreateContact(ContactCreateDto contactDto)
        {
            var orgService = _crmService.GetInstance();
            OrganizationRequest ContactCreateAndInvite = new OrganizationRequest("pwc_EBPValidateContactCreation");

            ContactCreateAndInvite["FirstName"] = contactDto.FirstName;
            ContactCreateAndInvite["LastName"] = contactDto.LastName;
            ContactCreateAndInvite["FirstNameAr"] = contactDto.FirstNameAr;
            ContactCreateAndInvite["LastNameAr"] = contactDto.LastNameAr;
            ContactCreateAndInvite["PhoneNumber"] = contactDto.MobilePhone;
            ContactCreateAndInvite["EmailAddress"] = contactDto.Email;
            ContactCreateAndInvite["CompanyId"] = contactDto.Company;
            ContactCreateAndInvite["Nationality"] = contactDto.Nationality;
            ContactCreateAndInvite["CountryId"] = contactDto.Country;
            ContactCreateAndInvite["InviteToEBP"] = contactDto.InvitedToPortal;
            ContactCreateAndInvite["PortalRoleId"] = contactDto.PortalRole;
            ContactCreateAndInvite["DepartmentId"] = contactDto.Department;
            ContactCreateAndInvite["PCAdmin"] = _sessionService.GetContactId();
            ContactCreateAndInvite["Position"] = contactDto.Position;

            var result = new PortalConfigDto();
            string value = "";
            try
            {
                var orgResponse = orgService.Execute(ContactCreateAndInvite);
                value = orgResponse.Results.Values.FirstOrDefault().ToString();

                if (!value.StartsWith("ERR"))
                {
                    return new CreateContactResultDto
                    {
                        Message = value,
                        RequireAdminApproval = value.ToLower().Contains("lead")
                    };
                }
                else
                    throw new UserFriendlyException(value);
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("Required"))
                    throw new UserFriendlyException("RequiredParameters", HttpStatusCode.BadRequest);
                else if (value.StartsWith("ERR"))
                {
                    result = _portalConfigAppService.RetrievePortalConfigurationByValue(value);
                    throw new UserFriendlyException(result.Key);
                }
                else
                    throw new UserFriendlyException("MsgUnexpectedError");
            }
        }

        public async Task<ListPagingResponse<ContactListResponse>> RetrieveContactList(ContactListReq oContactListReq)
        {

            ListPagingResponse<ContactListResponse> oResponse = new ListPagingResponse<ContactListResponse>();
            List<ContactListResponse> contacts = new List<ContactListResponse>();
            List<ContactListResponse> pinnedContacts = new List<ContactListResponse>();

            pinnedContacts = GetPinnedContacts(oContactListReq);
            if (oContactListReq.PagingRequest.PageNo == 1 && string.IsNullOrEmpty(oContactListReq.PagingRequest.SortField) && pinnedContacts.Any())
            {
                contacts.AddRange(pinnedContacts);
                oContactListReq.PagingRequest.PageSize = oContactListReq.PagingRequest.PageSize - pinnedContacts.Count;
            }
            var result = GetContactsHavingMembershipOrExperience(oContactListReq, pinnedContacts?.Select(x => x.Id.ToString()) ?? null);
            contacts.AddRange(result.Item1);

            int totalCount = result.Item2;

            oResponse.ListResponse = new List<ContactListResponse>();
            oResponse.ListResponse.AddRange(contacts);
            oResponse.TotalCount = totalCount;
            return oResponse;
        }

        public IEnumerable<ContactDto> GetAllByCompanyId(string companyId)
        {
            string contactEntityName = EntityNames.Contact;
            var contactEntities = _crmService.GetAll(contactEntityName, ContactDto.GetContactColumns(), Guid.Parse(companyId), "accountid");
            List<ContactDto> contacts = new List<ContactDto>();
            foreach (var contactEntity in contactEntities.Entities)
            {
                contacts.Add(ContactDto.ConvertToContactDto(contactEntity));
            }
            return contacts;
        }

        public async Task<GetContactById> GetById(string contactId)
        {
            try
            {

                var contactEntity = GetContactQueryExpression(contactId);

                if (contactEntity.Attributes == null || contactEntity.Attributes.Count == 0)
                    throw new UserFriendlyException("InvalidCredentials", HttpStatusCode.BadRequest);

                var contact = MapToContactDto(contactEntity);

                if (contact.InvitedToPortal)
                {
                    await AssignPortalRoleAndDepartment(contact, contactId);
                }

                return contact;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<GetContactById> GetContactById(string contactId)
        {
                var contactEntity = _crmService.GetById(EntityNames.Contact,
                        ContactDto.GetContactColumns(), Guid.Parse(contactId), "contactid");

                if (contactEntity.Attributes == null || contactEntity.Attributes.Count == 0)
                    throw new UserFriendlyException("ContactDoesNotExist", HttpStatusCode.BadRequest);

                var contact = MapContactToContactDto(contactEntity);

                return contact;
        }

        private GetContactById MapToContactDto(Entity contactEntity)
        {
            var companyId = CRMOperations.GetValueByAttrNameAlised(contactEntity, "Membership.ntw_companyname");
            var companyName = CRMOperations.GetNameValueByAttrNameAlised(contactEntity, "Membership.ntw_companyname");
            return new GetContactById
            {
                FirstName = CRMOperations.GetValueByAttributeName<string>(contactEntity, "firstname"),
                FirstNameAr = CRMOperations.GetValueByAttributeName<string>(contactEntity, "ntw_firstnamearabic"),
                LastName = CRMOperations.GetValueByAttributeName<string>(contactEntity, "lastname"),
                LastNameAr = CRMOperations.GetValueByAttributeName<string>(contactEntity, "ntw_lastnamearabic"),
                Email = CRMOperations.GetValueByAttributeName<string>(contactEntity, "emailaddress1"),
                MobilePhone = CRMOperations.GetValueByAttributeName<string>(contactEntity, "mobilephone"),
                InvitedToPortal = CRMOperations.GetValueByAttributeName<bool>(contactEntity, "hexa_invitetoportal"),
                Company = new EntityReferenceDto()
                {
                    Id = string.IsNullOrEmpty(companyId) ? CRMOperations.GetValueByAttrNameAlised(contactEntity, "Experience.ntw_companyname") : companyId,
                    Name = string.IsNullOrEmpty(companyName) ? CRMOperations.GetNameValueByAttrNameAlised(contactEntity, "Experience.ntw_companyname") : companyName,
                },
                Country = CRMOperations.GetValueByAttributeName<EntityReferenceDto>(contactEntity, "pwc_countryid", "Country.ntw_arabicname"),
                Position = CRMOperations.GetValueByAttributeName<EntityReferenceDto>(contactEntity, "pwc_position", "Position.ntw_namear"),
                Nationality = CRMOperations.GetValueByAttributeName<EntityOptionSetDto>(contactEntity, "ntw_nationalityset"),
            };
        }

        private GetContactById MapContactToContactDto(Entity contactEntity)
        {
            return new GetContactById
            {
                FirstName = CRMOperations.GetValueByAttributeName<string>(contactEntity, "firstname"),
                FirstNameAr = CRMOperations.GetValueByAttributeName<string>(contactEntity, "ntw_firstnamearabic"),
                LastName = CRMOperations.GetValueByAttributeName<string>(contactEntity, "lastname"),
                LastNameAr = CRMOperations.GetValueByAttributeName<string>(contactEntity, "ntw_lastnamearabic"),
                Email = CRMOperations.GetValueByAttributeName<string>(contactEntity, "emailaddress1"),
                MobilePhone = CRMOperations.GetValueByAttributeName<string>(contactEntity, "mobilephone"),
            };
        }

        private Entity GetContactQueryExpression(string contactId)
        {
            var companyId = _sessionService.GetCompanyId();
            var contactCols = new ColumnSet("contactid", "firstname", "lastname", "ntw_firstnamearabic", "ntw_lastnamearabic", "pwc_position", "pwc_countryid", "emailaddress1", "ntw_nationalityset", "hexa_invitetoportal", "mobilephone");

            QueryExpression Contact_MembershipQuery = new QueryExpression(EntityNames.Contact)
            {
                ColumnSet = contactCols
            };
            QueryExpression Contact_ExperienceQuery = new QueryExpression(EntityNames.Contact)
            {
                ColumnSet = contactCols,
            };

            Contact_MembershipQuery.Criteria.AddCondition("contactid", ConditionOperator.Equal, contactId);
            Contact_ExperienceQuery.Criteria.AddCondition("contactid", ConditionOperator.Equal, contactId);

            var Membershiplink = new LinkEntity
            {
                LinkFromEntityName = EntityNames.Contact,
                LinkFromAttributeName = "contactid",
                LinkToEntityName = EntityNames.Membership,
                LinkToAttributeName = "ntw_contactname",
                JoinOperator = JoinOperator.Inner,
                Columns = new ColumnSet("ntw_membershipid", "ntw_companyname"),
                EntityAlias = "Membership",
            };
            var Experiencelink = new LinkEntity
            {
                LinkFromEntityName = EntityNames.Contact,
                LinkFromAttributeName = "contactid",
                LinkToEntityName = EntityNames.Experience,
                LinkToAttributeName = "ntw_contactname",
                JoinOperator = JoinOperator.Inner,
                Columns = new ColumnSet("ntw_experienceid", "ntw_companyname"),
                EntityAlias = "Experience"
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
            var includeEFilter = new FilterExpression();
            includeEFilter.AddCondition(new ConditionExpression("ntw_companyname", ConditionOperator.Equal, new Guid(companyId)));
            Membershiplink.LinkCriteria = includeEFilter;
            Experiencelink.LinkCriteria = includeEFilter;


            Contact_MembershipQuery.LinkEntities.Add(Membershiplink);
            Contact_ExperienceQuery.LinkEntities.Add(Experiencelink);
            Contact_MembershipQuery.LinkEntities.Add(Countrylink);
            Contact_ExperienceQuery.LinkEntities.Add(Countrylink);
            Contact_MembershipQuery.LinkEntities.Add(Positionlink);
            Contact_ExperienceQuery.LinkEntities.Add(Positionlink);

            var contacsWithMembership = new EntityCollection();
            var contacsWithExperience = new EntityCollection();
            Parallel.Invoke(
              () => contacsWithMembership = _crmService.GetInstance().RetrieveMultiple(Contact_MembershipQuery),
              () => contacsWithExperience = _crmService.GetInstance().RetrieveMultiple(Contact_ExperienceQuery)
              );

            var combinedEntities = contacsWithMembership.Entities.Concat(contacsWithExperience.Entities);

            return combinedEntities.FirstOrDefault();
        }

        private async Task AssignPortalRoleAndDepartment(GetContactById contact, string contactId)
        {
            var roles = await _roleService.GetContactRolesByContactId(contactId);

            if (roles?.Count > 0 && contact.Company != null)
            {
                var currentRole = roles.FirstOrDefault(role => role.Company.Id == contact.Company.Id);
                if (currentRole == null) return;

                var cachedItem = await _accessManagementCacheManager.GetAccessManagementCacheItem();
                var portalRole = cachedItem.PortalRolesList.FirstOrDefault(role => role.Id == currentRole.PortalRole.Id);
                if (portalRole == null) return;

                contact.PortalRole = new EntityReferenceDto
                {
                    Id = portalRole.ParentportalRole != null ? portalRole.ParentportalRole.Id : portalRole.Id,
                    Name = portalRole.ParentportalRole != null ? portalRole.ParentportalRole.Name : portalRole.Name,
                };

                if (portalRole.Department != null)
                {
                    contact.Department = new EntityReferenceDto
                    {
                        Id = portalRole.Department.Id,
                        Name = portalRole.Department.Name
                    };
                }
            }
        }

        public static string[] GetContactRoleAssocColumns()
        {
            string[] contactColumns = new string[] { "hexa_portalroleid" };
            return contactColumns;
        }

        public async Task<string> Update(ContactUpdateDto contactDto)
        {
            try
            {
                var orgService = _crmService.GetInstance();
                var transactionRequest = new ExecuteTransactionRequest()
                {
                    Requests = new OrganizationRequestCollection()
                };

                var contactEntity = PrepareContactEntity(contactDto);
                transactionRequest.Requests.Add(new UpdateRequest { Target = contactEntity });

                if (!string.IsNullOrEmpty(contactDto.PortalRole))
                {
                    var roleUpdateRequest = await PrepareRoleUpdateRequest(contactDto);
                    if (roleUpdateRequest != null)
                    {
                        transactionRequest.Requests.Add(roleUpdateRequest);
                    }
                }

                var response = orgService.Execute(transactionRequest);

                return response.Results.Values.FirstOrDefault().ToString();
            }
            catch (Exception ex)
            {
                if (ex.Message.ToLower().Contains("phone"))
                {
                    var result = _portalConfigAppService.RetrievePortalConfigurationByValue(ex.Message);
                    throw new UserFriendlyException(result.Key);
                }
                else
                    throw new UserFriendlyException("CouldNotUpdateContact");
            }
        }

        private Entity PrepareContactEntity(ContactUpdateDto contactDto)
        {
            var contactEntity = _crmService.GetById(EntityNames.Contact,
                        ContactDto.GetContactColumns(), Guid.Parse(contactDto.Id), "contactid");

            var phone = CRMOperations.GetValueByAttributeName<string>(contactEntity, "mobilephone");
            var firstNameAr = CRMOperations.GetValueByAttributeName<string>(contactEntity, "ntw_firstnamearabic");
            var lastNameAr = CRMOperations.GetValueByAttributeName<string>(contactEntity, "ntw_lastnamearabic");

            Entity objcontact = new Entity
            {
                Id = contactEntity.Id,
                LogicalName = EntityNames.Contact
            };


            if (!contactDto.MobilePhone.Equals(phone))
                objcontact["mobilephone"] = contactDto.MobilePhone;
            ValidateContactArName(contactDto, contactEntity, objcontact);

            objcontact["emailaddress1"] = contactDto.Email;
            objcontact["hexa_invitetoportal"] = contactDto.InvitedToPortal;
            objcontact["pwc_countryid"] = new EntityReference(EntityNames.Country, new Guid(contactDto.Country));
            objcontact["pwc_position"] = new EntityReference(EntityNames.Position, new Guid(contactDto.Position));
            objcontact["ntw_nationalityset"] = new OptionSetValue(contactDto.Nationality);

            return objcontact;
        }

        private void ValidateContactArName(ContactUpdateDto contactDto, Entity contactEntity, Entity objcontact)
        {
            var firstNameAr = CRMOperations.GetValueByAttributeName<string>(contactEntity, "ntw_firstnamearabic");
            var lastNameAr = CRMOperations.GetValueByAttributeName<string>(contactEntity, "ntw_lastnamearabic");

            if (!string.IsNullOrEmpty(firstNameAr) && !string.IsNullOrEmpty(contactDto.FirstNameAr) && firstNameAr != contactDto.FirstNameAr)
            {
                throw new UserFriendlyException("MsgUnexpectedError");
            }

            if (!string.IsNullOrEmpty(lastNameAr) && !string.IsNullOrEmpty(contactDto.LastNameAr) && lastNameAr != contactDto.LastNameAr)
            {
                throw new UserFriendlyException("MsgUnexpectedError");
            }

            if (string.IsNullOrEmpty(firstNameAr) && !string.IsNullOrEmpty(contactDto.FirstNameAr))
            {
                objcontact["ntw_firstnamearabic"] = contactDto.FirstNameAr;
            }

            if (string.IsNullOrEmpty(lastNameAr) && !string.IsNullOrEmpty(contactDto.LastNameAr))
            {
                objcontact["ntw_lastnamearabic"] = contactDto.LastNameAr;
            }
        }

        private async Task<UpdateRequest> PrepareRoleUpdateRequest(ContactUpdateDto contactDto)
        {
            Guid portalRole = Guid.Empty;
            var roles = await _roleService.GetContactRolesByContactId(contactDto.Id);
            var currentRole = roles.FirstOrDefault(x => x.Company.Id.Equals(contactDto.Company));

            if (string.IsNullOrEmpty(contactDto.Department))
            {
                portalRole = new Guid(contactDto.PortalRole);
            }
            else
            {
                try
                {
                    portalRole = new Guid(await _roleService.GetParentPortalRole(
                            contactDto.PortalRole, contactDto.Department));
                }
                catch (Exception)
                {
                    throw new UserFriendlyException("CouldNotUpdateRoleForContact");
                }
            }

            if (currentRole != null && !currentRole.PortalRole.Id.Equals(contactDto.PortalRole))
            {
                Entity newContactRoleAssoc = new Entity("hexa_contactroleassociation");
                newContactRoleAssoc.Id = new Guid(currentRole.Id);
                newContactRoleAssoc["hexa_portalroleid"] = new EntityReference("hexa_portalrole", portalRole);

                return new UpdateRequest { Target = newContactRoleAssoc };
            }
            return null;
        }

        public bool Delete(string contactId)
        {
            var companyId = _sessionService.GetCompanyId();
            var orgContext = new OrganizationServiceContext(_crmService.GetInstance());
            IQueryable<Entity> query = orgContext.CreateQuery(EntityNames.Experience).AsQueryable();
            query = query.Where(x => ((Guid)x["ntw_contactname"]).Equals(contactId) && ((int)x["statecode"]).Equals(0) /*Active*/);
            var experienceEntities = query.Select(x => new
            {
                Id = x.Id
            }).ToList();

            IQueryable<Entity> roleAsscociationQuery = orgContext.CreateQuery(EntityNames.ContactAssociation).AsQueryable();
            roleAsscociationQuery = roleAsscociationQuery.Where(x => ((Guid)x["hexa_contactid"]).Equals(contactId) && ((Guid)x["hexa_companyid"]).Equals(companyId));
            var associations = roleAsscociationQuery.Select(x => new
            {
                Id = x.Id
            }).ToList();

            if (experienceEntities.Any() || associations.Any())
            {
                var service = _crmService.GetInstance();
                var executeMultipleRequest = new ExecuteMultipleRequest()
                {
                    Settings = new ExecuteMultipleSettings()
                    {
                        ContinueOnError = false,
                        ReturnResponses = true
                    },
                    Requests = new OrganizationRequestCollection()
                };

                foreach (var experience in experienceEntities)
                {
                    var updateRequest = new UpdateRequest()
                    {
                        Target = new Entity(EntityNames.Experience)
                        {
                            Id = experience.Id,
                            ["statecode"] = new OptionSetValue(1) // Set statecode to 1 InActive
                        }
                    };

                    executeMultipleRequest.Requests.Add(updateRequest);
                }
                foreach (var association in associations)
                {
                    var updateRequest = new UpdateRequest()
                    {
                        Target = new Entity(EntityNames.ContactAssociation)
                        {
                            Id = association.Id,
                            ["hexa_associationstatustypecode"] = new OptionSetValue((int)PortalAssociationStatus.Deleted)
                        }
                    };

                    executeMultipleRequest.Requests.Add(updateRequest);
                }

                var executeMultipleResponse = (ExecuteMultipleResponse)service.Execute(executeMultipleRequest);
                if (executeMultipleResponse.Responses.Where(z => z.Fault != null).Select(x => x.Fault).Any())
                {
                    throw new UserFriendlyException("ErrorInDeletingContact");
                }
            }
            else
            {
                throw new UserFriendlyException("ErrorInDeletingContactNoExperience");
            }
            return true;
        }

        public void UpdatePrimaryEmail(string contactId, string email)
        {
            string contactEntityName = EntityNames.Contact;
            string[] contactColumns = new string[] { "contactid", "emailaddress1" };
            var contactEntity = new Entity(contactEntityName);
            contactEntity["contactid"] = new Guid(contactId);
            contactEntity["emailaddress1"] = email;
            _crmService.Update(contactEntity, contactEntityName);
        }

        public UserPreferenceDto GetUserPreferences(string contactId)
        {
            var oUserPreferenceDto = new UserPreferenceDto();
            string contactEntityName = EntityNames.Contact;
            var primaryId = "contactid";
            string[] columns = new string[] { primaryId, "pwc_portallanguagetypecode", "pwc_notificationspreference" };
            var contactEntity = _crmService.GetById(contactEntityName, columns, Guid.Parse(contactId), primaryId);
            var contact = ContactDto.ConvertToContactDto(contactEntity);
            oUserPreferenceDto.NotificationsPreference = contact.NotificationsPreference;
            oUserPreferenceDto.UserLanguage = contact.UserLanguage;
            return oUserPreferenceDto;

        }
      
        public void UpdateUserPreferences(string contactId, UpdateUserPreferenceDto oUpdateUserPreferenceDto)
        {
            string contactEntityName = EntityNames.Contact;
            var primaryId = "contactid";
            string[] columns = new string[] { primaryId, "pwc_portallanguagetypecode", "pwc_notificationspreference" };
            var contactEntity = _crmService.GetById(contactEntityName, columns, Guid.Parse(contactId), primaryId);
            contactEntity[primaryId] = new Guid(contactId);
            if (oUpdateUserPreferenceDto.UserLanguage == (int)PortalLanguage.Arabic || oUpdateUserPreferenceDto.UserLanguage == (int)PortalLanguage.English)
            {
                contactEntity["pwc_portallanguagetypecode"] = new OptionSetValue(Convert.ToInt32(oUpdateUserPreferenceDto.UserLanguage));
            }
            if (oUpdateUserPreferenceDto.NotificationPreferences != null)
            {
                var optionSetValues = oUpdateUserPreferenceDto.NotificationPreferences
                                                              .Select(preference => new OptionSetValue(Convert.ToInt32(preference)))
                                                              .ToList();
                contactEntity["pwc_notificationspreference"] = new OptionSetValueCollection(optionSetValues);
            }

            _crmService.Update(contactEntity, contactEntityName);

        }

        public async Task<bool> PinContact(Guid pinContactId, bool isPin)
        {
            var orgContext = new OrganizationServiceContext(_crmService.GetInstance());
            var contactId = _sessionService.GetContactId();
            var companyId = _sessionService.GetCompanyId();
            var configs = _portalConfigAppService.RetrievePortalConfiguration(new List<string> { PortalConfigurations.MaxPInContact });
            var pinNumber = Convert.ToInt32(configs.Single(a => a.Key == PortalConfigurations.MaxPInContact).Value);
            var result = GetPortalPins(contactId, companyId, orgContext);

            if (result.Count == pinNumber && isPin)
            {
                throw new UserFriendlyException("TheUserShouldBeAbleToPinUpToItems", pinNumber.ToString());
            }

            var oPortalPin = result.FirstOrDefault(x => x.ContactId != null && x.ContactId.Id == pinContactId.ToString());
            if (isPin && oPortalPin == null)
            {

                Entity PortalPinEntity = new Entity(EntityNames.PortalPinned);

                PortalPinEntity["pwc_name"] = contactId;
                PortalPinEntity["pwc_useridid"] = new EntityReference(EntityNames.Contact, new Guid(contactId));
                PortalPinEntity["pwc_companyidid"] = new EntityReference(EntityNames.Account, new Guid(companyId));
                PortalPinEntity["pwc_contactid"] = new EntityReference(EntityNames.Contact, pinContactId);
                orgContext.AddObject(PortalPinEntity);

                orgContext.SaveChanges();
            }
            else if (!isPin && oPortalPin != null)
            {
                //delete
                _crmService.Delete(oPortalPin.Id.ToString(), EntityNames.PortalPinned);
            }
            else
            {
                throw new UserFriendlyException("TheItemAlreadyPinned");
            }
            return true;
        }

        public ContactImageDto RetrieveContactImageById(Guid contactId)
        {
            string[] columns = new string[] { "contactid", "firstname", "lastname", "ntw_firstnamearabic", "ntw_lastnamearabic", "entityimage" };
            var entity = _crmService.GetById(EntityNames.Contact, columns, contactId, "contactid");
            Guard.AssertArgumentNotNull(entity);
            var oContactImageDto = new ContactImageDto
            {
                Id = entity.Id.ToString(),
                Name = $"{CRMUtility.GetAttributeValue(entity, "firstname", string.Empty)} {CRMUtility.GetAttributeValue(entity, "lastname", string.Empty)}".Trim(),
                NameAr = $"{CRMUtility.GetAttributeValue(entity, "ntw_firstnamearabic", string.Empty)} {CRMUtility.GetAttributeValue(entity, "ntw_lastnamearabic", string.Empty)}".Trim(),
                EntityImage = CRMUtility.GetAttributeValue<byte[]>(entity, "entityimage")
            };
            return oContactImageDto;
        }
        public async Task<bool> ChangeEmailByContactId(Guid contactId, string newEmail)
        {
            var content = new StringContent(
                JsonConvert.SerializeObject(new UpdateEmailRequest()
                {
                    ContactId = contactId.ToString(),
                    NewEmail = newEmail,
                }),
                Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(new HttpMethod("PATCH"), $"{_serverUrl}/change-email")
            {
                Content = content
            };
            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
                throw new UserFriendlyException("ErrorFromAuthenticationLayer", System.Net.HttpStatusCode.InternalServerError);
            return true;
        }

        private List<PortalPinDto> GetPortalPins(string contactId, string companyId, OrganizationServiceContext orgContext)
        {
            IQueryable<Entity> Query = orgContext.CreateQuery(EntityNames.PortalPinned).AsQueryable();
            Query = Query.Where(x => ((Guid)x["pwc_useridid"]).Equals(contactId) && ((Guid)x["pwc_companyidid"]).Equals(companyId));
            var selectQuery = Query.Select(x => new PortalPinDto
            {
                Id = x.Id,
                UserId = CRMOperations.GetValueByAttributeName<EntityReferenceDto>(x, "pwc_useridid", ""),
                CompanyId = CRMOperations.GetValueByAttributeName<EntityReferenceDto>(x, "pwc_companyidid", ""),
                ContactId = CRMOperations.GetValueByAttributeName<EntityReferenceDto>(x, "pwc_contactid", "")
            }).ToList();
            return selectQuery;
        }

        private (List<ContactListResponse>, int) GetContactsHavingMembershipOrExperience(ContactListReq oContactListReq, IEnumerable<string> pinnedContacts)
        {
            var companyId = _sessionService.GetCompanyId();
            List<ContactListResponse> contacts = new List<ContactListResponse>();

            var cols = new ColumnSet(
                "contactid", "firstname", "lastname", "ntw_firstnamearabic", "ntw_lastnamearabic",
                "pwc_position", "pwc_countryid", "emailaddress1", "mobilephone", "pwc_city", "entityimage"
            );

            QueryExpression query = new QueryExpression(EntityNames.Contact)
            {
                ColumnSet = cols,
            };

            // Membership Link – to exclude contacts with any membership
            var membershipLink = new LinkEntity
            {
                LinkFromEntityName = EntityNames.Contact,
                LinkFromAttributeName = "contactid",
                LinkToEntityName = EntityNames.Membership,
                LinkToAttributeName = "ntw_contactname",
                JoinOperator = JoinOperator.LeftOuter,
                Columns = new ColumnSet("ntw_membershipid", "ntw_companyname"),
                EntityAlias = "Membership"
            };

            var experienceLink = new LinkEntity
            {
                LinkFromEntityName = EntityNames.Contact,
                LinkFromAttributeName = "contactid",
                LinkToEntityName = EntityNames.Experience,
                LinkToAttributeName = "ntw_contactname",
                JoinOperator = JoinOperator.LeftOuter,
                Columns = new ColumnSet("ntw_experienceid", "ntw_companyname", "statecode"),
                EntityAlias = "Experience"
            };

            // Role Association Link – to include only PC Non-Board members from same company
            var roleAssociationLink = new LinkEntity
            {
                LinkFromEntityName = EntityNames.Contact,
                LinkFromAttributeName = "contactid",
                LinkToEntityName = EntityNames.ContactAssociation,
                LinkToAttributeName = "hexa_contactid",
                JoinOperator = JoinOperator.LeftOuter,
                Columns = new ColumnSet("hexa_contactroleassociationid", "hexa_portalroleid", "hexa_companyid"),
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
                               EntityAlias = "RoleTypeCode"
        }
    }
            };

            // Country Link
            var countryLink = new LinkEntity
            {
                LinkFromEntityName = EntityNames.Contact,
                LinkFromAttributeName = "pwc_countryid",
                LinkToEntityName = EntityNames.Country,
                LinkToAttributeName = "ntw_countryid",
                JoinOperator = JoinOperator.LeftOuter,
                Columns = new ColumnSet("ntw_name", "ntw_arabicname"),
                EntityAlias = "Country"
            };

            // Position Link
            var positionLink = new LinkEntity
            {
                LinkFromEntityName = EntityNames.Contact,
                LinkFromAttributeName = "pwc_position",
                LinkToEntityName = EntityNames.Position,
                LinkToAttributeName = "ntw_positionid",
                JoinOperator = JoinOperator.LeftOuter,
                Columns = new ColumnSet("ntw_name", "ntw_namear"),
                EntityAlias = "Position"
            };

            // City Link
            var cityLink = new LinkEntity
            {
                LinkFromEntityName = EntityNames.Contact,
                LinkFromAttributeName = "pwc_city",
                LinkToEntityName = EntityNames.City,
                LinkToAttributeName = "ntw_citiesid",
                JoinOperator = JoinOperator.LeftOuter,
                Columns = new ColumnSet("ntw_name", "pwc_namear"),
                EntityAlias = "City"
            };

            // Add links to query
            query.LinkEntities.Add(roleAssociationLink);
            query.LinkEntities.Add(countryLink);
            query.LinkEntities.Add(cityLink);
            query.LinkEntities.Add(positionLink);
            query.LinkEntities.Add(experienceLink);

            // Filter: Exclude contacts with membership
            membershipLink.LinkCriteria = new FilterExpression(LogicalOperator.And);
            membershipLink.LinkCriteria.AddCondition("ntw_companyname", ConditionOperator.Equal, new Guid(companyId));
            query.LinkEntities.Add(membershipLink);

            // Now, to EXCLUDE contacts who have any matching membership,
            // we check if the result of the LEFT OUTER JOIN is NULL (i.e., no match)
            var membershipFilter = new FilterExpression();
            membershipFilter.AddCondition("Membership", "ntw_membershipid", ConditionOperator.Null);
            query.Criteria.AddFilter(membershipFilter);
            // Filter: include contacts with experince with same company
            var experinceFilter = new FilterExpression(LogicalOperator.And);
            experinceFilter.AddCondition("Experience", "ntw_companyname", ConditionOperator.NotNull);
            experinceFilter.AddCondition("Experience", "ntw_companyname", ConditionOperator.Equal, new Guid(companyId));
            query.Criteria.AddFilter(experinceFilter);
            // Apply search filter if provided
            if (!string.IsNullOrEmpty(oContactListReq.SearchTerm))
            {
                var searchFilter = new FilterExpression(LogicalOperator.Or);
                searchFilter.AddCondition("firstname", ConditionOperator.Like, $"%{oContactListReq.SearchTerm}%");
                searchFilter.AddCondition("lastname", ConditionOperator.Like, $"%{oContactListReq.SearchTerm}%");
                searchFilter.AddCondition("ntw_firstnamearabic", ConditionOperator.Like, $"%{oContactListReq.SearchTerm}%");
                searchFilter.AddCondition("ntw_lastnamearabic", ConditionOperator.Like, $"%{oContactListReq.SearchTerm}%");
                searchFilter.AddCondition("emailaddress1", ConditionOperator.Like, $"%{oContactListReq.SearchTerm}%");
                searchFilter.AddCondition("mobilephone", ConditionOperator.Like, $"%{oContactListReq.SearchTerm}%");
                searchFilter.AddCondition("Country", "ntw_name", ConditionOperator.Like, $"%{oContactListReq.SearchTerm}%");
                searchFilter.AddCondition("Country", "ntw_arabicname", ConditionOperator.Like, $"%{oContactListReq.SearchTerm}%");
                searchFilter.AddCondition("City", "ntw_name", ConditionOperator.Like, $"%{oContactListReq.SearchTerm}%");
                searchFilter.AddCondition("City", "pwc_namear", ConditionOperator.Like, $"%{oContactListReq.SearchTerm}%");
                searchFilter.AddCondition("Position", "ntw_name", ConditionOperator.Like, $"%{oContactListReq.SearchTerm}%");
                searchFilter.AddCondition("Position", "ntw_namear", ConditionOperator.Like, $"%{oContactListReq.SearchTerm}%");
                query.Criteria.AddFilter(searchFilter);
            }

            // Filter out pinned contacts if any
            if (pinnedContacts != null && pinnedContacts.Any())
            {
                query.Criteria.AddCondition("contactid", ConditionOperator.NotIn, pinnedContacts.ToArray());
            }

            // Sorting

            ApplySorting(oContactListReq, query, null, null);

            // Execute query
            EntityCollection result = _crmService.GetInstance().RetrieveMultiple(query);
            contacts.AddRange(MapContactListToDto(result.Entities.ToList()));

            // Pagination
            var distinctContacts = contacts
                .GroupBy(e => e?.Id)
                .Select(g => g.First())
                .ToList();

            var paginatedContacts = distinctContacts
                .Skip((oContactListReq.PagingRequest.PageNo - 1) * oContactListReq.PagingRequest.PageSize)
                .Take(oContactListReq.PagingRequest.PageSize)
                .ToList();

            int totalCount = distinctContacts.Count;
            if (pinnedContacts.Any())
            {
                totalCount += pinnedContacts.Count();
            }

            return (paginatedContacts, totalCount);


        }

        private void ApplySorting(ContactListReq oContactListReq, QueryExpression query, LinkEntity membershipLink, LinkEntity experienceLink)
        {
            if (!string.IsNullOrEmpty(oContactListReq.PagingRequest.SortField))
            {
                switch (oContactListReq.PagingRequest.SortField.ToLower())
                {
                    case "firstname":
                        oContactListReq.PagingRequest.SortField = "firstname";
                        query.AddOrder(oContactListReq.PagingRequest.SortField, (OrderType)oContactListReq.PagingRequest.SortOrder);
                        break;
                    case "lastname":
                        oContactListReq.PagingRequest.SortField = "lastname";
                        query.AddOrder(oContactListReq.PagingRequest.SortField, (OrderType)oContactListReq.PagingRequest.SortOrder);
                        break;
                    case "email":
                        oContactListReq.PagingRequest.SortField = "emailaddress1";
                        query.AddOrder(oContactListReq.PagingRequest.SortField, (OrderType)oContactListReq.PagingRequest.SortOrder);
                        break;
                    case "position":
                        oContactListReq.PagingRequest.SortField = "pwc_position";
                        query.AddOrder(oContactListReq.PagingRequest.SortField, (OrderType)oContactListReq.PagingRequest.SortOrder);
                        break;
                    case "country":
                        oContactListReq.PagingRequest.SortField = "pwc_countryid";
                        query.AddOrder(oContactListReq.PagingRequest.SortField, (OrderType)oContactListReq.PagingRequest.SortOrder);
                        break;
                    case "company":
                        oContactListReq.PagingRequest.SortField = "ntw_companyname";
                        CRMOperations.AddOrderToLinkEntity(membershipLink, oContactListReq.PagingRequest);
                        CRMOperations.AddOrderToLinkEntity(experienceLink, oContactListReq.PagingRequest);
                        break;
                    default:
                        break;
                }
            }
        }

        private List<ContactListResponse> MapContactListToDto(List<Entity> entities)
        {
            var isAdmin = _userPermissionAppService.IsLoggedInUserIsAdmin();

            return entities.Select(entity =>
            {
                var contactId = CRMOperations.GetValueByAttributeName<Guid>(entity, "contactid");
                var company = CRMOperations.GetValueByAttrNameAlised(entity, "Membership.ntw_companyname");
                var experienceId = CRMOperations.GetValueByAttributeName<AliasedValue>(entity, "Experience.ntw_experienceid")?.Value.ToString() ?? string.Empty;
                var membershipId = CRMOperations.GetValueByAttributeName<AliasedValue>(entity, "Membership.ntw_membershipid")?.Value.ToString() ?? string.Empty;
                var hideAction = false;              
                                   
                if (isAdmin)
                {
                    var role = (EntityReference)CRMOperations.GetValueByAttributeName<AliasedValue>(entity, "RoleAssociation.hexa_portalroleid")?.Value;
                    if (role != null)
                    {
                        var ispermission = _userPermissionAppService.CheckReadUserPermissions(contactId, role.Id);
                        hideAction = !ispermission;
                    }
                }

                if (!string.IsNullOrEmpty(membershipId))
                {
                    hideAction = true;
                }

                return new ContactListResponse
                {
                    Id = contactId,
                    FirstName = CRMOperations.GetValueByAttributeName<string>(entity, "firstname"),
                    LastName = CRMOperations.GetValueByAttributeName<string>(entity, "lastname"),
                    FirstNameAr = CRMOperations.GetValueByAttributeName<string>(entity, "ntw_firstnamearabic"),
                    LastNameAr = CRMOperations.GetValueByAttributeName<string>(entity, "ntw_lastnamearabic"),
                    Email = CRMOperations.GetValueByAttributeName<string>(entity, "emailaddress1"),
                    MobilePhone = CRMOperations.GetValueByAttributeName<string>(entity, "mobilephone"),
                    Country = CRMOperations.GetValueByAttributeName<EntityReferenceDto>(entity, "pwc_countryid", "Country.ntw_arabicname"),
                    City = CRMOperations.GetValueByAttributeName<EntityReferenceDto>(entity, "pwc_city", "City.pwc_namear"),
                    Position = CRMOperations.GetValueByAttributeName<EntityReferenceDto>(entity, "pwc_position", "Position.ntw_namear"),
                    CompanyId = string.IsNullOrEmpty(company) ? CRMOperations.GetValueByAttrNameAlised(entity, "Experience.ntw_companyname") : company,
                    Entityimage = CRMOperations.GetValueByAttributeName<byte[]>(entity, "entityimage"),
                    IsDeletable = !string.IsNullOrEmpty(experienceId),
                    HideActions = hideAction
                };
            }).ToList();
        }


        private List<ContactListResponse> GetPinnedContacts(ContactListReq oContactListReq)
        {
            var contactId = _sessionService.GetContactId();
            var companyId = _sessionService.GetCompanyId();
            // Create the QueryExpression
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
                        LinkCriteria =
                        {
                            Conditions =
                            {
                                new ConditionExpression("pwc_companyidid", ConditionOperator.Equal, companyId),
                                new ConditionExpression("pwc_useridid", ConditionOperator.Equal, contactId)
                            }
                        }
                    }


                }
            };
            if (!string.IsNullOrEmpty(oContactListReq.SearchTerm))
            {
                var includeEFilter = new FilterExpression(LogicalOperator.Or);
                includeEFilter.AddCondition(new ConditionExpression("firstname", ConditionOperator.Like, $"%{oContactListReq.SearchTerm}%"));
                includeEFilter.AddCondition(new ConditionExpression("lastname", ConditionOperator.Like, $"%{oContactListReq.SearchTerm}%"));
                includeEFilter.AddCondition(new ConditionExpression("ntw_firstnamearabic", ConditionOperator.Like, $"%{oContactListReq.SearchTerm}%"));
                includeEFilter.AddCondition(new ConditionExpression("ntw_lastnamearabic", ConditionOperator.Like, $"%{oContactListReq.SearchTerm}%"));
                includeEFilter.AddCondition(new ConditionExpression("emailaddress1", ConditionOperator.Like, $"%{oContactListReq.SearchTerm}%"));
                includeEFilter.AddCondition(new ConditionExpression("mobilephone", ConditionOperator.Like, $"%{oContactListReq.SearchTerm}%"));
                includeEFilter.AddCondition(new ConditionExpression("Country", "ntw_name", ConditionOperator.Like, $"%{oContactListReq.SearchTerm}%"));
                includeEFilter.AddCondition(new ConditionExpression("Country", "ntw_arabicname", ConditionOperator.Like, $"%{oContactListReq.SearchTerm}%"));
                includeEFilter.AddCondition(new ConditionExpression("City", "ntw_name", ConditionOperator.Like, $"%{oContactListReq.SearchTerm}%"));
                includeEFilter.AddCondition(new ConditionExpression("City", "pwc_namear", ConditionOperator.Like, $"%{oContactListReq.SearchTerm}%"));
                includeEFilter.AddCondition(new ConditionExpression("Position", "ntw_name", ConditionOperator.Like, $"%{oContactListReq.SearchTerm}%"));
                includeEFilter.AddCondition(new ConditionExpression("Position", "ntw_namear", ConditionOperator.Like, $"%{oContactListReq.SearchTerm}%"));
                query.Criteria.AddFilter(includeEFilter);
            }

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
      
        private Dictionary<string, string> GetFieldsMapping()
        {
            var mappedColmnsDics = new Dictionary<string, string>
            {
                { "firstname", "FirstName" },
                { "lastname", "LastName" },
                { "emailaddress1", "Email" },
                { "pwc_position", "Company" },
                { "pwc_position", "Position" },
                { "pwc_countryid", "Country" },
            };
            return mappedColmnsDics;

        }

    public  PortalPermissionDto GetContactPortalPermission()
    {
        var parentRole =  _roleService.GetContactParentRole(_sessionService.GetContactId()?? "930cd429-28c3-f011-a4de-005056992b12", _sessionService.GetCompanyId() ?? "3375f62d-f3c2-f011-a4de-005056992b12");


        PortalPermissionType roleType = PortalPermissionType.None;

        string adminId = ConfigurationManager.AppSettings["PortalAdminPermssion"];
        string contributorId = ConfigurationManager.AppSettings["PortalContributorPermssion"];
        string viewerId = ConfigurationManager.AppSettings["PortalViewerPermssion"];


        if (string.Equals(parentRole, adminId, StringComparison.OrdinalIgnoreCase))
        {
            roleType = PortalPermissionType.Admin;
        }
        else if (string.Equals(parentRole, contributorId, StringComparison.OrdinalIgnoreCase))
        {
            roleType = PortalPermissionType.Contributor;
        }
        else if (string.Equals(parentRole, viewerId, StringComparison.OrdinalIgnoreCase))
        {
            roleType = PortalPermissionType.Viewer;
        }

        return new PortalPermissionDto
        {
            PortalRole = roleType,
            PortalRoleId = Guid.Parse(parentRole),
            PortalRoleName = roleType.ToString(),
        };
    }
}
}
