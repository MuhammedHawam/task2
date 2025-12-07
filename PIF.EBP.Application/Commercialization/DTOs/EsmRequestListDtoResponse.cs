using PIF.EBP.Application.Commercialization.DTOs.IESMServiceModels;
using PIF.EBP.Application.MetaData.DTOs;
using System;

namespace PIF.EBP.Application.Commercialization.DTOs
{
    public class EsmRequestListDtoResponse
    {
        public string RequestId { get; set; }
        public string RequestNumber { get; set; }
        public string ServiceName { get; set; }
        public string ServiceType { get; set; }
        public string ServiceSubtype { get; set; }
        public EntityReferenceDto Initiator { get; set; }
        public EsmOptionsDto Status { get; set; }
        public DateTime InitiationDate { get; set; }
    }
}
