using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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

            var clientCredentials = new AzureClientCredentials();
            Configuration.GetSection("AzureAD").Bind(clientCredentials);

            services.AddHttpClient();
            services.AddMemoryCache();
            services.AddSingleton<IAuthenticationProvider>(sp => new AzureAuthenticationClient(clientCredentials, sp.GetRequiredService<IHttpClientFactory>().CreateClient(nameof(AzureAuthenticationClient)), sp.GetService<IMemoryCache>()));

            using var container = services.BuildServiceProvider();

            var authProvider = container.GetRequiredService<IAuthenticationProvider>();

            var tenant = new AzureTenant(Configuration.GetValue<string>("Tenant"));
            var scope = Configuration.GetValue<string>("Scope");

            using var request = new HttpRequestMessage();

            await authProvider.AuthenticateRequestAsync(tenant, scope, request);

            Console.WriteLine(request.Headers.Authorization);
        }
    }
}
