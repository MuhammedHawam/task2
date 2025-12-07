using System;

namespace PIF.EBP.Core.Search.Entities
{
    public class RequestStepEntity : EntityBase
    {
        public string RequestId { get; set; }
        public string Status { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime DueDate { get; set; }
        public string Assignee { get; set; }
    }
}
