using System.Collections.Generic;

namespace PIF.EBP.Application.Accounts.Dtos
{
    public class RegisterDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string ContactId { get; set; }
        public string PhoneNumber { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
        public string Email { get; set; }
        public Dictionary<string, string> Keys { get; set; }
    }
}
