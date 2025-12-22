using Newtonsoft.Json;
using PIF.EBP.Core.GRTTable;
using System;
using System.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PIF.EBP.Integrations.GRTTable.Lookup
{
    public class LookupService : ILookupIntegrationService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        public LookupService()
        {
            _baseUrl = ConfigurationManager.AppSettings["GRTApiBaseUrl"] ?? "http://solutionsuat.pif.gov.sa:80";
            var username = ConfigurationManager.AppSettings["GRTApiUsername"] ?? "PartnerHub_Admin@pif.gov.sa";
            var password = ConfigurationManager.AppSettings["GRTApiPassword"] ?? "123456";

            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(_baseUrl)
            };

            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            // Generate Basic Authentication header
            var authValue = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username}:{password}"));
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authValue);
        }
        public async Task<GRTListTypeDefinitionResponse> GetListTypeDefinitionByExternalReferenceCodeAsync(
            string externalReferenceCode,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(externalReferenceCode))
            {
                throw new ArgumentNullException(nameof(externalReferenceCode), "External reference code cannot be null or empty");
            }

            try
            {
                var url = $"/o/headless-admin-list-type/v1.0/list-type-definitions/by-external-reference-code/{externalReferenceCode}";

                var response = await _httpClient.GetAsync(url, cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<GRTListTypeDefinitionResponse>(responseContent);
                    return result;
                }
                else
                {
                    System.Diagnostics.Trace.TraceError(
                        $"GRT API error: {response.StatusCode} - {response.ReasonPhrase}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"GRT API exception: {ex.Message}");
                throw;
            }
        }

    }
}
