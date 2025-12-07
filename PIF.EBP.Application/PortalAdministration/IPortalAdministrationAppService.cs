using PIF.EBP.Application.PortalAdministration.DTOs;
using PIF.EBP.Core.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PIF.EBP.Application.PortalAdministration
{
    public interface IPortalAdministrationAppService : ITransientDependency
    {
        Task<List<CompanyDto>> RetrievecompaniesByContactId();
        CompanyDto RetrieveCompanyById(Guid companyId);
        Task<bool> SwitchProfile(string portalRoleAssociationId);
    }
}
