using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SlimLib.Auth.Azure;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Usage
{
    public class Program
    {
        public static IConfigurationRoot? Configuration { get; private set; }

        public static async Task Main(string[] args)
        {
            var builder = new ConfigurationBuilder();
            builder.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            builder.AddUserSecrets<Program>();

            Configuration = builder.Build();

            var services = new ServiceCollection();

            services.AddLogging(builder =>
            {
                builder
                    .AddFilter("Microsoft", LogLevel.Warning)
                    .AddFilter("System", LogLevel.Warning)
                    .AddFilter("SlimAzureAuth", LogLevel.Trace)
                    .AddConsole();
            });

            services.Configure<AzureClientCredentials>("MyCredentials", Configuration.GetSection("AzureAD"));

            services.AddHttpClient();
            services.AddMemoryCache();

            services.AddSingleton<IAuthenticationProvider>(sp =>
            {
                var clientCredentials = sp.GetRequiredService<IOptionsMonitor<AzureClientCredentials>>().Get("MyCredentials");

                return new DemoAuthenticationClient(
                    clientCredentials,
                    sp.GetRequiredService<IHttpClientFactory>().CreateClient(nameof(DemoAuthenticationClient)),
                    sp.GetRequiredService<IMemoryCache>(),
                    sp.GetRequiredService<ILogger<DemoAuthenticationClient>>());
            });

            using var container = services.BuildServiceProvider();
            using var serviceScope = container.CreateScope();

            var logger = serviceScope.ServiceProvider.GetRequiredService<ILogger<Program>>();
            var authProvider = serviceScope.ServiceProvider.GetRequiredService<IAuthenticationProvider>();

            var tenant = new AzureTenant(Configuration.GetValue<string>("Tenant"));
            var scope = Configuration.GetValue<string>("Scope");

            using var request = new HttpRequestMessage();

            await authProvider.AuthenticateRequestAsync(tenant, scope, request);
            await authProvider.AuthenticateRequestAsync(tenant, scope, request);
            await authProvider.AuthenticateRequestAsync(tenant, scope, request);

            logger.LogInformation("Access token: {at}", request.Headers.Authorization);
        }
    }
}
