using System.Net.Http;
using System.Text.Json;

namespace SlimLib.Auth.Azure.TokenProvider
{
    public class RequestAccessTokenUsingAuthorizationCodeTokenProvider : BaseAuthAzureTokenProvider, IAuthAzureTokenProvider
    {
        public RequestAccessTokenUsingAuthorizationCodeTokenProvider(HttpClient httpClient) : base(httpClient)
        {
        }

        protected override IAuthSuccessResponse ParseSuccessResponse(JsonDocument document)
        {
            return new AuthCodeGrantSuccessResponse(
                    document.RootElement.GetProperty("access_token").GetString()!,
                    document.RootElement.GetProperty("expires_in").GetInt32(),
                    document.RootElement.GetProperty("id_token").GetString(),
                    document.RootElement.GetProperty("refresh_token").GetString());
        }
    }
}
