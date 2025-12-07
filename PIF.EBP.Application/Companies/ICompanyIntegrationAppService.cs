using PIF.EBP.Application.Companies.DTOs;
using PIF.EBP.Core.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PIF.EBP.Application.Companies
{
    public interface ICompanyIntegrationAppService : ITransientDependency
    {
        Task<CompanyIntegrationResponseDto> GetCompanies(CompanyIntegrationRequestDto request);
        Task<CompanyIntegrationDto> GetCompanyById(Guid companyId);
        Task<List<CompanySectorDto>> GetSectors();
    }
}
