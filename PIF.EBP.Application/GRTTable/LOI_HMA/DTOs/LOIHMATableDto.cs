using PIF.EBP.Core.GRT;
using System;
using System.Collections.Generic;


namespace PIF.EBP.Application.GRTTable.LOI_HMA.DTOs
{
    public class LOIHMATableDto
    {
        public long? Id { get; set; }

        public string ExternalReferenceCode { get; set; }

        public DateTime? DateCreated { get; set; }

        public DateTime? DateModified { get; set; }

        /// <summary>
        /// JSON string containing array of LOI / HMA rows
        /// </summary>
        public string GRTLOITable { get; set; }

        // Relationship to Project Overview
        public long? ProjectToLOIHMATableRelationshipProjectOverviewId { get; set; }

        public string ProjectToLOIHMATableRelationshipProjectOverviewERC { get; set; }

        public List<GRTAuditEvent> AuditEvents { get; set; }
    }

}
