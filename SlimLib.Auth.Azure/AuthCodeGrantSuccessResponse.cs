using System.Text.Json.Serialization;

namespace SlimLib.Auth.Azure
{
    public class AuthCodeGrantSuccessResponse : AuthSuccessResponse, IAuthSuccessResponse
    {
        /// <summary>
        /// The only type that Microsoft Entra ID supports is Bearer
        /// </summary>
        private const string TOKEN_TYPE = "Bearer";

        [JsonPropertyName("id_token")]
        public string? IdToken { get; set; }
        [JsonPropertyName("refresh_token")]
        public string? RefreshToken { get; }

        public AuthCodeGrantSuccessResponse(string accessToken, int expiresIn, string? idToken, string? refreshToken) : base(accessToken, TOKEN_TYPE, expiresIn)
        {
            IdToken = idToken;
            RefreshToken = refreshToken;
        }
    }
}
