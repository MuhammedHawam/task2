using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PIF.EBP.Core.GRT;
using System.Collections.Generic;

namespace PIF.EBP.Core.GRTTable.ProjectOverview
{
    /// <summary>
    /// Response from creating GRT Project Overview
    /// </summary>
    public class ProjectOverviewTableResponse
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("externalReferenceCode")]
        public string ExternalReferenceCode { get; set; }

        [JsonProperty("dateCreated")]
        public string DateCreated { get; set; }

        [JsonProperty("dateModified")]
        public string DateModified { get; set; }

        [JsonProperty("creator")]
        public GRTCreator Creator { get; set; }

        [JsonProperty("status")]
        public GRTStatus Status { get; set; }

        [JsonProperty("projectCompanyFullName")]
        public string ProjectCompanyFullName { get; set; }

        [JsonProperty("locationCity")]
        public string LocationCity { get; set; }

        [JsonProperty("conceptDescription")]
        public string ConceptDescription { get; set; }

        [JsonProperty("landSize")]
        public double? LandSize { get; set; }

        [JsonProperty("landTake")]
        public double? LandTake { get; set; }

        [JsonProperty("developableLand")]
        public double? DevelopableLand { get; set; }

        [JsonProperty("landValueUsedInIRRCalculation")]
        public double? LandValueUsedInIRRCalculation { get; set; }

        [JsonProperty("totalFundingRequiredAllSources")]
        public double? TotalFundingRequiredAllSources { get; set; }

        [JsonProperty("lastYearOfFundingRequired")]
        public GRTKeyValue LastYearOfFundingRequired { get; set; }

        [JsonProperty("latitude")]
        public double? Latitude { get; set; }

        [JsonProperty("longitude")]
        public double? Longitude { get; set; }

        [JsonProperty("dataFilledBasedOnAnApprovedBPByCompanyBoD")]
        public GRTKeyValue DataFilledBasedOnAnApprovedBPByCompanyBoD { get; set; }

        [JsonProperty("dataFilledBasedOnAnApprovedBPByPIF")]
        public GRTKeyValue DataFilledBasedOnAnApprovedBPByPIF { get; set; }

        [JsonProperty("cEO")]
        public string CEO { get; set; }

        [JsonProperty("cEOIsActing")]
        public GRTKeyValue CEOIsActing { get; set; }

        [JsonProperty("cFO")]
        public string CFO { get; set; }

        [JsonProperty("cFOIsActing")]
        public GRTKeyValue CFOIsActing { get; set; }

        [JsonProperty("cDO")]
        public string CDO { get; set; }

        [JsonProperty("cDOIsActing")]
        public GRTKeyValue CDOIsActing { get; set; }

        [JsonProperty("cOO")]
        public string COO { get; set; }

        [JsonProperty("cOOIsActing")]
        public GRTKeyValue COOIsActing { get; set; }

        [JsonProperty("cSO")]
        public string CSO { get; set; }

        [JsonProperty("cSOIsActing")]
        public GRTKeyValue CSOIsActing { get; set; }

        [JsonProperty("companyEstablishmentPlanned")]
        public GRTKeyValue CompanyEstablishmentPlanned { get; set; }

        [JsonProperty("companyEstablishmentActual")]
        public GRTKeyValue CompanyEstablishmentActual { get; set; }

        [JsonProperty("companyIncorporationCRPlanned")]
        public GRTKeyValue CompanyIncorporationCRPlanned { get; set; }

        [JsonProperty("companyIncorporationCRActual")]
        public GRTKeyValue CompanyIncorporationCRActual { get; set; }

        [JsonProperty("firstDesignContractsAwardPlanned")]
        public GRTKeyValue FirstDesignContractsAwardPlanned { get; set; }

        [JsonProperty("firstDesignContractsAwardActual")]
        public GRTKeyValue FirstDesignContractsAwardActual { get; set; }

        [JsonProperty("firstInfrastructureAwardPlanned")]
        public GRTKeyValue FirstInfrastructureAwardPlanned { get; set; }

        [JsonProperty("firstInfrastructureAwardActual")]
        public GRTKeyValue FirstInfrastructureAwardActual { get; set; }

        [JsonProperty("firstInfrastructureStartDatePlanned")]
        public GRTKeyValue FirstInfrastructureStartDatePlanned { get; set; }

        [JsonProperty("firstInfrastructureStartDateActual")]
        public GRTKeyValue FirstInfrastructureStartDateActual { get; set; }

        [JsonProperty("firstVerticalConstructionAwardPlanned")]
        public GRTKeyValue FirstVerticalConstructionAwardPlanned { get; set; }

        [JsonProperty("firstVerticalConstructionAwardActual")]
        public GRTKeyValue FirstVerticalConstructionAwardActual { get; set; }

        [JsonProperty("firstVerticalConstructionStartDatePlanned")]
        public GRTKeyValue FirstVerticalConstructionStartDatePlanned { get; set; }

        [JsonProperty("firstVerticalConstructionStartDateActual")]
        public GRTKeyValue FirstVerticalConstructionStartDateActual { get; set; }

        [JsonProperty("lastInfrastructureCompleteDatePlanned")]
        public GRTKeyValue LastInfrastructureCompleteDatePlanned { get; set; }

        [JsonProperty("lastInfrastructureCompleteDateActual")]
        public GRTKeyValue LastInfrastructureCompleteDateActual { get; set; }

        [JsonProperty("lastVerticalConstructionCompletePlanned")]
        public GRTKeyValue LastVerticalConstructionCompletePlanned { get; set; }

        [JsonProperty("lastVerticalConstructionCompleteActual")]
        public GRTKeyValue LastVerticalConstructionCompleteActual { get; set; }

        [JsonProperty("operationsStartDateFirstGuestPlanned")]
        public GRTKeyValue OperationsStartDateFirstGuestPlanned { get; set; }

        [JsonProperty("operationsStartDateFirstGuestActual")]
        public GRTKeyValue OperationsStartDateFirstGuestActual { get; set; }

        [JsonProperty("capRate")]
        public double? CapRate { get; set; }

        [JsonProperty("terminalValueGrowthRate")]
        public double? TerminalValueGrowthRate { get; set; }

        [JsonProperty("inflation")]
        public double? Inflation { get; set; }

        [JsonProperty("costOfEquity")]
        public double? CostOfEquity { get; set; }

        [JsonProperty("wACC")]
        public double? WACC { get; set; }

        [JsonProperty("costOfDebt")]
        public double? CostOfDebt { get; set; }

        [JsonProperty("debtToEquityRatio")]
        public double? DebtToEquityRatio { get; set; }

        [JsonProperty("stableReturnOnInvestedCapitalROIC")]
        public double? StableReturnOnInvestedCapitalROIC { get; set; }

        [JsonProperty("targetDebtServiceCoverageRatioDSCR")]
        public double? TargetDebtServiceCoverageRatioDSCR { get; set; }

        [JsonProperty("referenceDocumentName1")]
        public string ReferenceDocumentName1 { get; set; }

        [JsonProperty("referenceDocumentName2")]
        public string ReferenceDocumentName2 { get; set; }

        [JsonProperty("referenceDocumentName3")]
        public string ReferenceDocumentName3 { get; set; }

        [JsonProperty("referenceDocumentName4")]
        public string ReferenceDocumentName4 { get; set; }

        [JsonProperty("referenceDocumentName5")]
        public string ReferenceDocumentName5 { get; set; }

        [JsonProperty("referenceDocumentName6")]
        public string ReferenceDocumentName6 { get; set; }

        [JsonProperty("referenceDocumentName7")]
        public string ReferenceDocumentName7 { get; set; }

        [JsonProperty("referenceDocumentName8")]
        public string ReferenceDocumentName8 { get; set; }

        [JsonProperty("referenceDocumentName9")]
        public string ReferenceDocumentName9 { get; set; }

        [JsonProperty("referenceDocumentName10")]
        public string ReferenceDocumentName10 { get; set; }

        [JsonProperty("gRTCycleCompanyMapRelationshipId")]
        public long? GRTCycleCompanyMapRelationshipId { get; set; }

        [JsonProperty("gRTCycleCompanyMapRelationshipERC")]
        public string GRTCycleCompanyMapRelationshipERC { get; set; }

        [JsonProperty("auditEvents")]
        public List<GRTAuditEvent> AuditEvents { get; set; }
    }
}
