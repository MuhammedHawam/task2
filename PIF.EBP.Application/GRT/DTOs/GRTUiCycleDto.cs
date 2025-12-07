using System;

namespace PIF.EBP.Application.GRT
{
    /// <summary>
    /// UI-friendly DTO for GRT Cycle with combined data from cycle company map
    /// </summary>
    public class GRTUiCycleDto
    {
        public long CycleId { get; set; }
        public string PoId { get; set; }
        public long CompanyId { get; set; }
        public string CycleName { get; set; }
        public DateTime? CycleStartDate { get; set; }
        public DateTime? CycleEndDate { get; set; }
        public string Status { get; set; }
        public string RawCycleCompanyStatus { get; set; }
        public string RawSystemStatus { get; set; }
    }

    /// <summary>
    /// Paginated response for UI-friendly GRT Cycles with Active/Previous separation
    /// </summary>
    public class GRTUiCyclesPagedDto
    {
        public System.Collections.Generic.List<GRTUiCycleDto> ActiveCycles { get; set; }
        public System.Collections.Generic.List<GRTUiCycleDto> PreviousCycles { get; set; }
        
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int LastPage { get; set; }
    }
}
