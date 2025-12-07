using Newtonsoft.Json;
using PIF.EBP.Application.Accounts;
using PIF.EBP.Application.Accounts.Dtos;
using PIF.EBP.Application.ciamcommunication.DTOs;
using PIF.EBP.Application.CIAMCommunication;
using PIF.EBP.Application.CIAMCommunication.DTOs;
using PIF.EBP.Core.CIAMCommunication.DTOs;
using PIF.EBP.Core.DependencyInjection;
using PIF.EBP.Core.Exceptions;
using PIF.EBP.WebAPI.Controllers.Requests.Account;
using System;
using System.Configuration;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;

namespace PIF.EBP.WebAPI.Controllers
{
    public class SCIMCommunicationController : BaseController
    {
        private readonly ICIAMUserService _ciamUserService;
        private readonly IAccountAppService _accountService;
        private readonly bool _logRequests;

        public SCIMCommunicationController()
        {
            _ciamUserService = WindsorContainerProvider.Container.Resolve<ICIAMUserService>();
            _accountService = WindsorContainerProvider.Container.Resolve<IAccountAppService>();

            bool.TryParse(ConfigurationManager.AppSettings["LogCiamUserCreation"], out bool enableLogging);
            _logRequests = enableLogging;
        }

        [HttpPost]
        [Route("Create-user")]
        public async Task<IHttpActionResult> CreateUser(SCIMContactCreateDto request)
        {
            var result = await _ciamUserService.CreateContact(request);
            return Ok(result);
        }

        [HttpPost]
        [Route("login")]
        public async Task<IHttpActionResult> Login(LoginRequest request)
        {
            if (request == null ||
                string.IsNullOrWhiteSpace(request.Username) ||
                string.IsNullOrWhiteSpace(request.Password))
            {
                throw new UserFriendlyException("UsernameAndPasswordAreRequired", HttpStatusCode.BadRequest);
            }

            var model = new LoginDto
            {
                Email = request.Username.Trim(),
                Password = request.Password.Trim()
            };

            var loginResult = await _accountService.Login(model);

            if (loginResult.IsSuccess)
            {
                if (loginResult.TwoFactorEnabled)
                {
                    return Ok(new
                    {
                        OtpSent = true,
                        MaskedPhone = loginResult.MaskedPhone,
                        UserLanguage = loginResult.UserLanguage
                    });
                }

                return Ok(new { UserLanguage = loginResult.UserLanguage });
            }

            throw new UserFriendlyException("InvalidCredentials");
        }

        [HttpPost]
        [Route("Create-SCIM-user")]
        public async Task<IHttpActionResult> CreateSCIMInvitedUser(CreateSCIMUserRequest request)
        {
            var result = await _ciamUserService.CreateInvitedUserAsync(request);
            return Ok(result);
        }

        [HttpPut]
        [Route("users/{userId}/{userName}/lock")]
        public async Task<IHttpActionResult> LockUser([FromUri] string userId, [FromUri] string userName)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return BadRequest("UserId is required.");

            try
            {
                var resp = await _ciamUserService.SetAccountLockedAsync(userId, userName, true);
                return resp.IsSuccess
                    ? (IHttpActionResult)Ok(new { Success = true })
                    : BadRequest(resp.ErrorMessage);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpPut]
        [Route("users/{userId}/{userName}/unlock")]
        public async Task<IHttpActionResult> UnLockUser([FromUri] string userId, [FromUri] string userName)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return BadRequest("UserId is required.");

            try
            {
                var resp = await _ciamUserService.SetAccountLockedAsync(userId, userName, false);
                return resp.IsSuccess
                    ? (IHttpActionResult)Ok(new { Success = true })
                    : BadRequest(resp.ErrorMessage);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpPut]
        [Route("users/{userId}/{userName}/disable")]
        public async Task<IHttpActionResult> DisableUser([FromUri] string userId, [FromUri] string userName)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return BadRequest("UserId is required.");

            try
            {
                var resp = await _ciamUserService.SetAccountDisabledAsync(userId, userName, true);
                return resp.IsSuccess
                    ? (IHttpActionResult)Ok(new { Success = true })
                    : BadRequest(resp.ErrorMessage);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpPut]
        [Route("users/{userId}/{userName}/enable")]
        public async Task<IHttpActionResult> EnableUser([FromUri] string userId, [FromUri] string userName)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return BadRequest("UserId is required.");

            try
            {
                var resp = await _ciamUserService.SetAccountDisabledAsync(userId, userName, false);
                return resp.IsSuccess
                    ? (IHttpActionResult)Ok(new { Success = true })
                    : BadRequest(resp.ErrorMessage);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpGet]
        [Route("users")]
        public async Task<IHttpActionResult> GetAllUsers()
        {
            var users = await _ciamUserService.ListAllUsersAsync();
            return Ok(users);
        }

        [HttpGet]
        [Route("users/by-username/{userName}")]
        public async Task<IHttpActionResult> GetUserByUserName(string userName)
        {
            var user = await _ciamUserService.GetUserByUserNameAsync(userName);
            if (user == null)
                return NotFound();

            return Ok(user);
        }

        [HttpGet]
        [Route("users/by-email")]
        public async Task<IHttpActionResult> GetUsersByEmail([FromUri] string email)
        {
            var users = await _ciamUserService.GetUsersByEmailAsync(email);
            return Ok(users);
        }
    }
}