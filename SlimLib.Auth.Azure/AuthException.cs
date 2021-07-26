using System;
using System.Net;

namespace SlimLib.Auth.Azure
{
    public class AuthException : Exception
    {
        internal AuthException(HttpStatusCode httpStatusCode, AuthErrorResponse response) : base(response.ErrorDescription)
        {
            HttpStatusCode = httpStatusCode;
            Error = response.Error;
            Response = response;
        }

        public HttpStatusCode HttpStatusCode { get; }
        public string Error { get; set; }
        public AuthErrorResponse Response { get; }
    }
}