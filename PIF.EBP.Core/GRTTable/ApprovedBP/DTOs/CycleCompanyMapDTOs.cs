using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace PIF.EBP.Core.GRTTable
{
    public class GRTCycleCompanyMapItem
    {
        [JsonProperty("actions")]
        public JObject Actions { get; set; }

        [JsonProperty("creator")]
        public JObject Creator { get; set; }

        [JsonProperty("dateCreated")]
        public string DateCreated { get; set; }

        [JsonProperty("dateModified")]
        public string DateModified { get; set; }

        [JsonProperty("externalReferenceCode")]
        public string ExternalReferenceCode { get; set; }

        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("status")]
        public JObject Status { get; set; }

        [JsonProperty("r_companyInCyclesRelationship_c_companiesId")]
        public long? CompanyId { get; set; }

        [JsonProperty("r_cycleHasCompaniesRelationship_c_grtCyclesId")]
        public long? GrtCycleId { get; set; }

        [JsonProperty("poId")]
        public string PoId { get; set; }

        [JsonExtensionData]
        public IDictionary<string, JToken> AdditionalData { get; set; }
    }
}

