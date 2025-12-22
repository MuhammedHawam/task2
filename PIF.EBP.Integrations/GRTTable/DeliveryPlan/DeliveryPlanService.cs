using Newtonsoft.Json;
using PIF.EBP.Core.GRTTable;
using PIF.EBP.Core.GRTTable.DeliveryPlan.Interfaces;
using System;
using System.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PIF.EBP.Integrations.GRTTable
{
    public class DeliveryPlanService : IDeliveryPlanIntegrationService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        public DeliveryPlanService()
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
        private string AddAuditEventsNestedField(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return url;

            // If the URL already has query parameters, use '&'
            var separator = url.Contains("?") ? "&" : "?";

            // Avoid adding it twice
            if (url.Contains("nestedFields=auditEvents"))
                return url;

            return $"{url}{separator}nestedFields=auditEvents";
        }
        public async Task<DeliveryPlanTablesPagedResponse> GetDeliveryPlanTablesPagedAsync(
    long projectOverviewId,
    int page = 1,
    int pageSize = 20,
    string search = null,
    CancellationToken cancellationToken = default)
        {
            if (projectOverviewId <= 0)
            {
                throw new ArgumentException("Project overview ID must be greater than zero", nameof(projectOverviewId));
            }

            try
            {
                var url = $"/o/c/grtprojectoverviews/{projectOverviewId}/projectToDeliveryPlanTableRelationship?page={page}&pageSize={pageSize}";

                if (!string.IsNullOrWhiteSpace(search))
                {
                    url += $"&search={Uri.EscapeDataString(search)}";
                }

                var response = await _httpClient.GetAsync(url, cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<DeliveryPlanTablesPagedResponse>(responseContent);
                    return result;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Trace.TraceError(
                        $"GRT API error getting delivery plan tables: {response.StatusCode} - {response.ReasonPhrase}. Error: {errorContent}");
                    return new DeliveryPlanTablesPagedResponse
                    {
                        Items = new System.Collections.Generic.List<DeliveryPlanTable>(),
                        Page = page,
                        PageSize = pageSize,
                        TotalCount = 0,
                        LastPage = 1
                    };
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"GRT API exception getting delivery plan tables: {ex.Message}");
                throw;
            }
        }

        public async Task<DeliveryPlanTable> GetDeliveryPlanTableByIdAsync(
            long id,
            CancellationToken cancellationToken = default)
        {
            if (id <= 0)
            {
                throw new ArgumentException("Delivery plan table ID must be greater than zero", nameof(id));
            }

            try
            {
                var url = $"/o/c/grtdeliveryplantables/{id}";

                url = AddAuditEventsNestedField(url);

                var response = await _httpClient.GetAsync(url, cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<DeliveryPlanTable>(responseContent);
                    return result;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Trace.TraceError(
                        $"GRT API error getting delivery plan table: {response.StatusCode} - {response.ReasonPhrase}. Error: {errorContent}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"GRT API exception getting delivery plan table: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> DeleteDeliveryPlanTableAsync(
            long id,
            CancellationToken cancellationToken = default)
        {
            if (id <= 0)
            {
                throw new ArgumentException("Delivery plan table ID must be greater than zero", nameof(id));
            }

            try
            {
                var url = $"/o/c/grtdeliveryplantables/{id}";

                var response = await _httpClient.DeleteAsync(url, cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Trace.TraceError(
                        $"GRT API error deleting delivery plan table: {response.StatusCode} - {response.ReasonPhrase}. Error: {errorContent}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"GRT API exception deleting delivery plan table: {ex.Message}");
                throw;
            }
        }

        public async Task<DeliveryPlanTableResponse> CreateDeliveryPlanTableAsync(
            DeliveryPlanTableRequest request,
            CancellationToken cancellationToken = default)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request), "Delivery plan table request cannot be null");
            }

            try
            {
                var url = "/o/c/grtdeliveryplantables/";

                var jsonContent = JsonConvert.SerializeObject(request, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    Formatting = Formatting.None
                });

                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(url, content, cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<DeliveryPlanTableResponse>(responseContent);
                    return result;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Trace.TraceError(
                        $"GRT API error creating delivery plan table: {response.StatusCode} - {response.ReasonPhrase}. Error: {errorContent}");
                    throw new Exception($"Failed to create delivery plan table: {response.StatusCode} - {errorContent}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"GRT API exception creating delivery plan table: {ex.Message}");
                throw;
            }
        }

        public async Task<DeliveryPlanTableResponse> UpdateDeliveryPlanTableAsync(
            long id,
            DeliveryPlanTableRequest request,
            CancellationToken cancellationToken = default)
        {
            if (id <= 0)
            {
                throw new ArgumentException("Delivery plan table ID must be greater than zero", nameof(id));
            }

            if (request == null)
            {
                throw new ArgumentNullException(nameof(request), "Delivery plan table request cannot be null");
            }

            try
            {
                var url = $"/o/c/grtdeliveryplantables/{id}";

                var jsonContent = JsonConvert.SerializeObject(request, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    Formatting = Formatting.None
                });

                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                // Use PATCH method as shown in the cURL request
                var httpRequest = new HttpRequestMessage(new HttpMethod("PATCH"), url)
                {
                    Content = content
                };

                var response = await _httpClient.SendAsync(httpRequest, cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<DeliveryPlanTableResponse>(responseContent);
                    return result;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Trace.TraceError(
                        $"GRT API error updating delivery plan table: {response.StatusCode} - {response.ReasonPhrase}. Error: {errorContent}");
                    throw new Exception($"Failed to update delivery plan table: {response.StatusCode} - {errorContent}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"GRT API exception updating delivery plan table: {ex.Message}");
                throw;
            }
        }
    }
}
