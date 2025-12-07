using Microsoft.Xrm.Sdk;
using Newtonsoft.Json;
using PIF.EBP.Application.AccessManagement;
using PIF.EBP.Application.Accounts.Dtos;
using PIF.EBP.Application.Contacts.Dtos;
using PIF.EBP.Application.MetaData.DTOs;
using PIF.EBP.Application.Otp;
using PIF.EBP.Application.PortalConfiguration;
using PIF.EBP.Application.Shared;
using PIF.EBP.Application.Shared.Helpers;
using PIF.EBP.Core.CRM;
using PIF.EBP.Core.Exceptions;
using PIF.EBP.Core.Helpers;
using PIF.EBP.Core.Session;
using PIF.EBP.Core.Shared;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using static PIF.EBP.Application.Shared.Enums;

namespace PIF.EBP.Application.Accounts.Implementation
{
    public class AccountAppService : IAccountAppService
    {
        private readonly ICrmService _crmService;
        private readonly string _serverUrl;
        private HttpClient _httpClient;
        private readonly IAccessManagementAppService _accessMangementService;
        private readonly IOtpService _otpService;
        private readonly IPortalConfigAppService _portalConfigAppService;
        private readonly ISessionService _sessionService;

        public AccountAppService(ICrmService crmService,
            IAccessManagementAppService accessMangementService,
            IOtpService otpService,
            IPortalConfigAppService portalConfigAppService,
            ISessionService sessionService)
        {
            _httpClient = new HttpClient();
            _crmService = crmService;
            _serverUrl = ConfigurationManager.AppSettings["IdentityServerUrl"];
            _httpClient.BaseAddress = new Uri(_serverUrl);
            _httpClient.DefaultRequestHeaders.Add(ConfigurationManager.AppSettings["IdentityServerAPIKeyName"], ConfigurationManager.AppSettings["IdentityServerAPIKeyValue"]);
            _accessMangementService = accessMangementService;
            _otpService = otpService;
            _portalConfigAppService = portalConfigAppService;
            _sessionService = sessionService;
        }

        public async Task<UserCreationResponse> CreateNewUserPassword(string encryptedInvitationId, string password)
        {
            var invitationModel = GetInvitationDtoByEncryptedId(encryptedInvitationId);

            if (Convert.ToInt32(invitationModel.Status.Value) != (int)PortalInvitationStatus.Invited)
            {
                throw new UserFriendlyException("InvalidInvitationLink");
            }

            if (invitationModel.ExpiryDate < DateTime.UtcNow)
            {
                throw new UserFriendlyException("InvitationLinkHasExpired");
            }

            var passRules = (_portalConfigAppService
                .RetrievePortalConfiguration(new List<string> { PortalConfigurations.PasswordPolicyRules })).FirstOrDefault();

            if (passRules != null)
            {
                if (!ValidatePassword(password))
                    throw new UserFriendlyException("PasswordDoesNotMatchRules");
            }

            var contactEntity = _crmService.GetById(EntityNames.Contact,
                                                ContactDto.GetContactColumns(),
                                                new Guid(invitationModel.Contact.Id),
                                                "contactid");

            if (contactEntity == null)
            {
                throw new UserFriendlyException("InvalidInvitationLink");
            }

            var contactModel = ContactDto.ConvertToContactDto(contactEntity);

            var keys = new Dictionary<string, string>();
            keys.Add(TokenClaimsKeys.ContactId, contactModel.Id.ToString());
            var roles = await _accessMangementService.GetAssociatedRoles(contactModel.Id.ToString());
            if (roles == null || roles.Count == 0)
                throw new UserFriendlyException("UserCannotAccessPortal");

            var companies = await _accessMangementService.GetCompaniesByContactId(contactModel.Id.ToString());

            keys.Add(TokenClaimsKeys.RoleId, roles.FirstOrDefault().Id);
            keys.Add(TokenClaimsKeys.CompanyId, companies.FirstOrDefault()?.Id);

            var content = new StringContent(JsonConvert.SerializeObject(
                new RegisterDto
                {
                    FirstName = contactModel.FirstName,
                    LastName = contactModel.LastName,
                    Email = contactModel.Email,
                    UserName = contactModel.Email,
                    PhoneNumber = contactModel.MobilePhone,
                    ContactId = contactModel.Id.ToString(),
                    Password = password,
                    ConfirmPassword = password,
                    Keys = keys
                }), System.Text.Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{_serverUrl}/register", content);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                HttpContext.Current.Response.Headers.Add("Authorization", "Bearer " + responseContent);
            }
            else
            {
                throw new UserFriendlyException("ErrorHappenedWhileTryingToCreateCredentials", System.Net.HttpStatusCode.InternalServerError);
            }

            UpdatePortalInvitationStatus(invitationModel.Id, PortalInvitationStatus.Registered);

            return new UserCreationResponse { IsSuccess = true };
        }

