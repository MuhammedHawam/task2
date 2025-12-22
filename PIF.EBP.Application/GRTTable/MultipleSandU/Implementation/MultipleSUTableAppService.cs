using PIF.EBP.Core.GRTTable.MultipleSandU.DTOs;
using PIF.EBP.Core.GRTTable.MultipleSandU.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PIF.EBP.Application.GRTTable.MultipleSandU.Implementation
{
    public class MultipleSUTableAppService : IMultipleSUTableAppService
    {
        private readonly IMultipleSUTableIntegrationService _multipleSUTableIntegrationService;

        public MultipleSUTableAppService(IMultipleSUTableIntegrationService multipleSUTableIntegrationService)
        {
            _multipleSUTableIntegrationService = multipleSUTableIntegrationService;
        }

        public async Task<GRTMultipleSUTablesPagedResponse> GetMultipleSUTablesAsync(
            long projectOverviewId,
            int page = 1,
            int pageSize = 1,
            long? scopeGroupId = null,
            string currentUrl = null,
            CancellationToken cancellationToken = default)
        {
            if (projectOverviewId <= 0)
            {
                throw new ArgumentException("Project overview ID must be greater than zero", nameof(projectOverviewId));
            }

            return await _multipleSUTableIntegrationService.GetMultipleSUTablesByProjectOverviewIdAsync(
                projectOverviewId,
                page,
                pageSize,
                scopeGroupId,
                currentUrl,
                cancellationToken);
        }

        public async Task<GRTMultipleSUTableItem> UpdateMultipleSUTableAsync(
            long id,
            GRTMultipleSUTableUpdateRequest request,
            long? scopeGroupId = null,
            string currentUrl = null,
            CancellationToken cancellationToken = default)
        {
            if (id <= 0)
            {
                throw new ArgumentException("Multiple S&U table ID must be greater than zero", nameof(id));
            }

            if (request == null)
            {
                throw new ArgumentNullException(nameof(request), "Update request cannot be null");
            }

            return await _multipleSUTableIntegrationService.UpdateMultipleSUTableAsync(
                id,
                request,
                scopeGroupId,
                currentUrl,
                cancellationToken);
        }
    }
}

