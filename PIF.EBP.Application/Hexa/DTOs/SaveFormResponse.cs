using PIF.EBP.Application.MetaData.DTOs;
using System;

namespace PIF.EBP.Application.Hexa.DTOs
{
    public class SaveFormResponse
    {
        public Guid EntityId { get; set; }
        public string EntityName { get; set; }
        public EntityReferenceDto InternalStatus { get; set; }
    }
}
