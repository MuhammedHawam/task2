using PIF.EBP.Application.PerfomanceDashboard;
using PIF.EBP.Application.PerformanceDashboard.DTOs;
using PIF.EBP.Core.DependencyInjection;
using PIF.EBP.WebAPI.Middleware.ActionFilter;
using System;
using System.Threading.Tasks;
using System.Web.Http;

namespace PIF.EBP.WebAPI.Controllers
{
    [ApiResponseWrapper]
    [RoutePrefix("PerfomanceDashboard")]
    public class PerformanceDashboardController : BaseController
    {
        private readonly IPerformanceDashboardAppService _perfomanceDashboardAppService;

        public PerformanceDashboardController()
        {
            _perfomanceDashboardAppService = WindsorContainerProvider.Container.Resolve<IPerformanceDashboardAppService>();
        }

        [HttpGet]
        [Route("get-my-company")]
        public async Task<IHttpActionResult> GetMyComapny()
        {
            var result = await _perfomanceDashboardAppService.GetMyCompany();

            return Ok(result);
        }

        [HttpGet]
        [Route("get-companies")]
        public async Task<IHttpActionResult> GetCompanies(int pageNumber, int pageSize, string searchText = null, bool AllPIFCompanies = false)
        {
            var result = await _perfomanceDashboardAppService.RetrieveCompanies(pageNumber, pageSize, searchText, AllPIFCompanies);

            return Ok(result);
        }

        [HttpGet]
        [Route("get-company-overview")]
        public async Task<IHttpActionResult> GetCompanyOverview(Guid companyId)
        {
            if (companyId != Guid.Empty)
            {
                var result = await _perfomanceDashboardAppService.RetrieveCompanyOverview(companyId);
                return Ok(result);
            }
            return BadRequest();
        }

        [HttpPost]
        [Route("get-company-kpis-milestones")]
        public async Task<IHttpActionResult> GetCompanyKPIsMilestones(CompanyKPIsMilestonesRequestDto companyKPIsMilestonesRequestDto)
        {
            var result = await _perfomanceDashboardAppService.RetrieveCompanyKPIsMilestones(companyKPIsMilestonesRequestDto);

            return Ok(result);
        }

        [HttpPost]
        [Route("get-company-governance-management")]
        public async Task<IHttpActionResult> GetCompanyGovernanceManagement(CompanyGovernanceManagementRequestDto companyGovernanceManagementRequestDto)
        {
            var result = await _perfomanceDashboardAppService.RetrieveCompanyGovernanceManagement(companyGovernanceManagementRequestDto);

            return Ok(result);
        }

        [HttpPost]
        [Route("pin-company")]
        public async Task<IHttpActionResult> PinCompany(PinCompanyReq request)
        {
            var result = await _perfomanceDashboardAppService.PinCompany(request.Id, request.IsPin,request.AreaType);

            return Ok(result);
        }
    }
}