using PIF.EBP.Application.GRTTable;
using PIF.EBP.Application.GRTTable.ApprovedBP;
using PIF.EBP.Application.GRTTable.Budget;
using PIF.EBP.Application.GRTTable.MultipleSandU;
using PIF.EBP.Core.DependencyInjection;
using PIF.EBP.Core.GRTTable.MultipleSandU.DTOs;
using PIF.EBP.WebAPI.Middleware.ActionFilter;
using System;
using System.Threading.Tasks;
using System.Web.Http;

namespace PIF.EBP.WebAPI.Controllers
{
    /// <summary>
    /// Controller for GRT Table (table-based usage) endpoints.
    /// </summary>
    [ApiResponseWrapper]
    [RoutePrefix("GRT")]
    public class GRTTableController : BaseController
    {
        private readonly ILookupAppService _grtTableLookupAppService;
        private readonly IBudgetTableAppService _budgetTableAppService;
        private readonly IMultipleSUTableAppService _multipleSUTableAppService;
        private readonly IApprovedBPAppService _approvedBPAppService;

        public GRTTableController()
        {
            _grtTableLookupAppService = WindsorContainerProvider.Container.Resolve<ILookupAppService>();
            _budgetTableAppService = WindsorContainerProvider.Container.Resolve<IBudgetTableAppService>();
            _multipleSUTableAppService = WindsorContainerProvider.Container.Resolve<IMultipleSUTableAppService>();
            _approvedBPAppService = WindsorContainerProvider.Container.Resolve<IApprovedBPAppService>();
        }

