using PIF.EBP.Application.GRTTable;
using PIF.EBP.Application.GRTTable.ApprovedBP;
using PIF.EBP.Application.GRTTable.Budget;
using PIF.EBP.Application.GRTTable.CashFlow;
using PIF.EBP.Application.GRTTable.DeliveryPlan;
using PIF.EBP.Application.GRTTable.InfraDeliveryPlan;
using PIF.EBP.Application.GRTTable.MultipleSandU;
using PIF.EBP.Core.DependencyInjection;
using PIF.EBP.Core.GRTTable.MultipleSandU.DTOs;
using PIF.EBP.WebAPI.Middleware.ActionFilter;
using System;
using System.Threading.Tasks;
using System.Web.Http;

namespace PIF.EBP.WebAPI.Controllers.Comments
{
    /// <summary>
    /// Controller for GRT (Government Reference Tables) Lookups
    /// </summary>
    [ApiResponseWrapper]
    [RoutePrefix("GRTTables")]
    public class GRTTablesController : ApiController
    {
        private readonly IDeliveryPlanAppService _deliveryPlanAppService;
        private readonly IInfraDeliveryPlanAppService _infraDeliveryPlanAppService;
        private readonly ILookupAppService _lookupAppService;
        private readonly IBudgetTableAppService _budgetTableAppService;
        private readonly IMultipleSUTableAppService _multipleSUTableAppService;
        private readonly IApprovedBPAppService _approvedBPAppService;
        private readonly ICashFlowAppService _cashFlowAppService;

