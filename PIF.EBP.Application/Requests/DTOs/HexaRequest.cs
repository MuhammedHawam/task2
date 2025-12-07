using PIF.EBP.Application.MetaData.DTOs;
using System;

namespace PIF.EBP.Application.Requests.DTOs
{
    public class HexaRequest
    {
        public Guid Id { get; set; }
        public string RequestNumber { get; set; }
        public string RequestName { get; set; }
        public string Title { get; set; }
        public string TitleAr { get; set; }
        public string DealOverview { get; set; }
        public string DealOverviewAr { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime DueOnDate { get; set; }
        public EntityReferenceDto Stage { get; set; }
        public EntityReferenceDto InternalStatus { get; set; }
        public EntityReferenceDto ExternalStatus { get; set; }
        public EntityReferenceDto Owner { get; set; }
        public EntityReferenceDto ProcessTemplate { get; set; }
        public EntityReferenceDto CurrentStep { get; set; }
        public EntityOptionSetDto StateCode { get; set; }
    }
}
