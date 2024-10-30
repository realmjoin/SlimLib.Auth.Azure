using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace SlimLib.Auth.Azure.TokenProvider
{
    public abstract class BaseAuthAzureTokenProvider : IAuthAzureTokenProvider
    {
        private readonly HttpClient _httpClient;

        public BaseAuthAzureTokenProvider(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        protected abstract IAuthSuccessResponse ParseSuccessResponse(JsonDocument document);

        public async Task<IAuthSuccessResponse> GetTokenAsync(IAzureTenant tenant, IAzureCredentials credentials, string scope)
        {
            using FormUrlEncodedContent content = new(credentials.GetRequestData(scope));
            using HttpResponseMessage response = await _httpClient.PostAsync(tenant.TokenUrl, content).ConfigureAwait(false);
            using Stream stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                // {"token_type":"Bearer","scope":"email openid profile https://graph.microsoft.com/User.Read","expires_in":3599,"ext_expires_in":3599,"access_token":"xxx","refresh_token":"xxx","id_token":"xxx"}
                using JsonDocument doc = await JsonDocument.ParseAsync(stream).ConfigureAwait(false);
                return ParseSuccessResponse(doc);
            }
            var error = await JsonSerializer.DeserializeAsync<AuthErrorResponse>(stream).ConfigureAwait(false);
            throw new AuthException(response.StatusCode, error ?? new AuthErrorResponse());
        }
    }
}
