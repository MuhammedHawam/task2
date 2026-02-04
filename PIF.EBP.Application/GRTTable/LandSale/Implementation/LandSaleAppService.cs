using PIF.EBP.Application.GRTTable.LandSale.DTOs;
using PIF.EBP.Core.GRTTable.LandSale;
using PIF.EBP.Core.GRTTable.LandSale.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PIF.EBP.Application.GRTTable.LandSale.Implementation
{
    public class LandSaleAppService :ILandSaleAppService
    {

        private readonly ILandSaleIntegrationService _landSaleIntegrationService;

        public LandSaleAppService(ILandSaleIntegrationService landSaleIntegrationService)
        {
            _landSaleIntegrationService = landSaleIntegrationService;
        }
        public async Task<LandSalePagedDto> GetLandSaleTablesPagedAsync(
                     long projectOverviewId,
                     int page = 1,
                     int pageSize = 20,
                     string search = null,
                     CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _landSaleIntegrationService
                    .GetLandSaleTablesPagedAsync(projectOverviewId, page, pageSize, search, cancellationToken);

                if (response?.Items == null)
                {
                    return new LandSalePagedDto
                    {
                        Items = new List<LandSaleDto>(),
                        Page = page,
                        PageSize = pageSize,
                        TotalCount = 0,
                        LastPage = 1
                    };
                }

                return new LandSalePagedDto
                {
                    Page = response.Page,
                    PageSize = response.PageSize,
                    TotalCount = response.TotalCount,
                    LastPage = response.LastPage,
                    Items = response.Items.Select(item => new LandSaleDto
                    {
                        Id = item.Id,
                        ExternalReferenceCode = item.ExternalReferenceCode,
                        DateCreated = DateTime.TryParse(item.DateCreated, out var dc) ? dc : (DateTime?)null,
                        DateModified = DateTime.TryParse(item.DateModified, out var dm) ? dm : (DateTime?)null,
                        LandSales = item.LandSales,
                        ProjectToLandSaleTableRelationshipProjectOverviewId =
                            item.ProjectToLandSaleTableRelationshipProjectOverviewId,
                        ProjectToLandSaleTableRelationshipProjectOverviewERC =
                            item.ProjectToLandSaleTableRelationshipProjectOverviewERC,
                        AuditEvents = item.AuditEvents
                    }).ToList()
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError(
                    $"Error in GRTAppService.GetLandSaleTablesPagedAsync: {ex.Message}");
                throw;
            }
        }


        public async Task<LandSaleResponseDto> UpdateLandSaleTableAsync(
                     long id,
                     LandSaleTableRequest landSaleTable,
                     CancellationToken cancellationToken = default)
        {
            if (id <= 0)
                throw new ArgumentException("Land sale table ID must be greater than zero", nameof(id));

            if (landSaleTable == null)
                throw new ArgumentNullException(nameof(landSaleTable));

            try
            {
              

                var response = await _landSaleIntegrationService
                    .UpdateLandSaleTableAsync(id, landSaleTable, cancellationToken);

                return new LandSaleResponseDto
                {
                    Id = response.Id,
                    ExternalReferenceCode = response.ExternalReferenceCode,
                    DateCreated = DateTime.TryParse(response.DateCreated, out var dc) ? dc : DateTime.Now,
                    DateModified = DateTime.TryParse(response.DateModified, out var dm) ? dm : DateTime.Now,
                    Success = true,
                    Message = "Land sale table updated successfully"
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError(
                    $"Error in GRTAppService.UpdateLandSaleTableAsync: {ex.Message}");

                return new LandSaleResponseDto
                {
                    Success = false,
                    Message = $"Error updating land sale table: {ex.Message}"
                };
            }
        }


        public async Task<LandSaleTableCreateResponseDto> CreateLandSaleTableAsync(
        LandSaleTableCreateRequest table,
        CancellationToken cancellationToken = default)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));

            try
            {
               

                var response = await _landSaleIntegrationService.CreateLandSaleTableAsync(table, cancellationToken);

                return new LandSaleTableCreateResponseDto
                {
                    Id = response.Id,
                    ExternalReferenceCode = response.ExternalReferenceCode,
                    DateCreated = DateTime.TryParse(response.DateCreated, out var created) ? created : DateTime.Now,
                    DateModified = DateTime.TryParse(response.DateModified, out var modified) ? modified : DateTime.Now,
                    Success = true,
                    Message = "Land sale table created successfully"
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"Error creating land sale table: {ex.Message}");
                return new LandSaleTableCreateResponseDto
                {
                    Success = false,
                    Message = $"Error creating land sale table: {ex.Message}"
                };
            }
        }

 




        }
    }
