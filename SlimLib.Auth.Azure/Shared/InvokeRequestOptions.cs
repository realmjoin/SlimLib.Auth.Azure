﻿using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json.Nodes;

namespace SlimLib;

public class InvokeRequestOptions
{
    public ConsistencyLevel ConsistencyLevel { get; set; }
    public string? UserAgent { get; set; }
    public int? MaxPageSize { get; set; }
    public ReturnOptions Return { get; set; }

    public virtual void ConfigureHttpRequest(HttpRequestMessage request)
    {
        if (ConsistencyLevel == ConsistencyLevel.Eventual)
            request.Headers.Add("ConsistencyLevel", "eventual");

        if (UserAgent is not null)
            request.Headers.UserAgent.Add(new ProductInfoHeaderValue(UserAgent));

        if (MaxPageSize.HasValue)
            request.Headers.Add("Prefer", $"odata.maxpagesize={MaxPageSize.Value}");

        if (Return.HasFlag(ReturnOptions.Minimal))
            request.Headers.Add("Prefer", "return=minimal");
        else if (Return.HasFlag(ReturnOptions.Representation))
            request.Headers.Add("Prefer", "return=representation");

        if (Return.HasFlag(ReturnOptions.IncludeUnknownEnumMembers))
            request.Headers.Add("Prefer", "return=include-unknown-enum-members");
    }

    public virtual void ConfigureBatchRequest(JsonObject request)
    {
        var headers = request["headers"];

        if (headers is null)
        {
            headers = new JsonObject();
            request.Add("headers", headers);
        }
        else if (headers is not JsonObject)
        {
            throw new InvalidOperationException("The 'headers' property must be a JSON object.");
        }

        if (ConsistencyLevel == ConsistencyLevel.Eventual)
            headers["ConsistencyLevel"] = "eventual";

        if (UserAgent is not null)
            headers["User-Agent"] = UserAgent;

        var preferValues = new List<string>();

        if (MaxPageSize.HasValue)
            preferValues.Add($"odata.maxpagesize={MaxPageSize.Value}");

        if (Return.HasFlag(ReturnOptions.Minimal))
            preferValues.Add("return=minimal");
        else if (Return.HasFlag(ReturnOptions.Representation))
            preferValues.Add("return=representation");

        if (Return.HasFlag(ReturnOptions.IncludeUnknownEnumMembers))
            preferValues.Add("return=include-unknown-enum-members");

        if (preferValues.Count > 0)
            request["Prefer"] = string.Join(",", preferValues);
    }
}