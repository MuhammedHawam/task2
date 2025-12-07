using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Query;
using Newtonsoft.Json;
using PIF.EBP.Application.Dashboards.DTOs;
using PIF.EBP.Application.EntitiesCache;
using PIF.EBP.Application.EntitiesCache.Implementation;
using PIF.EBP.Application.MetaData;
using PIF.EBP.Application.MetaData.DTOs;
using PIF.EBP.Application.Otp;
using PIF.EBP.Application.PerformanceDashboard.DTOs;
using PIF.EBP.Application.PortalConfiguration;
using PIF.EBP.Application.Settings.DTOs;
using PIF.EBP.Application.Shared;
using PIF.EBP.Application.Shared.Helpers;
using PIF.EBP.Core.CRM;
using PIF.EBP.Core.Exceptions;
using PIF.EBP.Core.Session;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using static PIF.EBP.Application.Shared.Enums;

namespace PIF.EBP.Application.Settings.Implementation
{
    public class UserProfileAppService : IUserProfileAppService
    {
        private readonly ICrmService _crmService;
        private readonly ISessionService _sessionService;
        private Guid _contactId;
        private Guid _companyId;
        private readonly IOtpService _otpService;
        private readonly string _serverUrl;
        private readonly HttpClient _httpClient;
        private readonly IEntitiesCacheAppService _entitiesCacheAppService;

        public UserProfileAppService(ICrmService crmService,
            ISessionService sessionService,
            IOtpService otpService,
            IEntitiesCacheAppService entitiesCacheAppService)
        {
            _crmService = crmService;
            _sessionService = sessionService;
            _otpService = otpService;
            _entitiesCacheAppService = entitiesCacheAppService;
            _serverUrl = ConfigurationManager.AppSettings["IdentityServerUrl"];
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri(_serverUrl);
            _httpClient.DefaultRequestHeaders.Add(
                               ConfigurationManager.AppSettings["IdentityServerAPIKeyName"],
                                              ConfigurationManager.AppSettings["IdentityServerAPIKeyValue"]);
        }

        public ContactDetails RetrieveUserDetails()
        {
            _contactId = new Guid(_sessionService.GetContactId());
            var contactEntity = _crmService.GetAll(EntityNames.Contact, new[] { "entityimage", "firstname", "lastname", "ntw_firstnamearabic", "ntw_lastnamearabic" }, _contactId, "contactid").Entities.FirstOrDefault();
            return new ContactDetails
            {
                ContactImage = CRMOperations.GetValueByAttributeName<byte[]>(contactEntity, "entityimage"),
                FirstName = CRMOperations.GetValueByAttributeName<string>(contactEntity, "firstname"),
                LastName = CRMOperations.GetValueByAttributeName<string>(contactEntity, "lastname"),
                FirstNameAr = CRMOperations.GetValueByAttributeName<string>(contactEntity, "ntw_firstnamearabic"),
                LastNameAr = CRMOperations.GetValueByAttributeName<string>(contactEntity, "ntw_lastnamearabic")
            };
        }

        public UserProfileData RetrieveUserProfileData()
        {
            _contactId = new Guid(_sessionService.GetContactId());
            var userProfileData = new UserProfileData();
            var orgContext = new OrganizationServiceContext(_crmService.GetInstance());

            IQueryable<Entity> requestQuery = orgContext.CreateQuery(EntityNames.Contact).AsQueryable();

            if (!(_contactId == null || _contactId == Guid.Empty))
            {
                requestQuery = requestQuery.Where(x => ((Guid)x["contactid"]).Equals(_contactId));
            }
            else
            {
                throw new UserFriendlyException("InvalidContactIdValue", System.Net.HttpStatusCode.BadRequest);
            }

            userProfileData = requestQuery.Select(entity => FillUserProfileData(entity)).FirstOrDefault();
            userProfileData.Educations = GetUserProfileEducationData(_contactId);

            return userProfileData;
        }

        public string UpdateUserProfileImage(UserProfileImageDto userProfileImageDto)
        {
            try
            {
                _contactId = new Guid(_sessionService.GetContactId());

                var entity = new Entity(EntityNames.Contact, _contactId);

                entity["entityimage"] = Convert.FromBase64String(userProfileImageDto.Image);

                _crmService.Update(entity, EntityNames.Contact);

                return "Profile image updated successfully";
            }
            catch (Exception)
            {
                throw new UserFriendlyException("CouldNotUpdateProfileImage");
            }

        }

