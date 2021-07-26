using System.Text.Json.Serialization;

namespace SlimLib.Auth.Azure
{
    public class AuthSuccessResponse : AuthResponse
    {
        internal AuthSuccessResponse()
        {
        }

        public AuthSuccessResponse(string? accessToken, string? tokenType, int expiresIn)
        {
            AccessToken = accessToken ?? "";
            TokenType = tokenType ?? "";
            ExpiresIn = expiresIn;
        }

        [JsonPropertyName("access_token")]
        public string AccessToken { get; } = "";

        [JsonPropertyName("token_type")]
        public string TokenType { get; } = "";

        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; }
    }
}
