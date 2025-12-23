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
            long? scopeGroupId = null,
            string currentUrl = null,
            CancellationToken cancellationToken = default);

        Task<GRTApprovedBPsPagedResponse> GetApprovedBPsAsync(
            long projectOverviewId,
            int page = 1,
            int pageSize = 1000,
            long? scopeGroupId = null,
            string currentUrl = null,
            CancellationToken cancellationToken = default);

        Task<GRTApprovedBPItem> CreateApprovedBPAsync(
            GRTApprovedBPCreateRequest request,
            long? scopeGroupId = null,
            string currentUrl = null,
            CancellationToken cancellationToken = default);
    }
}

