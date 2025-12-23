using PIF.EBP.Core.GRTTable;
using PIF.EBP.Core.GRTTable.ApprovedBP.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PIF.EBP.Application.GRTTable.ApprovedBP.Implementation
{
    public class ApprovedBPAppService : IApprovedBPAppService
    {
        private readonly IApprovedBPIntegrationService _approvedBPIntegrationService;

        public ApprovedBPAppService(IApprovedBPIntegrationService approvedBPIntegrationService)
        {
            _approvedBPIntegrationService = approvedBPIntegrationService;
        }

        public async Task<GRTCycleCompanyMapItem> GetCycleCompanyMapByIdAsync(
            long id,
            long? scopeGroupId = null,
            string currentUrl = null,
            CancellationToken cancellationToken = default)
        {
            if (id <= 0)
            {
                throw new ArgumentException("Cycle company map ID must be greater than zero", nameof(id));
            }

            return await _approvedBPIntegrationService.GetCycleCompanyMapByIdAsync(
                id,
                scopeGroupId,
                currentUrl,
                cancellationToken);
        }

        public async Task<GRTApprovedBPsPagedResponse> GetApprovedBPsAsync(
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

            return await _approvedBPIntegrationService.GetApprovedBPsByProjectOverviewIdAsync(
                projectOverviewId,
                page,
                pageSize,
                scopeGroupId,
                currentUrl,
                cancellationToken);
        }

        public async Task<GRTApprovedBPItem> CreateApprovedBPAsync(
            GRTApprovedBPCreateRequest request,
            long? scopeGroupId = null,
            string currentUrl = null,
            CancellationToken cancellationToken = default)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request), "Request body is required");
            }

            if (request.ProjectOverviewId.HasValue && request.ProjectOverviewId.Value <= 0)
            {
                throw new ArgumentException("Project overview ID must be greater than zero", nameof(request.ProjectOverviewId));
            }

            return await _approvedBPIntegrationService.CreateApprovedBPAsync(
                request,
                scopeGroupId,
                currentUrl,
                cancellationToken);
        }
    }
}

