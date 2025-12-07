using System.Net.Http;
using System.Threading.Tasks;
using System.Threading;
using PIF.EBP.Core.DependencyInjection;
using Castle.Core.Logging;

namespace PIF.EBP.WebAPI.Middleware.Logging
{
    public class LoggingMiddleware : DelegatingHandler, ITransientDependency
    {
        private readonly ILogger _logger;

        public LoggingMiddleware(ILogger logger)
        {
            _logger = logger;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            _logger.Info($"Request: {request.Method} {request.RequestUri} - Corl ID : { request.GetCorrelationId()}");

            var response = await base.SendAsync(request, cancellationToken);

            _logger.Info($"Response: {response.StatusCode} - Corl ID : {request.GetCorrelationId()}");

            return response;
        }
    }
}