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
        #region Lookup And Cycles And Project overview

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
                url = AddAuditEventsNestedField(url);

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
                mapUrl = AddAuditEventsNestedField(mapUrl);
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
                cyclesUrl = AddAuditEventsNestedField(cyclesUrl);
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

        #endregion
        #region Delivery Plans
        public async Task<GRTDeliveryPlansPagedResponse> GetDeliveryPlansPagedAsync(
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
                var url = $"/o/c/grtprojectoverviews/{projectOverviewId}/projectToDeliveryPlanRelationship?page={page}&pageSize={pageSize}";

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

                url = AddAuditEventsNestedField(url);

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
                // Update the object entry directly by its ID.
                // Using the relationship endpoint here can lead to no-op updates if the provided id
                // is the delivery plan id (object entry id), not the relationship entry id.
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

        #endregion
        #region Infra Delivery Plans
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
                url = AddAuditEventsNestedField(url);

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

                url = AddAuditEventsNestedField(url);
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
        #endregion
        #region Land Sales
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

                url = AddAuditEventsNestedField(url);

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

        #endregion
        #region Cashflows
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

                    // If no cashflows exist, create a new one and return it
                    if (result == null || result.Items == null || !result.Items.Any())
                    {
                        var createRequest = new GRTCashflowRequest
                        {
                            ProjectToCashflowRelationshipProjectOverviewId = projectOverviewId
                        };

                        var createdCashflow = await CreateCashflowAsync(createRequest, cancellationToken);

                        if (createdCashflow != null)
                        {
                            // Fetch the newly created cashflow to get full details
                            var newCashflow = await GetCashflowByIdAsync(createdCashflow.Id, cancellationToken);

                            return new GRTCashflowsPagedResponse
                            {
                                Items = newCashflow != null ? new List<GRTCashflow> { newCashflow } : new List<GRTCashflow>(),
                                Page = page,
                                PageSize = pageSize,
                                TotalCount = newCashflow != null ? 1 : 0,
                                LastPage = 1
                            };
                        }
                    }

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

        public async Task<GRTCashflowResponse> CreateCashflowAsync(
            GRTCashflowRequest request,
            CancellationToken cancellationToken = default)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request), "Cashflow request cannot be null");
            }

            try
            {
                var url = "/o/c/grtcashflows/";

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
                    var result = JsonConvert.DeserializeObject<GRTCashflowResponse>(responseContent);
                    return result;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Trace.TraceError(
                        $"GRT API error creating cashflow: {response.StatusCode} - {response.ReasonPhrase}. Error: {errorContent}");
                    throw new Exception($"Failed to create cashflow: {response.StatusCode} - {errorContent}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"GRT API exception creating cashflow: {ex.Message}");
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
                var url = $"/o/c/grtcashflows/{id}";
                url = AddAuditEventsNestedField(url);

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

        public async Task<GRTCashflowResponse> UpdateCashflowAsync(
            long id,
            GRTCashflowRequest request,
            CancellationToken cancellationToken = default)
        {
            if (id <= 0)
            {
                throw new ArgumentException("Cashflow ID must be greater than zero", nameof(id));
            }

            if (request == null)
            {
                throw new ArgumentNullException(nameof(request), "Cashflow request cannot be null");
            }

            try
            {
                var url = $"/o/c/grtcashflows/{id}";

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
                    var result = JsonConvert.DeserializeObject<GRTCashflowResponse>(responseContent);
                    return result;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Trace.TraceError(
                        $"GRT API error updating cashflow: {response.StatusCode} - {response.ReasonPhrase}. Error: {errorContent}");
                    throw new Exception($"Failed to update cashflow: {response.StatusCode} - {errorContent}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"GRT API exception updating cashflow: {ex.Message}");
                throw;
            }
        }
        #endregion
        #region Budgets
        public async Task<GRTBudgetsResponse> GetGRTBudgetsPagedAsync(long poid, int page = 1, int pageSize = 20, string search = null, CancellationToken cancellationToken = default)
        {
            if (poid <= 0)
            {
                throw new ArgumentException("Project overview ID must be greater than zero", nameof(poid));
            }
            try
            {
                var filter = $"r_projectToBudgetRelationship_c_grtProjectOverviewId eq '{poid}'";
                var url = $"/o/c/grtbudgets?filter={Uri.EscapeDataString(filter)}&page={page}&pageSize={pageSize}";

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
                        Items = new List<GRTBudgetResponse>(),
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

        public async Task<GRTBudgetResponse> GetBudgetByIdAsync(long id, CancellationToken cancellationToken = default)
        {
            try
            {
                var url = $"/o/c/grtbudgets/{id}";
                url = AddAuditEventsNestedField(url);
                var response = await _httpClient.GetAsync(url, cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<GRTBudgetResponse>(responseContent);
                    return result;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Trace.TraceError(
                        $"GRT API error getting budget by id: {response.StatusCode} - {response.ReasonPhrase}. Error: {errorContent}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"GRT API exception getting budget by id: {ex.Message}");
                throw;
            }
        }

        public async Task<GRTBudgetResponse> CreateBudgetAsync(GRTBudgetRequest request, CancellationToken cancellationToken = default)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request), "Budget request cannot be null");
            }

            try
            {
                var url = "/o/c/grtbudgets/";

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
                    return JsonConvert.DeserializeObject<GRTBudgetResponse>(responseContent);
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Trace.TraceError(
                    $"GRT API error creating budget: {response.StatusCode} - {response.ReasonPhrase}. Error: {errorContent}");
                throw new Exception($"Failed to create budget: {response.StatusCode} - {errorContent}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"GRT API exception creating budget: {ex.Message}");
                throw;
            }
        }

        public async Task<GRTBudgetResponse> UpdateBudgetAsync(long id, GRTBudgetRequest request, CancellationToken cancellationToken = default)
        {
            if (id <= 0)
            {
                throw new ArgumentException("Budget ID must be greater than zero", nameof(id));
            }

            if (request == null)
            {
                throw new ArgumentNullException(nameof(request), "Budget request cannot be null");
            }

            try
            {
                var url = $"/o/c/grtbudgets/{id}";

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
                    return JsonConvert.DeserializeObject<GRTBudgetResponse>(responseContent);
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Trace.TraceError(
                    $"GRT API error updating budget: {response.StatusCode} - {response.ReasonPhrase}. Error: {errorContent}");
                throw new Exception($"Failed to update budget: {response.StatusCode} - {errorContent}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"GRT API exception updating budget: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> DeleteBudgetAsync(long budgetId, CancellationToken cancellationToken = default)
        {
            if (budgetId <= 0)
            {
                throw new ArgumentException("Budget ID must be greater than zero", nameof(budgetId));
            }

            try
            {
                var url = $"/o/c/grtbudgets/{budgetId}";
                var response = await _httpClient.DeleteAsync(url, cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    return true;
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Trace.TraceError(
                    $"GRT API error deleting budget: {response.StatusCode} - {response.ReasonPhrase}. Error: {errorContent}");
                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"GRT API exception deleting budget: {ex.Message}");
                throw;
            }
        }

        public async Task<GRTBudgetResponse> PatchBudgetByExternalReferenceAsync(
            string externalReferenceCode,
            GRTBudgetRequest request,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(externalReferenceCode))
            {
                throw new ArgumentNullException(nameof(externalReferenceCode), "External reference code cannot be null or empty");
            }

            if (request == null)
            {
                throw new ArgumentNullException(nameof(request), "Budget request cannot be null");
            }

            try
            {
                var url = $"/o/c/grtbudgets/by-external-reference-code/{externalReferenceCode}";

                var jsonContent = JsonConvert.SerializeObject(request, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    Formatting = Formatting.None
                });

                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                var httpRequest = new HttpRequestMessage(new HttpMethod("PATCH"), url)
                {
                    Content = content
                };

                var response = await _httpClient.SendAsync(httpRequest, cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<GRTBudgetResponse>(responseContent);
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Trace.TraceError(
                    $"GRT API error patching budget: {response.StatusCode} - {response.ReasonPhrase}. Error: {errorContent}");
                throw new Exception($"Failed to patch budget: {response.StatusCode} - {errorContent}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"GRT API exception patching budget: {ex.Message}");
                throw;
            }
        }

        #endregion
        #region LOI & HMA
        public async Task<GRTLOIHMAsPagedResponse> GetLOIHMAsByProjectIdAsync(
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
                var url = $"/o/c/grtprojectoverviews/{projectOverviewId}/projectToLOIHMARelationship?page={page}&pageSize={pageSize}";
                var response = await _httpClient.GetAsync(url, cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<GRTLOIHMAsPagedResponse>(responseContent);
                    return result;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Trace.TraceError(
                        $"GRT API error getting LOI & HMA: {response.StatusCode} - {response.ReasonPhrase}. Error: {errorContent}");
                    return new GRTLOIHMAsPagedResponse
                    {
                        Items = new List<GRTLOIHMA>(),
                        Page = page,
                        PageSize = pageSize,
                        TotalCount = 0,
                        LastPage = 1
                    };
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"GRT API exception getting LOI & HMA: {ex.Message}");
                throw;
            }
        }

        public async Task<GRTLOIHMA> GetLOIHMAByIdAsync(
            long id,
            CancellationToken cancellationToken = default)
        {
            if (id <= 0)
            {
                throw new ArgumentException("LOI & HMA ID must be greater than zero", nameof(id));
            }

            try
            {
                var url = $"/o/c/grtlois/{id}";
                url = AddAuditEventsNestedField(url);

                var response = await _httpClient.GetAsync(url, cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<GRTLOIHMA>(responseContent);
                    return result;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Trace.TraceError(
                        $"GRT API error getting LOI & HMA: {response.StatusCode} - {response.ReasonPhrase}. Error: {errorContent}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"GRT API exception getting LOI & HMA: {ex.Message}");
                throw;
            }
        }

        public async Task<GRTLOIHMAResponse> CreateLOIHMAAsync(
            GRTLOIHMARequest request,
            CancellationToken cancellationToken = default)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request), "LOI & HMA request cannot be null");
            }

            try
            {
                var url = "/o/c/grtlois/";

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
                    var result = JsonConvert.DeserializeObject<GRTLOIHMAResponse>(responseContent);
                    return result;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Trace.TraceError(
                        $"GRT API error creating LOI & HMA: {response.StatusCode} - {response.ReasonPhrase}. Error: {errorContent}");
                    throw new Exception($"Failed to create LOI & HMA: {response.StatusCode} - {errorContent}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"GRT API exception creating LOI & HMA: {ex.Message}");
                throw;
            }
        }

        public async Task<GRTLOIHMAResponse> UpdateLOIHMAAsync(
            long projectOverviewId,
            long id,
            GRTLOIHMARequest request,
            CancellationToken cancellationToken = default)
        {
            if (projectOverviewId <= 0)
            {
                throw new ArgumentException("Project overview ID must be greater than zero", nameof(projectOverviewId));
            }

            if (id <= 0)
            {
                throw new ArgumentException("LOI & HMA ID must be greater than zero", nameof(id));
            }

            if (request == null)
            {
                throw new ArgumentNullException(nameof(request), "LOI & HMA request cannot be null");
            }

            try
            {
                var url = $"/o/c/grtprojectoverviews/{projectOverviewId}/projectToLOIHMARelationship/{id}";

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
                    var result = JsonConvert.DeserializeObject<GRTLOIHMAResponse>(responseContent);
                    return result;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Trace.TraceError(
                        $"GRT API error updating LOI & HMA: {response.StatusCode} - {response.ReasonPhrase}. Error: {errorContent}");
                    throw new Exception($"Failed to update LOI & HMA: {response.StatusCode} - {errorContent}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"GRT API exception updating LOI & HMA: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> DeleteLOIHMAAsync(
            long id,
            CancellationToken cancellationToken = default)
        {
            if (id <= 0)
            {
                throw new ArgumentException("LOI & HMA ID must be greater than zero", nameof(id));
            }

            try
            {
                var url = $"/o/c/grtlois/{id}";

                var response = await _httpClient.DeleteAsync(url, cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Trace.TraceError(
                        $"GRT API error deleting LOI & HMA: {response.StatusCode} - {response.ReasonPhrase}. Error: {errorContent}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"GRT API exception deleting LOI & HMA: {ex.Message}");
                throw;
            }
        }


        #endregion
        #region Project Impact

        public async Task<GRTProjectImpact> GetProjectImpactByIdAsync(
                     long id,
                     CancellationToken cancellationToken = default)
        {
            if (id <= 0)
            {
                throw new ArgumentException("Project impact ID must be greater than zero", nameof(id));
            }

            try
            {
                var url = $"/o/c/grtprojectimpacts/{id}";
                url = AddAuditEventsNestedField(url);

                var response = await _httpClient.GetAsync(url, cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<GRTProjectImpact>(responseContent);
                    return result;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Trace.TraceError(
                        $"GRT API error getting project impact: {response.StatusCode} - {response.ReasonPhrase}. Error: {errorContent}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"GRT API exception getting project impact: {ex.Message}");
                throw;
            }
        }

        public async Task<GRTProjectImpactPagedResponse> GetProjectImpactByProjectIdAsync(
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

                var url = $"/o/c/grtprojectoverviews/{projectOverviewId}/projectToProjectImpactRelationship?page={page}&pageSize={pageSize}";


                var response = await _httpClient.GetAsync(url, cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<GRTProjectImpactPagedResponse>(responseContent);
                    return result;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Trace.TraceError(
                        $"GRT API error getting project impact: {response.StatusCode} - {response.ReasonPhrase}. Error: {errorContent}");
                    return new GRTProjectImpactPagedResponse
                    {
                        Items = new List<GRTProjectImpact>(),
                        Page = page,
                        PageSize = pageSize,
                        TotalCount = 0,
                        LastPage = 1
                    };
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"GRT API exception getting project impact: {ex.Message}");
                throw;
            }
        }

        public async Task<GRTProjectImpactResponse> UpdateProjectImpactAsync(
            long id,
            long projectOverviewId,
            GRTProjectImpactRequest request,
            CancellationToken cancellationToken = default)
        {
            if (id <= 0)
            {
                throw new ArgumentException("Project impact ID must be greater than zero", nameof(id));
            }

            if (request == null)
            {
                throw new ArgumentNullException(nameof(request), "Project impact request cannot be null");
            }

            try
            {
                var url = $"/o/c/grtprojectimpacts/{id}";
               // var url = $"/o/c/grtprojectoverviews/{projectOverviewId}/projectToProjectImpactRelationship/{id}";
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
                    var result = JsonConvert.DeserializeObject<GRTProjectImpactResponse>(responseContent);
                    return result;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Trace.TraceError(
                        $"GRT API error updating project impact: {response.StatusCode} - {response.ReasonPhrase}. Error: {errorContent}");
                    throw new Exception($"Failed to update project impact: {response.StatusCode} - {errorContent}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"GRT API exception updating project impact: {ex.Message}");
                throw;
            }
        }


        public async Task<GRTProjectImpactResponse> CreateProjectImpactAsync(
                         GRTProjectImpactRequest request,
                         CancellationToken cancellationToken = default)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request), "Project impact request cannot be null");
            }

            try
            {
                var url = "/o/c/grtprojectimpacts";

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
                    var result = JsonConvert.DeserializeObject<GRTProjectImpactResponse>(responseContent);
                    return result;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Trace.TraceError(
                        $"GRT API error creating project impact: {response.StatusCode} - {response.ReasonPhrase}. Error: {errorContent}");
                    throw new Exception($"Failed to create project impact: {response.StatusCode} - {errorContent}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"GRT API exception creating project impact: {ex.Message}");
                throw;
            }
        }

        #endregion
        #region MultipleSand

        public async Task<GRTMultipleSandUsPagedResponse> GetMultipleSandUsByProjectIdAsync(
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
                var url = $"/o/c/grtprojectoverviews/{projectOverviewId}/projectToMultipleSandURelationship?page={page}&pageSize={pageSize}";

                var response = await _httpClient.GetAsync(url, cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<GRTMultipleSandUsPagedResponse>(responseContent);
                    return result;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Trace.TraceError(
                        $"GRT API error getting Multiple S&U: {response.StatusCode} - {response.ReasonPhrase}. Error: {errorContent}");
                    return new GRTMultipleSandUsPagedResponse
                    {
                        Items = new List<GRTMultipleSandU>(),
                        Page = page,
                        PageSize = pageSize,
                        TotalCount = 0,
                        LastPage = 1
                    };
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"GRT API exception getting Multiple S&U: {ex.Message}");
                throw;
            }
        }

        public async Task<GRTMultipleSandU> GetMultipleSandUByIdAsync(
            long id,
            CancellationToken cancellationToken = default)
        {
            if (id <= 0)
            {
                throw new ArgumentException("Multiple S&U ID must be greater than zero", nameof(id));
            }

            try
            {
                var url = $"/o/c/grtmultiplesandus/{id}";
                url = AddAuditEventsNestedField(url);

                var response = await _httpClient.GetAsync(url, cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<GRTMultipleSandU>(responseContent);
                    return result;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Trace.TraceError(
                        $"GRT API error getting Multiple S&U: {response.StatusCode} - {response.ReasonPhrase}. Error: {errorContent}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"GRT API exception getting Multiple S&U: {ex.Message}");
                throw;
            }
        }

        public async Task<GRTMultipleSandUResponse> CreateMultipleSandUAsync(
            GRTMultipleSandURequest request,
            CancellationToken cancellationToken = default)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request), "Multiple S&U request cannot be null");
            }

            try
            {
                var url = "/o/c/grtmultiplesandus/";

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
                    var result = JsonConvert.DeserializeObject<GRTMultipleSandUResponse>(responseContent);
                    return result;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Trace.TraceError(
                        $"GRT API error creating Multiple S&U: {response.StatusCode} - {response.ReasonPhrase}. Error: {errorContent}");
                    throw new Exception($"Failed to create Multiple S&U: {response.StatusCode} - {errorContent}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"GRT API exception creating Multiple S&U: {ex.Message}");
                throw;
            }
        }

        public async Task<GRTMultipleSandUResponse> UpdateMultipleSandUAsync(
            long projectOverviewId,
            long id,
            GRTMultipleSandURequest request,
            CancellationToken cancellationToken = default)
        {
            if (projectOverviewId <= 0)
            {
                throw new ArgumentException("Project overview ID must be greater than zero", nameof(projectOverviewId));
            }

            if (id <= 0)
            {
                throw new ArgumentException("Multiple S&U ID must be greater than zero", nameof(id));
            }

            if (request == null)
            {
                throw new ArgumentNullException(nameof(request), "Multiple S&U request cannot be null");
            }

            try
            {
                var url = $"/o/c/grtmultiplesandus/{id}";

                var jsonContent = JsonConvert.SerializeObject(request, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    Formatting = Formatting.Indented
                });

                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var httpRequest = new HttpRequestMessage(new HttpMethod("PATCH"), url)
                {
                    Content = content
                };

                var response = await _httpClient.SendAsync(httpRequest, cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<GRTMultipleSandUResponse>(responseContent);
                    return result;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Trace.TraceError(
                        $"GRT API error updating Multiple S&U: {response.StatusCode} - {response.ReasonPhrase}. Error: {errorContent}");
                    throw new Exception($"Failed to update Multiple S&U: {response.StatusCode} - {errorContent}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"GRT API exception updating Multiple S&U: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> DeleteMultipleSandUAsync(
            long projectOverviewId,
            long id,
            CancellationToken cancellationToken = default)
        {
            if (projectOverviewId <= 0)
            {
                throw new ArgumentException("Project overview ID must be greater than zero", nameof(projectOverviewId));
            }

            if (id <= 0)
            {
                throw new ArgumentException("Multiple S&U ID must be greater than zero", nameof(id));
            }

            try
            {
                var url = $"/o/c/grtprojectoverviews/{projectOverviewId}/projectToMultipleSandURelationship/{id}";

                var response = await _httpClient.DeleteAsync(url, cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Trace.TraceError(
                        $"GRT API error deleting Multiple S&U: {response.StatusCode} - {response.ReasonPhrase}. Error: {errorContent}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"GRT API exception deleting Multiple S&U: {ex.Message}");
                throw;
            }
        }
        #endregion
        #region Approved BP
        public async Task<GRTApprovedBPsPagedResponse> GetApprovedBPsByProjectIdAsync(
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
                var url = $"/o/c/grtprojectoverviews/{projectOverviewId}/projectToApprovedBPRelationship?page={page}&pageSize={pageSize}";
                
                var response = await _httpClient.GetAsync(url, cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<GRTApprovedBPsPagedResponse>(responseContent);
                    return result;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Trace.TraceError(
                        $"GRT API error getting Approved BP: {response.StatusCode} - {response.ReasonPhrase}. Error: {errorContent}");
                    return new GRTApprovedBPsPagedResponse
                    {
                        Items = new List<GRTApprovedBP>(),
                        Page = page,
                        PageSize = pageSize,
                        TotalCount = 0,
                        LastPage = 1
                    };
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"GRT API exception getting Approved BP: {ex.Message}");
                throw;
            }
        }

        public async Task<GRTApprovedBP> GetApprovedBPByIdAsync(
            long id,
            CancellationToken cancellationToken = default)
        {
            if (id <= 0)
            {
                throw new ArgumentException("Approved BP ID must be greater than zero", nameof(id));
            }

            try
            {
                var url = $"/o/c/grtapprovedbps/{id}";
                url = AddAuditEventsNestedField(url);
                
                var response = await _httpClient.GetAsync(url, cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<GRTApprovedBP>(responseContent);
                    return result;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Trace.TraceError(
                        $"GRT API error getting Approved BP: {response.StatusCode} - {response.ReasonPhrase}. Error: {errorContent}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"GRT API exception getting Approved BP: {ex.Message}");
                throw;
            }
        }

        public async Task<GRTApprovedBPResponse> CreateApprovedBPAsync(
            GRTApprovedBPRequest request,
            CancellationToken cancellationToken = default)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request), "Approved BP request cannot be null");
            }

            try
            {
                var url = "/o/c/grtapprovedbps/";
                
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
                    var result = JsonConvert.DeserializeObject<GRTApprovedBPResponse>(responseContent);
                    return result;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Trace.TraceError(
                        $"GRT API error creating Approved BP: {response.StatusCode} - {response.ReasonPhrase}. Error: {errorContent}");
                    throw new Exception($"Failed to create Approved BP: {response.StatusCode} - {errorContent}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"GRT API exception creating Approved BP: {ex.Message}");
                throw;
            }
        }

        public async Task<GRTApprovedBPResponse> UpdateApprovedBPAsync(
            long id,
            GRTApprovedBPRequest request,
            CancellationToken cancellationToken = default)
        {
            if (id <= 0)
            {
                throw new ArgumentException("Approved BP ID must be greater than zero", nameof(id));
            }

            if (request == null)
            {
                throw new ArgumentNullException(nameof(request), "Approved BP request cannot be null");
            }

            try
            {
                var url = $"/o/c/grtapprovedbps/{id}";
                
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
                    var result = JsonConvert.DeserializeObject<GRTApprovedBPResponse>(responseContent);
                    return result;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Trace.TraceError(
                        $"GRT API error updating Approved BP: {response.StatusCode} - {response.ReasonPhrase}. Error: {errorContent}");
                    throw new Exception($"Failed to update Approved BP: {response.StatusCode} - {errorContent}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"GRT API exception updating Approved BP: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> DeleteApprovedBPAsync(
            long id,
            CancellationToken cancellationToken = default)
        {
            if (id <= 0)
            {
                throw new ArgumentException("Approved BP ID must be greater than zero", nameof(id));
            }

            try
            {
                var url = $"/o/c/grtapprovedbps/{id}";
                
                var response = await _httpClient.DeleteAsync(url, cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Trace.TraceError(
                        $"GRT API error deleting Approved BP: {response.StatusCode} - {response.ReasonPhrase}. Error: {errorContent}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"GRT API exception deleting Approved BP: {ex.Message}");
                throw;
            }
        }
        #endregion

        #region audit Event
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

        #endregion
    }
}
