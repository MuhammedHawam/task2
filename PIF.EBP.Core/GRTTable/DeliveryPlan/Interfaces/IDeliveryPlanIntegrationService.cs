using PIF.EBP.Core.DependencyInjection;
using System.Threading;
using System.Threading.Tasks;

namespace PIF.EBP.Core.GRTTable.DeliveryPlan.Interfaces
{
    public interface IDeliveryPlanIntegrationService : ITransientDependency
    {
        /// <summary>
        /// Get GRT Delivery Plan Tables by project overview ID with pagination and search
        /// This is the new table-based API where delivery plans are stored as JSON arrays.
        /// </summary>
        /// <param name="projectOverviewId">The ID of the project overview</param>
        /// <param name="page">Page number (1-based)</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <param name="search">Search query (optional)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Paginated GRT Delivery Plan Tables response</returns>
        Task<DeliveryPlanTablesPagedResponse> GetDeliveryPlanTablesPagedAsync(
            long projectOverviewId,
            int page = 1,
            int pageSize = 20,
            string search = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get delivery plan table by ID
        /// </summary>
        /// <param name="id">The ID of the delivery plan table</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Delivery plan table data with JSON string of delivery plans</returns>
        Task<DeliveryPlanTable> GetDeliveryPlanTableByIdAsync(
            long id,
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

        /// <summary>
        /// Create a new delivery plan table in GRT
        /// </summary>
        /// <param name="request">Delivery plan table data with JSON string of delivery plans</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Created delivery plan table response</returns>
        Task<DeliveryPlanTableResponse> CreateDeliveryPlanTableAsync(
            DeliveryPlanTableRequest request,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Update delivery plan table by ID using PATCH method
        /// </summary>
        /// <param name="id">The ID of the delivery plan table</param>
        /// <param name="request">Delivery plan table data to update</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Updated delivery plan table response</returns>
        Task<DeliveryPlanTableResponse> UpdateDeliveryPlanTableAsync(
            long id,
            DeliveryPlanTableRequest request,
            CancellationToken cancellationToken = default);
    }
}
