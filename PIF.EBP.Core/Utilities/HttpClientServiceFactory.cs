using PIF.EBP.Core.DependencyInjection;
using PIF.EBP.Core.Utilities.Interfaces;
using System.Net.Http;

namespace PIF.EBP.Core.Utilities
{
    public class HttpClientServiceFactory
    {
        public IHttpClientService Create(string clientName)
        {
            var httpClient = WindsorContainerProvider.Container.Resolve<HttpClient>(clientName);
            return new HttpClientService(httpClient);
        }
    }
}
