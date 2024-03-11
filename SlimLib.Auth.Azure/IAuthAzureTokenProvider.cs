using System.Threading.Tasks;

namespace SlimLib.Auth.Azure
{
    public interface IAuthAzureTokenProvider
    {
        Task<IAuthSuccessResponse> GetTokenAsync(IAzureTenant tenant, IAzureCredentials credentials, string scope);
    }
}
