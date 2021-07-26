using System;

namespace SlimLib.Auth.Azure
{
    public interface IAzureTenant
    {
        /// <summary>
        /// Gets the tenant identifier. May be its Guid (as string) or Name (e.g. contoso.onmicrosoft.com).
        /// </summary>
        string Identifier { get; }

        /// <summary>
        /// Gets the AzureAD token URL (e.g. https://login.microsoftonline.com/contoso.onmicrosoft.com/oauth2/v2.0/token).
        /// </summary>
        Uri TokenUrl { get; }

        /// <summary>
        /// Gets the AzureAD logout URL (e.g. https://login.microsoftonline.com/contoso.onmicrosoft.com/oauth2/v2.0/logout).
        /// </summary>
        Uri LogoutUrl { get; }
    }
}