        public GRTTablesController()
        {
            _deliveryPlanAppService = WindsorContainerProvider.Container.Resolve<IDeliveryPlanAppService>();
            _infraDeliveryPlanAppService = WindsorContainerProvider.Container.Resolve<IInfraDeliveryPlanAppService>();
            _lookupAppService = WindsorContainerProvider.Container.Resolve<ILookupAppService>();
            _budgetTableAppService = WindsorContainerProvider.Container.Resolve<IBudgetTableAppService>();
            _multipleSUTableAppService = WindsorContainerProvider.Container.Resolve<IMultipleSUTableAppService>();
            _approvedBPAppService = WindsorContainerProvider.Container.Resolve<IApprovedBPAppService>();
            _cashFlowAppService = WindsorContainerProvider.Container.Resolve<ICashFlowAppService>();
        }
        #region Lookup
        /// <summary>
        /// Get lookup entries by external reference code
        /// </summary>
        /// <param name="externalReferenceCode">The external reference code of the lookup (e.g., 6886ba46-023c-9d25-c379-2985f1b2e381 for Saudi Arabia Regions)</param>
        /// <returns>List of lookup entries with Arabic and English names</returns>
        [HttpGet]
        [Route("get-lookup")]
        public async Task<IHttpActionResult> GetLookup(string externalReferenceCode)
        {
            if (string.IsNullOrWhiteSpace(externalReferenceCode))
            {
                return BadRequest("External reference code is required");
            }

            try
            {
                var result = await _lookupAppService.GetLookupByExternalReferenceCodeAsync(externalReferenceCode);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }
        #endregion

        #region GRT Table (grtTable/*) Endpoints (Absolute Routes Under /GRT)
        /// <summary>
        /// Get project overviews filtered by cycleCompanyMapId (table-based usage).
        /// Mirrors: /o/c/grtprojectoverviews?filter=...&sort=dateModified:desc
        /// </summary>
        [HttpGet]
        [Route("~/GRT/grtTable/budget/project-overviews")]
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
        [Route("~/GRT/grtTable/budget/list-type-entries")]
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
                var result = await _lookupAppService.GetListTypeEntriesByExternalReferenceCodeAsync(
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
        [Route("~/GRT/grtTable/budget/budget-tables")]
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
        [Route("~/GRT/grtTable/budget/budget-tables/{id:long}")]
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
        [Route("~/GRT/grtTable/multiple-sandu/project-overviews")]
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
        [Route("~/GRT/grtTable/multiple-sandu/list-type-entries")]
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
                var result = await _lookupAppService.GetListTypeEntriesByExternalReferenceCodeAsync(
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
        [Route("~/GRT/grtTable/multiple-sandu/multiple-su-tables")]
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
        [Route("~/GRT/grtTable/multiple-sandu/multiple-su-tables/{id:long}")]
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
        [Route("~/GRT/grtTable/approved-bp/project-overviews")]
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
        [Route("~/GRT/grtTable/approved-bp/list-type-entries")]
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
                var result = await _lookupAppService.GetListTypeEntriesByExternalReferenceCodeAsync(
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
        [Route("~/GRT/grtTable/approved-bp/cycle-company-maps/{id:long}")]
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
        [Route("~/GRT/grtTable/approved-bp/approved-bps")]
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
        [Route("~/GRT/grtTable/approved-bp/approved-bps")]
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

        #region CashFlow (grtTable/*) Endpoints (Absolute Routes Under /GRT)
        /// <summary>
        /// CashFlow: project overviews filtered by cycleCompanyMapId (table-based usage).
        /// Mirrors: /o/c/grtprojectoverviews?filter=r_gRTCycleCompanyMapRelationship_c_cycleCompanyMapId+eq+%27312726%27&pageSize=1000&sort=dateModified%3Adesc
        /// </summary>
        [HttpGet]
        [Route("~/GRT/grtTable/cashflow/project-overviews")]
        public async Task<IHttpActionResult> GetCashFlowProjectOverviewsByCycleCompanyMapId(
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
                var result = await _cashFlowAppService.GetProjectOverviewsByCycleCompanyMapIdAsync(
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
        /// CashFlow: get cashflows by project overview id (table-based usage).
        /// Mirrors: /o/c/grtcashflows?filter=r_projectToCashflowRelationship_c_grtProjectOverviewId+eq+%27312740%27&pageSize=1000
        /// </summary>
        [HttpGet]
        [Route("~/GRT/grtTable/cashflow/cashflows")]
        public async Task<IHttpActionResult> GetCashflowsByProjectOverviewId(
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
                var result = await _cashFlowAppService.GetCashflowsByProjectOverviewIdAsync(
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
        /// CashFlow: update cashflow by id (PATCH) (table-based usage).
        /// Mirrors: PATCH /o/c/grtcashflows/{id}?scopeGroupId=...&currentURL=...
        /// </summary>
        [HttpPatch]
        [Route("~/GRT/grtTable/cashflow/cashflows/{id:long}")]
        public async Task<IHttpActionResult> UpdateCashflow(
            long id,
            [FromBody] PIF.EBP.Core.GRT.GRTCashflowRequest request,
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
                var result = await _cashFlowAppService.UpdateCashflowAsync(
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
        #endregion

        #region Delivery Plan Tables (New Table-based API)
        /// <summary>
        /// Get delivery plan tables by project overview ID (paginated)
        /// This is the new table-based API where delivery plans are stored as JSON arrays
        /// </summary>
        /// <param name="projectOverviewId">The ID of the project overview</param>
        /// <param name="page">Page number (default: 1)</param>
        /// <param name="pageSize">Items per page (default: 20)</param>
        /// <param name="search">Optional search query to filter delivery plan tables</param>
        /// <returns>Paginated list of GRT Delivery Plan Tables</returns>
        [HttpGet]
        [Route("project-overview/{projectOverviewId}/delivery-plan-tables")]
        public async Task<IHttpActionResult> GetDeliveryPlanTables(long projectOverviewId, int page = 1, int pageSize = 20, string search = null)
        {
            if (projectOverviewId <= 0)
            {
                return BadRequest("Project overview ID must be greater than zero");
            }

            if (page <= 0)
            {
                return BadRequest("Page number must be greater than zero");
            }

            if (pageSize <= 0 || pageSize > 100)
            {
                return BadRequest("Page size must be between 1 and 100");
            }

            try
            {
                var result = await _deliveryPlanAppService.GetDeliveryPlanTablesPagedAsync(projectOverviewId, page, pageSize, search);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// Get delivery plan table by ID
        /// Returns the table with delivery plans as JSON string
        /// </summary>
        /// <param name="id">The ID of the delivery plan table</param>
        /// <returns>Delivery plan table details</returns>
        [HttpGet]
        [Route("delivery-plan-table/{id}")]
        public async Task<IHttpActionResult> GetDeliveryPlanTable(long id)
        {
            if (id <= 0)
            {
                return BadRequest("Delivery plan table ID must be greater than zero");
            }

            try
            {
                var result = await _deliveryPlanAppService.GetDeliveryPlanTableByIdAsync(id);

                if (result == null)
                {
                    return NotFound();
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// Create a new delivery plan table
        /// The deliveryPlan field should contain a JSON string array of plans
        /// </summary>
        /// <param name="deliveryPlanTable">Delivery plan table data</param>
        /// <returns>Created delivery plan table details</returns>
        [HttpPost]
        [Route("delivery-plan-table")]
        public async Task<IHttpActionResult> CreateDeliveryPlanTable([FromBody] DeliveryPlanDto deliveryPlanTable)
        {
            if (deliveryPlanTable == null)
            {
                return BadRequest("Delivery plan table data is required");
            }

            try
            {
                var result = await _deliveryPlanAppService.CreateDeliveryPlanTableAsync(deliveryPlanTable);

                if (result.Success)
                {
                    return Ok(result);
                }
                else
                {
                    return BadRequest(result.Message);
                }
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// Update delivery plan table by ID using PATCH
        /// </summary>
        /// <param name="id">The ID of the delivery plan table</param>
        /// <param name="deliveryPlanTable">Delivery plan table data to update</param>
        /// <returns>Updated delivery plan table details</returns>
        [AcceptVerbs("PATCH", "PUT")]
        [Route("delivery-plan-table/{id}")]
        public async Task<IHttpActionResult> UpdateDeliveryPlanTable(long id, [FromBody] DeliveryPlanDto deliveryPlanTable)
        {
            if (id <= 0)
            {
                return BadRequest("Delivery plan table ID must be greater than zero");
            }

            if (deliveryPlanTable == null)
            {
                return BadRequest("Delivery plan table data is required");
            }

            try
            {
                var result = await _deliveryPlanAppService.UpdateDeliveryPlanTableAsync(id, deliveryPlanTable);

                if (result.Success)
                {
                    return Ok(result);
                }
                else
                {
                    return BadRequest(result.Message);
                }
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// Delete delivery plan table by ID
        /// </summary>
        /// <param name="id">The ID of the delivery plan table to delete</param>
        /// <returns>Success status</returns>
        [HttpDelete]
        [Route("delivery-plan-table/{id}")]
        public async Task<IHttpActionResult> DeleteDeliveryPlanTable(long id)
        {
            if (id <= 0)
            {
                return BadRequest("Delivery plan table ID must be greater than zero");
            }

            try
            {
                var result = await _deliveryPlanAppService.DeleteDeliveryPlanTableAsync(id);

                if (result)
                {
                    return Ok(new { success = true, message = "Delivery plan table deleted successfully" });
                }
                else
                {
                    return BadRequest("Failed to delete delivery plan table");
                }
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }
        #endregion

        #region Infrastructure Delivery Plan Tables (New Table-based API)
        /// <summary>
        /// Get infrastructure delivery plan tables by project overview ID (paginated)
        /// This is the new table-based API where infrastructure delivery plans are stored as JSON
        /// </summary>
        /// <param name="projectOverviewId">The ID of the project overview</param>
        /// <param name="page">Page number (default: 1)</param>
        /// <param name="pageSize">Items per page (default: 20)</param>
        /// <param name="search">Optional search query</param>
        /// <returns>Paginated list of GRT Infrastructure Delivery Plan Tables</returns>
        [HttpGet]
        [Route("project-overview/{projectOverviewId}/infra-delivery-plan-tables")]
        public async Task<IHttpActionResult> GetInfraDeliveryPlanTables(long projectOverviewId, int page = 1, int pageSize = 20, string search = null)
        {
            if (projectOverviewId <= 0)
            {
                return BadRequest("Project overview ID must be greater than zero");
            }

            if (page <= 0)
            {
                return BadRequest("Page number must be greater than zero");
            }

            if (pageSize <= 0 || pageSize > 100)
            {
                return BadRequest("Page size must be between 1 and 100");
            }

            try
            {
                var result = await _infraDeliveryPlanAppService.GetInfraDeliveryPlanTablesPagedAsync(projectOverviewId, page, pageSize, search);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// Get infrastructure delivery plan table by ID
        /// Returns the table with infrastructure delivery plans as JSON string
        /// </summary>
        /// <param name="id">The ID of the infrastructure delivery plan table</param>
        /// <returns>Infrastructure delivery plan table details</returns>
        [HttpGet]
        [Route("infra-delivery-plan-table/{id}")]
        public async Task<IHttpActionResult> GetInfraDeliveryPlanTable(long id)
        {
            if (id <= 0)
            {
                return BadRequest("Infrastructure delivery plan table ID must be greater than zero");
            }

            try
            {
                var result = await _infraDeliveryPlanAppService.GetInfraDeliveryPlanTableByIdAsync(id);

                if (result == null)
                {
                    return NotFound();
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// Create a new infrastructure delivery plan table
        /// The gRTInfraDeliveryPlanTable field should contain a JSON string with the matrix data
        /// </summary>
        /// <param name="infraDeliveryPlanTable">Infrastructure delivery plan table data</param>
        /// <returns>Created infrastructure delivery plan table details</returns>
        [HttpPost]
        [Route("infra-delivery-plan-table")]
        public async Task<IHttpActionResult> CreateInfraDeliveryPlanTable([FromBody] InfraDeliveryPlanDto infraDeliveryPlanTable)
        {
            if (infraDeliveryPlanTable == null)
            {
                return BadRequest("Infrastructure delivery plan table data is required");
            }

            try
            {
                var result = await _infraDeliveryPlanAppService.CreateInfraDeliveryPlanTableAsync(infraDeliveryPlanTable);

                if (result.Success)
                {
                    return Ok(result);
                }
                else
                {
                    return BadRequest(result.Message);
                }
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// Update infrastructure delivery plan table by ID
        /// </summary>
        /// <param name="id">The ID of the infrastructure delivery plan table</param>
        /// <param name="infraDeliveryPlanTable">Infrastructure delivery plan table data to update</param>
        /// <returns>Updated infrastructure delivery plan table details</returns>
        [AcceptVerbs("PUT")]
        [Route("infra-delivery-plan-table/{id}")]
        public async Task<IHttpActionResult> UpdateInfraDeliveryPlanTable(long id, [FromBody] InfraDeliveryPlanDto infraDeliveryPlanTable)
        {
            if (id <= 0)
            {
                return BadRequest("Infrastructure delivery plan table ID must be greater than zero");
            }

            if (infraDeliveryPlanTable == null)
            {
                return BadRequest("Infrastructure delivery plan table data is required");
            }

            try
            {
                var result = await _infraDeliveryPlanAppService.UpdateInfraDeliveryPlanTableAsync(id, infraDeliveryPlanTable);

                if (result.Success)
                {
                    return Ok(result);
                }
                else
                {
                    return BadRequest(result.Message);
                }
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// Delete infrastructure delivery plan table by ID
        /// </summary>
        /// <param name="id">The ID of the infrastructure delivery plan table to delete</param>
        /// <returns>Success status</returns>
        [HttpDelete]
        [Route("infra-delivery-plan-table/{id}")]
        public async Task<IHttpActionResult> DeleteInfraDeliveryPlanTable(long id)
        {
            if (id <= 0)
            {
                return BadRequest("Infrastructure delivery plan table ID must be greater than zero");
            }

            try
            {
                var result = await _infraDeliveryPlanAppService.DeleteInfraDeliveryPlanTableAsync(id);

                if (result)
                {
                    return Ok(new { success = true, message = "Infrastructure delivery plan table deleted successfully" });
                }
                else
                {
                    return BadRequest("Failed to delete infrastructure delivery plan table");
                }
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }
        #endregion
    }
}
