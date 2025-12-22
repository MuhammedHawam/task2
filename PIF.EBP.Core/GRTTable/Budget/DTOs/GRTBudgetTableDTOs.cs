using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace PIF.EBP.Core.GRTTable
{
    /// <summary>
    /// Paged response for /o/c/grtbudgettables?... queries.
    /// </summary>
    public class GRTBudgetTablesPagedResponse
    {
        [JsonProperty("actions")]
        public JObject Actions { get; set; }

        [JsonProperty("facets")]
        public JArray Facets { get; set; }

        [JsonProperty("items")]
        public List<GRTBudgetTableItem> Items { get; set; }

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
    /// Single budget table object entry.
    /// Note: matrix fields are stored as JSON strings.
    /// </summary>
    public class GRTBudgetTableItem
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("externalReferenceCode")]
        public string ExternalReferenceCode { get; set; }

        [JsonProperty("dateCreated")]
        public string DateCreated { get; set; }

        [JsonProperty("dateModified")]
        public string DateModified { get; set; }

        [JsonProperty("r_projectToBudgetTableRelationship_c_grtProjectOverviewId")]
        public long? ProjectOverviewId { get; set; }

        [JsonProperty("r_projectToBudgetTableRelationship_c_grtProjectOverviewERC")]
        public string ProjectOverviewERC { get; set; }

        [JsonProperty("projectToBudgetTableRelationshipERC")]
        public string ProjectToBudgetTableRelationshipERC { get; set; }

        [JsonProperty("commitments")]
        public string Commitments { get; set; }

        [JsonProperty("cashDeposits")]
        public string CashDeposits { get; set; }

        [JsonProperty("forecastSpendingBudgetByMonth")]
        public string ForecastSpendingBudgetByMonth { get; set; }

        [JsonProperty("actualSpendingBudgetByMonth")]
        public string ActualSpendingBudgetByMonth { get; set; }

        [JsonProperty("actions")]
        public JObject Actions { get; set; }

        // Any additional fields returned by Liferay
        [JsonExtensionData]
        public IDictionary<string, JToken> AdditionalData { get; set; }
    }
}

