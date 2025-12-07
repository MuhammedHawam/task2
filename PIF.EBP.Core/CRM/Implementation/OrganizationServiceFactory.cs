using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using System.Net;

namespace PIF.EBP.Core.CRM.Implementation
{
    public class OrganizationServiceFactory
    {
        private readonly IServiceManagement<IOrganizationService> _orgServiceManagement;
        private readonly AuthenticationCredentials _authCredentials;

        public OrganizationServiceFactory(IServiceManagement<IOrganizationService> orgServiceManagement, AuthenticationCredentials authCredentials)
        {
            _orgServiceManagement = orgServiceManagement;
            _authCredentials = authCredentials;
        }

        public IOrganizationService Create()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            return new OrganizationServiceProxy(_orgServiceManagement, _authCredentials.ClientCredentials);
        }

    }
}
