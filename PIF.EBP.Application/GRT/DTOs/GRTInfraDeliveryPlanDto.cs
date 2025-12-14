using System;
using System.Collections.Generic;

namespace PIF.EBP.Application.GRT
{
    /// <summary>
    /// DTO for GRT Infrastructure Delivery Plan list item (simplified for list view)
    /// </summary>
    public class GRTInfraDeliveryPlanListDto
    {
        public long Id { get; set; }
        public string InfrastructureType { get; set; }
        public string InfrastructureTypeKey { get; set; }
        public string InfrastructureSector { get; set; }
        public string InfrastructureSectorKey { get; set; }
        public double? Total { get; set; }
    }

    /// <summary>
    /// DTO for creating/updating GRT Infrastructure Delivery Plan
    /// </summary>
    public class GRTInfraDeliveryPlanDto
    {
        public string InfrastructureTypeKey { get; set; }
        public string InfrastructureSectorKey { get; set; }
        public long? ProjectToInfraDeliveryPlanRelationshipProjectOverviewId { get; set; }
        public string ProjectToInfraDeliveryPlanRelationshipProjectOverviewERC { get; set; }
        
        // Year entries
        public List<GRTInfraDeliveryPlanYearDto> Years { get; set; }
    }

    /// <summary>
    /// Response DTO for GRT Infrastructure Delivery Plan creation/update
    /// </summary>
    public class GRTInfraDeliveryPlanResponseDto
    {
        public long Id { get; set; }
        public string ExternalReferenceCode { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateModified { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }
    }

    /// <summary>
    /// DTO for GRT Infrastructure Delivery Plan details (full details for get by ID)
    /// </summary>
    public class GRTInfraDeliveryPlanDetailDto
    {
        public long Id { get; set; }
        public string ExternalReferenceCode { get; set; }
        public DateTime? DateCreated { get; set; }
        public DateTime? DateModified { get; set; }
        
        public string InfrastructureType { get; set; }
        public string InfrastructureTypeKey { get; set; }
        public string InfrastructureSector { get; set; }
        public string InfrastructureSectorKey { get; set; }
        public double? Total { get; set; }
        
        public long? ProjectToInfraDeliveryPlanRelationshipProjectOverviewId { get; set; }
        public string ProjectToInfraDeliveryPlanRelationshipProjectOverviewERC { get; set; }
        
        // Year entries
        public List<GRTInfraDeliveryPlanYearDto> Years { get; set; }
    }

    /// <summary>
    /// Paginated response for GRT Infrastructure Delivery Plans list
    /// </summary>
    public class GRTInfraDeliveryPlansPagedDto
    {
        public List<GRTInfraDeliveryPlanListDto> Items { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int LastPage { get; set; }
    }

    /// <summary>
    /// DTO for GRT Infrastructure Delivery Plan Year entry
    /// </summary>
    public class GRTInfraDeliveryPlanYearDto
    {
        public long? Id { get; set; }
        public string ActualPlannedKey { get; set; }
        public string ActualPlanned { get; set; }
        public string YearKey { get; set; }
        public string Year { get; set; }
        public double? Amount { get; set; }
    }
}
