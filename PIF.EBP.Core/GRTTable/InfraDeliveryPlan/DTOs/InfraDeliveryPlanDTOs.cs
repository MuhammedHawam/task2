using Newtonsoft.Json;
using PIF.EBP.Core.GRT;
using System.Collections.Generic;

namespace PIF.EBP.Core.GRTTable
{
    /// <summary>
    /// GRT Infrastructure Delivery Plan Table item - stores infra delivery plans as JSON string
    /// </summary>
    public class InfraDeliveryPlanTable
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("externalReferenceCode")]
        public string ExternalReferenceCode { get; set; }

        [JsonProperty("dateCreated")]
        public string DateCreated { get; set; }

        [JsonProperty("dateModified")]
        public string DateModified { get; set; }

        [JsonProperty("creator")]
        public GRTCreator Creator { get; set; }

        [JsonProperty("status")]
        public GRTStatus Status { get; set; }

        [JsonProperty("gRTInfraDeliveryPlanTable")]
        public string InfraDeliveryPlanTableJson { get; set; }

        [JsonProperty("r_projectToInfraDeliveryPlanRelationshipTab_c_grtProjectOverviewERC")]
        public string ProjectToInfraDeliveryPlanRelationshipTabProjectOverviewERC { get; set; }

        [JsonProperty("r_projectToInfraDeliveryPlanRelationshipTab_c_grtProjectOverviewId")]
        public long? ProjectToInfraDeliveryPlanRelationshipTabProjectOverviewId { get; set; }

        [JsonProperty("projectToInfraDeliveryPlanRelationshipTabERC")]
        public string ProjectToInfraDeliveryPlanRelationshipTabERC { get; set; }

        [JsonProperty("auditEvents")]
        public List<GRTAuditEvent> AuditEvents { get; set; }
    }

    /// <summary>
    /// Paginated response for GRT Infrastructure Delivery Plan Tables
    /// </summary>
    public class InfraDeliveryPlanTablesPagedResponse
    {
        [JsonProperty("items")]
        public List<InfraDeliveryPlanTable> Items { get; set; }

        [JsonProperty("page")]
        public int Page { get; set; }

        [JsonProperty("pageSize")]
        public int PageSize { get; set; }

        [JsonProperty("totalCount")]
        public int TotalCount { get; set; }

        [JsonProperty("lastPage")]
        public int LastPage { get; set; }
    }

    /// <summary>
    /// Request for creating/updating GRT Infrastructure Delivery Plan Table
    /// </summary>
    public class InfraDeliveryPlanTableRequest
    {
        [JsonProperty("gRTInfraDeliveryPlanTable")]
        public string InfraDeliveryPlanTableJson { get; set; }

        [JsonProperty("r_projectToInfraDeliveryPlanRelationshipTab_c_grtProjectOverviewId")]
        public long? ProjectToInfraDeliveryPlanRelationshipTabProjectOverviewId { get; set; }

        [JsonProperty("r_projectToInfraDeliveryPlanRelationshipTab_c_grtProjectOverviewERC")]
        public string ProjectToInfraDeliveryPlanRelationshipTabProjectOverviewERC { get; set; }
    }

    /// <summary>
    /// Response from creating/updating GRT Infrastructure Delivery Plan Table
    /// </summary>
    public class InfraDeliveryPlanTableResponse
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("externalReferenceCode")]
        public string ExternalReferenceCode { get; set; }

        [JsonProperty("dateCreated")]
        public string DateCreated { get; set; }

        [JsonProperty("dateModified")]
        public string DateModified { get; set; }
    }
}
