using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace PIF.EBP.Core.GRTTable
{
    /// <summary>
    /// Paged response for /o/c/grtprojectoverviews?... queries (used by GRT tables).
    /// </summary>
    public class GRTProjectOverviewsPagedResponse
    {
        [JsonProperty("actions")]
        public JObject Actions { get; set; }

        [JsonProperty("facets")]
        public JArray Facets { get; set; }

        [JsonProperty("items")]
        public List<GRTProjectOverviewListItem> Items { get; set; }

        [JsonProperty("lastPage")]
        public int LastPage { get; set; }

        [JsonProperty("page")]
        public int Page { get; set; }

        [JsonProperty("pageSize")]
        public int PageSize { get; set; }

        [JsonProperty("totalCount")]
        public int TotalCount { get; set; }
    }

    /// <summary>
    /// A single project overview item. We keep most fields as extension data to stay resilient to schema changes.
    /// </summary>
    public class GRTProjectOverviewListItem
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("externalReferenceCode")]
        public string ExternalReferenceCode { get; set; }

        [JsonProperty("dateCreated")]
        public string DateCreated { get; set; }

        [JsonProperty("dateModified")]
        public string DateModified { get; set; }

        [JsonProperty("r_gRTCycleCompanyMapRelationship_c_cycleCompanyMapId")]
        public long? CycleCompanyMapId { get; set; }

        [JsonProperty("r_gRTCycleCompanyMapRelationship_c_cycleCompanyMapERC")]
        public string CycleCompanyMapERC { get; set; }

        [JsonProperty("actions")]
        public JObject Actions { get; set; }

        // Any additional fields returned by Liferay
        [JsonExtensionData]
        public IDictionary<string, JToken> AdditionalData { get; set; }
    }
}

