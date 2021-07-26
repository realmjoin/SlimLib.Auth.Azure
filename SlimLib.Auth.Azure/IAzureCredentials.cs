using System.Collections.Generic;

namespace SlimLib.Auth.Azure
{
    public interface IAzureCredentials
    {
        IDictionary<string, string> GetRequestData(string scope);
    }
}