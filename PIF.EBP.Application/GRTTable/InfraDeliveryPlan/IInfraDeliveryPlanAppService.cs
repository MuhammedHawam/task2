using PIF.EBP.Core.DependencyInjection;
using System.Threading;
using System.Threading.Tasks;

namespace PIF.EBP.Application.GRTTable.InfraDeliveryPlan
{
    public interface IInfraDeliveryPlanAppService : ITransientDependency
    {
        /// <summary>
        /// Get infrastructure delivery plan tables by project overview ID (paginated)
        /// </summary>
        Task<InfraDeliveryPlanPagedDto> GetInfraDeliveryPlanTablesPagedAsync(
            long projectOverviewId,
            int page = 1,
            int pageSize = 20,
            string search = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get infrastructure delivery plan table by ID
        /// </summary>
        Task<InfraDeliveryPlanDto> GetInfraDeliveryPlanTableByIdAsync(
            long id,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Create a new infrastructure delivery plan table
        /// </summary>
        Task<InfraDeliveryPlanResponseDto> CreateInfraDeliveryPlanTableAsync(
            InfraDeliveryPlanDto infraDeliveryPlanTable,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Update infrastructure delivery plan table by ID
        /// </summary>
        Task<InfraDeliveryPlanResponseDto> UpdateInfraDeliveryPlanTableAsync(
            long id,
            InfraDeliveryPlanDto infraDeliveryPlanTable,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Delete infrastructure delivery plan table by ID
        /// </summary>
        Task<bool> DeleteInfraDeliveryPlanTableAsync(
            long id,
            CancellationToken cancellationToken = default);
    }
}
