using System;

namespace PIF.EBP.Application.GRTTable
{
    /// <summary>
    /// Response DTO for creating/updating Delivery Plan Table
    /// </summary>
    public class DeliveryPlanResponseDto
    {
        public long Id { get; set; }
        public string ExternalReferenceCode { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateModified { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }
    }
}
