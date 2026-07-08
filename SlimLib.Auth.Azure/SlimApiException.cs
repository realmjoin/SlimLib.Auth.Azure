using System;
using System.Collections.Generic;
using System.Net;

namespace SlimLib;

public class SlimApiException(HttpStatusCode httpStatusCode, IEnumerable<KeyValuePair<string, IEnumerable<string>>> headers, string errorCode, string errorMessage) : Exception(FormatErrorMessage(errorCode, errorMessage))
{
    public HttpStatusCode HttpStatusCode { get; } = httpStatusCode;
    public IEnumerable<KeyValuePair<string, IEnumerable<string>>> Headers { get; } = headers;
    public string ErrorCode { get; } = errorCode;

    private static string FormatErrorMessage(string errorCode, string errorMessage)
    {
        if (string.IsNullOrEmpty(errorMessage))
            return errorCode;

        return $"{errorCode}: {errorMessage}";
    }
}