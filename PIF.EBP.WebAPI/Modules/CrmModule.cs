using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using PIF.EBP.Core.CRM.Implementation;


namespace PIF.EBP.WebAPI.Modules
{
    public class CrmModule : IWindsorInstaller
    {
        private readonly IServiceManagement<IOrganizationService> _orgServiceManagement;
        private readonly AuthenticationCredentials _authCredentials;
        public CrmModule(IServiceManagement<IOrganizationService> orgServiceManagement,
            AuthenticationCredentials authCredentials)
        {
            _orgServiceManagement = orgServiceManagement;
            _authCredentials = authCredentials;
        }

        public void Install(IWindsorContainer container, Castle.MicroKernel.SubSystems.Configuration.IConfigurationStore configurationStore)
        {
            container.Register(Component.For<IOrganizationService>()
                                                                .UsingFactoryMethod(() => new OrganizationServiceFactory(_orgServiceManagement, _authCredentials).Create())
                                                                .LifestyleScoped());
        }
    }
}