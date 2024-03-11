using System.Collections.Generic;

namespace SlimLib.Auth.Azure
{
    public class AzureRefreshTokenCredentials : AzureClientCredentials
    {
        public AzureRefreshTokenCredentials(string clientID, string clientSecret, string refreshToken) : base(clientID, clientSecret)
        {
            ClientID = clientID;
            ClientSecret = clientSecret;
            RefreshToken = refreshToken;
        }

        public string RefreshToken { get; private set; }

        public override string GetIdentifier(string scope)
        {
            return $"{ClientID}_{scope}";
        }

        public override IDictionary<string, string> GetRequestData(string scope)
        {
            var requestData = new Dictionary<string, string>
            {
                ["client_id"] = ClientID,
                ["grant_type"] = "refresh_token",
                ["refresh_token"] = RefreshToken,
                ["client_secret"] = ClientSecret
            };
            if (!string.IsNullOrEmpty(scope))
            {
                requestData["scope"] = scope;
            }
            return requestData;
        }
    }
}