        public async Task<LoginResponse> Login(LoginDto model)
        {
            var result = new LoginResponse();
            var validations = _portalConfigAppService.RetrievePortalConfiguration(new List<string> { PortalConfigurations.TwoFactorEnabled });

            var contactEntity = _crmService.GetAll(EntityNames.Contact,
                ContactDto.GetContactColumns(), model.Email, "emailaddress1")
                .Entities.FirstOrDefault();

            if (contactEntity == null)
            {
                throw new UserFriendlyException("InvalidCredentials");
            }

            var contactModel = ContactDto.ConvertToContactDto(contactEntity);

            if (Convert.ToInt32(contactModel.PortalUserStatus.Value) == (int)PortalUserStatus.Locked)
            {
                throw new UserFriendlyException("UserStatusLocked");
            }

            model.Keys = new Dictionary<string, string>
            {
                { TokenClaimsKeys.ContactId, contactModel.Id.ToString() }
            };

            var roles = await _accessMangementService.GetAssociatedRoles(contactModel.Id.ToString());
            if (roles == null || roles.Count == 0)
                throw new UserFriendlyException("UserCannotAccessPortal");

            var companies = await _accessMangementService.GetCompaniesByContactId(contactModel.Id.ToString());

            bool.TryParse(validations.Where(a => a.Key == PortalConfigurations.TwoFactorEnabled).SingleOrDefault().Value, out bool twoFactor);

            model.GenerateToken = !twoFactor;

            model.Keys.Add(TokenClaimsKeys.RoleId, roles.FirstOrDefault().Id);
            model.Keys.Add(TokenClaimsKeys.CompanyId, companies.FirstOrDefault()?.Id);

            var content = new StringContent(JsonConvert.SerializeObject(model),
                        System.Text.Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_serverUrl}/login", content);



            result.IsSuccess = response.IsSuccessStatusCode;
            result.TwoFactorEnabled = twoFactor;
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();

                if (twoFactor)
                {
                    await _otpService.GenerateOtp(OtpType.Login, contactModel.Id);
                    result.MaskedPhone = DataMaskerHelper.MaskPhone(contactModel.MobilePhone);
                    result.UserLanguage = GetUserLanguage(contactModel.UserLanguage);
                }
                else
                {
                    HttpContext.Current.Response.Headers.Add("Authorization", "Bearer " + responseContent);
                    result.UserLanguage = GetUserLanguage(contactModel.UserLanguage);
                }
            }

            await UpdateUserLoginAttempts(contactModel, result.IsSuccess);
            return result;

        }

