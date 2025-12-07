using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace PIF.EBP.Application.Commercialization.DTOs.IESMServiceModels
{
    public class ServiceRequestResponse
    {
        [JsonProperty("count")]
        public int Count { get; set; }
        [JsonProperty("requested_items")]
        public List<ServiceRequest> RequestedItems { get; set; }
    }
    public class ServiceRequest
    {
        [JsonProperty("number")]
        public string Number { get; set; }

        [JsonProperty("sys_id")]
        public string SysId { get; set; }

        [JsonProperty("service_name")]
        public string ServiceName { get; set; }

        [JsonProperty("requested_by")]
        public string RequestedBy { get; set; }

        [JsonProperty("initiation_date")]
        public DateTime InitiationDate { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }

        [JsonProperty("company_id")]
        public string CompanyId { get; set; }

        [JsonProperty("due_date")]
        public DateTime DueDate { get; set; }

        [JsonProperty("updated")]
        public DateTime Updated { get; set; }

        [JsonProperty("updated_by")]
        public string UpdatedBy { get; set; }

        [JsonProperty("survey_status")]
        public string SurveyStatus { get; set; }
    }
}
