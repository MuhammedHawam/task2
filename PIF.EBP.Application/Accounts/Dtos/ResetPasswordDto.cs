using System.ComponentModel.DataAnnotations;


namespace PIF.EBP.Application.Accounts.Dtos
{
    public class ResetPasswordDto
    {
        public string Email { get; set; }
        public string NewPassword { get; set; }
        public int LastPasswordTimesToCheck { get; set; }
    }
}
