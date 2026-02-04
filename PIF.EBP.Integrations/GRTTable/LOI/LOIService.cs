using Newtonsoft.Json;
using PIF.EBP.Application.GRTTable.LOI_HMA.DTOs;
using PIF.EBP.Core.GRTTable.LOI.DTOs;
using PIF.EBP.Core.GRTTable.LOI.Interfaces;
using System;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace PIF.EBP.Integrations.GRTTable.LOI
{
    public class LOIService : ILOIHMAIntegrationService
    {


        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        public LOIService()
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
        public async Task<LOIHMATablePagedResponse> GetLOIHMATablesPagedAsync(
    long projectOverviewId,
    int page,
    int pageSize,
    string search,
    CancellationToken cancellationToken = default)
        {
                var filter =
                    $"r_projectToLOIHMATableRelationship_c_grtProjectOverviewId eq '{projectOverviewId}'";

                if (!string.IsNullOrWhiteSpace(search))
                {
                    filter += $" and contains(gRTLOITable, '{search}')";
                }

            // Build URL
            var url =
                $"/o/c/grtprojectoverviews/{projectOverviewId}/projectToLOIHMATableRelationship" +
                $"?page={page}&pageSize={pageSize}";



                var response = await _httpClient.GetAsync(url, cancellationToken);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<LOIHMATablePagedResponse>(json);
            }

            public async Task<LOIHMATableItemResponse> GetLOIHMATableByIdAsync(
            long id,
            CancellationToken cancellationToken = default)
        {
                var response = await _httpClient.GetAsync(
                    $"/o/c/grtloihmatables/{id}",
                    cancellationToken);

                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<LOIHMATableItemResponse>(json);
            }

            public async Task<LOIHMATableOperationResponse> CreateLOIHMATableAsync(
            LOIHMATableItemRequest dto,
            CancellationToken cancellationToken = default)
        {
                var payload = new
                {
                    gRTLOITable = dto.GRTLOITable,
                    
                    r_projectToLOIHMATableRelationship_c_grtProjectOverviewId =
                        dto.ProjectToLOIHMATableRelationshipProjectOverviewId
                };

                var response = await _httpClient.PostAsync(
                    "/o/c/grtloitables",
                    new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json"),
                    cancellationToken);

                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<LOIHMATableDto>(json);

                return new LOIHMATableOperationResponse
                {
                    Id = result.Id ?? 0,
                    ExternalReferenceCode = result.ExternalReferenceCode,
                    DateCreated = result.DateCreated ?? DateTime.Now,
                    DateModified = result.DateModified ?? DateTime.Now,
                    Success = true,
                    Message = "LOI / HMA table created successfully"
                };
            }

            public async Task<LOIHMATableOperationResponse> UpdateLOIHMATableAsync(
            long id,
            LOIHMATableItemRequest dto,
            CancellationToken cancellationToken = default)
        {
                var payload = new
                {
                    gRTLOITable = dto.GRTLOITable
                };

                var request = new HttpRequestMessage(
                    new HttpMethod("PUT"),
                    $"/o/c/grtloitables/{id}")
                {
                    Content = new StringContent(
                        JsonConvert.SerializeObject(payload),
                        Encoding.UTF8,
                        "application/json")
                };

                var response = await _httpClient.SendAsync(request, cancellationToken);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<LOIHMATableDto>(json);

                return new LOIHMATableOperationResponse
                {
                    Id = result.Id ?? 0,
                    ExternalReferenceCode = result.ExternalReferenceCode,
                    DateCreated = result.DateCreated ?? DateTime.Now,
                    DateModified = result.DateModified ?? DateTime.Now,
                    Success = true,
                    Message = "LOI / HMA table updated successfully"
                };
            }

            public async Task<bool> DeleteLOIHMATableAsync(
                long id,
                CancellationToken cancellationToken = default)
            {
                var response = await _httpClient.DeleteAsync(
                    $"/o/c/grtloihmatables/{id}",
                    cancellationToken);

                return response.IsSuccessStatusCode;
            }
        }
}
