using System;
using System.Collections.Generic;

namespace PIF.EBP.Application.GRT
{
    /// <summary>
    /// DTO for GRT Multiple S&U list item (simplified for list view)
    /// </summary>
    public class GRTMultipleSandUListDto
    {
        public long Id { get; set; }
        public string ExternalReferenceCode { get; set; }
        public string Region { get; set; }
        public string RegionKey { get; set; }
        public double? CapexTotal { get; set; }
        public double? OpexTotal { get; set; }
        public double? SourcesTotal { get; set; }
        public double? FinancialsTotal { get; set; }
    }

    /// <summary>
    /// DTO for creating/updating GRT Multiple S&U
    /// </summary>
    public class GRTMultipleSandUDto
    {
        public long? Id { get; set; }
        public string ExternalReferenceCode { get; set; }
        public DateTime? DateCreated { get; set; }
        public DateTime? DateModified { get; set; }

        // Region
        public string RegionKey { get; set; }

        // JSON data fields for financial planning
        public string CapexJSON { get; set; }
        public string OpexJSON { get; set; }
        public string TotalSourcesJSON { get; set; }
        public string FinancialsSARJSON { get; set; }

        // Relationships
        public long? ProjectToMultipleSandURelationshipProjectOverviewId { get; set; }
        public string ProjectToMultipleSandURelationshipProjectOverviewERC { get; set; }
    }

    /// <summary>
    /// Response DTO for GRT Multiple S&U creation/update
    /// </summary>
    public class GRTMultipleSandUResponseDto
    {
        public long Id { get; set; }
        public string ExternalReferenceCode { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateModified { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }
    }

    /// <summary>
    /// DTO for GRT Multiple S&U details (full details for get by ID)
    /// </summary>
    public class GRTMultipleSandUDetailDto
    {
        public long Id { get; set; }
        public string ExternalReferenceCode { get; set; }
        public DateTime? DateCreated { get; set; }
        public DateTime? DateModified { get; set; }

        // Region
        public string Region { get; set; }
        public string RegionKey { get; set; }

        // JSON data fields for financial planning
        public string CapexJSON { get; set; }
        public string OpexJSON { get; set; }
        public string TotalSourcesJSON { get; set; }
        public string FinancialsSARJSON { get; set; }

        // Relationships
        public long? ProjectToMultipleSandURelationshipProjectOverviewId { get; set; }
        public string ProjectToMultipleSandURelationshipProjectOverviewERC { get; set; }
    }

    /// <summary>
    /// Paginated response for GRT Multiple S&U list
    /// </summary>
    public class GRTMultipleSandUsPagedDto
    {
        public List<GRTMultipleSandUListDto> Items { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int LastPage { get; set; }
    }
}
