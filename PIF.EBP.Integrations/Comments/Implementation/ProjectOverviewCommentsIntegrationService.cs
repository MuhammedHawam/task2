using Newtonsoft.Json;
using PIF.EBP.Core.Comments;
using PIF.EBP.Core.Comments.DTOs;
using System;
using System.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PIF.EBP.Integrations.Comments.Implementation
{
    public class ProjectOverviewCommentsIntegrationService : IProjectOverviewCommentsIntegrationService
    {
        private readonly HttpClient _httpClient;

        public ProjectOverviewCommentsIntegrationService()
        {
            var baseUrl = ConfigurationManager.AppSettings["ProjectOverviewCommentsApiBaseUrl"]
                          ?? ConfigurationManager.AppSettings["GRTApiBaseUrl"]
                          ?? "https://solutionsuat.pif.gov.sa";
            var username = ConfigurationManager.AppSettings["ProjectOverviewCommentsApiUsername"]
                           ?? ConfigurationManager.AppSettings["GRTApiUsername"]
                           ?? "PartnerHub_Admin@pif.gov.sa";
            var password = ConfigurationManager.AppSettings["ProjectOverviewCommentsApiPassword"]
                           ?? ConfigurationManager.AppSettings["GRTApiPassword"]
                           ?? "123456";

            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(baseUrl)
            };

            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var authValue = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username}:{password}"));
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authValue);
        }

        public async Task<ProjectOverviewCommentsCollectionResponse> GetProjectOverviewCommentsAsync(
            long projectOverviewId,
            int page = 1,
            int pageSize = 1000,
            CancellationToken cancellationToken = default)
        {
            if (projectOverviewId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(projectOverviewId), "Project overview ID must be greater than zero.");
            }

            if (page <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(page), "Page must be greater than zero.");
            }

            if (pageSize <= 0 || pageSize > 1000)
            {
                throw new ArgumentOutOfRangeException(nameof(pageSize), "Page size must be between 1 and 1000.");
            }

            var url = $"/o/c/grtprojectoverviews/{projectOverviewId}/relationProjectOverviewComment?page={page}&pageSize={pageSize}";
            var response = await _httpClient.GetAsync(url, cancellationToken);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Failed to retrieve project overview comments: {response.StatusCode} - {responseContent}");
            }

            return JsonConvert.DeserializeObject<ProjectOverviewCommentsCollectionResponse>(responseContent);
        }

        public async Task<ProjectOverviewCommentItem> CreateProjectOverviewCommentAsync(
            ProjectOverviewCommentCreateRequest request,
            CancellationToken cancellationToken = default)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (request.ProjectOverviewId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(request.ProjectOverviewId), "Project overview ID must be greater than zero.");
            }

            if (string.IsNullOrWhiteSpace(request.FieldId))
            {
                throw new ArgumentException("FieldId is required.", nameof(request.FieldId));
            }

            if (string.IsNullOrWhiteSpace(request.Comment))
            {
                throw new ArgumentException("Comment is required.", nameof(request.Comment));
            }

            var url = "/o/c/projectoverviewcomments";
            var payload = JsonConvert.SerializeObject(request, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            });

            var content = new StringContent(payload, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(url, content, cancellationToken);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Failed to create project overview comment: {response.StatusCode} - {responseContent}");
            }

            return JsonConvert.DeserializeObject<ProjectOverviewCommentItem>(responseContent);
        }
    }
}
