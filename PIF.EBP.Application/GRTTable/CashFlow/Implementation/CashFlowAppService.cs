using PIF.EBP.Core.GRT;
using PIF.EBP.Core.GRTTable;
using PIF.EBP.Core.GRTTable.CashFlow.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PIF.EBP.Application.GRTTable.CashFlow.Implementation
{
    public class CashFlowAppService : ICashFlowAppService
    {
        private readonly ICashFlowIntegrationService _cashFlowIntegrationService;

        public CashFlowAppService(ICashFlowIntegrationService cashFlowIntegrationService)
        {
            _cashFlowIntegrationService = cashFlowIntegrationService;
        }

        public async Task<GRTProjectOverviewsPagedResponse> GetProjectOverviewsByCycleCompanyMapIdAsync(
            long cycleCompanyMapId,
            int page = 1,
            int pageSize = 1000,
            string sort = "dateModified:desc",
            long? scopeGroupId = null,
            string currentUrl = null,
            CancellationToken cancellationToken = default)
        {
            if (cycleCompanyMapId <= 0)
            {
                throw new ArgumentException("Cycle company map ID must be greater than zero", nameof(cycleCompanyMapId));
            }

            return await _cashFlowIntegrationService.GetProjectOverviewsByCycleCompanyMapIdAsync(
                cycleCompanyMapId,
                page,
                pageSize,
                sort,
                scopeGroupId,
                currentUrl,
                cancellationToken);
        }

        public async Task<GRTCashflowsPagedResponse> GetCashflowsByProjectOverviewIdAsync(
            long projectOverviewId,
            int page = 1,
            int pageSize = 1000,
            long? scopeGroupId = null,
            string currentUrl = null,
            CancellationToken cancellationToken = default)
        {
            if (projectOverviewId <= 0)
            {
                throw new ArgumentException("Project overview ID must be greater than zero", nameof(projectOverviewId));
            }

            return await _cashFlowIntegrationService.GetCashflowsByProjectOverviewIdAsync(
                projectOverviewId,
                page,
                pageSize,
                scopeGroupId,
                currentUrl,
                cancellationToken);
        }

        public async Task<GRTCashflow> UpdateCashflowAsync(
            long id,
            GRTCashflowRequest request,
            long? scopeGroupId = null,
            string currentUrl = null,
            CancellationToken cancellationToken = default)
        {
            if (id <= 0)
            {
                throw new ArgumentException("Cashflow ID must be greater than zero", nameof(id));
            }

            if (request == null)
            {
                throw new ArgumentNullException(nameof(request), "Update request cannot be null");
            }

            return await _cashFlowIntegrationService.UpdateCashflowAsync(
                id,
                request,
                scopeGroupId,
                currentUrl,
                cancellationToken);
        }
    }
}
