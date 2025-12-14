using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIF.EBP.Application.GRT.DTOs
{

    public class GRTProjectImpactDto
    {
        public long Id { get; set; }
        public string ExternalReferenceCode { get; set; }
        public DateTime? DateCreated { get; set; }
        public DateTime? DateModified { get; set; }
        public long? ProjectOverviewId { get; set; }
        public string ProjectOverviewERC { get; set; }
        public string ProjectImpactRelationshipERC { get; set; }
        public string AveragePersonalDisposableIncome { get; set; }
        public string EntertainmentSpendHouseholdAnnumSAR { get; set; }
        public string MacroeconomicImpactSection { get; set; }
        public string TotalDomesticOvernightVisits { get; set; }
        public string TotalHotelOvernightVisits { get; set; }
        public string TotalInternationalOvernightVisits { get; set; }
        public string TotalNumberOfEmployees { get; set; }
        public string TotalNumberOfHospitalityStaffLabor { get; set; }
        public string TotalPopulationOfTheProjectSection { get; set; }
    }

    /// <summary>
    /// Paginated DTO for GRT Project Impacts
    /// </summary>
    public class GRTProjectImpactsPagedDto
    {
        public List<GRTProjectImpactDto> Items { get; set; } = new List<GRTProjectImpactDto>();
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int LastPage { get; set; }
    }

    /// <summary>
    /// Response DTO for GRT Project Impact create/update operations
    /// </summary>
    public class GRTProjectImpactResponseDto
    {
        public long Id { get; set; }
        public string ExternalReferenceCode { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateModified { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }
    }

 

}
