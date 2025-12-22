using Newtonsoft.Json;
using PIF.EBP.Core.GRTTable;
using PIF.EBP.Core.GRTTable.Budget.Interfaces;
using System;
using System.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PIF.EBP.Integrations.GRTTable.Budget
{
    public class BudgetService : IBudgetIntegrationService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        public BudgetService()
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

        public async Task<GRTProjectOverviewsPagedResponse> GetProjectOverviewsByCycleCompanyMapIdAsync(
            long cycleCompanyMapId,
            int page = 1,
            int pageSize = 1000,
            string sort = "dateModified:desc",
            long? scopeGroupId = null,
            string currentUrl = null,
            CancellationToken cancellationToken = default)
        {
            if (cycleCompanyMapId <= 0)
            {
                throw new ArgumentException("Cycle company map ID must be greater than zero", nameof(cycleCompanyMapId));
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
                var filter = $"r_gRTCycleCompanyMapRelationship_c_cycleCompanyMapId eq '{cycleCompanyMapId}'";
                var url =
                    $"/o/c/grtprojectoverviews" +
                    $"?filter={Uri.EscapeDataString(filter)}" +
                    $"&page={page}" +
                    $"&pageSize={pageSize}" +
                    $"&sort={Uri.EscapeDataString(sort ?? "dateModified:desc")}";

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
                    return JsonConvert.DeserializeObject<GRTProjectOverviewsPagedResponse>(responseContent);
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Trace.TraceError(
                    $"GRT API error getting project overviews by cycleCompanyMapId: {response.StatusCode} - {response.ReasonPhrase}. Error: {errorContent}");

                return new GRTProjectOverviewsPagedResponse
                {
                    Items = new System.Collections.Generic.List<GRTProjectOverviewListItem>(),
                    Page = page,
                    PageSize = pageSize,
                    TotalCount = 0,
                    LastPage = 1
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"GRT API exception getting project overviews: {ex.Message}");
                throw;
            }
        }
    }
}
