using PIF.EBP.Core.GRT;
using System;
using System.Collections.Generic;

namespace PIF.EBP.Application.GRT
{
    /// <summary>
    /// DTO for GRT Cashflow data
    /// </summary>
    public class GRTCashflowDto
    {
        public long Id { get; set; }
        public string ExternalReferenceCode { get; set; }
        public DateTime? DateCreated { get; set; }
        public DateTime? DateModified { get; set; }
        public long? ProjectOverviewId { get; set; }
        public string ProjectOverviewERC { get; set; }
        
        // Asset class cashflow data - stored as JSON strings
        public string Summary { get; set; }
        public string Education { get; set; }
        public string PrivateSector { get; set; }
        public string LaborAndStaffAccommodation { get; set; }
        public string Office { get; set; }
        public string SocialInfrastructure { get; set; }
        public string SourcesOfFunds { get; set; }
        public string ProjectLevelIRR { get; set; }
        public string Retail { get; set; }
        public string Healthcare { get; set; }
        public string TransportLogisticIndustrial { get; set; }
        public string GeneralInfrastructure { get; set; }
        public string OtherAssetClasses { get; set; }
        public string Hospitality { get; set; }
        public string EntertainmentAndSport { get; set; }
        public string UsesOfFunds { get; set; }
        public string DevcoFinancials { get; set; }
        public List<GRTAuditEvent> AuditEvents { get; set; }
    }

    /// <summary>
    /// Paginated DTO for GRT Cashflows
    /// </summary>
    public class GRTCashflowsPagedDto
    {
        public System.Collections.Generic.List<GRTCashflowDto> Items { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int LastPage { get; set; }
    }

    /// <summary>
    /// Response DTO for GRT Cashflow create/update operations
    /// </summary>
    public class GRTCashflowResponseDto
    {
        public long Id { get; set; }
        public string ExternalReferenceCode { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateModified { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }
    }
}
