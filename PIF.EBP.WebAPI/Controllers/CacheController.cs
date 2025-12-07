using PIF.EBP.Application.Cache;
using PIF.EBP.Core.DependencyInjection;
using PIF.EBP.WebAPI.Middleware.ActionFilter;
using PIF.EBP.WebAPI.Middleware.Authorize;
using System.Threading.Tasks;
using System.Web.Http;

namespace PIF.EBP.WebAPI.Controllers
{
    [ApiResponseWrapper]
    [RoutePrefix("Cache")]
    [APIKEY]
    public class CacheController : ApiController
    {
        private readonly ICacheAppService _cacheAppService;

        public CacheController()
        {
            _cacheAppService = WindsorContainerProvider.Container.Resolve<ICacheAppService>();
        }

        [HttpPost]
        [Route("clear-cache-by-entity-name")]
        public async Task<IHttpActionResult> ClearCacheByEntityName(string entityName)
        {
            await _cacheAppService.ClearEntityCache(entityName);

            return Ok();
        }

        [HttpPost]
        [Route("clear-cache-by-meta-data-name")]
        public async Task<IHttpActionResult> ClearCacheByMetaDataName(string entityName)
        {
            await _cacheAppService.ClearMetaDataCache(entityName);

            return Ok();
        }

        [HttpPost]
        [Route("clear-access-management-cache")]
        public async Task<IHttpActionResult> ClearAccessManagementCache()
        {
            await _cacheAppService.ClearAccessManagementCache();

            return Ok();
        }

        [HttpPost]
        [Route("clear-all-cache")]
        public async Task<IHttpActionResult> ClearAllCache()
        {
            await _cacheAppService.ClearAllCache();

            return Ok();
        }

        [HttpPost]
        [Route("clear-all-cache-for-node")]
        public async Task<IHttpActionResult> ClearAllCacheForNode()
        {
            await _cacheAppService.ClearAllCacheForNode();

            return Ok();
        }

    }
}
