using PIF.EBP.Core.DependencyInjection;
using PIF.EBP.Core.GRTTable.LOI.DTOs;
using System.Threading.Tasks;

namespace PIF.EBP.Application.GRTTable.LOI_HMA
{
    public interface ILOIAppService : ITransientDependency
    {

        Task<LOIHMATablePagedResponse> GetLOIHMATablesPagedAsync(
                long projectOverviewId,
                int page,
                int pageSize,
                string search);

        Task<LOIHMATableItemResponse> GetLOIHMATableByIdAsync(long id);

        Task<LOIHMATableOperationResponse> CreateLOIHMATableAsync(LOIHMATableItemRequest dto);

        Task<LOIHMATableOperationResponse> UpdateLOIHMATableAsync(long id, LOIHMATableItemRequest dto);
        Task<bool> DeleteLOIHMATableAsync(long id);

    }
}
