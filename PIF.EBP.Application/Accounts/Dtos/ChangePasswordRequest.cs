namespace PIF.EBP.Application.Accounts.Dtos
{
    public class ChangePasswordRequest
    {
        public int LastPasswordTimesToCheck { get; set; }
        public string Email { get; set; }
        public string CurrentPassword { get; set; }
        public string NewPassword { get; set; }
    }
}
