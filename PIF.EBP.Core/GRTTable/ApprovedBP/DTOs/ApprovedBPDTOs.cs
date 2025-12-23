using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace PIF.EBP.Core.GRTTable
{
    public class GRTKeyNameValue
    {
        [JsonProperty("key")]
        public string Key { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public class GRTApprovedBPsPagedResponse
    {
        [JsonProperty("actions")]
        public JObject Actions { get; set; }

        [JsonProperty("facets")]
        public JArray Facets { get; set; }

        [JsonProperty("items")]
        public List<GRTApprovedBPItem> Items { get; set; }

        [JsonProperty("lastPage")]
        public int LastPage { get; set; }

        [JsonProperty("page")]
        public int Page { get; set; }

        [JsonProperty("pageSize")]
        public int PageSize { get; set; }

        [JsonProperty("totalCount")]
        public int TotalCount { get; set; }
    }

    public class GRTApprovedBPItem
    {
        [JsonProperty("actions")]
        public JObject Actions { get; set; }

        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("externalReferenceCode")]
        public string ExternalReferenceCode { get; set; }

        [JsonProperty("r_projectToApprovedBPRelationship_c_grtProjectOverviewId")]
        public long? ProjectOverviewId { get; set; }

        [JsonProperty("pIFDateOfApproval")]
        public string PIFDateOfApproval { get; set; }

        [JsonProperty("firstInfrastructureStartDate")]
        public GRTKeyNameValue FirstInfrastructureStartDate { get; set; }

        // Some responses contain a typo field name "firstInfrastructureStartSate".
        [JsonProperty("firstInfrastructureStartSate")]
        public GRTKeyNameValue FirstInfrastructureStartSate { get; set; }

        [JsonProperty("lastInfrastructureCompleteDate")]
        public GRTKeyNameValue LastInfrastructureCompleteDate { get; set; }

        [JsonProperty("firstVerticalConstructionStartDate")]
        public GRTKeyNameValue FirstVerticalConstructionStartDate { get; set; }

        [JsonProperty("lastVerticalConstructionCompleteDate")]
        public GRTKeyNameValue LastVerticalConstructionCompleteDate { get; set; }

        [JsonProperty("operationsStartDate")]
        public GRTKeyNameValue OperationsStartDate { get; set; }

        [JsonProperty("lastYearOfFundingRequired")]
        public GRTKeyNameValue LastYearOfFundingRequired { get; set; }

        [JsonExtensionData]
        public IDictionary<string, JToken> AdditionalData { get; set; }
    }

    public class GRTApprovedBPCreateRequest
    {
        [JsonProperty("firstInfrastructureStartDate")]
        public GRTKeyNameValue FirstInfrastructureStartDate { get; set; }

        [JsonProperty("lastInfrastructureCompleteDate")]
        public GRTKeyNameValue LastInfrastructureCompleteDate { get; set; }

        [JsonProperty("firstVerticalConstructionStartDate")]
        public GRTKeyNameValue FirstVerticalConstructionStartDate { get; set; }

        [JsonProperty("lastVerticalConstructionCompleteDate")]
        public GRTKeyNameValue LastVerticalConstructionCompleteDate { get; set; }

        [JsonProperty("operationsStartDate")]
        public GRTKeyNameValue OperationsStartDate { get; set; }

        [JsonProperty("lastYearOfFundingRequired")]
        public GRTKeyNameValue LastYearOfFundingRequired { get; set; }

        [JsonProperty("pIFDateOfApproval")]
        public string PIFDateOfApproval { get; set; }

        [JsonProperty("r_projectToApprovedBPRelationship_c_grtProjectOverviewId")]
        public long? ProjectOverviewId { get; set; }

        [JsonExtensionData]
        public IDictionary<string, JToken> AdditionalData { get; set; }
    }
}

