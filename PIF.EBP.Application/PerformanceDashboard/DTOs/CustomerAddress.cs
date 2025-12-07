using PIF.EBP.Application.MetaData.DTOs;
using System;

namespace PIF.EBP.Application.PerformanceDashboard.DTOs
{
    public class CustomerAddress
    {
        public string Id { get; set; }
        public string Line1 { get; set; }
        public string Line2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public DateTime CreationDate { get; set; }
        public EntityOptionSetDto AddressType { get; set; }
    }
}
