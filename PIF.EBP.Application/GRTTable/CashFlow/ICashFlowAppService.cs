using PIF.EBP.Core.DependencyInjection;
using PIF.EBP.Core.GRT;
using PIF.EBP.Core.GRTTable;
using System.Threading;
using System.Threading.Tasks;

namespace PIF.EBP.Application.GRTTable.CashFlow
{
    public interface ICashFlowAppService : ITransientDependency
    {
        Task<GRTProjectOverviewsPagedResponse> GetProjectOverviewsByCycleCompanyMapIdAsync(
            long cycleCompanyMapId,
            int page = 1,
            int pageSize = 1000,
            string sort = "dateModified:desc",
            long? scopeGroupId = null,
            string currentUrl = null,
            CancellationToken cancellationToken = default);

        Task<GRTCashflowsPagedResponse> GetCashflowsByProjectOverviewIdAsync(
            long projectOverviewId,
            int page = 1,
            int pageSize = 1000,
            long? scopeGroupId = null,
            string currentUrl = null,
            CancellationToken cancellationToken = default);

        Task<GRTCashflow> UpdateCashflowAsync(
            long id,
            GRTCashflowRequest request,
            long? scopeGroupId = null,
            string currentUrl = null,
            CancellationToken cancellationToken = default);
    }
}