        private async Task UpdateUserLoginAttempts(ContactDto contactDto, bool sucess)
        {
            var validations = _portalConfigAppService.RetrievePortalConfiguration(new List<string> { PortalConfigurations.InvalidLoginAttempts });
            var invalidLoginAttempt = Convert.ToInt32(validations.Where(a => a.Key == PortalConfigurations.InvalidLoginAttempts).Single().Value);

            var contactEntity = new Entity
            {
                Id = contactDto.Id,
                LogicalName = EntityNames.Contact
            };

            if (sucess)
            {
                contactEntity["hexa_invalidloginattempts"] = 0;
                contactEntity["hexa_portaluserstatustypecode"] = new OptionSetValue((int)PortalUserStatus.Active);
                contactDto.InvalidLoginAttempts = 0;
                _crmService.Update(contactEntity, EntityNames.Contact);
                return;
            }
            else
            {
                contactEntity["hexa_invalidloginattempts"] = ++contactDto.InvalidLoginAttempts;
                _crmService.Update(contactEntity, EntityNames.Contact);
                if (contactDto.InvalidLoginAttempts >= invalidLoginAttempt)
                {
                    contactEntity["hexa_portaluserstatustypecode"] = new OptionSetValue((int)PortalUserStatus.Locked);
                    _crmService.Update(contactEntity, EntityNames.Contact);
                    await LogoutUserIfSignedIn();
                    throw new UserFriendlyException("UserStatusLocked", new { LoginAttempt = contactDto.InvalidLoginAttempts });
                }
                if (contactDto.InvalidLoginAttempts + 1 == invalidLoginAttempt)
                {
                    throw new UserFriendlyException("LastVerifyAttemptForLogin", new { LoginAttempt = contactDto.InvalidLoginAttempts });
                }
            }
        }

        private async Task UpdateOtpAttempts(ContactDto contactDto, bool sucess)
        {
            var validations = _portalConfigAppService.RetrievePortalConfiguration(new List<string> { PortalConfigurations.InvalidOTPAttempts });
            var invalidOtpAttempt = Convert.ToInt32(validations.Where(a => a.Key == PortalConfigurations.InvalidOTPAttempts).Single().Value);

            var contactEntity = new Entity
            {
                Id = contactDto.Id,
                LogicalName = EntityNames.Contact
            };

            if (sucess)
            {
                contactEntity["pwc_invalidotpattempts"] = 0;
                contactEntity["hexa_portaluserstatustypecode"] = new OptionSetValue((int)PortalUserStatus.Active);
                contactDto.InvalidOtpAttempts = 0;
                _crmService.Update(contactEntity, EntityNames.Contact);
                return;
            }
            else
            {
                contactEntity["pwc_invalidotpattempts"] = ++contactDto.InvalidOtpAttempts;
                _crmService.Update(contactEntity, EntityNames.Contact);
                if (contactDto.InvalidOtpAttempts >= invalidOtpAttempt)
                {
                    contactEntity["hexa_portaluserstatustypecode"] = new OptionSetValue((int)PortalUserStatus.Locked);
                    _crmService.Update(contactEntity, EntityNames.Contact);
                    await LogoutUserIfSignedIn();
                    throw new UserFriendlyException("UserStatusLocked", new { OtpAttempt = contactDto.InvalidOtpAttempts });
                }
                if (contactDto.InvalidOtpAttempts + 1 == invalidOtpAttempt)
                {
                    throw new UserFriendlyException("LastVerifyAttemptForOtp", new { OtpAttempt = contactDto.InvalidOtpAttempts });
                }
            }

        }

