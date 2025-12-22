using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace PIF.EBP.Core.GRTTable
{

    /// <summary>
    /// Response from GRT API for list type definitions
    /// </summary>
    public class GRTListTypeDefinitionResponse
    {
        [JsonProperty("externalReferenceCode")]
        public string ExternalReferenceCode { get; set; }

        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("listTypeEntries")]
        public List<GRTListTypeEntry> ListTypeEntries { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("name_i18n")]
        public GRTNameI18n Name_i18n { get; set; }


    }

    /// <summary>
    /// Individual entry in a GRT list type
    /// </summary>
    public class GRTListTypeEntry
    {
        [JsonProperty("externalReferenceCode")]
        public string ExternalReferenceCode { get; set; }

        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("key")]
        public string Key { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("name_i18n")]
        public GRTNameI18n Name_i18n { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }
    }

    /// <summary>
    /// Internationalized name for GRT entries
    /// </summary>
    public class GRTNameI18n
    {
        [JsonProperty("ar-SA")]
        public string ArSA { get; set; }

        [JsonProperty("en-US")]
        public string EnUS { get; set; }

    }

    /// <summary>
    /// Response from Liferay list-type-entries endpoint
    /// </summary>
    public class GRTListTypeEntriesPagedResponse
    {
        [JsonProperty("actions")]
        public JObject Actions { get; set; }

        [JsonProperty("facets")]
        public JArray Facets { get; set; }

        [JsonProperty("items")]
        public List<GRTListTypeEntryItem> Items { get; set; }

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
    /// Item in list-type-entries response
    /// </summary>
    public class GRTListTypeEntryItem
    {
        [JsonProperty("actions")]
        public JObject Actions { get; set; }

        [JsonProperty("dateCreated")]
        public string DateCreated { get; set; }

        [JsonProperty("dateModified")]
        public string DateModified { get; set; }

        [JsonProperty("externalReferenceCode")]
        public string ExternalReferenceCode { get; set; }

        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("key")]
        public string Key { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("name_i18n")]
        public GRTNameI18n Name_i18n { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }
    }
}
