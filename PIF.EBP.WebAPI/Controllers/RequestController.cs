using PIF.EBP.Application.Requests;
using PIF.EBP.Application.Requests.DTOs;
using PIF.EBP.Core.DependencyInjection;
using PIF.EBP.WebAPI.Middleware.ActionFilter;
using System.Threading.Tasks;
using System.Web.Http;

namespace PIF.EBP.WebAPI.Controllers
{
    [ApiResponseWrapper]
    [RoutePrefix("Request")]
    public class RequestController : BaseController
    {
        private readonly IRequestAppService _requestAppService;

        public RequestController()
        {
            _requestAppService = WindsorContainerProvider.Container.Resolve<IRequestAppService>();
        }

        [HttpPost]
        [Route("request-list")]
        public async Task<IHttpActionResult> GetRequestList(HexaRequestListDto_REQ oHexaRequestListDto_REQ)
        {
            var result = await _requestAppService.RetrieveRequestList(oHexaRequestListDto_REQ);

            return Ok(result);
        }
    }
}