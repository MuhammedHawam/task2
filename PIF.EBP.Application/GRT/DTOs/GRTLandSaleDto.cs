using PIF.EBP.Core.GRT;
using System;
using System.Collections.Generic;

namespace PIF.EBP.Application.GRT
{
    /// <summary>
    /// DTO for GRT Land Sale list item (simplified for list view)
    /// </summary>
    public class GRTLandSaleListDto
    {
        public long Id { get; set; }
        public string PlotName { get; set; }
        public string LandUse { get; set; }
        public string LandType { get; set; }
        public string City { get; set; }
        public string Region { get; set; }
    }

    /// <summary>
    /// DTO for creating/updating GRT Land Sale
    /// </summary>
    public class GRTLandSaleDto
    {
        public string PlotName { get; set; }
        public string LandUseKey { get; set; }
        public string LandTypeKey { get; set; }
        public string City { get; set; }
        public string RegionKey { get; set; }
        public string RestrictedDevelopmentToSpecificCriteriaKey { get; set; }
        public string SaleLeaseKey { get; set; }
        public double? NumberOfPlots { get; set; }
        public double? TotalLandArea { get; set; }
        public double? AvgSaleLeaseRate { get; set; }
        public string YearOfSaleLeaseStartKey { get; set; }
        public double? ValueOfInfrastructureAllocatedToLand { get; set; }

        // Relationships
        public long? ProjectToLandSaleRelationshipProjectOverviewId { get; set; }
        public string ProjectToLandSaleRelationshipProjectOverviewERC { get; set; }
    }

    /// <summary>
    /// Response DTO for GRT Land Sale creation/update
    /// </summary>
    public class GRTLandSaleResponseDto
    {
        public long Id { get; set; }
        public string ExternalReferenceCode { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateModified { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }
    }

    /// <summary>
    /// DTO for GRT Land Sale details (full details for get by ID)
    /// </summary>
    public class GRTLandSaleDetailDto
    {
        public long Id { get; set; }
        public string ExternalReferenceCode { get; set; }
        public DateTime? DateCreated { get; set; }
        public DateTime? DateModified { get; set; }

        public string PlotName { get; set; }
        public string LandUse { get; set; }
        public string LandUseKey { get; set; }
        public string LandType { get; set; }
        public string LandTypeKey { get; set; }
        public string City { get; set; }
        public string Region { get; set; }
        public string RegionKey { get; set; }
        public string RestrictedDevelopmentToSpecificCriteria { get; set; }
        public string RestrictedDevelopmentToSpecificCriteriaKey { get; set; }
        public string SaleLease { get; set; }
        public string SaleLeaseKey { get; set; }
        public double? NumberOfPlots { get; set; }
        public double? TotalLandArea { get; set; }
        public double? AvgSaleLeaseRate { get; set; }
        public string YearOfSaleLeaseStart { get; set; }
        public string YearOfSaleLeaseStartKey { get; set; }
        public double? ValueOfInfrastructureAllocatedToLand { get; set; }

        // Relationships
        public long? ProjectToLandSaleRelationshipProjectOverviewId { get; set; }
        public string ProjectToLandSaleRelationshipProjectOverviewERC { get; set; }

        public List<GRTAuditEvent> AuditEvents { get; set; }
    }

    /// <summary>
    /// Paginated response for GRT Land Sales list
    /// </summary>
    public class GRTLandSalesPagedDto
    {
        public List<GRTLandSaleListDto> Items { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int LastPage { get; set; }
    }
}
