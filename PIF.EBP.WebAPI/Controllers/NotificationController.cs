using PIF.EBP.Application.Notification;
using PIF.EBP.Application.Shared.AppRequest;
using PIF.EBP.Core.DependencyInjection;
using PIF.EBP.WebAPI.Middleware.ActionFilter;
using System;
using System.Threading.Tasks;
using System.Web.Http;

namespace PIF.EBP.WebAPI.Controllers
{
    [ApiResponseWrapper]
    [RoutePrefix("Notification")]
    public class NotificationController : BaseController
    {
        private readonly INotificationAppService _notificationAppService;

        public NotificationController()
        {
            _notificationAppService = WindsorContainerProvider.Container.Resolve<INotificationAppService>();
        }

        [HttpPost]
        [Route("get-notifications")]
        public async Task<IHttpActionResult> GetNotificatios(PagingRequest pagingRequest)
        {
            var result = await _notificationAppService.RetrieveNotifications(pagingRequest);

            return Ok(result);
        }

        [HttpGet]
        [Route("get-unread-notifications")]
        public async Task<IHttpActionResult> GetUnreadNotificatios()
        {
            var result = await _notificationAppService.RetrieveUnreadNotifications();

            return Ok(result);
        }

        [HttpPost]
        [Route("update-notification")]
        public async Task<IHttpActionResult> UpdateNotificationReadStatus([FromBody] Guid Id)
        {
            var result = await _notificationAppService.UpdateNotificationReadStatus(Id);

            return Ok(result);
        }
    }
}