using PIF.EBP.Application.GRTTable.InfraDeliveryPlan;
using PIF.EBP.Core.GRTTable;
using PIF.EBP.Core.GRTTable.DeliveryPlan.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PIF.EBP.Application.GRTTable
{
    public class InfraDeliveryPlanAppService : IInfraDeliveryPlanAppService
    {
        private readonly IInfraDeliveryPlanIntegrationService _infraDeliveryPlanIntegrationService;

        public InfraDeliveryPlanAppService(IInfraDeliveryPlanIntegrationService infraDeliveryPlanIntegrationService)
        {
            _infraDeliveryPlanIntegrationService = infraDeliveryPlanIntegrationService;
        }
        public async Task<InfraDeliveryPlanPagedDto> GetInfraDeliveryPlanTablesPagedAsync(
     long projectOverviewId,
     int page = 1,
     int pageSize = 20,
     string search = null,
     CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _infraDeliveryPlanIntegrationService.GetInfraDeliveryPlanTablesPagedAsync(
                    projectOverviewId, page, pageSize, search, cancellationToken);

                if (response == null || response.Items == null)
                {
                    return new InfraDeliveryPlanPagedDto
                    {
                        Items = new List<InfraDeliveryPlanDto>(),
                        Page = page,
                        PageSize = pageSize,
                        TotalCount = 0,
                        LastPage = 1
                    };
                }

                return new InfraDeliveryPlanPagedDto
                {
                    Page = response.Page,
                    PageSize = response.PageSize,
                    TotalCount = response.TotalCount,
                    LastPage = response.LastPage,
                    Items = response.Items.Select(table => new InfraDeliveryPlanDto
                    {
                        Id = table.Id,
                        ExternalReferenceCode = table.ExternalReferenceCode,
                        DateCreated = DateTime.TryParse(table.DateCreated, out var dateCreated) ? dateCreated : (DateTime?)null,
                        DateModified = DateTime.TryParse(table.DateModified, out var dateModified) ? dateModified : (DateTime?)null,
                        InfraDeliveryPlanTableJson = table.InfraDeliveryPlanTableJson,
                        ProjectToInfraDeliveryPlanRelationshipTabProjectOverviewId = table.ProjectToInfraDeliveryPlanRelationshipTabProjectOverviewId,
                        ProjectToInfraDeliveryPlanRelationshipTabProjectOverviewERC = table.ProjectToInfraDeliveryPlanRelationshipTabProjectOverviewERC,
                        AuditEvents = table.AuditEvents
                    }).ToList()
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"Error in GRTAppService.GetInfraDeliveryPlanPagedAsync: {ex.Message}");
                throw;
            }
        }

        public async Task<InfraDeliveryPlanDto> GetInfraDeliveryPlanTableByIdAsync(
            long id,
            CancellationToken cancellationToken = default)
        {
            if (id <= 0)
            {
                throw new ArgumentException("Infrastructure delivery plan table ID must be greater than zero", nameof(id));
            }

            try
            {
                var response = await _infraDeliveryPlanIntegrationService.GetInfraDeliveryPlanTableByIdAsync(id, cancellationToken);
                if (response == null) return null;

                return new InfraDeliveryPlanDto
                {
                    Id = response.Id,
                    ExternalReferenceCode = response.ExternalReferenceCode,
                    DateCreated = DateTime.TryParse(response.DateCreated, out var dateCreated) ? dateCreated : (DateTime?)null,
                    DateModified = DateTime.TryParse(response.DateModified, out var dateModified) ? dateModified : (DateTime?)null,
                    InfraDeliveryPlanTableJson = response.InfraDeliveryPlanTableJson,
                    ProjectToInfraDeliveryPlanRelationshipTabProjectOverviewId = response.ProjectToInfraDeliveryPlanRelationshipTabProjectOverviewId,
                    ProjectToInfraDeliveryPlanRelationshipTabProjectOverviewERC = response.ProjectToInfraDeliveryPlanRelationshipTabProjectOverviewERC,
                    AuditEvents = response.AuditEvents
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"Error in GRTAppService.GetInfraDeliveryPlanTableByIdAsync: {ex.Message}");
                throw;
            }
        }

        public async Task<InfraDeliveryPlanResponseDto> CreateInfraDeliveryPlanTableAsync(
            InfraDeliveryPlanDto infraDeliveryPlanTable,
            CancellationToken cancellationToken = default)
        {
            if (infraDeliveryPlanTable == null)
            {
                throw new ArgumentNullException(nameof(infraDeliveryPlanTable), "Infrastructure delivery plan table cannot be null");
            }

            try
            {
                var request = new InfraDeliveryPlanTableRequest
                {
                    InfraDeliveryPlanTableJson = infraDeliveryPlanTable.InfraDeliveryPlanTableJson,
                    ProjectToInfraDeliveryPlanRelationshipTabProjectOverviewId = infraDeliveryPlanTable.ProjectToInfraDeliveryPlanRelationshipTabProjectOverviewId,
                    ProjectToInfraDeliveryPlanRelationshipTabProjectOverviewERC = infraDeliveryPlanTable.ProjectToInfraDeliveryPlanRelationshipTabProjectOverviewERC
                };

                var response = await _infraDeliveryPlanIntegrationService.CreateInfraDeliveryPlanTableAsync(request, cancellationToken);

                return new InfraDeliveryPlanResponseDto
                {
                    Id = response.Id,
                    ExternalReferenceCode = response.ExternalReferenceCode,
                    DateCreated = DateTime.TryParse(response.DateCreated, out var dateCreated) ? dateCreated : DateTime.Now,
                    DateModified = DateTime.TryParse(response.DateModified, out var dateModified) ? dateModified : DateTime.Now,
                    Success = true,
                    Message = "Infrastructure delivery plan table created successfully"
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"Error in GRTAppService.CreateInfraDeliveryPlanTableAsync: {ex.Message}");
                return new InfraDeliveryPlanResponseDto
                {
                    Success = false,
                    Message = $"Error creating infrastructure delivery plan table: {ex.Message}"
                };
            }
        }

        public async Task<InfraDeliveryPlanResponseDto> UpdateInfraDeliveryPlanTableAsync(
            long id,
            InfraDeliveryPlanDto infraDeliveryPlanTable,
            CancellationToken cancellationToken = default)
        {
            if (id <= 0)
            {
                throw new ArgumentException("Infrastructure delivery plan table ID must be greater than zero", nameof(id));
            }

            if (infraDeliveryPlanTable == null)
            {
                throw new ArgumentNullException(nameof(infraDeliveryPlanTable), "Infrastructure delivery plan table cannot be null");
            }

            try
            {
                var request = new InfraDeliveryPlanTableRequest
                {
                    InfraDeliveryPlanTableJson = infraDeliveryPlanTable.InfraDeliveryPlanTableJson,
                    ProjectToInfraDeliveryPlanRelationshipTabProjectOverviewId = infraDeliveryPlanTable.ProjectToInfraDeliveryPlanRelationshipTabProjectOverviewId,
                    ProjectToInfraDeliveryPlanRelationshipTabProjectOverviewERC = infraDeliveryPlanTable.ProjectToInfraDeliveryPlanRelationshipTabProjectOverviewERC
                };

                var response = await _infraDeliveryPlanIntegrationService.UpdateInfraDeliveryPlanTableAsync(id, request, cancellationToken);

                return new InfraDeliveryPlanResponseDto
                {
                    Id = response.Id,
                    ExternalReferenceCode = response.ExternalReferenceCode,
                    DateCreated = DateTime.TryParse(response.DateCreated, out var dateCreated) ? dateCreated : DateTime.Now,
                    DateModified = DateTime.TryParse(response.DateModified, out var dateModified) ? dateModified : DateTime.Now,
                    Success = true,
                    Message = "Infrastructure delivery plan table updated successfully"
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"Error in GRTAppService.UpdateInfraDeliveryPlanTableAsync: {ex.Message}");
                return new InfraDeliveryPlanResponseDto
                {
                    Success = false,
                    Message = $"Error updating infrastructure delivery plan table: {ex.Message}"
                };
            }
        }

        public async Task<bool> DeleteInfraDeliveryPlanTableAsync(
            long id,
            CancellationToken cancellationToken = default)
        {
            if (id <= 0)
            {
                throw new ArgumentException("Infrastructure delivery plan table ID must be greater than zero", nameof(id));
            }

            try
            {
                return await _infraDeliveryPlanIntegrationService.DeleteInfraDeliveryPlanTableAsync(id, cancellationToken);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"Error in GRTAppService.DeleteInfraDeliveryPlanTableAsync: {ex.Message}");
                throw;
            }
        }
    }
}
