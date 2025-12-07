using Newtonsoft.Json;
using System.Collections.Generic;

namespace PIF.EBP.Application.Commercialization.DTOs.IESMServiceModels
{
    public class FieldsMetadata
    {
        [JsonProperty("static_fields")]
        public List<StaticField> StaticFields { get; set; }

        [JsonProperty("dynamic_fields")]
        public List<DynamicField> DynamicFields { get; set; }
    }

    public class StaticField
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("label")]
        public string Label { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("rules")]
        public string Rules { get; set; }

        [JsonProperty("mandatory")]
        public bool Mandatory { get; set; }

        [JsonProperty("order")]
        public string Order { get; set; }

        [JsonProperty("choices")]
        public List<Choice> Choices { get; set; }
    }

    public class DynamicField : StaticField
    {
        [JsonProperty("condition")]
        public string Condition { get; set; }
    }
    public class Choice
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("label")]
        public string Label { get; set; }
    }
}
