using PIF.EBP.Application.GRT;
using PIF.EBP.Application.GRT.Budget.DTOs;
using PIF.EBP.Application.GRT.DTOs;
using PIF.EBP.Application.GRTTable;
using PIF.EBP.Application.GRTTable.ApprovedBP;
using PIF.EBP.Application.GRTTable.Budget;
using PIF.EBP.Application.GRTTable.MultipleSandU;
using PIF.EBP.Core.DependencyInjection;
using PIF.EBP.Core.GRTTable.MultipleSandU.DTOs;
using PIF.EBP.WebAPI.Middleware.ActionFilter;
using PIF.EBP.WebAPI.Middleware.Authorize;
using System;
using System.Drawing.Printing;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.UI;

namespace PIF.EBP.WebAPI.Controllers
{
    /// <summary>
    /// Controller for GRT (Government Reference Tables) Lookups
    /// </summary>
    [ApiResponseWrapper]
    [RoutePrefix("GRT")]
    public class GRTController : BaseController
    {
        private readonly IGRTAppService _grtAppService;

        public GRTController()
        {
            _grtAppService = WindsorContainerProvider.Container.Resolve<IGRTAppService>();
        }

        #region Lookup And Cycles And Project Overview
        /// <summary>
        /// Assign PC to a cycle for a company
        /// </summary>
        /// <param name="companyId">The GUID of the company</param>
        /// <param name="cycleId">The ID of the cycle</param>
        /// <returns>Success status</returns>
        [APIKEY]
        [HttpPost]
        [Route("pc-assigned")]
        public async Task<IHttpActionResult> PcAssigned(PcAssignedDto request)
        {
            if (string.IsNullOrWhiteSpace(request.companyId))
            {
                return BadRequest("Company ID is required");
            }

            if (request.cycleId <= 0)
            {
                return BadRequest("Cycle ID must be greater than zero");
            }

            try
            {
                // Implementation pending
                return Ok(new { success = true, message = "PC assigned successfully" });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

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
                var result = await _grtAppService.GetLookupByExternalReferenceCodeAsync(externalReferenceCode);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// Create a new project overview
        /// </summary>
        /// <param name="projectOverview">Project overview data</param>
        /// <returns>Created project overview details</returns>
        [HttpPost]
        [Route("project-overview")]
        public async Task<IHttpActionResult> CreateProjectOverview([FromBody] GRTProjectOverviewDto projectOverview)
        {
            if (projectOverview == null)
            {
                return BadRequest("Project overview data is required");
            }

            try
            {
                var result = await _grtAppService.CreateProjectOverviewAsync(projectOverview);
                
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
        /// Get project overview by ID
        /// </summary>
        /// <param name="id">The ID of the project overview (e.g., 236496)</param>
        /// <returns>Project overview data</returns>
        [HttpGet]
        [Route("project-overview/{id}")]
        public async Task<IHttpActionResult> GetProjectOverview(long id)
        {
            if (id <= 0)
            {
                return BadRequest("Project overview ID must be greater than zero");
            }

            try
            {
                var result = await _grtAppService.GetProjectOverviewAsync(id);
                
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
        /// Update project overview by ID
        /// </summary>
        /// <param name="id">The ID of the project overview (e.g., 250880)</param>
        /// <param name="projectOverview">Project overview data to update</param>
        /// <returns>Updated project overview details</returns>
        [HttpPut]
        [Route("project-overview/{id}")]
        public async Task<IHttpActionResult> UpdateProjectOverview(long id, [FromBody] GRTProjectOverviewDto projectOverview)
        {
            if (id <= 0)
            {
                return BadRequest("Project overview ID must be greater than zero");
            }

            if (projectOverview == null)
            {
                return BadRequest("Project overview data is required");
            }

            try
            {
                var result = await _grtAppService.UpdateProjectOverviewAsync(id, projectOverview);
                
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
        /// Get GRT Cycles for a specific company with pagination and optional search
        /// </summary>
        /// <param name="companyId">Company ID to filter cycles</param>
        /// <param name="page">Page number (default: 1)</param>
        /// <param name="pageSize">Items per page (default: 20)</param>
        /// <param name="search">Optional search query to filter cycles by name</param>
        /// <returns>Paginated list of GRT Cycles with combined cycle company map data</returns>
        [HttpGet]
        [Route("cycles")]
        public async Task<IHttpActionResult> GetCycles(long companyId, int page = 1, int pageSize = 20, string search = null)
        {
            if (companyId <= 0)
            {
                return BadRequest("Company ID must be greater than zero");
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
                var result = await _grtAppService.GetCyclesPagedAsync(companyId, page, pageSize, search);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }
        #endregion

        #region Delivery Plans
        /// <summary>
        /// Get GRT Delivery Plans by project overview ID with pagination and optional search
        /// </summary>
        /// <param name="projectOverviewId">The ID of the project overview (e.g., 267703)</param>
        /// <param name="page">Page number (default: 1)</param>
        /// <param name="pageSize">Items per page (default: 20)</param>
        /// <param name="search">Optional search query to filter delivery plans</param>
        /// <returns>Paginated list of GRT Delivery Plans (simplified: Id, PlanNumber, ParcelID, AssetID, AssetName)</returns>
        [HttpGet]
        [Route("project-overview/{projectOverviewId}/delivery-plans")]
        public async Task<IHttpActionResult> GetDeliveryPlans(long projectOverviewId, int page = 1, int pageSize = 20, string search = null)
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
                var result = await _grtAppService.GetDeliveryPlansPagedAsync(projectOverviewId, page, pageSize, search);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// Get delivery plan by ID with all details
        /// </summary>
        /// <param name="id">The ID of the delivery plan (e.g., 270535)</param>
        /// <returns>Delivery plan details including all sections</returns>
        [HttpGet]
        [Route("delivery-plan/{id}")]
        public async Task<IHttpActionResult> GetDeliveryPlan(long id)
        {
            if (id <= 0)
            {
                return BadRequest("Delivery plan ID must be greater than zero");
            }

            try
            {
                var result = await _grtAppService.GetDeliveryPlanByIdAsync(id);
                
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
        /// Delete delivery plan by ID
        /// </summary>
        /// <param name="id">The ID of the delivery plan to delete (e.g., 270535)</param>
        /// <returns>Success status</returns>
        [HttpDelete]
        [Route("delivery-plan/{id}")]
        public async Task<IHttpActionResult> DeleteDeliveryPlan(long id)
        {
            if (id <= 0)
            {
                return BadRequest("Delivery plan ID must be greater than zero");
            }

            try
            {
                var result = await _grtAppService.DeleteDeliveryPlanAsync(id);
                
                if (result)
                {
                    return Ok(new { success = true, message = "Delivery plan deleted successfully" });
                }
                else
                {
                    return BadRequest("Failed to delete delivery plan");
                }
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// Create a new delivery plan
        /// </summary>
        /// <param name="deliveryPlan">Delivery plan data</param>
        /// <returns>Created delivery plan details</returns>
        [HttpPost]
        [Route("delivery-plan")]
        public async Task<IHttpActionResult> CreateDeliveryPlan([FromBody] GRTDeliveryPlanDto deliveryPlan)
        {
            if (deliveryPlan == null)
            {
                return BadRequest("Delivery plan data is required");
            }

            try
            {
                var result = await _grtAppService.CreateDeliveryPlanAsync(deliveryPlan);
                
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
        /// Update delivery plan by ID
        /// </summary>
        /// <param name="id">The ID of the delivery plan (e.g., 270535)</param>
        /// <param name="deliveryPlan">Delivery plan data to update</param>
        /// <returns>Updated delivery plan details</returns>
        [HttpPut]
        [Route("delivery-plan/{id}")]
        public async Task<IHttpActionResult> UpdateDeliveryPlan(long id, [FromBody] GRTDeliveryPlanDto deliveryPlan)
        {
            if (id <= 0)
            {
                return BadRequest("Delivery plan ID must be greater than zero");
            }

            if (deliveryPlan == null)
            {
                return BadRequest("Delivery plan data is required");
            }

            try
            {
                var result = await _grtAppService.UpdateDeliveryPlanAsync(id, deliveryPlan);
                
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
        #endregion

        #region Infra Delivery Plans
        /// <summary>
        /// Get infrastructure delivery plans by project overview ID
        /// </summary>
        /// <param name="projectOverviewId">The ID of the project overview (e.g., 267703)</param>
        /// <param name="page">Page number (default: 1)</param>
        /// <param name="pageSize">Items per page (default: 20)</param>
        /// <returns>Paginated list of infrastructure delivery plans</returns>
        [HttpGet]
        [Route("project-overview/{projectOverviewId}/infra-delivery-plans")]
        public async Task<IHttpActionResult> GetInfraDeliveryPlansByProject(long projectOverviewId, int page = 1, int pageSize = 20)
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
                var result = await _grtAppService.GetInfraDeliveryPlansByProjectIdAsync(projectOverviewId, page, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// Get infrastructure delivery plan by ID with all details including years
        /// </summary>
        /// <param name="id">The ID of the infrastructure delivery plan (e.g., 277099)</param>
        /// <returns>Infrastructure delivery plan details including year entries</returns>
        [HttpGet]
        [Route("infra-delivery-plan/{id}")]
        public async Task<IHttpActionResult> GetInfraDeliveryPlan(long id)
        {
            if (id <= 0)
            {
                return BadRequest("Infrastructure delivery plan ID must be greater than zero");
            }

            try
            {
                var result = await _grtAppService.GetInfraDeliveryPlanByIdAsync(id);
                
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
        /// Create a new infrastructure delivery plan
        /// </summary>
        /// <param name="infraDeliveryPlan">Infrastructure delivery plan data</param>
        /// <returns>Created infrastructure delivery plan details</returns>
        [HttpPost]
        [Route("infra-delivery-plan")]
        public async Task<IHttpActionResult> CreateInfraDeliveryPlan([FromBody] GRTInfraDeliveryPlanDto infraDeliveryPlan)
        {
            if (infraDeliveryPlan == null)
            {
                return BadRequest("Infrastructure delivery plan data is required");
            }

            try
            {
                var result = await _grtAppService.CreateInfraDeliveryPlanAsync(infraDeliveryPlan);
                
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
        /// Update infrastructure delivery plan by ID
        /// </summary>
        /// <param name="id">The ID of the infrastructure delivery plan (e.g., 277099)</param>
        /// <param name="infraDeliveryPlan">Infrastructure delivery plan data to update</param>
        /// <returns>Updated infrastructure delivery plan details</returns>
        [HttpPut]
        [Route("infra-delivery-plan/{id}")]
        public async Task<IHttpActionResult> UpdateInfraDeliveryPlan(long id, [FromBody] GRTInfraDeliveryPlanDto infraDeliveryPlan)
        {
            if (id <= 0)
            {
                return BadRequest("Infrastructure delivery plan ID must be greater than zero");
            }

            if (infraDeliveryPlan == null)
            {
                return BadRequest("Infrastructure delivery plan data is required");
            }

            try
            {
                var result = await _grtAppService.UpdateInfraDeliveryPlanAsync(id, infraDeliveryPlan);
                
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
        /// Delete infrastructure delivery plan by ID
        /// </summary>
        /// <param name="id">The ID of the infrastructure delivery plan to delete (e.g., 277099)</param>
        /// <returns>Success status</returns>
        [HttpDelete]
        [Route("infra-delivery-plan/{id}")]
        public async Task<IHttpActionResult> DeleteInfraDeliveryPlan(long id)
        {
            if (id <= 0)
            {
                return BadRequest("Infrastructure delivery plan ID must be greater than zero");
            }

            try
            {
                var result = await _grtAppService.DeleteInfraDeliveryPlanAsync(id);
                
                if (result)
                {
                    return Ok(new { success = true, message = "Infrastructure delivery plan deleted successfully" });
                }
                else
                {
                    return BadRequest("Failed to delete infrastructure delivery plan");
                }
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }
        #endregion

        #region Land Sales
        /// <summary>
        /// Get land sales by project overview ID
        /// </summary>
        /// <param name="projectOverviewId">The ID of the project overview (e.g., 266536)</param>
        /// <param name="page">Page number (default: 1)</param>
        /// <param name="pageSize">Items per page (default: 20)</param>
        /// <returns>Paginated list of land sales</returns>
        [HttpGet]
        [Route("project-overview/{projectOverviewId}/land-sales")]
        public async Task<IHttpActionResult> GetLandSalesByProject(long projectOverviewId, int page = 1, int pageSize = 20)
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
                var result = await _grtAppService.GetLandSalesByProjectIdAsync(projectOverviewId, page, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// Get land sale by ID with all details
        /// </summary>
        /// <param name="id">The ID of the land sale (e.g., 267664)</param>
        /// <returns>Land sale details</returns>
        [HttpGet]
        [Route("land-sale/{id}")]
        public async Task<IHttpActionResult> GetLandSale(long id)
        {
            if (id <= 0)
            {
                return BadRequest("Land sale ID must be greater than zero");
            }

            try
            {
                var result = await _grtAppService.GetLandSaleByIdAsync(id);
                
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
        /// Create a new land sale
        /// </summary>
        /// <param name="landSale">Land sale data</param>
        /// <returns>Created land sale details</returns>
        [HttpPost]
        [Route("land-sale")]
        public async Task<IHttpActionResult> CreateLandSale([FromBody] GRTLandSaleDto landSale)
        {
            if (landSale == null)
            {
                return BadRequest("Land sale data is required");
            }

            try
            {
                var result = await _grtAppService.CreateLandSaleAsync(landSale);
                
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
        /// Update land sale by ID
        /// </summary>
        /// <param name="id">The ID of the land sale (e.g., 267664)</param>
        /// <param name="landSale">Land sale data to update</param>
        /// <returns>Updated land sale details</returns>
        [HttpPut]
        [Route("land-sale/{id}")]
        public async Task<IHttpActionResult> UpdateLandSale(long id, [FromBody] GRTLandSaleDto landSale)
        {
            if (id <= 0)
            {
                return BadRequest("Land sale ID must be greater than zero");
            }

            if (landSale == null)
            {
                return BadRequest("Land sale data is required");
            }

            try
            {
                var result = await _grtAppService.UpdateLandSaleAsync(id, landSale);
                
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
        /// Delete land sale by ID
        /// </summary>
        /// <param name="id">The ID of the land sale to delete (e.g., 267664)</param>
        /// <returns>Success status</returns>
        [HttpDelete]
        [Route("land-sale/{id}")]
        public async Task<IHttpActionResult> DeleteLandSale(long id)
        {
            if (id <= 0)
            {
                return BadRequest("Land sale ID must be greater than zero");
            }

            try
            {
                var result = await _grtAppService.DeleteLandSaleAsync(id);
                
                if (result)
                {
                    return Ok(new { success = true, message = "Land sale deleted successfully" });
                }
                else
                {
                    return BadRequest("Failed to delete land sale");
                }
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }
        #endregion

        #region Cashflows
        /// <summary>
        /// Get cashflows by project overview ID with pagination
        /// </summary>
        /// <param name="projectOverviewId">The ID of the project overview (e.g., 267703)</param>
        /// <param name="page">Page number (default: 1)</param>
        /// <param name="pageSize">Items per page (default: 20)</param>
        /// <returns>Paginated list of cashflows with all asset class data</returns>
        [HttpGet]
        [Route("project-overview/{projectOverviewId}/cashflows")]
        public async Task<IHttpActionResult> GetCashflowsByProject(long projectOverviewId, int page = 1, int pageSize = 20)
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
                var result = await _grtAppService.GetCashflowsByProjectIdAsync(projectOverviewId, page, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// Get cashflow by ID with all details
        /// </summary>
        /// <param name="id">The ID of the cashflow (e.g., 267707)</param>
        /// <returns>Cashflow details with all asset class financial data</returns>
        [HttpGet]
        [Route("cashflow/{id}")]
        public async Task<IHttpActionResult> GetCashflow(long id)
        {
            if (id <= 0)
            {
                return BadRequest("Cashflow ID must be greater than zero");
            }

            try
            {
                var result = await _grtAppService.GetCashflowByIdAsync(id);
                
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

        #endregion

        #region Budget
        [HttpGet]
        [Route("budgets")]
        public async Task<IHttpActionResult> GetBudgets(long poid, int page = 1, int pageSize = 20, string search = null)
        {
            if (poid <= 0)
            {
                return BadRequest("poid must be greater than zero");
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
                var result = await _grtAppService.GetGRTBudgetsPagedAsync(poid, page, pageSize, search);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpGet]
        [Route("budget/{id}")]
        public async Task<IHttpActionResult> GetBudgetById(long id)
        {
            if (id <= 0)
            {
                return BadRequest("Budget ID must be greater than zero");
            }

            try
            {
                var result = await _grtAppService.GetBudgetByIdAsync(id);

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

        [HttpPost]
        [Route("budget")]
        public async Task<IHttpActionResult> CreateBudget([FromBody] GRTBudgetCreateDto budget)
        {
            if (budget == null)
            {
                return BadRequest("Budget data is required");
            }
            if (!budget.ProjectOverviewId.HasValue || budget.ProjectOverviewId <= 0)
            {
                return BadRequest("ProjectOverviewId is required when creating a budget");
            }
            try
            {
                var result = await _grtAppService.CreateBudgetAsync(budget);

                if (result?.Success == true)
                {
                    return Ok(result);
                }

                return BadRequest(result?.Message ?? "Failed to create budget");
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

        [HttpPut]
        [Route("budget/{id}")]
        public async Task<IHttpActionResult> UpdateBudget(long id, [FromBody] GRTBudgetCreateDto budget)
        {
            if (id <= 0)
            {
                return BadRequest("Budget ID must be greater than zero");
            }

            if (budget == null)
            {
                return BadRequest("Budget data is required");
            }

            try
            {
                var result = await _grtAppService.UpdateBudgetAsync(id, budget);

                if (result?.Success == true)
                {
                    return Ok(result);
                }

                return BadRequest(result?.Message ?? "Failed to update budget");
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

        [HttpDelete]
        [Route("budget/{budgetId:long}")]
        public Task<IHttpActionResult> DeleteBudget(long budgetId)
        {
            return DeleteBudgetInternal(budgetId);
        }

        private async Task<IHttpActionResult> DeleteBudgetInternal(long budgetId)
        {
            if (budgetId <= 0)
            {
                return BadRequest("Budget ID must be greater than zero");
            }

            try
            {
                var result = await _grtAppService.DeleteBudgetAsync(budgetId);

                if (result)
                {
                    return Ok(new { success = true, message = "Budget deleted successfully" });
                }

                return BadRequest("Failed to delete budget");
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [AcceptVerbs("PATCH")]
        [Route("budget/{externalReferenceCode}/sections")]
        public async Task<IHttpActionResult> UpdateBudgetSections(
            string externalReferenceCode,
            [FromBody] GRTBudgetSectionsDto sections)
        {
            if (string.IsNullOrWhiteSpace(externalReferenceCode))
            {
                return BadRequest("External reference code is required");
            }

            if (sections == null)
            {
                return BadRequest("Budget sections data is required");
            }

            try
            {
                var result = await _grtAppService.UpdateBudgetSectionsAsync(externalReferenceCode, sections);

                if (result?.Success == true)
                {
                    return Ok(result);
                }

                return BadRequest(result?.Message ?? "Failed to update budget sections");
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
        /// Update cashflow by ID
        /// </summary>
        /// <param name="id">The ID of the cashflow (e.g., 267707)</param>
        /// <param name="cashflow">Cashflow data to update</param>
        /// <returns>Updated cashflow details</returns>
        [HttpPut]
        [Route("cashflow/{id}")]
        public async Task<IHttpActionResult> UpdateCashflow(long id, [FromBody] GRTCashflowDto cashflow)
        {
            if (id <= 0)
            {
                return BadRequest("Cashflow ID must be greater than zero");
            }

            if (cashflow == null)
            {
                return BadRequest("Cashflow data is required");
            }

            try
            {
                var result = await _grtAppService.UpdateCashflowAsync(id, cashflow);
                
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
        #endregion

        #region LOI & HMA
        /// <summary>
        /// Get LOI & HMA (Approved Business Plan) by project overview ID with pagination
        /// </summary>
        /// <param name="projectOverviewId">The ID of the project overview (e.g., 267703)</param>
        /// <param name="page">Page number (default: 1)</param>
        /// <param name="pageSize">Items per page (default: 20)</param>
        /// <returns>Paginated list of LOI & HMA entries</returns>
        [HttpGet]
        [Route("project-overview/{projectOverviewId}/loi-hma")]
        public async Task<IHttpActionResult> GetLOIHMAsByProject(long projectOverviewId, int page = 1, int pageSize = 20)
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
                var result = await _grtAppService.GetLOIHMAsByProjectIdAsync(projectOverviewId, page, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// Get LOI & HMA by ID with all details
        /// </summary>
        /// <param name="id">The ID of the LOI & HMA</param>
        /// <returns>LOI & HMA details</returns>
        [HttpGet]
        [Route("loi-hma/{id}")]
        public async Task<IHttpActionResult> GetLOIHMA(long id)
        {
            if (id <= 0)
            {
                return BadRequest("LOI & HMA ID must be greater than zero");
            }

            try
            {
                var result = await _grtAppService.GetLOIHMAByIdAsync(id);
                
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
        /// Create a new LOI & HMA
        /// </summary>
        /// <param name="loihma">LOI & HMA data</param>
        /// <returns>Created LOI & HMA details</returns>
        [HttpPost]
        [Route("loi-hma")]
        public async Task<IHttpActionResult> CreateLOIHMA([FromBody] GRTLOIHMADto loihma)
        {
            if (loihma == null)
            {
                return BadRequest("LOI & HMA data is required");
            }

            try
            {
                var result = await _grtAppService.CreateLOIHMAAsync(loihma);
                
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
        /// Update LOI & HMA by project overview ID and LOI & HMA ID
        /// </summary>
        /// <param name="projectOverviewId">The ID of the project overview</param>
        /// <param name="id">The ID of the LOI & HMA</param>
        /// <param name="loihma">LOI & HMA data to update</param>
        /// <returns>Updated LOI & HMA details</returns>
        [HttpPut]
        [Route("project-overview/{projectOverviewId}/loi-hma/{id}")]
        public async Task<IHttpActionResult> UpdateLOIHMA(long projectOverviewId, long id, [FromBody] GRTLOIHMADto loihma)
        {
            if (projectOverviewId <= 0)
            {
                return BadRequest("Project overview ID must be greater than zero");
            }

            if (id <= 0)
            {
                return BadRequest("LOI & HMA ID must be greater than zero");
            }

            if (loihma == null)
            {
                return BadRequest("LOI & HMA data is required");
            }

            try
            {
                var result = await _grtAppService.UpdateLOIHMAAsync(projectOverviewId, id, loihma);
                
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
        /// Delete LOI & HMA by ID
        /// </summary>
        /// <param name="id">The ID of the LOI & HMA to delete</param>
        /// <returns>Success status</returns>
        [HttpDelete]
        [Route("loi-hma/{id}")]
        public async Task<IHttpActionResult> DeleteLOIHMA(long id)
        {
            if (id <= 0)
            {
                return BadRequest("LOI & HMA ID must be greater than zero");
            }

            try
            {
                var result = await _grtAppService.DeleteLOIHMAAsync(id);
                
                if (result)
                {
                    return Ok(new { success = true, message = "LOI & HMA deleted successfully" });
                }
                else
                {
                    return BadRequest("Failed to delete LOI & HMA");
                }
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }
        #endregion

        #region Project Impact
        /// <summary>
        /// Get project impact by ID with all details
        /// </summary>
        /// <param name="id">The ID of the project impact (e.g., 284542)</param>
        /// <returns>Project impact details</returns>
        [HttpGet]
        [Route("projectImpact/{id}")]
        public async Task<IHttpActionResult> GetProjectImpact(long id)
        {
            if (id <= 0)
            {
                return BadRequest("Project impact ID must be greater than zero");
            }

            try
            {
                var result = await _grtAppService.GetProjectImpactByIdAsync(id);

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
        /// Get project impact by ID with all details
        /// </summary>
        /// <param name="id">The ID of the project impact (e.g., 284542)</param>
        /// <returns>Project impact details</returns>
        [HttpGet]
        [Route("project-overview/{projectOverviewId}/projectImpact")]
        public async Task<IHttpActionResult> GetProjectImpactByProject(long projectOverviewId ,int page = 1, int pageSize = 20)
        {
            if (projectOverviewId <= 0)
            {
                return BadRequest("Project overview ID must be greater than zero");
            }

            try
            {
                var result =  await _grtAppService.GetProjectImpactByProjectIdAsync(projectOverviewId, page, pageSize);

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
        /// Update project impact by ID
        /// </summary>
        /// <param name="id">The ID of the project impact (e.g., 284542)</param>
        /// <param name="projectImpact">Project impact data to update</param>
        /// <returns>Update result and metadata</returns>
        [HttpPut]
        [Route("project-overview/{projectOverviewId}/projectImpact/{id}")]
        public async Task<IHttpActionResult> UpdateProjectImpact(
            long projectOverviewId, long id,
            [FromBody] GRTProjectImpactDto projectImpact)
        {
          
            if (projectImpact == null)
            {
                return BadRequest("Project impact data is required");
            }

            try
            {
                var result = new GRTProjectImpactResponseDto();
                if (id <= 0)
                {
                     result = await _grtAppService.CreateProjectImpactAsync(projectImpact);
                }
                else {
                     result = await _grtAppService.UpdateProjectImpactAsync(id, projectOverviewId, projectImpact);
                }
                   

                if (result.Success)
                {
                    return Ok(result);
                }

                return BadRequest(result.Message);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }



        [HttpPost]
        [Route("projectImpact")]
        public async Task<IHttpActionResult> CreateProjectImpact(
                             [FromBody] GRTProjectImpactDto projectImpact)
        {
            if (projectImpact == null)
            {
                return BadRequest("Project impact data is required");
            }

            try
            {
                var result = await _grtAppService.CreateProjectImpactAsync(projectImpact);

                if (result.Success)
                {
                    return Ok(result);
                }

                return BadRequest(result.Message);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }
        #endregion

        #region Multiple S&U
        /// <summary>
        /// Get Multiple S&U (Sources & Uses) Financial Planning by project overview ID
        /// </summary>
        /// <param name="projectOverviewId">The ID of the project overview (e.g., 280572)</param>
        /// <param name="page">Page number (default: 1)</param>
        /// <param name="pageSize">Items per page (default: 20)</param>
        /// <returns>Paginated list of Multiple S&U entries with region and financial totals</returns>
        [HttpGet]
        [Route("project-overview/{projectOverviewId}/multiple-sandu")]
        public async Task<IHttpActionResult> GetMultipleSandUsByProject(long projectOverviewId, int page = 1, int pageSize = 20)
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
                var result = await _grtAppService.GetMultipleSandUsByProjectIdAsync(projectOverviewId, page, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// Get Multiple S&U by ID with all details
        /// </summary>
        /// <param name="id">The ID of the Multiple S&U (e.g., 280599)</param>
        /// <returns>Multiple S&U details including all JSON financial data</returns>
        [HttpGet]
        [Route("multiple-sandu/{id}")]
        public async Task<IHttpActionResult> GetMultipleSandU(long id)
        {
            if (id <= 0)
            {
                return BadRequest("Multiple S&U ID must be greater than zero");
            }

            try
            {
                var result = await _grtAppService.GetMultipleSandUByIdAsync(id);
                
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
        /// Create a new Multiple S&U (Sources & Uses) Financial Planning
        /// </summary>
        /// <param name="multipleSandU">Multiple S&U data including region and financial JSON data</param>
        /// <returns>Created Multiple S&U details</returns>
        [HttpPost]
        [Route("multiple-sandu")]
        public async Task<IHttpActionResult> CreateMultipleSandU([FromBody] GRTMultipleSandUDto multipleSandU)
        {
            if (multipleSandU == null)
            {
                return BadRequest("Multiple S&U data is required");
            }

            try
            {
                var result = await _grtAppService.CreateMultipleSandUAsync(multipleSandU);
                
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
        /// Update Multiple S&U by project overview ID and Multiple S&U ID
        /// </summary>
        /// <param name="projectOverviewId">The ID of the project overview (e.g., 56466)</param>
        /// <param name="id">The ID of the Multiple S&U (e.g., 666)</param>
        /// <param name="multipleSandU">Multiple S&U data to update</param>
        /// <returns>Updated Multiple S&U details</returns>
        [HttpPut]
        [Route("project-overview/{projectOverviewId}/multiple-sandu/{id}")]
        public async Task<IHttpActionResult> UpdateMultipleSandU(long projectOverviewId, long id, [FromBody] GRTMultipleSandUDto multipleSandU)
        {
            if (projectOverviewId <= 0)
            {
                return BadRequest("Project overview ID must be greater than zero");
            }

            if (id <= 0)
            {
                return BadRequest("Multiple S&U ID must be greater than zero");
            }

            if (multipleSandU == null)
            {
                return BadRequest("Multiple S&U data is required");
            }

            try
            {
                var result = await _grtAppService.UpdateMultipleSandUAsync(projectOverviewId, id, multipleSandU);
                
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
        /// Delete Multiple S&U by project overview ID and Multiple S&U ID
        /// </summary>
        /// <param name="projectOverviewId">The ID of the project overview (e.g., 54554)</param>
        /// <param name="id">The ID of the Multiple S&U to delete (e.g., 555)</param>
        /// <returns>Success status</returns>
        [HttpDelete]
        [Route("project-overview/{projectOverviewId}/multiple-sandu/{id}")]
        public async Task<IHttpActionResult> DeleteMultipleSandU(long projectOverviewId, long id)
        {
            if (projectOverviewId <= 0)
            {
                return BadRequest("Project overview ID must be greater than zero");
            }

            if (id <= 0)
            {
                return BadRequest("Multiple S&U ID must be greater than zero");
            }

            try
            {
                var result = await _grtAppService.DeleteMultipleSandUAsync(projectOverviewId, id);
                
                if (result)
                {
                    return Ok(new { success = true, message = "Multiple S&U deleted successfully" });
                }
                else
                {
                    return BadRequest("Failed to delete Multiple S&U");
                }
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }
        #endregion

        #region Approved BP
        /// <summary>
        /// Get Approved BPs by project overview ID with pagination
        /// </summary>
        /// <param name="projectOverviewId">The ID of the project overview</param>
        /// <param name="page">Page number (default: 1)</param>
        /// <param name="pageSize">Items per page (default: 20)</param>
        /// <returns>Paginated list of Approved BPs</returns>
        [HttpGet]
        [Route("project-overview/{projectOverviewId}/approved-bps")]
        public async Task<IHttpActionResult> GetApprovedBPsByProject(long projectOverviewId, int page = 1, int pageSize = 20)
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
                var result = await _grtAppService.GetApprovedBPsByProjectIdAsync(projectOverviewId, page, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// Get Approved BP by ID with all details
        /// </summary>
        /// <param name="id">The ID of the Approved BP</param>
        /// <returns>Approved BP details</returns>
        [HttpGet]
        [Route("approved-bp/{id}")]
        public async Task<IHttpActionResult> GetApprovedBP(long id)
        {
            if (id <= 0)
            {
                return BadRequest("Approved BP ID must be greater than zero");
            }

            try
            {
                var result = await _grtAppService.GetApprovedBPByIdAsync(id);
                
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
        /// Create a new Approved BP
        /// </summary>
        /// <param name="approvedBP">Approved BP data</param>
        /// <returns>Created Approved BP details</returns>
        [HttpPost]
        [Route("approved-bp")]
        public async Task<IHttpActionResult> CreateApprovedBP([FromBody] GRTApprovedBPDto approvedBP)
        {
            if (approvedBP == null)
            {
                return BadRequest("Approved BP data is required");
            }

            try
            {
                var result = await _grtAppService.CreateApprovedBPAsync(approvedBP);
                
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
        /// Update Approved BP by ID
        /// </summary>
        /// <param name="id">The ID of the Approved BP</param>
        /// <param name="approvedBP">Approved BP data to update</param>
        /// <returns>Updated Approved BP details</returns>
        [HttpPut]
        [Route("approved-bp/{id}")]
        public async Task<IHttpActionResult> UpdateApprovedBP(long id, [FromBody] GRTApprovedBPDto approvedBP)
        {
            if (id <= 0)
            {
                return BadRequest("Approved BP ID must be greater than zero");
            }

            if (approvedBP == null)
            {
                return BadRequest("Approved BP data is required");
            }

            try
            {
                var result = await _grtAppService.UpdateApprovedBPAsync(id, approvedBP);
                
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
        /// Delete Approved BP by ID
        /// </summary>
        /// <param name="id">The ID of the Approved BP to delete</param>
        /// <returns>Success status</returns>
        [HttpDelete]
        [Route("approved-bp/{id}")]
        public async Task<IHttpActionResult> DeleteApprovedBP(long id)
        {
            if (id <= 0)
            {
                return BadRequest("Approved BP ID must be greater than zero");
            }

            try
            {
                var result = await _grtAppService.DeleteApprovedBPAsync(id);
                
                if (result)
                {
                    return Ok(new { success = true, message = "Approved BP deleted successfully" });
                }
                else
                {
                    return BadRequest("Failed to delete Approved BP");
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
