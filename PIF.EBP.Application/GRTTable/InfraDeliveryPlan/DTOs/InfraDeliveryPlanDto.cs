using PIF.EBP.Core.GRT;
using System;
using System.Collections.Generic;

namespace PIF.EBP.Application.GRTTable
{
    /// <summary>
    /// Application DTO for GRT Infrastructure Delivery Plan Table (stores infra delivery plans as JSON string)
    /// </summary>
    public class InfraDeliveryPlanDto
    {
        public long? Id { get; set; }
        public string ExternalReferenceCode { get; set; }
        public DateTime? DateCreated { get; set; }
        public DateTime? DateModified { get; set; }

        /// <summary>
        /// JSON string containing infrastructure delivery plan matrix data
        /// Example: {"cols":["Total","2015","2016",...],"rows":{"INFRDCPX":[0,null,...],...}}
        /// </summary>
        public string InfraDeliveryPlanTableJson { get; set; }

        // Relationships
        public long? ProjectToInfraDeliveryPlanRelationshipTabProjectOverviewId { get; set; }
        public string ProjectToInfraDeliveryPlanRelationshipTabProjectOverviewERC { get; set; }

        public List<GRTAuditEvent> AuditEvents { get; set; }
    }

}
