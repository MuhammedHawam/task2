using System;

namespace PIF.EBP.Application.UserManagement.DTOs
{
    public class UserInviteRes
    {
        public bool IsSuccess { get; set; }
        public Guid ContactId { get; set; } = Guid.Empty;
        public string Message { get; set; }
        public string MessageAr { get; set; }
    }
}
