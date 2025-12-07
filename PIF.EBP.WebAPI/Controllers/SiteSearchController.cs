using PIF.EBP.Application.SiteSearch;
using PIF.EBP.Core.DependencyInjection;
using PIF.EBP.WebAPI.Middleware.ActionFilter;
using System.Threading.Tasks;
using System.Web.Http;

namespace PIF.EBP.WebAPI.Controllers
{
    [ApiResponseWrapper]
    [RoutePrefix("SiteSearch")]
    public class SiteSearchController : ApiController
    {
        private readonly ISiteSearchAppService _siteSearchAppService;
        public SiteSearchController()
        {
            _siteSearchAppService = WindsorContainerProvider.Container.Resolve<ISiteSearchAppService>();
        }

        [HttpGet]
        [Route("search-something")]
        public async Task<IHttpActionResult> SearchSomething(string searchParam)
        {
            var result = await _siteSearchAppService.SearchAsync(searchParam);
            return Ok();
        }

        [HttpPost]
        [Route("create-index")]
        public async Task<IHttpActionResult> CreateIndex(string indexName)
        {
            var result = await _siteSearchAppService.CreateIndex(indexName);
            return Ok(result);
        }

        [HttpDelete]
        [Route("delete-index")]
        public async Task<IHttpActionResult> DeleteIndex(string indexName)
        {
            var result = await _siteSearchAppService.DeleteIndex(indexName);
            return Ok(result);
        }
    }
}
