using PIF.EBP.Core.DependencyInjection;
using PIF.EBP.Core.GRTTable.MultipleSandU.DTOs;
using System.Threading;
using System.Threading.Tasks;

namespace PIF.EBP.Core.GRTTable.MultipleSandU.Interfaces
{
    public interface IMultipleSUTableIntegrationService : ITransientDependency
    {
        /// <summary>
        /// Get Multiple S&U tables by project overview id using Liferay filter:
        /// r_projectToMultipleSUTableRelationship_c_grtProjectOverviewId eq '{projectOverviewId}'
        /// </summary>
        Task<GRTMultipleSUTablesPagedResponse> GetMultipleSUTablesByProjectOverviewIdAsync(
            long projectOverviewId,
            int page = 1,
            int pageSize = 1,
            long? scopeGroupId = null,
            string currentUrl = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Update Multiple S&U table by id (PUT).
        /// </summary>
        Task<GRTMultipleSUTableItem> UpdateMultipleSUTableAsync(
            long id,
            GRTMultipleSUTableUpdateRequest request,
            long? scopeGroupId = null,
            string currentUrl = null,
            CancellationToken cancellationToken = default);
    }
}

