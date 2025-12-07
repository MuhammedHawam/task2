using System.Collections.Generic;

namespace PIF.EBP.Application.Accounts.Dtos
{
    public class LoginDto
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public Dictionary<string, string> Keys { get; set; } = null;
        public bool? GenerateToken { get; set; } = true;
    }
    public class SwitchProfileDto
    {
        public Dictionary<string, string> Keys { get; set; } = null;
    }
}
