using System.Collections.Generic;

namespace SlimLib.Auth.Azure
{
    public class AzureClientCredentials : AzureCredentials
    {
        public AzureClientCredentials()
        {
        }

        public AzureClientCredentials(string clientID, string clientSecret)
        {
            ClientID = clientID;
            ClientSecret = clientSecret;
        }

        public string ClientID { get; set; } = "";
        public string ClientSecret { get; set; } = "";

        public override IDictionary<string, string> GetRequestData(string scope)
        {
            return new Dictionary<string, string>
            {
                ["grant_type"] = "client_credentials",
                ["client_id"] = ClientID,
                ["client_secret"] = ClientSecret,
                ["scope"] = scope,
            };
        }
    }
}