using PIF.EBP.Core.DependencyInjection;
using PIF.EBP.Core.GRTTable.LOI.DTOs;
using System.Threading;
using System.Threading.Tasks;

namespace PIF.EBP.Core.GRTTable.LOI.Interfaces
{
    public interface ILOIHMAIntegrationService : ITransientDependency
    {

        Task<LOIHMATablePagedResponse> GetLOIHMATablesPagedAsync(
    long projectOverviewId,
    int page,
    int pageSize,
    string search,
    CancellationToken cancellationToken = default);

        Task<LOIHMATableItemResponse> GetLOIHMATableByIdAsync(
            long id,
            CancellationToken cancellationToken = default);

        Task<LOIHMATableOperationResponse> CreateLOIHMATableAsync(
            LOIHMATableItemRequest dto,
            CancellationToken cancellationToken = default);

        Task<LOIHMATableOperationResponse> UpdateLOIHMATableAsync(
            long id,
            LOIHMATableItemRequest dto,
            CancellationToken cancellationToken = default);

        Task<bool> DeleteLOIHMATableAsync(
            long id,
            CancellationToken cancellationToken = default);


    }

}
