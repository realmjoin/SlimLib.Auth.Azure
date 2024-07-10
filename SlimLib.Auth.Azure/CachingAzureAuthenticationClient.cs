using Microsoft.Extensions.Caching.Memory;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace SlimLib.Auth.Azure
{
    public class CachingAzureAuthenticationClient : TransientAzureAuthenticationClient
    {
        private const int MinRemainingTokenCacheSeconds = 1;
        private const int MinRemainingTokenLifetimeSeconds = 300;

        public static readonly string CachePrefix = $"{typeof(CachingAzureAuthenticationClient).FullName}.{nameof(GetAuthenticationAsync)}_";

        private readonly IMemoryCache memoryCache;

        public CachingAzureAuthenticationClient(IAzureCredentials azureCredentials, HttpClient httpClient, IMemoryCache memoryCache) : base(azureCredentials, httpClient)
        {
            this.memoryCache = memoryCache;
        }

        public static string GetCachePrefixForTenant(IAzureTenant tenant) => CachePrefix + tenant.Identifier + "_";
        public string GetCacheKey(IAzureTenant tenant, string scope) => GetCachePrefixForTenant(tenant) + AzureCredentials.GetIdentifier(scope);

        public void ClearCache(IAzureTenant tenant, string scope)
        {
            memoryCache.Remove(GetCacheKey(tenant, scope));
        }

        public override async ValueTask<AuthSuccessResponse> GetAuthenticationAsync(IAzureTenant tenant, string scope)
        {
            var cached = await GetAuthenticationFromCacheAsync(tenant, scope).ConfigureAwait(false);

            if (cached != null)
                return cached;

            using var entry = memoryCache.CreateEntry(GetCacheKey(tenant, scope));

            var response = await GetAuthenticationFromServerAsync(tenant, scope).ConfigureAwait(false);
            var expiration = TimeSpan.FromSeconds(response.ExpiresIn - MinRemainingTokenLifetimeSeconds);

            if (expiration > TimeSpan.FromSeconds(MinRemainingTokenCacheSeconds))
            {
                entry.SetAbsoluteExpiration(expiration);
                entry.Value = response;
            }

            return response;
        }

        protected virtual ValueTask<AuthSuccessResponse?> GetAuthenticationFromCacheAsync(IAzureTenant tenant, string scope)
        {
            return ValueTask.FromResult(memoryCache.TryGetValue(GetCacheKey(tenant, scope), out AuthSuccessResponse? result) ? result : null);
        }

        protected override Task<AuthSuccessResponse> GetAuthenticationFromServerAsync(IAzureTenant tenant, string scope)
        {
            return base.GetAuthenticationFromServerAsync(tenant, scope);
        }
    }
}