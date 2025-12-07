using PIF.EBP.Application.MetaData.DTOs;
using System;

namespace PIF.EBP.Application.Hexa.DTOs
{
    public class TransitionHistoryDto
    {
        public Guid Id { get; set; }
        public string Comment { get; set; }
        public string TaskName { get; set; }
        public string ActionName { get; set; }
        public DateTime CreationDate { get; set; }
        public EntityOptionSetDto ActionType { get; set; }
        public EntityReferenceDto PortalContact { get; set; }
        public EntityReferenceDto Owner { get; set; }
        
    }
}
