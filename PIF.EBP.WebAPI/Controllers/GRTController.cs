using PIF.EBP.Application.GRT;
using PIF.EBP.Application.GRT.DTOs;
using PIF.EBP.Core.DependencyInjection;
using PIF.EBP.WebAPI.Middleware.ActionFilter;
using System;
using System.Threading.Tasks;
using System.Web.Http;

namespace PIF.EBP.WebAPI.Controllers
{
    /// <summary>
    /// Controller for GRT (Government Reference Tables) Lookups
    /// </summary>
    [ApiResponseWrapper]
    [RoutePrefix("GRT")]
    public class GRTController : ApiController
    {
        private readonly IGRTAppService _grtAppService;

        public GRTController()
        {
            _grtAppService = WindsorContainerProvider.Container.Resolve<IGRTAppService>();
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

        /// <summary>
        /// Get GRT Delivery Plans with pagination and optional search
        /// </summary>
        /// <param name="page">Page number (default: 1)</param>
        /// <param name="pageSize">Items per page (default: 20)</param>
        /// <param name="search">Optional search query to filter delivery plans</param>
        /// <returns>Paginated list of GRT Delivery Plans (simplified: Id, PlanNumber, ParcelID, AssetID, AssetName)</returns>
        [HttpGet]
        [Route("delivery-plans")]
        public async Task<IHttpActionResult> GetDeliveryPlans(int page = 1, int pageSize = 20, string search = null)
        {
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
                var result = await _grtAppService.GetDeliveryPlansPagedAsync(page, pageSize, search);
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

        [HttpDelete]
        [Route("budget")]
        public Task<IHttpActionResult> DeleteBudgetByQuery(long budgetId)
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
        #endregion
    }
}
