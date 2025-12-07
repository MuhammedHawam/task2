using Newtonsoft.Json;

namespace PIF.EBP.Application.Lookups.DTOs
{
    public class LookupValue
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string NameAr { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string CountryFlag { get; set; }
    }
}
