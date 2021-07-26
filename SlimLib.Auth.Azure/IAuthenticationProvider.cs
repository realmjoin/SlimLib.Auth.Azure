using System.Net.Http;
using System.Threading.Tasks;

namespace SlimLib.Auth.Azure
{
    public interface IAuthenticationProvider
    {
        ValueTask AuthenticateRequestAsync(IAzureTenant tenant, string scope, HttpRequestMessage request);
    }
}