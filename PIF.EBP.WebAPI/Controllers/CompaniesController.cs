using PIF.EBP.Application.Companies;
using PIF.EBP.Application.Companies.DTOs;
using PIF.EBP.Core.DependencyInjection;
using PIF.EBP.WebAPI.Middleware.ActionFilter;
using PIF.EBP.WebAPI.Middleware.Authorize;
using System;
using System.Threading.Tasks;
using System.Web.Http;

namespace PIF.EBP.WebAPI.Controllers
{
    [ApiResponseWrapper]
    [RoutePrefix("Companies")]
    [APIKEY]
    public class CompaniesController : ApiController
    {
        private readonly ICompanyIntegrationAppService _companyAppService;

        public CompaniesController()
        {
            _companyAppService = WindsorContainerProvider.Container.Resolve<ICompanyIntegrationAppService>();
        }

        [HttpPost]
        [Route("get-companies")]
        public async Task<IHttpActionResult> GetCompanies(CompanyIntegrationRequestDto request)
        {
            if (request == null)
            {
                request = new CompanyIntegrationRequestDto();
            }

            var result = await _companyAppService.GetCompanies(request);
            return Ok(result);
        }

        [HttpGet]
        [Route("get-company/{companyId}")]
        public async Task<IHttpActionResult> GetCompanyById(Guid companyId)
        {
            if (companyId == Guid.Empty)
            {
                return BadRequest("Company ID is required");
            }

            var result = await _companyAppService.GetCompanyById(companyId);
            return Ok(result);
        }

        [HttpGet]
        [Route("get-sectors")]
        public async Task<IHttpActionResult> GetSectors()
        {
            var result = await _companyAppService.GetSectors();
            return Ok(result);
        }
    }
}