        public async Task<string> UpdateUserProfile(UserPorfileDto userPorfileDto)
        {
            _contactId = new Guid(_sessionService.GetContactId());
            bool IsEmailUpdated = !string.IsNullOrEmpty(userPorfileDto.Email) && IsEmailChanged(_contactId, userPorfileDto.Email);
            bool isMobileUpdate = !string.IsNullOrEmpty(userPorfileDto.Mobile) && IsMobileChanged(_contactId, userPorfileDto.Mobile);
            if (IsEmailUpdated || isMobileUpdate)
            {
                var otpDto = _otpService.GetOtpEntity(OtpType.UpdateProfile, new Guid(_sessionService.GetContactId()), null);

                if (userPorfileDto.Otp != otpDto.OTPNumber)
                {
                    throw new UserFriendlyException("OtpDoesNotMatch");
                }

                if (otpDto == null)
                {
                    throw new UserFriendlyException("CouldNotFindOtp");
                }

            }

            var entity = new Entity(EntityNames.Contact);

            entity.Id = _contactId;
            entity["contactid"] = _contactId;
            if (userPorfileDto.PositionId.HasValue)
            {
                entity["pwc_position"] = new EntityReference(EntityNames.Position, userPorfileDto.PositionId.Value);
            }
            if (userPorfileDto.CountryId.HasValue)
            {
                entity["pwc_countryid"] = new EntityReference(EntityNames.Country, userPorfileDto.CountryId.Value);
            }
            if (userPorfileDto.CityId.HasValue)
            {
                entity["pwc_city"] = new EntityReference(EntityNames.City, userPorfileDto.CityId.Value);
            }
            if (!string.IsNullOrEmpty(userPorfileDto.Mobile) && isMobileUpdate)
            {
                entity["mobilephone"] = userPorfileDto.Mobile;
            }
            if (!string.IsNullOrEmpty(userPorfileDto.WorkExperience))
            {
                entity["pwc_workexperience"] = userPorfileDto.WorkExperience;
            }
            if (userPorfileDto.NationalityId.HasValue)
            {
                entity["ntw_nationalityset"] = new OptionSetValue(userPorfileDto.NationalityId.Value);
            }

            if (IsEmailUpdated)
            {
                UserCompany userCompany = GetUserCompanyDetails();
                if (userCompany != null && !string.IsNullOrEmpty(userCompany.ValidDomain))
                {
                    if (validateNewEmail(userPorfileDto.Email, userCompany.ValidDomain))
                    {
                        entity["emailaddress1"] = userPorfileDto.Email;

                        var content = new StringContent(
                       JsonConvert.SerializeObject(new UpdateEmailRequest()
                       {
                           ContactId = _contactId.ToString(),
                           NewEmail = userPorfileDto.Email,
                       }),
                       Encoding.UTF8, "application/json");

                        var request = new HttpRequestMessage(new HttpMethod("PATCH"), $"{_serverUrl}/change-email")
                        {
                            Content = content
                        };

                        var response = await _httpClient.SendAsync(request);

                        if (!response.IsSuccessStatusCode)
                            throw new UserFriendlyException("ErrorFromAuthenticationLayer", System.Net.HttpStatusCode.InternalServerError);
                    }
                    else
                    {
                        throw new UserFriendlyException("InvalidEmailDomainValue", System.Net.HttpStatusCode.BadRequest);
                    }
                }
                else
                {
                    throw new UserFriendlyException("InvalidEmailDomainValue", System.Net.HttpStatusCode.BadRequest);
                }
            }


            _crmService.Update(entity, EntityNames.Contact);

            if (userPorfileDto.EducationDeletedIds != null && userPorfileDto.EducationDeletedIds.Count > 0)
            {
                foreach (Guid Id in userPorfileDto.EducationDeletedIds)
                {
                    if (Id != Guid.Empty)
                    {
                        _crmService.Delete(Id.ToString(), EntityNames.Education);
                    }
                }
            }

            if (userPorfileDto.Educations != null && userPorfileDto.Educations.Count > 0)
            {
                UpdateUserProfileEducation(userPorfileDto.Educations, _contactId);
            }

            return "User profile updated successfully";
        }

