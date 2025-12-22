using PIF.EBP.Core.GRT;
using System;
using System.Collections.Generic;

namespace PIF.EBP.Application.GRT.DTOs
{
    /// <summary>
    /// DTO for GRT Approved BP list item (simplified for list view)
    /// </summary>
    public class GRTApprovedBPListDto
    {
        public long Id { get; set; }
        public string ExternalReferenceCode { get; set; }
        public string FirstInfrastructureStartDate { get; set; }
        public string OperationsStartDate { get; set; }
        public int? LastYearOfFundingRequired { get; set; }
        public DateTime? PIFDateOfApproval { get; set; }
    }

    /// <summary>
    /// DTO for creating/updating GRT Approved BP
    /// </summary>
    public class GRTApprovedBPDto
    {
        // Overview As Per Approved BP
        public string FirstInfrastructureStartDateKey { get; set; }
        public string LastInfrastructureCompleteDateKey { get; set; }
        public string FirstVerticalConstructionStartDateKey { get; set; }
        public string LastVerticalConstructionCompleteDateKey { get; set; }
        public string OperationsStartDateKey { get; set; }
        public int? LastYearOfFundingRequiredId { get; set; }
        public DateTime? PIFDateOfApproval { get; set; }

        // IRR approved by PIF
        public double? ProjectIRR { get; set; }
        public double? IRRAfterGovernmentSubsidies { get; set; }
        public double? EquityIRR { get; set; }

        public string DoesApprovedIRRIncludeLandKey { get; set; }
        public string DoesApprovedIRRIncludeInfrastructureCostKey { get; set; }
        public string DoesApprovedIRRIncludeGovernmentSubsidiesKey { get; set; }

        public string ProjectPaybackYearKey { get; set; }
        public int? ProjectPaybackPeriod { get; set; }

        // Development Plans
        public string DevelopmentPlanBy2030 { get; set; }
        public string DevelopmentPlanFullDevelopment { get; set; }

        // Sources of Funds
        public string SourcesOfFunds { get; set; }

        // Financials
        public string Financials { get; set; }

        // Relationship
        public long? ProjectToApprovedBPRelationshipProjectOverviewId { get; set; }
        public string ProjectToApprovedBPRelationshipProjectOverviewERC { get; set; }
    }

    /// <summary>
    /// Response DTO for GRT Approved BP creation/update
    /// </summary>
    public class GRTApprovedBPResponseDto
    {
        public long Id { get; set; }
        public string ExternalReferenceCode { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateModified { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }
    }

    /// <summary>
    /// DTO for GRT Approved BP details (full details for get by ID)
    /// </summary>
    public class GRTApprovedBPDetailDto
    {
        public long Id { get; set; }
        public string ExternalReferenceCode { get; set; }
        public DateTime? DateCreated { get; set; }
        public DateTime? DateModified { get; set; }

        // Overview As Per Approved BP
        public string FirstInfrastructureStartDate { get; set; }
        public string FirstInfrastructureStartDateKey { get; set; }
        public string LastInfrastructureCompleteDate { get; set; }
        public string LastInfrastructureCompleteDateKey { get; set; }
        public string FirstVerticalConstructionStartDate { get; set; }
        public string FirstVerticalConstructionStartDateKey { get; set; }
        public string LastVerticalConstructionCompleteDate { get; set; }
        public string LastVerticalConstructionCompleteDateKey { get; set; }
        public string OperationsStartDate { get; set; }
        public string OperationsStartDateKey { get; set; }
        public string LastYearOfFundingRequired { get; set; }
        public int? LastYearOfFundingRequiredId { get; set; }
        public DateTime? PIFDateOfApproval { get; set; }

        // IRR approved by PIF
        public double? ProjectIRR { get; set; }
        public double? IRRAfterGovernmentSubsidies { get; set; }
        public double? EquityIRR { get; set; }

        public string DoesApprovedIRRIncludeLand { get; set; }
        public string DoesApprovedIRRIncludeLandKey { get; set; }
        public string DoesApprovedIRRIncludeInfrastructureCost { get; set; }
        public string DoesApprovedIRRIncludeInfrastructureCostKey { get; set; }
        public string DoesApprovedIRRIncludeGovernmentSubsidies { get; set; }
        public string DoesApprovedIRRIncludeGovernmentSubsidiesKey { get; set; }

        public string ProjectPaybackYear { get; set; }
        public string ProjectPaybackYearKey { get; set; }
        public int? ProjectPaybackPeriod { get; set; }

        // Development Plans
        public string DevelopmentPlanBy2030 { get; set; }
        public string DevelopmentPlanFullDevelopment { get; set; }

        // Sources of Funds
        public string SourcesOfFunds { get; set; }

        // Financials
        public string Financials { get; set; }

        // Relationship
        public long? ProjectToApprovedBPRelationshipProjectOverviewId { get; set; }
        public string ProjectToApprovedBPRelationshipProjectOverviewERC { get; set; }

        public List<GRTAuditEvent> AuditEvents { get; set; }
    }

    /// <summary>
    /// Paginated response for GRT Approved BP list
    /// </summary>
    public class GRTApprovedBPsPagedDto
    {
        public List<GRTApprovedBPListDto> Items { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int LastPage { get; set; }
    }
}
