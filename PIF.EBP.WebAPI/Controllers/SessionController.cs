using PIF.EBP.WebAPI.Middleware.ActionFilter;
using PIF.EBP.WebAPI.Middleware.Authorize;
using System.Web.Http;

namespace PIF.EBP.WebAPI.Controllers
{
    [RoutePrefix("Session")]
    [ApiResponseWrapper]
    [EBPAuthorize()]
    public class SessionController : ApiController
    {
        public SessionController()
        {
            
        }
        [HttpGet]
        [Route("validate-session")]
        public IHttpActionResult ValidateSession()
        {
            if (string.IsNullOrEmpty(Request.Headers.Authorization?.Parameter))
            {
                return Unauthorized();
            }
            return Ok(new { valid = true });
        }
    }
}
