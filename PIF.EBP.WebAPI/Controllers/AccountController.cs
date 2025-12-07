using PIF.EBP.Application.Accounts;
using System.Web.Http;
using System.Threading.Tasks;
using PIF.EBP.Application.Accounts.Dtos;
using PIF.EBP.Core.DependencyInjection;
using PIF.EBP.WebAPI.Middleware.ActionFilter;
using PIF.EBP.WebAPI.Controllers.Requests.Account;
using PIF.EBP.Core.Exceptions;
using System.Net;
using PIF.EBP.WebAPI.Middleware.Authorize;
using PIF.EBP.Core.Session;

namespace PIF.EBP.WebAPI.Controllers
{
    [RoutePrefix("Account")]
    [ApiResponseWrapper]
    public class AccountController : ApiController
    {
        private readonly IAccountAppService _accountService;
        private readonly ISessionService _sessionService;

        public AccountController()
        {
            _accountService = WindsorContainerProvider.Container.Resolve<IAccountAppService>();
            _sessionService = WindsorContainerProvider.Container.Resolve<ISessionService>();
        }

        [HttpPost]
        [Route("login")]
        public async Task<IHttpActionResult> Login(LoginRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            {
                throw new UserFriendlyException("UsernameAndPasswordAreRequired", System.Net.HttpStatusCode.BadRequest);
            }
            var model = new LoginDto
            {
                Email = request.Username.Trim(),
                Password = request.Password.Trim(),
            };
            var result = await _accountService.Login(model);

            if (result.IsSuccess)
            {
                if (result.TwoFactorEnabled)
                {
                    return Ok(new { OtpSent = true, MaskedPhone = result.MaskedPhone, UserLanguage = result.UserLanguage });

                }
                return Ok(new { UserLanguage = result.UserLanguage });
            }

            throw new UserFriendlyException("InvalidCredentials");
        }

        [HttpGet]
        [Route("validate_invitation_link/{encryptedInvitationId}")]
        public async Task<IHttpActionResult> ValidateInvitationLink(string encryptedInvitationId)
        {
            if (string.IsNullOrWhiteSpace(encryptedInvitationId))
                throw new UserFriendlyException("EncryptedInvitationIdIsRequired", System.Net.HttpStatusCode.BadRequest);

            var result = await _accountService.ValidateInvitationLink(encryptedInvitationId);
            return Ok(result);
        }

        [HttpPost]
        [Route("reset-password")]
        public async Task<IHttpActionResult> ResetPassword(ChangePasswordDto model)
        {
            if (model == null)
                throw new UserFriendlyException("ModelCannotBeNull", System.Net.HttpStatusCode.BadRequest);

            if (model?.CurrentPassword == model?.NewPassword)
                throw new UserFriendlyException("UsedPasswordBefore", System.Net.HttpStatusCode.BadRequest);

            // Extract the token from the Authorization header
            var token = Request.Headers.Authorization?.Parameter;
            ChangePasswordResponse response;
            if (string.IsNullOrEmpty(token))
            {
                return BadRequest();
            }
            else
            {

                var resetPasswordReuest = new ResetPasswordDto
                {
                    NewPassword = model.NewPassword
                };

                if (_sessionService.IsResetToken())
                {
                    response = await _accountService.ResetPassword(resetPasswordReuest);
                }
                else
                {

                    response = await _accountService.ChangePassword(model);
                }
            }

            if (response.IsSuccess)
            {
                await _accountService.Logout(token);
                return Ok(new { IsSucess = true });
            }
            return BadRequest();
        }

        [HttpPost]
        [Route("create-password-for-new-user")]
        public async Task<IHttpActionResult> CreatePasswordForNewUser(NewUserPasswordRequest model)
        {
            if (model == null)
                throw new UserFriendlyException("BadRequest", System.Net.HttpStatusCode.BadRequest);

            if (string.IsNullOrWhiteSpace(model.EncryptedInvitationId) || string.IsNullOrWhiteSpace(model.Password))
                throw new UserFriendlyException("BadRequest", System.Net.HttpStatusCode.BadRequest);

            var response = await _accountService.CreateNewUserPassword(model.EncryptedInvitationId, model.Password);
            if (response.IsSuccess)
            {
                return Ok(new { Success = true });
            }
            throw new UserFriendlyException("ErrorInCreatingUserCredentials");
        }

        [HttpGet]
        [Route("validate-email")]
        public async Task<IHttpActionResult> ValidateEmail(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                throw new UserFriendlyException("EmailFieldCannotBeEmpty", HttpStatusCode.BadRequest);
            }
            await _accountService.ValidateEmail(email.Trim());
            return Ok();
        }

        [HttpPatch]
        [Route("logout")]
        public async Task<IHttpActionResult> Logout()
        {
            var token = Request.Headers.Authorization?.Parameter;
            if (string.IsNullOrEmpty(token))
            {
                throw new UserFriendlyException("TokenIsRequired", HttpStatusCode.BadRequest);
            }
            await _accountService.Logout(token);
            return Ok();
        }
    }

    public class NewUserPasswordRequest
    {
        public string EncryptedInvitationId { get; set; }
        public string Password { get; set; }
    }
}
