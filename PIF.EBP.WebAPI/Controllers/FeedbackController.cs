using PIF.EBP.Application.Feedback;
using PIF.EBP.Application.Feedback.DTOs;
using PIF.EBP.Core.DependencyInjection;
using PIF.EBP.WebAPI.Middleware.ActionFilter;
using System.Threading.Tasks;
using System.Web.Http;

namespace PIF.EBP.WebAPI.Controllers
{
    [RoutePrefix("Support")]
    [ApiResponseWrapper]
    public class FeedbackController : BaseController
    {
        private readonly IFeedbackAppService _feedbackAppService;

        public FeedbackController()
        {
            _feedbackAppService = WindsorContainerProvider.Container.Resolve<IFeedbackAppService>();
        }

        [HttpPost]
        [Route("share-feedback")]
        public async Task<IHttpActionResult> ShareFeedback(FeedbackDto feedbackDto)
        {
            var result = await _feedbackAppService.CreateFeedback(feedbackDto);

            return Ok(result);
        }
    }
}