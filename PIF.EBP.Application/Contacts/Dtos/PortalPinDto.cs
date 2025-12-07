using PIF.EBP.Application.MetaData.DTOs;
using System;

namespace PIF.EBP.Application.Contacts.Dtos
{
    public class PortalPinDto
    {
        public Guid Id { get; internal set; }
        public EntityReferenceDto UserId { get; set; }
        public EntityReferenceDto CompanyId { get; set; }
        public EntityReferenceDto ContactId { get; set; }
        public EntityReferenceDto KnowledgeItemId { get; set; }
        public EntityReferenceDto CompanyPinnedId { get; set; }
    }
}