        #region grtTable
        /// <summary>
        /// Get project overviews filtered by cycleCompanyMapId (table-based usage).
        /// Mirrors: /o/c/grtprojectoverviews?filter=...&sort=dateModified:desc
        /// </summary>
        [HttpGet]
        [Route("grtTable/budget/project-overviews")]
        public async Task<IHttpActionResult> GetProjectOverviewsByCycleCompanyMapId(
            long cycleCompanyMapId,
            int page = 1,
            int pageSize = 1000,
            string sort = "dateModified:desc",
            long? scopeGroupId = null,
            string currentURL = null)
        {
            if (cycleCompanyMapId <= 0)
            {
                return BadRequest("cycleCompanyMapId must be greater than zero");
            }

            if (page <= 0)
            {
                return BadRequest("Page number must be greater than zero");
            }

            if (pageSize <= 0)
            {
                return BadRequest("Page size must be greater than zero");
            }

            try
            {
                var result = await _budgetTableAppService.GetProjectOverviewsByCycleCompanyMapIdAsync(
                    cycleCompanyMapId,
                    page,
                    pageSize,
                    sort,
                    scopeGroupId,
                    currentURL);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// Get list type entries by list type definition external reference code (table-based usage).
        /// Mirrors: /o/headless-admin-list-type/v1.0/list-type-definitions/by-external-reference-code/{erc}/list-type-entries
        /// </summary>
        [HttpGet]
        [Route("grtTable/budget/list-type-entries")]
        public async Task<IHttpActionResult> GetListTypeEntries(
            string externalReferenceCode,
            int page = 1,
            int pageSize = 1000,
            long? scopeGroupId = null,
            string currentURL = null)
        {
            if (string.IsNullOrWhiteSpace(externalReferenceCode))
            {
                return BadRequest("externalReferenceCode is required");
            }

            if (page <= 0)
            {
                return BadRequest("Page number must be greater than zero");
            }

            if (pageSize <= 0)
            {
                return BadRequest("Page size must be greater than zero");
            }

            try
            {
                var result = await _grtTableLookupAppService.GetListTypeEntriesByExternalReferenceCodeAsync(
                    externalReferenceCode,
                    page,
                    pageSize,
                    scopeGroupId,
                    currentURL);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// Get GRT budget tables by project overview id (table-based usage).
        /// Mirrors: /o/c/grtbudgettables?filter=...&pageSize=1
        /// </summary>
        [HttpGet]
        [Route("grtTable/budget/budget-tables")]
        public async Task<IHttpActionResult> GetGrtBudgetTables(
            long projectOverviewId,
            int page = 1,
            int pageSize = 1,
            long? scopeGroupId = null,
            string currentURL = null)
        {
            if (projectOverviewId <= 0)
            {
                return BadRequest("projectOverviewId must be greater than zero");
            }

            if (page <= 0)
            {
                return BadRequest("Page number must be greater than zero");
            }

            if (pageSize <= 0)
            {
                return BadRequest("Page size must be greater than zero");
            }

            try
            {
                var result = await _budgetTableAppService.GetGrtBudgetTablesAsync(
                    projectOverviewId,
                    page,
                    pageSize,
                    scopeGroupId,
                    currentURL);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// Update GRT budget table by id (table-based usage).
        /// Mirrors: PUT /o/c/grtbudgettables/{id}?scopeGroupId=...&currentURL=...
        /// </summary>
        [HttpPut]
        [Route("grtTable/budget/budget-tables/{id:long}")]
        public async Task<IHttpActionResult> UpdateGrtBudgetTable(
            long id,
            [FromBody] PIF.EBP.Core.GRTTable.GRTBudgetTableUpdateRequest request,
            long? scopeGroupId = null,
            string currentURL = null)
        {
            if (id <= 0)
            {
                return BadRequest("id must be greater than zero");
            }

            if (request == null)
            {
                return BadRequest("Request body is required");
            }

            try
            {
                var result = await _budgetTableAppService.UpdateGrtBudgetTableAsync(
                    id,
                    request,
                    scopeGroupId,
                    currentURL);

                return Ok(result);
            }
            catch (ArgumentException argEx)
            {
                return BadRequest(argEx.Message);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// Multiple S&U: project overviews filtered by cycleCompanyMapId (table-based usage).
        /// Mirrors: /o/c/grtprojectoverviews?filter=...&sort=dateModified:desc
        /// </summary>
        [HttpGet]
        [Route("grtTable/multiple-sandu/project-overviews")]
        public async Task<IHttpActionResult> GetMultipleSandUProjectOverviewsByCycleCompanyMapId(
            long cycleCompanyMapId,
            int page = 1,
            int pageSize = 1000,
            string sort = "dateModified:desc",
            long? scopeGroupId = null,
            string currentURL = null)
        {
            if (cycleCompanyMapId <= 0)
            {
                return BadRequest("cycleCompanyMapId must be greater than zero");
            }

            if (page <= 0)
            {
                return BadRequest("Page number must be greater than zero");
            }

            if (pageSize <= 0)
            {
                return BadRequest("Page size must be greater than zero");
            }

            try
            {
                // Reuse the same underlying project-overview query used by Budget.
                var result = await _budgetTableAppService.GetProjectOverviewsByCycleCompanyMapIdAsync(
                    cycleCompanyMapId,
                    page,
                    pageSize,
                    sort,
                    scopeGroupId,
                    currentURL);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// Multiple S&U: list type entries by list type definition external reference code (table-based usage).
        /// Mirrors: /o/headless-admin-list-type/v1.0/list-type-definitions/by-external-reference-code/{erc}/list-type-entries
        /// </summary>
        [HttpGet]
        [Route("grtTable/multiple-sandu/list-type-entries")]
        public async Task<IHttpActionResult> GetMultipleSandUListTypeEntries(
            string externalReferenceCode,
            int page = 1,
            int pageSize = 1000,
            long? scopeGroupId = null,
            string currentURL = null)
        {
            if (string.IsNullOrWhiteSpace(externalReferenceCode))
            {
                return BadRequest("externalReferenceCode is required");
            }

            if (page <= 0)
            {
                return BadRequest("Page number must be greater than zero");
            }

            if (pageSize <= 0)
            {
                return BadRequest("Page size must be greater than zero");
            }

            try
            {
                // Reuse the same underlying list-type-entries query used by Budget.
                var result = await _grtTableLookupAppService.GetListTypeEntriesByExternalReferenceCodeAsync(
                    externalReferenceCode,
                    page,
                    pageSize,
                    scopeGroupId,
                    currentURL);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// Multiple S&U: get multiple SU tables by project overview id (table-based usage).
        /// Mirrors: /o/c/grtmultiplesutables?filter=...&pageSize=1
        /// </summary>
        [HttpGet]
        [Route("grtTable/multiple-sandu/multiple-su-tables")]
        public async Task<IHttpActionResult> GetMultipleSUTables(
            long projectOverviewId,
            int page = 1,
            int pageSize = 1,
            long? scopeGroupId = null,
            string currentURL = null)
        {
            if (projectOverviewId <= 0)
            {
                return BadRequest("projectOverviewId must be greater than zero");
            }

            if (page <= 0)
            {
                return BadRequest("Page number must be greater than zero");
            }

            if (pageSize <= 0)
            {
                return BadRequest("Page size must be greater than zero");
            }

            try
            {
                var result = await _multipleSUTableAppService.GetMultipleSUTablesAsync(
                    projectOverviewId,
                    page,
                    pageSize,
                    scopeGroupId,
                    currentURL);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// Multiple S&U: update multiple SU table by id (PUT).
        /// Mirrors: PUT /o/c/grtmultiplesutables/{id}?scopeGroupId=...&currentURL=...
        /// </summary>
        [HttpPut]
        [Route("grtTable/multiple-sandu/multiple-su-tables/{id:long}")]
        public async Task<IHttpActionResult> UpdateMultipleSUTable(
            long id,
            [FromBody] GRTMultipleSUTableUpdateRequest request,
            long? scopeGroupId = null,
            string currentURL = null)
        {
            if (id <= 0)
            {
                return BadRequest("id must be greater than zero");
            }

            if (request == null)
            {
                return BadRequest("Request body is required");
            }

            try
            {
                var result = await _multipleSUTableAppService.UpdateMultipleSUTableAsync(
                    id,
                    request,
                    scopeGroupId,
                    currentURL);

                return Ok(result);
            }
            catch (ArgumentException argEx)
            {
                return BadRequest(argEx.Message);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// ApprovedBP: project overviews filtered by cycleCompanyMapId (table-based usage).
        /// Mirrors: /o/c/grtprojectoverviews?filter=...&sort=dateModified:desc
        /// </summary>
        [HttpGet]
        [Route("grtTable/approved-bp/project-overviews")]
        public async Task<IHttpActionResult> GetApprovedBPProjectOverviewsByCycleCompanyMapId(
            long cycleCompanyMapId,
            int page = 1,
            int pageSize = 1000,
            string sort = "dateModified:desc",
            long? scopeGroupId = null,
            string currentURL = null)
        {
            if (cycleCompanyMapId <= 0)
            {
                return BadRequest("cycleCompanyMapId must be greater than zero");
            }

            if (page <= 0)
            {
                return BadRequest("Page number must be greater than zero");
            }

            if (pageSize <= 0)
            {
                return BadRequest("Page size must be greater than zero");
            }

            try
            {
                // Reuse the same underlying project-overview query used by Budget.
                var result = await _budgetTableAppService.GetProjectOverviewsByCycleCompanyMapIdAsync(
                    cycleCompanyMapId,
                    page,
                    pageSize,
                    sort,
                    scopeGroupId,
                    currentURL);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// ApprovedBP: list type entries by list type definition external reference code (table-based usage).
        /// Mirrors: /o/headless-admin-list-type/v1.0/list-type-definitions/by-external-reference-code/{erc}/list-type-entries
        /// </summary>
        [HttpGet]
        [Route("grtTable/approved-bp/list-type-entries")]
        public async Task<IHttpActionResult> GetApprovedBPListTypeEntries(
            string externalReferenceCode,
            int page = 1,
            int pageSize = 1000,
            long? scopeGroupId = null,
            string currentURL = null)
        {
            if (string.IsNullOrWhiteSpace(externalReferenceCode))
            {
                return BadRequest("externalReferenceCode is required");
            }

            if (page <= 0)
            {
                return BadRequest("Page number must be greater than zero");
            }

            if (pageSize <= 0)
            {
                return BadRequest("Page size must be greater than zero");
            }

            try
            {
                // Reuse the same underlying list-type-entries query used by Budget.
                var result = await _grtTableLookupAppService.GetListTypeEntriesByExternalReferenceCodeAsync(
                    externalReferenceCode,
                    page,
                    pageSize,
                    scopeGroupId,
                    currentURL);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// ApprovedBP: get cycle company map by id (table-based usage).
        /// Mirrors: GET /o/c/cyclecompanymaps/{id}?scopeGroupId=...&currentURL=...
        /// </summary>
        [HttpGet]
        [Route("grtTable/approved-bp/cycle-company-maps/{id:long}")]
        public async Task<IHttpActionResult> GetCycleCompanyMapById(
            long id,
            long? scopeGroupId = null,
            string currentURL = null)
        {
            if (id <= 0)
            {
                return BadRequest("id must be greater than zero");
            }

            try
            {
                var result = await _approvedBPAppService.GetCycleCompanyMapByIdAsync(
                    id,
                    scopeGroupId,
                    currentURL);

                return Ok(result);
            }
            catch (ArgumentException argEx)
            {
                return BadRequest(argEx.Message);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// ApprovedBP: get approved BP entries by project overview id (table-based usage).
        /// Mirrors: /o/c/grtapprovedbps?filter=...&pageSize=1000
        /// </summary>
        [HttpGet]
        [Route("grtTable/approved-bp/approved-bps")]
        public async Task<IHttpActionResult> GetApprovedBPs(
            long projectOverviewId,
            int page = 1,
            int pageSize = 1000,
            long? scopeGroupId = null,
            string currentURL = null)
        {
            if (projectOverviewId <= 0)
            {
                return BadRequest("projectOverviewId must be greater than zero");
            }

            if (page <= 0)
            {
                return BadRequest("Page number must be greater than zero");
            }

            if (pageSize <= 0)
            {
                return BadRequest("Page size must be greater than zero");
            }

            try
            {
                var result = await _approvedBPAppService.GetApprovedBPsAsync(
                    projectOverviewId,
                    page,
                    pageSize,
                    scopeGroupId,
                    currentURL);

                return Ok(result);
            }
            catch (ArgumentException argEx)
            {
                return BadRequest(argEx.Message);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// ApprovedBP: create approved BP entry (POST) (table-based usage).
        /// Mirrors: POST /o/c/grtapprovedbps?scopeGroupId=...&currentURL=...
        /// </summary>
        [HttpPost]
        [Route("grtTable/approved-bp/approved-bps")]
        public async Task<IHttpActionResult> CreateApprovedBP(
            [FromBody] PIF.EBP.Core.GRTTable.GRTApprovedBPCreateRequest request,
            long? scopeGroupId = null,
            string currentURL = null)
        {
            if (request == null)
            {
                return BadRequest("Request body is required");
            }

            try
            {
                var result = await _approvedBPAppService.CreateApprovedBPAsync(
                    request,
                    scopeGroupId,
                    currentURL);

                return Ok(result);
            }
            catch (ArgumentException argEx)
            {
                return BadRequest(argEx.Message);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }
        #endregion
    }
}

