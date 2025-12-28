using PIF.EBP.Core.DependencyInjection;
using PIF.EBP.Core.GRT;
using System.Threading;
using System.Threading.Tasks;

namespace PIF.EBP.Core.GRTTable.CashFlow.Interfaces
{
    public interface ICashFlowIntegrationService : ITransientDependency
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
        /// Get cashflows by project overview id using Liferay filter:
        /// r_projectToCashflowRelationship_c_grtProjectOverviewId eq '{projectOverviewId}'
        /// </summary>
        Task<GRTCashflowsPagedResponse> GetCashflowsByProjectOverviewIdAsync(
            long projectOverviewId,
            int page = 1,
            int pageSize = 1000,
            long? scopeGroupId = null,
            string currentUrl = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Update cashflow by id (PATCH).
        /// Mirrors: PATCH /o/c/grtcashflows/{id}?scopeGroupId=...&currentURL=...
        /// </summary>
        Task<GRTCashflow> UpdateCashflowAsync(
            long id,
            GRTCashflowRequest request,
            long? scopeGroupId = null,
            string currentUrl = null,
            CancellationToken cancellationToken = default);
    }
}
