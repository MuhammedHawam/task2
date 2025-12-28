using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace PIF.EBP.Integrations.Community
{
    public class NotificationApiClient : IDisposable
    {
        protected readonly HttpClient _http;

        public NotificationApiClient()
        {
            var baseUrl = ConfigurationManager.AppSettings["NotifiactionBaseUrl"];

            _http = new HttpClient
            {
                BaseAddress = new Uri(baseUrl, UriKind.Absolute),
                Timeout = TimeSpan.FromSeconds(100)
            };

            _http.DefaultRequestHeaders.Accept.Clear();
            _http.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
        }

        private void SetAuthorizationHeaderFromContext()
        {
            if (HttpContext.Current?.Request.Headers != null)
            {
                var authHeader = HttpContext.Current.Request.Headers["Authorization"];

                if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                {
                    var token = authHeader.Substring("Bearer ".Length).Trim();

                    _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }
                else
                {
                    _http.DefaultRequestHeaders.Authorization = null;
                }
            }
        }

        protected async Task<T> GetAsync<T>(string relativeUrl)
        {
            SetAuthorizationHeaderFromContext();
            var response = await _http.GetAsync(relativeUrl).ConfigureAwait(false);
            return await HandleResponse<T>(response).ConfigureAwait(false);
        }

        protected async Task<T> PostAsync<T>(string relativeUrl, object payload = null)
        {
            SetAuthorizationHeaderFromContext();
            var content = payload != null
                ? new StringContent(
                    JsonConvert.SerializeObject(payload),
                    Encoding.UTF8,
                    "application/json")
                : null;

            var response = await _http.PostAsync(relativeUrl, content).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                return await HandleResponse<T>(response).ConfigureAwait(false);
            }

            return await HandleResponse<T>(response).ConfigureAwait(false);
        }


        protected async Task<T> PutAsync<T>(string relativeUrl, object payload = null)
        {
            SetAuthorizationHeaderFromContext();
            var content = payload != null
                ? new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json")
                : null;

            var response = await _http.PutAsync(relativeUrl, content).ConfigureAwait(false);
            return await HandleResponse<T>(response).ConfigureAwait(false);
        }

        protected async Task DeleteAsync(string relativeUrl)
        {
            SetAuthorizationHeaderFromContext();
            var response = await _http.DeleteAsync(relativeUrl).ConfigureAwait(false);
            await EnsureSuccessStatusCode(response).ConfigureAwait(false);
        }

        private static async Task EnsureSuccessStatusCode(HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode) return;

            var body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            var apiError = TryDeserialize<ApiError>(body);
            throw new ApiException(response.StatusCode,
                                   apiError?.Message ?? response.ReasonPhrase,
                                   apiError?.Code);
        }
        private static async Task<T> HandleResponse<T>(HttpResponseMessage response)
        {
            // Ensure success status code first (this method should handle 4xx/5xx errors)
            // NOTE: This assumes EnsureSuccessStatusCode correctly throws exceptions for 4xx/5xx
            await EnsureSuccessStatusCode(response).ConfigureAwait(false);

            // --- 1. HANDLE 204 NO CONTENT ---
            if (response.StatusCode == HttpStatusCode.NoContent)
            {
                // 204 means success with NO body. We return the default value for T.
                // For a reference type (class), this will return null.
                // For a value type (struct, int, bool), this will return the default value (0, false, etc.).
                return default(T);
            }

            // 2. Read the raw content once. (Only executed if not 204)
            var payload = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            // --- OPTIONAL: Handle empty string payload for other status codes (like 200 or 201) ---
            // If an endpoint returns 200 OK with an empty body (""), the logic below will still fail.
            // If you expect 200/201 to sometimes have an empty body:
            if (string.IsNullOrWhiteSpace(payload))
            {
                return default(T);
            }

            try
            {
                // 3. Proceed with standard JSON deserialization.
                JsonSerializer serializer = JsonSerializer.Create(JsonSettings);

                // Parse the raw payload into a JToken
                JToken token = JToken.Parse(payload);

                // Convert the JToken to the target type T, applying the serializer
                var result = token.ToObject<T>(serializer);

                return result;
            }
            catch (JsonException ex)
            {
                // Catch Newtonsoft's base exception for parsing/deserialization issues.
                throw new InvalidOperationException(
                    $"Failed to deserialize payload to type {typeof(T).Name}. " +
                    $"Payload: {payload.Substring(0, Math.Min(500, payload.Length))}",
                    ex);
            }
        }

        private static T TryDeserialize<T>(string json) where T : class
        {
            try { return JsonConvert.DeserializeObject<T>(json); }
            catch { return null; }
        }

        private static readonly JsonSerializerSettings JsonSettings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            MissingMemberHandling = MissingMemberHandling.Ignore,
            DateTimeZoneHandling = DateTimeZoneHandling.Utc
        };

        public void Dispose()
        {
            _http?.Dispose();
        }

        private class ApiError
        {
            public string Message { get; set; }
            public string Code { get; set; }
        }

        public class ApiException : Exception
        {
            public HttpStatusCode StatusCode { get; }
            public string ApiCode { get; }

            public ApiException(HttpStatusCode statusCode, string message, string apiCode = null)
                : base(message)
            {
                StatusCode = statusCode;
                ApiCode = apiCode;
            }
        }


    }
}
