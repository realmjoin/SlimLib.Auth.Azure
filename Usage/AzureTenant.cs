using SlimLib.Auth.Azure;
using System;

namespace Usage
{
    public readonly struct AzureTenant : IAzureTenant
    {
        public AzureTenant(Guid id)
        {
            Identifier = id.ToString();
        }

        public AzureTenant(string name)
        {
            Identifier = name;
        }

        /// <summary>
        /// Gets the tenant identifier. May be its Guid (as string) or Name (e.g. contoso.onmicrosoft.com).
        /// </summary>
        public string Identifier { get; }

        /// <summary>
        /// Gets the AzureAD token URL (e.g. https://login.microsoftonline.com/contoso.onmicrosoft.com/oauth2/v2.0/token).
        /// </summary>
        public Uri TokenUrl => new Uri($"https://login.microsoftonline.com/{Identifier}/oauth2/v2.0/token");

        /// <summary>
        /// Gets the AzureAD logout URL (e.g. https://login.microsoftonline.com/contoso.onmicrosoft.com/oauth2/v2.0/logout).
        /// </summary>
        public Uri LogoutUrl => new Uri($"https://login.microsoftonline.com/{Identifier}/oauth2/v2.0/logout");
    }
}