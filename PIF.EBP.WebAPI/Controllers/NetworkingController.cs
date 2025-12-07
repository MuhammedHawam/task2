using PIF.EBP.Application.Networking;
using PIF.EBP.Application.Networking.DTOs;
using PIF.EBP.Core.DependencyInjection;
using PIF.EBP.WebAPI.Middleware.ActionFilter;
using System;
using System.Threading.Tasks;
using System.Web.Http;

namespace PIF.EBP.WebAPI.Controllers
{
    [ApiResponseWrapper]
    [RoutePrefix("Networking")]
    public class NetworkingController : BaseController
    {
        private readonly INetworkingAppService _networkingAppService;

        public NetworkingController()
        {
            _networkingAppService = WindsorContainerProvider.Container.Resolve<INetworkingAppService>();
        }

        /// <summary>
        /// Get networking companies with filters, search, and sorting
        /// </summary>
        /// <param name="request">Request containing filters, search text, sort order, and pagination</param>
        /// <returns>Paginated list of networking companies</returns>
        [HttpPost]
        [Route("get-networking-companies")]
        public async Task<IHttpActionResult> GetNetworkingCompanies(NetworkingCompaniesRequestDto request)
        {
            if (request == null)
            {
                request = new NetworkingCompaniesRequestDto();
            }

            var result = await _networkingAppService.RetrieveNetworkingCompanies(request);
            return Ok(result);
        }

        /// <summary>
        /// Get detailed information for a single networking company by ID
        /// </summary>
        /// <param name="companyId">The unique identifier of the company</param>
        /// <returns>Detailed company information including representative contact details</returns>
        [HttpGet]
        [Route("get-networking-company-by-id")]
        public async Task<IHttpActionResult> GetNetworkingCompanyById(string companyId)
        {
            if (string.IsNullOrEmpty(companyId))
            {
                return BadRequest("Company ID is required");
            }

            var result = await _networkingAppService.GetNetworkingCompanyById(companyId);
            return Ok(result);
        }

        /// <summary>
        /// Get all available filter options (cities, regions, sectors) for networking companies
        /// </summary>
        /// <returns>All filter options including cities, regions, and sectors</returns>
        [HttpGet]
        [Route("get-networking-filters")]
        public async Task<IHttpActionResult> GetNetworkingFilters()
        {
            var result = await _networkingAppService.GetNetworkingFilters();
            return Ok(result);
        }

        /// <summary>
        /// Get list of cities for networking company filters
        /// </summary>
        /// <returns>List of active cities with their region information</returns>
        [HttpGet]
        [Route("get-networking-cities")]
        public async Task<IHttpActionResult> GetNetworkingCities()
        {
            var result = await _networkingAppService.GetNetworkingCities();
            return Ok(result);
        }

        /// <summary>
        /// Get list of regions for networking company filters
        /// </summary>
        /// <returns>List of active regions</returns>
        [HttpGet]
        [Route("get-networking-regions")]
        public async Task<IHttpActionResult> GetNetworkingRegions()
        {
            var result = await _networkingAppService.GetNetworkingRegions();
            return Ok(result);
        }

        /// <summary>
        /// Get list of GICS sectors for networking company filters
        /// </summary>
        /// <returns>List of active GICS sectors</returns>
        [HttpGet]
        [Route("get-networking-sectors")]
        public async Task<IHttpActionResult> GetNetworkingSectors()
        {
            var result = await _networkingAppService.GetNetworkingSectors();
            return Ok(result);
        }
    }
}
