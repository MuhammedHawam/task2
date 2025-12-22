using Newtonsoft.Json;
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
}
