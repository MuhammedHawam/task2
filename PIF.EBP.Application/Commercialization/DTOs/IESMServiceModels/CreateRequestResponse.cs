using Newtonsoft.Json;

namespace PIF.EBP.Application.Commercialization.DTOs.IESMServiceModels
{
    public class CreateRequestResponse
    {
        [JsonProperty("sys_id")]
        public string SysId { get; set; }

        [JsonProperty("number")]
        public string Number { get; set; }
    }

}
