using PIF.EBP.Core.DependencyInjection;
using PIF.EBP.Core.GRTTable.MultipleSandU.DTOs;
using System.Threading;
using System.Threading.Tasks;

namespace PIF.EBP.Application.GRTTable.MultipleSandU
{
    public interface IMultipleSUTableAppService : ITransientDependency
    {
        Task<GRTMultipleSUTablesPagedResponse> GetMultipleSUTablesAsync(
            long projectOverviewId,
            int page = 1,
            int pageSize = 1,
            long? scopeGroupId = null,
            string currentUrl = null,
            CancellationToken cancellationToken = default);

        Task<GRTMultipleSUTableItem> UpdateMultipleSUTableAsync(
            long id,
            GRTMultipleSUTableUpdateRequest request,
            long? scopeGroupId = null,
            string currentUrl = null,
            CancellationToken cancellationToken = default);
    }
}

