using PIF.EBP.Application.Commercialization.DTOs.IESMServiceModels;
using System.Collections.Generic;

namespace PIF.EBP.Application.Commercialization.DTOs
{
    public class EsmWidgetDataRsp
    {
        public EsmWidgetDataRsp()
        {
            Requests = new List<RequestWidgetData>();
            WidgetDataCount = new WidgetDataCountDto();
            Statuses = new List<EsmOptionsDto>();
        }
        public WidgetDataCountDto WidgetDataCount { get; set; }
        public List<RequestWidgetData> Requests { get; set; }
        public List<EsmOptionsDto> Statuses { get; set; }

    }
    public class WidgetDataCountDto
    {
        public double UnderReview { get; set; }
        public double Returned { get; set; }
        public double Completed { get; set; }
        public double TotalPending { get; set; }
        public double Rejected { get; set; }
    }

    public class RequestWidgetData
    {
        public string RequestId { get; set; }
        public string RequestNumber { get; set; }
        public string ServiceName { get; set; }
        public EsmOptionsDto Status { get; set; }
    }
}
