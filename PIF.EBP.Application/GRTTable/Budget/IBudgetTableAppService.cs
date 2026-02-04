using PIF.EBP.Core.DependencyInjection;
using PIF.EBP.Core.GRTTable;
using System.Threading;
using System.Threading.Tasks;

namespace PIF.EBP.Application.GRTTable.Budget
{
    public interface IBudgetTableAppService : ITransientDependency
    {
        Task<GRTProjectOverviewsPagedResponse> GetProjectOverviewsByCycleCompanyMapIdAsync(
            long cycleCompanyMapId,
            int page = 1,
            int pageSize = 1000,
            string sort = "dateModified:desc",
            CancellationToken cancellationToken = default);

        Task<GRTBudgetTablesPagedResponse> GetGrtBudgetTablesAsync(
            long projectOverviewId,
            int page = 1,
            int pageSize = 1,
            CancellationToken cancellationToken = default);

        Task<GRTBudgetTableItem> UpdateGrtBudgetTableAsync(
            long id,
            GRTBudgetTableUpdateRequest request,
            CancellationToken cancellationToken = default);
    }
}

