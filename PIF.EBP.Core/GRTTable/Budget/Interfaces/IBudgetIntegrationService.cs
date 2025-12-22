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
    }
}

