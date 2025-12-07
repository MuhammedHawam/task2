using PIF.EBP.Application.PerformanceDashboard.DTOs;
using PIF.EBP.Core.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PIF.EBP.Application.PerfomanceDashboard
{
    public interface IPerformanceDashboardAppService : ITransientDependency
    {
        Task<CompaniesDto> RetrieveCompanies(int pageNumber, int pageSize, string searchText = null, bool AllPIFCompanies = false);
        Task<string> GetCompanyNameById(string companyId);
        Task<CompanyOverviewDto> RetrieveCompanyOverview(Guid companyId);
        Task<CompanyKPIsMilestonesDto> RetrieveCompanyKPIsMilestones(CompanyKPIsMilestonesRequestDto companyKPIsMilestonesRequestDto);
        Task<CompanyGovernanceManagementDto> RetrieveCompanyGovernanceManagement(CompanyGovernanceManagementRequestDto companyGovernanceManagementRequestDto);
        Task<Company> GetMyCompany();
        Task<bool> PinCompany(Guid companyToPin, bool isPin,int areaType);
        Task<List<Company>> GetCompaniesList(string companyField, string companyId,
            int pageNumber, int pageSize, string searchText = null);
    }
}
