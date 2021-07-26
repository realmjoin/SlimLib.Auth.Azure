using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SlimLib.Auth.Azure
{
    public class AuthErrorResponse : AuthResponse
    {
        internal AuthErrorResponse()
        {
        }

        public AuthErrorResponse(string? error, string? errorDescription, IList<int>? errorCodes, string? timestamp, string? traceID, string? correlationID)
        {
            Error = error ?? "";
            ErrorDescription = errorDescription ?? "";
            ErrorCodes = errorCodes ?? new List<int>();
            Timestamp = timestamp ?? "";
            TraceID = traceID ?? "";
            CorrelationID = correlationID ?? "";
        }

        [JsonPropertyName("error")]
        public string Error { get; } = "";

        [JsonPropertyName("error_description")]
        public string ErrorDescription { get; } = "";

        [JsonPropertyName("error_codes")]
        public IList<int> ErrorCodes { get; } = new List<int>();

        [JsonPropertyName("timestamp")]
        public string Timestamp { get; } = "";

        [JsonPropertyName("trace_id")]
        public string TraceID { get; } = "";

        [JsonPropertyName("correlation_id")]
        public string CorrelationID { get; } = "";
    }
}
