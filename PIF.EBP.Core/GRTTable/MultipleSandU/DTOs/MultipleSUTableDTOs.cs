using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace PIF.EBP.Core.GRTTable.MultipleSandU.DTOs
{
    /// <summary>
    /// Paged response for /o/c/grtmultiplesutables?... queries.
    /// </summary>
    public class GRTMultipleSUTablesPagedResponse
    {
        [JsonProperty("actions")]
        public JObject Actions { get; set; }

        [JsonProperty("facets")]
        public JArray Facets { get; set; }

        [JsonProperty("items")]
        public List<GRTMultipleSUTableItem> Items { get; set; }

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
    /// Single Multiple S&U table object entry.
    /// Note: matrix fields are stored as JSON strings.
    /// </summary>
    public class GRTMultipleSUTableItem
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("externalReferenceCode")]
        public string ExternalReferenceCode { get; set; }

        [JsonProperty("dateCreated")]
        public string DateCreated { get; set; }

        [JsonProperty("dateModified")]
        public string DateModified { get; set; }

        [JsonProperty("r_projectToMultipleSUTableRelationship_c_grtProjectOverviewId")]
        public long? ProjectOverviewId { get; set; }

        [JsonProperty("r_projectToMultipleSUTableRelationship_c_grtProjectOverviewERC")]
        public string ProjectOverviewERC { get; set; }

        [JsonProperty("projectToMultipleSUTableRelationshipERC")]
        public string ProjectToMultipleSUTableRelationshipERC { get; set; }

        [JsonProperty("financialsSARJson")]
        public string FinancialsSARJson { get; set; }

        [JsonProperty("capexJson")]
        public string CapexJson { get; set; }

        [JsonProperty("opexJson")]
        public string OpexJson { get; set; }

        [JsonProperty("totalSourcesJson")]
        public string TotalSourcesJson { get; set; }

        [JsonProperty("actions")]
        public JObject Actions { get; set; }

        [JsonExtensionData]
        public IDictionary<string, JToken> AdditionalData { get; set; }
    }

    /// <summary>
    /// Update request payload for /o/c/grtmultiplesutables/{id}
    /// </summary>
    public class GRTMultipleSUTableUpdateRequest
    {
        [JsonProperty("externalReferenceCode")]
        public string ExternalReferenceCode { get; set; }

        [JsonProperty("capexJson")]
        public string CapexJson { get; set; }

        [JsonProperty("opexJson")]
        public string OpexJson { get; set; }

        [JsonProperty("totalSourcesJson")]
        public string TotalSourcesJson { get; set; }

        [JsonProperty("financialsSARJson")]
        public string FinancialsSARJson { get; set; }

        [JsonProperty("r_projectToMultipleSUTableRelationship_c_grtProjectOverviewId")]
        public long? ProjectOverviewId { get; set; }
    }
}

