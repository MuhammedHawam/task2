using Newtonsoft.Json;
using PIF.EBP.Core.GRT;
using System.Collections.Generic;

namespace PIF.EBP.Core.GRTTable
{
    /// <summary>
    /// GRT Delivery Plan Table item - stores delivery plans as JSON string
    /// </summary>
    public class DeliveryPlanTable
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

        [JsonProperty("deliveryPlan")]
        public string DeliveryPlan { get; set; }

        [JsonProperty("r_projectToDeliveryPlanTableRelationship_c_grtProjectOverviewERC")]
        public string ProjectToDeliveryPlanTableRelationshipProjectOverviewERC { get; set; }

        [JsonProperty("r_projectToDeliveryPlanTableRelationship_c_grtProjectOverviewId")]
        public long? ProjectToDeliveryPlanTableRelationshipProjectOverviewId { get; set; }

        [JsonProperty("projectToDeliveryPlanTableRelationshipERC")]
        public string ProjectToDeliveryPlanTableRelationshipERC { get; set; }

        [JsonProperty("auditEvents")]
        public List<GRTAuditEvent> AuditEvents { get; set; }
    }

    /// <summary>
    /// Paginated response for GRT Delivery Plan Tables
    /// </summary>
    public class DeliveryPlanTablesPagedResponse
    {
        [JsonProperty("items")]
        public List<DeliveryPlanTable> Items { get; set; }

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
    /// Request for creating/updating GRT Delivery Plan Table
    /// </summary>
    public class DeliveryPlanTableRequest
    {
        [JsonProperty("deliveryPlan")]
        public string DeliveryPlan { get; set; }

        [JsonProperty("r_projectToDeliveryPlanTableRelationship_c_grtProjectOverviewId")]
        public long? ProjectToDeliveryPlanTableRelationshipProjectOverviewId { get; set; }

        [JsonProperty("r_projectToDeliveryPlanTableRelationship_c_grtProjectOverviewERC")]
        public string ProjectToDeliveryPlanTableRelationshipProjectOverviewERC { get; set; }
    }

    /// <summary>
    /// Response from creating/updating GRT Delivery Plan Table
    /// </summary>
    public class DeliveryPlanTableResponse
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
