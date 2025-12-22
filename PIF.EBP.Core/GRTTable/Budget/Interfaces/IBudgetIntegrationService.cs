using PIF.EBP.Core.DependencyInjection;
using System.Threading;
using System.Threading.Tasks;

namespace PIF.EBP.Core.GRTTable.Budget.Interfaces
{
    public interface IBudgetIntegrationService : ITransientDependency
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
        /// Get budget tables by project overview id using Liferay filter:
        /// r_projectToBudgetTableRelationship_c_grtProjectOverviewId eq '{projectOverviewId}'
        /// </summary>
        Task<GRTBudgetTablesPagedResponse> GetBudgetTablesByProjectOverviewIdAsync(
            long projectOverviewId,
            int page = 1,
            int pageSize = 1,
            long? scopeGroupId = null,
            string currentUrl = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Update budget table by id (PUT).
        /// Mirrors: PUT /o/c/grtbudgettables/{id}?scopeGroupId=...&currentURL=...
        /// </summary>
        Task<GRTBudgetTableItem> UpdateBudgetTableAsync(
            long id,
            GRTBudgetTableUpdateRequest request,
            long? scopeGroupId = null,
            string currentUrl = null,
            CancellationToken cancellationToken = default);
    }
}

