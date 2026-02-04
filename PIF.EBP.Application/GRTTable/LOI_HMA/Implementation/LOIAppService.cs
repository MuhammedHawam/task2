using PIF.EBP.Core.GRTTable.LOI.DTOs;
using PIF.EBP.Core.GRTTable.LOI.Interfaces;
using System.Threading.Tasks;


namespace PIF.EBP.Application.GRTTable.LOI_HMA.Implementation
{
    public class LOIAppService : ILOIAppService
    {


            private readonly ILOIHMAIntegrationService _integration;

            public LOIAppService(ILOIHMAIntegrationService integration)
            {
                _integration = integration;
            }

            public Task<LOIHMATablePagedResponse> GetLOIHMATablesPagedAsync(
                long projectOverviewId,
                int page,
                int pageSize,
                string search)
                => _integration.GetLOIHMATablesPagedAsync(projectOverviewId, page, pageSize, search);

            public Task<LOIHMATableItemResponse> GetLOIHMATableByIdAsync(long id)
                => _integration.GetLOIHMATableByIdAsync(id);

            public Task<LOIHMATableOperationResponse> CreateLOIHMATableAsync(LOIHMATableItemRequest dto)
                => _integration.CreateLOIHMATableAsync(dto);

            public Task<LOIHMATableOperationResponse> UpdateLOIHMATableAsync(long id, LOIHMATableItemRequest dto)
                => _integration.UpdateLOIHMATableAsync(id, dto);

            public Task<bool> DeleteLOIHMATableAsync(long id)
                => _integration.DeleteLOIHMATableAsync(id);
        }



    }

