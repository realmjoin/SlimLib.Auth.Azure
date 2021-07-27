using Microsoft.Extensions.Caching.Memory;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;

namespace SlimLib.Auth.Azure
{
    public class AzureAuthenticationClient : IAuthenticationProvider
    {
        private const int MinRemainingTokenCacheSeconds = 1;
        private const int MinRemainingTokenLifetimeSeconds = 300;

        private readonly IAzureCredentials azureCredentials;
        private readonly HttpClient httpClient;
        private readonly IMemoryCache? memoryCache;

        public AzureAuthenticationClient(IAzureCredentials azureCredentials, HttpClient httpClient, IMemoryCache? memoryCache = null)
        {
            this.azureCredentials = azureCredentials;
            this.httpClient = httpClient;
            this.memoryCache = memoryCache;
        }

        public string GetCacheKey(IAzureTenant tenant, string scope)
        {
            return $"{typeof(AzureAuthenticationClient).FullName}.{nameof(GetAuthenticationAsync)}_{tenant.Identifier}_{azureCredentials.GetIdentifier(scope)}";
        }

        public void ClearCache(IAzureTenant tenant, string scope)
        {
            var key = GetCacheKey(tenant, scope);
            memoryCache?.Remove(key);
        }

        public async ValueTask AuthenticateRequestAsync(IAzureTenant tenant, string scope, HttpRequestMessage request)
        {
            var auth = await GetAuthenticationAsync(tenant, scope).ConfigureAwait(false);

            if (auth != null)
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
            }
        }

        public async ValueTask<AuthSuccessResponse> GetAuthenticationAsync(IAzureTenant tenant, string scope)
        {
            if (memoryCache != null)
            {
                var key = GetCacheKey(tenant, scope);

                if (!memoryCache.TryGetValue(key, out AuthSuccessResponse result))
                {
                    using ICacheEntry entry = memoryCache.CreateEntry(key);

                    var response = await GetAuthenticationImplAsync(tenant, scope).ConfigureAwait(false);
                    var expiration = TimeSpan.FromSeconds(response.ExpiresIn - MinRemainingTokenLifetimeSeconds);

                    if (expiration <= TimeSpan.FromSeconds(MinRemainingTokenCacheSeconds))
                        return response;

                    entry.SetAbsoluteExpiration(expiration);
                    entry.Value = response;

                    result = response;
                }

                return result;
            }

            return await GetAuthenticationImplAsync(tenant, scope).ConfigureAwait(false);
        }

        private async Task<AuthSuccessResponse> GetAuthenticationImplAsync(IAzureTenant tenant, string scope)
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