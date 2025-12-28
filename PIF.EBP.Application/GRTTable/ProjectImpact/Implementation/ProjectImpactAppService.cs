using PIF.EBP.Core.GRT;
using PIF.EBP.Core.GRTTable;
using PIF.EBP.Core.GRTTable.ProjectImpact.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PIF.EBP.Application.GRTTable.ProjectImpact.Implementation
{
    public class ProjectImpactAppService : IProjectImpactAppService
    {
        private readonly IProjectImpactIntegrationService _projectImpactIntegrationService;

        public ProjectImpactAppService(IProjectImpactIntegrationService projectImpactIntegrationService)
        {
            _projectImpactIntegrationService = projectImpactIntegrationService;
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

            return await _projectImpactIntegrationService.GetProjectOverviewsByCycleCompanyMapIdAsync(
                cycleCompanyMapId,
                page,
                pageSize,
                sort,
                scopeGroupId,
                currentUrl,
                cancellationToken);
        }

        public async Task<GRTProjectImpactPagedResponse> GetProjectImpactsByProjectOverviewIdAsync(
            long projectOverviewId,
            int page = 1,
            int pageSize = 1,
            string sort = "dateModified:desc",
            long? scopeGroupId = null,
            string currentUrl = null,
            CancellationToken cancellationToken = default)
        {
            if (projectOverviewId <= 0)
            {
                throw new ArgumentException("Project overview ID must be greater than zero", nameof(projectOverviewId));
            }

            return await _projectImpactIntegrationService.GetProjectImpactsByProjectOverviewIdAsync(
                projectOverviewId,
                page,
                pageSize,
                sort,
                scopeGroupId,
                currentUrl,
                cancellationToken);
        }

        public async Task<GRTProjectImpact> UpdateProjectImpactAsync(
            long id,
            GRTProjectImpactRequest request,
            long? scopeGroupId = null,
            string currentUrl = null,
            CancellationToken cancellationToken = default)
        {
            if (id <= 0)
            {
                throw new ArgumentException("Project impact ID must be greater than zero", nameof(id));
            }

            if (request == null)
            {
                throw new ArgumentNullException(nameof(request), "Update request cannot be null");
            }

            return await _projectImpactIntegrationService.UpdateProjectImpactAsync(
                id,
                request,
                scopeGroupId,
                currentUrl,
                cancellationToken);
        }
    }
}
