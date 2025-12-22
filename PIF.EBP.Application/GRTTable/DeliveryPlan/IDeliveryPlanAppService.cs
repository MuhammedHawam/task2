using PIF.EBP.Core.DependencyInjection;
using System.Threading;
using System.Threading.Tasks;

namespace PIF.EBP.Application.GRTTable.DeliveryPlan
{
    public interface IDeliveryPlanAppService : ITransientDependency
    {
        /// <summary>
        /// Get delivery plan tables by project overview ID (paginated)
        /// This is the new table-based API where delivery plans are stored as JSON arrays
        /// </summary>
        /// <param name="projectOverviewId">The ID of the project overview</param>
        /// <param name="page">Page number (1-based)</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <param name="search">Search query (optional)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Paginated delivery plan tables response</returns>
        Task<DeliveryPlanPagedDto> GetDeliveryPlanTablesPagedAsync(
            long projectOverviewId,
            int page = 1,
            int pageSize = 20,
            string search = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get delivery plan table by ID
        /// Returns the table with delivery plans as JSON string
        /// </summary>
        /// <param name="id">The ID of the delivery plan table</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Delivery plan table details</returns>
        Task<DeliveryPlanDto> GetDeliveryPlanTableByIdAsync(
            long id,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Create a new delivery plan table
        /// The deliveryPlan field should contain a JSON string array of plans
        /// </summary>
        /// <param name="deliveryPlanTable">Delivery plan table data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Created delivery plan table response</returns>
        Task<DeliveryPlanResponseDto> CreateDeliveryPlanTableAsync(
            DeliveryPlanDto deliveryPlanTable,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Update delivery plan table by ID using PATCH
        /// </summary>
        /// <param name="id">The ID of the delivery plan table</param>
        /// <param name="deliveryPlanTable">Delivery plan table data to update</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Updated delivery plan table response</returns>
        Task<DeliveryPlanResponseDto> UpdateDeliveryPlanTableAsync(
            long id,
            DeliveryPlanDto deliveryPlanTable,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Delete delivery plan table by ID
        /// </summary>
        /// <param name="id">The ID of the delivery plan table</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>True if deleted successfully</returns>
        Task<bool> DeleteDeliveryPlanTableAsync(
            long id,
            CancellationToken cancellationToken = default);
    }
}
