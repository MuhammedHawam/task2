using Newtonsoft.Json;
using System.Collections.Generic;

namespace PIF.EBP.Core.GRT
{
    #region Lookup And Common DTOs
    /// <summary>
    /// Response from GRT API for list type definitions
    /// </summary>
    public class GRTListTypeDefinitionResponse
    {
        [JsonProperty("externalReferenceCode")]
        public string ExternalReferenceCode { get; set; }

        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("listTypeEntries")]
        public List<GRTListTypeEntry> ListTypeEntries { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("name_i18n")]
        public GRTNameI18n Name_i18n { get; set; }


    }

    /// <summary>
    /// Individual entry in a GRT list type
    /// </summary>
    public class GRTListTypeEntry
    {
        [JsonProperty("externalReferenceCode")]
        public string ExternalReferenceCode { get; set; }

        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("key")]
        public string Key { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("name_i18n")]
        public GRTNameI18n Name_i18n { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }
    }

    /// <summary>
    /// Internationalized name for GRT entries
    /// </summary>
    public class GRTNameI18n
    {
        [JsonProperty("ar-SA")]
        public string ArSA { get; set; }

        [JsonProperty("en-US")]
        public string EnUS { get; set; }
    }
    #endregion

    #region Cycles And Project Overview
    /// <summary>
    /// Request for creating GRT Project Overview
    /// </summary>
    public class GRTProjectOverviewRequest
    {
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
        public int? LastYearOfFundingRequired { get; set; }

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
    }

    /// <summary>
    /// Response from creating GRT Project Overview
    /// </summary>
    public class GRTProjectOverviewResponse
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

    /// <summary>
    /// Creator information from GRT API response
    /// </summary>
    public class GRTCreator
    {
        [JsonProperty("additionalName")]
        public string AdditionalName { get; set; }

        [JsonProperty("contentType")]
        public string ContentType { get; set; }

        [JsonProperty("familyName")]
        public string FamilyName { get; set; }

        [JsonProperty("givenName")]
        public string GivenName { get; set; }

        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }

    /// <summary>
    /// Status information from GRT API response
    /// </summary>
    public class GRTStatus
    {
        [JsonProperty("code")]
        public int Code { get; set; }

        [JsonProperty("label")]
        public string Label { get; set; }

        [JsonProperty("label_i18n")]
        public object Label_i18n { get; set; }
    }

    /// <summary>
    /// GRT Cycle entry from API response
    /// </summary>
    public class GRTCycle
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

        [JsonProperty("cycleName")]
        public string CycleName { get; set; }

        [JsonProperty("cycleStartDate")]
        public string CycleStartDate { get; set; }

        [JsonProperty("cycleEndDate")]
        public string CycleEndDate { get; set; }

        [JsonProperty("cycleStage")]
        public string CycleStage { get; set; }

        [JsonProperty("cycleStatus")]
        public GRTCycleStatus CycleStatus { get; set; }
    }

    /// <summary>
    /// GRT Cycle Status
    /// </summary>
    public class GRTCycleStatus
    {
        [JsonProperty("key")]
        public string Key { get; set; }
    }

    /// <summary>
    /// GRT Key-Value pair for lookup fields that return key and name
    /// </summary>
    public class GRTKeyValue
    {
        [JsonProperty("key")]
        public string Key { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }

    /// <summary>
    /// Paginated response for GRT Cycles
    /// </summary>
    public class GRTCyclesPagedResponse
    {
        [JsonProperty("items")]
        public List<GRTCycle> Items { get; set; }

        [JsonProperty("page")]
        public int Page { get; set; }

        [JsonProperty("pageSize")]
        public int PageSize { get; set; }

        [JsonProperty("totalCount")]
        public int TotalCount { get; set; }

        [JsonProperty("lastPage")]
        public int LastPage { get; set; }

    }
    #endregion

    #region Delivery Plans
    /// <summary>
    /// GRT Delivery Plan item
    /// </summary>
    public class GRTDeliveryPlan
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

        [JsonProperty("planNumber")]
        public string PlanNumber { get; set; }

        [JsonProperty("parcelID")]
        public string ParcelID { get; set; }

        [JsonProperty("assetID")]
        public string AssetID { get; set; }

        [JsonProperty("assetName")]
        public string AssetName { get; set; }

        [JsonProperty("assetType")]
        public string AssetType { get; set; }

        [JsonProperty("subAsset")]
        public string SubAsset { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("city")]
        public string City { get; set; }

        [JsonProperty("region")]
        public GRTKeyValue Region { get; set; }

        [JsonProperty("programAssetPackage")]
        public string ProgramAssetPackage { get; set; }

        [JsonProperty("typeOfDevelopment")]
        public GRTKeyValue TypeOfDevelopment { get; set; }

        [JsonProperty("primarySecondaryHomesForResidential")]
        public GRTKeyValue PrimarySecondaryHomesForResidential { get; set; }

        [JsonProperty("brandedNonBranded")]
        public GRTKeyValue BrandedNonBranded { get; set; }

        [JsonProperty("retailType")]
        public GRTKeyValue RetailType { get; set; }

        [JsonProperty("phase")]
        public string Phase { get; set; }

        [JsonProperty("constructionStart")]
        public GRTKeyValue ConstructionStart { get; set; }

        [JsonProperty("assetStartOperatingDeliveryDate")]
        public GRTKeyValue AssetStartOperatingDeliveryDate { get; set; }

        [JsonProperty("parkingBaysLinkedToAsset")]
        public string ParkingBaysLinkedToAsset { get; set; }

        [JsonProperty("numberOfFloorsForBuildingsOnly")]
        public string NumberOfFloorsForBuildingsOnly { get; set; }

        [JsonProperty("landTake")]
        public double? LandTake { get; set; }

        [JsonProperty("bUA")]
        public double? BUA { get; set; }

        [JsonProperty("gFA")]
        public double? GFA { get; set; }

        [JsonProperty("gLA")]
        public double? GLA { get; set; }

        [JsonProperty("residentialHospitalityKeysLaborStaffRooms")]
        public int? ResidentialHospitalityKeysLaborStaffRooms { get; set; }

        [JsonProperty("developmentIsFundedBy")]
        public GRTKeyValue DevelopmentIsFundedBy { get; set; }

        [JsonProperty("revenueDriver")]
        public GRTKeyValue RevenueDriver { get; set; }

        [JsonProperty("saleForecastYear")]
        public GRTKeyValue SaleForecastYear { get; set; }

        [JsonProperty("saleStrategy")]
        public GRTKeyValue SaleStrategy { get; set; }

        [JsonProperty("avgSaleRate")]
        public double? AvgSaleRate { get; set; }

        [JsonProperty("leaseRateOrADRForKeys")]
        public double? LeaseRateOrADRForKeys { get; set; }

        [JsonProperty("occupancyInFirstYearOfOperation")]
        public double? OccupancyInFirstYearOfOperation { get; set; }

        [JsonProperty("yearOfStabilization")]
        public GRTKeyValue YearOfStabilization { get; set; }

        [JsonProperty("stableLeaseADR")]
        public double? StableLeaseADR { get; set; }

        [JsonProperty("stableOccupancy")]
        public double? StableOccupancy { get; set; }

        [JsonProperty("valueOfLandAllocatedToAsset")]
        public double? ValueOfLandAllocatedToAsset { get; set; }

        [JsonProperty("valueOfInfrastructureAllocatedToAsset")]
        public double? ValueOfInfrastructureAllocatedToAsset { get; set; }

        [JsonProperty("softCost")]
        public double? SoftCost { get; set; }

        [JsonProperty("contingencies")]
        public double? Contingencies { get; set; }

        [JsonProperty("verticalConstructionCost")]
        public double? VerticalConstructionCost { get; set; }

        [JsonProperty("revenueProceeds")]
        public double? RevenueProceeds { get; set; }

        [JsonProperty("unleveredIRR")]
        public double? UnleveredIRR { get; set; }

        [JsonProperty("verticalConstructionCostPerSqm")]
        public double? VerticalConstructionCostPerSqm { get; set; }

        [JsonProperty("totalDevelopmentCost")]
        public double? TotalDevelopmentCost { get; set; }

        [JsonProperty("totalDevelopmentCostPerSqm")]
        public double? TotalDevelopmentCostPerSqm { get; set; }

        [JsonProperty("comments")]
        public string Comments { get; set; }

        [JsonProperty("r_projectToDeliveryPlanRelationship_c_grtProjectOverviewId")]
        public long? ProjectToDeliveryPlanRelationshipProjectOverviewId { get; set; }

        [JsonProperty("r_projectToDeliveryPlanRelationship_c_grtProjectOverviewERC")]
        public string ProjectToDeliveryPlanRelationshipProjectOverviewERC { get; set; }

        [JsonProperty("projectToDeliveryPlanRelationshipERC")]
        public string ProjectToDeliveryPlanRelationshipERC { get; set; }

        [JsonProperty("auditEvents")]
        public List<GRTAuditEvent> AuditEvents { get; set; }
    }

    /// <summary>
    /// Paginated response for GRT Delivery Plans
    /// </summary>
    public class GRTDeliveryPlansPagedResponse
    {
        [JsonProperty("items")]
        public List<GRTDeliveryPlan> Items { get; set; }

        [JsonProperty("page")]
        public int Page { get; set; }

        [JsonProperty("pageSize")]
        public int PageSize { get; set; }

        [JsonProperty("totalCount")]
        public int TotalCount { get; set; }

        [JsonProperty("lastPage")]
        public int LastPage { get; set; }

        [JsonProperty("auditEvents")]
        public List<GRTAuditEvent> AuditEvents { get; set; }
    }

    /// <summary>
    /// Request for creating/updating GRT Delivery Plan
    /// </summary>
    public class GRTDeliveryPlanRequest
    {
        [JsonProperty("planNumber")]
        public string PlanNumber { get; set; }

        [JsonProperty("parcelID")]
        public string ParcelID { get; set; }

        [JsonProperty("assetID")]
        public string AssetID { get; set; }

        [JsonProperty("assetName")]
        public string AssetName { get; set; }

        [JsonProperty("assetType")]
        public string AssetType { get; set; }

        [JsonProperty("subAsset")]
        public string SubAsset { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("city")]
        public string City { get; set; }

        [JsonProperty("region")]
        public GRTKeyValue Region { get; set; }

        [JsonProperty("programAssetPackage")]
        public string ProgramAssetPackage { get; set; }

        [JsonProperty("typeOfDevelopment")]
        public GRTKeyValue TypeOfDevelopment { get; set; }

        [JsonProperty("primarySecondaryHomesForResidential")]
        public GRTKeyValue PrimarySecondaryHomesForResidential { get; set; }

        [JsonProperty("brandedNonBranded")]
        public GRTKeyValue BrandedNonBranded { get; set; }

        [JsonProperty("retailType")]
        public GRTKeyValue RetailType { get; set; }

        [JsonProperty("phase")]
        public string Phase { get; set; }

        [JsonProperty("constructionStart")]
        public GRTKeyValue ConstructionStart { get; set; }

        [JsonProperty("assetStartOperatingDeliveryDate")]
        public GRTKeyValue AssetStartOperatingDeliveryDate { get; set; }

        [JsonProperty("parkingBaysLinkedToAsset")]
        public string ParkingBaysLinkedToAsset { get; set; }

        [JsonProperty("numberOfFloorsForBuildingsOnly")]
        public string NumberOfFloorsForBuildingsOnly { get; set; }

        [JsonProperty("landTake")]
        public double? LandTake { get; set; }

        [JsonProperty("bUA")]
        public double? BUA { get; set; }

        [JsonProperty("gFA")]
        public double? GFA { get; set; }

        [JsonProperty("gLA")]
        public double? GLA { get; set; }

        [JsonProperty("residentialHospitalityKeysLaborStaffRooms")]
        public int? ResidentialHospitalityKeysLaborStaffRooms { get; set; }

        [JsonProperty("developmentIsFundedBy")]
        public GRTKeyValue DevelopmentIsFundedBy { get; set; }

        [JsonProperty("revenueDriver")]
        public GRTKeyValue RevenueDriver { get; set; }

        [JsonProperty("saleForecastYear")]
        public GRTKeyValue SaleForecastYear { get; set; }

        [JsonProperty("saleStrategy")]
        public GRTKeyValue SaleStrategy { get; set; }

        [JsonProperty("avgSaleRate")]
        public double? AvgSaleRate { get; set; }

        [JsonProperty("leaseRateOrADRForKeys")]
        public double? LeaseRateOrADRForKeys { get; set; }

        [JsonProperty("occupancyInFirstYearOfOperation")]
        public double? OccupancyInFirstYearOfOperation { get; set; }

        [JsonProperty("yearOfStabilization")]
        public GRTKeyValue YearOfStabilization { get; set; }

        [JsonProperty("stableLeaseADR")]
        public double? StableLeaseADR { get; set; }

        [JsonProperty("stableOccupancy")]
        public double? StableOccupancy { get; set; }

        [JsonProperty("valueOfLandAllocatedToAsset")]
        public double? ValueOfLandAllocatedToAsset { get; set; }

        [JsonProperty("valueOfInfrastructureAllocatedToAsset")]
        public double? ValueOfInfrastructureAllocatedToAsset { get; set; }

        [JsonProperty("softCost")]
        public double? SoftCost { get; set; }

        [JsonProperty("contingencies")]
        public double? Contingencies { get; set; }

        [JsonProperty("verticalConstructionCost")]
        public double? VerticalConstructionCost { get; set; }

        [JsonProperty("revenueProceeds")]
        public double? RevenueProceeds { get; set; }

        [JsonProperty("unleveredIRR")]
        public double? UnleveredIRR { get; set; }

        [JsonProperty("verticalConstructionCostPerSqm")]
        public double? VerticalConstructionCostPerSqm { get; set; }

        [JsonProperty("totalDevelopmentCost")]
        public double? TotalDevelopmentCost { get; set; }

        [JsonProperty("totalDevelopmentCostPerSqm")]
        public double? TotalDevelopmentCostPerSqm { get; set; }

        [JsonProperty("comments")]
        public string Comments { get; set; }

        [JsonProperty("r_projectToDeliveryPlanRelationship_c_grtProjectOverviewId")]
        public long? ProjectToDeliveryPlanRelationshipProjectOverviewId { get; set; }

        [JsonProperty("r_projectToDeliveryPlanRelationship_c_grtProjectOverviewERC")]
        public string ProjectToDeliveryPlanRelationshipProjectOverviewERC { get; set; }
    }

    /// <summary>
    /// Response from creating GRT Delivery Plan
    /// </summary>
    public class GRTDeliveryPlanResponse
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("externalReferenceCode")]
        public string ExternalReferenceCode { get; set; }

        [JsonProperty("dateCreated")]
        public string DateCreated { get; set; }

        [JsonProperty("dateModified")]
        public string DateModified { get; set; }
    }
    #endregion

    #region Cycle Company Maps
    /// <summary>
    /// Cycle Company Map from GRT API
    /// </summary>
    public class GRTCycleCompanyMap
    {
        [JsonProperty("r_cycleHasCompaniesRelationship_c_grtCyclesId")]
        public long CycleId { get; set; }

        [JsonProperty("cycleCompanyStatus")]
        public string CycleCompanyStatus { get; set; }

        [JsonProperty("poId")]
        public string PoId { get; set; }

        [JsonProperty("r_companyInCyclesRelationship_c_companiesId")]
        public long CompanyId { get; set; }
    }

    /// <summary>
    /// Response for Cycle Company Maps
    /// </summary>
    public class GRTCycleCompanyMapResponse
    {
        [JsonProperty("items")]
        public List<GRTCycleCompanyMap> Items { get; set; }

        [JsonProperty("page")]
        public int Page { get; set; }

        [JsonProperty("pageSize")]
        public int PageSize { get; set; }

        [JsonProperty("totalCount")]
        public int TotalCount { get; set; }

        [JsonProperty("lastPage")]
        public int LastPage { get; set; }


        [JsonProperty("auditEvents")]
        public List<GRTAuditEvent> AuditEvents { get; set; }
    }

    /// <summary>
    /// Batch response for GRT Cycles
    /// </summary>
    public class GRTCyclesBatchResponse
    {
        [JsonProperty("items")]
        public List<GRTCycle> Items { get; set; }
 
        [JsonProperty("auditEvents")]
        public List<GRTAuditEvent> AuditEvents { get; set; }
    }

    /// <summary>
    /// UI-friendly cycle with combined data from cycle company map and cycle details
    /// </summary>
    public class GRTUiCycle
    {
        public long CycleId { get; set; }
        public string PoId { get; set; }
        public long CompanyId { get; set; }
        public string CycleName { get; set; }
        public string CycleStartDate { get; set; }
        public string CycleEndDate { get; set; }
        public string Status { get; set; }
        public string RawCycleCompanyStatus { get; set; }
        public string RawSystemStatus { get; set; }
    }

    /// <summary>
    /// Paginated response for UI Cycles
    /// </summary>
    public class GRTUiCyclesPagedResponse
    {
        public List<GRTUiCycle> ActiveCycles { get; set; }
        public List<GRTUiCycle> PreviousCycles { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int LastPage { get; set; }

    }
    #endregion

    #region Infra Delivery Plans
    /// <summary>
    /// GRT Infrastructure Delivery Plan item
    /// </summary>
    public class GRTInfraDeliveryPlan
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

        [JsonProperty("r_projectToInfraDeliveryPlanRelationship_c_grtProjectOverviewId")]
        public long? ProjectToInfraDeliveryPlanRelationshipProjectOverviewId { get; set; }

        [JsonProperty("total")]
        public double? Total { get; set; }

        [JsonProperty("projectToInfraDeliveryPlanRelationshipERC")]
        public string ProjectToInfraDeliveryPlanRelationshipERC { get; set; }

        [JsonProperty("r_projectToInfraDeliveryPlanRelationship_c_grtProjectOverviewERC")]
        public string ProjectToInfraDeliveryPlanRelationshipProjectOverviewERC { get; set; }

        [JsonProperty("infrastructureType")]
        public GRTKeyValue InfrastructureType { get; set; }

        [JsonProperty("infrastructureSector")]
        public GRTKeyValue InfrastructureSector { get; set; }

        [JsonProperty("auditEvents")]
        public List<GRTAuditEvent> AuditEvents { get; set; }
    }

    /// <summary>
    /// Paginated response for GRT Infrastructure Delivery Plans
    /// </summary>
    public class GRTInfraDeliveryPlansPagedResponse
    {
        [JsonProperty("items")]
        public List<GRTInfraDeliveryPlan> Items { get; set; }

        [JsonProperty("page")]
        public int Page { get; set; }

        [JsonProperty("pageSize")]
        public int PageSize { get; set; }

        [JsonProperty("totalCount")]
        public int TotalCount { get; set; }

        [JsonProperty("lastPage")]
        public int LastPage { get; set; }
 
    }

    /// <summary>
    /// Request for creating/updating GRT Infrastructure Delivery Plan
    /// </summary>
    public class GRTInfraDeliveryPlanRequest
    {
        [JsonProperty("infrastructureType")]
        public GRTKeyValue InfrastructureType { get; set; }

        [JsonProperty("infrastructureSector")]
        public GRTKeyValue InfrastructureSector { get; set; }

        [JsonProperty("total")]
        public double? Total { get; set; }

        [JsonProperty("r_projectToInfraDeliveryPlanRelationship_c_grtProjectOverviewId")]
        public long? ProjectToInfraDeliveryPlanRelationshipProjectOverviewId { get; set; }

        [JsonProperty("r_projectToInfraDeliveryPlanRelationship_c_grtProjectOverviewERC")]
        public string ProjectToInfraDeliveryPlanRelationshipProjectOverviewERC { get; set; }
    }

    /// <summary>
    /// Response from creating/updating GRT Infrastructure Delivery Plan
    /// </summary>
    public class GRTInfraDeliveryPlanResponse
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("externalReferenceCode")]
        public string ExternalReferenceCode { get; set; }

        [JsonProperty("dateCreated")]
        public string DateCreated { get; set; }

        [JsonProperty("dateModified")]
        public string DateModified { get; set; }
    }

    /// <summary>
    /// GRT Infrastructure Delivery Plan Year entry
    /// </summary>
    public class GRTInfraDeliveryPlanYear
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

        [JsonProperty("amount")]
        public double? Amount { get; set; }

        [JsonProperty("r_infraDeliveryPlanToYearsRelationship_c_grtInfraDeliveryPlanId")]
        public long? InfraDeliveryPlanToYearsRelationshipInfraDeliveryPlanId { get; set; }

        [JsonProperty("year")]
        public GRTKeyValue Year { get; set; }

        [JsonProperty("r_infraDeliveryPlanToYearsRelationship_c_grtInfraDeliveryPlanERC")]
        public string InfraDeliveryPlanToYearsRelationshipInfraDeliveryPlanERC { get; set; }

        [JsonProperty("infraDeliveryPlanToYearsRelationshipERC")]
        public string InfraDeliveryPlanToYearsRelationshipERC { get; set; }

        [JsonProperty("actualPlanned")]
        public GRTKeyValue ActualPlanned { get; set; }
    }

    /// <summary>
    /// Paginated response for Infrastructure Delivery Plan Years
    /// </summary>
    public class GRTInfraDeliveryPlanYearsPagedResponse
    {
        [JsonProperty("items")]
        public List<GRTInfraDeliveryPlanYear> Items { get; set; }

        [JsonProperty("page")]
        public int Page { get; set; }

        [JsonProperty("pageSize")]
        public int PageSize { get; set; }

        [JsonProperty("totalCount")]
        public int TotalCount { get; set; }

        [JsonProperty("lastPage")]
        public int LastPage { get; set; }

        [JsonProperty("auditEvents")]
        public List<GRTAuditEvent> AuditEvents { get; set; }
    }

    /// <summary>
    /// Request for creating/updating Infrastructure Delivery Plan Year
    /// </summary>
    public class GRTInfraDeliveryPlanYearRequest
    {
        [JsonProperty("actualPlanned")]
        public GRTKeyValue ActualPlanned { get; set; }

        [JsonProperty("year")]
        public GRTKeyValue Year { get; set; }

        [JsonProperty("amount")]
        public double? Amount { get; set; }

        [JsonProperty("r_infraDeliveryPlanToYearsRelationship_c_grtInfraDeliveryPlanId")]
        public long? InfraDeliveryPlanToYearsRelationshipInfraDeliveryPlanId { get; set; }

        [JsonProperty("r_infraDeliveryPlanToYearsRelationship_c_grtInfraDeliveryPlanERC")]
        public string InfraDeliveryPlanToYearsRelationshipInfraDeliveryPlanERC { get; set; }
    }
    #endregion

    #region Land Sales
    /// <summary>
    /// GRT Land Sale item
    /// </summary>
    public class GRTLandSale
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

        [JsonProperty("plotName")]
        public string PlotName { get; set; }

        [JsonProperty("landUse")]
        public GRTKeyValue LandUse { get; set; }

        [JsonProperty("landType")]
        public GRTKeyValue LandType { get; set; }

        [JsonProperty("city")]
        public string City { get; set; }

        [JsonProperty("region")]
        public GRTKeyValue Region { get; set; }

        [JsonProperty("restrictedDevelopmentToSpecificCriteria")]
        public GRTKeyValue RestrictedDevelopmentToSpecificCriteria { get; set; }

        [JsonProperty("saleLease")]
        public GRTKeyValue SaleLease { get; set; }

        [JsonProperty("numberOfPlots")]
        public double? NumberOfPlots { get; set; }

        [JsonProperty("totalLandArea")]
        public double? TotalLandArea { get; set; }

        [JsonProperty("avgSaleLeaseRate")]
        public double? AvgSaleLeaseRate { get; set; }

        [JsonProperty("yearOfSaleLeaseStart")]
        public GRTKeyValue YearOfSaleLeaseStart { get; set; }

        [JsonProperty("valueOfInfrastructureAllocatedToLand")]
        public double? ValueOfInfrastructureAllocatedToLand { get; set; }

        [JsonProperty("r_projectToLandSaleRelationship_c_grtProjectOverviewId")]
        public long? ProjectToLandSaleRelationshipProjectOverviewId { get; set; }

        [JsonProperty("r_projectToLandSaleRelationship_c_grtProjectOverviewERC")]
        public string ProjectToLandSaleRelationshipProjectOverviewERC { get; set; }

        [JsonProperty("projectToLandSaleRelationshipERC")]
        public string ProjectToLandSaleRelationshipERC { get; set; }

        [JsonProperty("auditEvents")]
        public List<GRTAuditEvent> AuditEvents { get; set; }
    }

    /// <summary>
    /// Paginated response for GRT Land Sales
    /// </summary>
    public class GRTLandSalesPagedResponse
    {
        [JsonProperty("items")]
        public List<GRTLandSale> Items { get; set; }

        [JsonProperty("page")]
        public int Page { get; set; }

        [JsonProperty("pageSize")]
        public int PageSize { get; set; }

        [JsonProperty("totalCount")]
        public int TotalCount { get; set; }

        [JsonProperty("lastPage")]
        public int LastPage { get; set; }

    }

    /// <summary>
    /// Request for creating/updating GRT Land Sale
    /// </summary>
    public class GRTLandSaleRequest
    {
        [JsonProperty("plotName")]
        public string PlotName { get; set; }

        [JsonProperty("landUse")]
        public GRTKeyValue LandUse { get; set; }

        [JsonProperty("landType")]
        public GRTKeyValue LandType { get; set; }

        [JsonProperty("city")]
        public string City { get; set; }

        [JsonProperty("region")]
        public GRTKeyValue Region { get; set; }

        [JsonProperty("restrictedDevelopmentToSpecificCriteria")]
        public GRTKeyValue RestrictedDevelopmentToSpecificCriteria { get; set; }

        [JsonProperty("saleLease")]
        public GRTKeyValue SaleLease { get; set; }

        [JsonProperty("numberOfPlots")]
        public double? NumberOfPlots { get; set; }

        [JsonProperty("totalLandArea")]
        public double? TotalLandArea { get; set; }

        [JsonProperty("avgSaleLeaseRate")]
        public double? AvgSaleLeaseRate { get; set; }

        [JsonProperty("yearOfSaleLeaseStart")]
        public GRTKeyValue YearOfSaleLeaseStart { get; set; }

        [JsonProperty("valueOfInfrastructureAllocatedToLand")]
        public double? ValueOfInfrastructureAllocatedToLand { get; set; }

        [JsonProperty("r_projectToLandSaleRelationship_c_grtProjectOverviewId")]
        public long? ProjectToLandSaleRelationshipProjectOverviewId { get; set; }

        [JsonProperty("r_projectToLandSaleRelationship_c_grtProjectOverviewERC")]
        public string ProjectToLandSaleRelationshipProjectOverviewERC { get; set; }
    }

    /// <summary>
    /// Response from creating/updating GRT Land Sale
    /// </summary>
    public class GRTLandSaleResponse
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("externalReferenceCode")]
        public string ExternalReferenceCode { get; set; }

        [JsonProperty("dateCreated")]
        public string DateCreated { get; set; }

        [JsonProperty("dateModified")]
        public string DateModified { get; set; }
    }
    #endregion

    #region Cashflows
    /// <summary>
    /// GRT Cashflow item - represents financial data for different asset classes
    /// </summary>
    public class GRTCashflow
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

        [JsonProperty("r_projectToCashflowRelationship_c_grtProjectOverviewId")]
        public long? ProjectToCashflowRelationshipProjectOverviewId { get; set; }

        [JsonProperty("r_projectToCashflowRelationship_c_grtProjectOverviewERC")]
        public string ProjectToCashflowRelationshipProjectOverviewERC { get; set; }

        [JsonProperty("projectToCashflowRelationshipERC")]
        public string ProjectToCashflowRelationshipERC { get; set; }

        // Asset class cashflow data - stored as JSON strings
        [JsonProperty("summary")]
        public string Summary { get; set; }

        [JsonProperty("education")]
        public string Education { get; set; }

        [JsonProperty("privateSector")]
        public string PrivateSector { get; set; }

        [JsonProperty("laborAndStaffAccommodation")]
        public string LaborAndStaffAccommodation { get; set; }

        [JsonProperty("office")]
        public string Office { get; set; }

        [JsonProperty("socialInfrastructure")]
        public string SocialInfrastructure { get; set; }

        [JsonProperty("sourcesOfFunds")]
        public string SourcesOfFunds { get; set; }

        [JsonProperty("projectLevelIRR")]
        public string ProjectLevelIRR { get; set; }

        [JsonProperty("retail")]
        public string Retail { get; set; }

        [JsonProperty("healthcare")]
        public string Healthcare { get; set; }

        [JsonProperty("transportLogisticIndustrial")]
        public string TransportLogisticIndustrial { get; set; }

        [JsonProperty("generalInfrastructure")]
        public string GeneralInfrastructure { get; set; }

        [JsonProperty("otherAssetClasses")]
        public string OtherAssetClasses { get; set; }

        [JsonProperty("hospitality")]
        public string Hospitality { get; set; }

        [JsonProperty("entertainmentAndSport")]
        public string EntertainmentAndSport { get; set; }

        [JsonProperty("usesOfFunds")]
        public string UsesOfFunds { get; set; }

        [JsonProperty("devcoFinancials")]
        public string DevcoFinancials { get; set; }

        [JsonProperty("auditEvents")]
        public List<GRTAuditEvent> AuditEvents { get; set; }
    }

    /// <summary>
    /// Paginated response for GRT Cashflows
    /// </summary>
    public class GRTCashflowsPagedResponse
    {
        [JsonProperty("items")]
        public List<GRTCashflow> Items { get; set; }

        [JsonProperty("page")]
        public int Page { get; set; }

        [JsonProperty("pageSize")]
        public int PageSize { get; set; }

        [JsonProperty("totalCount")]
        public int TotalCount { get; set; }

        [JsonProperty("lastPage")]
        public int LastPage { get; set; }

    }
    #region Budgets
    public class GRTBudgetsResponse
    {
        [JsonProperty("items")]
        public List<GRTBudgetResponse> Items { get; set; }

        [JsonProperty("page")]
        public int Page { get; set; }

        [JsonProperty("pageSize")]
        public int PageSize { get; set; }

        [JsonProperty("totalCount")]
        public int TotalCount { get; set; }

        [JsonProperty("lastPage")]
        public int LastPage { get; set; }


    }

    public class GRTBudgetRequest
    {
        [JsonProperty("budgetYear")]
        public GRTKeyValue BudgetYear { get; set; }

        [JsonProperty("budgetApprovedByCompanyBoDOrItsDelegation")]
        public GRTKeyValue BudgetApprovedByCompanyBoDOrItsDelegation { get; set; }

        [JsonProperty("r_projectToBudgetRelationship_c_grtProjectOverviewId")]
        public long? ProjectOverviewId { get; set; }

        [JsonProperty("r_projectToBudgetRelationship_c_grtProjectOverviewERC")]
        public string ProjectOverviewERC { get; set; }

        [JsonProperty("forecastSpendingBudgetByMonth")]
        public string ForecastSpendingBudgetByMonth { get; set; }

        [JsonProperty("actualSpendingBudgetByMonth")]
        public string ActualSpendingBudgetByMonth { get; set; }

        [JsonProperty("varianceBudgetByMonth")]
        public string VarianceBudgetByMonth { get; set; }

        [JsonProperty("variance")]
        public string Variance { get; set; }

        [JsonProperty("cashDepositsBudgetByMonth")]
        public string CashDepositsBudgetByMonth { get; set; }

        [JsonProperty("cashDeposits")]
        public string CashDeposits { get; set; }

        [JsonProperty("commitmentsForecastBudgetByMonth")]
        public string CommitmentsForecastBudgetByMonth { get; set; }

        [JsonProperty("commitments")]
        public string Commitments { get; set; }

        [JsonProperty("commitmentsActualBudgetByMonth")]
        public string CommitmentsActualBudgetByMonth { get; set; }

        [JsonProperty("commitmentActual")]
        public string CommitmentActual { get; set; }
    }

    public class GRTBudgetResponse
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("externalReferenceCode")]
        public string ExternalReferenceCode { get; set; }

        [JsonProperty("dateCreated")]
        public string DateCreated { get; set; }

        [JsonProperty("dateModified")]
        public string DateModified { get; set; }

        [JsonProperty("status")]
        public GRTStatus Status { get; set; }

        [JsonProperty("budgetYear")]
        public GRTKeyValue BudgetYear { get; set; }

        [JsonProperty("budgetApprovedByCompanyBoDOrItsDelegation")]
        public GRTKeyValue BudgetApprovedByCompanyBoDOrItsDelegation { get; set; }

        [JsonProperty("r_projectToBudgetRelationship_c_grtProjectOverviewId")]
        public long? ProjectOverviewId { get; set; }

        [JsonProperty("r_projectToBudgetRelationship_c_grtProjectOverviewERC")]
        public string ProjectOverviewERC { get; set; }

        [JsonProperty("projectToBudgetRelationshipERC")]
        public string ProjectToBudgetRelationshipERC { get; set; }

        [JsonProperty("forecastSpendingBudgetByMonth")]
        public string ForecastSpendingBudgetByMonth { get; set; }

        [JsonProperty("actualSpendingBudgetByMonth")]
        public string ActualSpendingBudgetByMonth { get; set; }

        [JsonProperty("varianceBudgetByMonth")]
        public string VarianceBudgetByMonth { get; set; }

        [JsonProperty("variance")]
        public string Variance { get; set; }

        [JsonProperty("cashDepositsBudgetByMonth")]
        public string CashDepositsBudgetByMonth { get; set; }

        [JsonProperty("cashDeposits")]
        public string CashDeposits { get; set; }

        [JsonProperty("commitmentsForecastBudgetByMonth")]
        public string CommitmentsForecastBudgetByMonth { get; set; }

        [JsonProperty("commitments")]
        public string Commitments { get; set; }

        [JsonProperty("commitmentsActualBudgetByMonth")]
        public string CommitmentsActualBudgetByMonth { get; set; }

        [JsonProperty("commitmentActual")]
        public string CommitmentActual { get; set; }

        [JsonProperty("auditEvents")]
        public List<GRTAuditEvent> AuditEvents { get; set; }
    }
    #endregion
    #endregion

    #region Cashflow Requests and Responses
    /// <summary>
    /// Request for creating/updating GRT Cashflow
    /// </summary>
    public class GRTCashflowRequest
    {
        [JsonProperty("r_projectToCashflowRelationship_c_grtProjectOverviewId")]
        public long? ProjectToCashflowRelationshipProjectOverviewId { get; set; }

        [JsonProperty("r_projectToCashflowRelationship_c_grtProjectOverviewERC")]
        public string ProjectToCashflowRelationshipProjectOverviewERC { get; set; }

        [JsonProperty("summary")]
        public string Summary { get; set; }

        [JsonProperty("education")]
        public string Education { get; set; }

        [JsonProperty("privateSector")]
        public string PrivateSector { get; set; }

        [JsonProperty("laborAndStaffAccommodation")]
        public string LaborAndStaffAccommodation { get; set; }

        [JsonProperty("office")]
        public string Office { get; set; }

        [JsonProperty("socialInfrastructure")]
        public string SocialInfrastructure { get; set; }

        [JsonProperty("sourcesOfFunds")]
        public string SourcesOfFunds { get; set; }

        [JsonProperty("projectLevelIRR")]
        public string ProjectLevelIRR { get; set; }

        [JsonProperty("retail")]
        public string Retail { get; set; }

        [JsonProperty("healthcare")]
        public string Healthcare { get; set; }

        [JsonProperty("transportLogisticIndustrial")]
        public string TransportLogisticIndustrial { get; set; }

        [JsonProperty("generalInfrastructure")]
        public string GeneralInfrastructure { get; set; }

        [JsonProperty("otherAssetClasses")]
        public string OtherAssetClasses { get; set; }

        [JsonProperty("hospitality")]
        public string Hospitality { get; set; }

        [JsonProperty("entertainmentAndSport")]
        public string EntertainmentAndSport { get; set; }

        [JsonProperty("usesOfFunds")]
        public string UsesOfFunds { get; set; }

        [JsonProperty("devcoFinancials")]
        public string DevcoFinancials { get; set; }
    }

    /// <summary>
    /// Response from creating/updating GRT Cashflow
    /// </summary>
    public class GRTCashflowResponse
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("externalReferenceCode")]
        public string ExternalReferenceCode { get; set; }

        [JsonProperty("dateCreated")]
        public string DateCreated { get; set; }

        [JsonProperty("dateModified")]
        public string DateModified { get; set; }
    }
    #endregion

    #region LOI & HMA
    /// <summary>
    /// GRT LOI & HMA (Approved Business Plan) item
    /// </summary>
    public class GRTLOIHMA
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

        [JsonProperty("assetName")]
        public string AssetName { get; set; }

        [JsonProperty("brand")]
        public string Brand { get; set; }

        [JsonProperty("city")]
        public string City { get; set; }

        [JsonProperty("hMASigned")]
        public bool? HMASigned { get; set; }

        [JsonProperty("hotelOperator")]
        public GRTKeyValue HotelOperator { get; set; }

        [JsonProperty("ifHMALOISignedContractDuration")]
        public string IfHMALOISignedContractDuration { get; set; }

        [JsonProperty("ifOtherHotelOperatorFillHere")]
        public string IfOtherHotelOperatorFillHere { get; set; }

        [JsonProperty("ifOtherOperatingModelFillHere")]
        public string IfOtherOperatingModelFillHere { get; set; }

        [JsonProperty("item")]
        public string Item { get; set; }

        [JsonProperty("keysPerHotel")]
        public string KeysPerHotel { get; set; }

        [JsonProperty("latitude")]
        public string Latitude { get; set; }

        [JsonProperty("lOISigned")]
        public bool? LOISigned { get; set; }

        [JsonProperty("longitude")]
        public string Longitude { get; set; }

        [JsonProperty("operatingModel")]
        public GRTKeyValue OperatingModel { get; set; }

        [JsonProperty("operatingModelNew")]
        public GRTKeyValue OperatingModelNew { get; set; }

        [JsonProperty("operationalYear")]
        public string OperationalYear { get; set; }

        [JsonProperty("positionscale")]
        public GRTKeyValue Positionscale { get; set; }

        [JsonProperty("r_projectToLOIHMARelationship_c_grtProjectOverviewId")]
        public long? ProjectToLOIHMARelationshipProjectOverviewId { get; set; }

        [JsonProperty("r_projectToLOIHMARelationship_c_grtProjectOverviewERC")]
        public string ProjectToLOIHMARelationshipProjectOverviewERC { get; set; }

        [JsonProperty("projectToLOIHMARelationshipERC")]
        public string ProjectToLOIHMARelationshipERC { get; set; }

        [JsonProperty("auditEvents")]
        public List<GRTAuditEvent> AuditEvents { get; set; }
    }

    /// <summary>
    /// Paginated response for GRT LOI & HMA
    /// </summary>
    public class GRTLOIHMAsPagedResponse
    {
        [JsonProperty("items")]
        public List<GRTLOIHMA> Items { get; set; }

        [JsonProperty("page")]
        public int Page { get; set; }

        [JsonProperty("pageSize")]
        public int PageSize { get; set; }

        [JsonProperty("totalCount")]
        public int TotalCount { get; set; }

        [JsonProperty("lastPage")]
        public int LastPage { get; set; }

    }

    /// <summary>
    /// Request for creating/updating GRT LOI & HMA
    /// </summary>
    public class GRTLOIHMARequest
    {
        [JsonProperty("assetName")]
        public string AssetName { get; set; }

        [JsonProperty("brand")]
        public string Brand { get; set; }

        [JsonProperty("city")]
        public string City { get; set; }

        [JsonProperty("hMASigned")]
        public bool? HMASigned { get; set; }

        [JsonProperty("hotelOperator")]
        public GRTKeyValue HotelOperator { get; set; }

        [JsonProperty("ifHMALOISignedContractDuration")]
        public string IfHMALOISignedContractDuration { get; set; }

        [JsonProperty("ifOtherHotelOperatorFillHere")]
        public string IfOtherHotelOperatorFillHere { get; set; }

        [JsonProperty("ifOtherOperatingModelFillHere")]
        public string IfOtherOperatingModelFillHere { get; set; }

        [JsonProperty("item")]
        public string Item { get; set; }

        [JsonProperty("keysPerHotel")]
        public string KeysPerHotel { get; set; }

        [JsonProperty("latitude")]
        public string Latitude { get; set; }

        [JsonProperty("lOISigned")]
        public bool? LOISigned { get; set; }

        [JsonProperty("longitude")]
        public string Longitude { get; set; }

        [JsonProperty("operatingModel")]
        public GRTKeyValue OperatingModel { get; set; }

        [JsonProperty("operatingModelNew")]
        public GRTKeyValue OperatingModelNew { get; set; }

        [JsonProperty("operationalYear")]
        public string OperationalYear { get; set; }

        [JsonProperty("positionscale")]
        public GRTKeyValue Positionscale { get; set; }

        [JsonProperty("r_projectToLOIHMARelationship_c_grtProjectOverviewId")]
        public long? ProjectToLOIHMARelationshipProjectOverviewId { get; set; }

        [JsonProperty("r_projectToLOIHMARelationship_c_grtProjectOverviewERC")]
        public string ProjectToLOIHMARelationshipProjectOverviewERC { get; set; }
    }

    /// <summary>
    /// Response from creating/updating GRT LOI & HMA
    /// </summary>
    public class GRTLOIHMAResponse
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("externalReferenceCode")]
        public string ExternalReferenceCode { get; set; }

        [JsonProperty("dateCreated")]
        public string DateCreated { get; set; }

        [JsonProperty("dateModified")]
        public string DateModified { get; set; }
    }
    #endregion

    #region Project Impact
    public class GRTProjectImpact
    {
        [JsonProperty("id")]
        public long Id { get; set; }
        [JsonProperty("externalReferenceCode")]
        public string ExternalReferenceCode { get; set; }
        [JsonProperty("dateCreated")]
        public string DateCreated { get; set; }
        [JsonProperty("dateModified")]
        public string DateModified { get; set; }
        [JsonProperty("r_projectToProjectImpactRelationship_c_grtProjectOverviewId")]
        public long? ProjectToProjectImpactRelationshipProjectOverviewId { get; set; }
        [JsonProperty("pr_projectToProjectImpactRelationship_c_grtProjectOverviewERC")]
        public string ProjectToProjectImpactRelationshipProjectOverviewERC { get; set; }
        [JsonProperty("projectToProjectImpactRelationshipERC")]
        public string ProjectToProjectImpactRelationshipERC { get; set; }
        [JsonProperty("averagePersonalDisposableIncome")]
        public string AveragePersonalDisposableIncome { get; set; }
        [JsonProperty("entertainmentSpendHouseholdAnnumSAR")]
        public string EntertainmentSpendHouseholdAnnumSAR { get; set; }
        [JsonProperty("macroeconomicImpactSection")]
        public string MacroeconomicImpactSection { get; set; }
        [JsonProperty("totalDomesticOvernightVisits")]
        public string TotalDomesticOvernightVisits { get; set; }
        [JsonProperty("totalHotelOvernightVisits")]
        public string TotalHotelOvernightVisits { get; set; }
        [JsonProperty("totalInternationalOvernightVisits")]
        public string TotalInternationalOvernightVisits { get; set; }
        [JsonProperty("totalNumberOfEmployees")]
        public string TotalNumberOfEmployees { get; set; }
        [JsonProperty("totalNumberOfHospitalityStaffLabor")]
        public string TotalNumberOfHospitalityStaffLabor { get; set; }
        [JsonProperty("totalPopulationOfTheProjectSection")]
        public string TotalPopulationOfTheProjectSection { get; set; }

        [JsonProperty("auditEvents")]
        public List<GRTAuditEvent> AuditEvents { get; set; }
    }

    public class GRTProjectImpactRequest
    {
        [JsonProperty("projectToProjectImpactRelationshipProjectOverviewId")]
        public long? ProjectToProjectImpactRelationshipProjectOverviewId { get; set; }
        [JsonProperty("projectToProjectImpactRelationshipProjectOverviewERC")]
        public string ProjectToProjectImpactRelationshipProjectOverviewERC { get; set; }
        [JsonProperty("projectToProjectImpactRelationshipERC")]
        public string ProjectToProjectImpactRelationshipERC { get; set; }
        [JsonProperty("averagePersonalDisposableIncome")]
        public string AveragePersonalDisposableIncome { get; set; }
        [JsonProperty("entertainmentSpendHouseholdAnnumSAR")]
        public string EntertainmentSpendHouseholdAnnumSAR { get; set; }
        [JsonProperty("macroeconomicImpactSection")]
        public string MacroeconomicImpactSection { get; set; }
        [JsonProperty("totalDomesticOvernightVisits")]
        public string TotalDomesticOvernightVisits { get; set; }
        [JsonProperty("totalHotelOvernightVisits")]
        public string TotalHotelOvernightVisits { get; set; }
        [JsonProperty("totalInternationalOvernightVisits")]
        public string TotalInternationalOvernightVisits { get; set; }
        [JsonProperty("totalNumberOfEmployees")]
        public string TotalNumberOfEmployees { get; set; }
        [JsonProperty("totalNumberOfHospitalityStaffLabor")]
        public string TotalNumberOfHospitalityStaffLabor { get; set; }
        [JsonProperty("totalPopulationOfTheProjectSection")]
        public string TotalPopulationOfTheProjectSection { get; set; }
    }

    public class GRTProjectImpactResponse
    {
        public long Id { get; set; }
        public string ExternalReferenceCode { get; set; }
        public string DateCreated { get; set; }
        public string DateModified { get; set; }
    }

    /// <summary>
    /// Paginated response for GRT Infrastructure Delivery Plans
    /// </summary>
    public class GRTProjectImpactPagedResponse
    {
        [JsonProperty("items")]
        public List<GRTProjectImpact> Items { get; set; }

        [JsonProperty("page")]
        public int Page { get; set; }

        [JsonProperty("pageSize")]
        public int PageSize { get; set; }

        [JsonProperty("totalCount")]
        public int TotalCount { get; set; }

        [JsonProperty("lastPage")]
        public int LastPage { get; set; }
    }

    #endregion

    #region Multiple S&U
    /// <summary>
    /// GRT Multiple S&U (Sources & Uses) Financial Planning item
    /// </summary>
    public class GRTMultipleSandU
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

        [JsonProperty("regions")]
        public GRTKeyValue Regions { get; set; }

        [JsonProperty("capexJSON")]
        public string CapexJSON { get; set; }

        [JsonProperty("opexJSON")]
        public string OpexJSON { get; set; }

        [JsonProperty("totalSourcesJSON")]
        public string TotalSourcesJSON { get; set; }

        [JsonProperty("financialsSARJSON")]
        public string FinancialsSARJSON { get; set; }

        [JsonProperty("r_projectToMultipleSandURelationship_c_grtProjectOverviewId")]
        public long? ProjectToMultipleSandURelationshipProjectOverviewId { get; set; }

        [JsonProperty("r_projectToMultipleSandURelationship_c_grtProjectOverviewERC")]
        public string ProjectToMultipleSandURelationshipProjectOverviewERC { get; set; }

        [JsonProperty("projectToMultipleSandURelationshipERC")]
        public string ProjectToMultipleSandURelationshipERC { get; set; }

        [JsonProperty("auditEvents")]
        public List<GRTAuditEvent> AuditEvents { get; set; }
    }

    /// <summary>
    /// Paginated response for GRT Multiple S&U
    /// </summary>
    public class GRTMultipleSandUsPagedResponse
    {
        [JsonProperty("items")]
        public List<GRTMultipleSandU> Items { get; set; }

        [JsonProperty("page")]
        public int Page { get; set; }

        [JsonProperty("pageSize")]
        public int PageSize { get; set; }

        [JsonProperty("totalCount")]
        public int TotalCount { get; set; }

        [JsonProperty("lastPage")]
        public int LastPage { get; set; }
    }

    /// <summary>
    /// Request for creating/updating GRT Multiple S&U
    /// </summary>
    public class GRTMultipleSandURequest
    {
        [JsonProperty("regions")]
        public GRTKeyValue Regions { get; set; }

        [JsonProperty("capexJSON")]
        public string CapexJSON { get; set; }

        [JsonProperty("opexJSON")]
        public string OpexJSON { get; set; }

        [JsonProperty("totalSourcesJSON")]
        public string TotalSourcesJSON { get; set; }

        [JsonProperty("financialsSARJSON")]
        public string FinancialsSARJSON { get; set; }

        [JsonProperty("r_projectToMultipleSandURelationship_c_grtProjectOverviewId")]
        public long? ProjectToMultipleSandURelationshipProjectOverviewId { get; set; }

        [JsonProperty("r_projectToMultipleSandURelationship_c_grtProjectOverviewERC")]
        public string ProjectToMultipleSandURelationshipProjectOverviewERC { get; set; }
    }

    /// <summary>
    /// Response from creating/updating GRT Multiple S&U
    /// </summary>
    public class GRTMultipleSandUResponse
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("externalReferenceCode")]
        public string ExternalReferenceCode { get; set; }

        [JsonProperty("dateCreated")]
        public string DateCreated { get; set; }

        [JsonProperty("dateModified")]
        public string DateModified { get; set; }
    }
    #endregion

    #region Approved BP
    /// <summary>
    /// GRT Approved BP item
    /// </summary>
    public class GRTApprovedBP
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

        // Overview As Per Approved BP
        [JsonProperty("firstInfrastructureStartDate")]
        public GRTKeyValue FirstInfrastructureStartDate { get; set; }

        [JsonProperty("firstInfrastructureStartSate")]
        public GRTKeyValue FirstInfrastructureStartSate { get; set; }

        [JsonProperty("lastInfrastructureCompleteDate")]
        public GRTKeyValue LastInfrastructureCompleteDate { get; set; }

        [JsonProperty("firstVerticalConstructionStartDate")]
        public GRTKeyValue FirstVerticalConstructionStartDate { get; set; }

        [JsonProperty("lastVerticalConstructionCompleteDate")]
        public GRTKeyValue LastVerticalConstructionCompleteDate { get; set; }

        [JsonProperty("operationsStartDate")]
        public GRTKeyValue OperationsStartDate { get; set; }

        [JsonProperty("lastYearOfFundingRequired")]
        public GRTKeyValue LastYearOfFundingRequired { get; set; }

        [JsonProperty("pIFDateOfApproval")]
        public string PIFDateOfApproval { get; set; }

        // IRR approved by PIF
        [JsonProperty("projectIRR")]
        public double? ProjectIRR { get; set; }

        [JsonProperty("iRRAfterGovernmentSubsidies")]
        public double? IRRAfterGovernmentSubsidies { get; set; }

        [JsonProperty("equityIRR")]
        public double? EquityIRR { get; set; }

        [JsonProperty("doesApprovedIRRIncludeLand")]
        public GRTKeyValue DoesApprovedIRRIncludeLand { get; set; }

        [JsonProperty("doesApprovedIRRIncludeInfrastructureCost")]
        public GRTKeyValue DoesApprovedIRRIncludeInfrastructureCost { get; set; }

        [JsonProperty("doesApprovedIRRIncludeGovernmentSubsidies")]
        public GRTKeyValue DoesApprovedIRRIncludeGovernmentSubsidies { get; set; }

        [JsonProperty("projectPaybackYear")]
        public GRTKeyValue ProjectPaybackYear { get; set; }

        [JsonProperty("projectPaybackPeriod")]
        public int? ProjectPaybackPeriod { get; set; }

        // Development Plans
        [JsonProperty("developmentPlanBy2030")]
        public string DevelopmentPlanBy2030 { get; set; }

        [JsonProperty("developmentPlanFullDevelopment")]
        public string DevelopmentPlanFullDevelopment { get; set; }

        // Sources of Funds
        [JsonProperty("sourcesOfFunds")]
        public string SourcesOfFunds { get; set; }

        // Financials
        [JsonProperty("financials")]
        public string Financials { get; set; }

        // Relationship
        [JsonProperty("r_projectToApprovedBPRelationship_c_grtProjectOverviewId")]
        public long? ProjectToApprovedBPRelationshipProjectOverviewId { get; set; }

        [JsonProperty("r_projectToApprovedBPRelationship_c_grtProjectOverviewERC")]
        public string ProjectToApprovedBPRelationshipProjectOverviewERC { get; set; }

        [JsonProperty("projectToApprovedBPRelationshipERC")]
        public string ProjectToApprovedBPRelationshipERC { get; set; }

        [JsonProperty("auditEvents")]
        public List<GRTAuditEvent> AuditEvents { get; set; }
    }

    /// <summary>
    /// Paginated response for GRT Approved BP
    /// </summary>
    public class GRTApprovedBPsPagedResponse
    {
        [JsonProperty("items")]
        public List<GRTApprovedBP> Items { get; set; }

        [JsonProperty("page")]
        public int Page { get; set; }

        [JsonProperty("pageSize")]
        public int PageSize { get; set; }

        [JsonProperty("totalCount")]
        public int TotalCount { get; set; }

        [JsonProperty("lastPage")]
        public int LastPage { get; set; }
    }

    /// <summary>
    /// Request for creating/updating GRT Approved BP
    /// </summary>
    public class GRTApprovedBPRequest
    {
        // Overview As Per Approved BP
        [JsonProperty("firstInfrastructureStartDate")]
        public GRTKeyValue FirstInfrastructureStartDate { get; set; }

        [JsonProperty("firstInfrastructureStartSate")]
        public GRTKeyValue FirstInfrastructureStartSate { get; set; }

        [JsonProperty("lastInfrastructureCompleteDate")]
        public GRTKeyValue LastInfrastructureCompleteDate { get; set; }

        [JsonProperty("firstVerticalConstructionStartDate")]
        public GRTKeyValue FirstVerticalConstructionStartDate { get; set; }

        [JsonProperty("lastVerticalConstructionCompleteDate")]
        public GRTKeyValue LastVerticalConstructionCompleteDate { get; set; }

        [JsonProperty("operationsStartDate")]
        public GRTKeyValue OperationsStartDate { get; set; }

        [JsonProperty("lastYearOfFundingRequired")]
        public GRTKeyValue LastYearOfFundingRequired { get; set; }

        [JsonProperty("pIFDateOfApproval")]
        public string PIFDateOfApproval { get; set; }

        // IRR approved by PIF
        [JsonProperty("projectIRR")]
        public double? ProjectIRR { get; set; }

        [JsonProperty("iRRAfterGovernmentSubsidies")]
        public double? IRRAfterGovernmentSubsidies { get; set; }

        [JsonProperty("equityIRR")]
        public double? EquityIRR { get; set; }

        [JsonProperty("doesApprovedIRRIncludeLand")]
        public GRTKeyValue DoesApprovedIRRIncludeLand { get; set; }

        [JsonProperty("doesApprovedIRRIncludeInfrastructureCost")]
        public GRTKeyValue DoesApprovedIRRIncludeInfrastructureCost { get; set; }

        [JsonProperty("doesApprovedIRRIncludeGovernmentSubsidies")]
        public GRTKeyValue DoesApprovedIRRIncludeGovernmentSubsidies { get; set; }

        [JsonProperty("projectPaybackYear")]
        public GRTKeyValue ProjectPaybackYear { get; set; }

        [JsonProperty("projectPaybackPeriod")]
        public int? ProjectPaybackPeriod { get; set; }

        // Development Plans
        [JsonProperty("developmentPlanBy2030")]
        public string DevelopmentPlanBy2030 { get; set; }

        [JsonProperty("developmentPlanFullDevelopment")]
        public string DevelopmentPlanFullDevelopment { get; set; }

        // Sources of Funds
        [JsonProperty("sourcesOfFunds")]
        public string SourcesOfFunds { get; set; }

        // Financials
        [JsonProperty("financials")]
        public string Financials { get; set; }

        // Relationship
        [JsonProperty("r_projectToApprovedBPRelationship_c_grtProjectOverviewId")]
        public long? ProjectToApprovedBPRelationshipProjectOverviewId { get; set; }

        [JsonProperty("r_projectToApprovedBPRelationship_c_grtProjectOverviewERC")]
        public string ProjectToApprovedBPRelationshipProjectOverviewERC { get; set; }
    }

    /// <summary>
    /// Response from creating/updating GRT Approved BP
    /// </summary>
    public class GRTApprovedBPResponse
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("externalReferenceCode")]
        public string ExternalReferenceCode { get; set; }

        [JsonProperty("dateCreated")]
        public string DateCreated { get; set; }

        [JsonProperty("dateModified")]
        public string DateModified { get; set; }
    }
    #endregion

    #region Audit Event
    public class GRTAuditEvent
    {
        [JsonProperty("auditFieldChanges")]
        public List<AuditFieldChange> AuditFieldChanges { get; set; }

        [JsonProperty("creator")]
        public GRTCreator Creator { get; set; }

        [JsonProperty("dateCreated")]
        public string DateCreated { get; set; }

        [JsonProperty("eventType")]
        public string EventType { get; set; }
    }

    public class AuditFieldChange
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("newValue")]
        public object NewValue { get; set; }

        [JsonProperty("oldValue")]
        public object OldValue { get; set; }
    }

  
    #endregion
}
