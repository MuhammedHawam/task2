using PIF.EBP.Application.MetaData.DTOs;
using System;

namespace PIF.EBP.Application.Contacts.Dtos
{
    public class ContactListResponse
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FirstNameAr { get; set; }
        public string LastNameAr { get; set; }
        public string Email { get; set; }
        public string MobilePhone { get; set; }
        public string CompanyId { get; set; }
        public byte[] Entityimage { get; set; }
        public bool IsPinned { get; set; }
        public bool IsDeletable { get; set; }
        public bool HideActions { get; set; }
        public EntityReferenceDto Country { get; set; }
        public EntityReferenceDto City { get; set; }
        public EntityReferenceDto Position { get; set; }

    }
}
