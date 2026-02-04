using PIF.EBP.Core.GRT;
using System;
using System.Collections.Generic;


namespace PIF.EBP.Application.GRTTable.LandSale.DTOs
{
    public class LandSaleDto
    {
        public long? Id { get; set; }
        public string ExternalReferenceCode { get; set; }
        public DateTime? DateCreated { get; set; }
        public DateTime? DateModified { get; set; }

        /// <summary>
        /// JSON string containing array of land sales
        /// </summary>
        public string LandSales { get; set; }

        // Relationships
        public long? ProjectToLandSaleTableRelationshipProjectOverviewId { get; set; }
        public string ProjectToLandSaleTableRelationshipProjectOverviewERC { get; set; }

        public List<GRTAuditEvent> AuditEvents { get; set; }
    }


    public class LandSaleTableCreateDto
    {
        public long? Id { get; set; }
        public string ExternalReferenceCode { get; set; }
        public DateTime? DateCreated { get; set; }
        public DateTime? DateModified { get; set; }

        /// <summary>
        /// JSON string containing array of land sale entries
        /// </summary>
        public string LandSales { get; set; }

        // Relationships
        public long? ProjectOverviewId { get; set; }
        public string ProjectToLandSaleTableRelationshipERC { get; set; }
    }

}
