using System.Collections.Generic;

namespace SlimLib.Auth.Azure
{
    public abstract class AzureCredentials : IAzureCredentials
    {
        public abstract IDictionary<string, string> GetRequestData(string scope);
    }
}