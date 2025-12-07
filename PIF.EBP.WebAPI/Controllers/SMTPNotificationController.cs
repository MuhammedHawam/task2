using PIF.EBP.Application.SMTPNotificaation;
using PIF.EBP.Application.SMTPNotificaation.DTOs;
using PIF.EBP.Core.DependencyInjection;
using System.Threading.Tasks;
using System.Web.Http;

namespace PIF.EBP.WebAPI.Controllers
{
    public class SMTPNotificationController : BaseController
    {
        private readonly ISMTPNotificationService _notificationService;

        public SMTPNotificationController()
        {
            _notificationService = WindsorContainerProvider.Container.Resolve<ISMTPNotificationService>();
        }

        [HttpPost]
        [Route("Send-Email")]
        public async Task<IHttpActionResult> SendEmail(SendEmailDto emailDto)
        {
            var serviceResult = await _notificationService.SendCrmEmailAsync(emailDto);
            return Ok(new { EmailId = serviceResult, Message = "Email sent successfully" });
        }
    }
}