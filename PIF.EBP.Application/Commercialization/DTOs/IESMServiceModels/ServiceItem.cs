using Newtonsoft.Json;

namespace PIF.EBP.Application.Commercialization.DTOs.IESMServiceModels
{
    public class ServiceItem
    {
        [JsonProperty("type")]
        public string Type { get; set; }
        [JsonProperty("sys_id")]
        public string SysId { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("short_description")]
        public string ShortDescription { get; set; }
        [JsonProperty("description")]
        public string Description { get; set; }
        [JsonProperty("icon")]
        public string Icon { get; set; }
        [JsonProperty("price")]
        public string Price { get; set; }
        [JsonProperty("recurring_price")]
        public string RecurringPrice { get; set; }
        [JsonProperty("parent")]
        public ParentServiceItem Parent { get; set; }

    }

    public class ParentServiceItem
    {
        [JsonProperty("sys_id")]
        public string SysId { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
    }
}
