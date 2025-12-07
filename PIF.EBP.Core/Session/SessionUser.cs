namespace PIF.EBP.Core.Session
{
    public class SessionUser
    {
        public string ContactId { get; set; }
        public string RoleId { get; set; }
        public string CompanyId { get; set; }
        public string Email { get; set; }
        public string Token { get; set; }
        public bool IsResetToken { get; set; }
    }
}
