using PIF.EBP.Application.GRTTable.LandSale.DTOs;
using PIF.EBP.Core.DependencyInjection;
using PIF.EBP.Core.GRTTable.LandSale.DTOs;
using System.Threading;
using System.Threading.Tasks;

namespace PIF.EBP.Application.GRTTable.LandSale
{
    public interface ILandSaleAppService : ITransientDependency
    {

        Task<LandSalePagedDto> GetLandSaleTablesPagedAsync(
                     long projectOverviewId,
                     int page = 1,
                     int pageSize = 20,
                     string search = null,
                     CancellationToken cancellationToken = default);

        Task<LandSaleResponseDto> UpdateLandSaleTableAsync(
                     long id,
                     LandSaleTableRequest landSaleTable,
                     CancellationToken cancellationToken = default);


        Task<LandSaleTableCreateResponseDto> CreateLandSaleTableAsync(
        LandSaleTableCreateRequest table,
        CancellationToken cancellationToken = default);
    }
}
