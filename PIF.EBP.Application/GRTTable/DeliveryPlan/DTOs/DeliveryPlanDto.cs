using PIF.EBP.Core.GRT;
using System;
using System.Collections.Generic;

namespace PIF.EBP.Application.GRTTable
{
    /// <summary>
    /// Application DTO for GRT Delivery Plan Table (stores delivery plans as JSON string)
    /// </summary>
    public class DeliveryPlanDto
    {
        public long? Id { get; set; }
        public string ExternalReferenceCode { get; set; }
        public DateTime? DateCreated { get; set; }
        public DateTime? DateModified { get; set; }

        /// <summary>
        /// JSON string containing array of delivery plans
        /// </summary>
        public string DeliveryPlan { get; set; }

        // Relationships
        public long? ProjectToDeliveryPlanTableRelationshipProjectOverviewId { get; set; }
        public string ProjectToDeliveryPlanTableRelationshipProjectOverviewERC { get; set; }

        public List<GRTAuditEvent> AuditEvents { get; set; }
    }

}
