using PIF.EBP.Core.DependencyInjection;
using System.Threading;
using System.Threading.Tasks;

namespace PIF.EBP.Core.GRTTable.ApprovedBP.Interfaces
{
    public interface IApprovedBPIntegrationService : ITransientDependency
    {
        /// <summary>
        /// Get CycleCompanyMap by id.
        /// Mirrors: GET /o/c/cyclecompanymaps/{id}?scopeGroupId=...&currentURL=...
        /// </summary>
        Task<GRTCycleCompanyMapItem> GetCycleCompanyMapByIdAsync(
            long id,
            long? scopeGroupId = null,
            string currentUrl = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get ApprovedBP entries by project overview id using Liferay filter:
        /// r_projectToApprovedBPRelationship_c_grtProjectOverviewId eq '{projectOverviewId}'
        /// Mirrors: GET /o/c/grtapprovedbps?filter=...&pageSize=1000
        /// </summary>
        Task<GRTApprovedBPsPagedResponse> GetApprovedBPsByProjectOverviewIdAsync(
            long projectOverviewId,
            int page = 1,
            int pageSize = 1000,
            long? scopeGroupId = null,
            string currentUrl = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Create ApprovedBP entry (POST).
        /// Mirrors: POST /o/c/grtapprovedbps?scopeGroupId=...&currentURL=...
        /// </summary>
        Task<GRTApprovedBPItem> CreateApprovedBPAsync(
            GRTApprovedBPCreateRequest request,
            long? scopeGroupId = null,
            string currentUrl = null,
            CancellationToken cancellationToken = default);
    }
}

