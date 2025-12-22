using Newtonsoft.Json;
using PIF.EBP.Core.GRTTable.MultipleSandU.DTOs;
using PIF.EBP.Core.GRTTable.MultipleSandU.Interfaces;
using System;
using System.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PIF.EBP.Integrations.GRTTable.MultipleSandU
{
    public class MultipleSUTableService : IMultipleSUTableIntegrationService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        public MultipleSUTableService()
        {
            _baseUrl = ConfigurationManager.AppSettings["GRTApiBaseUrl"] ?? "http://solutionsuat.pif.gov.sa:80";
            var username = ConfigurationManager.AppSettings["GRTApiUsername"] ?? "PartnerHub_Admin@pif.gov.sa";
            var password = ConfigurationManager.AppSettings["GRTApiPassword"] ?? "123456";

            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(_baseUrl)
            };

            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var authValue = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username}:{password}"));
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authValue);
        }

        public async Task<GRTMultipleSUTablesPagedResponse> GetMultipleSUTablesByProjectOverviewIdAsync(
            long projectOverviewId,
            int page = 1,
            int pageSize = 1,
            long? scopeGroupId = null,
            string currentUrl = null,
            CancellationToken cancellationToken = default)
        {
            if (projectOverviewId <= 0)
            {
                throw new ArgumentException("Project overview ID must be greater than zero", nameof(projectOverviewId));
            }

            if (page <= 0)
            {
                throw new ArgumentException("Page number must be greater than zero", nameof(page));
            }

            if (pageSize <= 0)
            {
                throw new ArgumentException("Page size must be greater than zero", nameof(pageSize));
            }

            try
            {
                var filter = $"r_projectToMultipleSUTableRelationship_c_grtProjectOverviewId eq '{projectOverviewId}'";
                var url =
                    $"/o/c/grtmultiplesutables" +
                    $"?filter={Uri.EscapeDataString(filter)}" +
                    $"&page={page}" +
                    $"&pageSize={pageSize}";

                if (scopeGroupId.HasValue)
                {
                    url += $"&scopeGroupId={scopeGroupId.Value}";
                }

                if (!string.IsNullOrWhiteSpace(currentUrl))
                {
                    var normalized = Uri.EscapeDataString(Uri.UnescapeDataString(currentUrl));
                    url += $"&currentURL={normalized}";
                }

                var response = await _httpClient.GetAsync(url, cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<GRTMultipleSUTablesPagedResponse>(responseContent);
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Trace.TraceError(
                    $"GRT API error getting multiple SU tables: {response.StatusCode} - {response.ReasonPhrase}. Error: {errorContent}");

                return new GRTMultipleSUTablesPagedResponse
                {
                    Items = new System.Collections.Generic.List<GRTMultipleSUTableItem>(),
                    Page = page,
                    PageSize = pageSize,
                    TotalCount = 0,
                    LastPage = 1
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"GRT API exception getting multiple SU tables: {ex.Message}");
                throw;
            }
        }

        public async Task<GRTMultipleSUTableItem> UpdateMultipleSUTableAsync(
            long id,
            GRTMultipleSUTableUpdateRequest request,
            long? scopeGroupId = null,
            string currentUrl = null,
            CancellationToken cancellationToken = default)
        {
            if (id <= 0)
            {
                throw new ArgumentException("Multiple S&U table ID must be greater than zero", nameof(id));
            }

            if (request == null)
            {
                throw new ArgumentNullException(nameof(request), "Update request cannot be null");
            }

            try
            {
                var url = $"/o/c/grtmultiplesutables/{id}";

                var hasQuery = false;
                if (scopeGroupId.HasValue)
                {
                    url += $"{(hasQuery ? "&" : "?")}scopeGroupId={scopeGroupId.Value}";
                    hasQuery = true;
                }

                if (!string.IsNullOrWhiteSpace(currentUrl))
                {
                    var normalized = Uri.EscapeDataString(Uri.UnescapeDataString(currentUrl));
                    url += $"{(hasQuery ? "&" : "?")}currentURL={normalized}";
                    hasQuery = true;
                }

                var jsonContent = JsonConvert.SerializeObject(request, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    Formatting = Formatting.None
                });

                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                var response = await _httpClient.PutAsync(url, content, cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<GRTMultipleSUTableItem>(responseContent);
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Trace.TraceError(
                    $"GRT API error updating multiple SU table: {response.StatusCode} - {response.ReasonPhrase}. Error: {errorContent}");
                throw new Exception($"Failed to update multiple SU table: {response.StatusCode} - {errorContent}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"GRT API exception updating multiple SU table: {ex.Message}");
                throw;
            }
        }
    }
}

