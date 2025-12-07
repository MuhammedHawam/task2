namespace PIF.EBP.Application.Accounts.Dtos
{
    public class ValidatInvitationResponse
    {
        public string FullName { get; set; }
        public string FullNameAr { get; set; }
        public string MaskedPhone { get; set; }
        public string MaskedEmail { get; set; }
        public string EncryptedInvitationId { get; set; }
        public bool OtpSent { get; set; } = true;
    }
}
