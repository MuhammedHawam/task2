using System;
using System.Text.Json;

namespace PIF.EBP.WebAPI.Response
{
    [Serializable]
    public class ApiResponseException : Exception
    {
        public ApiResponseError Error { get; private set; }

        public ApiResponseException(ApiResponseError error)
            : base(JsonSerializer.Serialize(error))
        {
            Error = error;
        }
    }
}