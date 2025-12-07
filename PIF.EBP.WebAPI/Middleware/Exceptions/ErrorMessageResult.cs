using System.Net.Http;
using System.Threading.Tasks;
using System.Threading;
using System.Web.Http;

namespace PIF.EBP.WebAPI.Middleware.Exceptions
{
    public class ErrorMessageResult : IHttpActionResult
    {
        private readonly HttpResponseMessage _response;

        public ErrorMessageResult(HttpResponseMessage response)
        {
            _response = response;
        }

        public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(_response);
        }
    }
}