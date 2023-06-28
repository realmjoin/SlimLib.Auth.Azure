using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;

namespace SlimLib.Auth.Azure
{
    public class TransientAzureAuthenticationClient : IAuthenticationProvider
    {
        private readonly IAzureCredentials azureCredentials;
        private readonly HttpClient httpClient;

        public TransientAzureAuthenticationClient(IAzureCredentials azureCredentials, HttpClient httpClient)
        {
            this.azureCredentials = azureCredentials;
            this.httpClient = httpClient;
        }

        protected IAzureCredentials AzureCredentials => azureCredentials;

        public virtual async ValueTask AuthenticateRequestAsync(IAzureTenant tenant, string scope, HttpRequestMessage request)
        {
            var auth = await GetAuthenticationAsync(tenant, scope).ConfigureAwait(false);

            if (auth != null)
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
            }
        }

        public virtual async ValueTask<AuthSuccessResponse> GetAuthenticationAsync(IAzureTenant tenant, string scope)
        {
            return await GetAuthenticationFromServerAsync(tenant, scope).ConfigureAwait(false);
        }

        protected virtual async Task<AuthSuccessResponse> GetAuthenticationFromServerAsync(IAzureTenant tenant, string scope)
        {
            var data = azureCredentials.GetRequestData(scope);

            using var content = new FormUrlEncodedContent(data);
            using var response = await httpClient.PostAsync(tenant.TokenUrl, content).ConfigureAwait(false);
            using var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                var error = await JsonSerializer.DeserializeAsync<AuthErrorResponse>(stream).ConfigureAwait(false);
                throw new AuthException(response.StatusCode, error ?? new AuthErrorResponse());
            }

            var auth = await JsonSerializer.DeserializeAsync<AuthSuccessResponse>(stream).ConfigureAwait(false);
            return auth ?? new AuthSuccessResponse();
        }
    }
}