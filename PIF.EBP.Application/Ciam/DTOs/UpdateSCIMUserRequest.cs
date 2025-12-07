using System.Collections.Generic;

namespace PIF.EBP.Application.ciamcommunication.DTOs
{
    public class UpdateSCIMUserRequest
    {
        public string UserName { get; set; }
        public string DisplayName { get; set; }
        public string GivenName { get; set; }
        public string FamilyName { get; set; }
        public string MiddleName { get; set; }
        public string Email { get; set; }
        public bool? EmailIsPrimary { get; set; }
        public string MobileNumber { get; set; }
        public bool? AccountLocked { get; set; }
        public bool? AccountDisabled { get; set; }
        public string Organization { get; set; }
        public bool? AskPassword { get; set; }
        public string Country { get; set; }
        public string AccountState { get; set; }
        public string ContactID { get; set; }
        public List<CompanyRolesDto> CompanyRoles { get; set; }
        public string Participant { get; set; }
    }
}
