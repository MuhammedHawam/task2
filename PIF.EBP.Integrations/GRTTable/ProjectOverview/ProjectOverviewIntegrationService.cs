using Newtonsoft.Json;
using PIF.EBP.Core.GRT;
using PIF.EBP.Core.GRTTable.ProjectOverview;
using System;
using System.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PIF.EBP.Integrations.GRTTable.ProjectOverview
{
    /// <summary>
    /// Integration service for GRT Project Overview operations
    /// </summary>
    public class ProjectOverviewIntegrationService : IProjectOverviewIntegrationService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        public ProjectOverviewIntegrationService()
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

        public async Task<ProjectOverviewTableResponse> CreateProjectOverviewAsync(
            GRTProjectOverviewRequest request,
            CancellationToken cancellationToken = default)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request), "Project overview request cannot be null");
            }

            try
            {
                var url = "/o/c/grtprojectoverviews/";

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
                    var result = JsonConvert.DeserializeObject<ProjectOverviewTableResponse>(responseContent);
                    return result;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Trace.TraceError(
                        $"GRT API error creating project overview: {response.StatusCode} - {response.ReasonPhrase}. Error: {errorContent}");
                    throw new Exception($"Failed to create project overview: {response.StatusCode} - {errorContent}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"GRT API exception creating project overview: {ex.Message}");
                throw;
            }
        }

        public async Task<ProjectOverviewTableResponse> GetProjectOverviewByIdAsync(
            long id,
            CancellationToken cancellationToken = default)
        {
            if (id <= 0)
            {
                throw new ArgumentException("Cycle company map ID must be greater than zero", nameof(id));
            }
            try
            {
                // Build filter-based query matching the cURL pattern
                var filter = $"r_gRTCycleCompanyMapRelationship_c_cycleCompanyMapId eq '{id}'";
                var url = $"/o/c/grtprojectoverviews" +
                    $"?filter={Uri.EscapeDataString(filter)}" +
                    $"&page=1" +
                    $"&pageSize=20" +
                    $"&sort={Uri.EscapeDataString("dateModified:desc" ?? "dateModified:desc")}";



                var response = await _httpClient.GetAsync(url, cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<ProjectOverviewTableResponse>(responseContent);
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Trace.TraceError(
                    $"GRT API error getting project overviews by id: {response.StatusCode} - {response.ReasonPhrase}. Error: {errorContent}");

                return null;

            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"GRT API exception getting project overviews: {ex.Message}");
                throw;
            }
        }


        public async Task<ProjectOverviewTableResponse> UpdateProjectOverviewAsync(
            long id,
            GRTProjectOverviewRequest request,
            CancellationToken cancellationToken = default)
        {
            if (id <= 0)
            {
                throw new ArgumentException("Project overview ID must be greater than zero", nameof(id));
            }

            if (request == null)
            {
                throw new ArgumentNullException(nameof(request), "Project overview request cannot be null");
            }

            try
            {
                var url = $"/o/c/grtprojectoverviews/{id}";

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
                    var result = JsonConvert.DeserializeObject<ProjectOverviewTableResponse>(responseContent);
                    return result;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Trace.TraceError(
                        $"GRT API error updating project overview: {response.StatusCode} - {response.ReasonPhrase}. Error: {errorContent}");
                    throw new Exception($"Failed to update project overview: {response.StatusCode} - {errorContent}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"GRT API exception updating project overview: {ex.Message}");
                throw;
            }
        }

    }
}
