using PIF.EBP.Application.Companies;
using PIF.EBP.Application.Companies.DTOs;
using PIF.EBP.Core.DependencyInjection;
using PIF.EBP.WebAPI.Middleware.ActionFilter;
using System;
using System.Threading.Tasks;
using System.Web.Http;

namespace PIF.EBP.WebAPI.Controllers
{
    /// <summary>
    /// Controller for general-purpose company operations
    /// </summary>
    [ApiResponseWrapper]
    [RoutePrefix("Company")]
    public class CompanyController : BaseController
    {
        private readonly ICompanyAppService _companyAppService;

        public CompanyController()
        {
            _companyAppService = WindsorContainerProvider.Container.Resolve<ICompanyAppService>();
        }

        /// <summary>
        /// Get companies with filters, search, and sorting
        /// </summary>
        /// <param name="request">Request containing filters, search text, sort order, and pagination</param>
        /// <returns>Paginated list of companies</returns>
        [HttpPost]
        [Route("get-companies")]
        public async Task<IHttpActionResult> GetCompanies(CompanyRequestDto request)
        {
            if (request == null)
            {
                request = new CompanyRequestDto();
            }

            var result = await _companyAppService.GetCompanies(request);
            return Ok(result);
        }

        /// <summary>
        /// Get detailed information for a single company by ID
        /// </summary>
        /// <param name="companyId">The unique identifier of the company</param>
        /// <returns>Detailed company information including representative contact details</returns>
        [HttpGet]
        [Route("get-company-by-id")]
        public async Task<IHttpActionResult> GetCompanyById(string companyId)
        {
            if (string.IsNullOrEmpty(companyId))
            {
                return BadRequest("Company ID is required");
            }

            var result = await _companyAppService.GetCompanyById(companyId);
            return Ok(result);
        }

        /// <summary>
        /// Get list of GICS sectors for company filters
        /// </summary>
        /// <returns>List of active GICS sectors</returns>
        [HttpGet]
        [Route("get-company-sectors")]
        public async Task<IHttpActionResult> GetCompanySectors()
        {
            var result = await _companyAppService.GetCompanySectors();
            return Ok(result);
        }
    }
}
