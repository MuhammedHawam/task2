using PIF.EBP.Application.MetaData.DTOs;
using System;

namespace PIF.EBP.Application.UserManagement.DTOs
{
    public class UserListResponse
    {
        public Guid Id { get; set; }
        public string ContactId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FirstNameAr { get; set; }
        public string LastNameAr { get; set; }
        public string Email { get; set; }
        public bool ReInvite { get; set; }
        public bool Resend { get; set; }
        public bool Deactivate { get; set; }
        public bool Editable { get; set; }
        public byte[] Entityimage { get; set; }
        public EntityReferenceDto Role { get; set; }
        public EntityReferenceDto Company { get; set; }
        public EntityOptionSetDto AssociationStatus { get; set; }
        public EntityReferenceDto Position { get; set; }
        public EntityReferenceDto Department { get; set; }
    }
}
