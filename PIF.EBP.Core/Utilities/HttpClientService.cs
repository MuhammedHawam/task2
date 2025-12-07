using Newtonsoft.Json.Linq;
using PIF.EBP.Core.Exceptions;
using PIF.EBP.Core.Utilities.Interfaces;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace PIF.EBP.Core.Utilities
{
    public class HttpClientService : IHttpClientService
    {
        private readonly HttpClient _httpClient;

        public HttpClientService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string> GetAsync(string url)
        {
            var response = await _httpClient.GetAsync($"{url}");
            return await response.Content.ReadAsStringAsync();
        }
        public async Task<(byte[],string, JObject)> GetByteAsync(string url)
        {
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                throw new UserFriendlyException("Failed to retrieve image");
            }
            var contentType = response.Content.Headers.ContentType?.ToString();

            // Retrieve the custom header value "x-attachment-metadata"
            response.Headers.TryGetValues("x-attachment-metadata", out var metadataValues);
            var metadataJson = metadataValues?.FirstOrDefault();

            // Convert the metadata to JObject (safely)
            JObject metadata = null;
            if (!string.IsNullOrEmpty(metadataJson))
            {
                try
                {
                    metadata = JObject.Parse(metadataJson);
                }
                catch (Exception)
                {
                    throw new UserFriendlyException("Invalid metadata JSON format");
                }
            }

            return (await response.Content.ReadAsByteArrayAsync(), contentType, metadata);
        }

        public async Task<(bool, string)> PostAsync(string url, HttpContent content)
        {
            var response = await _httpClient.PostAsync(url, content);

            return (response.IsSuccessStatusCode, await response.Content.ReadAsStringAsync());
        }

        public async Task<string> PutAsync(string url, HttpContent content)
        {
            var response = await _httpClient.PutAsync(url, content);
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string> DeleteAsync(string url)
        {
            var response = await _httpClient.DeleteAsync(url);
            return await response.Content.ReadAsStringAsync();
        }
        public async Task<(bool, string)> SendAsync(HttpRequestMessage request)
        {
            var response = await _httpClient.SendAsync(request);
            return (response.IsSuccessStatusCode, await response.Content.ReadAsStringAsync());
        }
    }
}
