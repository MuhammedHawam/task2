using PIF.EBP.WebAPI.Middleware.Authorize;
using System.Web.Http;

namespace PIF.EBP.WebAPI.Controllers
{
    [EBPAuthorize]
    public class BaseController : ApiController
    { 
        public BaseController()
        {
        }
    }
}