using Microsoft.SharePoint.Client.Publishing;
using Newtonsoft.Json;
using PIF.EBP.Application.GRT.DTOs;
using PIF.EBP.Core.GRT;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PIF.EBP.Integrations.GRT.Implementation
{
    /// <summary>
    /// Implementation of GRT Integration Service
    /// </summary>
    public class GRTIntegrationService : IGRTIntegrationService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        public GRTIntegrationService()
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

        public async Task<GRTProjectOverviewResponse> CreateProjectOverviewAsync(
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
                    var result = JsonConvert.DeserializeObject<GRTProjectOverviewResponse>(responseContent);
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

        public async Task<GRTProjectOverviewResponse> GetProjectOverviewByIdAsync(
            long id,
            CancellationToken cancellationToken = default)
        {
            if (id <= 0)
            {
                throw new ArgumentException("Project overview ID must be greater than zero", nameof(id));
            }

            try
            {
                var url = $"/o/c/grtprojectoverviews/{id}";

                var response = await _httpClient.GetAsync(url, cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<GRTProjectOverviewResponse>(responseContent);
                    return result;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Trace.TraceError(
                        $"GRT API error getting project overview: {response.StatusCode} - {response.ReasonPhrase}. Error: {errorContent}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"GRT API exception getting project overview: {ex.Message}");
                throw;
            }
        }

        public async Task<GRTProjectOverviewResponse> UpdateProjectOverviewAsync(
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
                    var result = JsonConvert.DeserializeObject<GRTProjectOverviewResponse>(responseContent);
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

        public async Task<GRTCyclesPagedResponse> GetCyclesPagedAsync(
            int page = 1,
            int pageSize = 20,
            string search = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var url = $"/o/c/grtcycleses/?page={page}&pageSize={pageSize}";

                if (!string.IsNullOrWhiteSpace(search))
                {
                    url += $"&search={Uri.EscapeDataString(search)}";
                }

                var response = await _httpClient.GetAsync(url, cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<GRTCyclesPagedResponse>(responseContent);
                    return result;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Trace.TraceError(
                        $"GRT API error getting cycles: {response.StatusCode} - {response.ReasonPhrase}. Error: {errorContent}");
                    return new GRTCyclesPagedResponse
                    {
                        Items = new System.Collections.Generic.List<GRTCycle>(),
                        Page = page,
                        PageSize = pageSize,
                        TotalCount = 0,
                        LastPage = 1
                    };
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"GRT API exception getting cycles: {ex.Message}");
                throw;
            }
        }

        public async Task<GRTUiCyclesPagedResponse> GetCyclesPagedAsync(
    long companyId,
    int page = 1,
    int pageSize = 20,
    string search = null,
    CancellationToken cancellationToken = default)
        {
            try
            {
                // 1) GET cyclecompanymaps
                var mapUrl = $"/o/c/cyclecompanymaps/?filter=r_companyInCyclesRelationship_c_companiesId%20eq%20%27{companyId}%27";
                var mapResponse = await _httpClient.GetAsync(mapUrl, cancellationToken);

                if (!mapResponse.IsSuccessStatusCode)
                {
                    return new GRTUiCyclesPagedResponse
                    {
                        ActiveCycles = new List<GRTUiCycle>(),
                        PreviousCycles = new List<GRTUiCycle>(),
                        Page = page,
                        PageSize = pageSize,
                        TotalCount = 0,
                        LastPage = 1
                    };
                }

                var mapContent = await mapResponse.Content.ReadAsStringAsync();
                var mapData = JsonConvert.DeserializeObject<GRTCycleCompanyMapResponse>(mapContent);

                if (mapData?.Items == null || !mapData.Items.Any())
                {
                    return new GRTUiCyclesPagedResponse
                    {
                        ActiveCycles = new List<GRTUiCycle>(),
                        PreviousCycles = new List<GRTUiCycle>(),
                        Page = page,
                        PageSize = pageSize,
                        TotalCount = 0,
                        LastPage = 1
                    };
                }

                // 2) Extract unique cycleIds
                var cycleIds = mapData.Items.Select(m => m.CycleId).Distinct().ToList();

                // 3) GET cycles batch
                var idsFilter = string.Join("%27%2C%27", cycleIds);
                var cyclesUrl = $"/o/c/grtcycleses/?filter=id%20in%20(%27{idsFilter}%27)";
                var cyclesResponse = await _httpClient.GetAsync(cyclesUrl, cancellationToken);

                if (!cyclesResponse.IsSuccessStatusCode)
                {
                    return new GRTUiCyclesPagedResponse
                    {
                        ActiveCycles = new List<GRTUiCycle>(),
                        PreviousCycles = new List<GRTUiCycle>(),
                        Page = page,
                        PageSize = pageSize,
                        TotalCount = 0,
                        LastPage = 1
                    };
                }

                var cyclesContent = await cyclesResponse.Content.ReadAsStringAsync();
                var cyclesData = JsonConvert.DeserializeObject<GRTCyclesBatchResponse>(cyclesContent);

                var cyclesDict = cyclesData.Items.ToDictionary(c => c.Id, c => c);

                // 4) Merge both results
                var merged = new List<GRTUiCycle>();

                foreach (var map in mapData.Items)
                {
                    if (!cyclesDict.ContainsKey(map.CycleId))
                        continue;

                    var cycle = cyclesDict[map.CycleId];

                    merged.Add(new GRTUiCycle
                    {
                        CycleId = map.CycleId,
                        PoId = map.PoId,
                        CompanyId = map.CompanyId,
                        CycleName = cycle.CycleName,
                        CycleStartDate = cycle.CycleStartDate,
                        CycleEndDate = cycle.CycleEndDate,
                        RawCycleCompanyStatus = map.CycleCompanyStatus,
                        RawSystemStatus = cycle.Status?.Label,
                        Status = MapCycleStatus(map.CycleCompanyStatus)
                    });
                }

                // 5) Search (if provided)
                if (!string.IsNullOrWhiteSpace(search))
                {
                    var s = search.ToLower();
                    merged = merged.Where(c =>
                        (c.CycleName?.ToLower().Contains(s) ?? false) ||
                        (c.PoId?.ToLower().Contains(s) ?? false)
                    ).ToList();
                }

                // 6) Split into Active / Previous
                var active = merged.Where(IsActiveCycle).ToList();
                var previous = merged.Where(c => !IsActiveCycle(c)).ToList();

                // 7) Paging applies ONLY to previous cycles
                var totalCount = previous.Count;
                var lastPage = (int)Math.Ceiling((double)totalCount / pageSize);

                var pagedPrevious = previous
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                return new GRTUiCyclesPagedResponse
                {
                    ActiveCycles = active,              // no paging
                    PreviousCycles = pagedPrevious,     // paging only here
                    Page = page,
                    PageSize = pageSize,
                    TotalCount = totalCount,
                    LastPage = lastPage > 0 ? lastPage : 1
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"GRT API exception getting cycles: {ex.Message}");
                throw;
            }
        }

        private bool IsActiveCycle(GRTUiCycle cycle)
        {
            if (cycle.RawCycleCompanyStatus?.ToLower() == "completed")
                return false;

            var endDate = DateTime.Parse(cycle.CycleEndDate);
            return endDate >= DateTime.UtcNow;
        }
        private string MapCycleStatus(string rawStatus)
        {
            if (string.IsNullOrWhiteSpace(rawStatus))
                return "Pending";

            var statusLower = rawStatus.ToLower();

            if (statusLower.Contains("submission"))
                return "Submitted";
            if (statusLower.Contains("pending"))
                return "Pending";
            if (statusLower.Contains("assign"))
                return "Assigned";

            return "Pending";
        }

        public async Task<GRTDeliveryPlansPagedResponse> GetDeliveryPlansPagedAsync(
            int page = 1,
            int pageSize = 20,
            string search = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var url = $"/o/c/grtdeliveryplans/?page={page}&pageSize={pageSize}";

                if (!string.IsNullOrWhiteSpace(search))
                {
                    url += $"&search={Uri.EscapeDataString(search)}";
                }

                var response = await _httpClient.GetAsync(url, cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<GRTDeliveryPlansPagedResponse>(responseContent);
                    return result;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Trace.TraceError(
                        $"GRT API error getting delivery plans: {response.StatusCode} - {response.ReasonPhrase}. Error: {errorContent}");
                    return new GRTDeliveryPlansPagedResponse
                    {
                        Items = new System.Collections.Generic.List<GRTDeliveryPlan>(),
                        Page = page,
                        PageSize = pageSize,
                        TotalCount = 0,
                        LastPage = 1
                    };
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"GRT API exception getting delivery plans: {ex.Message}");
                throw;
            }
        }

        public async Task<GRTDeliveryPlan> GetDeliveryPlanByIdAsync(
            long id,
            CancellationToken cancellationToken = default)
        {
            if (id <= 0)
            {
                throw new ArgumentException("Delivery plan ID must be greater than zero", nameof(id));
            }

            try
            {
                var url = $"/o/c/grtdeliveryplans/{id}";

                var response = await _httpClient.GetAsync(url, cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<GRTDeliveryPlan>(responseContent);
                    return result;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Trace.TraceError(
                        $"GRT API error getting delivery plan: {response.StatusCode} - {response.ReasonPhrase}. Error: {errorContent}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"GRT API exception getting delivery plan: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> DeleteDeliveryPlanAsync(
            long id,
            CancellationToken cancellationToken = default)
        {
            if (id <= 0)
            {
                throw new ArgumentException("Delivery plan ID must be greater than zero", nameof(id));
            }

            try
            {
                var url = $"/o/c/grtdeliveryplans/{id}";

                var response = await _httpClient.DeleteAsync(url, cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Trace.TraceError(
                        $"GRT API error deleting delivery plan: {response.StatusCode} - {response.ReasonPhrase}. Error: {errorContent}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"GRT API exception deleting delivery plan: {ex.Message}");
                throw;
            }
        }

        public async Task<GRTDeliveryPlanResponse> CreateDeliveryPlanAsync(
            GRTDeliveryPlanRequest request,
            CancellationToken cancellationToken = default)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request), "Delivery plan request cannot be null");
            }

            try
            {
                var url = "/o/c/grtdeliveryplans/";

                var jsonContent = JsonConvert.SerializeObject(request, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    Formatting = Formatting.Indented
                });

                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(url, content, cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<GRTDeliveryPlanResponse>(responseContent);
                    return result;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Trace.TraceError(
                        $"GRT API error creating delivery plan: {response.StatusCode} - {response.ReasonPhrase}. Error: {errorContent}");
                    throw new Exception($"Failed to create delivery plan: {response.StatusCode} - {errorContent}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"GRT API exception creating delivery plan: {ex.Message}");
                throw;
            }
        }

        public async Task<GRTDeliveryPlanResponse> UpdateDeliveryPlanAsync(
            long id,
            GRTDeliveryPlanRequest request,
            CancellationToken cancellationToken = default)
        {
            if (id <= 0)
            {
                throw new ArgumentException("Delivery plan ID must be greater than zero", nameof(id));
            }

            if (request == null)
            {
                throw new ArgumentNullException(nameof(request), "Delivery plan request cannot be null");
            }

            try
            {
                var url = $"/o/c/grtdeliveryplans/{id}";

                var jsonContent = JsonConvert.SerializeObject(request, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    Formatting = Formatting.Indented
                });

                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync(url, content, cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<GRTDeliveryPlanResponse>(responseContent);
                    return result;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Trace.TraceError(
                        $"GRT API error updating delivery plan: {response.StatusCode} - {response.ReasonPhrase}. Error: {errorContent}");
                    throw new Exception($"Failed to update delivery plan: {response.StatusCode} - {errorContent}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"GRT API exception updating delivery plan: {ex.Message}");
                throw;
            }
        }

        public async Task<GRTInfraDeliveryPlansPagedResponse> GetInfraDeliveryPlansByProjectIdAsync(
            long projectOverviewId,
            int page = 1,
            int pageSize = 20,
            CancellationToken cancellationToken = default)
        {
            if (projectOverviewId <= 0)
            {
                throw new ArgumentException("Project overview ID must be greater than zero", nameof(projectOverviewId));
            }

            try
            {
                var url = $"/o/c/grtprojectoverviews/{projectOverviewId}/projectToInfraDeliveryPlanRelationship?page={page}&pageSize={pageSize}";

                var response = await _httpClient.GetAsync(url, cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<GRTInfraDeliveryPlansPagedResponse>(responseContent);
                    return result;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Trace.TraceError(
                        $"GRT API error getting infra delivery plans: {response.StatusCode} - {response.ReasonPhrase}. Error: {errorContent}");
                    return new GRTInfraDeliveryPlansPagedResponse
                    {
                        Items = new List<GRTInfraDeliveryPlan>(),
                        Page = page,
                        PageSize = pageSize,
                        TotalCount = 0,
                        LastPage = 1
                    };
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"GRT API exception getting infra delivery plans: {ex.Message}");
                throw;
            }
        }

        public async Task<GRTInfraDeliveryPlan> GetInfraDeliveryPlanByIdAsync(
            long id,
            CancellationToken cancellationToken = default)
        {
            if (id <= 0)
            {
                throw new ArgumentException("Infrastructure delivery plan ID must be greater than zero", nameof(id));
            }

            try
            {
                var url = $"/o/c/grtinfradeliveryplans/{id}";

                var response = await _httpClient.GetAsync(url, cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<GRTInfraDeliveryPlan>(responseContent);
                    return result;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Trace.TraceError(
                        $"GRT API error getting infra delivery plan: {response.StatusCode} - {response.ReasonPhrase}. Error: {errorContent}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"GRT API exception getting infra delivery plan: {ex.Message}");
                throw;
            }
        }

        public async Task<GRTInfraDeliveryPlanResponse> CreateInfraDeliveryPlanAsync(
            GRTInfraDeliveryPlanRequest request,
            CancellationToken cancellationToken = default)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request), "Infrastructure delivery plan request cannot be null");
            }

            try
            {
                var url = "/o/c/grtinfradeliveryplans/";

                var jsonContent = JsonConvert.SerializeObject(request, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    Formatting = Formatting.Indented
                });

                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(url, content, cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<GRTInfraDeliveryPlanResponse>(responseContent);
                    return result;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Trace.TraceError(
                        $"GRT API error creating infra delivery plan: {response.StatusCode} - {response.ReasonPhrase}. Error: {errorContent}");
                    throw new Exception($"Failed to create infra delivery plan: {response.StatusCode} - {errorContent}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"GRT API exception creating infra delivery plan: {ex.Message}");
                throw;
            }
        }

        public async Task<GRTInfraDeliveryPlanResponse> UpdateInfraDeliveryPlanAsync(
            long id,
            GRTInfraDeliveryPlanRequest request,
            CancellationToken cancellationToken = default)
        {
            if (id <= 0)
            {
                throw new ArgumentException("Infrastructure delivery plan ID must be greater than zero", nameof(id));
            }

            if (request == null)
            {
                throw new ArgumentNullException(nameof(request), "Infrastructure delivery plan request cannot be null");
            }

            try
            {
                var url = $"/o/c/grtinfradeliveryplans/{id}";

                var jsonContent = JsonConvert.SerializeObject(request, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    Formatting = Formatting.Indented
                });

                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync(url, content, cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<GRTInfraDeliveryPlanResponse>(responseContent);
                    return result;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Trace.TraceError(
                        $"GRT API error updating infra delivery plan: {response.StatusCode} - {response.ReasonPhrase}. Error: {errorContent}");
                    throw new Exception($"Failed to update infra delivery plan: {response.StatusCode} - {errorContent}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"GRT API exception updating infra delivery plan: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> DeleteInfraDeliveryPlanAsync(
            long id,
            CancellationToken cancellationToken = default)
        {
            if (id <= 0)
            {
                throw new ArgumentException("Infrastructure delivery plan ID must be greater than zero", nameof(id));
            }

            try
            {
                var url = $"/o/c/grtinfradeliveryplans/{id}";

                var response = await _httpClient.DeleteAsync(url, cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Trace.TraceError(
                        $"GRT API error deleting infra delivery plan: {response.StatusCode} - {response.ReasonPhrase}. Error: {errorContent}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"GRT API exception deleting infra delivery plan: {ex.Message}");
                throw;
            }
        }

        public async Task<GRTInfraDeliveryPlanYearsPagedResponse> GetInfraDeliveryPlanYearsByPlanIdAsync(
            long infraDeliveryPlanId,
            int page = 1,
            int pageSize = 100,
            CancellationToken cancellationToken = default)
        {
            if (infraDeliveryPlanId <= 0)
            {
                throw new ArgumentException("Infrastructure delivery plan ID must be greater than zero", nameof(infraDeliveryPlanId));
            }

            try
            {
                var url = $"/o/c/grtinfradeliveryplanyearses/?search={infraDeliveryPlanId}&page={page}&pageSize={pageSize}";

                var response = await _httpClient.GetAsync(url, cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<GRTInfraDeliveryPlanYearsPagedResponse>(responseContent);
                    return result;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Trace.TraceError(
                        $"GRT API error getting infra delivery plan years: {response.StatusCode} - {response.ReasonPhrase}. Error: {errorContent}");
                    return new GRTInfraDeliveryPlanYearsPagedResponse
                    {
                        Items = new List<GRTInfraDeliveryPlanYear>(),
                        Page = page,
                        PageSize = pageSize,
                        TotalCount = 0,
                        LastPage = 1
                    };
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"GRT API exception getting infra delivery plan years: {ex.Message}");
                throw;
            }
        }

        public async Task<GRTInfraDeliveryPlanYear> CreateInfraDeliveryPlanYearAsync(
            GRTInfraDeliveryPlanYearRequest request,
            CancellationToken cancellationToken = default)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request), "Infrastructure delivery plan year request cannot be null");
            }

            try
            {
                var url = "/o/c/grtinfradeliveryplanyearses/";

                var jsonContent = JsonConvert.SerializeObject(request, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    Formatting = Formatting.Indented
                });

                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(url, content, cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<GRTInfraDeliveryPlanYear>(responseContent);
                    return result;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Trace.TraceError(
                        $"GRT API error creating infra delivery plan year: {response.StatusCode} - {response.ReasonPhrase}. Error: {errorContent}");
                    throw new Exception($"Failed to create infra delivery plan year: {response.StatusCode} - {errorContent}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"GRT API exception creating infra delivery plan year: {ex.Message}");
                throw;
            }
        }

        public async Task<GRTInfraDeliveryPlanYear> UpdateInfraDeliveryPlanYearAsync(
            long id,
            GRTInfraDeliveryPlanYearRequest request,
            CancellationToken cancellationToken = default)
        {
            if (id <= 0)
            {
                throw new ArgumentException("Infrastructure delivery plan year ID must be greater than zero", nameof(id));
            }

            if (request == null)
            {
                throw new ArgumentNullException(nameof(request), "Infrastructure delivery plan year request cannot be null");
            }

            try
            {
                var url = $"/o/c/grtinfradeliveryplanyearses/{id}";

                var jsonContent = JsonConvert.SerializeObject(request, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    Formatting = Formatting.Indented
                });

                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync(url, content, cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<GRTInfraDeliveryPlanYear>(responseContent);
                    return result;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Trace.TraceError(
                        $"GRT API error updating infra delivery plan year: {response.StatusCode} - {response.ReasonPhrase}. Error: {errorContent}");
                    throw new Exception($"Failed to update infra delivery plan year: {response.StatusCode} - {errorContent}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"GRT API exception updating infra delivery plan year: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> DeleteInfraDeliveryPlanYearAsync(
            long id,
            CancellationToken cancellationToken = default)
        {
            if (id <= 0)
            {
                throw new ArgumentException("Infrastructure delivery plan year ID must be greater than zero", nameof(id));
            }

            try
            {
                var url = $"/o/c/grtinfradeliveryplanyearses/{id}";

                var response = await _httpClient.DeleteAsync(url, cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Trace.TraceError(
                        $"GRT API error deleting infra delivery plan year: {response.StatusCode} - {response.ReasonPhrase}. Error: {errorContent}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"GRT API exception deleting infra delivery plan year: {ex.Message}");
                throw;
            }
        }

        public async Task<GRTLandSalesPagedResponse> GetLandSalesByProjectIdAsync(
            long projectOverviewId,
            int page = 1,
            int pageSize = 20,
            CancellationToken cancellationToken = default)
        {
            if (projectOverviewId <= 0)
            {
                throw new ArgumentException("Project overview ID must be greater than zero", nameof(projectOverviewId));
            }

            try
            {
                var url = $"/o/c/grtprojectoverviews/{projectOverviewId}/projectToLandSaleRelationship?page={page}&pageSize={pageSize}";

                var response = await _httpClient.GetAsync(url, cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<GRTLandSalesPagedResponse>(responseContent);
                    return result;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Trace.TraceError(
                        $"GRT API error getting land sales: {response.StatusCode} - {response.ReasonPhrase}. Error: {errorContent}");
                    return new GRTLandSalesPagedResponse
                    {
                        Items = new List<GRTLandSale>(),
                        Page = page,
                        PageSize = pageSize,
                        TotalCount = 0,
                        LastPage = 1
                    };
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"GRT API exception getting land sales: {ex.Message}");
                throw;
            }
        }

        public async Task<GRTLandSale> GetLandSaleByIdAsync(
            long id,
            CancellationToken cancellationToken = default)
        {
            if (id <= 0)
            {
                throw new ArgumentException("Land sale ID must be greater than zero", nameof(id));
            }

            try
            {
                var url = $"/o/c/grtlandsales/{id}";

                var response = await _httpClient.GetAsync(url, cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<GRTLandSale>(responseContent);
                    return result;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Trace.TraceError(
                        $"GRT API error getting land sale: {response.StatusCode} - {response.ReasonPhrase}. Error: {errorContent}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"GRT API exception getting land sale: {ex.Message}");
                throw;
            }
        }

        public async Task<GRTLandSaleResponse> CreateLandSaleAsync(
            GRTLandSaleRequest request,
            CancellationToken cancellationToken = default)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request), "Land sale request cannot be null");
            }

            try
            {
                var url = "/o/c/grtlandsales/";

                var jsonContent = JsonConvert.SerializeObject(request, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    Formatting = Formatting.Indented
                });

                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(url, content, cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<GRTLandSaleResponse>(responseContent);
                    return result;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Trace.TraceError(
                        $"GRT API error creating land sale: {response.StatusCode} - {response.ReasonPhrase}. Error: {errorContent}");
                    throw new Exception($"Failed to create land sale: {response.StatusCode} - {errorContent}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"GRT API exception creating land sale: {ex.Message}");
                throw;
            }
        }

        public async Task<GRTLandSaleResponse> UpdateLandSaleAsync(
            long id,
            GRTLandSaleRequest request,
            CancellationToken cancellationToken = default)
        {
            if (id <= 0)
            {
                throw new ArgumentException("Land sale ID must be greater than zero", nameof(id));
            }

            if (request == null)
            {
                throw new ArgumentNullException(nameof(request), "Land sale request cannot be null");
            }

            try
            {
                var url = $"/o/c/grtlandsales/{id}";

                var jsonContent = JsonConvert.SerializeObject(request, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    Formatting = Formatting.Indented
                });

                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync(url, content, cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<GRTLandSaleResponse>(responseContent);
                    return result;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Trace.TraceError(
                        $"GRT API error updating land sale: {response.StatusCode} - {response.ReasonPhrase}. Error: {errorContent}");
                    throw new Exception($"Failed to update land sale: {response.StatusCode} - {errorContent}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"GRT API exception updating land sale: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> DeleteLandSaleAsync(
            long id,
            CancellationToken cancellationToken = default)
        {
            if (id <= 0)
            {
                throw new ArgumentException("Land sale ID must be greater than zero", nameof(id));
            }

            try
            {
                var url = $"/o/c/grtlandsales/{id}";

                var response = await _httpClient.DeleteAsync(url, cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Trace.TraceError(
                        $"GRT API error deleting land sale: {response.StatusCode} - {response.ReasonPhrase}. Error: {errorContent}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"GRT API exception deleting land sale: {ex.Message}");
                throw;
            }
        }

        public async Task<GRTCashflowsPagedResponse> GetCashflowsByProjectIdAsync(
            long projectOverviewId,
            int page = 1,
            int pageSize = 20,
            CancellationToken cancellationToken = default)
        {
            if (projectOverviewId <= 0)
            {
                throw new ArgumentException("Project overview ID must be greater than zero", nameof(projectOverviewId));
            }

            try
            {
                var url = $"/o/c/grtprojectoverviews/{projectOverviewId}/projectToCashflowRelationship?page={page}&pageSize={pageSize}";

                var response = await _httpClient.GetAsync(url, cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<GRTCashflowsPagedResponse>(responseContent);
                    return result;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Trace.TraceError(
                        $"GRT API error getting cashflows: {response.StatusCode} - {response.ReasonPhrase}. Error: {errorContent}");
                    return new GRTCashflowsPagedResponse
                    {
                        Items = new List<GRTCashflow>(),
                        Page = page,
                        PageSize = pageSize,
                        TotalCount = 0,
                        LastPage = 1
                    };
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"GRT API exception getting cashflows: {ex.Message}");
                throw;
            }
        }

        public async Task<GRTCashflow> GetCashflowByIdAsync(
            long id,
            CancellationToken cancellationToken = default)
        {
            if (id <= 0)
            {
                throw new ArgumentException("Cashflow ID must be greater than zero", nameof(id));
            }

            try
            {
                var url = $"/o/c/grtprojectoverviews/{id}";

                var response = await _httpClient.GetAsync(url, cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<GRTCashflow>(responseContent);
                    return result;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Trace.TraceError(
                        $"GRT API error getting cashflow: {response.StatusCode} - {response.ReasonPhrase}. Error: {errorContent}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"GRT API exception getting cashflow: {ex.Message}");
                throw;
            }
        }
        #region Budgets
        public async Task<GRTBudgetsResponse> GetGRTBudgetsPagedAsync(long poid, int page = 1, int pageSize = 20, string search = null, CancellationToken cancellationToken = default)
        {
            if (poid <= 0)
            {
                throw new ArgumentException("Project overview ID must be greater than zero", nameof(poid));
            }
            try
            {
                var url = $"/o/c/grtbudgets?poid={poid}/?page={page}&pageSize={pageSize}";

                if (!string.IsNullOrWhiteSpace(search))
                {
                    url += $"&search={Uri.EscapeDataString(search)}";
                }

                var response = await _httpClient.GetAsync(url, cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<GRTBudgetsResponse>(responseContent);
                    return result;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Trace.TraceError(
                        $"GRT API error getting budgets: {response.StatusCode} - {response.ReasonPhrase}. Error: {errorContent}");
                    return new GRTBudgetsResponse
                    {
                        Items = new List<GRTBudgets>(),
                        Page = page,
                        PageSize = pageSize,
                        TotalCount = 0,
                        LastPage = 1
                    };
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"GRT API exception getting budgets: {ex.Message}");
                throw;
            }
        }

        public async Task<GRTBudgets> GetBudgetByIdAsync(long id, CancellationToken cancellationToken = default)
        {
            try
            {
                var url = $"o/c/grtbudgets/{id}";
                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<GRTBudgets>(responseContent);
                    return result;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Trace.TraceError(
                        $"GRT API error getting budget by id: {response.StatusCode} - {response.ReasonPhrase}. Error: {errorContent}");
                    return new GRTBudgets();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"GRT API exception getting budget by id: {ex.Message}");
                throw;
            }
        }

        #endregion
    }
}
