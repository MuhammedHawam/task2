using PIF.EBP.Application.GRTTable.DeliveryPlan;
using PIF.EBP.Core.GRT;
using PIF.EBP.Core.GRTTable;
using PIF.EBP.Core.GRTTable.DeliveryPlan.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PIF.EBP.Application.GRTTable
{

    public class DeliveryPlanAppService: IDeliveryPlanAppService
    {
        private readonly IDeliveryPlanIntegrationService _deliveryPlanIntegrationService;

        public DeliveryPlanAppService(IDeliveryPlanIntegrationService deliveryPlanIntegrationService)
        {
            _deliveryPlanIntegrationService = deliveryPlanIntegrationService;
        }
        public async Task<DeliveryPlanPagedDto> GetDeliveryPlanTablesPagedAsync(
     long projectOverviewId,
     int page = 1,
     int pageSize = 20,
     string search = null,
     CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _deliveryPlanIntegrationService.GetDeliveryPlanTablesPagedAsync(
                    projectOverviewId,
                    page,
                    pageSize,
                    search,
                    cancellationToken);

                if (response == null || response.Items == null)
                {
                    return new DeliveryPlanPagedDto
                    {
                        Items = new List<DeliveryPlanDto>(),
                        Page = page,
                        PageSize = pageSize,
                        TotalCount = 0,
                        LastPage = 1
                    };
                }

                // Map integration DTOs to application DTOs
                var result = new DeliveryPlanPagedDto
                {
                    Page = response.Page,
                    PageSize = response.PageSize,
                    TotalCount = response.TotalCount,
                    LastPage = response.LastPage,
                    Items = response.Items.Select(table => new DeliveryPlanDto
                    {
                        Id = table.Id,
                        ExternalReferenceCode = table.ExternalReferenceCode,
                        DateCreated = DateTime.TryParse(table.DateCreated, out var dateCreated) ? dateCreated : (DateTime?)null,
                        DateModified = DateTime.TryParse(table.DateModified, out var dateModified) ? dateModified : (DateTime?)null,
                        DeliveryPlan = table.DeliveryPlan,
                        ProjectToDeliveryPlanTableRelationshipProjectOverviewId = table.ProjectToDeliveryPlanTableRelationshipProjectOverviewId,
                        ProjectToDeliveryPlanTableRelationshipProjectOverviewERC = table.ProjectToDeliveryPlanTableRelationshipProjectOverviewERC,
                        AuditEvents = table.AuditEvents
                    }).ToList()
                };

                return result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"Error in GRTAppService.GetDeliveryPlanPagedAsync: {ex.Message}");
                throw;
            }
        }

        public async Task<DeliveryPlanDto> GetDeliveryPlanTableByIdAsync(
            long id,
            CancellationToken cancellationToken = default)
        {
            if (id <= 0)
            {
                throw new ArgumentException("Delivery plan table ID must be greater than zero", nameof(id));
            }

            try
            {
                var response = await _deliveryPlanIntegrationService.GetDeliveryPlanTableByIdAsync(
                    id,
                    cancellationToken);

                if (response == null)
                {
                    return null;
                }

                // Map integration response DTO to application DTO
                var result = new DeliveryPlanDto
                {
                    Id = response.Id,
                    ExternalReferenceCode = response.ExternalReferenceCode,
                    DateCreated = DateTime.TryParse(response.DateCreated, out var dateCreated) ? dateCreated : (DateTime?)null,
                    DateModified = DateTime.TryParse(response.DateModified, out var dateModified) ? dateModified : (DateTime?)null,
                    DeliveryPlan = response.DeliveryPlan,
                    ProjectToDeliveryPlanTableRelationshipProjectOverviewId = response.ProjectToDeliveryPlanTableRelationshipProjectOverviewId,
                    ProjectToDeliveryPlanTableRelationshipProjectOverviewERC = response.ProjectToDeliveryPlanTableRelationshipProjectOverviewERC,
                    AuditEvents = response.AuditEvents
                };

                return result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"Error in GRTAppService.GetDeliveryPlanTableByIdAsync: {ex.Message}");
                throw;
            }
        }

        public async Task<DeliveryPlanResponseDto> CreateDeliveryPlanTableAsync(
            DeliveryPlanDto deliveryPlanTable,
            CancellationToken cancellationToken = default)
        {
            if (deliveryPlanTable == null)
            {
                throw new ArgumentNullException(nameof(deliveryPlanTable), "Delivery plan table cannot be null");
            }

            try
            {
                // Map application DTO to integration request DTO
                var request = new DeliveryPlanTableRequest
                {
                    DeliveryPlan = deliveryPlanTable.DeliveryPlan,
                    ProjectToDeliveryPlanTableRelationshipProjectOverviewId = deliveryPlanTable.ProjectToDeliveryPlanTableRelationshipProjectOverviewId,
                    ProjectToDeliveryPlanTableRelationshipProjectOverviewERC = deliveryPlanTable.ProjectToDeliveryPlanTableRelationshipProjectOverviewERC
                };

                var response = await _deliveryPlanIntegrationService.CreateDeliveryPlanTableAsync(request, cancellationToken);

                return new DeliveryPlanResponseDto
                {
                    Id = response.Id,
                    ExternalReferenceCode = response.ExternalReferenceCode,
                    DateCreated = DateTime.TryParse(response.DateCreated, out var dateCreated) ? dateCreated : DateTime.Now,
                    DateModified = DateTime.TryParse(response.DateModified, out var dateModified) ? dateModified : DateTime.Now,
                    Success = true,
                    Message = "Delivery plan table created successfully"
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"Error in GRTAppService.CreateDeliveryPlanTableAsync: {ex.Message}");
                return new DeliveryPlanResponseDto
                {
                    Success = false,
                    Message = $"Error creating delivery plan table: {ex.Message}"
                };
            }
        }

        public async Task<DeliveryPlanResponseDto> UpdateDeliveryPlanTableAsync(
            long id,
            DeliveryPlanDto deliveryPlanTable,
            CancellationToken cancellationToken = default)
        {
            if (id <= 0)
            {
                throw new ArgumentException("Delivery plan table ID must be greater than zero", nameof(id));
            }

            if (deliveryPlanTable == null)
            {
                throw new ArgumentNullException(nameof(deliveryPlanTable), "Delivery plan table cannot be null");
            }

            try
            {
                // Map application DTO to integration request DTO
                var request = new DeliveryPlanTableRequest
                {
                    DeliveryPlan = deliveryPlanTable.DeliveryPlan,
                    ProjectToDeliveryPlanTableRelationshipProjectOverviewId = deliveryPlanTable.ProjectToDeliveryPlanTableRelationshipProjectOverviewId,
                    ProjectToDeliveryPlanTableRelationshipProjectOverviewERC = deliveryPlanTable.ProjectToDeliveryPlanTableRelationshipProjectOverviewERC
                };

                var response = await _deliveryPlanIntegrationService.UpdateDeliveryPlanTableAsync(id, request, cancellationToken);

                return new DeliveryPlanResponseDto
                {
                    Id = response.Id,
                    ExternalReferenceCode = response.ExternalReferenceCode,
                    DateCreated = DateTime.TryParse(response.DateCreated, out var dateCreated) ? dateCreated : DateTime.Now,
                    DateModified = DateTime.TryParse(response.DateModified, out var dateModified) ? dateModified : DateTime.Now,
                    Success = true,
                    Message = "Delivery plan table updated successfully"
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"Error in GRTAppService.UpdateDeliveryPlanTableAsync: {ex.Message}");
                return new DeliveryPlanResponseDto
                {
                    Success = false,
                    Message = $"Error updating delivery plan table: {ex.Message}"
                };
            }
        }

        public async Task<bool> DeleteDeliveryPlanTableAsync(
            long id,
            CancellationToken cancellationToken = default)
        {
            if (id <= 0)
            {
                throw new ArgumentException("Delivery plan table ID must be greater than zero", nameof(id));
            }

            try
            {
                return await _deliveryPlanIntegrationService.DeleteDeliveryPlanTableAsync(id, cancellationToken);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"Error in GRTAppService.DeleteDeliveryPlanTableAsync: {ex.Message}");
                throw;
            }
        }
    }
}
