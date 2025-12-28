using PIF.EBP.Core.DependencyInjection;
using PIF.EBP.Core.GRT;
using PIF.EBP.Core.GRTTable;
using System.Threading;
using System.Threading.Tasks;

namespace PIF.EBP.Application.GRTTable.ProjectImpact
{
    public interface IProjectImpactAppService : ITransientDependency
    {
        Task<GRTProjectOverviewsPagedResponse> GetProjectOverviewsByCycleCompanyMapIdAsync(
            long cycleCompanyMapId,
            int page = 1,
            int pageSize = 1000,
            string sort = "dateModified:desc",
            long? scopeGroupId = null,
            string currentUrl = null,
            CancellationToken cancellationToken = default);

        Task<GRTProjectImpactPagedResponse> GetProjectImpactsByProjectOverviewIdAsync(
            long projectOverviewId,
            int page = 1,
            int pageSize = 1,
            string sort = "dateModified:desc",
            long? scopeGroupId = null,
            string currentUrl = null,
            CancellationToken cancellationToken = default);

        Task<GRTProjectImpact> UpdateProjectImpactAsync(
            long id,
            GRTProjectImpactRequest request,
            long? scopeGroupId = null,
            string currentUrl = null,
            CancellationToken cancellationToken = default);
    }
}
