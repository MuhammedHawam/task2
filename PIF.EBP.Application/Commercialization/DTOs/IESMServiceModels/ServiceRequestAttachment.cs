using Newtonsoft.Json;

namespace PIF.EBP.Application.Commercialization.DTOs.IESMServiceModels
{
    public class ServiceRequestAttachment
    {
        [JsonProperty("file_name")]
        public string FileName { get; set; }

        [JsonProperty("content_type")]
        public string ContentType { get; set; }

        [JsonProperty("sys_id")]
        public string SysId { get; set; }

        [JsonProperty("size")]
        public string Size { get; set; }

        [JsonProperty("issue_date")]
        public string IssueDate { get; set; }
    }
}
