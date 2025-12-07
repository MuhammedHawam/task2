namespace PIF.EBP.Application.Accounts.Dtos
{
    public class ChangePasswordDto
    {
        public string CurrentPassword { get; set; }
        public string NewPassword { get; set; }
        public int Otp { get; set; }
        public string Email { get; set; }
    }
}
