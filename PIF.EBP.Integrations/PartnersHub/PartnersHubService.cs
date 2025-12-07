using Newtonsoft.Json;
using PIF.EBP.Core.PartnersHub;
using PIF.EBP.Core.PartnersHub.DTOs;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PIF.EBP.Integrations.PartnersHub
{
    /// <summary>
    /// Service implementation for PartnersHub Innovation Hub APIs
    /// </summary>
    public class PartnersHubService : IPartnersHubService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        public PartnersHubService()
        {
            _baseUrl = ConfigurationManager.AppSettings["PartnersHubApiBaseUrl"] ?? "https://localhost:7142";
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(_baseUrl)
            };
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(
                new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<ChallengeWithCampaignResponse> GetChallengesByCompanyIdAsync(
            List<Guid> companyIds,
            int pageNumber = 0,
            int pageSize = 0,
            CancellationToken cancellationToken = default)
        {
            if (companyIds == null || !companyIds.Any())
            {
                return new ChallengeWithCampaignResponse
                {
                    Challenges = new PagingResult<ChallengeCompanyDTO>
                    {
                        Items = new List<ChallengeCompanyDTO>(),
                        TotalCount = 0
                    },
                    Campaigns = new PagingResult<CampaignCompanyDTO>
                    {
                        Items = new List<CampaignCompanyDTO>(),
                        TotalCount = 0
                    }
                };
            }

            var request = new ChallengeByCompanyIdRequest
            {
                CopmanyIds = companyIds,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            var jsonContent = JsonConvert.SerializeObject(request);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            try
            {
                var response = await _httpClient.PostAsync(
                    "/api/v1/ChallengeRequest/ChallengeByCompanyId",
                    content,
                    cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();

                    // Deserialize the wrapper response
                    var apiResponse = JsonConvert.DeserializeObject<PartnersHubApiResponse>(responseContent);

                    if (apiResponse?.Data != null)
                    {
                        // Map Item1 and Item2 to Challenges and Campaigns
                        return new ChallengeWithCampaignResponse
                        {
                            Challenges = apiResponse.Data.Item1 ?? new PagingResult<ChallengeCompanyDTO>
                            {
                                Items = new List<ChallengeCompanyDTO>(),
                                TotalCount = 0
                            },
                            Campaigns = apiResponse.Data.Item2 ?? new PagingResult<CampaignCompanyDTO>
                            {
                                Items = new List<CampaignCompanyDTO>(),
                                TotalCount = 0
                            }
                        };
                    }

                    return new ChallengeWithCampaignResponse
                    {
                        Challenges = new PagingResult<ChallengeCompanyDTO>
                        {
                            Items = new List<ChallengeCompanyDTO>(),
                            TotalCount = 0
                        },
                        Campaigns = new PagingResult<CampaignCompanyDTO>
                        {
                            Items = new List<CampaignCompanyDTO>(),
                            TotalCount = 0
                        }
                    };
                }
                else
                {
                    // Log error and return empty result
                    System.Diagnostics.Trace.TraceError(
                        $"PartnersHub API error: {response.StatusCode} - {response.ReasonPhrase}");
                    return new ChallengeWithCampaignResponse
                    {
                        Challenges = new PagingResult<ChallengeCompanyDTO>
                        {
                            Items = new List<ChallengeCompanyDTO>(),
                            TotalCount = 0
                        },
                        Campaigns = new PagingResult<CampaignCompanyDTO>
                        {
                            Items = new List<CampaignCompanyDTO>(),
                            TotalCount = 0
                        }
                    };
                }
            }
            catch (Exception ex)
            {
                // Log error and return empty result
                System.Diagnostics.Trace.TraceError($"PartnersHub API exception: {ex.Message}");
                return new ChallengeWithCampaignResponse
                {
                    Challenges = new PagingResult<ChallengeCompanyDTO>
                    {
                        Items = new List<ChallengeCompanyDTO>(),
                        TotalCount = 0
                    },
                    Campaigns = new PagingResult<CampaignCompanyDTO>
                    {
                        Items = new List<CampaignCompanyDTO>(),
                        TotalCount = 0
                    }
                };
            }
        }

        public async Task<Dictionary<Guid, (int ChallengesCount, int CampaignsCount)>> GetActivityCountsByCompanyIdsAsync(
            List<Guid> companyIds,
            CancellationToken cancellationToken = default)
        {
            var result = new Dictionary<Guid, (int ChallengesCount, int CampaignsCount)>();

            if (companyIds == null || !companyIds.Any())
            {
                return result;
            }

            // Initialize all company IDs with zero counts
            foreach (var companyId in companyIds)
            {
                result[companyId] = (0, 0);
            }

            try
            {
                // Get all challenges and campaigns for the companies
                var response = await GetChallengesByCompanyIdAsync(companyIds, 1, 1000, cancellationToken);

                if (response != null)
                {
                    // Count challenges per company using SourceCompanyId
                    if (response.Challenges != null && response.Challenges.Items != null)
                    {
                        foreach (var challenge in response.Challenges.Items)
                        {
                            if (result.ContainsKey(challenge.SourceCompanyId))
                            {
                                var currentCount = result[challenge.SourceCompanyId];
                                result[challenge.SourceCompanyId] = (currentCount.ChallengesCount + 1, currentCount.CampaignsCount);
                            }
                        }
                    }

                    // Count campaigns per company using SourceCompanyIds (campaigns can belong to multiple companies)
                    if (response.Campaigns != null && response.Campaigns.Items != null)
                    {
                        foreach (var campaign in response.Campaigns.Items)
                        {
                            if (campaign.SourceCompanyIds != null)
                            {
                                foreach (var sourceCompanyId in campaign.SourceCompanyIds)
                                {
                                    if (result.ContainsKey(sourceCompanyId))
                                    {
                                        var currentCount = result[sourceCompanyId];
                                        result[sourceCompanyId] = (currentCount.ChallengesCount, currentCount.CampaignsCount + 1);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"Error getting activity counts: {ex.Message}");
            }

            return result;
        }
    }

    /// <summary>
    /// Wrapper class for the PartnersHub API response structure
    /// </summary>
    internal class PartnersHubApiResponse
    {
        [JsonProperty("httpCode")]
        public int HttpCode { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("data")]
        public PartnersHubApiData Data { get; set; }
    }

    /// <summary>
    /// Data structure containing Item1 (Challenges) and Item2 (Campaigns)
    /// </summary>
    internal class PartnersHubApiData
    {
        [JsonProperty("item1")]
        public PagingResult<ChallengeCompanyDTO> Item1 { get; set; }

        [JsonProperty("item2")]
        public PagingResult<CampaignCompanyDTO> Item2 { get; set; }
    }
}