        private void UpdateUserProfileEducation(List<UserProfileEducationDto> Educations, Guid ContactId)
        {
            _contactId = new Guid(_sessionService.GetContactId());
            foreach (var education in Educations)
            {
                if (education.Id != Guid.Empty)
                {
                    var entity = new Entity(EntityNames.Education);

                    entity.Id = education.Id;
                    entity["ntw_contactname"] = new EntityReference(EntityNames.Contact, ContactId);
                    entity["ntw_majorid"] = new EntityReference(EntityNames.Major, education.MajorId);
                    entity["ntw_degreeid"] = new EntityReference(EntityNames.Degree, education.DegreeId);
                    entity["ntw_instituteid"] = new EntityReference(EntityNames.Institute, education.InstitutsId);
                    entity["ntw_startdate"] = education.StartDate;
                    entity["ntw_enddate"] = education.EndDate;

                    _crmService.Update(entity, EntityNames.Education);
                }
                else
                {
                    var entity = new Entity(EntityNames.Education);

                    entity["ntw_contactname"] = new EntityReference(EntityNames.Contact, _contactId);
                    entity["ntw_majorid"] = new EntityReference(EntityNames.Major, education.MajorId);
                    entity["ntw_degreeid"] = new EntityReference(EntityNames.Degree, education.DegreeId);
                    entity["ntw_instituteid"] = new EntityReference(EntityNames.Institute, education.InstitutsId);
                    entity["ntw_startdate"] = education.StartDate;
                    entity["ntw_enddate"] = education.EndDate;

                    _crmService.Create(entity, EntityNames.Education);
                }
            }
        }

        private bool IsEmailChanged(Guid ContactId, string NewEmail)
        {
            string oldEmail = string.Empty;

            var orgContext = new OrganizationServiceContext(_crmService.GetInstance());

            IQueryable<Entity> requestQuery = orgContext.CreateQuery(EntityNames.Contact).AsQueryable();
            requestQuery = requestQuery.Where(x => ((Guid)x["contactid"]).Equals(ContactId));

            var selectedFieldsQuery = requestQuery.Select(entity => FillContactData(entity)).FirstOrDefault();
            oldEmail = selectedFieldsQuery.Email;

            return NewEmail != oldEmail;
        }

        private bool IsMobileChanged(Guid ContactId, string NewMobile)
        {
            string oldMobile = string.Empty;

            var orgContext = new OrganizationServiceContext(_crmService.GetInstance());

            IQueryable<Entity> requestQuery = orgContext.CreateQuery(EntityNames.Contact).AsQueryable();
            requestQuery = requestQuery.Where(x => ((Guid)x["contactid"]).Equals(ContactId));

            var selectedFieldsQuery = requestQuery.Select(entity => FillContactData(entity)).FirstOrDefault();
            oldMobile = selectedFieldsQuery.Mobile;

            return NewMobile != oldMobile;
        }

        private bool validateNewEmail(string Email, string CompanyDomain)
        {
            var validDomains = CompanyDomain.Split(',');

            return validDomains.Any(x => Email.Contains(x));
        }

        private UserPorfileDto FillContactData(Entity entity)
        {
            return new UserPorfileDto
            {
                Email = entity.GetAttributeValue<string>("emailaddress1"),
                Mobile = entity.GetAttributeValue<string>("mobilephone")
            };
        }

        private UserProfileData FillUserProfileData(Entity entity)
        {
            _contactId = new Guid(_sessionService.GetContactId());
            UserProfileData userProfileData = new UserProfileData();

            userProfileData.FirstName = entity.GetValueByAttributeName<string>("firstname");
            userProfileData.LastName = entity.GetValueByAttributeName<string>("lastname");
            userProfileData.FirstNameAr = entity.GetValueByAttributeName<string>("ntw_firstnamearabic");
            userProfileData.LastNameAr = entity.GetValueByAttributeName<string>("ntw_lastnamearabic");
            userProfileData.Country = entity.GetValueByAttributeName<EntityReferenceDto>("pwc_countryid");
            userProfileData.City = entity.GetValueByAttributeName<EntityReferenceDto>("pwc_city");
            userProfileData.Position = entity.GetValueByAttributeName<EntityReferenceDto>("pwc_position");
            userProfileData.Nationality = _entitiesCacheAppService.RetrieveOptionSetCacheByKeyWithValue(OptionSetKey.Nationality, CRMOperations.GetValueByAttributeName<OptionSetValue>(entity, "ntw_nationalityset"));
            userProfileData.WorkExperience = entity.GetValueByAttributeName<string>("pwc_workexperience");
            userProfileData.Email = entity.GetValueByAttributeName<string>("emailaddress1");
            userProfileData.Mobile = entity.GetValueByAttributeName<string>("mobilephone");

            ColumnSet columns = new ColumnSet("entityimage");
            Entity entity11 = _crmService.GetInstance().Retrieve(EntityNames.Contact, _contactId, columns);

            if (entity11.Contains("entityimage"))
            {
                userProfileData.Image = entity11["entityimage"] as byte[];
            }

            UserCompany userCompany = GetUserCompanyDetails();
            userProfileData.UserCompany = userCompany;

            return userProfileData;
        }

