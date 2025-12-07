using PIF.EBP.Application.Commercialization.DTOs.IESMServiceModels;
using System.Collections.Generic;

namespace PIF.EBP.Application.Commercialization.DTOs
{
    public class EsmRequestListRsp
    {
        public EsmRequestListRsp()
        {
            Requests = new List<EsmRequestListDtoResponse>();
            RequestCount = new RequestCountDto();
            Statuses = new List<EsmOptionsDto>();
        }
        public int TotalCount { get; set; } = 0;
        public RequestCountDto RequestCount { get; set; }
        public List<EsmRequestListDtoResponse> Requests { get; set; }
        public List<EsmOptionsDto> Statuses { get; set; }

    }
}
