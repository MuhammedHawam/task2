using Newtonsoft.Json;

namespace PIF.EBP.Application.Commercialization.DTOs.IESMServiceModels
{
    public class SurveyStatus
    {
        [JsonProperty("state")]
        public string State { get; set; }
    }
}
