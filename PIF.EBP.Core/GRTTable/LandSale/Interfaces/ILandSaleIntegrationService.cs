using PIF.EBP.Core.DependencyInjection;
using PIF.EBP.Core.GRTTable.LandSale.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PIF.EBP.Core.GRTTable.LandSale
{
    public interface ILandSaleIntegrationService : ITransientDependency
    {
        Task<LandSaleTablesPagedResponse> GetLandSaleTablesPagedAsync(
                                          long projectOverviewId,
                                          int page = 1,
                                          int pageSize = 20,
                                          string search = null,
                                          CancellationToken cancellationToken = default);

        Task<LandSaleTableResponse> UpdateLandSaleTableAsync(
                                    long id,
                                    LandSaleTableRequest request,
                                    CancellationToken cancellationToken = default);

        Task<LandSaleTableCreateResponse> CreateLandSaleTableAsync(
                                          LandSaleTableCreateRequest request,
                                          CancellationToken cancellationToken = default);
    }
}