        public async Task<bool> ValidateLoginOtp(string emailaddress, int inputOtp, string password)
        {
            var contactEntity = _crmService.GetAll(EntityNames.Contact,
                ContactDto.GetContactColumns(), emailaddress, "emailaddress1")
                .Entities.FirstOrDefault();

            if (contactEntity == null)
            {
                throw new UserFriendlyException("InvalidCredentials");
            }

            var contactModel = ContactDto.ConvertToContactDto(contactEntity);

            if (Convert.ToInt32(contactModel.PortalUserStatus.Value) == (int)PortalUserStatus.Locked)
            {
                throw new UserFriendlyException("UserStatusLocked");
            }

            var otpDto = _otpService.GetOtpEntity(OtpType.Login, contactModel.Id, null);

            if (otpDto == null)
            {
                throw new UserFriendlyException("CouldNotFindOtp");
            }


            var keys = new Dictionary<string, string>
            {
                { TokenClaimsKeys.ContactId, contactModel.Id.ToString() }
            };

            var roles = await _accessMangementService.GetAssociatedRoles(contactModel.Id.ToString());
            if (roles == null || roles.Count == 0)
                throw new UserFriendlyException("UserCannotAccessPortal");

            var companies = await _accessMangementService.GetCompaniesByContactId(contactModel.Id.ToString());

            if (roles.Count == 1)
            {
                keys.Add(TokenClaimsKeys.RoleId, roles.FirstOrDefault().Id);
                keys.Add(TokenClaimsKeys.CompanyId, companies.FirstOrDefault().Id);
            }

            var model = new LoginDto
            {
                Email = emailaddress,
                Password = password,
                Keys = keys,
                GenerateToken = true
            };

            await UpdateOtpAttempts(contactModel, (otpDto.OTPNumber == inputOtp));

            if (otpDto.OTPNumber != inputOtp)
            {
                throw new UserFriendlyException("OtpDoesNotMatch");
            }
            var content = new StringContent(JsonConvert.SerializeObject(model),
                    System.Text.Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{_serverUrl}/login", content);
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();

                HttpContext.Current.Response.Headers.Add("Authorization", "Bearer " + responseContent);
            }

            return otpDto.OTPNumber == inputOtp;

        }

        public async Task<ValidatInvitationResponse> ValidateInvitationLink(string encryptedInvitationId)
        {
            var invitationModel = GetInvitationDtoByEncryptedId(encryptedInvitationId);

            if (Convert.ToInt32(invitationModel.Status.Value) == (int)PortalInvitationStatus.Expired)
            {
                throw new UserFriendlyException("InvitationLinkHasExpired");
            }
            if (Convert.ToInt32(invitationModel.Status.Value) != (int)PortalInvitationStatus.Invited)
            {
                throw new UserFriendlyException("InvalidInvitationLink");
            }

            if (invitationModel.ExpiryDate < DateTime.UtcNow)
            {
                UpdatePortalInvitationStatus(invitationModel.Id, PortalInvitationStatus.Expired);
                throw new UserFriendlyException("InvitationLinkHasExpired");
            }

            var contactEntity = _crmService.GetById(EntityNames.Contact,
                                                ContactDto.GetContactColumns(),
                                                new Guid(invitationModel.Contact.Id),
                                                "contactid");
            var contactModel = ContactDto.ConvertToContactDto(contactEntity);
            if (contactEntity == null)
            {
                throw new UserFriendlyException("InvalidInvitationLink");
            }

            await _otpService.GenerateOtp(OtpType.Invitation, new Guid(invitationModel.Contact.Id), invitationModel.Id);

            UpdateUserLanguage(invitationModel.Contact.Id);
            var firstNameAr = "";
            var lastNameAr = "";
            if (contactEntity.Contains("ntw_firstnamearabic"))
            {
                firstNameAr = contactEntity["ntw_firstnamearabic"].ToString();
            }
            if (contactEntity.Contains("ntw_lastnamearabic"))
            {
                lastNameAr = contactEntity["ntw_lastnamearabic"].ToString();
            }
            var fullNameAr = $"{firstNameAr} {lastNameAr}".Trim();
            return new ValidatInvitationResponse()
            {
                FullName = contactModel.FullName,
                FullNameAr = !string.IsNullOrWhiteSpace(fullNameAr) ? fullNameAr : null,
                MaskedEmail = DataMaskerHelper.MaskEmail(contactModel.Email),
                MaskedPhone = DataMaskerHelper.MaskPhone(contactModel.MobilePhone),
                EncryptedInvitationId = encryptedInvitationId,
                OtpSent = true
            };
        }

