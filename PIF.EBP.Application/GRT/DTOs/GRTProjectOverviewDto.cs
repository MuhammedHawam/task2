using System;
using System.Collections.Generic;

namespace PIF.EBP.Application.GRT
{
    /// <summary>
    /// DTO for creating/updating GRT Project Overview
    /// </summary>
    public class GRTProjectOverviewDto
    {
        // Overview fields
        public string ProjectCompanyFullName { get; set; }
        public string LocationCity { get; set; }
        public string ConceptDescription { get; set; }
        public double? LandSize { get; set; }
        public double? LandTake { get; set; }
        public double? DevelopableLand { get; set; }
        public double? LandValueUsedInIRRCalculation { get; set; }
        public double? TotalFundingRequiredAllSources { get; set; }
        public int? LastYearOfFundingRequiredId { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        // Approval Status
        public string DataFilledBasedOnAnApprovedBPByCompanyBoD { get; set; }
        public string DataFilledBasedOnAnApprovedBPByPIF { get; set; }

        // Management
        public string CEO { get; set; }
        public string CEOIsActing { get; set; }
        public string CFO { get; set; }
        public string CFOIsActing { get; set; }
        public string CDO { get; set; }
        public string CDOIsActing { get; set; }
        public string COO { get; set; }
        public string COOIsActing { get; set; }
        public string CSO { get; set; }
        public string CSOIsActing { get; set; }

        // Project Key Stages
        public string CompanyEstablishmentPlanned { get; set; }
        public string CompanyEstablishmentActual { get; set; }
        public string CompanyIncorporationCRPlanned { get; set; }
        public string CompanyIncorporationCRActual { get; set; }
        public string FirstDesignContractsAwardPlanned { get; set; }
        public string FirstDesignContractsAwardActual { get; set; }
        public string FirstInfrastructureAwardPlanned { get; set; }
        public string FirstInfrastructureAwardActual { get; set; }
        public string FirstInfrastructureStartDatePlanned { get; set; }
        public string FirstInfrastructureStartDateActual { get; set; }
        public string FirstVerticalConstructionAwardPlanned { get; set; }
        public string FirstVerticalConstructionAwardActual { get; set; }
        public string FirstVerticalConstructionStartDatePlanned { get; set; }
        public string FirstVerticalConstructionStartDateActual { get; set; }
        public string LastInfrastructureCompleteDatePlanned { get; set; }
        public string LastInfrastructureCompleteDateActual { get; set; }
        public string LastVerticalConstructionCompletePlanned { get; set; }
        public string LastVerticalConstructionCompleteActual { get; set; }
        public string OperationsStartDateFirstGuestPlanned { get; set; }
        public string OperationsStartDateFirstGuestActual { get; set; }

        // Key Financials Assumptions
        public double? CapRate { get; set; }
        public double? TerminalValueGrowthRate { get; set; }
        public double? Inflation { get; set; }
        public double? CostOfEquity { get; set; }
        public double? WACC { get; set; }
        public double? CostOfDebt { get; set; }
        public double? DebtToEquityRatio { get; set; }
        public double? StableReturnOnInvestedCapitalROIC { get; set; }
        public double? TargetDebtServiceCoverageRatioDSCR { get; set; }

        // Reference Documents
        public string ReferenceDocumentName1 { get; set; }
        public string ReferenceDocumentName2 { get; set; }
        public string ReferenceDocumentName3 { get; set; }
        public string ReferenceDocumentName4 { get; set; }
        public string ReferenceDocumentName5 { get; set; }
        public string ReferenceDocumentName6 { get; set; }
        public string ReferenceDocumentName7 { get; set; }
        public string ReferenceDocumentName8 { get; set; }
        public string ReferenceDocumentName9 { get; set; }
        public string ReferenceDocumentName10 { get; set; }

        // Relationship fields
        public long? GRTCycleCompanyMapRelationshipId { get; set; }
        public string GRTCycleCompanyMapRelationshipERC { get; set; }
    }

    /// <summary>
    /// Response DTO for GRT Project Overview creation
    /// </summary>
    public class GRTProjectOverviewResponseDto
    {
        public long Id { get; set; }
        public string ExternalReferenceCode { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateModified { get; set; }
        public string Message { get; set; }
        public bool Success { get; set; }
    }
}
