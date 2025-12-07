using System;
using System.Collections.Generic;

namespace PIF.EBP.Application.ciamcommunication.DTOs
{
    public class ScimUserDto
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string DisplayName { get; set; }
        public string Email { get; set; }
        public string MobileNumber { get; set; }
        public string GivenName { get; set; }
        public string FamilyName { get; set; }
        public bool? AccountLocked { get; set; }
        public bool? AccountDisabled { get; set; }
        public string Organization { get; set; }
        public bool? AskPassword { get; set; }
        public string Country { get; set; }
        public string AccountState { get; set; }
        public List<CompanyRolesDto> CompanyRoles { get; set; }
        public string ContactID { get; set; }
        public string Participant { get; set; }
    }

    public class CompanyRolesDto
    {
        public string CompanyId { get; set; }
        public List<Guid> RoleID { get; set; } = new List<Guid>();
    }

    public class ScimUserListDto
    {
        public int TotalResults { get; set; }
        public int StartIndex { get; set; }
        public int ItemsPerPage { get; set; }
        public IList<ScimUserDto> Users { get; set; } = new List<ScimUserDto>();
    }
}