        private void UpdateUserLanguage(string id)
        {
            var langCode = _sessionService.GetLanguage() == "ar" ? PortalLanguage.Arabic : PortalLanguage.English;
            var contactEntity = new Entity(EntityNames.Contact);
            contactEntity["contactid"] = new Guid(id);
            contactEntity["pwc_portallanguagetypecode"] = new OptionSetValue((int)langCode);
            _crmService.Update(contactEntity, EntityNames.Contact);
        }

        private PortalInvitation GetInvitationDtoByEncryptedId(string encryptedInvitationId)
        {
            var invitationId = CryptoUtils.Decrypt(encryptedInvitationId);
            var invitationEntity = _crmService.GetById(EntityNames.PortalInvitation,
                            PortalInvitation.GetColumns(), new Guid(invitationId), "hexa_portalinvitationid");

            if (invitationEntity == null)
            {
                throw new UserFriendlyException("InvalidInvitationLink");
            }

            return PortalInvitation.ConvertToModel(invitationEntity);
        }

        public async Task<bool> ValidateInvitationOtp(string encryptedInvitationId, int inputOtp)
        {
            var invitationId = CryptoUtils.Decrypt(encryptedInvitationId);

            var otpDto = _otpService.GetOtpEntity(OtpType.Invitation, null, new Guid(invitationId));
            var contactEntity = _crmService.GetById(EntityNames.Contact, ContactDto.GetContactColumns(), new Guid(otpDto.Contact.Id), "contactid");
            var contactModel = ContactDto.ConvertToContactDto(contactEntity);
            if (inputOtp == otpDto.OTPNumber)
            {
                await UpdateOtpAttempts(contactModel, true);
                return true;
            }
            else
            {
                await UpdateOtpAttempts(contactModel, false);
                throw new UserFriendlyException("OtpDoesntMatch");
            }

        }

