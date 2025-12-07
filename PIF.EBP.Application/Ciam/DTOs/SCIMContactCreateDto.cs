using PIF.EBP.Application.ciamcommunication.DTOs;
using System.Collections.Generic;

namespace PIF.EBP.Application.CIAMCommunication.DTOs
{
    public class SCIMContactCreateDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FirstNameAr { get; set; }
        public string LastNameAr { get; set; }
        public string Email { get; set; }
        public string MobilePhone { get; set; }
        public string Country { get; set; }
        public string Position { get; set; }
        public string Department { get; set; }
        public List<CompanyRolesDto> CompanyRoles { get; set; }
        public int Nationality { get; set; }
    }
}
