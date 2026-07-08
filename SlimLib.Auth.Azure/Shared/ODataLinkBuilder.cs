using System;
using System.Collections.Generic;

namespace SlimLib;

public static class ODataLinkBuilder
{
    public static string BuildLink(ScalarRequestOptions? options, string call)
    {
        var args = new List<string>();

        if (options?.Select != null)
            args.Add("$select=" + Uri.EscapeDataString(string.Join(",", options.Select)));

        if (options?.Expand != null)
            args.Add("$expand=" + Uri.EscapeDataString(options.Expand));

        return RequestOptions.BuildLink(call, args);
    }

    public static string BuildLink(ListRequestOptions? options, string call)
    {
        var args = new List<string>();

        if (options?.Select.Count > 0)
            args.Add("$select=" + Uri.EscapeDataString(string.Join(",", options.Select)));

        if (options?.Filter != null)
            args.Add("$filter=" + Uri.EscapeDataString(options.Filter));

        if (options?.Search != null)
            args.Add("$search=" + Uri.EscapeDataString(options.Search));

        if (options?.Expand != null)
            args.Add("$expand=" + Uri.EscapeDataString(options.Expand));

        if (options?.OrderBy.Count > 0)
            args.Add("$orderby=" + Uri.EscapeDataString(string.Join(",", options.OrderBy)));

        if (options?.Count != null)
            args.Add("$count=" + (options.Count.Value ? "true" : "false"));

        if (options?.Skip != null)
            args.Add("$skip=" + options.Skip);

        if (options?.Top != null)
            args.Add("$top=" + options.Top);

        return RequestOptions.BuildLink(call, args);
    }

    public static string BuildLink(InvokeRequestOptions? options, string call)
        => RequestOptions.BuildLink(call, []);

    public static string BuildLink(string call, IEnumerable<string>? select, string? filter = null, string? expand = null)
    {
        var args = new List<string>();

        if (select != null)
            args.Add("$select=" + Uri.EscapeDataString(string.Join(",", select)));

        if (filter != null)
            args.Add("$filter=" + Uri.EscapeDataString(filter));

        if (expand != null)
            args.Add("$expand=" + Uri.EscapeDataString(expand));

        return RequestOptions.BuildLink(call, args);
    }
}