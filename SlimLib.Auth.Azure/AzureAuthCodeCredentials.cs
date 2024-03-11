using System;
using System.Collections.Generic;

namespace SlimLib.Auth.Azure
{
    public class AzureAuthCodeCredentials : AzureClientCredentials
    {
        public AzureAuthCodeCredentials(string clientID, string clientSecret, string code, string redirectUri, string? codeVerifier) : base(clientID, clientSecret)
        {
            Code = code;
            RedirectUri = redirectUri;
            CodeVerifier = codeVerifier;
        }

        public string Code { get; private set; }
        public string RedirectUri { get; private set; }
        public string? CodeVerifier { get; private set; }

        public override string GetIdentifier(string scope)
        {
            return $"{ClientID}_{scope}";
        }

        public override IDictionary<string, string> GetRequestData(string scope)
        {
            var requestData = new Dictionary<string, string>
            {
                ["client_id"] = ClientID,
                ["grant_type"] = "authorization_code",
                ["code"] = Code,
                ["redirect_uri"] = RedirectUri,
                ["client_secret"] = ClientSecret
            };
            if (!string.IsNullOrEmpty(scope))
            {
                requestData["scope"] = scope;
            }
            if (!string.IsNullOrEmpty(CodeVerifier))
            {
                requestData["code_verifier"] = CodeVerifier;
            }
            return requestData;
        }
    }
}
