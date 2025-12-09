using System;
using System.Collections.Generic;

namespace PIF.EBP.Application.GRT
{
    /// <summary>
    /// DTO for GRT Delivery Plan list item (simplified for list view)
    /// </summary>
    public class GRTDeliveryPlanListDto
    {
        public long Id { get; set; }
        public string PlanNumber { get; set; }
        public string ParcelID { get; set; }
        public string AssetID { get; set; }
        public string AssetName { get; set; }
    }

    /// <summary>
    /// DTO for creating/updating GRT Delivery Plan
    /// </summary>
    public class GRTDeliveryPlanDto
    {
        // ASSET IDENTIFICATION
        public string PlanNumber { get; set; }
        public string ParcelID { get; set; }
        public string AssetID { get; set; }
        public string AssetName { get; set; }
        public string AssetType { get; set; }
        public string SubAsset { get; set; }
        public string Description { get; set; }

        // LOCATION DETAILS
        public string City { get; set; }
        public string RegionKey { get; set; }
        public string ProgramAssetPackage { get; set; }

        // DEVELOPMENT PROFILE
        public string TypeOfDevelopmentKey { get; set; }
        public string PrimarySecondaryHomesForResidentialKey { get; set; }
        public string BrandedNonBrandedKey { get; set; }
        public string RetailTypeKey { get; set; }
        public string Phase { get; set; }
        public string ConstructionStartKey { get; set; }
        public string AssetStartOperatingDeliveryDateKey { get; set; }

        // ASSET SPECIFICATION
        public string ParkingBaysLinkedToAsset { get; set; }
        public string NumberOfFloorsForBuildingsOnly { get; set; }
        public double? LandTake { get; set; }
        public double? BUA { get; set; }
        public double? GFA { get; set; }
        public double? GLA { get; set; }
        public int? ResidentialHospitalityKeysLaborStaffRooms { get; set; }

        // FINANCIAL INPUTS
        public string DevelopmentIsFundedByKey { get; set; }
        public string RevenueDriverKey { get; set; }
        public string SaleForecastYearKey { get; set; }
        public string SaleStrategyKey { get; set; }
        public double? AvgSaleRate { get; set; }
        public double? LeaseRateOrADRForKeys { get; set; }
        public double? OccupancyInFirstYearOfOperation { get; set; }
        public string YearOfStabilizationKey { get; set; }
        public double? StableLeaseADR { get; set; }
        public double? StableOccupancy { get; set; }

        // COST BREAKDOWN
        public double? ValueOfLandAllocatedToAsset { get; set; }
        public double? ValueOfInfrastructureAllocatedToAsset { get; set; }
        public double? SoftCost { get; set; }
        public double? Contingencies { get; set; }
        public double? VerticalConstructionCost { get; set; }

        // RETURN & PERFORMANCE
        public double? RevenueProceeds { get; set; }
        public double? UnleveredIRR { get; set; }
        public double? VerticalConstructionCostPerSqm { get; set; }
        public double? TotalDevelopmentCost { get; set; }
        public double? TotalDevelopmentCostPerSqm { get; set; }

        // ADDITIONAL NOTES
        public string Comments { get; set; }

        // Relationships
        public long? ProjectToDeliveryPlanRelationshipProjectOverviewId { get; set; }
        public string ProjectToDeliveryPlanRelationshipProjectOverviewERC { get; set; }
    }

    /// <summary>
    /// Response DTO for GRT Delivery Plan creation/update
    /// </summary>
    public class GRTDeliveryPlanResponseDto
    {
        public long Id { get; set; }
        public string ExternalReferenceCode { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateModified { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }
    }

    /// <summary>
    /// DTO for GRT Delivery Plan details (full details for get by ID)
    /// </summary>
    public class GRTDeliveryPlanDetailDto
    {
        public long Id { get; set; }
        public string ExternalReferenceCode { get; set; }
        public DateTime? DateCreated { get; set; }
        public DateTime? DateModified { get; set; }

        // ASSET IDENTIFICATION
        public string PlanNumber { get; set; }
        public string ParcelID { get; set; }
        public string AssetID { get; set; }
        public string AssetName { get; set; }
        public string AssetType { get; set; }
        public string SubAsset { get; set; }
        public string Description { get; set; }

        // LOCATION DETAILS
        public string City { get; set; }
        public string Region { get; set; }
        public string RegionKey { get; set; }
        public string ProgramAssetPackage { get; set; }

        // DEVELOPMENT PROFILE
        public string TypeOfDevelopment { get; set; }
        public string TypeOfDevelopmentKey { get; set; }
        public string PrimarySecondaryHomesForResidential { get; set; }
        public string PrimarySecondaryHomesForResidentialKey { get; set; }
        public string BrandedNonBranded { get; set; }
        public string BrandedNonBrandedKey { get; set; }
        public string RetailType { get; set; }
        public string RetailTypeKey { get; set; }
        public string Phase { get; set; }
        public string ConstructionStart { get; set; }
        public string ConstructionStartKey { get; set; }
        public string AssetStartOperatingDeliveryDate { get; set; }
        public string AssetStartOperatingDeliveryDateKey { get; set; }

        // ASSET SPECIFICATION
        public string ParkingBaysLinkedToAsset { get; set; }
        public string NumberOfFloorsForBuildingsOnly { get; set; }
        public double? LandTake { get; set; }
        public double? BUA { get; set; }
        public double? GFA { get; set; }
        public double? GLA { get; set; }
        public int? ResidentialHospitalityKeysLaborStaffRooms { get; set; }

        // FINANCIAL INPUTS
        public string DevelopmentIsFundedBy { get; set; }
        public string DevelopmentIsFundedByKey { get; set; }
        public string RevenueDriver { get; set; }
        public string RevenueDriverKey { get; set; }
        public string SaleForecastYear { get; set; }
        public string SaleForecastYearKey { get; set; }
        public string SaleStrategy { get; set; }
        public string SaleStrategyKey { get; set; }
        public double? AvgSaleRate { get; set; }
        public double? LeaseRateOrADRForKeys { get; set; }
        public double? OccupancyInFirstYearOfOperation { get; set; }
        public string YearOfStabilization { get; set; }
        public string YearOfStabilizationKey { get; set; }
        public double? StableLeaseADR { get; set; }
        public double? StableOccupancy { get; set; }

        // COST BREAKDOWN
        public double? ValueOfLandAllocatedToAsset { get; set; }
        public double? ValueOfInfrastructureAllocatedToAsset { get; set; }
        public double? SoftCost { get; set; }
        public double? Contingencies { get; set; }
        public double? VerticalConstructionCost { get; set; }

        // RETURN & PERFORMANCE
        public double? RevenueProceeds { get; set; }
        public double? UnleveredIRR { get; set; }
        public double? VerticalConstructionCostPerSqm { get; set; }
        public double? TotalDevelopmentCost { get; set; }
        public double? TotalDevelopmentCostPerSqm { get; set; }

        // ADDITIONAL NOTES
        public string Comments { get; set; }

        // Relationships
        public long? ProjectToDeliveryPlanRelationshipProjectOverviewId { get; set; }
        public string ProjectToDeliveryPlanRelationshipProjectOverviewERC { get; set; }
    }

    /// <summary>
    /// Paginated response for GRT Delivery Plans list
    /// </summary>
    public class GRTDeliveryPlansPagedDto
    {
        public List<GRTDeliveryPlanListDto> Items { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int LastPage { get; set; }
    }
}
