using Newtonsoft.Json;
using PIF.EBP.Core.GRTTable.LandSale;
using PIF.EBP.Core.GRTTable.LandSale.DTOs;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PIF.EBP.Integrations.GRTTable.LandSale
{
    public class LandSaleService : ILandSaleIntegrationService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        public LandSaleService()
        {
            _baseUrl = ConfigurationManager.AppSettings["GRTApiBaseUrl"] ?? "https://automation.netways1.com";
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


        public async Task<LandSaleTablesPagedResponse> GetLandSaleTablesPagedAsync(
                     long projectOverviewId,
                     int page = 1,
                     int pageSize = 20,
                     string search = null,
                     CancellationToken cancellationToken = default)
        {
            if (projectOverviewId <= 0)
            {
                throw new ArgumentException(
                    "Project overview ID must be greater than zero",
                    nameof(projectOverviewId));
            }

            try
            {
                // Build filter
                var filter =
                    $"r_projectToLandSaleTableRelationship_c_grtProjectOverviewId eq '{projectOverviewId}'";

                if (!string.IsNullOrWhiteSpace(search))
                {
                    filter += $" and contains(landSales, '{search}')";
                }

                // Build URL
                var url =
                    $"/o/c/grtprojectoverviews/{projectOverviewId}/projectToLandSaleTableRelationship" +
                    $"?page={page}&pageSize={pageSize}";

                var response = await _httpClient.GetAsync(url, cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<LandSaleTablesPagedResponse>(responseContent);
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Trace.TraceError(
                    $"GRT API error getting land sale tables: " +
                    $"{response.StatusCode} - {response.ReasonPhrase}. Error: {errorContent}");

                return new LandSaleTablesPagedResponse
                {
                    Items = new List<LandSaleTable>(),
                    Page = page,
                    PageSize = pageSize,
                    TotalCount = 0,
                    LastPage = 1
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError(
                    $"GRT API exception getting land sale tables: {ex.Message}");
                throw;
            }
        }



        public async Task<LandSaleTableResponse> UpdateLandSaleTableAsync(
                     long id,
                     LandSaleTableRequest request,
                     CancellationToken cancellationToken = default)
        {
            if (id <= 0)
            {
                throw new ArgumentException(
                    "Land sale table ID must be greater than zero",
                    nameof(id));
            }

            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            try
            {
                var url = $"/o/c/grtlandsaletables/{id}";

                var jsonContent = JsonConvert.SerializeObject(request);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var httpRequest = new HttpRequestMessage(new HttpMethod("PATCH"), url)
                {
                    Content = content
                };


                var response = await _httpClient.SendAsync(httpRequest, cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<LandSaleTableResponse>(responseContent);
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Trace.TraceError(
                    $"GRT API error updating land sale table (ID: {id}): " +
                    $"{response.StatusCode} - {response.ReasonPhrase}. Error: {errorContent}");

                throw new HttpRequestException(
                    $"Failed to update land sale table. Status: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError(
                    $"GRT API exception updating land sale table (ID: {id}): {ex.Message}");
                throw;
            }
        }



        public async Task<LandSaleTableCreateResponse> CreateLandSaleTableAsync(
                     LandSaleTableCreateRequest request,
                     CancellationToken cancellationToken = default)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (request.ProjectOverviewId <= 0)
                throw new ArgumentException(
                    "Project overview ID must be greater than zero",
                    nameof(request.ProjectOverviewId));

            try
            {
                var url = "/o/c/grtlandsaletables";

                var json = JsonConvert.SerializeObject(request);
                var content = new StringContent(
                    json,
                    Encoding.UTF8,
                    "application/json");

                var response = await _httpClient.PostAsync(
                    url,
                    content,
                    cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<LandSaleTableCreateResponse>(responseContent);
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Trace.TraceError(
                    $"GRT API error creating land sale table: " +
                    $"{response.StatusCode} - {response.ReasonPhrase}. " +
                    $"Error: {errorContent}");

                throw new HttpRequestException(
                    $"Failed to create land sale table. Status: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError(
                    $"GRT API exception creating land sale table: {ex.Message}");
                throw;
            }
        }





    }
}