        public async Task<ChangePasswordResponse> ResetPassword(ResetPasswordDto model)
        {
            var configs = (_portalConfigAppService
                .RetrievePortalConfiguration(new List<string> { PortalConfigurations.PasswordPolicyRules, PortalConfigurations.LastPasswordTimesToCheck })).ToList();

            var passRules = configs.Where(x => x.Key == PortalConfigurations.PasswordPolicyRules).FirstOrDefault();

            if (passRules != null)
            {
                if (!ValidatePassword(model.NewPassword))
                    throw new UserFriendlyException("PasswordDoesNotMatchRules");
            }

            var contactEntity = _crmService.GetAll(EntityNames.Contact,
                ContactDto.GetContactColumns(), model.Email, "emailaddress1")
                .Entities.FirstOrDefault();

            if (contactEntity == null)
            {
                throw new UserFriendlyException("InvalidCredentials");
            }

            model.LastPasswordTimesToCheck = int.Parse(configs.Where(x => x.Key == PortalConfigurations.LastPasswordTimesToCheck).Select(x => x.Value).FirstOrDefault());

            var content = new StringContent(
                   JsonConvert.SerializeObject(model),
                   Encoding.UTF8, "application/json");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _sessionService.GetToken());
            var request = new HttpRequestMessage(new HttpMethod("PATCH"), $"{_serverUrl}/reset-password")
            {
                Content = content
            };

            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                UpdateUserStatusToActive(contactEntity);
                _crmService.ExecuteWorkflow(_crmService.GetInstance(), contactEntity.Id, new Guid(Consts.ResetPasswordWorkflowId));
                return new ChangePasswordResponse { IsSuccess = true };
            }
            if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                var contentResponse = await response.Content.ReadAsStringAsync();
                if (contentResponse.ToLower().Contains("used this password before"))
                    throw new UserFriendlyException("PasswordUserBefore");
            }

            throw new UserFriendlyException("InvalidCredentials");
        }

        private void UpdateUserStatusToActive(Entity contactEntity)
        {
            Entity updatedEntity = new Entity
            {
                Id = contactEntity.Id,
                LogicalName = EntityNames.Contact
            };
            updatedEntity["hexa_portaluserstatustypecode"] = new OptionSetValue((int)PortalUserStatus.Active);
            updatedEntity["pwc_invalidotpattempts"] = 0;
            updatedEntity["hexa_invalidloginattempts"] = 0;
            _crmService.Update(updatedEntity, EntityNames.Contact);
        }

        public async Task<ChangePasswordResponse> ChangePassword(ChangePasswordDto request)
        {
            var validations = _portalConfigAppService.RetrievePortalConfiguration(new List<string> { PortalConfigurations.LastPasswordTimesToCheck, PortalConfigurations.PasswordPolicyRules });
            var contactEntity = _crmService.GetAll(EntityNames.Contact,
                               ContactDto.GetContactColumns(), new Guid(_sessionService.GetContactId()), "contactid")
                .Entities.FirstOrDefault();

            var contactModel = ContactDto.ConvertToContactDto(contactEntity);

            var otpDto = _otpService.GetOtpEntity(OtpType.UpdateProfile, contactModel.Id, null);

            if (otpDto == null)
            {
                throw new UserFriendlyException("CouldNotFindOtp");
            }

            var passRules = validations.Where(x => x.Key == PortalConfigurations.PasswordPolicyRules).FirstOrDefault();

            if (passRules != null)
            {
                if (!ValidatePassword(request.NewPassword))
                    throw new UserFriendlyException("PasswordDoesNotMatchRules");
            }


            if (otpDto.OTPNumber != request.Otp)
            {
                await UpdateOtpAttempts(contactModel, false);
                throw new UserFriendlyException("OtpDoesNotMatch");
            }

            var passToCheck = int.Parse(validations
                                   .Where(x => x.Key == PortalConfigurations.LastPasswordTimesToCheck)
                                   .Select(c => c.Value).FirstOrDefault());
            var content = new StringContent(
                               JsonConvert.SerializeObject(new ChangePasswordRequest
                               {
                                   LastPasswordTimesToCheck = passToCheck,
                                   Email = contactModel.Email,
                                   CurrentPassword = request.CurrentPassword,
                                   NewPassword = request.NewPassword
                               }),
                                              Encoding.UTF8, "application/json");

            var httpRequest = new HttpRequestMessage(new HttpMethod("PATCH"), $"{_serverUrl}/change-password")
            {
                Content = content
            };

            var response = await _httpClient.SendAsync(httpRequest);



            if (!response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();

                if (responseContent.ToLower().Contains("invalid current password"))
                    throw new UserFriendlyException("IncorrectCurrentPassword", new { Error = responseContent });


                if (responseContent.ToLower().Contains("used this password before"))
                    throw new UserFriendlyException("PasswordUserBefore");

                throw new UserFriendlyException("MsgUnexpectedError");
            }

            return new ChangePasswordResponse { IsSuccess = true };
        }

        public async Task ValidateEmail(string email)
        {
            var content = new StringContent(
               JsonConvert.SerializeObject(new ValidateEmailRequest() { Email = email }),
               Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(
                new HttpMethod("PATCH"), $"{_serverUrl}/validate-email")
            { Content = content };

            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var contactEntity = _crmService.GetAll(EntityNames.Contact,
                ContactDto.GetContactColumns(), email, "emailaddress1")
                .Entities.FirstOrDefault() ?? throw new UserFriendlyException("ContactDoesNotExist");

                var contactModel = ContactDto.ConvertToContactDto(contactEntity);

                await _otpService.GenerateOtp(OtpType.ForgetPassword, contactModel.Id);

                //return new ValidateEmailResponse
                //{
                //    MaskedPhone = DataMaskerHelper.MaskPhone(contactModel.MobilePhone)
                //};
            }
        }

        public async Task Logout(string token)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add(ConfigurationManager.AppSettings["IdentityServerAPIKeyName"],
                    ConfigurationManager.AppSettings["IdentityServerAPIKeyValue"]);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var request = new HttpRequestMessage
                {
                    Method = new HttpMethod("PATCH"),
                    RequestUri = new Uri($"{_serverUrl}/logout")
                };

                var response = await client.SendAsync(request);

                try
                {
                    response.EnsureSuccessStatusCode();
                }
                catch (Exception)
                {
                    throw new UserFriendlyException("ErrorHappenedWhileTryingToLogout");
                }
            }

        }

        public async Task<bool> ValidateForgetPasswordOtp(string email, int inputOtp)
        {
            var contactEntity = _crmService.GetAll(EntityNames.Contact,
                            ContactDto.GetContactColumns(), email, "emailaddress1")
                            .Entities.FirstOrDefault();

            if (contactEntity == null)
            {
                throw new UserFriendlyException("InvalidCredentials");
            }

            var contactModel = ContactDto.ConvertToContactDto(contactEntity);


            var otpDto = _otpService.GetOtpEntity(OtpType.ForgetPassword, contactModel.Id, null);

            if (inputOtp == otpDto.OTPNumber)
            {
                await UpdateOtpAttempts(contactModel, true);

                var model = new LoginDto
                {
                    Email = email,
                };
                var content = new StringContent(JsonConvert.SerializeObject(model),
                        System.Text.Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"{_serverUrl}/get-temp-token", content);
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();

                    HttpContext.Current.Response.Headers.Add("Authorization", "Bearer " + responseContent);
                }
                return true;
            }
            else
            {
                await UpdateOtpAttempts(contactModel, false);
                throw new UserFriendlyException("OtpDoesntMatch");
            }

        }

        private async Task LogoutUserIfSignedIn()
        {
            var authHeader = HttpContext.Current.Request.Headers["Authorization"];
            if (!string.IsNullOrEmpty(authHeader))
            {
                var token = authHeader.Split(' ').Last();
                await Logout(token);
            }
        }

        private void UpdatePortalInvitationStatus(Guid Id, PortalInvitationStatus status)
        {
            var invitationEntity = new Entity(EntityNames.PortalInvitation);
            invitationEntity["hexa_portalinvitationid"] = Id;
            invitationEntity["hexa_invitationstatustypecode"] = new OptionSetValue((int)status);
            _crmService.Update(invitationEntity, EntityNames.PortalInvitation);
        }

        private bool ValidatePassword(string pass)
        {
            var hasNumber = new Regex(@"[0-9]+");
            var hasUpperChar = new Regex(@"[A-Z]+");
            var hasLowerChar = new Regex(@"[a-z]+");
            var hasMinimum8Chars = new Regex(@".{8,}");
            var hasSymbols = new Regex(@"[!@#$%^&*()_+=\[{\]};:<>|./?,-]");

            var isValidated = hasNumber.IsMatch(pass)
                && hasUpperChar.IsMatch(pass)
                && hasLowerChar.IsMatch(pass)
                && hasSymbols.IsMatch(pass)
                && hasMinimum8Chars.IsMatch(pass);
            return isValidated;
        }

        private string GetUserLanguage(EntityOptionSetDto userLanguage)
        {
            if (userLanguage != null && !string.IsNullOrEmpty(userLanguage.Value))
            {
                return userLanguage.Value;
            }
            else
            {
                return ((int)PortalLanguage.English).ToString();
            }

        }
    }

    public class LoginResponse
    {
        public bool IsSuccess { get; set; }
        public bool TwoFactorEnabled { get; set; }
        public string MaskedPhone { get; set; }
        public string UserLanguage { get; set; }
    }

    public class UserCreationResponse
    {
        public bool IsSuccess { get; set; }
        public string Token { get; set; }
    }
}
