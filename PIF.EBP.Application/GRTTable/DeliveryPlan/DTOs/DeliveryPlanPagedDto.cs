using System.Collections.Generic;

namespace PIF.EBP.Application.GRTTable
{
    /// <summary>
    /// Paginated response for GRT Delivery Plan Tables
    /// </summary>
    public class DeliveryPlanPagedDto
    {
        public List<DeliveryPlanDto> Items { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int LastPage { get; set; }
    }
}
