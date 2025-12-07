using System;

namespace PIF.EBP.Application.PerformanceDashboard.DTOs
{
    public class PinCompanyReq
    {
        public Guid Id { get; set; }
        public bool IsPin { get; set; }
        public int AreaType { get; set; }
    }
}
