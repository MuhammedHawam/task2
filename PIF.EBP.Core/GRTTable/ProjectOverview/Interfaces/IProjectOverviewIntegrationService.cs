using PIF.EBP.Core.DependencyInjection;
using PIF.EBP.Core.GRT;
using System.Threading;
using System.Threading.Tasks;

namespace PIF.EBP.Core.GRTTable.ProjectOverview
{
    /// <summary>
    /// Integration service for GRT Project Overview operations
    /// </summary>
    public interface IProjectOverviewIntegrationService : ITransientDependency
    {

        /// <summary>
        /// Create a new project overview in GRT
        /// </summary>
        /// <param name="request">Project overview data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Created project overview response</returns>
        Task<ProjectOverviewTableResponse> CreateProjectOverviewAsync(
            GRTProjectOverviewRequest request,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get project overview by ID
        /// </summary>
        /// <param name="id">The ID of the project overview</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Project overview data</returns>
        Task<ProjectOverviewTableResponse> GetProjectOverviewByIdAsync(
            long id,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Update project overview by ID
        /// </summary>
        /// <param name="id">The ID of the project overview</param>
        /// <param name="request">Project overview data to update</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Updated project overview response</returns>
        Task<ProjectOverviewTableResponse> UpdateProjectOverviewAsync(
            long id,
            GRTProjectOverviewRequest request,
            CancellationToken cancellationToken = default);

    }
}
