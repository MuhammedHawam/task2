using PIF.EBP.Application.Companies.DTOs;
using PIF.EBP.Core.DependencyInjection;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PIF.EBP.Application.Companies
{
    /// <summary>
    /// Service interface for general-purpose company operations
    /// </summary>
    public interface ICompanyAppService : ITransientDependency
    {
        /// <summary>
        /// Get paginated list of companies with filters, search, and sorting
        /// </summary>
        /// <param name="request">Request containing filters, search text, sort order, and pagination</param>
        /// <returns>Paginated list of companies</returns>
        Task<CompanyResponseDto> GetCompanies(CompanyRequestDto request);
        
        /// <summary>
        /// Get detailed information for a single company by ID
        /// </summary>
        /// <param name="companyId">The unique identifier of the company</param>
        /// <returns>Detailed company information</returns>
        Task<CompanyDetailsDto> GetCompanyById(string companyId);
        
        /// <summary>
        /// Get list of GICS sectors for company filters
        /// </summary>
        /// <returns>List of active GICS sectors</returns>
        Task<List<CompanySectorDto>> GetCompanySectors();
    }
}
