using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.JsonWebTokens;
using SlimLib.Auth.Azure;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Usage
{
    public class DemoAuthenticationClient : CachingAzureAuthenticationClient
    {
        private readonly ILogger<DemoAuthenticationClient> logger;

        public DemoAuthenticationClient(IAzureCredentials azureCredentials, HttpClient httpClient, IMemoryCache memoryCache, ILogger<DemoAuthenticationClient> logger)
            : base(azureCredentials, httpClient, memoryCache)
        {
            this.logger = logger;
        }

        protected override async ValueTask<AuthSuccessResponse?> GetAuthenticationFromCacheAsync(IAzureTenant tenant, string scope)
        {
            var message = await base.GetAuthenticationFromCacheAsync(tenant, scope);

            if (message != null)
                logger.LogInformation("Retrieved token from cache for {Tenant} with scope {Scope}", tenant.Identifier, scope);
            else
                logger.LogInformation("No token in cache for {Tenant} with scope {Scope}", tenant.Identifier, scope);

            return message;
        }

        protected override async Task<AuthSuccessResponse> GetAuthenticationFromServerAsync(IAzureTenant tenant, string scope)
        {
            var message = await base.GetAuthenticationFromServerAsync(tenant, scope);

            logger.LogInformation("Received new token from server for {Tenant} with scope {Scope}", tenant.Identifier, scope);

            var jwt = new JsonWebToken(message.AccessToken);

            foreach (var item in jwt.Claims.Where(x => x.Type == "roles"))
            {
                logger.LogInformation("Role: {Role}", item.Value);
            }

            return message;
        }
    }
}