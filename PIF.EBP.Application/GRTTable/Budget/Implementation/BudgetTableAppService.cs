using PIF.EBP.Core.GRTTable;
using PIF.EBP.Core.GRTTable.Budget.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PIF.EBP.Application.GRTTable.Budget.Implementation
{
    public class BudgetTableAppService : IBudgetTableAppService
    {
        private readonly IBudgetIntegrationService _budgetIntegrationService;

        public BudgetTableAppService(IBudgetIntegrationService budgetIntegrationService)
        {
            _budgetIntegrationService = budgetIntegrationService;
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

            return await _budgetIntegrationService.GetProjectOverviewsByCycleCompanyMapIdAsync(
                cycleCompanyMapId,
                page,
                pageSize,
                sort,
                scopeGroupId,
                currentUrl,
                cancellationToken);
        }

        public async Task<GRTBudgetTablesPagedResponse> GetGrtBudgetTablesAsync(
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

            return await _budgetIntegrationService.GetBudgetTablesByProjectOverviewIdAsync(
                projectOverviewId,
                page,
                pageSize,
                scopeGroupId,
                currentUrl,
                cancellationToken);
        }
    }
}

