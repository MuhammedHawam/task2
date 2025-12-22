using PIF.EBP.Application.GRTTable;
using PIF.EBP.Application.GRTTable.DeliveryPlan;
using PIF.EBP.Application.GRTTable.InfraDeliveryPlan;
using PIF.EBP.Core.DependencyInjection;
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

        public GRTTablesController()
        {
            _deliveryPlanAppService = WindsorContainerProvider.Container.Resolve<IDeliveryPlanAppService>();
            _infraDeliveryPlanAppService = WindsorContainerProvider.Container.Resolve<IInfraDeliveryPlanAppService>();
            _lookupAppService = WindsorContainerProvider.Container.Resolve<ILookupAppService>();
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
