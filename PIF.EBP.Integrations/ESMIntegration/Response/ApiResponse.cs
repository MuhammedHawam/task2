using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PIF.EBP.Core.Exceptions;
using System;
using System.Runtime.Serialization;

namespace PIF.EBP.Integrations.ESMIntegration.Response
{
    public class ApiResponse<T>
    {
        [JsonProperty("result")]
        public JToken ResultToken { get; set; }

        [JsonProperty("error")]
        public string TopLevelError { get; set; } // Handles top-level "error"

        [JsonIgnore]
        public T Result { get; private set; }

        [JsonIgnore]
        public string Error { get; private set; }

        [OnDeserialized]
        internal void OnDeserializedMethod(StreamingContext context)
        {
            // Check for top-level error
            if (!string.IsNullOrEmpty(TopLevelError))
            {
                Error = TopLevelError;
                Result = default(T);
                return;
            }

            // Handle cases where "result" contains an error
            if (ResultToken != null && ResultToken.Type == JTokenType.Object && ResultToken["error"] != null)
            {
                Error = ResultToken["error"].ToString();
                Result = default(T);
                return;
            }

            // Attempt to deserialize the result if no errors are present
            if (ResultToken != null && ResultToken.Type != JTokenType.Null)
            {
                try
                {
                    Result = ResultToken.ToObject<T>();
                }
                catch (Exception ex)
                {
                    throw new UserFriendlyException("Unable to deserialize the API result.", ex);
                }
            }
            else
            {
                Result = default(T);
            }
        }
    }
}
