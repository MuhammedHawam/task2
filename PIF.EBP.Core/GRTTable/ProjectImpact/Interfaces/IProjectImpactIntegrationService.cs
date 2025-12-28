using PIF.EBP.Core.DependencyInjection;
using PIF.EBP.Core.GRT;
using PIF.EBP.Core.GRTTable;
using System.Threading;
using System.Threading.Tasks;

namespace PIF.EBP.Core.GRTTable.ProjectImpact.Interfaces
{
    public interface IProjectImpactIntegrationService : ITransientDependency
    {
        /// <summary>
        /// Get project overviews by cycle company map id using Liferay filter:
        /// r_gRTCycleCompanyMapRelationship_c_cycleCompanyMapId eq '{cycleCompanyMapId}'
        /// </summary>
        Task<GRTProjectOverviewsPagedResponse> GetProjectOverviewsByCycleCompanyMapIdAsync(
            long cycleCompanyMapId,
            int page = 1,
            int pageSize = 1000,
            string sort = "dateModified:desc",
            long? scopeGroupId = null,
            string currentUrl = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get project impacts by project overview id using relationship endpoint:
        /// /o/c/grtprojectoverviews/{projectOverviewId}/projectToProjectImpactRelationship
        /// </summary>
        Task<GRTProjectImpactPagedResponse> GetProjectImpactsByProjectOverviewIdAsync(
            long projectOverviewId,
            int page = 1,
            int pageSize = 1,
            string sort = "dateModified:desc",
            long? scopeGroupId = null,
            string currentUrl = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Update project impact by id (PATCH).
        /// Mirrors: PATCH /o/c/grtprojectimpacts/{id}?scopeGroupId=...&currentURL=...
        /// </summary>
        Task<GRTProjectImpact> UpdateProjectImpactAsync(
            long id,
            GRTProjectImpactRequest request,
            long? scopeGroupId = null,
            string currentUrl = null,
            CancellationToken cancellationToken = default);
    }
}
