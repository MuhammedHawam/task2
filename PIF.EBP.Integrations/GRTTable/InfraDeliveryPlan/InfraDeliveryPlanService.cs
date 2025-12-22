using Newtonsoft.Json;
using PIF.EBP.Core.GRTTable;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PIF.EBP.Integrations.GRTTable
{
    public class InfraDeliveryPlanService : IInfraDeliveryPlanIntegrationService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        public InfraDeliveryPlanService()
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

        public async Task<InfraDeliveryPlanTablesPagedResponse> GetInfraDeliveryPlanTablesPagedAsync(
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
                var filter = $"r_projectToInfraDeliveryPlanRelationshipTab_c_grtProjectOverviewId eq '{projectOverviewId}'";
                if (!string.IsNullOrWhiteSpace(search))
                {
                    filter += $" and contains(gRTInfraDeliveryPlanTable, '{search}')";
                }

                var url = $"/o/c/grtinfradeliveryplantables?filter={Uri.EscapeDataString(filter)}&page={page}&pageSize={pageSize}";
                var response = await _httpClient.GetAsync(url, cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<InfraDeliveryPlanTablesPagedResponse>(responseContent);
                    return result;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Trace.TraceError(
                        $"GRT API error getting infra delivery plan tables: {response.StatusCode} - {response.ReasonPhrase}. Error: {errorContent}");
                    return new InfraDeliveryPlanTablesPagedResponse
                    {
                        Items = new List<InfraDeliveryPlanTable>(),
                        Page = page,
                        PageSize = pageSize,
                        TotalCount = 0,
                        LastPage = 1
                    };
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"GRT API exception getting infra delivery plan tables: {ex.Message}");
                throw;
            }
        }

        public async Task<InfraDeliveryPlanTable> GetInfraDeliveryPlanTableByIdAsync(
            long id,
            CancellationToken cancellationToken = default)
        {
            if (id <= 0)
            {
                throw new ArgumentException("Infrastructure delivery plan table ID must be greater than zero", nameof(id));
            }

            try
            {
                var url = $"/o/c/grtinfradeliveryplantables/{id}";
                url = AddAuditEventsNestedField(url);

                var response = await _httpClient.GetAsync(url, cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<InfraDeliveryPlanTable>(responseContent);
                    return result;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Trace.TraceError(
                        $"GRT API error getting infra delivery plan table: {response.StatusCode} - {response.ReasonPhrase}. Error: {errorContent}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"GRT API exception getting infra delivery plan table: {ex.Message}");
                throw;
            }
        }

        public async Task<InfraDeliveryPlanTableResponse> CreateInfraDeliveryPlanTableAsync(
            InfraDeliveryPlanTableRequest request,
            CancellationToken cancellationToken = default)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request), "Infrastructure delivery plan table request cannot be null");
            }

            try
            {
                var url = "/o/c/grtinfradeliveryplantables/";

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
                    var result = JsonConvert.DeserializeObject<InfraDeliveryPlanTableResponse>(responseContent);
                    return result;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Trace.TraceError(
                        $"GRT API error creating infra delivery plan table: {response.StatusCode} - {response.ReasonPhrase}. Error: {errorContent}");
                    throw new Exception($"Failed to create infra delivery plan table: {response.StatusCode} - {errorContent}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"GRT API exception creating infra delivery plan table: {ex.Message}");
                throw;
            }
        }

        public async Task<InfraDeliveryPlanTableResponse> UpdateInfraDeliveryPlanTableAsync(
            long id,
            InfraDeliveryPlanTableRequest request,
            CancellationToken cancellationToken = default)
        {
            if (id <= 0)
            {
                throw new ArgumentException("Infrastructure delivery plan table ID must be greater than zero", nameof(id));
            }

            if (request == null)
            {
                throw new ArgumentNullException(nameof(request), "Infrastructure delivery plan table request cannot be null");
            }

            try
            {
                var url = $"/o/c/grtinfradeliveryplantables/{id}";

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
                    var result = JsonConvert.DeserializeObject<InfraDeliveryPlanTableResponse>(responseContent);
                    return result;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Trace.TraceError(
                        $"GRT API error updating infra delivery plan table: {response.StatusCode} - {response.ReasonPhrase}. Error: {errorContent}");
                    throw new Exception($"Failed to update infra delivery plan table: {response.StatusCode} - {errorContent}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"GRT API exception updating infra delivery plan table: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> DeleteInfraDeliveryPlanTableAsync(
            long id,
            CancellationToken cancellationToken = default)
        {
            if (id <= 0)
            {
                throw new ArgumentException("Infrastructure delivery plan table ID must be greater than zero", nameof(id));
            }

            try
            {
                var url = $"/o/c/grtinfradeliveryplantables/{id}";

                var response = await _httpClient.DeleteAsync(url, cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Trace.TraceError(
                        $"GRT API error deleting infra delivery plan table: {response.StatusCode} - {response.ReasonPhrase}. Error: {errorContent}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"GRT API exception deleting infra delivery plan table: {ex.Message}");
                throw;
            }
        }
    }
}