        private List<UserProfileEducation> GetUserProfileEducationData(Guid ContactId)
        {
            List<UserProfileEducation> userProfileEducations = new List<UserProfileEducation>();
            var orgContext = new OrganizationServiceContext(_crmService.GetInstance());

            IQueryable<Entity> requestQuery = orgContext.CreateQuery(EntityNames.Education).AsQueryable();
            requestQuery = requestQuery.Where(x => ((Guid)x["ntw_contactname"]).Equals(ContactId));

            userProfileEducations.AddRange(requestQuery.Select(entity => FillUserProfileEducationData(entity)).ToList());

            return userProfileEducations;
        }

        private UserProfileEducation FillUserProfileEducationData(Entity entity)
        {
            return new UserProfileEducation
            {
                Id = entity.Id,
                StartDate = entity.GetValueByAttributeName<DateTime>("ntw_startdate"),
                EndDate = entity.GetValueByAttributeName<DateTime>("ntw_enddate"),
                Degree = entity.GetValueByAttributeName<EntityReferenceDto>("ntw_degreeid"),
                Instituts = entity.GetValueByAttributeName<EntityReferenceDto>("ntw_instituteid"),
                Major = entity.GetValueByAttributeName<EntityReferenceDto>("ntw_majorid")
            };
        }

        private UserCompany GetUserCompanyDetails()
        {
            _companyId = new Guid(_sessionService.GetCompanyId());
            var orgContext = new OrganizationServiceContext(_crmService.GetInstance());

            var requestQuery = new QueryExpression(EntityNames.Account)
            {
                ColumnSet = new ColumnSet("accountid", "name", "ntw_companynamearabic", "entityimage"),
                Criteria = { Conditions = { new ConditionExpression("accountid", ConditionOperator.Equal, _companyId) } }
            };

            var entityCollection = _crmService.GetInstance().RetrieveMultiple(requestQuery);

            UserCompany userCompany = entityCollection.Entities.Select(entity => FillCompanyData(entity)).FirstOrDefault();

            return userCompany;
        }

        private UserCompany FillCompanyData(Entity entity)
        {
            return new UserCompany
            {
                Name = entity.GetValueByAttributeName<string>("name"),
                NameAR = entity.GetValueByAttributeName<string>("ntw_companynamearabic"),
                ValidDomain = entity.GetValueByAttributeName<string>("pwc_validdomains"),
                Image = entity.GetValueByAttributeName<byte[]>("entityimage")
            };
        }

        public void UpdateShowTutorial(bool value)
        {
            _contactId = new Guid(_sessionService.GetContactId());

            try
            {
                var entity = new Entity(EntityNames.Contact, _contactId);

                entity["pwc_showtutorial"] = value;

                _crmService.Update(entity, EntityNames.Contact);
            }
            catch
            {
                throw new UserFriendlyException("CouldNotUpdateTutorial");
            }
        }

        public async Task<string> AddUserProfileQualification(List<UserProfileEducationDto> userProfileEducationDto)
        {
            _contactId = new Guid(_sessionService.GetContactId());

            foreach (UserProfileEducationDto educationDto in userProfileEducationDto)
            {
                var entity = new Entity(EntityNames.Education);

                entity["ntw_contactname"] = new EntityReference(EntityNames.Contact, _contactId);
                entity["ntw_majorid"] = new EntityReference(EntityNames.Major, educationDto.MajorId);
                entity["ntw_degreeid"] = new EntityReference(EntityNames.Degree, educationDto.DegreeId);
                entity["ntw_instituteid"] = new EntityReference(EntityNames.Institute, educationDto.InstitutsId);
                entity["ntw_startdate"] = educationDto.StartDate;
                entity["ntw_enddate"] = educationDto.EndDate;

                _crmService.Create(entity, EntityNames.Education);
            }

            return "User profile qualifications added successfully";
        }
    }
}
