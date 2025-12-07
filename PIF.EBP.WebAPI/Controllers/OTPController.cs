using PIF.EBP.Application.Accounts;
using PIF.EBP.Application.Otp;
using PIF.EBP.Core.DependencyInjection;
using PIF.EBP.Core.Session;
using PIF.EBP.WebAPI.Middleware.ActionFilter;
using System;
using System.Threading.Tasks;
using System.Web.Http;
using static PIF.EBP.Application.Shared.Enums;

namespace PIF.EBP.WebAPI.Controllers
{
    [ApiResponseWrapper]
    [RoutePrefix("OTP")]
    public class OTPController : ApiController
    {
        private readonly IAccountAppService _accountAppService;
        private readonly IOtpService _otpService;
        private readonly ISessionService _sessionService;
        public OTPController()
        {
            _accountAppService = WindsorContainerProvider.Container.Resolve<IAccountAppService>();
            _otpService = WindsorContainerProvider.Container.Resolve<IOtpService>();
            _sessionService = WindsorContainerProvider.Container.Resolve<ISessionService>();
        }

        [HttpGet]
        [Route("send-otp")]
        public async Task<IHttpActionResult> SendOtpForUpdateProfile()
        {
            await _otpService.GenerateOtp(OtpType.UpdateProfile, new Guid(_sessionService.GetContactId()), null);
            return Ok(new { OtpSent = true});
        }

        [HttpPost]
        [Route("verify-otp")]
        public async Task<IHttpActionResult> VerifyOtp(VerifyOtpRequest verifyOtp)
        {
            object result = null;
            if((OtpType)verifyOtp.Operation == OtpType.Invitation)
            {
                result = await _accountAppService.ValidateInvitationOtp(verifyOtp.Identifier, verifyOtp.Otp);
            }
            if ((OtpType)verifyOtp.Operation == OtpType.Login)
            {
                result = await _accountAppService.ValidateLoginOtp(verifyOtp.Identifier, verifyOtp.Otp, verifyOtp.Password);
            }
            if ((OtpType)verifyOtp.Operation == OtpType.ForgetPassword)
            {
                result = await _accountAppService.ValidateForgetPasswordOtp(verifyOtp.Identifier, verifyOtp.Otp);
            }

            return Ok(result);
        }

    }

    public class VerifyOtpRequest
    {
        public int Operation { get; set; }
        public string Identifier { get; set; }
        public int Otp { get; set; }
        public string Password { get; set; }
    }
}
