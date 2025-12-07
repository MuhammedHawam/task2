using Microsoft.Xrm.Sdk;
using System;

namespace PIF.EBP.Application.Requests.DTOs
{
    public class HexaRequestStep
    {
        public Guid HexaRequestStepId { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? HexaDueOn { get; set; }
        public EntityReference HexaStepStatus { get; set; }
        public EntityReference HexaRequestStatus { get; set; }
        public EntityReference HexaRequest { get; set; }
        public OptionSetValue StateCode { get; set; }
        public int? HexaType { get; set; }
    }
}
