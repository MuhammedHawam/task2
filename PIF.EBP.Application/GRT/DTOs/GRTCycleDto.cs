using System;

namespace PIF.EBP.Application.GRT
{
    /// <summary>
    /// DTO for GRT Cycle for frontend
    /// </summary>
    public class GRTCycleDto
    {
        public long Id { get; set; }
        public string ExternalReferenceCode { get; set; }
        public string CycleName { get; set; }
        public DateTime? CycleStartDate { get; set; }
        public DateTime? CycleEndDate { get; set; }
        public string CycleStage { get; set; }
        public string CycleStatusKey { get; set; }
        public DateTime? DateCreated { get; set; }
        public DateTime? DateModified { get; set; }
        public string StatusLabel { get; set; }
        public int SectionsComplete { get; set; }
        public int TotalSections { get; set; }
    }

    /// <summary>
    /// Paginated response for GRT Cycles for frontend
    /// </summary>
    public class GRTCyclesPagedDto
    {
        public System.Collections.Generic.List<GRTCycleDto> Items { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int LastPage { get; set; }
    }
}
