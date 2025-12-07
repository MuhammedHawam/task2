using Newtonsoft.Json;

namespace PIF.EBP.Application.Commercialization.DTOs.IESMServiceModels
{
    public class SurveyQuestion
    {
        [JsonProperty("question")]
        public string Question { get; set; }

        [JsonProperty("order")]
        public string Order { get; set; }

        [JsonProperty("mandatory")]
        public string Mandatory { get; set; }

        [JsonProperty("datatype")]
        public string Datatype { get; set; }

        [JsonProperty("min")]
        public string Min { get; set; }

        [JsonProperty("max")]
        public string Max { get; set; }
    }
}
