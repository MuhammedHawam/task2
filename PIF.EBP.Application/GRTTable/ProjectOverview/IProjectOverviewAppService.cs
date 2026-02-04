using PIF.EBP.Application.GRT;
using PIF.EBP.Core.DependencyInjection;
using PIF.EBP.Core.GRTTable;
using PIF.EBP.Core.GRTTable.ProjectOverview;
using System.Threading;
using System.Threading.Tasks;

namespace PIF.EBP.Application.GRTTable.ProjectOverview
{
    /// <summary>
    /// Application service for GRT Project Overview operations
    /// </summary>
    public interface IProjectOverviewAppService : ITransientDependency
    {
        /// <summary>
        /// Create a new project overview
        /// </summary>
        /// <param name="projectOverview">Project overview data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Created project overview response</returns>
        Task<ProjectOverviewResponseDto> CreateProjectOverviewAsync(
            ProjectOverviewDto projectOverview,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get project overview by ID
        /// </summary>
        /// <param name="id">The ID of the project overview</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Project overview data</returns>
        Task<ProjectOverviewDto> GetProjectOverviewAsync(
            long id,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Update project overview by ID
        /// </summary>
        /// <param name="id">The ID of the project overview</param>
        /// <param name="projectOverview">Project overview data to update</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Updated project overview response</returns>
        Task<ProjectOverviewResponseDto> UpdateProjectOverviewAsync(
            long id,
            ProjectOverviewDto projectOverview,
            CancellationToken cancellationToken = default);

    }
}
