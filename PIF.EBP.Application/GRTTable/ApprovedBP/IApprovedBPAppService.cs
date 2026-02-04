using PIF.EBP.Core.DependencyInjection;
using PIF.EBP.Core.GRTTable;
using System.Threading;
using System.Threading.Tasks;

namespace PIF.EBP.Application.GRTTable.ApprovedBP
{
    public interface IApprovedBPAppService : ITransientDependency
    {
        Task<GRTCycleCompanyMapItem> GetCycleCompanyMapByIdAsync(
            long id,
            
            CancellationToken cancellationToken = default);

        Task<GRTApprovedBPsPagedResponse> GetApprovedBPsAsync(
            long projectOverviewId,
            int page = 1,
            int pageSize = 1000,
            CancellationToken cancellationToken = default);

        Task<GRTApprovedBPItem> CreateApprovedBPAsync(
            GRTApprovedBPCreateRequest request,
            CancellationToken cancellationToken = default);
    }
}

