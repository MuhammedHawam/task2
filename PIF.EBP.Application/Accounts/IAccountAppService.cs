using PIF.EBP.Application.Accounts.Dtos;
using PIF.EBP.Application.Accounts.Implementation;
using PIF.EBP.Core.DependencyInjection;
using System.Threading.Tasks;

namespace PIF.EBP.Application.Accounts
{
    public interface IAccountAppService : ITransientDependency
    {
        Task<LoginResponse> Login(LoginDto model);
        Task<bool> ValidateLoginOtp(string emailaddress, int inputOtp, string password);
        Task<ValidatInvitationResponse> ValidateInvitationLink(string encryptedInvitationId);
        Task<bool> ValidateInvitationOtp(string encryptedInvitationId, int inputOtp);
        Task<UserCreationResponse> CreateNewUserPassword(string encryptedInvitationId, string password);
        Task ValidateEmail(string model);
        Task<ChangePasswordResponse> ResetPassword(ResetPasswordDto model);
        Task<ChangePasswordResponse> ChangePassword(ChangePasswordDto request);
        Task<bool> ValidateForgetPasswordOtp(string email, int otp);
        Task Logout(string token);
    }
}
