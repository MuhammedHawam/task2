using PIF.EBP.Application.SiteSearch;
using PIF.EBP.Core.DependencyInjection;
using PIF.EBP.Core.Exceptions;
using PIF.EBP.WebAPI.Middleware.ActionFilter;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;

namespace PIF.EBP.WebAPI.Controllers
{
    [ApiResponseWrapper]
    [RoutePrefix("SiteSearch")]
    public class DocumentIndexingController : ApiController
    {
        private readonly ISiteSearchAppService _siteSearchAppService;

        public DocumentIndexingController()
        {
            _siteSearchAppService = WindsorContainerProvider.Container.Resolve<ISiteSearchAppService>();
        }

        [HttpPost]
        [Route("update-index")]
        public async Task<IHttpActionResult> CreateIndex(UpdateIndexRequest request)
        {
            if (string.IsNullOrEmpty(request.LogicalName) || string.IsNullOrEmpty(request.DocumentId))
            {
                throw new UserFriendlyException("RequiredParameters", HttpStatusCode.BadRequest);
            }

            var result = await _siteSearchAppService.UpdateDocumentFromCrmToSearchEngine(request.LogicalName, request.DocumentId);
            return Ok(result);
        }
    }

    public class UpdateIndexRequest
    {
        public string LogicalName { get; set; }
        public string DocumentId { get; set; }
    }
}
