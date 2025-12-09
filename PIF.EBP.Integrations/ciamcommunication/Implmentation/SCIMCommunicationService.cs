using Newtonsoft.Json;
using PIF.EBP.Core.CIAMCommunication;
using PIF.EBP.Core.CIAMCommunication.DTOs;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace PIF.EBP.Integrations.CIAMCommunication.Implmentation
{
    public class SCIMCommunicationService : ISCIMCommunicationService
    {
        private readonly ScimOptions _options;
        private readonly HttpClient _client;
        private string _accessToken;
        private DateTime _tokenExpiry;

        public SCIMCommunicationService()
        {
            var jsonConfig = ConfigurationManager.AppSettings["ScimConfig"] ??
                throw new InvalidOperationException("Scim configuration (AppSettings[\"ScimConfig\"]) is missing.");

            _options = JsonConvert.DeserializeObject<ScimOptions>(jsonConfig)
                       ?? throw new InvalidOperationException("Failed to deserialize ScimConfig.");

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls |
                                                    SecurityProtocolType.Tls11 |
                                                    SecurityProtocolType.Tls12;

            var handler = new HttpClientHandler();

            _client = new HttpClient(handler)
            {
                BaseAddress = new Uri(_options.BaseUrl.TrimEnd('/') + "/"),
                Timeout = TimeSpan.FromSeconds(_options.TimeoutSeconds)
            };
            _client.DefaultRequestHeaders.ConnectionClose = false;
        }

        public async Task<ScimOperationResponse> CreateUserAsync(ScimUserRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            return await SendScimRequestAsync(HttpMethod.Post, "Users", request).ConfigureAwait(false);
        }

        public async Task<ScimOperationResponse> UpdateUserAsync(string userId, ScimPatchRequest payload)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException(nameof(userId));

            var url = $"Users/{WebUtility.UrlEncode(userId)}";
            var patchMethod = new HttpMethod("PATCH");
            return await SendScimRequestAsync(patchMethod, url, payload).ConfigureAwait(false);
        }

        public async Task<ScimOperationResponse> SetAccountLockedAsync(string userId, string userName, bool lockAccount)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException(nameof(userId));
            if (string.IsNullOrWhiteSpace(userName))
                throw new ArgumentException(nameof(userName));

            var payload = new ScimEnterpriseReplaceRequest
            {
                UserName = userName,
                Enterprise = new ScimEnterpriseExtension
                {
                    AccountLocked = lockAccount
                }
            };

            var url = $"Users/{WebUtility.UrlEncode(userId)}";
            return await SendScimRequestAsync(HttpMethod.Put, url, payload).ConfigureAwait(false);
        }

        public async Task<ScimOperationResponse> SetAccountDisabledAsync(string userId, string userName, bool disableAccount)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException(nameof(userId));
            if (string.IsNullOrWhiteSpace(userName))
                throw new ArgumentException(nameof(userName));

            var payload = new ScimEnterpriseReplaceRequest
            {
                UserName = userName,
                Enterprise = new ScimEnterpriseExtension
                {
                    AccountDisabled = disableAccount
                }
            };

            var url = $"Users/{WebUtility.UrlEncode(userId)}";
            return await SendScimRequestAsync(HttpMethod.Put, url, payload).ConfigureAwait(false);
        }

        public async Task<ScimListOperationResponse> GetAllUsersAsync()
        {
            const string relativeUrl = "Users?excludedAttributes=groups,roles";
            return await SendScimListRequestAsync(relativeUrl).ConfigureAwait(false);
        }

        public async Task<ScimListOperationResponse> GetUserByUserNameAsync(string userName)
        {
            if (string.IsNullOrWhiteSpace(userName))
                throw new ArgumentException(nameof(userName));

            string relativeUrl = $"Users?filter=userName eq '{WebUtility.UrlEncode(userName)}'";
            return await SendScimListRequestAsync(relativeUrl).ConfigureAwait(false);
        }

        public async Task<ScimListOperationResponse> GetUserByEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException(nameof(email));

            string relativeUrl = $"Users?filter=emails eq '{WebUtility.UrlEncode(email)}'";
            return await SendScimListRequestAsync(relativeUrl).ConfigureAwait(false);
        }

        public async Task<ScimOperationResponse> ResendInvitationAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException(nameof(userId));

            var patchRequest = new ScimPatchRequest
            {
                Schemas = new List<string> { "urn:ietf:params:scim:api:messages:2.0:PatchOp" },
                Operations = new List<ScimPatchOperation>
                {
                    new ScimPatchOperation
                    {
                        Op = "add",
                        Value = new Dictionary<string, object>
                        {
                            { "urn:scim:wso2:schema", new { forcePasswordReset = true } }
                        }
                    }
                }
            };

            var url = $"Users/{WebUtility.UrlEncode(userId)}";
            var patchMethod = new HttpMethod("PATCH");
            return await SendScimRequestAsync(patchMethod, url, patchRequest).ConfigureAwait(false);
        }

        private async Task<string> GetAccessTokenAsync()
        {
            if (!string.IsNullOrEmpty(_accessToken) && DateTime.UtcNow < _tokenExpiry)
            {
                return _accessToken;
            }

            var tokenRequest = new Dictionary<string, string>
            {
                { "grant_type", "client_credentials" },
                { "client_id", _options.ClientId },
                { "client_secret", _options.ClientSecret },
                { "scope", @"internal_oauth2_introspect internal_bulk_resource_create internal_group_mgt_view internal_group_mgt_create internal_group_mgt_update internal_user_mgt_create internal_user_mgt_list internal_user_mgt_update internal_user_mgt_view internal_user_code_mgt_create internal_user_code_mgt_update internal_user_code_mgt_view internal_user_code_mgt_delete internal_offline_invite" }
            };

            using (var tokenClient = new HttpClient())
            {
                var response = await tokenClient.PostAsync(
                    _options.TokenUrl,
                    new FormUrlEncodedContent(tokenRequest)).ConfigureAwait(false);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    throw new InvalidOperationException($"Failed to get access token: {error}");
                }

                var responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                var tokenResponse = JsonConvert.DeserializeObject<OAuth2TokenResponse>(responseContent);

                _accessToken = tokenResponse.AccessToken;
                _tokenExpiry = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn - 60);

                return _accessToken;
            }
        }

        private async Task<ScimOperationResponse> SendScimRequestAsync(HttpMethod method, string relativeUrl, object requestObject)
        {
            var token = await GetAccessTokenAsync().ConfigureAwait(false);

            _client.DefaultRequestHeaders.Remove("Authorization");
            _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

            string payload = JsonConvert.SerializeObject(
                requestObject,
                Formatting.None,
                new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

            var httpRequest = new HttpRequestMessage(method, relativeUrl)
            {
                Content = new StringContent(payload, Encoding.UTF8, "application/scim+json")
            };

            try
            {
                var httpResponse = await _client.SendAsync(httpRequest).ConfigureAwait(false);
                var rawContent = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);

                if (httpResponse.IsSuccessStatusCode)
                {
                    var responseDto = JsonConvert.DeserializeObject<ScimUserResponse>(rawContent);
                    return new ScimOperationResponse
                    {
                        IsSuccess = true,
                        Id = responseDto?.Id,
                        RawResponse = rawContent
                    };
                }
                else
                {
                    return new ScimOperationResponse
                    {
                        IsSuccess = false,
                        ErrorMessage = $"SCIM {method} failed - {(int)httpResponse.StatusCode} {httpResponse.ReasonPhrase}",
                        RawResponse = rawContent
                    };
                }
            }
            catch (Exception ex)
            {
                return new ScimOperationResponse
                {
                    IsSuccess = false,
                    ErrorMessage = $"Exception while calling SCIM {method}: {ex.Message}",
                    RawResponse = JsonConvert.SerializeObject(ex)
                };
            }
        }

        private async Task<ScimListOperationResponse> SendScimListRequestAsync(string relativeUrl)
        {
            var token = await GetAccessTokenAsync().ConfigureAwait(false);

            _client.DefaultRequestHeaders.Remove("Authorization");
            _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

            var httpRequest = new HttpRequestMessage(HttpMethod.Get, relativeUrl);
            try
            {
                var httpResponse = await _client.SendAsync(httpRequest).ConfigureAwait(false);
                var rawBody = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);

                if (httpResponse.IsSuccessStatusCode)
                {
                    var payload = JsonConvert.DeserializeObject<ScimUserListResponse>(rawBody);
                    return new ScimListOperationResponse
                    {
                        IsSuccess = true,
                        Payload = payload,
                        RawResponse = rawBody
                    };
                }
                else
                {
                    return new ScimListOperationResponse
                    {
                        IsSuccess = false,
                        ErrorMessage = $"SCIM GET failed. Status: {(int)httpResponse.StatusCode} {httpResponse.ReasonPhrase}",
                        RawResponse = rawBody
                    };
                }
            }
            catch (Exception ex)
            {
                return new ScimListOperationResponse
                {
                    IsSuccess = false,
                    ErrorMessage = $"Exception during SCIM GET: {ex.Message}",
                    RawResponse = JsonConvert.SerializeObject(ex)
                };
            }
        }

        private class OAuth2TokenResponse
        {
            [JsonProperty("access_token")]
            public string AccessToken { get; set; }

            [JsonProperty("expires_in")]
            public int ExpiresIn { get; set; }

            [JsonProperty("token_type")]
            public string TokenType { get; set; }
        }
    }
}
