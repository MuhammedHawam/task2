using System;
using System.Collections.Generic;

namespace PIF.EBP.Application.CIAMCommunication.DTOs
{
    public class CreateSCIMUserRequest
    {
        public string FirstName { get; set; } // ==>GivenName
        public string FamilyName { get; set; } // ==>FamilyName
        public string UserName { get; set; } // ==>UserName
        public string DisplayName { get; set; } // ==>DisplayName
        public string Email { get; set; } // ==>ScimEmail.Value ScimEmail.Primary true
        public string MobileNumber { get; set; } // ==>ScimPhoneNumber.Value ScimPhoneNumber.Type mobile
        public string CompanyId { get; set; } //ScimCustomExtension.CompanyId
        public string ContactID { get; set; } //ScimCustomExtension.ContactID
        public bool IsInvited { get; set; }
        public string Password { get; set; } //Password
        public List<Guid> RolesID { get; set; }//ScimCustomExtension.RoleID
    }
}
