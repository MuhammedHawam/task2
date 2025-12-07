using Newtonsoft.Json;
using System.Collections.Generic;

namespace PIF.EBP.Application.Commercialization.DTOs.IESMServiceModels
{
    public class SurveyForm
    {
        [JsonProperty("user_id")]
        public string UserId { get; set; }

        [JsonProperty("user")]
        public string User { get; set; }

        [JsonProperty("company_id")]
        public string CompanyId { get; set; }

        [JsonProperty("company")]
        public string Company { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("result")]
        public List<SurveyResultForm> Result { get; set; } = new List<SurveyResultForm>();
    }
    public class SurveyResultForm
    {

        [JsonProperty("question")]
        public string Question { get; set; }

        [JsonProperty("answer")]
        public string Answer { get; set; }
    } 
}
