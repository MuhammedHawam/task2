using PIF.EBP.Core.DependencyInjection;
using System.Threading;
using System.Threading.Tasks;

namespace PIF.EBP.Core.GRTTable
{
    public interface IInfraDeliveryPlanIntegrationService : ITransientDependency
    {
        /// <summary>
        /// Get GRT Infrastructure Delivery Plan Tables by project overview ID with pagination and search
        /// This is the new table-based API where infrastructure delivery plans are stored as JSON.
        /// </summary>
        /// <param name="projectOverviewId">The ID of the project overview</param>
        /// <param name="page">Page number (1-based)</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <param name="search">Search query (optional)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Paginated GRT Infrastructure Delivery Plan Tables response</returns>
        Task<InfraDeliveryPlanTablesPagedResponse> GetInfraDeliveryPlanTablesPagedAsync(
            long projectOverviewId,
            int page = 1,
            int pageSize = 20,
            string search = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get infrastructure delivery plan table by ID
        /// </summary>
        /// <param name="id">The ID of the infrastructure delivery plan table</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Infrastructure delivery plan table data with JSON string</returns>
        Task<InfraDeliveryPlanTable> GetInfraDeliveryPlanTableByIdAsync(
            long id,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Delete infrastructure delivery plan table by ID
        /// </summary>
        /// <param name="id">The ID of the infrastructure delivery plan table</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>True if deleted successfully</returns>
        Task<bool> DeleteInfraDeliveryPlanTableAsync(
            long id,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Create a new infrastructure delivery plan table in GRT
        /// </summary>
        /// <param name="request">Infrastructure delivery plan table data with JSON string</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Created infrastructure delivery plan table response</returns>
        Task<InfraDeliveryPlanTableResponse> CreateInfraDeliveryPlanTableAsync(
            InfraDeliveryPlanTableRequest request,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Update infrastructure delivery plan table by ID using PUT method
        /// </summary>
        /// <param name="id">The ID of the infrastructure delivery plan table</param>
        /// <param name="request">Infrastructure delivery plan table data to update</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Updated infrastructure delivery plan table response</returns>
        Task<InfraDeliveryPlanTableResponse> UpdateInfraDeliveryPlanTableAsync(
            long id,
            InfraDeliveryPlanTableRequest request,
            CancellationToken cancellationToken = default);
    }
}
