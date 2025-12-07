using PIF.EBP.Application.Accounts.Dtos;
using PIF.EBP.Core.DependencyInjection;
using System;
using System.Threading.Tasks;
using static PIF.EBP.Application.Shared.Enums;

namespace PIF.EBP.Application.Otp
{
    public interface IOtpService : IScopedDependency
    {
        Task GenerateOtp(OtpType otpOperation, Guid contactId, Guid? invitationId = null);
        OtpDto GetOtpEntity(OtpType otpOperation, Guid? contactId, Guid? invitationId = null);
    }
}
