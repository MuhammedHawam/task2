using PIF.EBP.Application.AttendeeEvent;
using PIF.EBP.Core.DependencyInjection;
using PIF.EBP.WebAPI.Middleware.ActionFilter;
using System.Threading.Tasks;
using System.Web.Http;
using static PIF.EBP.Application.Shared.Enums;

namespace PIF.EBP.WebAPI.Controllers
{
    [ApiResponseWrapper]
    [RoutePrefix("attendees")]
    public class AttendeeEventController : ApiController
    {
        private readonly IAttendeeEventAppService _attendeesAppService;

        public AttendeeEventController()
        {
            _attendeesAppService = WindsorContainerProvider.Container.Resolve<IAttendeeEventAppService>();
        }

        [HttpGet]
        [Route("attendees-details-by-refid")]
        public async Task<IHttpActionResult> GetAttendeesdetailsByRefId(string refId)
        {
            var response = await _attendeesAppService.RetrieveAttendeesDetailsByRefId(refId);

            return Ok(response);
        }
        [HttpPut]
        [Route("update-attendees-details")]
        public async Task<IHttpActionResult> UpdateAttendeesdetails(string refId, int RSVP = (int)RSVP.Pending, string guestName = null, string guestRole = null)
        {
            var response = await _attendeesAppService.UpdateAttendeesDetails(refId, RSVP, guestName, guestRole);
            return Ok(response);
        }


    }
}
