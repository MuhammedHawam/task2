using PIF.EBP.Application.Networking.DTOs;
using PIF.EBP.Core.DependencyInjection;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PIF.EBP.Application.Networking
{
    public interface INetworkingAppService : ITransientDependency
    {
        Task<NetworkingCompaniesResponseDto> RetrieveNetworkingCompanies(NetworkingCompaniesRequestDto request);
        Task<NetworkingCompanyDetailsDto> GetNetworkingCompanyById(string companyId);
        
        /// <summary>
        /// Get all available filter options (cities, regions, sectors) for networking companies
        /// </summary>
        Task<NetworkingFiltersResponseDto> GetNetworkingFilters();
        
        /// <summary>
        /// Get list of cities for networking company filters
        /// </summary>
        Task<List<NetworkingCityDto>> GetNetworkingCities();
      
        /// <summary>
        /// Get list of regions for networking company filters
        /// </summary>
        Task<List<NetworkingRegionDto>> GetNetworkingRegions();
        
        /// <summary>
        /// Get list of GICS sectors for networking company filters
        /// </summary>
        Task<List<NetworkingSectorDto>> GetNetworkingSectors();
    }
}
