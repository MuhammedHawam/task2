using PIF.EBP.Application.ExternalFormConfiguration.DTOs;
using PIF.EBP.Application.MetaData.DTOs;
using PIF.EBP.Application.Shared.AppResponse;
using System;
using System.Collections.Generic;

namespace PIF.EBP.Application.Requests.DTOs
{
    public class HexaRequestListDto_RES
    {
        public int TotalNewRequestsToday { get; set; } = 0;
        public int TotalDueNext2DaysRequests { get; set; } = 0;
        public int TotalOverDueRequests { get; set; } = 0;
        public int TotalPendingRequests { get; set; } = 0;
        public ListPagingResponse<HexaRequestList> RequestsList { get; set; } = new ListPagingResponse<HexaRequestList>();
    }
    public class HexaRequestList
    {
        public Guid RequestId { get; set; }
        public string RequestNumber { get; set; }
        public string RequestTitle { get; set; }
        public string RequestTitleAr { get; set; }
        public string OverView { get; set; }
        public string OverViewAr { get; set; }
        public EntityReferenceDto ExternalStatus { get; set; }
        public DateTime? DueOnDate { get; set; }
        public DateTime CreationDate { get; set; }
        public int StateCode { get; set; }
        public bool OverDue { get; set; }
        public List<ExternalFormConfigSimplifiedDto> ExternalFormConfiguration { get; set; } = new List<ExternalFormConfigSimplifiedDto>();
        public EntityReferenceDto ProcessTemplate { get; set; }
    }
}
