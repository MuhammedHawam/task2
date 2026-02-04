using Newtonsoft.Json;
using PIF.EBP.Core.GRT;
using System;
using System.Collections.Generic;


namespace PIF.EBP.Core.GRTTable.LandSale.DTOs
{
    public class LandSaleTablesPagedResponse
    {
        public List<LandSaleTable> Items { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int LastPage { get; set; }
    }


    public class LandSaleTable
    {
        public long Id { get; set; }
        public string ExternalReferenceCode { get; set; }
        public string DateCreated { get; set; }
        public string DateModified { get; set; }
        public string LandSales { get; set; }

        public long? ProjectToLandSaleTableRelationshipProjectOverviewId { get; set; }
        public string ProjectToLandSaleTableRelationshipProjectOverviewERC { get; set; }

        public List<GRTAuditEvent> AuditEvents { get; set; }
    }


    public class LandSaleTableRequest
    {
        [JsonProperty("landSales")]
        public string LandSales { get; set; }

        [JsonProperty("r_projectToLandSaleTableRelationship_c_grtProjectOverviewId")]
        public long? ProjectToLandSaleTableRelationshipProjectOverviewId { get; set; }


    }

    public class LandSaleTableCreateRequest
    {
        [JsonProperty("landSales")]
        public string LandSales { get; set; }

        [JsonProperty("externalReferenceCode")]
        public string ExternalReferenceCode { get; set; }

        [JsonProperty("r_projectToLandSaleTableRelationship_c_grtProjectOverviewId")]
        public long ProjectOverviewId { get; set; }
    }


    public class LandSaleTableResponse
    {
        public long Id { get; set; }
        public string ExternalReferenceCode { get; set; }
        public string DateCreated { get; set; }
        public string DateModified { get; set; }

        public string LandSales { get; set; }

        public long? ProjectToLandSaleTableRelationshipProjectOverviewId { get; set; }
        public string ProjectToLandSaleTableRelationshipProjectOverviewERC { get; set; }

        public List<GRTAuditEvent> AuditEvents { get; set; }
    }


    public class LandSaleTableCreateResponse
    {
        public long Id { get; set; }
        public string ExternalReferenceCode { get; set; }
        public string DateCreated { get; set; }
        public string DateModified { get; set; }
        public string LandSales { get; set; }

        public long R_ProjectToLandSaleTableRelationship_C_GrtProjectOverviewId { get; set; }
        public string R_ProjectToLandSaleTableRelationship_C_GrtProjectOverviewERC { get; set; }
    }



}
