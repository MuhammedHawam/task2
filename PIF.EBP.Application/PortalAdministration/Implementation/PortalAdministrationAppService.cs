using Microsoft.Xrm.Sdk;
using PIF.EBP.Application.Shared;
using PIF.EBP.Core.CRM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PIF.EBP.Application.AccessManagement;
using Microsoft.Xrm.Sdk.Query;
using PIF.EBP.Application.PortalAdministration.DTOs;
using PIF.EBP.Core.Session;
using Newtonsoft.Json;
using PIF.EBP.Application.Accounts.Dtos;
using PIF.EBP.Core.Shared;
using System.Net.Http;
using System.Web;
using System.Configuration;
using PIF.EBP.Core.Utilities;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;

namespace PIF.EBP.Application.PortalAdministration.Implementation
{
    public class PortalAdministrationAppService : IPortalAdministrationAppService
    {
        private readonly ICrmService _crmService;
        private readonly IAccessManagementAppService _accessManagementAppService;
        private readonly ISessionService _sessionService;
        private readonly HttpClient _httpClient;
        private readonly string _serverUrl;

        public PortalAdministrationAppService(ICrmService crmService,
            IAccessManagementAppService accessManagementAppService,
            ISessionService sessionService)
        {
            _crmService = crmService;
            _accessManagementAppService=accessManagementAppService;
            _sessionService = sessionService;
            _serverUrl = ConfigurationManager.AppSettings["IdentityServerUrl"];
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri(_serverUrl);
            _httpClient.DefaultRequestHeaders.Add(
                ConfigurationManager.AppSettings["IdentityServerAPIKeyName"],
                ConfigurationManager.AppSettings["IdentityServerAPIKeyValue"]);

        }
        public async Task<List<CompanyDto>> RetrievecompaniesByContactId()
        {
            List<CompanyDto> response = new List<CompanyDto>();
            var contactId = _sessionService.GetContactId();
            var contactRoles = await _accessManagementAppService.GetContactRolesByContactId(contactId);
            if (contactRoles == null || !contactRoles.Any())
            {
                return response;
            }
            var companyIds = contactRoles.Select(x => x.Company.Id).Distinct().ToArray();
            var query = new QueryExpression(EntityNames.Account)
            {
                ColumnSet = new ColumnSet("accountid", "name", "ntw_companynamearabic", "entityimage"),
                Criteria = { Conditions = { new ConditionExpression("accountid", ConditionOperator.In, companyIds) } }
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
                        Name=oCompanyDto.Name,
                        NameAr=oCompanyDto.NameAr,
                        EntityImage = oCompanyDto.EntityImage
                    };

                    // Add the new instance to the response list
                    response.Add(newCompanyDto);
                }
            }
            return response;
        }
        public CompanyDto RetrieveCompanyById(Guid companyId)
        {
            string[] columns = new string[] { "accountid", "name", "ntw_companynamearabic", "entityimage" };
            var entity = _crmService.GetById(EntityNames.Account, columns, companyId, "accountid");
            Guard.AssertArgumentNotNull(entity);
            var oCompanyDto = FillEntityRoles(entity);
            return oCompanyDto;
        }
        public async Task<bool> SwitchProfile(string portalRoleAssociationId)
        {
            string contactId = _sessionService.GetContactId();
            var contactRolesList = await _accessManagementAppService.GetContactRolesByContactId(contactId);

            Guard.AssertArgumentNotNull(contactRolesList);
            var contactRole = contactRolesList.First(x => x.Id==portalRoleAssociationId);
            Guard.AssertArgumentNotNull(contactRole);

            var keys = new Dictionary<string, string>
            {
                { TokenClaimsKeys.ContactId, contactId},
                { TokenClaimsKeys.RoleId, contactRole.PortalRole.Id},
                { TokenClaimsKeys.CompanyId, contactRole.Company.Id}
            };

            var model = new SwitchProfileDto
            {
                Keys = keys
            };

            var content = new StringContent(JsonConvert.SerializeObject(model),
                    System.Text.Encoding.UTF8, "application/json");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _sessionService.GetToken());
            var response = await _httpClient.PostAsync($"{_serverUrl}/switch-profile", content);
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();

                HttpContext.Current.Response.Headers.Add("Authorization", "Bearer " + responseContent);
                return true;
            }
            return false;
           
        }

        private CompanyDto FillEntityRoles(Entity entity)
        {
            return new CompanyDto
            {
                Id=entity.Id.ToString(),
                Name =  CRMUtility.GetAttributeValue(entity, "name", string.Empty),
                NameAr =  CRMUtility.GetAttributeValue(entity, "ntw_companynamearabic", string.Empty),
                EntityImage =  CRMUtility.GetAttributeValue<byte[]>(entity, "entityimage")

            };

        }
    }
}
