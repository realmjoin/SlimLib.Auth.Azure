using System.Collections.Generic;

namespace SlimLib.Auth.Azure
{
    public interface IAzureCredentials
    {
        string GetIdentifier(string scope);
        IDictionary<string, string> GetRequestData(string scope);
    }
}